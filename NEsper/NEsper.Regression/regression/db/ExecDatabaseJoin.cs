///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Data;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.db.drivers;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabaseJoin : RegressionExecution
    {
        private const string ALL_FIELDS = "mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";
    
        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;

            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertion3Stream(epService);
            RunAssertionTimeBatchEPL(epService);
            RunAssertion2HistoricalStar(epService);
            RunAssertion2HistoricalStarInner(epService);
            RunAssertionVariables(epService);
            RunAssertionTimeBatchOM(epService);
            RunAssertionTimeBatchCompile(epService);
            RunAssertionInvalidSQL(epService);
            RunAssertionInvalidBothHistorical(epService);
            RunAssertionInvalidPropertyEvent(epService);
            RunAssertionInvalidPropertyHistorical(epService);
            RunAssertionInvalid1Stream(epService);
            RunAssertionInvalidSubviews(epService);
            RunAssertionStreamNamesAndRename(epService);
            RunAssertionWithPattern(epService);
            RunAssertionPropertyResolution(epService);
            RunAssertionSimpleJoinLeft(epService);
            RunAssertionRestartStatement(epService);
            RunAssertionSimpleJoinRight(epService);
            RunAssertionSQLDatabaseConnection(epService);
        }
    
        private void RunAssertion3Stream(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType<SupportBeanTwo>();
    
            string stmtText = "select * from SupportBean#lastevent sb, SupportBeanTwo#lastevent sbt, " +
                    "sql:MyDB ['select myint from mytesttable'] as s1 " +
                    "  where sb.TheString = sbt.stringTwo and s1.myint = sbt.IntPrimitiveTwo";
    
            EPStatementSPI statement = (EPStatementSPI) epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(statement.StatementContext.IsStatelessSelect);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T1", 2));
            epService.EPRuntime.SendEvent(new SupportBean("T1", -1));
    
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T2", 30));
            epService.EPRuntime.SendEvent(new SupportBean("T2", -1));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T2", "T2", 30});
    
            epService.EPRuntime.SendEvent(new SupportBean("T3", -1));
            epService.EPRuntime.SendEvent(new SupportBeanTwo("T3", 40));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new object[]{"T3", "T3", 40});
    
            statement.Dispose();
        }
    
        private void RunAssertionTimeBatchEPL(EPServiceProvider epService) {
            string stmtText = "select " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable \n\r where ${IntPrimitive} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBean).FullName + "#time_batch(10 sec) as s1";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            RuntestTimeBatch(epService, stmt);
        }
    
        private void RunAssertion2HistoricalStar(EPServiceProvider epService) {
            string[] fields = "IntPrimitive,myint,myvarchar".Split(',');
            string stmtText = "select IntPrimitive, myint, myvarchar from " +
                    typeof(SupportBean).FullName + "#keepall as s0, " +
                    " sql:MyDB ['select myint from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s1," +
                    " sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s2 ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, 6);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{6, 60, "F"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new object[] {6, 60, "F"}});
    
            SendSupportBeanEvent(epService, 9);
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{9, 90, "I"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new object[] {6, 60, "F"}, new object[] {9, 90, "I"}});
    
            SendSupportBeanEvent(epService, 20);
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new object[] {6, 60, "F"}, new object[] {9, 90, "I"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertion2HistoricalStarInner(EPServiceProvider epService) {
            string[] fields = "a,b,c,d".Split(',');
            string stmtText = "select TheString as a, IntPrimitive as b, s1.myvarchar as c, s2.myvarchar as d from " +
                    typeof(SupportBean).FullName + "#keepall as s0 " +
                    " inner join " +
                    " sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.mybigint'] as s1 " +
                    " on s1.myvarchar=s0.TheString " +
                    " inner join " +
                    " sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.myint'] as s2 " +
                    " on s2.myvarchar=s0.TheString ";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            epService.EPRuntime.SendEvent(new SupportBean("A", 10));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean("B", 3));
            EPAssertionUtil.AssertProps(listener.AssertOneGetNewAndReset(), fields, new object[]{"B", 3, "B", "B"});
    
            epService.EPRuntime.SendEvent(new SupportBean("D", 4));
            Assert.IsFalse(listener.IsInvoked);
    
            stmt.Dispose();
        }
    
        private void RunAssertionVariables(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            epService.EPAdministrator.CreateEPL("create variable int queryvar");
            epService.EPAdministrator.CreateEPL("on SupportBean set queryvar=IntPrimitive");
    
            string stmtText = "select myint from " +
                    " sql:MyDB ['select myint from mytesttable where ${queryvar} = mytesttable.mybigint'] as s0, " +
                    "A#keepall as s1";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportBeanEvent(epService, 5);
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
    
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(50, received.Get("myint"));
            stmt.Dispose();
    
            stmtText = "select myint from " +
                    "A#keepall as s1, " +
                    "sql:MyDB ['select myint from mytesttable where ${queryvar} = mytesttable.mybigint'] as s0";
    
            stmt = epService.EPAdministrator.CreateEPL(stmtText);
            listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendSupportBeanEvent(epService, 6);
            epService.EPRuntime.SendEvent(new SupportBean_A("A1"));
    
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(60, received.Get("myint"));
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchOM(EPServiceProvider epService) {
            string[] fields = ALL_FIELDS.Split(',');
            string sql = "select " + ALL_FIELDS + " from mytesttable where ${IntPrimitive} = mytesttable.mybigint";
    
            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create(fields);
            FromClause fromClause = FromClause.Create(
                    SQLStream.Create("MyDB", sql, "s0"),
                    FilterStream.Create(typeof(SupportBean).FullName, "s1").AddView(View.Create("time_batch", Expressions.Constant(10))
                    ));
            model.FromClause = fromClause;
            SerializableObjectCopier.Copy(epService.Container, model);
    
            Assert.AreEqual("select mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from sql:MyDB[\"select mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from mytesttable where ${IntPrimitive} = mytesttable.mybigint\"] as s0, " + typeof(SupportBean).FullName + "#time_batch(10) as s1",
                    model.ToEPL());
    
            EPStatement stmt = epService.EPAdministrator.Create(model);
            RuntestTimeBatch(epService, stmt);
            epService.EPAdministrator.CreateEPL(model.ToEPL()).Dispose();
        }
    
        private void RunAssertionTimeBatchCompile(EPServiceProvider epService) {
            string stmtText = "select " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBean).FullName + "#time_batch(10 sec) as s1";
    
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(stmtText);
            SerializableObjectCopier.Copy(epService.Container, model);
            EPStatement stmt = epService.EPAdministrator.Create(model);
            RuntestTimeBatch(epService, stmt);
        }
    
        private void RuntestTimeBatch(EPServiceProvider epService, EPStatement statement) {
            var fields = new[]{"myint"};
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, 10);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new object[] {100}});
    
            SendSupportBeanEvent(epService, 5);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new object[] {100}, new object[] {50}});
    
            SendSupportBeanEvent(epService, 2);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new object[] {100}, new object[] {50}, new object[] {20}});
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EventBean[] received = listener.LastNewData;
            Assert.AreEqual(3, received.Length);
            Assert.AreEqual(100, received[0].Get("myint"));
            Assert.AreEqual(50, received[1].Get("myint"));
            Assert.AreEqual(20, received[2].Get("myint"));
    
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, null);
    
            SendSupportBeanEvent(epService, 9);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new object[] {90}});
    
            SendSupportBeanEvent(epService, 8);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new object[] {90}, new object[] {80}});
    
            statement.Dispose();
        }
    
        private void RunAssertionInvalidSQL(EPServiceProvider epService) {
            string stmtText = "select myvarchar from " +
                    " sql:MyDB ['select mychar,, from mytesttable where '] as s0," +
                    typeof(SupportBeanComplexProps).FullName + " as s1";

            SupportMessageAssertUtil.TryInvalid(
                epService, stmtText,
                "Error starting statement: Error in statement 'select mychar,, from mytesttable where ', failed to obtain result metadata, consider turning off metadata interrogation via configuration, please check the statement, " +
                "reason: 42601: syntax error at or near");
                //" near ' from mytesttable where' at line 1");
        }
    
        private void RunAssertionInvalidBothHistorical(EPServiceProvider epService) {
            string sqlOne = "sql:MyDB ['select myvarchar from mytesttable where ${mychar} = mytesttable.mybigint']";
            string sqlTwo = "sql:MyDB ['select mychar from mytesttable where ${myvarchar} = mytesttable.mybigint']";
            string stmtText = "select s0.myvarchar as s0Name, s1.mychar as s1Name from " +
                    sqlOne + " as s0, " + sqlTwo + "  as s1";
    
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Circular dependency detected between historical streams [select s0.myvarchar as s0Name, s1.mychar as s1Name from sql:MyDB ['select myvarchar from mytesttable where ${mychar} = mytesttable.mybigint'] as s0, sql:MyDB ['select mychar from mytesttable where ${myvarchar} = mytesttable.mybigint']  as s1]", ex.Message);
            }
        }
    
        private void RunAssertionInvalidPropertyEvent(EPServiceProvider epService) {
            string stmtText = "select myvarchar from " +
                    " sql:MyDB ['select mychar from mytesttable where ${s1.xxx[0]} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBeanComplexProps).FullName + " as s1";
    
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                SupportMessageAssertUtil.AssertMessage(ex, "Error starting statement: Failed to validate from-clause database-access parameter expression 's1.xxx[0]': Failed to resolve property 's1.xxx[0]' to a stream or nested property in a stream");
            }
    
            stmtText = "select myvarchar from " +
                    " sql:MyDB ['select mychar from mytesttable where ${} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBeanComplexProps).FullName + " as s1";
            SupportMessageAssertUtil.TryInvalid(epService, stmtText,
                    "Missing expression within ${...} in SQL statement [");
        }
    
        private void RunAssertionInvalidPropertyHistorical(EPServiceProvider epService) {
            string stmtText = "select myvarchar from " +
                    " sql:MyDB ['select myvarchar from mytesttable where ${myvarchar} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBeanComplexProps).FullName + " as s1";
            SupportMessageAssertUtil.TryInvalid(epService, stmtText,
                    "Error starting statement: Invalid expression 'myvarchar' resolves to the historical data itself");
        }
    
        private void RunAssertionInvalid1Stream(EPServiceProvider epService) {
            string sql = "sql:MyDB ['select myvarchar, mybigint from mytesttable where ${mybigint} = myint']";
            string stmtText = "select myvarchar as s0Name from " + sql + " as s0";
    
            try {
                epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            } catch (EPStatementException ex) {
                Assert.AreEqual("Error starting statement: Invalid expression 'mybigint' resolves to the historical data itself [select myvarchar as s0Name from sql:MyDB ['select myvarchar, mybigint from mytesttable where ${mybigint} = myint'] as s0]", ex.Message);
            }
        }
    
        private void RunAssertionInvalidSubviews(EPServiceProvider epService) {
            string sql = "sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myint']#time(30 sec)";
            string stmtText = "select myvarchar as s0Name from " +
                    sql + " as s0, " + typeof(SupportBean).FullName + " as s1";
            SupportMessageAssertUtil.TryInvalid(epService, stmtText,
                    "Error starting statement: Historical data joins do not allow views onto the data, view 'time' is not valid in this context [select myvarchar as s0Name from sql:MyDB [");
        }
    
        private void RunAssertionStreamNamesAndRename(EPServiceProvider epService) {
            string stmtText = "select s1.a as mybigint, " +
                    " s1.b as myint," +
                    " s1.c as myvarchar," +
                    " s1.d as mychar," +
                    " s1.e as mybool," +
                    " s1.f as mynumeric," +
                    " s1.g as mydecimal," +
                    " s1.h as mydouble," +
                    " s1.i as myreal " +
                    " from " + typeof(SupportBean_S0).FullName + " as s0," +
                    " sql:MyDB ['select mybigint as a, " +
                    " myint as b," +
                    " myvarchar as c," +
                    " mychar as d," +
                    " mybool as e," +
                    " mynumeric as f," +
                    " mydecimal as g," +
                    " mydouble as h," +
                    " myreal as i " +
                    "from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEventS0(epService, 1);
            AssertReceived(listener, 1, 10, "A", "Z", true, 5000.0m, 100.0m, 1.2, 1.3f);
        }
    
        private void RunAssertionWithPattern(EPServiceProvider epService) {
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            string stmtText = "select mychar from " +
                    " sql:MyDB ['select mychar from mytesttable where mytesttable.mybigint = 2'] as s0," +
                    " pattern [every timer:interval(5 sec) ]";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            Assert.AreEqual("Y", listener.AssertOneGetNewAndReset().Get("mychar"));
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(9999));
            Assert.IsFalse(listener.IsInvoked);
    
            epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            Assert.AreEqual("Y", listener.AssertOneGetNewAndReset().Get("mychar"));
    
            // with variable
            epService.EPAdministrator.CreateEPL("create variable long VarLastTimestamp = 0");
            string epl = "@Name('Poll every 5 seconds') insert into PollStream" +
                    " select * from pattern[every timer:interval(5 sec)]," +
                    " sql:MyDB ['select mychar from mytesttable where mytesttable.mybigint > ${VarLastTimestamp}'] as s0";
            EPStatementObjectModel model = epService.EPAdministrator.CompileEPL(epl);
            epService.EPAdministrator.Create(model);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionPropertyResolution(EPServiceProvider epService) {
            string stmtText = "select " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s1.arrayProperty[0]} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBeanComplexProps).FullName + " as s1";
            // s1.arrayProperty[0] returns 10 for that bean
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            AssertReceived(listener, 10, 100, "J", "P", true, null, 1000.0m, 10.2, 10.3f);
    
            statement.Dispose();
        }
    
        private void RunAssertionSimpleJoinLeft(EPServiceProvider epService) {
            string stmtText = "select " + ALL_FIELDS + " from " +
                    typeof(SupportBean_S0).FullName + " as s0," +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEventS0(epService, 1);
            AssertReceived(listener, 1, 10, "A", "Z", true, 5000.0m, 100.0m, 1.2, 1.3f);
    
            statement.Dispose();
        }
    
        private void RunAssertionRestartStatement(EPServiceProvider epService) {
            string stmtText = "select mychar from " +
                    typeof(SupportBean_S0).FullName + " as s0," +
                    " sql:MyDB ['select mychar from mytesttable where ${id} = mytesttable.mybigint'] as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            // Too many connections unless the stop actually relieves them
            for (int i = 0; i < 100; i++) {
                statement.Stop();
    
                SendEventS0(epService, 1);
                Assert.IsFalse(listener.IsInvoked);
    
                statement.Start();
                SendEventS0(epService, 1);
                Assert.AreEqual("Z", listener.AssertOneGetNewAndReset().Get("mychar"));
            }
    
            statement.Dispose();
        }
    
        private void RunAssertionSimpleJoinRight(EPServiceProvider epService) {
            string stmtText = "select " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${id} = mytesttable.mybigint'] as s0," +
                    typeof(SupportBean_S0).FullName + " as s1";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            EventType eventType = statement.EventType;
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("mybigint"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myint"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("myvarchar"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mychar"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("mybool"));
            Assert.AreEqual(typeof(decimal?), eventType.GetPropertyType("mynumeric"));
            Assert.AreEqual(typeof(decimal?), eventType.GetPropertyType("mydecimal"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mydouble"));
            Assert.AreEqual(typeof(float?), eventType.GetPropertyType("myreal"));
    
            SendEventS0(epService, 1);
            AssertReceived(listener, 1, 10, "A", "Z", true, 5000.0m, 100.0m, 1.2, 1.3f);
    
            statement.Dispose();
        }
    
        private void AssertReceived(
            SupportUpdateListener listener,
            long mybigint, 
            int myint,
            string myvarchar,
            string mychar, 
            bool mybool,
            decimal? mynumeric,
            decimal? mydecimal,
            double? mydouble, 
            float? myreal)
        {
            EventBean theEvent = listener.AssertOneGetNewAndReset();
            AssertReceived(theEvent, mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal);
        }
    
        private void AssertReceived(
            EventBean theEvent,
            long mybigint, 
            int? myint,
            string myvarchar,
            string mychar,
            bool? mybool,
            decimal? mynumeric,
            decimal? mydecimal,
            double? mydouble, 
            float? myreal)
        {
            Assert.AreEqual(mybigint, theEvent.Get("mybigint"));
            Assert.AreEqual(myint, theEvent.Get("myint"));
            Assert.AreEqual(myvarchar, theEvent.Get("myvarchar"));
            Assert.AreEqual(mychar, theEvent.Get("mychar"));
            Assert.AreEqual(mybool, theEvent.Get("mybool"));
            Assert.AreEqual(mynumeric, theEvent.Get("mynumeric"));
            Assert.AreEqual(mydecimal, theEvent.Get("mydecimal"));
            Assert.AreEqual(mydouble, theEvent.Get("mydouble"));
            object r = theEvent.Get("myreal");
            Assert.AreEqual(myreal, theEvent.Get("myreal"));
        }

        private void RunAssertionSQLDatabaseConnection(EPServiceProvider epService) {
            var dbProviderFactoryManager = SupportContainer.Instance
                .Resolve<DbProviderFactoryManager>();
            var dbProviderFactory = dbProviderFactoryManager.GetFactory(
                SupportDatabaseService.PGSQLDB_PROVIDER_TYPE);

            var properties = SupportDatabaseService.DefaultProperties;
            var builder = dbProviderFactory.CreateConnectionStringBuilder();
            foreach (var keyValuePair in properties) {
                builder[keyValuePair.Key] = keyValuePair.Value;
            }
            using (var connection = dbProviderFactory.CreateConnection()) {
                connection.ConnectionString = builder.ConnectionString;
                connection.Open();

                using (var statement = connection.CreateCommand()) {
                    statement.CommandText = "SELECT * FROM mytesttable";
                    statement.CommandType = CommandType.Text;

                    using (statement.ExecuteReader()) {
                        ;
                    }
                }
            }
        }
    
        private void SendEventS0(EPServiceProvider epService, int id) {
            var bean = new SupportBean_S0(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBeanEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace

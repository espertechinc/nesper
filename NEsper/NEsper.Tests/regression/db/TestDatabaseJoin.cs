///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.soda;
using com.espertech.esper.client.time;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;
using com.espertech.esper.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabaseJoin
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.LoggingConfig.IsEnableQueryPlan = true;
            configuration.AddDatabaseReference("MyDB", configDB);

            _epService = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _epService.Dispose();
        }

        #endregion

        private const String ALL_FIELDS =
            "mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";

        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;

        private void RuntestTimeBatch(EPStatement statement)
        {
            var fields = new[] {"myint"};
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, null);

            SendSupportBeanEvent(10);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new Object[] {100}});

            SendSupportBeanEvent(5);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                                                  new[] {new Object[] {100}, new Object[] {50}});

            SendSupportBeanEvent(2);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                                                  new[] {new Object[] {100}, new Object[] {50}, new Object[] {20}});

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            EventBean[] received = _listener.LastNewData;
            Assert.AreEqual(3, received.Length);
            Assert.AreEqual(100, received[0].Get("myint"));
            Assert.AreEqual(50, received[1].Get("myint"));
            Assert.AreEqual(20, received[2].Get("myint"));

            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, null);

            SendSupportBeanEvent(9);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new[] {new Object[] {90}});

            SendSupportBeanEvent(8);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields,
                                                  new[] {new Object[] {90}, new Object[] {80}});
        }

        private void AssertReceived(long mybigint,
                                    int myint,
                                    String myvarchar,
                                    String mychar,
                                    bool mybool,
                                    decimal? mynumeric,
                                    decimal? mydecimal,
                                    Double mydouble,
                                    Double myreal)
        {
            EventBean theEvent = _listener.AssertOneGetNewAndReset();
            AssertReceived(theEvent, mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal);
        }

        private void AssertReceived(EventBean theEvent,
                                    long? mybigint,
                                    int? myint,
                                    String myvarchar,
                                    String mychar,
                                    bool? mybool,
                                    decimal? mynumeric,
                                    decimal? mydecimal,
                                    Double mydouble,
                                    Double myreal)
        {
            Assert.AreEqual(mybigint, theEvent.Get("mybigint"));
            Assert.AreEqual(myint, theEvent.Get("myint"));
            Assert.AreEqual(myvarchar, theEvent.Get("myvarchar"));
            Assert.AreEqual(mychar, theEvent.Get("mychar"));
            Assert.AreEqual(mybool, theEvent.Get("mybool"));
            Assert.AreEqual(mynumeric, theEvent.Get("mynumeric"));
            Assert.AreEqual(mydecimal, theEvent.Get("mydecimal"));
            Assert.AreEqual(mydouble, theEvent.Get("mydouble"));
            Object r = theEvent.Get("myreal");
            Assert.AreEqual(myreal, theEvent.Get("myreal"));
        }

        private void SendEventS0(int id)
        {
            var bean = new SupportBean_S0(id);
            _epService.EPRuntime.SendEvent(bean);
        }

        private void SendSupportBeanEvent(int intPrimitive)
        {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }

        [Test]
        public void Test2HistoricalStar()
        {
            String[] fields = "IntPrimitive,myint,myvarchar".Split(',');
            String stmtText = "select IntPrimitive, myint, myvarchar from " +
                              typeof(SupportBean).FullName + ".win:keepall() as s0, " +
                              " sql:MyDB ['select myint from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s1," +
                              " sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s2 ";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            SendSupportBeanEvent(6);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {6, 60, "F"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new[] {new Object[] {6, 60, "F"}});

            SendSupportBeanEvent(9);
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {9, 90, "I"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {6, 60, "F"}, new Object[] {9, 90, "I"}});

            SendSupportBeanEvent(20);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                                                  new[] {new Object[] {6, 60, "F"}, new Object[] {9, 90, "I"}});

            stmt.Dispose();
        }

        [Test]
        public void Test2HistoricalStarInner()
        {
            String[] fields = "a,b,c,d".Split(',');
            String stmtText = "select TheString as a, IntPrimitive as b, s1.myvarchar as c, s2.myvarchar as d from " +
                              typeof(SupportBean).FullName + ".win:keepall() as s0 " +
                              " inner join " +
                              " sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.mybigint'] as s1 " +
                              " on s1.myvarchar=s0.TheString " +
                              " inner join " +
                              " sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} <> mytesttable.myint'] as s2 " +
                              " on s2.myvarchar=s0.TheString ";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, null);

            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("A", 10));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new SupportBean("B", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"B", 3, "B", "B"});

            _epService.EPRuntime.SendEvent(new SupportBean("D", 4));
            Assert.IsFalse(_listener.IsInvoked);

            stmt.Dispose();
        }

        [Test]
        public void Test3Stream()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("SupportBeanTwo", typeof(SupportBeanTwo));

            String stmtText = "select * from SupportBean.std:lastevent() sb, SupportBeanTwo.std:lastevent() sbt, " +
                              "sql:MyDB ['select myint from mytesttable'] as s1 " +
                              "  where sb.TheString = sbt.stringTwo and s1.myint = sbt.IntPrimitiveTwo";

            var statement = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(stmtText);
            Assert.IsFalse(statement.StatementContext.IsStatelessSelect);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T1", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("T1", -1));

            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T2", 30));
            _epService.EPRuntime.SendEvent(new SupportBean("T2", -1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                                           "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[] {"T2", "T2", 30});

            _epService.EPRuntime.SendEvent(new SupportBean("T3", -1));
            _epService.EPRuntime.SendEvent(new SupportBeanTwo("T3", 40));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(),
                                           "sb.TheString,sbt.stringTwo,s1.myint".Split(','), new Object[] {"T3", "T3", 40});
        }

        [Test]
        public void TestInvalid1Stream()
        {
            String sql = "sql:MyDB ['select myvarchar, mybigint from mytesttable where ${mybigint} = myint']";
            String stmtText = "select myvarchar as s0Name from " + sql + " as s0";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Invalid expression 'mybigint' resolves to the historical data itself [select myvarchar as s0Name from sql:MyDB ['select myvarchar, mybigint from mytesttable where ${mybigint} = myint'] as s0]",
                    ex.Message);
            }
        }

        [Test]
        public void TestInvalidBothHistorical()
        {
            String sqlOne = "sql:MyDB ['select myvarchar from mytesttable where ${mychar} = mytesttable.mybigint']";
            String sqlTwo = "sql:MyDB ['select mychar from mytesttable where ${myvarchar} = mytesttable.mybigint']";
            String stmtText = "select s0.myvarchar as s0Name, s1.mychar as s1Name from " +
                              sqlOne + " as s0, " + sqlTwo + "  as s1";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Circular dependency detected between historical streams [select s0.myvarchar as s0Name, s1.mychar as s1Name from sql:MyDB ['select myvarchar from mytesttable where ${mychar} = mytesttable.mybigint'] as s0, sql:MyDB ['select mychar from mytesttable where ${myvarchar} = mytesttable.mybigint']  as s1]",
                    ex.Message);
            }
        }

        [Test]
        public void TestInvalidPropertyEvent()
        {
            String stmtText = "select myvarchar from " +
                              " sql:MyDB ['select mychar from mytesttable where ${s1.xxx[0]} = mytesttable.mybigint'] as s0," +
                              typeof(SupportBeanComplexProps).FullName + " as s1";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Failed to validate from-clause database-access parameter expression 's1.xxx[0]': Failed to resolve property 's1.xxx[0]' to a stream or nested property in a stream [select myvarchar from  sql:MyDB ['select mychar from mytesttable where ${s1.xxx[0]} = mytesttable.mybigint'] as s0,com.espertech.esper.support.bean.SupportBeanComplexProps as s1]",
                    ex.Message);
            }

            stmtText = "select myvarchar from " +
                       " sql:MyDB ['select mychar from mytesttable where ${} = mytesttable.mybigint'] as s0," +
                       typeof(SupportBeanComplexProps).FullName + " as s1";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Missing expression within ${...} in SQL statement [select myvarchar from  sql:MyDB ['select mychar from mytesttable where ${} = mytesttable.mybigint'] as s0,com.espertech.esper.support.bean.SupportBeanComplexProps as s1]",
                    ex.Message);
            }
        }

        [Test]
        public void TestInvalidPropertyHistorical()
        {
            String stmtText = "select myvarchar from " +
                              " sql:MyDB ['select myvarchar from mytesttable where ${myvarchar} = mytesttable.mybigint'] as s0," +
                              typeof(SupportBeanComplexProps).FullName + " as s1";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Invalid expression 'myvarchar' resolves to the historical data itself [select myvarchar from  sql:MyDB ['select myvarchar from mytesttable where ${myvarchar} = mytesttable.mybigint'] as s0,com.espertech.esper.support.bean.SupportBeanComplexProps as s1]",
                    ex.Message);
            }
        }

        [Test]
        public void TestInvalidSQL()
        {
            String stmtText = "select myvarchar from " +
                              " sql:MyDB ['select mychar,, from mytesttable where '] as s0," +
                              typeof(SupportBeanComplexProps).FullName + " as s1";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Error in statement 'select mychar,, from mytesttable where ', failed to obtain result metadata, consider turning off metadata interrogation via configuration, please check the statement, reason: You have an error in your SQL syntax; check the manual that corresponds to your MySQL server version for the right syntax to use near ' from mytesttable where' at line 1 [select myvarchar from  sql:MyDB ['select mychar,, from mytesttable where '] as s0,com.espertech.esper.support.bean.SupportBeanComplexProps as s1]",
                    ex.Message);
            }
        }

        [Test]
        public void TestInvalidSubviews()
        {
            String sql =
                "sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myint'].win:time(30 sec)";
            String stmtText = "select myvarchar as s0Name from " +
                              sql + " as s0, " + typeof(SupportBean).FullName + " as s1";

            try
            {
                _epService.EPAdministrator.CreateEPL(stmtText);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(
                    "Error starting statement: Historical data joins do not allow views onto the data, view 'win:time' is not valid in this context [select myvarchar as s0Name from sql:MyDB ['select myvarchar from mytesttable where ${IntPrimitive} = mytesttable.myint'].win:time(30 sec) as s0, com.espertech.esper.support.bean.SupportBean as s1]",
                    ex.Message);
            }
        }

        [Test]
        public void TestPropertyResolution()
        {
            String stmtText = "select " + ALL_FIELDS + " from " +
                              " sql:MyDB ['select " + ALL_FIELDS +
                              " from mytesttable where ${s1.ArrayProperty[0]} = mytesttable.mybigint'] as s0," +
                              typeof(SupportBeanComplexProps).FullName + " as s1";
            // s1.ArrayProperty[0] returns 10 for that bean

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            _epService.EPRuntime.SendEvent(SupportBeanComplexProps.MakeDefaultBean());
            AssertReceived(10, 100, "J", "P", true, null, 1000, 10.2, 10.3);
        }

        [Test]
        public void TestRestartStatement()
        {
            String stmtText = "select mychar from " +
                              typeof(SupportBean_S0).FullName + " as s0," +
                              " sql:MyDB ['select mychar from mytesttable where ${id} = mytesttable.mybigint'] as s1";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            // Too many connections unless the stop actually relieves them
            for (int i = 0; i < 100; i++)
            {
                statement.Stop();

                SendEventS0(1);
                Assert.IsFalse(_listener.IsInvoked);

                statement.Start();
                SendEventS0(1);
                Assert.AreEqual("Z", _listener.AssertOneGetNewAndReset().Get("mychar"));
            }
        }

        [Test]
        public void TestSimpleJoinLeft()
        {
            String stmtText = "select " + ALL_FIELDS + " from " +
                              typeof(SupportBean_S0).FullName + " as s0," +
                              " sql:MyDB ['select " + ALL_FIELDS +
                              " from mytesttable where ${id} = mytesttable.mybigint'] as s1";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            SendEventS0(1);
            AssertReceived(1, 10, "A", "Z", true, 5000, 100, 1.2, 1.3);
        }

        [Test]
        public void TestSimpleJoinRight()
        {
            String stmtText = "select " + ALL_FIELDS + " from " +
                              " sql:MyDB ['select " + ALL_FIELDS +
                              " from mytesttable where ${id} = mytesttable.mybigint'] as s0," +
                              typeof(SupportBean_S0).FullName + " as s1";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            EventType eventType = statement.EventType;
            Assert.AreEqual(typeof(long?), eventType.GetPropertyType("mybigint"));
            Assert.AreEqual(typeof(int?), eventType.GetPropertyType("myint"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("myvarchar"));
            Assert.AreEqual(typeof(string), eventType.GetPropertyType("mychar"));
            Assert.AreEqual(typeof(bool?), eventType.GetPropertyType("mybool"));
            Assert.AreEqual(typeof(decimal?), eventType.GetPropertyType("mynumeric"));
            Assert.AreEqual(typeof(decimal?), eventType.GetPropertyType("mydecimal"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("mydouble"));
            Assert.AreEqual(typeof(double?), eventType.GetPropertyType("myreal"));

            SendEventS0(1);
            AssertReceived(1, 10, "A", "Z", true, 5000, 100, 1.2, 1.3);
        }

        [Test]
        public void TestStreamNamesAndRename()
        {
            String stmtText = "select s1.a as mybigint, " +
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

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            SendEventS0(1);
            AssertReceived(1, 10, "A", "Z", true, 5000, 100, 1.2, 1.3);
        }

        [Test]
        public void TestTimeBatchCompile()
        {
            String stmtText = "select " + ALL_FIELDS + " from " +
                              " sql:MyDB ['select " + ALL_FIELDS +
                              " from mytesttable where ${IntPrimitive} = mytesttable.mybigint'] as s0," +
                              typeof(SupportBean).FullName + ".win:time_batch(10 sec) as s1";

            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(stmtText);
            SerializableObjectCopier.Copy(model);
            EPStatement stmt = _epService.EPAdministrator.Create(model);
            RuntestTimeBatch(stmt);
        }

        [Test]
        public void TestTimeBatchEPL()
        {
            String stmtText = "select " + ALL_FIELDS + " from " +
                              " sql:MyDB ['select " + ALL_FIELDS +
                              " from mytesttable \n\r where ${IntPrimitive} = mytesttable.mybigint'] as s0," +
                              typeof(SupportBean).FullName + ".win:time_batch(10 sec) as s1";
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            RuntestTimeBatch(stmt);
        }

        [Test]
        public void TestTimeBatchOM()
        {
            String[] fields = ALL_FIELDS.Split(',');
            String sql = "select " + ALL_FIELDS + " from mytesttable where ${IntPrimitive} = mytesttable.mybigint";

            var model = new EPStatementObjectModel();
            model.SelectClause = SelectClause.Create(fields);
            FromClause fromClause = FromClause.Create(
                SQLStream.Create("MyDB", sql, "s0"),
                FilterStream.Create(typeof(SupportBean).FullName, "s1").AddView(View.Create("win", "time_batch",
                                                                                             Expressions.Constant(10))
                    ));
            model.FromClause = fromClause;
            SerializableObjectCopier.Copy(model);

            Assert.AreEqual(
                "select mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from sql:MyDB[\"select mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal from mytesttable where ${IntPrimitive} = mytesttable.mybigint\"] as s0, com.espertech.esper.support.bean.SupportBean.win:time_batch(10) as s1",
                model.ToEPL());

            EPStatement stmt = _epService.EPAdministrator.Create(model);
            RuntestTimeBatch(stmt);

            stmt = _epService.EPAdministrator.CreateEPL(model.ToEPL());
        }

        [Test]
        public void TestVariables()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _epService.EPAdministrator.Configuration.AddEventType("A", typeof(SupportBean_A));
            _epService.EPAdministrator.CreateEPL("create variable int queryvar");
            _epService.EPAdministrator.CreateEPL("on SupportBean set queryvar=IntPrimitive");

            String stmtText = "select myint from " +
                              " sql:MyDB ['select myint from mytesttable where ${queryvar} = mytesttable.mybigint'] as s0, " +
                              "A.win:keepall() as s1";

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            SendSupportBeanEvent(5);
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));

            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(50, received.Get("myint"));
            stmt.Dispose();

            stmtText = "select myint from " +
                       "A.win:keepall() as s1, " +
                       "sql:MyDB ['select myint from mytesttable where ${queryvar} = mytesttable.mybigint'] as s0";

            stmt = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            stmt.Events += _listener.Update;

            SendSupportBeanEvent(6);
            _epService.EPRuntime.SendEvent(new SupportBean_A("A1"));

            received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(60, received.Get("myint"));
        }

        [Test]
        public void TestWithPattern()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));

            String stmtText = "select mychar from " +
                              " sql:MyDB ['select mychar from mytesttable where mytesttable.mybigint = 2'] as s0," +
                              " pattern [every timer:interval(5 sec) ]";

            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;

            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            Assert.AreEqual("Y", _listener.AssertOneGetNewAndReset().Get("mychar"));

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(9999));
            Assert.IsFalse(_listener.IsInvoked);

            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            Assert.AreEqual("Y", _listener.AssertOneGetNewAndReset().Get("mychar"));

            // with variable
            _epService.EPAdministrator.CreateEPL("create variable long VarLastTimestamp = 0");
            String epl = "@Name('Poll every 5 seconds') insert into PollStream" +
                         " select * from pattern[every timer:interval(5 sec)]," +
                         " sql:MyDB ['select mychar from mytesttable where mytesttable.mybigint > ${VarLastTimestamp}'] as s0";
            EPStatementObjectModel model = _epService.EPAdministrator.CompileEPL(epl);
            _epService.EPAdministrator.Create(model);
        }
    }
}

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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    public class ExecDatabase2StreamOuterJoin : RegressionExecution {
        private const string ALL_FIELDS = "mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";
    
        public override void Configure(Configuration configuration) {
            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configDB.ConnectionCatalog = "test";
            configDB.ConnectionTransactionIsolation = IsolationLevel.Serializable;

            configuration.AddDatabaseReference("MyDB", configDB);
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionOuterJoinLeftS0(epService);
            RunAssertionOuterJoinRightS1(epService);
            RunAssertionOuterJoinFullS0(epService);
            RunAssertionOuterJoinFullS1(epService);
            RunAssertionOuterJoinRightS0(epService);
            RunAssertionOuterJoinLeftS1(epService);
            RunAssertionLeftOuterJoinOnFilter(epService);
            RunAssertionRightOuterJoinOnFilter(epService);
            RunAssertionOuterJoinReversedOnFilter(epService);
        }
    
        private void RunAssertionOuterJoinLeftS0(EPServiceProvider epService) {
            string stmtText = "select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 left outer join " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 on IntPrimitive = mybigint";
            TryOuterJoinResult(epService, stmtText);
        }
    
        private void RunAssertionOuterJoinRightS1(EPServiceProvider epService) {
            string stmtText = "select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 right outer join " +
                    typeof(SupportBean).FullName + " as s0 on IntPrimitive = mybigint";
            TryOuterJoinResult(epService, stmtText);
        }
    
        private void RunAssertionOuterJoinFullS0(EPServiceProvider epService) {
            string stmtText = "select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 full outer join " +
                    typeof(SupportBean).FullName + " as s0 on IntPrimitive = mybigint";
            TryOuterJoinResult(epService, stmtText);
        }
    
        private void RunAssertionOuterJoinFullS1(EPServiceProvider epService) {
            string stmtText = "select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 full outer join " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 on IntPrimitive = mybigint";
            TryOuterJoinResult(epService, stmtText);
        }
    
        private void RunAssertionOuterJoinRightS0(EPServiceProvider epService) {
            string stmtText = "select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 right outer join " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 on IntPrimitive = mybigint";
            TryOuterJoinNoResult(epService, stmtText);
        }
    
        private void RunAssertionOuterJoinLeftS1(EPServiceProvider epService) {
            string stmtText = "select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 left outer join " +
                    typeof(SupportBean).FullName + " as s0 on IntPrimitive = mybigint";
            TryOuterJoinNoResult(epService, stmtText);
        }
    
        private void RunAssertionLeftOuterJoinOnFilter(EPServiceProvider epService) {
            string[] fields = "MyInt,myint".Split(',');
            string stmtText = "@IterableUnbound select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 " +
                    " left outer join " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 " +
                    "on TheString = myvarchar";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            // Result as the SQL query returns 1 row and therefore the on-clause filters it out, but because of left out still getting a row
            SendEvent(epService, 1, "xxx");
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {1, null}});
    
            // Result as the SQL query returns 0 rows
            SendEvent(epService, -1, "xxx");
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {-1, null}});
    
            SendEvent(epService, 2, "B");
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, received.Get("MyInt"));
            AssertReceived(received, 2L, 20, "B", "Y", false, 100.0m, 200.0m, 2.2d, 2.3f);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {2, 20}});
    
            statement.Dispose();
        }
    
        private void RunAssertionRightOuterJoinOnFilter(EPServiceProvider epService) {
            string[] fields = "MyInt,myint".Split(',');
            string stmtText = "@IterableUnbound select s0.IntPrimitive as MyInt, " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ExecDatabase2StreamOuterJoin.ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 right outer join " +
                    typeof(SupportBean).FullName + " as s0 on TheString = myvarchar";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            // No result as the SQL query returns 1 row and therefore the on-clause filters it out
            SendEvent(epService, 1, "xxx");
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {1, null}});
    
            // Result as the SQL query returns 0 rows
            SendEvent(epService, -1, "xxx");
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {-1, null}});
    
            SendEvent(epService, 2, "B");
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, received.Get("MyInt"));
            AssertReceived(received, 2L, 20, "B", "Y", false, 100.0m, 200.0m, 2.2d, 2.3f);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {2, 20}});
    
            statement.Dispose();
        }
    
        private void RunAssertionOuterJoinReversedOnFilter(EPServiceProvider epService) {
            // PGSQL does not care about "case" and columns are returned in lowercase,
            // so please be aware of this.  I have converted the use case accordingly.
            string[] fields = "MyInt,myvarcharx".Split(',');
            string stmtText = "select s0.IntPrimitive as MyInt, myvarcharx from " +
                    typeof(SupportBean).FullName + "#keepall as s0 " +
                    " right outer join " +
                    " sql:MyDB ['select myvarchar as myvarcharx from mytesttable'] as s1 " +
                    "on TheString = myvarcharx";
    
            EPStatement statement = epService.EPAdministrator.CreateEPL(stmtText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            // No result as the SQL query returns 1 row and therefore the on-clause filters it out
            SendEvent(epService, 1, "xxx");
            Assert.IsFalse(listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            SendEvent(epService, -1, "A");
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, received.Get("MyInt"));
            Assert.AreEqual("A", received.Get("myvarcharx"));
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new object[][]{new object[] {-1, "A"}});
    
            statement.Dispose();
        }
    
        private void TryOuterJoinNoResult(EPServiceProvider epService, string statementText) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, 2);
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, received.Get("MyInt"));
            AssertReceived(received, 2L, 20, "B", "Y", false, 100.0m, 200.0m, 2.2d, 2.3f);
    
            SendEvent(epService, 11);
            Assert.IsFalse(listener.IsInvoked);
    
            statement.Dispose();
        }
    
        private void TryOuterJoinResult(EPServiceProvider epService, string statementText) {
            EPStatement statement = epService.EPAdministrator.CreateEPL(statementText);
            var listener = new SupportUpdateListener();
            statement.Events += listener.Update;
    
            SendEvent(epService, 1);
            EventBean received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, received.Get("MyInt"));
            AssertReceived(received, 1L, 10, "A", "Z", true, 5000.0m, 100.0m, 1.2d, 1.3f);
    
            SendEvent(epService, 11);
            received = listener.AssertOneGetNewAndReset();
            Assert.AreEqual(11, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
    
            statement.Dispose();
        }
    
        private void AssertReceived(
            EventBean theEvent, 
            long? mybigint, 
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
            Assert.AreEqual(myreal, theEvent.Get("myreal"));
        }
    
        private void SendEvent(EPServiceProvider epService, int intPrimitive) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(EPServiceProvider epService, int intPrimitive, string theString) {
            var bean = new SupportBean();
            bean.IntPrimitive = intPrimitive;
            bean.TheString = theString;
            epService.EPRuntime.SendEvent(bean);
        }
    }
} // end of namespace

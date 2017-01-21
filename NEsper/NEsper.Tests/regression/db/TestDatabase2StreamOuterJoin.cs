///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.db
{
    [TestFixture]
    public class TestDatabase2StreamOuterJoin 
    {
        private const String ALL_FIELDS = "mybigint, myint, myvarchar, mychar, mybool, mynumeric, mydecimal, mydouble, myreal";
    
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
    
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddDatabaseReference("MyDB", configDB);
    
            _epService = EPServiceProviderManager.GetProvider("TestDatabaseJoinRetained", configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
        }
    
        [Test]
        public void TestOuterJoinLeftS0()
        {
            String stmtText = "select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 left outer join " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 on IntPrimitive = mybigint";
            TryOuterJoinResult(stmtText);
        }
    
        [Test]
        public void TestOuterJoinRightS1()
        {
            String stmtText = "select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 right outer join " +
                    typeof(SupportBean).FullName + " as s0 on IntPrimitive = mybigint";
            TryOuterJoinResult(stmtText);
        }
    
        [Test]
        public void TestOuterJoinFullS0()
        {
            String stmtText = "select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 full outer join " +
                    typeof(SupportBean).FullName + " as s0 on IntPrimitive = mybigint";
            TryOuterJoinResult(stmtText);
        }
    
        [Test]
        public void TestOuterJoinFullS1()
        {
            String stmtText = "select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 full outer join " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 on IntPrimitive = mybigint";
            TryOuterJoinResult(stmtText);
        }
    
        [Test]
        public void TestOuterJoinRightS0()
        {
            String stmtText = "select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    typeof(SupportBean).FullName + " as s0 right outer join " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 on IntPrimitive = mybigint";
            TryOuterJoinNoResult(stmtText);
        }
    
        [Test]
        public void TestOuterJoinLeftS1()
        {
            String stmtText = "select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 left outer join " +
                    typeof(SupportBean).FullName + " as s0 on IntPrimitive = mybigint";
            TryOuterJoinNoResult(stmtText);
        }
    
        [Test]
        public void TestLeftOuterJoinOnFilter()
        {
            String[] fields = "MyInt,myint".Split(',');
            String stmtText = string.Format(
                "@IterableUnbound select s0.IntPrimitive as MyInt, {0} from {1} as s0 " + 
                " left outer join " + 
                " sql:MyDB ['select {0} from mytesttable where ${{s0.IntPrimitive}} = mytesttable.mybigint'] as s1 " + 
                "on TheString = myvarchar",
                ALL_FIELDS, typeof(SupportBean).FullName);
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            // Result as the SQL query returns 1 row and therefore the on-clause filters it out, but because of left out still getting a row
            SendEvent(1, "xxx");
            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {1, null}});
    
            // Result as the SQL query returns 0 rows
            SendEvent(-1, "xxx");
            received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {-1, null}});
    
            SendEvent(2, "B");
            received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, received.Get("MyInt"));
            AssertReceived(received, 2l, 20, "B", "Y", false, 100m, 200m, 2.2d, 2.3d);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {2, 20}});
        }
    
        [Test]
        public void TestRightOuterJoinOnFilter()
        {
            String[] fields = "MyInt,myint".Split(',');
            String stmtText = "@IterableUnbound select s0.IntPrimitive as MyInt, " + ALL_FIELDS + " from " +
                    " sql:MyDB ['select " + ALL_FIELDS + " from mytesttable where ${s0.IntPrimitive} = mytesttable.mybigint'] as s1 right outer join " +
                    typeof(SupportBean).FullName + " as s0 on TheString = myvarchar";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            // No result as the SQL query returns 1 row and therefore the on-clause filters it out
            SendEvent(1, "xxx");
            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {1, null}});
    
            // Result as the SQL query returns 0 rows
            SendEvent(-1, "xxx");
            received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {-1, null}});
    
            SendEvent(2, "B");
            received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, received.Get("MyInt"));
            AssertReceived(received, 2l, 20, "B", "Y", false, 100m, 200m, 2.2d, 2.3d);
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {2, 20}});
        }
    
        [Test]
        public void TestOuterJoinReversedOnFilter()
        {
            String[] fields = "MyInt,MyVarChar".Split(',');
            String stmtText = "select s0.IntPrimitive as MyInt, MyVarChar from " +
                    typeof(SupportBean).FullName + ".win:keepall() as s0 " +
                    " right outer join " +
                    " sql:MyDB ['select myvarchar MyVarChar from mytesttable'] as s1 " +
                    "on TheString = MyVarChar";
    
            EPStatement statement = _epService.EPAdministrator.CreateEPL(stmtText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            // No result as the SQL query returns 1 row and therefore the on-clause filters it out
            SendEvent(1, "xxx");
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(statement.GetEnumerator(), fields, null);
    
            SendEvent(-1, "A");
            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(-1, received.Get("MyInt"));
            Assert.AreEqual("A", received.Get("MyVarChar"));
            EPAssertionUtil.AssertPropsPerRow(statement.GetEnumerator(), fields, new [] { new Object[] {-1, "A"}});
        }
    
        public void TryOuterJoinNoResult(String statementText)
        {
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            SendEvent(2);
            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(2, received.Get("MyInt"));
            AssertReceived(received, 2l, 20, "B", "Y", false, 100m, 200m, 2.2d, 2.3d);
    
            SendEvent(11);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        public void TryOuterJoinResult(String statementText)
        {
            EPStatement statement = _epService.EPAdministrator.CreateEPL(statementText);
            _listener = new SupportUpdateListener();
            statement.Events += _listener.Update;
    
            SendEvent(1);
            EventBean received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(1, received.Get("MyInt"));
            AssertReceived(received, 1l, 10, "A", "Z", true, 5000m, 100m, 1.2d, 1.3d);
    
            SendEvent(11);
            received = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual(11, received.Get("MyInt"));
            AssertReceived(received, null, null, null, null, null, null, null, null, null);
        }
    
        private static void AssertReceived(EventBean theEvent, long? mybigint, int? myint, string myvarchar, string mychar, bool? mybool, decimal? mynumeric, decimal? mydecimal, double? mydouble, double? myreal)
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
    
        private void SendEvent(int intPrimitive)
        {
            var bean = new SupportBean {IntPrimitive = intPrimitive};
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendEvent(int intPrimitive, String stringValue)
        {
            var bean = new SupportBean {IntPrimitive = intPrimitive, TheString = stringValue};
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}

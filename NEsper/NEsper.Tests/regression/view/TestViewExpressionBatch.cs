///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewExpressionBatch 
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestNewestEventOldestEvent()
        {
            // try with include-trigger-event
            String[] fields = new String[] {"TheString"};
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr_batch(newest_event.IntPrimitive != oldest_event.IntPrimitive, false)");
            stmtOne.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E3" } }, new Object[][] { new Object[] { "E1" }, new Object[] { "E2" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E4" }, new Object[] { "E5" }, new Object[] { "E6" } }, new Object[][] { new Object[] { "E3" } });
            stmtOne.Dispose();
    
            // try with include-trigger-event
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr_batch(newest_event.IntPrimitive != oldest_event.IntPrimitive, true)");
            stmtTwo.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E4" }, new Object[] { "E5" }, new Object[] { "E6" }, new Object[] { "E7" } }, new Object[][] { new Object[] { "E1" }, new Object[] { "E2" }, new Object[] { "E3" } });
        }
    
        [Test]
        public void TestLengthBatch()
        {
            String[] fields = new String[] {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr_batch(current_count >= 3, true)");
            stmt.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][]{new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastOldData(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][]{new Object[] {"E7"}, new Object[] {"E8"}, new Object[] {"E9"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastOldData(), fields, new Object[][]{new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}});
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestTimeBatch()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            String[] fields = new String[] {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr_batch(newest_timestamp - oldest_timestamp > 2000)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3100));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}, new Object[] {"E4"}, new Object[] {"E5"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5100));
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5101));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][]{new Object[] {"E6"}, new Object[] {"E7"}, new Object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastOldData(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}, new Object[] {"E4"}, new Object[] {"E5"}});
        }
    
        [Test]
        public void TestVariableBatch()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL("create variable bool POST = false");
    
            String[] fields = new String[] {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr_batch(POST)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SetVariableValue("POST", true);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1001));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E2" } }, new Object[][] { new Object[] { "E1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E3" } }, new Object[][] { new Object[] { "E2" } });
    
            _epService.EPRuntime.SetVariableValue("POST", false);
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 2));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SetVariableValue("POST", true);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2001));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E4" }, new Object[] { "E5" } }, new Object[][] { new Object[] { "E3" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E6" } }, new Object[][] { new Object[] { "E4" }, new Object[] { "E5" } });
    
            stmt.Stop();
        }
    
        [Test]
        public void TestDynamicTimeBatch()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL("create variable long SIZE = 1000");
    
            String[] fields = new String[] {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr_batch(newest_timestamp - oldest_timestamp > SIZE)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1900));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            Assert.IsFalse(_listener.IsInvoked);
            
            _epService.EPRuntime.SetVariableValue("SIZE", 500);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1901));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2300));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2500));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 0));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E3" }, new Object[] { "E4" }, new Object[] { "E5" } }, new Object[][] { new Object[] { "E1" }, new Object[] { "E2" } });
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3100));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SetVariableValue("SIZE", 999);
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3700));
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 0));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(4100));
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 0));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E6" }, new Object[] { "E7" }, new Object[] { "E8" } }, new Object[][] { new Object[] { "E3" }, new Object[] { "E4" }, new Object[] { "E5" } });
        }
    
        [Test]
        public void TestUDFBuiltin()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // not instrumented

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("udf", typeof(TestViewExpressionWindow.LocalUDF).FullName, "EvaluateExpiryUDF");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:expr_batch(udf(TheString, view_reference, expired_count))");
    
            TestViewExpressionWindow.LocalUDF.Result = true;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.AreEqual("E1", TestViewExpressionWindow.LocalUDF.Key);
            Assert.AreEqual(0, (int) TestViewExpressionWindow.LocalUDF.ExpiryCount);
            Assert.NotNull(TestViewExpressionWindow.LocalUDF.Viewref);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            TestViewExpressionWindow.LocalUDF.Result = false;
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            Assert.AreEqual("E3", TestViewExpressionWindow.LocalUDF.Key);
            Assert.AreEqual(0, (int) TestViewExpressionWindow.LocalUDF.ExpiryCount);
            Assert.NotNull(TestViewExpressionWindow.LocalUDF.Viewref);
        }
    
        [Test]
        public void TestInvalid() {
            TryInvalid("select * from SupportBean.win:expr_batch(1)",
                       "Error starting statement: Error attaching view to event stream: Invalid return value for expiry expression, expected a bool return value but received " + Name.Clean<int>() + " [select * from SupportBean.win:expr_batch(1)]");
    
            TryInvalid("select * from SupportBean.win:expr_batch((select * from SupportBean.std:lastevent()))",
                       "Error starting statement: Error attaching view to event stream: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean.win:expr_batch((select * from SupportBean.std:lastevent()))]");
        }
    
        public void TryInvalid(String epl, String message) {
            try {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex) {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        [Test]
        public void TestNamedWindowDelete() {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            
            String[] fields = new String[] {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window NW.win:expr_batch(current_count > 3) as SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPAdministrator.CreateEPL("insert into NW select * from SupportBean");
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from NW where TheString = id");
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E3"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E3"}, new Object[] {"E4"}, new Object[] {"E5"}}, null);
        }
    
        [Test]
        public void TestPrev() {
            String[] fields = new String[] {"val0"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select Prev(1, TheString) as val0 from SupportBean.win:expr_batch(current_count > 2)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            Assert.IsFalse(_listener.IsInvoked);
            
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {null}, new Object[] {"E1"}, new Object[] {"E2"}}, null);
        }
    
        [Test]
        public void TestEventPropBatch() {
            String[] fields = new String[] {"val0"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream TheString as val0 from SupportBean.win:expr_batch(IntPrimitive > 0)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E2" } }, new Object[][] { new Object[] { "E1" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", -1));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E3" }, new Object[] { "E4" } }, new Object[][] { new Object[] { "E2" } });
        }
    
        [Test]
        public void TestAggregation() {
            String[] fields = new String[] {"TheString"};
    
            // Test un-grouped
            EPStatement stmtUngrouped = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.win:expr_batch(sum(IntPrimitive) > 100)");
            stmtUngrouped.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 90));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 10));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 101));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E4" } }, new Object[][] { new Object[] { "E1" }, new Object[] { "E2" }, new Object[] { "E3" } });
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 99));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E5" }, new Object[] { "E6" }, new Object[] { "E7" } }, new Object[][] { new Object[] { "E4" } });
            stmtUngrouped.Dispose();
    
            // Test grouped
            EPStatement stmtGrouped = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.std:groupwin(IntPrimitive).win:expr_batch(sum(LongPrimitive) > 100)");
            stmtGrouped.Events += _listener.Update;
    
            SendEvent("E1", 1, 10);
            SendEvent("E2", 2, 10);
            SendEvent("E3", 1, 90);
            SendEvent("E4", 2, 80);
            SendEvent("E5", 2, 10);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E6", 2, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E2"}, new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}}, null);
    
            SendEvent("E7", 2, 50);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendEvent("E8", 1, 2);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E3"}, new Object[] {"E8"}}, null);
    
            SendEvent("E9", 2, 50);
            SendEvent("E10", 1, 101);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E10" } }, new Object[][] { new Object[] { "E1" }, new Object[] { "E3" }, new Object[] { "E8" } });
    
            SendEvent("E11", 2, 1);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] { "E7" }, new Object[] { "E9" }, new Object[] { "E11" } }, new Object[][] { new Object[] { "E2" }, new Object[] { "E4" }, new Object[] { "E5" }, new Object[] { "E6" } });
    
            SendEvent("E12", 1, 102);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E12"}}, new Object[][]{ new Object[]{"E10"}});
            stmtGrouped.Dispose();
    
            // Test on-delete
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window NW.win:expr_batch(sum(IntPrimitive) >= 10) as SupportBean");
            stmt.Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("insert into NW select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 8));
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from NW where TheString = id");
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 8));
            Assert.IsFalse(_listener.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E3"}, new Object[] {"E4"}}, null);
        }
    
        private void SendEvent(String theString, int intPrimitive, long longPrimitive) {
            SupportBean bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    }
}

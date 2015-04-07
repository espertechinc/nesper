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
using com.espertech.esper.compat.collections;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewExpressionWindow 
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
            String[] fields = {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr(newest_event.IntPrimitive = oldest_event.IntPrimitive)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E3" } }, new Object[][] { new Object[] { "E1" }, new Object[] { "E2" } });
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E3"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 3));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E4" } }, new Object[][] { new Object[] { "E3" } });
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E4"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E5"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E4"}, new Object[] {"E5"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E6"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields,
                    new Object[][] { new Object[] { "E7" } }, new Object[][] { new Object[] { "E4" }, new Object[] { "E5" }, new Object[] { "E6" } });
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E7"}});
        }
    
        [Test]
        public void TestLengthWindow()
        {
            String[] fields = {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:expr(current_count <= 2)");
            stmt.Events += _listener.Update;
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E2"}, new Object[] {"E3"}});
    
            stmt.Dispose();
        }
    
        [Test]
        public void TestTimeWindow()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
    
            String[] fields = {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr(oldest_timestamp > newest_timestamp - 2000)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1"});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1500));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E2"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3"});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2500));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 4));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}, new Object[] {"E4"}});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3000));
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 5));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E2"}, new Object[] {"E3"}, new Object[] {"E4"}, new Object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][]{new Object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new Object[][]{new Object[] {"E1"}});
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3499));
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 6));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E2"}, new Object[] {"E3"}, new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(3500));
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 7));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][]{new Object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new Object[][]{new Object[] {"E2"}, new Object[] {"E3"}});
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(10000));
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 8));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, fields, new Object[][]{new Object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.LastOldData, fields, new Object[][]{new Object[] {"E4"}, new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"}});
            _listener.Reset();
        }
    
        [Test]
        public void TestVariable()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL("create variable bool KEEP = true");
    
            String[] fields = {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr(KEEP)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}});
    
            _epService.EPRuntime.SetVariableValue("KEEP", false);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}});
            
            _listener.Reset();
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1001));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[]{"E1"});
            Assert.IsFalse(stmt.HasFirst());
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.LastNewData[0], fields, new Object[]{"E2"});
            EPAssertionUtil.AssertProps(_listener.LastOldData[0], fields, new Object[]{"E2"});
            _listener.Reset();
            Assert.IsFalse(stmt.HasFirst());
    
            _epService.EPRuntime.SetVariableValue("KEEP", true);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E3"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E3"}});
    
            stmt.Stop();
        }
    
        [Test]
        public void TestDynamicTimeWindow()
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
            _epService.EPAdministrator.CreateEPL("create variable long SIZE = 1000");
    
            String[] fields = {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select irstream * from SupportBean.win:expr(newest_timestamp - oldest_timestamp < SIZE)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(1000));
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}});
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(2000));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E2"}});
    
            _epService.EPRuntime.SetVariableValue("SIZE", 10000);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(5000));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPRuntime.SetVariableValue("SIZE", 2000);
    
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(6000));
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 0));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E3"}, new Object[] {"E4"}});
        }
    
        [Test]
        public void TestUDFBuiltin()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); } // not instrumented

            _epService.EPAdministrator.Configuration.AddPlugInSingleRowFunction("udf", typeof(LocalUDF).FullName, "EvaluateExpiryUDF");
            _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:expr(udf(TheString, view_reference, expired_count))");
    
            LocalUDF.Result = true;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 0));
            Assert.AreEqual("E1", LocalUDF.Key);
            Assert.AreEqual(0, (int) LocalUDF.ExpiryCount);
            Assert.NotNull(LocalUDF.Viewref);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 0));
    
            LocalUDF.Result = false;
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 0));
            Assert.AreEqual("E3", LocalUDF.Key);
            Assert.AreEqual(2, (int) LocalUDF.ExpiryCount);
            Assert.NotNull(LocalUDF.Viewref);
        }
    
        [Test]
        public void TestInvalid()
        {
            TryInvalid("select * from SupportBean.win:expr(1)",
                       "Error starting statement: Error attaching view to event stream: Invalid return value for expiry expression, expected a bool return value but received " + Name.Clean<int>() + " [select * from SupportBean.win:expr(1)]");
    
            TryInvalid("select * from SupportBean.win:expr((select * from SupportBean.std:lastevent()))",
                       "Error starting statement: Error attaching view to event stream: Invalid expiry expression: Sub-select, previous or prior functions are not supported in this context [select * from SupportBean.win:expr((select * from SupportBean.std:lastevent()))]");
        }
    
        public void TryInvalid(String epl, String message)
        {
            try
            {
                _epService.EPAdministrator.CreateEPL(epl);
                Assert.Fail();
            }
            catch (EPStatementException ex)
            {
                Assert.AreEqual(message, ex.Message);
            }
        }
    
        [Test]
        public void TestNamedWindowDelete()
        {
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            
            String[] fields = {"TheString"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window NW.win:expr(true) as SupportBean");
            stmt.Events += _listener.Update;
    
            _epService.EPAdministrator.CreateEPL("insert into NW select * from SupportBean");
            
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 3));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from NW where TheString = id");
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields, new Object[][]{new Object[] {"E1"}, new Object[] {"E3"}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetOldAndReset(), fields, new Object[]{"E2"});
        }
    
        [Test]
        public void TestPrev()
        {
            String[] fields = {"val0"};
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select Prev(1, TheString) as val0 from SupportBean.win:expr(true)");
            stmt.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{null});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[]{"E1"});
        }
    
        [Test]
        public void TestAggregation()
        {
            // Test ungrouped
            String[] fields = {"TheString"};
            EPStatement stmtUngrouped = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.win:expr(sum(IntPrimitive) < 10)");
            stmtUngrouped.Events += _listener.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] { new Object[] {"E1"}});
            EPAssertionUtil.AssertProps(_listener.AssertOneGetNewAndReset(), fields, new Object[] {"E1"});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 9));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] {new Object[] {"E2"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] {"E2"}}, new Object[][] { new Object[] {"E1"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 11));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E3"}}, new Object[][] {new Object[] {"E2"}, new Object[] {"E3"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 12));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] {"E4"}}, new Object[][] { new Object[] {"E4"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E5", 1));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] { new Object[] {"E5"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] {"E5"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E6", 2));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] {new Object[] {"E5"}, new Object[] {"E6"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] {"E6"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E7", 3));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] { new Object[] {"E5"}, new Object[] {"E6"}, new Object[] {"E7"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E7"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E8", 6));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] { new Object[] {"E7"}, new Object[] {"E8"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] { new Object[] {"E8"}}, new Object[][] { new Object[] {"E5"}, new Object[] {"E6"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E9", 9));
            EPAssertionUtil.AssertPropsPerRow(stmtUngrouped.GetEnumerator(), fields, new Object[][] { new Object[] {"E9"}});
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E9"}}, new Object[][] {new Object[] {"E7"}, new Object[] {"E8"}});
    
            stmtUngrouped.Dispose();
    
            // Test grouped
            EPStatement stmtGrouped = _epService.EPAdministrator.CreateEPL("select irstream TheString from SupportBean.std:groupwin(IntPrimitive).win:expr(sum(LongPrimitive) < 10)");
            stmtGrouped.Events += _listener.Update;
    
            SendEvent("E1", 1, 5);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E1"}}, null);
    
            SendEvent("E2", 2, 2);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E2"}}, null);
    
            SendEvent("E3", 1, 3);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E3"}}, null);
    
            SendEvent("E4", 2, 4);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E4"}}, null);
    
            SendEvent("E5", 2, 6);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E5"}}, new Object[][] {new Object[] {"E2"}, new Object[] {"E4"}});
    
            SendEvent("E6", 1, 2);
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E6"}}, new Object[][] {new Object[] {"E1"}});
    
            stmtGrouped.Dispose();
            
            // Test on-delete
            _epService.EPAdministrator.Configuration.AddEventType("SupportBean_A", typeof(SupportBean_A));
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("create window NW.win:expr(sum(IntPrimitive) < 10) as SupportBean");
            stmt.Events += _listener.Update;
            _epService.EPAdministrator.CreateEPL("insert into NW select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E1"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 8));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E2"}}, null);
    
            _epService.EPAdministrator.CreateEPL("on SupportBean_A delete from NW where TheString = id");
            _epService.EPRuntime.SendEvent(new SupportBean_A("E2"));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, null, new Object[][] {new Object[] {"E2"}});
    
            _epService.EPRuntime.SendEvent(new SupportBean("E3", 7));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E3"}}, null);
    
            _epService.EPRuntime.SendEvent(new SupportBean("E4", 2));
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetDataListsFlattened(), fields, new Object[][] {new Object[] {"E4"}}, new Object[][] {new Object[] {"E1"}});
        }
    
        private void SendEvent(String theString, int intPrimitive, long longPrimitive)
        {
            SupportBean bean = new SupportBean(theString, intPrimitive);
            bean.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(bean);
        }
    
        public class LocalUDF
        {
            public static bool EvaluateExpiryUDF(String key, Object viewref, int? expiryCount)
            {
                Key = key;
                Viewref = viewref;
                ExpiryCount = expiryCount;
                return Result;
            }

            public static string Key { get; private set; }

            public static int? ExpiryCount { get; private set; }

            public static object Viewref { get; private set; }

            public static bool Result { private get; set; }
        }
    }
}

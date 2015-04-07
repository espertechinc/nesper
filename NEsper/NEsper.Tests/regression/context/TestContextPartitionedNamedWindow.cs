///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextPartitionedNamedWindow
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listenerSelect;
        private SupportUpdateListener _listenerNamedWindow;
    
        [SetUp]
        public void SetUp()
        {
            var configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType("SupportBean", typeof(SupportBean));
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _listenerSelect = new SupportUpdateListener();
            _listenerNamedWindow = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listenerSelect = null;
            _listenerNamedWindow = null;
        }
    
        [Test]
        public void TestAggregatedSubquery()
        {
            _epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean, p00 from SupportBean_S0");
            _epService.EPAdministrator.CreateEPL("context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('insert') context SegmentedByString insert into MyWindow select * from SupportBean");
    
            var stmt = _epService.EPAdministrator.CreateEPL("@Audit context SegmentedByString " +
                    "select *, (select max(IntPrimitive) from MyWindow) as mymax from SupportBean_S0");
            stmt.Events += _listenerSelect.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[] {20});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[] {10});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E3"));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[] {null});
        }
    
        [Test]
        public void TestNWFireAndForgetInvalid() {
            _epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean");
    
            _epService.EPAdministrator.CreateEPL("context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("context SegmentedByString insert into MyWindow select * from SupportBean");
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 0));
    
            var expected = "Error executing statement: Named window 'MyWindow' is associated to context 'SegmentedByString' that is not available for querying without context partition selector, use the ExecuteQuery(epl, selector) method instead [select * from MyWindow]";
            try {
                _epService.EPRuntime.ExecuteQuery("select * from MyWindow");
            }
            catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
    
            var prepared = _epService.EPRuntime.PrepareQueryWithParameters("select * from MyWindow");
            try {
                _epService.EPRuntime.ExecuteQuery(prepared);
            }
            catch (EPException ex) {
                Assert.AreEqual(expected, ex.Message);
            }
        }
    
        [Test]
        public void TestSegmentedNWConsumeAll() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var stmtNamedWindow = _epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.std:lastevent() as SupportBean");
            stmtNamedWindow.Events += _listenerNamedWindow.Update;
            _epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            var stmtSelect = _epService.EPAdministrator.CreateEPL("@Name('select') select * from MyWindow");
            stmtSelect.Events += _listenerSelect.Update;
    
            var fields = new String[] {"TheString", "IntPrimitive"};
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20});
    
            stmtSelect.Dispose();
    
            // Out-of-context consumer not initialized
            var stmtSelectCount = _epService.EPAdministrator.CreateEPL("@Name('select') select Count(*) as cnt from MyWindow");
            stmtSelectCount.Events += _listenerSelect.Update;
            EPAssertionUtil.AssertProps(stmtSelectCount.First(), "cnt".Split(','), new Object[]{0L});
        }
    
        [Test]
        public void TestSegmentedNWConsumeSameContext() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            var stmtNamedWindow = _epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            stmtNamedWindow.Events += _listenerNamedWindow.Update;
            _epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            var fieldsNW = new String[] {"TheString", "IntPrimitive"};
            var fieldsCnt = new String[] {"TheString", "cnt"};
            var stmtSelect = _epService.EPAdministrator.CreateEPL("@Name('select') context SegmentedByString select TheString, Count(*) as cnt from MyWindow group by TheString");
            stmtSelect.Events += _listenerSelect.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 10});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G1", 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 20});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G2", 1L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 11});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G1", 2L});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 21});
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G2", 2L});
    
            stmtSelect.Dispose();
    
            // In-context consumer not initialized
            var stmtSelectCount = _epService.EPAdministrator.CreateEPL("@Name('select') context SegmentedByString select count(*) as cnt from MyWindow");
            stmtSelectCount.Events += _listenerSelect.Update;
            try {
                // EPAssertionUtil.AssertProps(stmtSelectCount.GetEnumerator().Next(), "cnt".Split(','), new Object[] {0L});
                stmtSelectCount.GetEnumerator();
            }
            catch (UnsupportedOperationException ex) {
                Assert.AreEqual("GetEnumerator not supported on statements that have a context attached", ex.Message);
            }
        }
    
        [Test]
        public void TestOnDeleteAndUpdate() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0, p10 from SupportBean_S1");
    
            var fieldsNW = new String[] {"TheString", "IntPrimitive"};
            _epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            _epService.EPAdministrator.CreateEPL("@Name('selectit') context SegmentedByString select irstream * from MyWindow").Events += _listenerSelect.Update;
    
            // Delete testing
            var stmtDelete = _epService.EPAdministrator.CreateEPL("@Name('on-delete') context SegmentedByString on SupportBean_S0 delete from MyWindow");
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G0"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetOldAndReset(), fieldsNW, new Object[]{"G1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 20});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G3", 3});
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 21});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.LastOldData, fieldsNW, new Object[][]{new Object[] {"G2", 20}, new Object[] {"G2", 21}});
            _listenerSelect.Reset();
    
            stmtDelete.Dispose();
    
            // Update testing
            var stmtUpdate = _epService.EPAdministrator.CreateEPL("@Name('on-merge') context SegmentedByString on SupportBean_S0 Update MyWindow set IntPrimitive = IntPrimitive + 1");
    
            _epService.EPRuntime.SendEvent(new SupportBean("G4", 4));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G4", 4});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G0"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G4"));
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[0], fieldsNW, new Object[]{"G4", 5});
            EPAssertionUtil.AssertProps(_listenerSelect.LastOldData[0], fieldsNW, new Object[]{"G4", 4});
            _listenerSelect.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("G5", 5));
            EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G5", 5});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G5"));
            EPAssertionUtil.AssertProps(_listenerSelect.LastNewData[0], fieldsNW, new Object[]{"G5", 6});
            EPAssertionUtil.AssertProps(_listenerSelect.LastOldData[0], fieldsNW, new Object[]{"G5", 5});
            _listenerSelect.Reset();
    
            stmtUpdate.Dispose();
        }
    
        [Test]
        public void TestSegmentedOnMergeUpdateSubq() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0, p10 from SupportBean_S1");
    
            var stmtNamedWindow = _epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            stmtNamedWindow.Events += _listenerNamedWindow.Update;
            _epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            var fieldsNW = new String[] {"TheString", "IntPrimitive"};
            var stmtSelect = _epService.EPAdministrator.CreateEPL("@Name('on-merge') context SegmentedByString " +
                    "on SupportBean_S0 " +
                    "merge MyWindow " +
                    "when matched then " +
                    "  Update set IntPrimitive = (select id from SupportBean_S1.std:lastevent())");
            stmtSelect.Events += _listenerSelect.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 1});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(99, "G1"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.LastNewData[0], fieldsNW, new Object[]{"G1", 99});
            EPAssertionUtil.AssertProps(_listenerNamedWindow.LastOldData[0], fieldsNW, new Object[]{"G1", 1});
            _listenerNamedWindow.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 2});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S1(98, "Gx"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.LastNewData[0], fieldsNW, new Object[]{"G2", 2});
            EPAssertionUtil.AssertProps(_listenerNamedWindow.LastOldData[0], fieldsNW, new Object[]{"G2", 2});
            _listenerNamedWindow.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            EPAssertionUtil.AssertProps(_listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G3", 3});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Gx"));
            Assert.IsFalse(_listenerNamedWindow.IsInvoked);
        }
    
        [Test]
        public void TestSegmentedOnSelect() {
            _epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0");
    
            _epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            _epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            var fieldsNW = new String[] {"TheString", "IntPrimitive"};
            var stmtSelect = _epService.EPAdministrator.CreateEPL("context SegmentedByString " +
                    "on SupportBean_S0 select mywin.* from MyWindow as mywin");
            stmtSelect.Events += _listenerSelect.Update;
    
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            _epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.GetAndResetLastNewData(), fieldsNW, new Object[][]{new Object[] {"G1", 1}, new Object[] {"G1", 3}});
    
            _epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertPropsPerRow(_listenerSelect.GetAndResetLastNewData(), fieldsNW, new Object[][]{new Object[] {"G2", 2}});
        }

        [Test]
        public void TestCreateIndex()
        {
            String epl =
                    "create context SegmentedByCustomer " +
                    "  initiated by SupportBean_S0 s0 " +
                    "  terminated by SupportBean_S1(p00 = p10);" +
                    "" +
                    "context SegmentedByCustomer\n" +
                    "create window MyWindow.win:keepall() as SupportBean;" +
                    "" +
                    "insert into MyWindow select * from SupportBean;" +
                    "" +
                    "context SegmentedByCustomer\n" +
                    "create index MyIndex on MyWindow(IntPrimitive);";
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);

            _epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            _epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B"));

            _epService.EPRuntime.SendEvent(new SupportBean("E0", 0));

            _epService.EPRuntime.ExecuteQuery("select * from MyWindow where IntPrimitive = 1", new ContextPartitionSelector[] { new EPContextPartitionAdminImpl.CPSelectorById(1) });

            _epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A"));
            _epService.EPAdministrator.DestroyAllStatements();
        }
    }
}

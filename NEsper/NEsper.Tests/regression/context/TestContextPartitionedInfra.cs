///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    [TestFixture]
    public class TestContextPartitionedInfra  {
    
        private EPServiceProvider epService;
        private SupportUpdateListener listenerSelect;
        private SupportUpdateListener listenerNamedWindow;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType<SupportBean_S1>();
            configuration.EngineDefaults.LoggingConfig.IsEnableExecutionDebug = true;
            epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, this.GetType(), GetType().FullName);}
    
            listenerSelect = new SupportUpdateListener();
            listenerNamedWindow = new SupportUpdateListener();
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            listenerSelect = null;
            listenerNamedWindow = null;
        }
    
        [Test]
        public void TestAggregatedSubquery() {
            RunAssertionAggregatedSubquery(true);
            RunAssertionAggregatedSubquery(false);
        }
    
        [Test]
        public void TestOnDeleteAndUpdate() {
            RunAssertionOnDeleteAndUpdate(true);
            RunAssertionOnDeleteAndUpdate(false);
        }
    
        [Test]
        public void TestCreateIndex() {
            RunAssertionCreateIndex(true);
            RunAssertionCreateIndex(false);
        }
    
        [Test]
        public void TestSegmentedOnSelect() {
            RunAssertionSegmentedOnSelect(true);
            RunAssertionSegmentedOnSelect(false);
        }
    
        public void RunAssertionSegmentedOnSelect(bool namedWindow) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0");
    
            string eplCreate = namedWindow ?
                    "@Name('named window') context SegmentedByString create window MyInfra.win:keepall() as SupportBean" :
                    "@Name('table') context SegmentedByString create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("@Name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean");
    
            string[] fieldsNW = new string[] {"TheString", "IntPrimitive"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("context SegmentedByString " +
                    "on SupportBean_S0 select mywin.* from MyInfra as mywin");
            stmtSelect.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerSelect.GetAndResetLastNewData(), fieldsNW, new object[][] { new object[] { "G1", 1 }, new object[] { "G1", 3 } });
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerSelect.GetAndResetLastNewData(), fieldsNW, new object[][] { new object[] { "G2", 2 } });
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionCreateIndex(bool namedWindow) {
            string epl = "@Name('create-ctx') create context SegmentedByCustomer " +
                         "  initiated by SupportBean_S0 s0 " +
                         "  terminated by SupportBean_S1(p00 = p10);" +
                         "" +
                         "@Name('create-infra') context SegmentedByCustomer\n" +
                         (namedWindow ?
                             "create window MyInfra.win:keepall() as SupportBean;" :
                             "create table MyInfra(TheString string primary key, IntPrimitive int);") +
                         "" +
                         (namedWindow ?
                             "@Name('insert-into-window') insert into MyInfra select TheString, IntPrimitive from SupportBean;" :
                             "@Name('insert-into-table') context SegmentedByCustomer insert into MyInfra select TheString, IntPrimitive from SupportBean;") +
                         "" +
                         "@Name('create-index') context SegmentedByCustomer\n" +
                         "create index MyIndex on MyInfra(IntPrimitive);";
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery("select * from MyInfra where IntPrimitive = 1", new ContextPartitionSelector[] {new EPContextPartitionAdminImpl.CPSelectorById(1)});
            EPAssertionUtil.AssertPropsPerRow(result.Array, "TheString,IntPrimitive".Split(','), new object[][] { new object[] { "E1", 1 } });
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A"));
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    
        public void RunAssertionOnDeleteAndUpdate(bool namedWindow) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0, p10 from SupportBean_S1");
    
            string[] fieldsNW = new string[] {"TheString", "IntPrimitive"};
            string eplCreate = namedWindow ?
                    "@Name('named window') context SegmentedByString create window MyInfra.win:keepall() as SupportBean" :
                    "@Name('named window') context SegmentedByString create table MyInfra(TheString string primary key, IntPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            string eplInsert = namedWindow ?
                    "@Name('insert') insert into MyInfra select TheString, IntPrimitive from SupportBean" :
                    "@Name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsert);
    
            epService.EPAdministrator.CreateEPL("@Name('selectit') context SegmentedByString select irstream * from MyInfra").AddListener(listenerSelect);
    
            // Delete testing
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL("@Name('on-delete') context SegmentedByString on SupportBean_S0 delete from MyInfra");
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G1", 1});
            }
            else {
                Assert.IsFalse(listenerSelect.IsInvoked);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(listenerSelect.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetOldAndReset(), fieldsNW, new object[]{"G1", 1});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G2", 20});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G3", 3});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G2", 21});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(listenerSelect.LastOldData, fieldsNW, new object[][] { new object[] { "G2", 20 }, new object[] { "G2", 21 } });
            }
            listenerSelect.Reset();
    
            stmtDelete.Dispose();
    
            // update testing
            EPStatement stmtUpdate = epService.EPAdministrator.CreateEPL("@Name('on-merge') context SegmentedByString on SupportBean_S0 update MyInfra set IntPrimitive = IntPrimitive + 1");
    
            epService.EPRuntime.SendEvent(new SupportBean("G4", 4));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G4", 4});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(listenerSelect.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G4"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fieldsNW, new object[]{"G4", 5});
                EPAssertionUtil.AssertProps(listenerSelect.LastOldData[0], fieldsNW, new object[]{"G4", 4});
                listenerSelect.Reset();
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G5", 5));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G5", 5});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G5"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fieldsNW, new object[]{"G5", 6});
                EPAssertionUtil.AssertProps(listenerSelect.LastOldData[0], fieldsNW, new object[]{"G5", 5});
                listenerSelect.Reset();
            }
    
            stmtUpdate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionAggregatedSubquery(bool namedWindow) {
            epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by TheString from SupportBean, p00 from SupportBean_S0");
            string eplCreate = namedWindow ?
                    "context SegmentedByString create window MyInfra.win:keepall() as SupportBean" :
                    "context SegmentedByString create table MyInfra (TheString string primary key, IntPrimitive int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("@Name('insert') context SegmentedByString insert into MyInfra select TheString, IntPrimitive from SupportBean");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Audit context SegmentedByString " +
                    "select *, (select max(IntPrimitive) from MyInfra) as mymax from SupportBean_S0");
            stmt.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new object[] {20});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new object[] {10});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E3"));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new object[] {null});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        [Test]
        public void TestSegmentedNWConsumeAll() {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.std:lastevent() as SupportBean");
            stmtNamedWindow.AddListener(listenerNamedWindow);
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('select') select * from MyWindow");
            stmtSelect.AddListener(listenerSelect);
    
            string[] fields = new string[] {"TheString", "IntPrimitive"};
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"G1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"G2", 20});
    
            stmtSelect.Dispose();
    
            // Out-of-context consumer not initialized
            EPStatement stmtSelectCount = epService.EPAdministrator.CreateEPL("@Name('select') select count(*) as cnt from MyWindow");
            stmtSelectCount.AddListener(listenerSelect);
            EPAssertionUtil.AssertProps(stmtSelectCount.First(), "cnt".Split(','), new object[]{0L});
        }
    
        [Test]
        public void TestSegmentedNWConsumeSameContext() {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by TheString from SupportBean");
    
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            stmtNamedWindow.AddListener(listenerNamedWindow);
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            string[] fieldsNW = new string[] {"TheString", "IntPrimitive"};
            string[] fieldsCnt = new string[] {"TheString", "cnt"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('select') context SegmentedByString select TheString, count(*) as cnt from MyWindow group by TheString");
            stmtSelect.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G1", 10});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new object[]{"G1", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G2", 20});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new object[]{"G2", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G1", 11});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new object[]{"G1", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G2", 21});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new object[]{"G2", 2L});
    
            stmtSelect.Dispose();
    
            // In-context consumer not initialized
            EPStatement stmtSelectCount = epService.EPAdministrator.CreateEPL("@Name('select') context SegmentedByString select count(*) as cnt from MyWindow");
            stmtSelectCount.AddListener(listenerSelect);
            try {
                // EPAssertionUtil.assertProps(stmtSelectCount.First(), "cnt".Split(','), new Object[] {0L});
                stmtSelectCount.GetEnumerator();
            }
            catch (UnsupportedOperationException ex) {
                Assert.AreEqual("GetEnumerator not supported on statements that have a context attached", ex.Message);
            }
        }
    
        [Test]
        public void TestSegmentedOnMergeUpdateSubq() {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by TheString from SupportBean, p00 from SupportBean_S0, p10 from SupportBean_S1");
    
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow.win:keepall() as SupportBean");
            stmtNamedWindow.AddListener(listenerNamedWindow);
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            string[] fieldsNW = new string[] {"TheString", "IntPrimitive"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('on-merge') context SegmentedByString " +
                    "on SupportBean_S0 " +
                    "merge MyWindow " +
                    "when matched then " +
                    "  update set IntPrimitive = (select id from SupportBean_S1.std:lastevent())");
            stmtSelect.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(99, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastNewData[0], fieldsNW, new object[]{"G1", 99});
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastOldData[0], fieldsNW, new object[]{"G1", 1});
            listenerNamedWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(98, "Gx"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastNewData[0], fieldsNW, new object[]{"G2", 2});
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastOldData[0], fieldsNW, new object[]{"G2", 2});
            listenerNamedWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new object[]{"G3", 3});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Gx"));
            Assert.IsFalse(listenerNamedWindow.IsInvoked);
        }
    }
}

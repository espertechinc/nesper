///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.context;
using com.espertech.esper.client.deploy;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

// using static org.junit.Assert.assertEquals;
// using static org.junit.Assert.assertFalse;

using NUnit.Framework;

namespace com.espertech.esper.regression.context
{
    public class ExecContextPartitionedInfra : RegressionExecution {
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType("SupportBean_S0", typeof(SupportBean_S0));
            configuration.AddEventType("SupportBean_S1", typeof(SupportBean_S1));
            configuration.EngineDefaults.Logging.EnableExecutionDebug = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            RunAssertionAggregatedSubquery(epService);
            RunAssertionOnDeleteAndUpdate(epService);
            RunAssertionCreateIndex(epService);
            RunAssertionSegmentedOnSelect(epService);
            RunAssertionSegmentedNWConsumeAll(epService);
            RunAssertionSegmentedNWConsumeSameContext(epService);
            RunAssertionSegmentedOnMergeUpdateSubq(epService);
        }
    
        private void RunAssertionAggregatedSubquery(EPServiceProvider epService) {
            TryAssertionAggregatedSubquery(epService, true);
            TryAssertionAggregatedSubquery(epService, false);
        }
    
        private void RunAssertionOnDeleteAndUpdate(EPServiceProvider epService) {
            TryAssertionOnDeleteAndUpdate(epService, true);
            TryAssertionOnDeleteAndUpdate(epService, false);
        }
    
        private void RunAssertionCreateIndex(EPServiceProvider epService) {
            TryAssertionCreateIndex(epService, true);
            TryAssertionCreateIndex(epService, false);
        }
    
        private void RunAssertionSegmentedOnSelect(EPServiceProvider epService) {
            TryAssertionSegmentedOnSelect(epService, true);
            TryAssertionSegmentedOnSelect(epService, false);
        }
    
        public void TryAssertionSegmentedOnSelect(EPServiceProvider epService, bool namedWindow) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by theString from SupportBean, p00 from SupportBean_S0");
    
            string eplCreate = namedWindow ?
                    "@Name('named window') context SegmentedByString create window MyInfra#keepall as SupportBean" :
                    "@Name('table') context SegmentedByString create table MyInfra(theString string primary key, intPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("@Name('insert') context SegmentedByString insert into MyInfra select theString, intPrimitive from SupportBean");
    
            var fieldsNW = new string[]{"theString", "intPrimitive"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("context SegmentedByString " +
                    "on SupportBean_S0 select mywin.* from MyInfra as mywin");
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            epService.EPRuntime.SendEvent(new SupportBean("G1", 3));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerSelect.GetAndResetLastNewData(), fieldsNW, new Object[][]{new object[] {"G1", 1}, new object[] {"G1", 3}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertPropsPerRowAnyOrder(listenerSelect.GetAndResetLastNewData(), fieldsNW, new Object[][]{new object[] {"G2", 2}});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void TryAssertionCreateIndex(EPServiceProvider epService, bool namedWindow) {
            string epl = "@Name('create-ctx') create context SegmentedByCustomer " +
                    "  initiated by SupportBean_S0 s0 " +
                    "  terminated by SupportBean_S1(p00 = p10);" +
                    "" +
                    "@Name('create-infra') context SegmentedByCustomer\n" +
                    (namedWindow ?
                            "create window MyInfra#keepall as SupportBean;" :
                            "create table MyInfra(theString string primary key, intPrimitive int);") +
                    "" +
                    (namedWindow ?
                            "@Name('insert-into-window') insert into MyInfra select theString, intPrimitive from SupportBean;" :
                            "@Name('insert-into-table') context SegmentedByCustomer insert into MyInfra select theString, intPrimitive from SupportBean;") +
                    "" +
                    "@Name('create-index') context SegmentedByCustomer\n" +
                    "create index MyIndex on MyInfra(intPrimitive);";
            DeploymentResult deployed = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(1, "A"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(2, "B"));
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
    
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery("select * from MyInfra where intPrimitive = 1", new ContextPartitionSelector[]{new EPContextPartitionAdminImpl.CPSelectorById(1)});
            EPAssertionUtil.AssertPropsPerRow(result.Array, "theString,intPrimitive".Split(','), new Object[][]{new object[] {"E1", 1}});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(3, "A"));
    
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deployed.DeploymentId);
        }
    
        private void TryAssertionOnDeleteAndUpdate(EPServiceProvider epService, bool namedWindow) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by theString from SupportBean, p00 from SupportBean_S0, p10 from SupportBean_S1");
    
            var fieldsNW = new string[]{"theString", "intPrimitive"};
            string eplCreate = namedWindow ?
                    "@Name('named window') context SegmentedByString create window MyInfra#keepall as SupportBean" :
                    "@Name('named window') context SegmentedByString create table MyInfra(theString string primary key, intPrimitive int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            string eplInsert = namedWindow ?
                    "@Name('insert') insert into MyInfra select theString, intPrimitive from SupportBean" :
                    "@Name('insert') context SegmentedByString insert into MyInfra select theString, intPrimitive from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsert);
    
            var listenerSelect = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("@Name('selectit') context SegmentedByString select irstream * from MyInfra").AddListener(listenerSelect);
    
            // Delete testing
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL("@Name('on-delete') context SegmentedByString on SupportBean_S0 delete from MyInfra");
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 1});
            } else {
                Assert.IsFalse(listenerSelect.IsInvoked);
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(listenerSelect.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetOldAndReset(), fieldsNW, new Object[]{"G1", 1});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 20});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G3", 3});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 21});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            if (namedWindow) {
                EPAssertionUtil.AssertPropsPerRow(listenerSelect.LastOldData, fieldsNW, new Object[][]{new object[] {"G2", 20}, new object[] {"G2", 21}});
            }
            listenerSelect.Reset();
    
            stmtDelete.Dispose();
    
            // update testing
            EPStatement stmtUpdate = epService.EPAdministrator.CreateEPL("@Name('on-merge') context SegmentedByString on SupportBean_S0 update MyInfra set intPrimitive = intPrimitive + 1");
    
            epService.EPRuntime.SendEvent(new SupportBean("G4", 4));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G4", 4});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G0"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            Assert.IsFalse(listenerSelect.IsInvoked);
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G4"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fieldsNW, new Object[]{"G4", 5});
                EPAssertionUtil.AssertProps(listenerSelect.LastOldData[0], fieldsNW, new Object[]{"G4", 4});
                listenerSelect.Reset();
            }
    
            epService.EPRuntime.SendEvent(new SupportBean("G5", 5));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G5", 5});
            }
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G5"));
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerSelect.LastNewData[0], fieldsNW, new Object[]{"G5", 6});
                EPAssertionUtil.AssertProps(listenerSelect.LastOldData[0], fieldsNW, new Object[]{"G5", 5});
                listenerSelect.Reset();
            }
    
            stmtUpdate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void TryAssertionAggregatedSubquery(EPServiceProvider epService, bool namedWindow) {
            epService.EPAdministrator.CreateEPL("create context SegmentedByString partition by theString from SupportBean, p00 from SupportBean_S0");
            string eplCreate = namedWindow ?
                    "context SegmentedByString create window MyInfra#keepall as SupportBean" :
                    "context SegmentedByString create table MyInfra (theString string primary key, intPrimitive int)";
            epService.EPAdministrator.CreateEPL(eplCreate);
            epService.EPAdministrator.CreateEPL("@Name('insert') context SegmentedByString insert into MyInfra select theString, intPrimitive from SupportBean");
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL("@Audit context SegmentedByString " +
                    "select *, (select max(intPrimitive) from MyInfra) as mymax from SupportBean_S0");
            var listenerSelect = new SupportUpdateListener();
            stmt.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 10));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 20));
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E2"));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[]{20});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E1"));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[]{10});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "E3"));
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), "mymax".Split(','), new Object[]{null});
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionSegmentedNWConsumeAll(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow#lastevent as SupportBean");
            var listenerNamedWindow = new SupportUpdateListener();
            stmtNamedWindow.AddListener(listenerNamedWindow);
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('select') select * from MyWindow");
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.AddListener(listenerSelect);
    
            var fields = new string[]{"theString", "intPrimitive"};
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"G1", 10});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new Object[]{"G2", 20});
    
            stmtSelect.Dispose();
    
            // Out-of-context consumer not initialized
            EPStatement stmtSelectCount = epService.EPAdministrator.CreateEPL("@Name('select') select count(*) as cnt from MyWindow");
            stmtSelectCount.AddListener(listenerSelect);
            EPAssertionUtil.AssertProps(stmtSelectCount.First(), "cnt".Split(','), new Object[]{0L});
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedNWConsumeSameContext(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString partition by theString from SupportBean");
    
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow#keepall as SupportBean");
            var listenerNamedWindow = new SupportUpdateListener();
            stmtNamedWindow.AddListener(listenerNamedWindow);
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            var fieldsNW = new string[]{"theString", "intPrimitive"};
            var fieldsCnt = new string[]{"theString", "cnt"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('select') context SegmentedByString select theString, count(*) as cnt from MyWindow group by theString");
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 10));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 10});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G1", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 20));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 20});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G2", 1L});
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 11));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 11});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G1", 2L});
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 21));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 21});
            EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fieldsCnt, new Object[]{"G2", 2L});
    
            stmtSelect.Dispose();
    
            // In-context consumer not initialized
            EPStatement stmtSelectCount = epService.EPAdministrator.CreateEPL("@Name('select') context SegmentedByString select count(*) as cnt from MyWindow");
            stmtSelectCount.AddListener(listenerSelect);
            try {
                // EPAssertionUtil.AssertProps(stmtSelectCount.First(), "cnt".Split(','), new Object[] {0L});
                stmtSelectCount.GetEnumerator();
            } catch (UnsupportedOperationException ex) {
                Assert.AreEqual("Iterator not supported on statements that have a context attached", ex.Message);
            }
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionSegmentedOnMergeUpdateSubq(EPServiceProvider epService) {
            epService.EPAdministrator.CreateEPL("@Name('context') create context SegmentedByString " +
                    "partition by theString from SupportBean, p00 from SupportBean_S0, p10 from SupportBean_S1");
    
            EPStatement stmtNamedWindow = epService.EPAdministrator.CreateEPL("@Name('named window') context SegmentedByString create window MyWindow#keepall as SupportBean");
            var listenerNamedWindow = new SupportUpdateListener();
            stmtNamedWindow.AddListener(listenerNamedWindow);
            epService.EPAdministrator.CreateEPL("@Name('insert') insert into MyWindow select * from SupportBean");
    
            var fieldsNW = new string[]{"theString", "intPrimitive"};
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL("@Name('on-merge') context SegmentedByString " +
                    "on SupportBean_S0 " +
                    "merge MyWindow " +
                    "when matched then " +
                    "  update set intPrimitive = (select id from SupportBean_S1#lastevent)");
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.AddListener(listenerSelect);
    
            epService.EPRuntime.SendEvent(new SupportBean("G1", 1));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G1", 1});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(99, "G1"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G1"));
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastNewData[0], fieldsNW, new Object[]{"G1", 99});
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastOldData[0], fieldsNW, new Object[]{"G1", 1});
            listenerNamedWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("G2", 2));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G2", 2});
    
            epService.EPRuntime.SendEvent(new SupportBean_S1(98, "Gx"));
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "G2"));
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastNewData[0], fieldsNW, new Object[]{"G2", 2});
            EPAssertionUtil.AssertProps(listenerNamedWindow.LastOldData[0], fieldsNW, new Object[]{"G2", 2});
            listenerNamedWindow.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("G3", 3));
            EPAssertionUtil.AssertProps(listenerNamedWindow.AssertOneGetNewAndReset(), fieldsNW, new Object[]{"G3", 3});
    
            epService.EPRuntime.SendEvent(new SupportBean_S0(0, "Gx"));
            Assert.IsFalse(listenerNamedWindow.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    }
} // end of namespace

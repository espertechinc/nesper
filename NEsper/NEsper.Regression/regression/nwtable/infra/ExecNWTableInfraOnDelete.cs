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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraOnDelete : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Logging.IsEnableQueryPlan = true;
        }
    
        public override void Run(EPServiceProvider epService) {
            foreach (var clazz in new[]{ typeof(SupportBean), typeof(SupportBean_A), typeof(SupportBean_B) }) {
                epService.EPAdministrator.Configuration.AddEventType(clazz);
            }
    
            RunAssertionDeleteCondition(epService, true);
            RunAssertionDeleteCondition(epService, false);
    
            RunAssertionDeletePattern(epService, true);
            RunAssertionDeletePattern(epService, false);
    
            RunAssertionDeleteAll(epService, true);
            RunAssertionDeleteAll(epService, false);
        }
    
        private void RunAssertionDeleteAll(EPServiceProvider epService, bool namedWindow) {
            // create window
            string stmtTextCreate = namedWindow ?
                    "@Name('CreateInfra') create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "@Name('CreateInfra') create table MyInfra (a string primary key, b int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerInfra = new SupportUpdateListener();
            stmtCreate.Events += listenerInfra.Update;
    
            // create delete stmt
            string stmtTextDelete = "@Name('OnDelete') on " + typeof(SupportBean_A).FullName + " delete from MyInfra";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerDelete.Update;
            EPAssertionUtil.AssertEqualsAnyOrder(stmtDelete.EventType.PropertyNames, new string[]{"a", "b"});
    
            // create insert into
            string stmtTextInsertOne = "@Name('Insert') insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var fields = new string[]{"a", "b"};
            string stmtTextSelect = "@Name('Select') select irstream MyInfra.a as a, b from MyInfra as s1";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
    
            // Delete all events, no result expected
            SendSupportBean_A(epService, "A1");
            Assert.IsFalse(listenerInfra.IsInvoked);
            Assert.IsFalse(listenerSelect.IsInvoked);
            Assert.IsFalse(listenerDelete.IsInvoked);
            Assert.AreEqual(0, GetCount(epService, "MyInfra"));
    
            // send 1 event
            SendSupportBean(epService, "E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            } else {
                Assert.IsFalse(listenerInfra.IsInvoked);
                Assert.IsFalse(listenerSelect.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, null);
            Assert.AreEqual(1, GetCount(epService, "MyInfra"));
    
            // Delete all events, 1 row expected
            SendSupportBean_A(epService, "A2");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            Assert.AreEqual(0, GetCount(epService, "MyInfra"));
    
            // send 2 events
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            listenerInfra.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}, new object[] {"E3", 3}});
            Assert.IsFalse(listenerDelete.IsInvoked);
            Assert.AreEqual(2, GetCount(epService, "MyInfra"));
    
            // Delete all events, 2 rows expected
            SendSupportBean_A(epService, "A2");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[0], fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[1], fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}, new object[] {"E3", 3}});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            Assert.AreEqual(2, listenerDelete.LastNewData.Length);
            EPAssertionUtil.AssertProps(listenerDelete.LastNewData[0], fields, new object[]{"E2", 2});
            EPAssertionUtil.AssertProps(listenerDelete.LastNewData[1], fields, new object[]{"E3", 3});
            Assert.AreEqual(0, GetCount(epService, "MyInfra"));
    
            listenerInfra.Reset();
            listenerDelete.Reset();
            listenerSelect.Reset();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionDeletePattern(EPServiceProvider epService, bool isNamedWindow) {
            // create infra
            string stmtTextCreate = isNamedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean" :
                    "create table MyInfra(a string primary key, b int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerInfra = new SupportUpdateListener();
            stmtCreate.Events += listenerInfra.Update;
    
            // create delete stmt
            string stmtTextDelete = "on pattern [every ea=" + typeof(SupportBean_A).FullName + " or every eb=" + typeof(SupportBean_B).FullName + "] " + " delete from MyInfra";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
            var listenerDelete = new SupportUpdateListener();
            stmtDelete.Events += listenerDelete.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 1 event
            var fields = new string[]{"a", "b"};
            SendSupportBean(epService, "E1", 1);
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, null);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            Assert.AreEqual(1, GetCount(epService, "MyInfra"));
    
            // Delete all events using A, 1 row expected
            SendSupportBean_A(epService, "A1");
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            Assert.AreEqual(0, GetCount(epService, "MyInfra"));
    
            // send 1 event
            SendSupportBean(epService, "E2", 2);
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}});
            Assert.AreEqual(1, GetCount(epService, "MyInfra"));
    
            // Delete all events using B, 1 row expected
            SendSupportBean_B(epService, "B1");
            if (isNamedWindow) {
                EPAssertionUtil.AssertProps(listenerInfra.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertPropsPerRow(stmtDelete.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
            EPAssertionUtil.AssertProps(listenerDelete.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            Assert.AreEqual(0, GetCount(epService, "MyInfra"));
    
            stmtDelete.Dispose();
            stmtCreate.Dispose();
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionDeleteCondition(EPServiceProvider epService, bool isNamedWindow) {
    
            // create infra
            string stmtTextCreate = isNamedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from SupportBean" :
                    "create table MyInfra (a string primary key, b int)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerInfra = new SupportUpdateListener();
            stmtCreate.Events += listenerInfra.Update;
    
            // create delete stmt
            string stmtTextDelete = "on SupportBean_A delete from MyInfra where 'X' || a || 'X' = id";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create delete stmt
            stmtTextDelete = "on SupportBean_B delete from MyInfra where b < 5";
            epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from SupportBean";
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // send 3 event
            SendSupportBean(epService, "E1", 1);
            SendSupportBean(epService, "E2", 2);
            SendSupportBean(epService, "E3", 3);
            Assert.AreEqual(3, GetCount(epService, "MyInfra"));
            listenerInfra.Reset();
            var fields = new string[]{"a", "b"};
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
    
            // delete E2
            SendSupportBean_A(epService, "XE2X");
            if (isNamedWindow) {
                Assert.AreEqual(1, listenerInfra.LastOldData.Length);
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[0], fields, new object[]{"E2", 2});
            }
            listenerInfra.Reset();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
            Assert.AreEqual(2, GetCount(epService, "MyInfra"));
    
            SendSupportBean(epService, "E7", 7);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}, new object[] {"E7", 7}});
            Assert.AreEqual(3, GetCount(epService, "MyInfra"));
    
            // delete all under 5
            SendSupportBean_B(epService, "B1");
            if (isNamedWindow) {
                Assert.AreEqual(2, listenerInfra.LastOldData.Length);
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[0], fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerInfra.LastOldData[1], fields, new object[]{"E3", 3});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E7", 7}});
            Assert.AreEqual(1, GetCount(epService, "MyInfra"));
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean_B(EPServiceProvider epService, string id) {
            var bean = new SupportBean_B(id);
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
        }
    
        private long GetCount(EPServiceProvider epService, string windowOrTableName) {
            return (long) epService.EPRuntime.ExecuteQuery("select count(*) as c0 from " + windowOrTableName).Array[0].Get("c0");
        }
    }
} // end of namespace

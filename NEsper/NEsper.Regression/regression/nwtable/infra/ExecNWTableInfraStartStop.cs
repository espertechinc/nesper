///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.infra
{
    public class ExecNWTableInfraStartStop : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            RunAssertionStartStopDeleter(epService, true);
            RunAssertionStartStopDeleter(epService, false);
    
            RunAssertionStartStopConsumer(epService, true);
            RunAssertionStartStopConsumer(epService, false);
    
            RunAssertionStartStopInserter(epService, true);
            RunAssertionStartStopInserter(epService, false);
        }
    
        private void RunAssertionStartStopInserter(EPServiceProvider epService, bool namedWindow) {
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var fields = new string[]{"a", "b"};
            string stmtTextSelect = "select a, b from MyInfra as s1";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
    
            // send 1 event
            SendSupportBean(epService, "E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // stop inserter
            stmtInsert.Stop();
            SendSupportBean(epService, "E2", 2);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerSelect.IsInvoked);
    
            // start inserter
            stmtInsert.Start();
    
            // consumer receives the next event
            SendSupportBean(epService, "E3", 3);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E3", 3}});
    
            // destroy inserter
            stmtInsert.Dispose();
            SendSupportBean(epService, "E4", 4);
            Assert.IsFalse(listenerWindow.IsInvoked);
            Assert.IsFalse(listenerSelect.IsInvoked);
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionStartStopConsumer(EPServiceProvider epService, bool namedWindow) {
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var fields = new string[]{"a", "b"};
            string stmtTextSelect = "select a, b from MyInfra as s1";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
    
            // send 1 event
            SendSupportBean(epService, "E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // stop consumer
            stmtSelect.Stop();
            SendSupportBean(epService, "E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            Assert.IsFalse(listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            // start consumer: the consumer has the last event even though he missed it
            stmtSelect.Start();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}});
    
            // consumer receives the next event
            SendSupportBean(epService, "E3", 3);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}, new object[] {"E2", 2}, new object[] {"E3", 3}});
    
            // destroy consumer
            stmtSelect.Dispose();
            SendSupportBean(epService, "E4", 4);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4});
            }
            Assert.IsFalse(listenerSelect.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionStartStopDeleter(EPServiceProvider epService, bool namedWindow) {
            var observer = new SupportNamedWindowObserver();
            NamedWindowLifecycleEvent theEvent;
            if (namedWindow) {
                ((EPServiceProviderSPI) epService).NamedWindowMgmtService.AddObserver(observer);
            }
    
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            EPStatement stmtCreate = epService.EPAdministrator.CreateEPL(stmtTextCreate);
            Assert.AreEqual(namedWindow ? StatementType.CREATE_WINDOW : StatementType.CREATE_TABLE, ((EPStatementSPI) stmtCreate).StatementMetadata.StatementType);
            var listenerWindow = new SupportUpdateListener();
            stmtCreate.Events += listenerWindow.Update;
            if (namedWindow) {
                theEvent = observer.GetFirstAndReset();
                Assert.AreEqual(NamedWindowLifecycleEvent.LifecycleEventType.CREATE, theEvent.EventType);
                Assert.AreEqual("MyInfra", theEvent.Name);
            }
    
            // stop and start, no consumers or deleters
            stmtCreate.Stop();
            if (namedWindow) {
                theEvent = observer.GetFirstAndReset();
                Assert.AreEqual(NamedWindowLifecycleEvent.LifecycleEventType.DESTROY, theEvent.EventType);
                Assert.AreEqual("MyInfra", theEvent.Name);
            }
    
            stmtCreate.Start();
            if (namedWindow) {
                Assert.AreEqual(NamedWindowLifecycleEvent.LifecycleEventType.CREATE, observer.GetFirstAndReset().EventType);
            }
    
            // create delete stmt
            string stmtTextDelete = "on " + typeof(SupportBean_A).FullName + " delete from MyInfra";
            EPStatement stmtDelete = epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            var fields = new string[]{"a", "b"};
            string stmtTextSelect = "select irstream a, b from MyInfra as s1";
            EPStatement stmtSelect = epService.EPAdministrator.CreateEPL(stmtTextSelect);
            var listenerSelect = new SupportUpdateListener();
            stmtSelect.Events += listenerSelect.Update;
    
            // send 1 event
            SendSupportBean(epService, "E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            } else {
                Assert.IsFalse(listenerWindow.IsInvoked);
                Assert.IsFalse(listenerSelect.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E1", 1}});
    
            // Delete all events, 1 row expected
            SendSupportBean_A(epService, "A2");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E2", 2}});
    
            // Stop the deleting statement
            stmtDelete.Stop();
            SendSupportBean_A(epService, "A2");
            Assert.IsFalse(listenerWindow.IsInvoked);
    
            // Start the deleting statement
            stmtDelete.Start();
    
            SendSupportBean_A(epService, "A3");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean(epService, "E3", 3);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertProps(listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][]{new object[] {"E3", 3}});
    
            stmtDelete.Dispose();
            SendSupportBean_A(epService, "A3");
            Assert.IsFalse(listenerWindow.IsInvoked);
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean_A SendSupportBean_A(EPServiceProvider epService, string id) {
            var bean = new SupportBean_A(id);
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(EPServiceProvider epService, string theString, int intPrimitive) {
            var bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
} // end of namespace

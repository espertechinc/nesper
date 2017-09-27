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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.named;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.epl;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestInfraStartStop 
    {
        private EPServiceProviderSPI _epService;
        private SupportUpdateListener _listenerWindow;
        private SupportUpdateListener _listenerSelect;
    
        [SetUp]
        public void SetUp()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Logging.IsEnableQueryPlan = true;
            _epService = (EPServiceProviderSPI) EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            _listenerWindow = new SupportUpdateListener();
            _listenerSelect = new SupportUpdateListener();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, this.GetType(), GetType().FullName);}
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
            _listenerWindow = null;
            _listenerSelect = null;
        }
    
        [Test]
        public void TestStartStopDeleter() {
            RunAssertionStartStopDeleter(true);
            RunAssertionStartStopDeleter(false);
        }
    
        [Test]
        public void TestStartStopConsumer() {
            RunAssertionStartStopConsumer(true);
            RunAssertionStartStopConsumer(false);
        }
    
        [Test]
        public void TestStartStopInserter() {
            RunAssertionStartStopInserter(true);
            RunAssertionStartStopInserter(false);
        }
    
        private void RunAssertionStartStopInserter(bool namedWindow)
        {
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            EPStatement stmtInsert = _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string[] fields = new string[] {"a", "b"};
            string stmtTextSelect = "select a, b from MyInfra as s1";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // send 1 event
            SendSupportBean("E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // stop inserter
            stmtInsert.Stop();
            SendSupportBean("E2", 2);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            // start inserter
            stmtInsert.Start();
    
            // consumer receives the next event
            SendSupportBean("E3", 3);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E3", 3 } });
    
            // destroy inserter
            stmtInsert.Dispose();
            SendSupportBean("E4", 4);
            Assert.IsFalse(_listenerWindow.IsInvoked);
            Assert.IsFalse(_listenerSelect.IsInvoked);
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionStartStopConsumer(bool namedWindow)
        {
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            stmtCreate.AddListener(_listenerWindow);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string[] fields = new string[] {"a", "b"};
            string stmtTextSelect = "select a, b from MyInfra as s1";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // send 1 event
            SendSupportBean("E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // stop consumer
            stmtSelect.Stop();
            SendSupportBean("E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            Assert.IsFalse(_listenerSelect.IsInvoked);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
    
            // start consumer: the consumer has the last event even though he missed it
            stmtSelect.Start();
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 } });
    
            // consumer receives the next event
            SendSupportBean("E3", 3);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertPropsPerRow(stmtSelect.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
            }
            EPAssertionUtil.AssertPropsPerRowAnyOrder(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 }, new object[] { "E2", 2 }, new object[] { "E3", 3 } });
    
            // destroy consumer
            stmtSelect.Dispose();
            SendSupportBean("E4", 4);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E4", 4});
            }
            Assert.IsFalse(_listenerSelect.IsInvoked);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private void RunAssertionStartStopDeleter(bool namedWindow)
        {
            SupportNamedWindowObserver observer = new SupportNamedWindowObserver();
            NamedWindowLifecycleEvent theEvent;
            if (namedWindow) {
                _epService.NamedWindowMgmtService.AddObserver(observer);
            }
    
            // create window
            string stmtTextCreate = namedWindow ?
                    "create window MyInfra#keepall as select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName :
                    "create table MyInfra(a string primary key, b int primary key)";
            EPStatement stmtCreate = _epService.EPAdministrator.CreateEPL(stmtTextCreate);
            Assert.AreEqual(namedWindow ? StatementType.CREATE_WINDOW : StatementType.CREATE_TABLE, ((EPStatementSPI) stmtCreate).StatementMetadata.StatementType);
            stmtCreate.AddListener(_listenerWindow);
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
            EPStatement stmtDelete = _epService.EPAdministrator.CreateEPL(stmtTextDelete);
    
            // create insert into
            string stmtTextInsertOne = "insert into MyInfra select TheString as a, IntPrimitive as b from " + typeof(SupportBean).FullName;
            _epService.EPAdministrator.CreateEPL(stmtTextInsertOne);
    
            // create consumer
            string[] fields = new string[] {"a", "b"};
            string stmtTextSelect = "select irstream a, b from MyInfra as s1";
            EPStatement stmtSelect = _epService.EPAdministrator.CreateEPL(stmtTextSelect);
            stmtSelect.AddListener(_listenerSelect);
    
            // send 1 event
            SendSupportBean("E1", 1);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E1", 1});
            }
            else {
                Assert.IsFalse(_listenerWindow.IsInvoked);
                Assert.IsFalse(_listenerSelect.IsInvoked);
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E1", 1 } });
    
            // Delete all events, 1 row expected
            SendSupportBean_A("A2");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetOldAndReset(), fields, new object[]{"E1", 1});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E2", 2);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E2", 2 } });
    
            // Stop the deleting statement
            stmtDelete.Stop();
            SendSupportBean_A("A2");
            Assert.IsFalse(_listenerWindow.IsInvoked);
    
            // Start the deleting statement
            stmtDelete.Start();
    
            SendSupportBean_A("A3");
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetOldAndReset(), fields, new object[]{"E2", 2});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, null);
    
            SendSupportBean("E3", 3);
            if (namedWindow) {
                EPAssertionUtil.AssertProps(_listenerWindow.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
                EPAssertionUtil.AssertProps(_listenerSelect.AssertOneGetNewAndReset(), fields, new object[]{"E3", 3});
            }
            EPAssertionUtil.AssertPropsPerRow(stmtCreate.GetEnumerator(), fields, new object[][] { new object[] { "E3", 3 } });
    
            stmtDelete.Dispose();
            SendSupportBean_A("A3");
            Assert.IsFalse(_listenerWindow.IsInvoked);
    
            _epService.EPAdministrator.DestroyAllStatements();
            _epService.EPAdministrator.Configuration.RemoveEventType("MyInfra", false);
        }
    
        private SupportBean_A SendSupportBean_A(string id)
        {
            SupportBean_A bean = new SupportBean_A(id);
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    
        private SupportBean SendSupportBean(string theString, int intPrimitive)
        {
            SupportBean bean = new SupportBean();
            bean.TheString = theString;
            bean.IntPrimitive = intPrimitive;
            _epService.EPRuntime.SendEvent(bean);
            return bean;
        }
    }
}

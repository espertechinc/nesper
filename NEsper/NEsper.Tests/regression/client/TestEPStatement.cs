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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    [TestFixture]
    public class TestEPStatement
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration config = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
        }
    
        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }
    
        [Test]
        public void TestListenerWithReplay()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatementSPI stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:length(2)");
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
    
            // test empty statement
            stmt.AddEventHandlerWithReplay(_listener.Update);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.IsNull(_listener.NewDataList[0]);
            _listener.Reset();
    
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("TheString"));
            stmt.Dispose();
            _listener.Reset();
    
            // test 1 event
            stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:length(2)");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmt.AddEventHandlerWithReplay(_listener.Update);
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("TheString"));
            stmt.Dispose();
            _listener.Reset();
    
            // test 2 events
            stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:length(2)");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            stmt.AddEventHandlerWithReplay(_listener.Update);
            EPAssertionUtil.AssertPropsPerRow(_listener.LastNewData, new String[]{"TheString"}, new Object[][]{new Object[] {"E1"}, new Object[] {"E2"}});
    
            // test stopped statement and destroyed statement
            _listener.Reset();
            stmt.Stop();
            stmt.RemoveAllEventHandlers();
    
            stmt.AddEventHandlerWithReplay(_listener.Update);
            Assert.IsTrue(_listener.IsInvoked);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.IsNull(_listener.NewDataList[0]);
            _listener.Reset();
    
            // test destroyed
            _listener.Reset();
            stmt.Dispose();
            try
            {
                stmt.AddEventHandlerWithReplay(_listener.Update);
                Assert.Fail();
            } catch (IllegalStateException ex) {
                //
            }
    
            stmt.RemoveAllEventHandlers();
            stmt.Events -= _listener.Update;
            //stmt.RemoveListener(new SupportStmtAwareUpdateListener());
            stmt.Subscriber = new SupportSubscriber();
    
            var a = stmt.Annotations;
            var b = stmt.State;
            var c = stmt.Subscriber;
    
            try {
                stmt.Events += _listener.Update;
                Assert.Fail();
            } catch (IllegalStateException ex) {
                //
            }
            try
            {
                stmt.Events += (new SupportStmtAwareUpdateListener()).Update;
                Assert.Fail();
            } catch (IllegalStateException ex) {
                //
            }

            // test named window and having-clause
            _epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                "create window SupportBeanWindow.win:keepall() as SupportBean;\n" +
                "insert into SupportBeanWindow select * from SupportBean;\n");
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            var stmtWHaving = _epService.EPAdministrator.CreateEPL("select theString, intPrimitive from SupportBeanWindow having intPrimitive > 4000");
            stmtWHaving.AddEventHandlerWithReplay(_listener.Update);
        }
    
        [Test]
        public void TestStartedDestroy() {
            SendTimer(1000);
    
            String text = "select * from " + typeof(SupportBean).FullName;
            EPStatementSPI stmt = (EPStatementSPI) _epService.EPAdministrator.CreateEPL(text, "s1");
            Assert.IsTrue(stmt.StatementContext.IsStatelessSelect);
            Assert.AreEqual(1000l, stmt.TimeLastStateChange);
            Assert.AreEqual(false, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(true, stmt.IsStarted);
    
            stmt.Events += _listener.Update;
            SendEvent();
            _listener.AssertOneGetNewAndReset();
    
            SendTimer(2000);
            stmt.Dispose();
            Assert.AreEqual(2000l, stmt.TimeLastStateChange);
            Assert.AreEqual(true, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(false, stmt.IsStarted);
    
            SendEvent();
            Assert.IsFalse(_listener.IsInvoked);
    
            AssertStmtDestroyed(stmt, text);
        }
    
        [Test]
        public void TestStopDestroy() {
            SendTimer(5000);
            String text = "select * from " + typeof(SupportBean).FullName;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(text, "s1");
            Assert.AreEqual(false, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(true, stmt.IsStarted);
            Assert.AreEqual(5000l, stmt.TimeLastStateChange);
            stmt.Events += _listener.Update;
            SendEvent();
            _listener.AssertOneGetNewAndReset();
    
            SendTimer(6000);
            stmt.Stop();
            Assert.AreEqual(6000l, stmt.TimeLastStateChange);
            Assert.AreEqual(false, stmt.IsDisposed);
            Assert.AreEqual(true, stmt.IsStopped);
            Assert.AreEqual(false, stmt.IsStarted);
    
            SendEvent();
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(7000);
            stmt.Dispose();
            Assert.AreEqual(true, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(false, stmt.IsStarted);
            Assert.AreEqual(7000l, stmt.TimeLastStateChange);
            SendEvent();
            Assert.IsFalse(_listener.IsInvoked);
    
            AssertStmtDestroyed(stmt, text);
    
            // test fire-stop service
            EPStatementSPI spiOne = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select * from System.Object");
            StopCallbackImpl callbackOne = new StopCallbackImpl();
            spiOne.StatementContext.StatementStopService.StatementStopped += callbackOne.StatementStopped;
            spiOne.Dispose();
            Assert.IsTrue(callbackOne.IsStopped);
    
            EPStatementSPI spiTwo = (EPStatementSPI) _epService.EPAdministrator.CreateEPL("select * from System.Object");
            StopCallbackImpl callbackTwo = new StopCallbackImpl();
            spiTwo.StatementContext.StatementStopService.StatementStopped += callbackTwo.StatementStopped;
            spiTwo.Stop();
            Assert.IsTrue(callbackTwo.IsStopped);
        }
    
        private void AssertStmtDestroyed(EPStatement stmt, String text) {
            Assert.AreEqual(EPStatementState.DESTROYED, stmt.State);
            Assert.AreEqual(text, stmt.Text);
            Assert.AreEqual("s1", stmt.Name);
            Assert.IsNull(_epService.EPAdministrator.GetStatement("s1"));
            EPAssertionUtil.AssertEqualsAnyOrder(new String[0], _epService.EPAdministrator.StatementNames);
    
            try {
                stmt.Dispose();
                Assert.Fail();
            } catch (IllegalStateException ex) {
                // expected
                Assert.AreEqual("Statement already destroyed", ex.Message);
            }
    
            try {
                stmt.Start();
                Assert.Fail();
            } catch (IllegalStateException ex) {
                // expected
                Assert.AreEqual("Cannot start statement, statement is in destroyed state", ex.Message);
            }
    
            try {
                stmt.Stop();
                Assert.Fail();
            } catch (IllegalStateException ex) {
                // expected
                Assert.AreEqual("Cannot stop statement, statement is in destroyed state", ex.Message);
            }
        }
    
        private void SendEvent() {
            SupportBean bean = new SupportBean();
            _epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(long timeInMSec) {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private class StopCallbackImpl
        {
            public StopCallbackImpl()
            {
                IsStopped = false;
            }

            public void StatementStopped() {
                IsStopped = true;
            }

            public bool IsStopped { get; set; }
        }
    }
}

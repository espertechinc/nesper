///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.client
{
    public class ExecClientEPStatement : RegressionExecution
    {
        public override void Run(EPServiceProvider epService) {
            RunAssertionListenerWithReplay(epService);
            RunAssertionStartedDestroy(epService);
            RunAssertionStopDestroy(epService);
        }
    
        private void RunAssertionListenerWithReplay(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from SupportBean#length(2)");
            Assert.IsFalse(stmt.StatementContext.IsStatelessSelect);
            var listener = new SupportUpdateListener();
    
            // test empty statement
            stmt.AddEventHandlerWithReplay(listener.Update);
            Assert.IsTrue(listener.IsInvoked);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.IsNull(listener.NewDataList[0]);
            listener.Reset();
    
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("TheString"));
            stmt.Dispose();
            listener.Reset();
    
            // test 1 event
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from SupportBean#length(2)");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            stmt.AddEventHandlerWithReplay(listener.Update);
            Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("TheString"));
            stmt.Dispose();
            listener.Reset();
    
            // test 2 events
            stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from SupportBean#length(2)");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            epService.EPRuntime.SendEvent(new SupportBean("E2", 1));
            stmt.AddEventHandlerWithReplay(listener.Update);
            EPAssertionUtil.AssertPropsPerRow(listener.LastNewData, new string[]{"TheString"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            // test stopped statement and destroyed statement
            listener.Reset();
            stmt.Stop();
            stmt.RemoveAllEventHandlers();

            stmt.AddEventHandlerWithReplay(listener.Update);
            Assert.IsTrue(listener.IsInvoked);
            Assert.AreEqual(1, listener.NewDataList.Count);
            Assert.IsNull(listener.NewDataList[0]);
            listener.Reset();
    
            // test destroyed
            listener.Reset();
            stmt.Dispose();
            try {
                stmt.AddEventHandlerWithReplay(listener.Update);
                Assert.Fail();
            } catch (IllegalStateException) {
                //
            }
    
            stmt.RemoveAllEventHandlers();
            stmt.Events -= listener.Update;
            stmt.Events -= (new SupportStmtAwareUpdateListener()).Update;
            stmt.Subscriber = new SupportSubscriber();
    
            Assert.NotNull(stmt.Annotations);
            Assert.NotNull(stmt.State);
            Assert.NotNull(stmt.Subscriber);
    
            try {
                stmt.Events += listener.Update;
                Assert.Fail();
            } catch (IllegalStateException) {
                //
            }
            try {
                stmt.Events += (new SupportStmtAwareUpdateListener()).Update;
                Assert.Fail();
            } catch (IllegalStateException) {
                //
            }
    
            // test named window and having-clause
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(
                    "create window SupportBeanWindow#keepall as SupportBean;\n" +
                            "insert into SupportBeanWindow select * from SupportBean;\n");
            epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            EPStatement stmtWHaving = epService.EPAdministrator.CreateEPL("select TheString, IntPrimitive from SupportBeanWindow having IntPrimitive > 4000");
            stmtWHaving.AddEventHandlerWithReplay(listener.Update);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void RunAssertionStartedDestroy(EPServiceProvider epService) {
            SendTimer(epService, 1000);
    
            string text = "select * from " + typeof(SupportBean).FullName;
            EPStatementSPI stmt = (EPStatementSPI) epService.EPAdministrator.CreateEPL(text, "s1");
            Assert.IsTrue(stmt.StatementContext.IsStatelessSelect);
            Assert.AreEqual(1000L, stmt.TimeLastStateChange);
            Assert.AreEqual(false, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(true, stmt.IsStarted);
    
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendEvent(epService);
            listener.AssertOneGetNewAndReset();
    
            SendTimer(epService, 2000);
            stmt.Dispose();
            Assert.AreEqual(2000L, stmt.TimeLastStateChange);
            Assert.AreEqual(true, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(false, stmt.IsStarted);
    
            SendEvent(epService);
            Assert.IsFalse(listener.IsInvoked);
    
            AssertStmtDestroyed(epService, stmt, text);
        }
    
        private void RunAssertionStopDestroy(EPServiceProvider epService) {
            SendTimer(epService, 5000);
            string text = "select * from " + typeof(SupportBean).FullName;
            EPStatement stmt = epService.EPAdministrator.CreateEPL(text, "s1");
            Assert.AreEqual(false, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(true, stmt.IsStarted);
            Assert.AreEqual(5000L, stmt.TimeLastStateChange);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
            SendEvent(epService);
            listener.AssertOneGetNewAndReset();
    
            SendTimer(epService, 6000);
            stmt.Stop();
            Assert.AreEqual(6000L, stmt.TimeLastStateChange);
            Assert.AreEqual(false, stmt.IsDisposed);
            Assert.AreEqual(true, stmt.IsStopped);
            Assert.AreEqual(false, stmt.IsStarted);
    
            SendEvent(epService);
            Assert.IsFalse(listener.IsInvoked);
    
            SendTimer(epService, 7000);
            stmt.Dispose();
            Assert.AreEqual(true, stmt.IsDisposed);
            Assert.AreEqual(false, stmt.IsStopped);
            Assert.AreEqual(false, stmt.IsStarted);
            Assert.AreEqual(7000L, stmt.TimeLastStateChange);
            SendEvent(epService);
            Assert.IsFalse(listener.IsInvoked);
    
            AssertStmtDestroyed(epService, stmt, text);
    
            // test fire-stop service
            EPStatementSPI spiOne = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from System.Object");
            var callbackOne = new StopCallbackImpl();
            spiOne.StatementContext.StatementStopService.StatementStopped += callbackOne.StatementStopped;
            spiOne.Dispose();
            Assert.IsTrue(callbackOne.IsStopped);
    
            EPStatementSPI spiTwo = (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from System.Object");
            var callbackTwo = new StopCallbackImpl();
            spiTwo.StatementContext.StatementStopService.StatementStopped += callbackTwo.StatementStopped;
            spiTwo.Stop();
            Assert.IsTrue(callbackTwo.IsStopped);
    
            epService.EPAdministrator.DestroyAllStatements();
        }
    
        private void AssertStmtDestroyed(EPServiceProvider epService, EPStatement stmt, string text) {
            Assert.AreEqual(EPStatementState.DESTROYED, stmt.State);
            Assert.AreEqual(text, stmt.Text);
            Assert.AreEqual("s1", stmt.Name);
            Assert.IsNull(epService.EPAdministrator.GetStatement("s1"));
            EPAssertionUtil.AssertEqualsAnyOrder(new string[0], epService.EPAdministrator.StatementNames);
    
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
    
        private void SendEvent(EPServiceProvider epService) {
            var bean = new SupportBean();
            epService.EPRuntime.SendEvent(bean);
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private class StopCallbackImpl
        {
            public bool IsStopped { get; private set; }
            public void StatementStopped() {
                IsStopped = true;
            }
        }
    }
} // end of namespace

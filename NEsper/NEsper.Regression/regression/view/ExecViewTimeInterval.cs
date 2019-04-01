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
using com.espertech.esper.client.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewTimeInterval : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            RunAssertionTimeWindowPreparedStmt(epService);
            RunAssertionTimeWindowVariableStmt(epService);
            RunAssertionTimeWindowTimePeriod(epService);
            RunAssertionTimeWindowVariableTimePeriodStmt(epService);
            RunAssertionTimeWindow(epService);
            RunAssertionTimeBatchNoRefPoint(epService);
            RunAssertionTimeBatchRefPoint(epService);
            RunAssertionExternallyTimedMonthScoped(epService);
            RunAssertionExternallyTimedBatchMonthScoped(epService);
            RunAssertionExternallyTimed(epService);
        }
    
        private void RunAssertionTimeWindowPreparedStmt(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string text = "select rstream TheString from SupportBean#time(?)";
            EPPreparedStatement prepared = epService.EPAdministrator.PrepareEPL(text);
    
            prepared.SetObject(1, 4);
            EPStatement stmtOne = epService.EPAdministrator.Create(prepared);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            prepared.SetObject(1, 3);
            EPStatement stmtTwo = epService.EPAdministrator.Create(prepared);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(epService, listenerOne, listenerTwo);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionTimeWindowVariableStmt(EPServiceProvider epService) {
            SendTimer(epService, 0);
            string text = "select rstream TheString from SupportBean#time(TIME_WIN_ONE)";
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            epService.EPAdministrator.Configuration.AddVariable("TIME_WIN_ONE", typeof(int), 4);
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(text);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            epService.EPRuntime.SetVariableValue("TIME_WIN_ONE", 3);
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(text);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(epService, listenerOne, listenerTwo);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionTimeWindowTimePeriod(EPServiceProvider epService) {
            SendTimer(epService, 0);
    
            string text = "select rstream TheString from SupportBean#time(4 sec)";
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(text);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            text = "select rstream TheString from SupportBean#time(3000 milliseconds)";
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(text);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(epService, listenerOne, listenerTwo);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionTimeWindowVariableTimePeriodStmt(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddVariable("TIME_WIN_TWO", typeof(double), 4000);
            SendTimer(epService, 0);
    
            string text = "select rstream TheString from SupportBean#time(TIME_WIN_TWO milliseconds)";
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
    
            EPStatement stmtOne = epService.EPAdministrator.CreateEPL(text);
            var listenerOne = new SupportUpdateListener();
            stmtOne.Events += listenerOne.Update;
    
            text = "select rstream TheString from SupportBean#time(TIME_WIN_TWO minutes)";
            epService.EPRuntime.SetVariableValue("TIME_WIN_TWO", 0.05);
            EPStatement stmtTwo = epService.EPAdministrator.CreateEPL(text);
            var listenerTwo = new SupportUpdateListener();
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(epService, listenerOne, listenerTwo);
    
            stmtOne.Dispose();
            stmtTwo.Dispose();
        }
    
        private void RunAssertionTimeWindow(EPServiceProvider epService) {
            TryTimeWindow(epService, "30000");
            TryTimeWindow(epService, "30E6 milliseconds");
            TryTimeWindow(epService, "30000 seconds");
            TryTimeWindow(epService, "500 minutes");
            TryTimeWindow(epService, "8.33333333333333333333 hours");
            TryTimeWindow(epService, "0.34722222222222222222222222222222 days");
            TryTimeWindow(epService, "0.1 hour 490 min 240 sec");
        }
    
        private void RunAssertionTimeBatchNoRefPoint(EPServiceProvider epService) {
            // Set up a time window with a unique view attached
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName +
                            "#time_batch(10 minutes)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 0);
    
            SendEvent(epService);
            listener.Reset();
    
            SendTimerAssertNotInvoked(epService, listener, 10 * 60 * 1000 - 1);
            SendTimerAssertInvoked(epService, listener, 10 * 60 * 1000);
    
            stmt.Dispose();
        }
    
        private void RunAssertionTimeBatchRefPoint(EPServiceProvider epService) {
            // Set up a time window with a unique view attached
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName +
                            "#time_batch(10 minutes, 10L)");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 10);
    
            SendEvent(epService);
            listener.Reset();
    
            SendTimerAssertNotInvoked(epService, listener, 10 * 60 * 1000 - 1 + 10);
            SendTimerAssertInvoked(epService, listener, 10 * 60 * 1000 + 10);
    
            stmt.Dispose();
        }
    
        private void RunAssertionExternallyTimedMonthScoped(EPServiceProvider epService) {
            var testListener = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanTimestamp));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select rstream * from SupportBean#ext_timed(LongPrimitive, 1 month)");
            stmt.Events += testListener.Update;
    
            SendExtTimeEvent(epService, DateTimeParser.ParseDefaultMSec("2002-02-01T09:00:00.000"), "E1");
            SendExtTimeEvent(epService, DateTimeParser.ParseDefaultMSec("2002-03-01T09:00:00.000") - 1, "E2");
            Assert.IsFalse(testListener.IsInvoked);
    
            SendExtTimeEvent(epService, DateTimeParser.ParseDefaultMSec("2002-03-01T09:00:00.000"), "E3");
            EPAssertionUtil.AssertProps(testListener.AssertOneGetNewAndReset(), "TheString".Split(','), new object[]{"E1"});
    
            stmt.Dispose();
        }
    
        private void RunAssertionExternallyTimedBatchMonthScoped(EPServiceProvider epService) {
            var testListener = new SupportUpdateListener();
            epService.EPAdministrator.Configuration.AddEventType(typeof(SupportBeanTimestamp));
            EPStatement stmt = epService.EPAdministrator.CreateEPL("select * from SupportBean#ext_timed_batch(LongPrimitive, 1 month)");
            stmt.Events += testListener.Update;
    
            SendExtTimeEvent(epService, DateTimeParser.ParseDefaultMSec("2002-02-01T09:00:00.000"), "E1");
            SendExtTimeEvent(epService, DateTimeParser.ParseDefaultMSec("2002-03-01T09:00:00.000") - 1, "E2");
            Assert.IsFalse(testListener.IsInvoked);
    
            SendExtTimeEvent(epService, DateTimeParser.ParseDefaultMSec("2002-03-01T09:00:00.000"), "E3");
            EPAssertionUtil.AssertPropsPerRow(testListener.GetAndResetLastNewData(), "TheString".Split(','), new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            stmt.Dispose();
        }
    
        private void RunAssertionExternallyTimed(EPServiceProvider epService) {
            // Set up a time window with a unique view attached
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName +
                            "#ext_timed(LongPrimitive, 10 minutes)");
            var testListener = new SupportUpdateListener();
            stmt.Events += testListener.Update;
    
            SendExtTimeEvent(epService, 0);
    
            testListener.Reset();
            SendExtTimeEvent(epService, 10 * 60 * 1000 - 1);
            Assert.IsNull(testListener.OldDataList[0]);
    
            testListener.Reset();
            SendExtTimeEvent(epService, 10 * 60 * 1000 + 1);
            Assert.AreEqual(1, testListener.OldDataList[0].Length);
    
            stmt.Dispose();
        }
    
        private void TryTimeWindow(EPServiceProvider epService, string intervalSpec) {
            // Set up a time window with a unique view attached
            EPStatement stmt = epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName +
                            "#time(" + intervalSpec + ")");
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            SendTimer(epService, 0);
    
            SendEvent(epService);
            listener.Reset();
    
            SendTimerAssertNotInvoked(epService, listener, 29999 * 1000);
            SendTimerAssertInvoked(epService, listener, 30000 * 1000);
    
            stmt.Dispose();
        }
    
        private void SendTimerAssertNotInvoked(EPServiceProvider epService, SupportUpdateListener listener, long timeInMSec) {
            SendTimer(epService, timeInMSec);
            Assert.IsFalse(listener.IsInvoked);
            listener.Reset();
        }
    
        private void SendTimerAssertInvoked(EPServiceProvider epService, SupportUpdateListener listener, long timeInMSec) {
            SendTimer(epService, timeInMSec);
            Assert.IsTrue(listener.IsInvoked);
            listener.Reset();
        }
    
        private void SendTimer(EPServiceProvider epService, long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService) {
            var theEvent = new SupportBean();
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvent(EPServiceProvider epService, string theString) {
            var theEvent = new SupportBean(theString, 1);
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendExtTimeEvent(EPServiceProvider epService, long longPrimitive) {
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendExtTimeEvent(EPServiceProvider epService, long longPrimitive, string theString) {
            var theEvent = new SupportBean(theString, 0);
            theEvent.LongPrimitive = longPrimitive;
            epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void RunAssertion(EPServiceProvider epService, SupportUpdateListener listenerOne, SupportUpdateListener listenerTwo) {
            SendTimer(epService, 1000);
            SendEvent(epService, "E1");
    
            SendTimer(epService, 2000);
            SendEvent(epService, "E2");
    
            SendTimer(epService, 3000);
            SendEvent(epService, "E3");
    
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            SendTimer(epService, 4000);
            Assert.AreEqual("E1", listenerTwo.AssertOneGetNewAndReset().Get("TheString"));
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            SendTimer(epService, 5000);
            Assert.AreEqual("E1", listenerOne.AssertOneGetNewAndReset().Get("TheString"));
            Assert.IsFalse(listenerOne.IsInvoked);
        }
    }
} // end of namespace

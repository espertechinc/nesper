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
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeInterval
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _testListener;
    
        [SetUp]
        public void SetUp() {
            _epService = EPServiceProviderManager.GetDefaultProvider(
                    SupportConfigFactory.GetConfiguration());
            _epService.Initialize();
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBean", typeof(SupportBean));
        }
    
        [TearDown]
        public void TearDown() {
            _testListener = null;
        }
    
        [Test]
        public void TestTimeWindowPreparedStmt() {
            SendTimer(0);
            String text = "select rstream TheString from SupportBean.win:time(?)";
            EPPreparedStatement prepared = _epService.EPAdministrator.PrepareEPL(
                    text);
    
            prepared.SetObject(1, 4);
            EPStatement stmtOne = _epService.EPAdministrator.Create(prepared);
            SupportUpdateListener listenerOne = new SupportUpdateListener();
    
            stmtOne.Events += listenerOne.Update;
    
            prepared.SetObject(1, 3);
            EPStatement stmtTwo = _epService.EPAdministrator.Create(prepared);
            SupportUpdateListener listenerTwo = new SupportUpdateListener();
    
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(listenerOne, listenerTwo);
        }
    
        [Test]
        public void TestTimeWindowVariableStmt() {
            SendTimer(0);
            String text = "select rstream TheString from SupportBean.win:time(TIME_WIN)";
    
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBean", typeof(SupportBean));
    
            _epService.EPAdministrator.Configuration.AddVariable("TIME_WIN",
                    typeof(int), 4);
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listenerOne = new SupportUpdateListener();
    
            stmtOne.Events += listenerOne.Update;
    
            _epService.EPRuntime.SetVariableValue("TIME_WIN", 3);
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listenerTwo = new SupportUpdateListener();
    
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(listenerOne, listenerTwo);
        }
    
        [Test]
        public void TestTimeWindowTimePeriod() {
            SendTimer(0);
    
            String text = "select rstream TheString from SupportBean.win:time(4 sec)";
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listenerOne = new SupportUpdateListener();
    
            stmtOne.Events += listenerOne.Update;
    
            text = "select rstream TheString from SupportBean.win:time(3000 milliseconds)";
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listenerTwo = new SupportUpdateListener();
    
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(listenerOne, listenerTwo);
        }
    
        [Test]
        public void TestTimeWindowVariableTimePeriodStmt() {
            _epService.EPAdministrator.Configuration.AddVariable("TIME_WIN",
                    typeof(double), 4000);
            SendTimer(0);
            
            String text = "select rstream TheString from SupportBean.win:time(TIME_WIN milliseconds)";
    
            _epService.EPAdministrator.Configuration.AddEventType(
                    "SupportBean", typeof(SupportBean));
    
            EPStatement stmtOne = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listenerOne = new SupportUpdateListener();
    
            stmtOne.Events += listenerOne.Update;
    
            text = "select rstream TheString from SupportBean.win:time(TIME_WIN minutes)";
            _epService.EPRuntime.SetVariableValue("TIME_WIN", 0.05);
            EPStatement stmtTwo = _epService.EPAdministrator.CreateEPL(text);
            SupportUpdateListener listenerTwo = new SupportUpdateListener();
    
            stmtTwo.Events += listenerTwo.Update;
    
            RunAssertion(listenerOne, listenerTwo);
        }
    
        [Test]
        public void TestTimeWindow() {
            TryTimeWindow("30000");
            TryTimeWindow("30E6 milliseconds");
            TryTimeWindow("30000 seconds");
            TryTimeWindow("500 minutes");
            TryTimeWindow("8.33333333333333333333 hours");
            TryTimeWindow("0.34722222222222222222222222222222 days");
            TryTimeWindow("0.1 hour 490 min 240 sec");
        }
    
        [Test]
        public void TestTimeBatchNoRefPoint() {
            // Set up a time window with a unique view attached
            EPStatement view = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName
                    + ".win:time_batch(10 minutes)");
    
            _testListener = new SupportUpdateListener();
            view.Events += _testListener.Update;
    
            SendTimer(0);
    
            SendEvent();
            _testListener.Reset();
    
            SendTimerAssertNotInvoked(10 * 60 * 1000 - 1);
            SendTimerAssertInvoked(10 * 60 * 1000);
        }
    
        [Test]
        public void TestTimeBatchRefPoint() {
            // Set up a time window with a unique view attached
            EPStatement view = _epService.EPAdministrator.CreateEPL(
                    "select * from " + typeof(SupportBean).FullName
                    + ".win:time_batch(10 minutes, 10L)");
    
            _testListener = new SupportUpdateListener();
            view.Events += _testListener.Update;
    
            SendTimer(10);
    
            SendEvent();
            _testListener.Reset();
    
            SendTimerAssertNotInvoked(10 * 60 * 1000 - 1 + 10);
            SendTimerAssertInvoked(10 * 60 * 1000 + 10);
        }

        [Test]
        public void TestExternallyTimedMonthScoped()
        {
            _testListener = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanTimestamp>();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select rstream * from SupportBean.win:ext_timed(LongPrimitive, 1 month)");
            stmt.Events += _testListener.Update;

            SendExtTimeEvent(DateTimeParser.ParseDefaultMSec("2002-02-01T9:00:00.000"), "E1");
            SendExtTimeEvent(DateTimeParser.ParseDefaultMSec("2002-03-01T9:00:00.000") - 1, "E2");
            Assert.IsFalse(_testListener.IsInvoked);

            SendExtTimeEvent(DateTimeParser.ParseDefaultMSec("2002-03-01T9:00:00.000"), "E3");
            EPAssertionUtil.AssertProps(_testListener.AssertOneGetNewAndReset(), "TheString".Split(','), new Object[]{"E1"});
        }

        [Test]
        public void TestExternallyTimedBatchMonthScoped()
        {
            _testListener = new SupportUpdateListener();
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanTimestamp>();
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select * from SupportBean.win:ext_timed_batch(LongPrimitive, 1 month)");
            stmt.Events += _testListener.Update;

            SendExtTimeEvent(DateTimeParser.ParseDefaultMSec("2002-02-01T9:00:00.000"), "E1");
            SendExtTimeEvent(DateTimeParser.ParseDefaultMSec("2002-03-01T9:00:00.000") - 1, "E2");
            Assert.IsFalse(_testListener.IsInvoked);

            SendExtTimeEvent(DateTimeParser.ParseDefaultMSec("2002-03-01T9:00:00.000"), "E3");
            EPAssertionUtil.AssertPropsPerRow(_testListener.GetAndResetLastNewData(), "TheString".Split(','), new Object[][]{ new object[] {"E1"}, new object[] {"E2"}});
        }
    
        [Test]
        public void TestExternallyTimed() {
            // Set up a time window with a unique view attached
            EPStatement view = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName
                    + ".win:ext_timed(LongPrimitive, 10 minutes)");
    
            _testListener = new SupportUpdateListener();
            view.Events += _testListener.Update;
    
            SendExtTimeEvent(0);
    
            _testListener.Reset();
            SendExtTimeEvent(10 * 60 * 1000 - 1);
            Assert.IsNull(_testListener.OldDataList[0]);
    
            _testListener.Reset();
            SendExtTimeEvent(10 * 60 * 1000 + 1);
            Assert.AreEqual(1, _testListener.OldDataList[0].Length);
        }
    
        private void TryTimeWindow(String intervalSpec) {
            // Set up a time window with a unique view attached
            EPStatement view = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBean).FullName
                    + ".win:time(" + intervalSpec + ")");
    
            _testListener = new SupportUpdateListener();
            view.Events += _testListener.Update;
    
            SendTimer(0);
    
            SendEvent();
            _testListener.Reset();
    
            SendTimerAssertNotInvoked(29999 * 1000);
            SendTimerAssertInvoked(30000 * 1000);
        }
    
        private void SendTimerAssertNotInvoked(long timeInMSec) {
            SendTimer(timeInMSec);
            Assert.IsFalse(_testListener.IsInvoked);
            _testListener.Reset();
        }
    
        private void SendTimerAssertInvoked(long timeInMSec) {
            SendTimer(timeInMSec);
            Assert.IsTrue(_testListener.IsInvoked);
            _testListener.Reset();
        }
    
        private void SendTimer(long timeInMSec) {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
    
            runtime.SendEvent(theEvent);
        }
    
        private void SendEvent() {
            SupportBean theEvent = new SupportBean();
    
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendEvent(String theString) {
            SupportBean theEvent = new SupportBean(theString, 1);
    
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void SendExtTimeEvent(long longPrimitive) {
            SupportBean theEvent = new SupportBean();
    
            theEvent.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }

        private void SendExtTimeEvent(long longPrimitive, String theString)
        {
            SupportBean theEvent = new SupportBean(theString, 0);
            theEvent.LongPrimitive = longPrimitive;
            _epService.EPRuntime.SendEvent(theEvent);
        }
    
        private void RunAssertion(SupportUpdateListener listenerOne, SupportUpdateListener listenerTwo)
        {
            SendTimer(1000);
            SendEvent("E1");
    
            SendTimer(2000);
            SendEvent("E2");
    
            SendTimer(3000);
            SendEvent("E3");
    
            Assert.IsFalse(listenerOne.IsInvoked);
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            SendTimer(4000);
            Assert.AreEqual("E1",
                    listenerTwo.AssertOneGetNewAndReset().Get("TheString"));
            Assert.IsFalse(listenerTwo.IsInvoked);
    
            SendTimer(5000);
            Assert.AreEqual("E1",
                    listenerOne.AssertOneGetNewAndReset().Get("TheString"));
            Assert.IsFalse(listenerOne.IsInvoked);
        }

        private void SendCurrentTime(String time)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time)));
        }

        private void SendCurrentTimeWithMinus(String time, long minus)
        {
            _epService.EPRuntime.SendEvent(new CurrentTimeEvent(DateTimeParser.ParseDefaultMSec(time) - minus));
        }
    }
}

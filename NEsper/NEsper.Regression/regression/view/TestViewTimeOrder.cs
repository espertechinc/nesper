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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeOrder
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
    
        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            var configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
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
        public void TestMonthScoped()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBeanTimestamp>();
            SendCurrentTime("2002-02-01T09:00:00.000");
            var stmt = _epService.EPAdministrator.CreateEPL("select rstream * from SupportBeanTimestamp#time_order(timestamp, 1 month)");
            stmt.Events += _listener.Update;

            SendEvent("E1", DateTimeParser.ParseDefaultMSec("2002-02-01T09:00:00.000"));
            SendCurrentTimeWithMinus("2002-03-01T09:00:00.000", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendCurrentTime("2002-03-01T09:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "id".Split(','), new object[][] { new object[] {"E1"}});
        }
    
        [Test]
        public void TestTimeOrderRemoveStream() {
            SendTimer(1000);
            _epService.EPAdministrator.CreateEPL(
                    "insert rstream into OrderedStream select rstream * from " + typeof(SupportBeanTimestamp).FullName +
                            "#time_order(timestamp, 10 sec)");
    
            var stmtTwo = _epService.EPAdministrator.CreateEPL(
                    "select * from OrderedStream");
            stmtTwo.Events += _listener.Update;
    
            // 1st event at 21 sec
            SendTimer(21000);
            SendEvent("E1", 21000);
    
            // 2nd event at 22 sec
            SendTimer(22000);
            SendEvent("E2", 22000);
    
            // 3nd event at 28 sec
            SendTimer(28000);
            SendEvent("E3", 28000);
    
            // 4th event at 30 sec, however is 27 sec (old 3 sec)
            SendTimer(30000);
            SendEvent("E4", 27000);
    
            // 5th event at 30 sec, however is 22 sec (old 8 sec)
            SendEvent("E5", 22000);
    
            // flush one
            SendTimer(30999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(31000);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E1", _listener.LastNewData[0].Get("id"));
            _listener.Reset();
    
            // 6th event at 31 sec, however is 21 sec (old 10 sec)
            SendEvent("E6", 21000);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E6", _listener.LastNewData[0].Get("id"));
            _listener.Reset();
    
            // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
            SendEvent("E7", 21300);
    
            // flush one
            SendTimer(31299);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(31300);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E7", _listener.LastNewData[0].Get("id"));
            _listener.Reset();
    
            // flush two
            SendTimer(31999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(32000);
    
            var result = _listener.GetNewDataListFlattened();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("E2", result[0].Get("id"));
            Assert.AreEqual("E5", result[1].Get("id"));
            _listener.Reset();
    
            // flush one
            SendTimer(36999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(37000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E4", _listener.LastNewData[0].Get("id"));
            _listener.Reset();
    
            // rather old event
            SendEvent("E8", 21000);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E8", _listener.LastNewData[0].Get("id"));
            _listener.Reset();
    
            // 9-second old event for posting at 38 sec
            SendEvent("E9", 28000);
    
            // flush two
            SendTimer(37999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(38000);
            result = _listener.GetNewDataListFlattened();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("E3", result[0].Get("id"));
            Assert.AreEqual("E9", result[1].Get("id"));
            _listener.Reset();
        }
    
        [Test]
        public void TestTimeOrder() {
            SendTimer(1000);
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBeanTimestamp).FullName +
                            "#time_order(timestamp, 10 sec)");
            stmt.Events += _listener.Update;
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            SendTimer(21000);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // 1st event at 21 sec
            SendEvent("E1", 21000);
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}});
    
            // 2nd event at 22 sec
            SendTimer(22000);
            SendEvent("E2", 22000);
            Assert.AreEqual("E2", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}});
    
            // 3nd event at 28 sec
            SendTimer(28000);
            SendEvent("E3", 28000);
            Assert.AreEqual("E3", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E3"}});
    
            // 4th event at 30 sec, however is 27 sec (old 3 sec)
            SendTimer(30000);
            SendEvent("E4", 27000);
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // 5th event at 30 sec, however is 22 sec (old 8 sec)
            SendEvent("E5", 22000);
            Assert.AreEqual("E5", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}, new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // flush one
            SendTimer(30999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(31000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E1", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // 6th event at 31 sec, however is 21 sec (old 10 sec)
            SendEvent("E6", 21000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E6", _listener.LastNewData[0].Get("id"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E6", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // 7th event at 31 sec, however is 21.3 sec (old 9.7 sec)
            SendEvent("E7", 21300);
            Assert.AreEqual("E7", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E7"}, new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // flush one
            SendTimer(31299);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(31300);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E7", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            // flush two
            SendTimer(31999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(32000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            Assert.AreEqual("E2", _listener.LastOldData[0].Get("id"));
            Assert.AreEqual("E5", _listener.LastOldData[1].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E4"}, new object[] {"E3"}});
    
            // flush one
            SendTimer(36999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(37000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E4", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E3"}});
    
            // rather old event
            SendEvent("E8", 21000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E8", _listener.LastNewData[0].Get("id"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E8", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E3"}});
    
            // 9-second old event for posting at 38 sec
            SendEvent("E9", 28000);
            Assert.AreEqual("E9", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E3"}, new object[] {"E9"}});
    
            // flush two
            SendTimer(37999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(38000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            Assert.AreEqual("E3", _listener.LastOldData[0].Get("id"));
            Assert.AreEqual("E9", _listener.LastOldData[1].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // new event
            SendEvent("E10", 38000);
            Assert.AreEqual("E10", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E10"}});
    
            // flush last
            SendTimer(47999);
            Assert.IsFalse(_listener.IsInvoked);
            SendTimer(48000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E10", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // last, in the future
            SendEvent("E11", 70000);
            Assert.AreEqual("E11", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E11"}});
    
            SendTimer(80000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E11", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            SendTimer(100000);
            Assert.IsFalse(_listener.IsInvoked);
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
        }
    
        [Test]
        public void TestGroupedWindow() {
            SendTimer(20000);
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportBeanTimestamp).FullName +
                            "#groupwin(groupId)#time_order(timestamp, 10 sec)");
            stmt.Events += _listener.Update;
    
            // 1st event is old
            SendEvent("E1", "G1", 10000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.LastNewData.Length);
            Assert.AreEqual("E1", _listener.LastNewData[0].Get("id"));
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E1", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            // 2nd just fits
            SendEvent("E2", "G2", 10001);
            Assert.AreEqual("E2", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}});
    
            SendEvent("E3", "G3", 20000);
            Assert.AreEqual("E3", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E3"}});
    
            SendEvent("E4", "G2", 20000);
            Assert.AreEqual("E4", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E2"}, new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(20001);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E2", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(22000);
            SendEvent("E5", "G2", 19000);
            Assert.AreEqual("E5", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E5"}, new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(29000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            Assert.AreEqual("E5", _listener.LastOldData[0].Get("id"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E4"}, new object[] {"E3"}});
    
            SendTimer(30000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            EPAssertionUtil.AssertPropsPerRowAnyOrder(_listener.LastOldData, "id".SplitCsv(), new object[][] { new object[] { "E4" }, new object[] { "E3" } });
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, null);
    
            SendTimer(100000);
            Assert.IsFalse(_listener.IsInvoked);
        }
    
        [Test]
        public void TestInvalid()
        {
            SupportMessageAssertUtil.TryInvalid(_epService, "select * from " + Name.Clean<SupportBeanTimestamp>() + "#time_order(bump, 10 sec)",
            "Error starting statement: Error attaching view to event stream: Invalid parameter expression 0 for Time-Order view: Failed to validate view parameter expression 'bump': Property named 'bump' is not valid in any stream [");

            SupportMessageAssertUtil.TryInvalid(_epService, "select * from " + Name.Clean<SupportBeanTimestamp>() + "#time_order(10 sec)",
            "Error starting statement: Error attaching view to event stream: Time-Order view requires the expression supplying timestamp values, and a numeric or time period parameter for interval size [");

            SupportMessageAssertUtil.TryInvalid(_epService, "select * from " + Name.Clean<SupportBeanTimestamp>() + "#time_order(timestamp, abc)",
            "Error starting statement: Error attaching view to event stream: Invalid parameter expression 1 for Time-Order view: Failed to validate view parameter expression 'abc': Property named 'abc' is not valid in any stream (did you mean 'Id'?) [");
        }
    
        [Test]
        public void TestPreviousAndPrior()
        {
            SendTimer(1000);
            var stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream id, " +
                            " prev(0, id) as prevIdZero, " +
                            " prev(1, id) as prevIdOne, " +
                            " prior(1, id) as priorIdOne," +
                            " prevtail(0, id) as prevTailIdZero, " +
                            " prevtail(1, id) as prevTailIdOne, " +
                            " prevcount(id) as prevCountId, " +
                            " prevwindow(id) as prevWindowId " +
                            " from " + typeof(SupportBeanTimestamp).FullName +
                            "#time_order(timestamp, 10 sec)");
            var fields = new string[]{"id", "prevIdZero", "prevIdOne", "priorIdOne", "prevTailIdZero", "prevTailIdOne", "prevCountId"};
            stmt.Events += _listener.Update;
    
            SendTimer(20000);
            SendEvent("E1", 25000);
            Assert.AreEqual("E1", _listener.AssertOneGetNewAndReset().Get("id"));
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), new string[]{"id"}, new object[][]{new object[] {"E1"}});
    
            SendEvent("E2", 21000);
            var theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E2", theEvent.Get("id"));
            Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
            Assert.AreEqual("E1", theEvent.Get("prevIdOne"));
            Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
            Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
            Assert.AreEqual("E2", theEvent.Get("prevTailIdOne"));
            Assert.AreEqual(2L, theEvent.Get("prevCountId"));
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("prevWindowId"), new object[]{"E2", "E1"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E2", "E1", "E1", "E1", "E2", 2L}, new object[] {"E1", "E2", "E1", null, "E1", "E2", 2L}});
    
            SendEvent("E3", 22000);
            theEvent = _listener.AssertOneGetNewAndReset();
            Assert.AreEqual("E3", theEvent.Get("id"));
            Assert.AreEqual("E2", theEvent.Get("prevIdZero"));
            Assert.AreEqual("E3", theEvent.Get("prevIdOne"));
            Assert.AreEqual("E2", theEvent.Get("priorIdOne"));
            Assert.AreEqual("E1", theEvent.Get("prevTailIdZero"));
            Assert.AreEqual("E3", theEvent.Get("prevTailIdOne"));
            Assert.AreEqual(3L, theEvent.Get("prevCountId"));
            EPAssertionUtil.AssertEqualsExactOrder((object[]) theEvent.Get("prevWindowId"), new object[]{"E2", "E3", "E1"});
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E2", "E2", "E3", "E1", "E1", "E3", 3L}, new object[] {"E3", "E2", "E3", "E2", "E1", "E3", 3L}, new object[] {"E1", "E2", "E3", null, "E1", "E3", 3L}});
    
            SendTimer(31000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            theEvent = _listener.LastOldData[0];
            Assert.AreEqual("E2", theEvent.Get("id"));
            Assert.AreEqual(null, theEvent.Get("prevIdZero"));
            Assert.AreEqual(null, theEvent.Get("prevIdOne"));
            Assert.AreEqual("E1", theEvent.Get("priorIdOne"));
            Assert.AreEqual(null, theEvent.Get("prevTailIdZero"));
            Assert.AreEqual(null, theEvent.Get("prevTailIdOne"));
            Assert.AreEqual(null, theEvent.Get("prevCountId"));
            Assert.AreEqual(null, theEvent.Get("prevWindowId"));
            _listener.Reset();
            EPAssertionUtil.AssertPropsPerRow(stmt.GetEnumerator(), fields,
                    new object[][]{new object[] {"E3", "E3", "E1", "E2", "E1", "E3", 2L}, new object[] {"E1", "E3", "E1", null, "E1", "E3", 2L}});
        }
    
        private SupportBeanTimestamp SendEvent(String id, String groupId, long timestamp) {
            var theEvent = new SupportBeanTimestamp(id, groupId, timestamp);
            var runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
            return theEvent;
        }
    
        private SupportBeanTimestamp SendEvent(String id, long timestamp) {
            var theEvent = new SupportBeanTimestamp(id, timestamp);
            var runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
            return theEvent;
        }
    
        private void SendTimer(long timeInMSec) {
            var theEvent = new CurrentTimeEvent(timeInMSec);
            var runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
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

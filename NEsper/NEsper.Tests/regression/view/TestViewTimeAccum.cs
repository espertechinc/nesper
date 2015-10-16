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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeAccum
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportMarketDataBean[] _events;
    
        [SetUp]
        public void SetUp() {
            _listener = new SupportUpdateListener();
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }
    
            _events = new SupportMarketDataBean[100];
            for (int i = 0; i < _events.Length; i++) {
                int group = i % 10;
                _events[i] = new SupportMarketDataBean("S" + Convert.ToString(group), "id_" + Convert.ToString(i), i);
            }
        }
    
        [TearDown]
        public void TearDown() {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
        }

        [Test]
        public void TestMonthScoped()
        {
            _epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            SendCurrentTime("2002-02-01T9:00:00.000");
            EPStatement stmt = _epService.EPAdministrator.CreateEPL("select rstream * from SupportBean.win:time_accum(1 month)");
            stmt.Events += _listener.Update;
            _epService.EPRuntime.SendEvent(new SupportBean("E1", 1));
            _epService.EPRuntime.SendEvent(new SupportBean("E2", 2));

            SendCurrentTimeWithMinus("2002-03-01T9:00:00.000", 1);
            Assert.IsFalse(_listener.IsInvoked);

            SendCurrentTime("2002-03-01T9:00:00.000");
            EPAssertionUtil.AssertPropsPerRow(_listener.GetAndResetLastNewData(), "TheString".Split(','),
                new Object[][]
                {
                    new object[] {"E1"},
                    new object[] {"E2"}
                });
        }
    
        [Test]
        public void TestTimeAccum() {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            ".win:time_accum(10 sec)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;
    
            SendTimer(startTime + 10000);
            Assert.IsFalse(_listener.IsInvoked);
    
            // 1st at 10 sec
            engine.SendEvent(_events[0]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[0]);
    
            // 2nd event at 14 sec
            SendTimer(startTime + 14000);
            engine.SendEvent(_events[1]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[1]);
    
            // 3nd event at 14 sec
            SendTimer(startTime + 14000);
            engine.SendEvent(_events[2]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[2]);
    
            // 3rd event at 23 sec
            SendTimer(startTime + 23000);
            engine.SendEvent(_events[3]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[3]);
    
            // no event till 33 sec
            SendTimer(startTime + 32999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(startTime + 33000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(4, _listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[0], _events[1], _events[2], _events[3]}, _listener.GetOldDataListFlattened());
            _listener.Reset();
    
            // no events till 50 sec
            SendTimer(startTime + 50000);
            Assert.IsFalse(_listener.IsInvoked);
    
            // next two events at 55 sec
            SendTimer(startTime + 55000);
            engine.SendEvent(_events[4]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[4]);
            engine.SendEvent(_events[5]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[5]);
    
            // no event till 65 sec
            SendTimer(startTime + 64999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(startTime + 65000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[4], _events[5]}, _listener.GetOldDataListFlattened());
            _listener.Reset();
    
            // next window
            engine.SendEvent(_events[6]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[6]);
    
            SendTimer(startTime + 74999);
            engine.SendEvent(_events[7]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[7]);
    
            SendTimer(startTime + 74999 + 10000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[6], _events[7]}, _listener.GetOldDataListFlattened());
            _listener.Reset();
        }
    
        [Test]
        public void TestTimeAccumRStream() {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select rstream * from " + typeof(SupportMarketDataBean).FullName +
                            ".win:time_accum(10 sec)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;
    
            SendTimer(startTime + 10000);
            Assert.IsFalse(_listener.IsInvoked);
    
            // some events at 10 sec
            engine.SendEvent(_events[0]);
            engine.SendEvent(_events[1]);
            engine.SendEvent(_events[2]);
            Assert.IsFalse(_listener.IsInvoked);
    
            // flush out of the window
            SendTimer(startTime + 20000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[0], _events[1], _events[2]}, _listener.GetNewDataListFlattened());
            _listener.Reset();
        }
    
        [Test]
        public void TestPreviousAndPrior() {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream Price, prev(1, Price) as prevPrice, prior(1, Price) as priorPrice from " + typeof(SupportMarketDataBean).FullName +
                            ".win:time_accum(10 sec)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;
    
            // 1st event
            SendTimer(startTime + 20000);
            engine.SendEvent(_events[5]);
            AssertData(_listener.AssertOneGetNewAndReset(), 5d, null, null);
    
            // 2nd event
            SendTimer(startTime + 25000);
            engine.SendEvent(_events[6]);
            AssertData(_listener.AssertOneGetNewAndReset(), 6d, 5d, 5d);
    
            // 3nd event
            SendTimer(startTime + 34000);
            engine.SendEvent(_events[7]);
            AssertData(_listener.AssertOneGetNewAndReset(), 7d, 6d, 6d);
    
            SendTimer(startTime + 43999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(startTime + 44000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(3, _listener.LastOldData.Length);
            AssertData(_listener.LastOldData[0], 5d, null, null);
            AssertData(_listener.LastOldData[1], 6d, null, 5d);
            AssertData(_listener.LastOldData[2], 7d, null, 6d);
            _listener.Reset();
        }
    
        [Test]
        public void TestSum() {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream sum(Price) as sumPrice from " + typeof(SupportMarketDataBean).FullName +
                            ".win:time_accum(10 sec)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;
    
            // 1st event
            SendTimer(startTime + 20000);
            engine.SendEvent(_events[5]);
            AssertData(_listener.LastNewData[0], 5d);
            AssertData(_listener.LastOldData[0], null);
            _listener.Reset();
    
            // 2nd event
            SendTimer(startTime + 25000);
            engine.SendEvent(_events[6]);
            AssertData(_listener.LastNewData[0], 11d);
            AssertData(_listener.LastOldData[0], 5d);
            _listener.Reset();
    
            SendTimer(startTime + 34999);
            Assert.IsFalse(_listener.IsInvoked);
    
            SendTimer(startTime + 35000);
            AssertData(_listener.LastNewData[0], null);
            AssertData(_listener.LastOldData[0], 11d);
            _listener.Reset();
        }
    
        [Test]
        public void TestGroupedWindow() {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            ".std:groupwin(symbol).win:time_accum(10 sec)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;
    
            // 1st S1 event
            SendTimer(startTime + 10000);
            engine.SendEvent(_events[1]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[1]);
    
            // 1st S2 event
            SendTimer(startTime + 12000);
            engine.SendEvent(_events[2]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[2]);
    
            // 2nd S1 event
            SendTimer(startTime + 15000);
            engine.SendEvent(_events[11]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[11]);
    
            // 2nd S2 event
            SendTimer(startTime + 18000);
            engine.SendEvent(_events[12]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[12]);
    
            // 3rd S1 event
            SendTimer(startTime + 21000);
            engine.SendEvent(_events[21]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[21]);
    
            SendTimer(startTime + 28000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(2, _listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[2], _events[12]}, _listener.GetOldDataListFlattened());
            _listener.Reset();
    
            // 3rd S2 event
            SendTimer(startTime + 29000);
            engine.SendEvent(_events[32]);
            Assert.AreSame(_listener.AssertOneGetNewAndReset().Underlying, _events[32]);
    
            SendTimer(startTime + 31000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            Assert.AreEqual(3, _listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[1], _events[11], _events[21]}, _listener.GetOldDataListFlattened());
            _listener.Reset();
    
            SendTimer(startTime + 39000);
            Assert.IsNull(_listener.LastNewData);
            Assert.AreEqual(1, _listener.LastOldData.Length);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[]{_events[32]}, _listener.GetOldDataListFlattened());
            _listener.Reset();
        }
    
        private void SendTimer(long timeInMSec) {
            CurrentTimeEvent theEvent = new CurrentTimeEvent(timeInMSec);
            EPRuntime runtime = _epService.EPRuntime;
            runtime.SendEvent(theEvent);
        }

        private void AssertData(EventBean theEvent, double price, double? prevPrice, double? priorPrice)
        {
            Assert.AreEqual(price, theEvent.Get("Price"));
            Assert.AreEqual(prevPrice, theEvent.Get("prevPrice"));
            Assert.AreEqual(priorPrice, theEvent.Get("priorPrice"));
        }

        private void AssertData(EventBean theEvent, double? sumPrice)
        {
            Assert.AreEqual(sumPrice, theEvent.Get("sumPrice"));
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

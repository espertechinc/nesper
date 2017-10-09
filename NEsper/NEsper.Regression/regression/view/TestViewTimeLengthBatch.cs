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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestViewTimeLengthBatch
    {
        private EPServiceProvider _epService;
        private SupportUpdateListener _listener;
        private SupportMarketDataBean[] _events;

        [SetUp]
        public void SetUp()
        {
            _listener = new SupportUpdateListener();
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _epService = EPServiceProviderManager.GetDefaultProvider(configuration);
            _epService.Initialize();
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(_epService, GetType(), GetType().FullName); }

            _events = new SupportMarketDataBean[100];
            for (int i = 0; i < _events.Length; i++)
            {
                _events[i] = new SupportMarketDataBean("S" + Convert.ToString(i), "id_" + Convert.ToString(i), i);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest(); }
            _listener = null;
            _events = null;
        }

        [Test]
        public void TestTimeLengthBatch()
        {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;

            // Send 3 events in batch
            engine.SendEvent(_events[0]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[1]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[2]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[0], _events[1], _events[2] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send another 3 events in batch
            engine.SendEvent(_events[3]);
            engine.SendEvent(_events[4]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[5]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[0], _events[1], _events[2] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[3], _events[4], _events[5] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Expire the last 3 events by moving time
            SendTimer(startTime + 9999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 10000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[3], _events[4], _events[5] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 10001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send an event, let the timer send the batch
            SendTimer(startTime + 10100);
            engine.SendEvent(_events[6]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 19999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 20000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[6] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 20001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send two events, let the timer send the batch
            SendTimer(startTime + 29998);
            engine.SendEvent(_events[7]);
            engine.SendEvent(_events[8]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 29999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 30000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[6] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[7], _events[8] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send three events, the the 3 events batch
            SendTimer(startTime + 30001);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[9]);
            engine.SendEvent(_events[10]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[11]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[7], _events[8] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[9], _events[10], _events[11] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send 1 event, let the timer to do the batch
            SendTimer(startTime + 39000 + 9999);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[12]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 10000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[9], _events[10], _events[11] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[12] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 39000 + 10001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send no events, let the timer to do the batch
            SendTimer(startTime + 39000 + 19999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 20000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[12] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 39000 + 20001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send no events, let the timer to do NO batch
            SendTimer(startTime + 39000 + 29999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 30000);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 30001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send 1 more event
            SendTimer(startTime + 90000);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[13]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 99999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 100000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[13] }, _listener.GetNewDataListFlattened());
            _listener.Reset();
        }

        [Test]
        public void TestTimeLengthBatchForceOutput()
        {
            long startTime = 1000;
            SendTimer(startTime);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select irstream * from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'FORCE_UPDATE')");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;

            // Send 3 events in batch
            engine.SendEvent(_events[0]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[1]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[2]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[0], _events[1], _events[2] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send another 3 events in batch
            engine.SendEvent(_events[3]);
            engine.SendEvent(_events[4]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[5]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[0], _events[1], _events[2] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[3], _events[4], _events[5] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Expire the last 3 events by moving time
            SendTimer(startTime + 9999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 10000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[3], _events[4], _events[5] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 10001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send an event, let the timer send the batch
            SendTimer(startTime + 10100);
            engine.SendEvent(_events[6]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 19999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 20000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[6] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 20001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send two events, let the timer send the batch
            SendTimer(startTime + 29998);
            engine.SendEvent(_events[7]);
            engine.SendEvent(_events[8]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 29999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 30000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[6] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[7], _events[8] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send three events, the the 3 events batch
            SendTimer(startTime + 30001);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[9]);
            engine.SendEvent(_events[10]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[11]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[7], _events[8] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[9], _events[10], _events[11] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send 1 event, let the timer to do the batch
            SendTimer(startTime + 39000 + 9999);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[12]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 10000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[9], _events[10], _events[11] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[12] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 39000 + 10001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send no events, let the timer to do the batch
            SendTimer(startTime + 39000 + 19999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 20000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[12] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 39000 + 20001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send no events, let the timer do a batch
            SendTimer(startTime + 39000 + 29999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 30000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            Assert.AreEqual(1, _listener.OldDataList.Count);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 39000 + 30001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send no events, let the timer do a batch
            SendTimer(startTime + 39000 + 39999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 39000 + 40000);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            SendTimer(startTime + 39000 + 40001);
            Assert.IsFalse(_listener.IsInvoked);

            // Send 1 more event
            SendTimer(startTime + 80000);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[13]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 88999);   // 10 sec from last batch
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 89000);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[13] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send 3 more events
            SendTimer(startTime + 90000);
            engine.SendEvent(_events[14]);
            engine.SendEvent(_events[15]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 92000);
            engine.SendEvent(_events[16]);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[13] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[14], _events[15], _events[16] }, _listener.GetNewDataListFlattened());
            _listener.Reset();

            // Send no events, let the timer do a batch
            SendTimer(startTime + 101999);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 102000);
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { _events[14], _events[15], _events[16] }, _listener.GetOldDataListFlattened());
            EPAssertionUtil.AssertEqualsExactOrderUnderlying(new Object[] { }, _listener.GetNewDataListFlattened());
            _listener.Reset();
        }

        [Test]
        public void TestTimeLengthBatchForceOutputSum()
        {
            long startTime = 1000;
            SendTimer(startTime);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select sum(Price) from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'FORCE_UPDATE')");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;

            // Send 1 events in batch
            engine.SendEvent(_events[10]);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 10000);
            Assert.AreEqual(10.0, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();

            SendTimer(startTime + 20000);
            Assert.AreEqual(null, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();

            SendTimer(startTime + 30000);
            Assert.AreEqual(null, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();

            SendTimer(startTime + 40000);
            Assert.AreEqual(null, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();
        }

        [Test]
        public void TestForceOutputStartEagerSum()
        {
            long startTime = 1000;
            SendTimer(startTime);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select sum(Price) from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'force_update, start_eager')");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 9999);
            Assert.IsFalse(_listener.IsInvoked);

            // Send batch off
            SendTimer(startTime + 10000);
            Assert.AreEqual(null, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();

            // Send batch off
            SendTimer(startTime + 20000);
            Assert.AreEqual(null, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();

            engine.SendEvent(_events[11]);
            engine.SendEvent(_events[12]);
            SendTimer(startTime + 30000);
            Assert.AreEqual(23.0, _listener.LastNewData[0].Get("sum(Price)"));
            _listener.Reset();
        }

        [Test]
        public void TestForceOutputStartNoEagerSum()
        {
            long startTime = 1000;
            SendTimer(startTime);

            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select sum(Price) from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3, 'force_update')");
            stmt.Events += _listener.Update;

            // No batch as we are not start eager
            SendTimer(startTime + 10000);
            Assert.IsFalse(_listener.IsInvoked);

            // No batch as we are not start eager
            SendTimer(startTime + 20000);
            Assert.IsFalse(_listener.IsInvoked);
        }

        [Test]
        public void TestPreviousAndPrior()
        {
            long startTime = 1000;
            SendTimer(startTime);
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select Price, prev(1, Price) as prevPrice, prior(1, Price) as priorPrice from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(10 sec, 3)");
            stmt.Events += _listener.Update;
            EPRuntime engine = _epService.EPRuntime;

            // Send 3 events in batch
            engine.SendEvent(_events[0]);
            engine.SendEvent(_events[1]);
            Assert.IsFalse(_listener.IsInvoked);

            engine.SendEvent(_events[2]);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            EventBean[] events = _listener.LastNewData;
            AssertData(events[0], 0, null, null);
            AssertData(events[1], 1.0, 0.0, 0.0);
            AssertData(events[2], 2.0, 1.0, 1.0);
            _listener.Reset();
        }

        [Test]
        public void TestGroupBySumStartEager()
        {
            long startTime = 1000;
            SendTimer(startTime);

            EPRuntime engine = _epService.EPRuntime;
            EPStatement stmt = _epService.EPAdministrator.CreateEPL(
                    "select Symbol, sum(Price) as s from " + typeof(SupportMarketDataBean).FullName +
                            "#time_length_batch(5, 10, \"START_EAGER\") group by Symbol order by Symbol asc");
            stmt.Events += _listener.Update;

            SendTimer(startTime + 4000);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 6000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            EventBean[] events = _listener.LastNewData;
            Assert.IsNull(events);
            _listener.Reset();

            SendTimer(startTime + 7000);
            engine.SendEvent(new SupportMarketDataBean("S1", "e1", 10d));

            SendTimer(startTime + 8000);
            engine.SendEvent(new SupportMarketDataBean("S2", "e2", 77d));

            SendTimer(startTime + 9000);
            engine.SendEvent(new SupportMarketDataBean("S1", "e3", 1d));

            SendTimer(startTime + 10000);
            Assert.IsFalse(_listener.IsInvoked);

            SendTimer(startTime + 11000);
            Assert.AreEqual(1, _listener.NewDataList.Count);
            events = _listener.LastNewData;
            Assert.AreEqual(2, events.Length);
            Assert.AreEqual("S1", events[0].Get("Symbol"));
            Assert.AreEqual(11d, events[0].Get("s"));
            Assert.AreEqual("S2", events[1].Get("Symbol"));
            Assert.AreEqual(77d, events[1].Get("s"));
            _listener.Reset();
        }

        private void SendTimer(long timeInMSec)
        {
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
    }
}

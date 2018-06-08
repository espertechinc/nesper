///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.core.support;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.util;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestTimeBatchView
    {
        private const long TEST_INTERVAL_MSEC = 10000;

        private TimeBatchView _myView;
        private SupportBeanClassView _childView;
        private SupportSchedulingServiceImpl _schedulingServiceStub;
        private IContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

            // Set the scheduling service to use
            _schedulingServiceStub = new SupportSchedulingServiceImpl();

            // Set up length window view and a test child view
            _myView = new TimeBatchView(
                new TimeBatchViewFactory(),
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container, _schedulingServiceStub),
                new ExprTimePeriodEvalDeltaConstGivenDelta(TEST_INTERVAL_MSEC), null, false, false, null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }

        [Test]
        public void TestViewPushNoRefPoint()
        {
            long startTime = 1000000;
            _schedulingServiceStub.Time = startTime;

            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);

            IDictionary<String, EventBean> events = EventFactoryHelper.MakeEventMap(
                    new String[] { "a1", "b1", "b2", "c1", "d1" });

            // Send new events to the view - should have scheduled a callback for X msec after
            _myView.Update(new EventBean[] { events.Get("a1") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC) != null);
            _schedulingServiceStub.Added.Clear();
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("a1") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);  // Data got batched, no data release till later

            _schedulingServiceStub.Time = (startTime + 5000);
            _myView.Update(new EventBean[] { events.Get("b1"), events.Get("b2") }, null);
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("a1"), events.Get("b1"), events.Get("b2") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);

            // Pretend we have a callback, check data, check scheduled new callback
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("a1"), events.Get("b1"), events.Get("b2") });
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC) != null);
            _schedulingServiceStub.Added.Clear();

            // Pretend callback received again, should schedule a callback since the last interval showed data
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 2);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("a1"), events.Get("b1"), events.Get("b2") });  // Old data is published
            SupportViewDataChecker.CheckNewData(_childView, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC) != null);
            _schedulingServiceStub.Added.Clear();

            // Pretend callback received again, not schedule a callback since the this and last interval showed no data
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 3);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);

            // Send new event to the view - pretend we are 500 msec into the interval
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 3 + 500);
            _myView.Update(new EventBean[] { events.Get("c1") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC - 500) != null);
            _schedulingServiceStub.Added.Clear();
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("c1") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);  // Data got batched, no data release till later

            // Pretend callback received again
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 4);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("c1") });
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC) != null);
            _schedulingServiceStub.Added.Clear();

            // Send new event to the view
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 4 + 500);
            _myView.Update(new EventBean[] { events.Get("d1") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("d1") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);

            // Pretend callback again
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 5);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("c1") });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("d1") });
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC) != null);
            _schedulingServiceStub.Added.Clear();

            // Pretend callback again
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 6);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("d1") });
            SupportViewDataChecker.CheckNewData(_childView, null);

            // Pretend callback again
            _schedulingServiceStub.Time = (startTime + TEST_INTERVAL_MSEC * 7);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);
        }

        [Test]
        public void TestViewPushWithRefPoint()
        {
            long startTime = 50000;
            _schedulingServiceStub.Time = startTime;

            _myView = new TimeBatchView(
                new TimeBatchViewFactory(),
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container, _schedulingServiceStub),
                new ExprTimePeriodEvalDeltaConstGivenDelta(TEST_INTERVAL_MSEC), 1505L, false, false, null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);

            IDictionary<String, EventBean> events = EventFactoryHelper.MakeEventMap(
                    new String[] { "A1", "A2", "A3" });

            // Send new events to the view - should have scheduled a callback for X msec after
            _myView.Update(new EventBean[] { events.Get("A1"), events.Get("A2"), events.Get("A3") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(1505L) != null);
            _schedulingServiceStub.Added.Clear();
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("A1"), events.Get("A2"), events.Get("A3") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);  // Data got batched, no data release till later

            // Pretend we have a callback, check data, check scheduled new callback
            _schedulingServiceStub.Time = (startTime + 1505);
            _myView.SendBatch();
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("A1"), events.Get("A2"), events.Get("A3") });
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_INTERVAL_MSEC) != null);
        }
    }
}

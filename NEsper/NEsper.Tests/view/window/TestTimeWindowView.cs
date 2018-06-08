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
    public class TestTimeWindowView
    {
        private const long TEST_WINDOW_MSEC = 60000;

        private TimeWindowView _myView;
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
            _myView = new TimeWindowView(
                SupportStatementContextFactory.MakeAgentInstanceViewFactoryContext(_container, _schedulingServiceStub),
                new TimeWindowViewFactory(),
                new ExprTimePeriodEvalDeltaConstGivenDelta(TEST_WINDOW_MSEC), null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }

        [Test]
        public void TestViewPushAndExpire()
        {
            long startTime = 1000000;
            _schedulingServiceStub.Time = startTime;
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);

            IDictionary<String, EventBean> events = EventFactoryHelper.MakeEventMap(
                    new String[] { "a1", "b1", "b2", "c1", "d1", "e1", "f1", "f2" });

            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);

            // Send new events to the view - should have scheduled a callback for X msec after
            _myView.Update(new EventBean[] { events.Get("a1") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(TEST_WINDOW_MSEC) != null);
            _schedulingServiceStub.Added.Clear();

            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("a1") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("a1") });

            // Send more events, check
            _schedulingServiceStub.Time = (startTime + 10000);
            _myView.Update(new EventBean[] { events.Get("b1"), events.Get("b2") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);

            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("a1"), events.Get("b1"), events.Get("b2") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("b1"), events.Get("b2") });

            // Send more events, check
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC - 1);
            _myView.Update(new EventBean[] { events.Get("c1") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);

            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("a1"), events.Get("b1"), events.Get("b2"), events.Get("c1") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("c1") });

            // Pretend we are getting the callback from scheduling, check old data and check new scheduling
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC);
            _myView.Expire();
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("b1"), events.Get("b2"), events.Get("c1") }, _myView.GetEnumerator());
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("a1") });
            SupportViewDataChecker.CheckNewData(_childView, null);

            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(10000L) != null);
            _schedulingServiceStub.Added.Clear();

            // Send another 2 events
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC);
            _myView.Update(new EventBean[] { events.Get("d1") }, null);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("d1") });

            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC + 1);
            _myView.Update(new EventBean[] { events.Get("e1") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("e1") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("b1"), events.Get("b2"), events.Get("c1"), events.Get("d1"), events.Get("e1") }, _myView.GetEnumerator());

            // Pretend callback received
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC + 10000);
            _myView.Expire();
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("b1"), events.Get("b2") });
            SupportViewDataChecker.CheckNewData(_childView, null);
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("c1"), events.Get("d1"), events.Get("e1") }, _myView.GetEnumerator());

            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(49999L) != null);
            _schedulingServiceStub.Added.Clear();

            // Pretend callback received
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC + 59999);
            _myView.Expire();
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("c1") });
            SupportViewDataChecker.CheckNewData(_childView, null);
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("d1"), events.Get("e1") }, _myView.GetEnumerator());

            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(1L) != null);
            _schedulingServiceStub.Added.Clear();

            // Send another event
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC + 200);
            _myView.Update(new EventBean[] { events.Get("f1"), events.Get("f2") }, null);
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("f1"), events.Get("f2") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("d1"), events.Get("e1"), events.Get("f1"), events.Get("f2") }, _myView.GetEnumerator());

            // Pretend callback received, we didn't schedule for 1 msec after, but for 100 msec after
            // testing what happens when clock resolution or some other delay happens
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC + 60099);
            _myView.Expire();
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("d1"), events.Get("e1") });
            SupportViewDataChecker.CheckNewData(_childView, null);
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("f1"), events.Get("f2") }, _myView.GetEnumerator());

            Assert.IsTrue(_schedulingServiceStub.Added.Count == 1);
            Assert.IsTrue(_schedulingServiceStub.Added.Get(101L) != null);
            _schedulingServiceStub.Added.Clear();

            // Pretend callback received
            _schedulingServiceStub.Time = (startTime + TEST_WINDOW_MSEC + 60201);
            _myView.Expire();
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("f1"), events.Get("f2") });
            SupportViewDataChecker.CheckNewData(_childView, null);
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());
            Assert.IsTrue(_schedulingServiceStub.Added.Count == 0);
        }

        public EventBean[] MakeEvents(String[] ids)
        {
            return EventFactoryHelper.MakeEvents(ids);
        }
    }
}

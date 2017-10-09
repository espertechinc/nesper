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
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.std
{
    [TestFixture]
    public class TestLastElementView
    {
        private LastElementView _myView;
        private SupportBeanClassView _childView;

        [SetUp]
        public void SetUp()
        {
            // Set up length window view and a test child view
            _myView = new LastElementView(null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }

        [Test]
        public void TestViewPush()
        {
            // Set up a feed for the view under test - it will have a depth of 3 trades
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportBean_A), 3);
            stream.AddView(_myView);

            IDictionary<String, EventBean> events = EventFactoryHelper.MakeEventMap(
                    new String[] { "a0", "a1", "b0", "c0", "c1", "c2", "d0", "e0" });

            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, null);
            EPAssertionUtil.AssertEqualsExactOrder(null, _myView.GetEnumerator());

            // View should keep the last element for iteration, should report new data as it arrives
            stream.Insert(new EventBean[] { events.Get("a0"), events.Get("a1") });
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("a0") });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("a0"), events.Get("a1") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("a1") }, _myView.GetEnumerator());

            stream.Insert(new EventBean[] { events.Get("b0") });
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("a1") });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("b0") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("b0") }, _myView.GetEnumerator());

            stream.Insert(new EventBean[] { events.Get("c0"), events.Get("c1"), events.Get("c2") });
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("b0"), events.Get("c0"), events.Get("c1") });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("c0"), events.Get("c1"), events.Get("c2") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("c2") }, _myView.GetEnumerator());

            stream.Insert(new EventBean[] { events.Get("d0") });
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("c2") });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("d0") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("d0") }, _myView.GetEnumerator());

            stream.Insert(new EventBean[] { events.Get("e0") });
            SupportViewDataChecker.CheckOldData(_childView, new EventBean[] { events.Get("d0") });
            SupportViewDataChecker.CheckNewData(_childView, new EventBean[] { events.Get("e0") });
            EPAssertionUtil.AssertEqualsExactOrder(new EventBean[] { events.Get("e0") }, _myView.GetEnumerator());
        }
    }
}

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
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.supportunit.view;

using NUnit.Framework;

namespace com.espertech.esper.view.window
{
    [TestFixture]
    public class TestLengthWindowView
    {
        private LengthWindowView _myView;
        private SupportBeanClassView _childView;
    
        [SetUp]
        public void SetUp() {
            // Set up length window view and a test child view
            _myView = new LengthWindowView(null, new LengthWindowViewFactory(), 5, null);
            _childView = new SupportBeanClassView(typeof(SupportMarketDataBean));
            _myView.AddView(_childView);
        }
    
        [Test]
        public void TestIncorrectUse() {
            try {
                _myView = new LengthWindowView(null, null, 0, null);
            } catch (ArgumentException) {
                // Expected exception
            }
        }
    
        // Check values against Microsoft Excel computed values
        [Test]
        public void TestViewPush() {
            // Set up a feed for the view under test - it will have a depth of 3 trades
            SupportStreamImpl stream = new SupportStreamImpl(typeof(SupportBean_A), 3);
            stream.AddView(_myView);
    
            IDictionary<String, EventBean> events = EventFactoryHelper.MakeEventMap(
                    new String[]{"a0", "b0", "b1", "c0", "c1", "d0", "e0", "e1", "e2", "f0", "f1",
                            "g0", "g1", "g2", "g3", "g4",
                            "h0", "h1", "h2", "h3", "h4", "h5", "h6",
                            "i0"
                    });
    
            // Fill the window with events up to the depth of 5
            stream.Insert(MakeArray(events, new String[]{"a0"}));
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"a0"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"a0"}), _myView.GetEnumerator());
    
            stream.Insert(MakeArray(events, new String[]{"b0", "b1"}));
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"b0", "b1"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"a0", "b0", "b1"}), _myView.GetEnumerator());
    
            stream.Insert(MakeArray(events, new String[]{"c0", "c1"}));
    
            SupportViewDataChecker.CheckOldData(_childView, null);
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"c0", "c1"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"a0", "b0", "b1", "c0", "c1"}), _myView.GetEnumerator());
    
            // Send further events, expect to get events back that fall out of the window (a0)
            stream.Insert(MakeArray(events, new String[]{"d0"}));
            SupportViewDataChecker.CheckOldData(_childView, MakeArray(events, new String[]{"a0"}));
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"d0"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"b0", "b1", "c0", "c1", "d0"}), _myView.GetEnumerator());
    
            stream.Insert(MakeArray(events, new String[]{"e0", "e1", "e2"}));
            SupportViewDataChecker.CheckOldData(_childView, MakeArray(events, new String[]{"b0", "b1", "c0"}));
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"e0", "e1", "e2"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"c1", "d0", "e0", "e1", "e2"}), _myView.GetEnumerator());
    
            stream.Insert(MakeArray(events, new String[]{"f0", "f1"}));
            SupportViewDataChecker.CheckOldData(_childView, MakeArray(events, new String[]{"c1", "d0"}));
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"f0", "f1"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"e0", "e1", "e2", "f0", "f1"}), _myView.GetEnumerator());
    
            // Push as many events as the window takes
            stream.Insert(MakeArray(events, new String[]{"g0", "g1", "g2", "g3", "g4"}));
            SupportViewDataChecker.CheckOldData(_childView, MakeArray(events, new String[]{"e0", "e1", "e2", "f0", "f1"}));
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"g0", "g1", "g2", "g3", "g4"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"g0", "g1", "g2", "g3", "g4"}), _myView.GetEnumerator());
    
            // Push 2 more events then the window takes
            stream.Insert(MakeArray(events, new String[]{"h0", "h1", "h2", "h3", "h4", "h5", "h6"}));
            SupportViewDataChecker.CheckOldData(_childView, MakeArray(events, new String[]{"g0", "g1", "g2", "g3", "g4", "h0", "h1"}));
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"h0", "h1", "h2", "h3", "h4", "h5", "h6"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"h2", "h3", "h4", "h5", "h6"}), _myView.GetEnumerator());
    
            // Push 1 last event to make sure the last overflow was handled correctly
            stream.Insert(MakeArray(events, new String[]{"i0"}));
            SupportViewDataChecker.CheckOldData(_childView, MakeArray(events, new String[]{"h2"}));
            SupportViewDataChecker.CheckNewData(_childView, MakeArray(events, new String[]{"i0"}));
            EPAssertionUtil.AssertEqualsExactOrder(MakeArray(events, new String[]{"h3", "h4", "h5", "h6", "i0"}), _myView.GetEnumerator());
        }
    
        private static EventBean[] MakeArray(IDictionary<String, EventBean> events, String[] ids) {
            return EventFactoryHelper.MakeArray(events, ids);
        }
    }
}

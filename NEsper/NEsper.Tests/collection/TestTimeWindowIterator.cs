///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
	public class TestTimeWindowIterator
    {
	    private IDictionary<string, EventBean> _events;

        [SetUp]
	    public void SetUp() {
	        _events = EventFactoryHelper.MakeEventMap(new string[]{"a", "b", "c", "d", "e", "f", "g"});
	    }

        [Test]
	    public void TestEmpty() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();
            IEnumerator<EventBean> it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(null, it);
	    }

        [Test]
	    public void TestOneElement() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();
	        var list = new ArrayDeque<EventBean>();
	        list.Add(_events.Get("a"));
	        AddToWindow(testWindow, 10L, list);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{_events.Get("a")}, it);
	    }

        [Test]
	    public void TestTwoInOneEntryElement() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();
	        var list = new ArrayDeque<EventBean>();
	        list.Add(_events.Get("a"));
	        list.Add(_events.Get("b"));
	        AddToWindow(testWindow, 10L, list);

            IEnumerator<EventBean> it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new EventBean[]{_events.Get("a"), _events.Get("b")}, it);
	    }

        [Test]
	    public void TestTwoSeparateEntryElement() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();
	        var list2 = new ArrayDeque<EventBean>();
	        list2.Add(_events.Get("b"));
	        AddToWindow(testWindow, 5L, list2); // Actually before list1
	        var list1 = new ArrayDeque<EventBean>();
	        list1.Add(_events.Get("a"));
	        AddToWindow(testWindow, 10L, list1);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{_events.Get("b"), _events.Get("a")}, it);
	    }

        [Test]
	    public void TestTwoByTwoEntryElement() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();
	        var list1 = new ArrayDeque<EventBean>();
	        list1.Add(_events.Get("a"));
	        list1.Add(_events.Get("b"));
	        AddToWindow(testWindow, 10L, list1);
	        var list2 = new ArrayDeque<EventBean>();
	        list2.Add(_events.Get("c"));
	        list2.Add(_events.Get("d"));
	        AddToWindow(testWindow, 15L, list2);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{_events.Get("a"), _events.Get("b"), _events.Get("c"), _events.Get("d")}, it);
	    }

        [Test]
	    public void TestMixedEntryElement() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();
	        var list1 = new ArrayDeque<EventBean>();
	        list1.Add(_events.Get("a"));
	        AddToWindow(testWindow, 10L, list1);
	        var list2 = new ArrayDeque<EventBean>();
	        list2.Add(_events.Get("c"));
	        list2.Add(_events.Get("d"));
	        AddToWindow(testWindow, 15L, list2);
	        var list3 = new ArrayDeque<EventBean>();
	        list3.Add(_events.Get("e"));
	        list3.Add(_events.Get("f"));
	        list3.Add(_events.Get("g"));
	        AddToWindow(testWindow, 20L, list3);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{_events.Get("a"), _events.Get("c"), _events.Get("d"),
	                _events.Get("e"), _events.Get("f"), _events.Get("g")
	        }, it);
	    }

        [Test]
	    public void TestEmptyList() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 10L, list1);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder((object[]) null, it);
	    }

        [Test]
	    public void TestTwoEmptyList() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 10L, list1);
	        var list2 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 20L, list2);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder((object[]) null, it);
	    }

        [Test]
	    public void TestThreeEmptyList() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 10L, list1);
	        var list2 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 20L, list2);
	        var list3 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 30L, list3);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder((object[]) null, it);
	    }

        [Test]
	    public void TestEmptyListFrontTail() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 10L, list1);

	        var list2 = new ArrayDeque<EventBean>();
	        list2.Add(_events.Get("c"));
	        list2.Add(_events.Get("d"));
	        AddToWindow(testWindow, 15L, list2);

	        var list3 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 20L, list3);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{_events.Get("c"), _events.Get("d")}, it);
	    }

        [Test]
	    public void TestEmptyListSprinkle() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        list1.Add(_events.Get("a"));
	        AddToWindow(testWindow, 10L, list1);

	        var list2 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 15L, list2);

	        var list3 = new ArrayDeque<EventBean>();
	        list3.Add(_events.Get("c"));
	        list3.Add(_events.Get("d"));
	        AddToWindow(testWindow, 20L, list3);

	        var list4 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 40L, list4);

            var it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(new object[]{_events.Get("a"), _events.Get("c"), _events.Get("d")}, it);
	    }

        [Test]
	    public void TestEmptyListFront() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        AddToWindow(testWindow, 10L, list1);

	        var list2 = new ArrayDeque<EventBean>();
	        list2.Add(_events.Get("a"));
	        AddToWindow(testWindow, 15L, list2);

	        var list3 = new ArrayDeque<EventBean>();
	        list3.Add(_events.Get("c"));
	        list3.Add(_events.Get("d"));
	        AddToWindow(testWindow, 20L, list3);

	        var list4 = new ArrayDeque<EventBean>();
	        list4.Add(_events.Get("e"));
	        AddToWindow(testWindow, 40L, list4);

            IEnumerator<EventBean> it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]{_events.Get("a"), _events.Get("c"), _events.Get("d"), _events.Get("e")}, it);
	    }

        [Test]
	    public void TestObjectAndNull() {
	        var testWindow = new ArrayDeque<TimeWindowPair>();

	        var list1 = new ArrayDeque<EventBean>();
	        list1.Add(_events.Get("c"));
	        list1.Add(_events.Get("d"));
	        AddToWindow(testWindow, 10L, list1);

	        AddToWindow(testWindow, 20L, _events.Get("a"));

	        AddToWindow(testWindow, 30L, null);

	        var list3 = new ArrayDeque<EventBean>();
	        list3.Add(_events.Get("e"));
	        AddToWindow(testWindow, 40L, list3);

            IEnumerator<EventBean> it = new TimeWindowEnumerator(testWindow);
	        EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]{_events.Get("c"), _events.Get("d"), _events.Get("a"), _events.Get("e")}, it);
	    }

	    private void AddToWindow(ArrayDeque<TimeWindowPair> testWindow,
	                             long key,
	                             object value) {
	        testWindow.Add(new TimeWindowPair(key, value));
	    }
	}
} // end of namespace

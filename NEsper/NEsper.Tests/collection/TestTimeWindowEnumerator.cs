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
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestTimeWindowEnumerator
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _events = EventFactoryHelper.MakeEventMap(new[] {"a", "b", "c", "d", "e", "f", "g"});
        }

        #endregion

        private IDictionary<String, EventBean> _events;

        private static void AddToWindow(LinkedList<TimeWindowPair> testWindow, long key, object value)
        {
            testWindow.AddLast(new TimeWindowPair(key, value));
        }

        [Test]
        public void TestEmpty()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            IEnumerator<EventBean> it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(null, it);
        }

        [Test]
        public void TestEmptyList()
        {
            var testWindow = new LinkedList<TimeWindowPair>();

            var list1 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 10L, list1);

            var it = new TimeWindowEnumerator(testWindow);

            EPAssertionUtil.AssertEqualsExactOrder((Object[])null, it);
        }

        [Test]
        public void TestEmptyListFront()
        {
            var testWindow = new LinkedList<TimeWindowPair>();

            var list1 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 10L, list1);

            var list2 = new LinkedList<EventBean>();
            list2.AddLast(_events.Get("a"));
            AddToWindow(testWindow, 15L, list2);

            var list3 = new LinkedList<EventBean>();
            list3.AddLast(_events.Get("c"));
            list3.AddLast(_events.Get("d"));
            AddToWindow(testWindow, 20L, list3);

            var list4 = new LinkedList<EventBean>();
            list4.AddLast(_events.Get("e"));
            AddToWindow(testWindow, 40L, list4);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[]
                {
                    _events.Get("a"), 
                    _events.Get("c"), 
                    _events.Get("d"),
                    _events.Get("e")
                }, it);
        }

        [Test]
        public void TestEmptyListFrontTail()
        {
            var testWindow = new LinkedList<TimeWindowPair>();

            var list1 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 10L, list1);

            var list2 = new LinkedList<EventBean>();
            list2.AddLast(_events.Get("c"));
            list2.AddLast(_events.Get("d"));
            AddToWindow(testWindow, 15L, list2);

            var list3 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 20L, list3);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[] {_events.Get("c"), _events.Get("d")}, it);
        }

        [Test]
        public void TestEmptyListSprinkle()
        {
            var testWindow = new LinkedList<TimeWindowPair>();

            var list1 = new LinkedList<EventBean>();
            list1.AddLast(_events.Get("a"));
            AddToWindow(testWindow, 10L, list1);

            var list2 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 15L, list2);

            var list3 = new LinkedList<EventBean>();
            list3.AddLast(_events.Get("c"));
            list3.AddLast(_events.Get("d"));
            AddToWindow(testWindow, 20L, list3);

            var list4 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 40L, list4);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[]
            {
                _events.Get("a"), _events.Get("c"), _events.Get("d")
            }, it);
        }

        [Test]
        public void TestMixedEntryElement()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            var list1 = new LinkedList<EventBean>();
            list1.AddLast(_events.Get("a"));
            AddToWindow(testWindow, 10L, list1);
            var list2 = new LinkedList<EventBean>();
            list2.AddLast(_events.Get("c"));
            list2.AddLast(_events.Get("d"));
            AddToWindow(testWindow, 15L, list2);
            var list3 = new LinkedList<EventBean>();
            list3.AddLast(_events.Get("e"));
            list3.AddLast(_events.Get("f"));
            list3.AddLast(_events.Get("g"));
            AddToWindow(testWindow, 20L, list3);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[]
                                              {
                                                  _events.Get("a"), _events.Get("c"), _events.Get("d"),
                                                  _events.Get("e"), _events.Get("f"), _events.Get("g")
                                              }, it);
        }

        [Test]
        public void TestOneElement()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            var list = new LinkedList<EventBean>();
            list.AddLast(_events.Get("a"));
            AddToWindow(testWindow, 10L, list);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[] {_events.Get("a")}, it);
        }

        [Test]
        public void TestThreeEmptyList()
        {
            var testWindow = new LinkedList<TimeWindowPair>();

            var list1 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 10L, list1);
            var list2 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 20L, list2);
            var list3 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 30L, list3);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder((Object[])null, it);
        }

        [Test]
        public void TestTwoByTwoEntryElement()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            var list1 = new LinkedList<EventBean>();
            list1.AddLast(_events.Get("a"));
            list1.AddLast(_events.Get("b"));
            AddToWindow(testWindow, 10L, list1);
            var list2 = new LinkedList<EventBean>();
            list2.AddLast(_events.Get("c"));
            list2.AddLast(_events.Get("d"));
            AddToWindow(testWindow, 15L, list2);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(
                new[]
                {
                    _events.Get("a"), _events.Get("b"), _events.Get("c"),
                    _events.Get("d")
                },
                it);
        }

        [Test]
        public void TestTwoEmptyList()
        {
            var testWindow = new LinkedList<TimeWindowPair>();

            var list1 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 10L, list1);
            var list2 = new LinkedList<EventBean>();
            AddToWindow(testWindow, 20L, list2);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder((Object[])null, it);
        }

        [Test]
        public void TestTwoInOneEntryElement()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            var list = new LinkedList<EventBean>();
            list.AddLast(_events.Get("a"));
            list.AddLast(_events.Get("b"));
            AddToWindow(testWindow, 10L, list);

            IEnumerator<EventBean> it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { _events.Get("a"), _events.Get("b") }, it);
        }

        [Test]
        public void TestTwoSeparateEntryElement()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            var list2 = new LinkedList<EventBean>();
            list2.AddLast(_events.Get("b"));
            AddToWindow(testWindow, 5L, list2); // Actually before list1
            var list1 = new LinkedList<EventBean>();
            list1.AddLast(_events.Get("a"));
            AddToWindow(testWindow, 10L, list1);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[] {_events.Get("b"), _events.Get("a")}, it);
        }

        [Test]
        public void TestObjectAndNull()
        {
            var testWindow = new LinkedList<TimeWindowPair>();
            var list1 = new LinkedList<EventBean>();
            list1.AddLast(_events.Get("c"));
            list1.AddLast(_events.Get("d"));
            AddToWindow(testWindow, 10L, list1);

            AddToWindow(testWindow, 20L, _events.Get("a"));

            AddToWindow(testWindow, 30L, null);

            var list3 = new LinkedList<EventBean>();
            list3.AddLast(_events.Get("e"));
            AddToWindow(testWindow, 40L, list3);

            var it = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(
                new Object[]
                {
                    _events.Get("c"), 
                    _events.Get("d"),
                    _events.Get("a"), 
                    _events.Get("e")
                }, it);
        }

    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestTimeWindowEnumerator : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            events = EventFactoryHelper.MakeEventMap(new[] { "a", "b", "c", "d", "e", "f", "g" }, supportEventTypeFactory);
        }

        private IDictionary<string, EventBean> events;

        private void AddToWindow(
            ArrayDeque<TimeWindowPair> testWindow,
            long key,
            object value)
        {
            testWindow.Add(new TimeWindowPair(key, value));
        }

        [Test]
        public void TestEmpty()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();
            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(null, enumerator);
        }

        [Test]
        public void TestEmptyList()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 10L, list1);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder((object[]) null, enumerator);
        }

        [Test]
        public void TestEmptyListFront()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 10L, list1);

            var list2 = new ArrayDeque<EventBean>();
            list2.Add(events.Get("a"));
            AddToWindow(testWindow, 15L, list2);

            var list3 = new ArrayDeque<EventBean>();
            list3.Add(events.Get("c"));
            list3.Add(events.Get("d"));
            AddToWindow(testWindow, 20L, list3);

            var list4 = new ArrayDeque<EventBean>();
            list4.Add(events.Get("e"));
            AddToWindow(testWindow, 40L, list4);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("a"), events.Get("c"), events.Get("d"), events.Get("e")
            }, enumerator);
        }

        [Test]
        public void TestEmptyListFrontTail()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 10L, list1);

            var list2 = new ArrayDeque<EventBean>();
            list2.Add(events.Get("c"));
            list2.Add(events.Get("d"));
            AddToWindow(testWindow, 15L, list2);

            var list3 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 20L, list3);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("c"), events.Get("d")
            }, enumerator);
        }

        [Test]
        public void TestEmptyListSprinkle()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            list1.Add(events.Get("a"));
            AddToWindow(testWindow, 10L, list1);

            var list2 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 15L, list2);

            var list3 = new ArrayDeque<EventBean>();
            list3.Add(events.Get("c"));
            list3.Add(events.Get("d"));
            AddToWindow(testWindow, 20L, list3);

            var list4 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 40L, list4);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("a"), events.Get("c"), events.Get("d")
            }, enumerator);
        }

        [Test]
        public void TestMixedEntryElement()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();
            var list1 = new ArrayDeque<EventBean>();
            list1.Add(events.Get("a"));
            AddToWindow(testWindow, 10L, list1);
            var list2 = new ArrayDeque<EventBean>();
            list2.Add(events.Get("c"));
            list2.Add(events.Get("d"));
            AddToWindow(testWindow, 15L, list2);
            var list3 = new ArrayDeque<EventBean>();
            list3.Add(events.Get("e"));
            list3.Add(events.Get("f"));
            list3.Add(events.Get("g"));
            AddToWindow(testWindow, 20L, list3);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(
                new object[] {
                    events.Get("a"), events.Get("c"), events.Get("d"),
                    events.Get("e"), events.Get("f"), events.Get("g")
                },
                enumerator);
        }

        [Test]
        public void TestObjectAndNull()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            list1.Add(events.Get("c"));
            list1.Add(events.Get("d"));
            AddToWindow(testWindow, 10L, list1);

            AddToWindow(testWindow, 20L, events.Get("a"));

            AddToWindow(testWindow, 30L, null);

            var list3 = new ArrayDeque<EventBean>();
            list3.Add(events.Get("e"));
            AddToWindow(testWindow, 40L, list3);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("c"), events.Get("d"), events.Get("a"), events.Get("e")
            }, enumerator);
        }

        [Test]
        public void TestOneElement()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();
            var list = new ArrayDeque<EventBean>();
            list.Add(events.Get("a"));
            AddToWindow(testWindow, 10L, list);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("a")
            }, enumerator);
        }

        [Test]
        public void TestThreeEmptyList()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 10L, list1);
            var list2 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 20L, list2);
            var list3 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 30L, list3);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder((object[]) null, enumerator);
        }

        [Test]
        public void TestTwoByTwoEntryElement()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();
            var list1 = new ArrayDeque<EventBean>();
            list1.Add(events.Get("a"));
            list1.Add(events.Get("b"));
            AddToWindow(testWindow, 10L, list1);
            var list2 = new ArrayDeque<EventBean>();
            list2.Add(events.Get("c"));
            list2.Add(events.Get("d"));
            AddToWindow(testWindow, 15L, list2);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("a"), events.Get("b"), events.Get("c"), events.Get("d")
            }, enumerator);
        }

        [Test]
        public void TestTwoEmptyList()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();

            var list1 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 10L, list1);
            var list2 = new ArrayDeque<EventBean>();
            AddToWindow(testWindow, 20L, list2);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder((object[]) null, enumerator);
        }

        [Test]
        public void TestTwoInOneEntryElement()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();
            var list = new ArrayDeque<EventBean>();
            list.Add(events.Get("a"));
            list.Add(events.Get("b"));
            AddToWindow(testWindow, 10L, list);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new[] {
                events.Get("a"), events.Get("b")
            }, enumerator);
        }

        [Test]
        public void TestTwoSeparateEntryElement()
        {
            var testWindow = new ArrayDeque<TimeWindowPair>();
            var list2 = new ArrayDeque<EventBean>();
            list2.Add(events.Get("b"));
            AddToWindow(testWindow, 5L, list2); // Actually before list1
            var list1 = new ArrayDeque<EventBean>();
            list1.Add(events.Get("a"));
            AddToWindow(testWindow, 10L, list1);

            IEnumerator<EventBean> enumerator = new TimeWindowEnumerator(testWindow);
            EPAssertionUtil.AssertEqualsExactOrder(new object[] {
                events.Get("b"), events.Get("a")
            }, enumerator);
        }
    }
} // end of namespace

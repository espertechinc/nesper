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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.view.sort
{
    [TestFixture]
    public class TestSortWindowIterator : AbstractCommonTest
    {
        private IDictionary<string, EventBean> events;
        private SortedDictionary<object, object> testMap;
        private IComparer<object> comparator;

        [SetUp]
        public void SetUp()
        {
            events = EventFactoryHelper.MakeEventMap(new[] { "a", "b", "c", "d", "f", "g" }, supportEventTypeFactory);
            comparator = new ComparatorHashableMultiKeyCasting(new ComparatorHashableMultiKey(new[] { false }));
            testMap = new SortedDictionary<object, object>(comparator);
        }

        [Test]
        public void TestEmpty()
        {
            IEnumerator<EventBean> enumerator = testMap.GetMultiLevelEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(null, enumerator);
        }

        [Test]
        public void TestMixedEntryElement()
        {
            var list1 = new List<EventBean>();
            list1.Add(events.Get("a"));
            var keyA = new HashableMultiKey(new object[] { "keyA" });
            testMap.Put(keyA, list1);
            var list2 = new List<EventBean>();
            list2.Add(events.Get("c"));
            list2.Add(events.Get("d"));
            var keyB = new HashableMultiKey(new object[] { "keyB" });
            testMap.Put(keyB, list2);
            var list3 = new List<EventBean>();
            list3.Add(events.Get("e"));
            list3.Add(events.Get("f"));
            list3.Add(events.Get("g"));
            var keyC = new HashableMultiKey(new object[] { "keyC" });
            testMap.Put(keyC, list3);

            IEnumerator<EventBean> enumerator = testMap.GetMultiLevelEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(
                new[] {
                    events.Get("a"), events.Get("c"), events.Get("d"),
                    events.Get("e"), events.Get("f"), events.Get("g")
                }, enumerator);
        }

        [Test]
        public void TestOneElement()
        {
            var list = new List<EventBean>();
            list.Add(events.Get("a"));
            var key = new HashableMultiKey(new object[] { "akey" });
            testMap.Put(key, list);

            IEnumerator<EventBean> enumerator = testMap.GetMultiLevelEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(new[] { events.Get("a") }, enumerator);
        }

        [Test]
        public void TestTwoByTwoEntryElement()
        {
            var list1 = new List<EventBean>();
            list1.Add(events.Get("a"));
            list1.Add(events.Get("b"));
            var keyB = new HashableMultiKey(new object[] { "keyB" });
            testMap.Put(keyB, list1);
            var list2 = new List<EventBean>();
            list2.Add(events.Get("c"));
            list2.Add(events.Get("d"));
            var keyC = new HashableMultiKey(new object[] { "keyC" });
            testMap.Put(keyC, list2);

            IEnumerator<EventBean> enumerator = testMap.GetMultiLevelEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(new[] { events.Get("a"), events.Get("b"), events.Get("c"), events.Get("d") }, enumerator);
        }

        [Test]
        public void TestTwoInOneEntryElement()
        {
            var list = new List<EventBean>();
            list.Add(events.Get("a"));
            list.Add(events.Get("b"));
            var key = new HashableMultiKey(new object[] { "keyA" });
            testMap.Put(key, list);

            IEnumerator<EventBean> enumerator = testMap.GetMultiLevelEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(new[] { events.Get("a"), events.Get("b") }, enumerator);
        }

        [Test]
        public void TestTwoSeparateEntryElement()
        {
            var list1 = new List<EventBean>();
            list1.Add(events.Get("a"));
            var keyB = new HashableMultiKey(new object[] { "keyB" });
            testMap.Put(keyB, list1);
            var list2 = new List<EventBean>();
            list2.Add(events.Get("b"));
            var keyA = new HashableMultiKey(new object[] { "keyA" });
            testMap.Put(keyA, list2); // Actually before list1

            IEnumerator<EventBean> enumerator = testMap.GetMultiLevelEnumerator();
            EPAssertionUtil.AssertEqualsExactOrder(new[] { events.Get("b"), events.Get("a") }, enumerator);
        }
    }
} // end of namespace

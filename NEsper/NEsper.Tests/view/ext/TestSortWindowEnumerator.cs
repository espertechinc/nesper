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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.util;

using NUnit.Framework;

namespace com.espertech.esper.view.ext
{
    [TestFixture]
    public class TestSortWindowEnumerator
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _events = EventFactoryHelper.MakeEventMap(new String[]
            {
                "a", "b", "c", "d", "f", "g"
            }
                );
            _comparator = new MultiKeyCastingComparator(
                new MultiKeyComparator(new bool[]
                {
                    false
                }
                    ));
            _testMap = new SortedDictionary<Object, Object>(_comparator);
        }

        #endregion

        private IDictionary<String, EventBean> _events;
        private SortedDictionary<Object, Object> _testMap;
        private IComparer<Object> _comparator;

        [Test]
        public void TestEmpty()
        {
            IEnumerator<EventBean> it = new SortWindowEnumerator(_testMap);
            EPAssertionUtil.AssertEqualsExactOrder(null, it);
        }

        [Test]
        public void TestMixedEntryElement()
        {
            var list1 = new List<EventBean>();

            list1.Add(_events.Get("a"));
            var keyA = new MultiKeyUntyped(new Object[] { "keyA" });

            _testMap.Put(keyA, list1);
            var list2 = new List<EventBean>();

            list2.Add(_events.Get("c"));
            list2.Add(_events.Get("d"));
            var keyB = new MultiKeyUntyped(
                new Object[]
                {
                    "keyB"
                }
                );

            _testMap.Put(keyB, list2);
            var list3 = new List<EventBean>();

            list3.Add(_events.Get("e"));
            list3.Add(_events.Get("f"));
            list3.Add(_events.Get("g"));
            var keyC = new MultiKeyUntyped(
                new Object[]
                {
                    "keyC"
                }
                );

            _testMap.Put(keyC, list3);

            IEnumerator<EventBean> it = new SortWindowEnumerator(_testMap);

            EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]
                {
                    _events.Get("a"), _events.Get("c"), _events.Get("d"), _events.Get("e"),
                    _events.Get("f"), _events.Get("g")
                }
                , it);
        }

        [Test]
        public void TestOneElement()
        {
            var list = new List<EventBean>();

            list.Add(_events.Get("a"));
            var key = new MultiKeyUntyped(
                new Object[]
                {
                    "akey"
                }
                );

            _testMap.Put(key, list);

            IEnumerator<EventBean> it = new SortWindowEnumerator(_testMap);

            EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]
                {
                    _events.Get("a")
                }
                , it);
        }

        [Test]
        public void TestTwoByTwoEntryElement()
        {
            var list1 = new List<EventBean>();

            list1.Add(_events.Get("a"));
            list1.Add(_events.Get("b"));
            var keyB = new MultiKeyUntyped(
                new Object[]
                {
                    "keyB"
                }
                );

            _testMap.Put(keyB, list1);
            var list2 = new List<EventBean>();

            list2.Add(_events.Get("c"));
            list2.Add(_events.Get("d"));
            var keyC = new MultiKeyUntyped(
                new Object[]
                {
                    "keyC"
                }
                );

            _testMap.Put(keyC, list2);

            IEnumerator<EventBean> it = new SortWindowEnumerator(_testMap);

            EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]
                {
                    _events.Get("a"), _events.Get("b"), _events.Get("c"), _events.Get("d")
                }
                , it);
        }

        [Test]
        public void TestTwoInOneEntryElement()
        {
            var list = new List<EventBean>();

            list.Add(_events.Get("a"));
            list.Add(_events.Get("b"));
            var key = new MultiKeyUntyped(
                new Object[]
                {
                    "keyA"
                }
                );

            _testMap.Put(key, list);

            IEnumerator<EventBean> it = new SortWindowEnumerator(_testMap);

            EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]
                {
                    _events.Get("a"), _events.Get("b")
                }
                , it);
        }

        [Test]
        public void TestTwoSeparateEntryElement()
        {
            var list1 = new List<EventBean>();

            list1.Add(_events.Get("a"));
            var keyB = new MultiKeyUntyped(
                new Object[]
                {
                    "keyB"
                }
                );

            _testMap.Put(keyB, list1);
            var list2 = new List<EventBean>();

            list2.Add(_events.Get("b"));
            var keyA = new MultiKeyUntyped(
                new Object[]
                {
                    "keyA"
                }
                );

            _testMap.Put(keyA, list2); // Actually before list1

            IEnumerator<EventBean> it = new SortWindowEnumerator(_testMap);

            EPAssertionUtil.AssertEqualsExactOrder(
                new EventBean[]
                {
                    _events.Get("b"), _events.Get("a")
                }
                , it);
        }
    }
}
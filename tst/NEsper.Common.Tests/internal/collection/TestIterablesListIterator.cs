///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestIterablesListEnumerator : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            events = EventFactoryHelper.MakeEventMap(
                new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "z" },
                supportEventTypeFactory);
        }

        private IDictionary<string, EventBean> events;

        private void CheckResults(
            ICollection<IEnumerable<EventBean>> iterables,
            EventBean[] expectedValues)
        {
            IEnumerable<EventBean> iterator = iterables.SelectMany(value => value);
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, iterator.GetEnumerator());
        }

        [Test]
        public void TestIterator()
        {
            ICollection<IEnumerable<EventBean>> iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "a", "b", "c" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "a", "b", "c" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "a" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "b" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "c" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "a", "b", "c" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "a", "b" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "c" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "a", "b", "c" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "a", "b" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "c" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "a", "b", "c" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, null);

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, null);

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "d" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "d" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "d" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "d" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "a", "b", "c" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "d" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "e", "f" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "g" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "h", "i" }));
            iterables.Add(EventFactoryHelper.MakeList(events, new[] { "z" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "z" }));

            iterables = new LinkedList<IEnumerable<EventBean>>();
            CheckResults(iterables, null);
        }
    }
} // end of namespace

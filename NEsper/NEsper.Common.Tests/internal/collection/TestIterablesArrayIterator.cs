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

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestIterablesArrayIterator : AbstractTestBase
    {
        private IDictionary<string, EventBean> events;

        [SetUp]
        public void SetUp()
        {
            events = EventFactoryHelper.MakeEventMap(
                new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "z" },
                supportEventTypeFactory);
        }

        [Test]
        public void TestIterator()
        {
            IEnumerable<EventBean>[][] iterables = new IEnumerable<EventBean>[1][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b", "c" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));

            iterables = new IEnumerable<EventBean>[3][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a" }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "b" }));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "c" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));

            iterables = new IEnumerable<EventBean>[2][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b" }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "c" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));

            iterables = new IEnumerable<EventBean>[5][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b" }));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "c" }));
            iterables[4] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));

            iterables = new IEnumerable<EventBean>[1][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, null);

            iterables = new IEnumerable<EventBean>[3][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, null);

            iterables = new IEnumerable<EventBean>[4][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "d" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "d" }));

            iterables = new IEnumerable<EventBean>[4][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "d" }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "d" }));

            iterables = new IEnumerable<EventBean>[8][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b", "c" }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "d" }));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "e", "f" }));
            iterables[4] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "g" }));
            iterables[5] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { }));
            iterables[6] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "h", "i" }));
            iterables[7] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "z" }));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "z" }));

            iterables = new IEnumerable<EventBean>[0][];
            CheckResults(iterables, null);
        }

        private void CheckResults(IEnumerable<EventBean>[][] iterables, EventBean[] expectedValues)
        {
            IterablesArrayEnumerator iterator = new IterablesArrayEnumerator(iterables);
            //IterablesArrayIterator iterator = new IterablesArrayIterator(iterables);
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, iterator);
        }

        private IEnumerable<EventBean>[] MakeArray(IList<EventBean> eventBeans)
        {
            return new IEnumerable<EventBean>[] { eventBeans };
        }
    }
} // end of namespace

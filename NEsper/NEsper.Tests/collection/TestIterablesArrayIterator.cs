///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestIterablesArrayIterator 
    {
        private IDictionary<String, EventBean> events;
    
        [SetUp]
        public void SetUp()
        {
            events = EventFactoryHelper.MakeEventMap(new string[] {"a", "b", "c", "d", "e", "f", "g", "h", "i", "z"});
        }
    
        [Test]
        public void TestIterator()
        {
            IEnumerable<EventBean>[][] iterables = new Iterable[1][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[]{"a", "b", "c"}));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));
    
            iterables = new Iterable[3][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a" } ));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "b" } ));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "c" } ));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));
    
            iterables = new Iterable[2][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b" }));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "c" } ));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));
    
            iterables = new Iterable[5][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b" } ));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "c" } ));
            iterables[4] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c" }));
    
            iterables = new Iterable[1][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            CheckResults(iterables, null);
    
            iterables = new Iterable[3][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            CheckResults(iterables, null);
    
            iterables = new Iterable[4][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "d" } ));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "d" } ));
    
            iterables = new Iterable[4][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "d" } ));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "d" } ));
    
            iterables = new Iterable[8][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b", "c" } ));
            iterables[1] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "d" } ));
            iterables[2] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[3] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "e", "f" } ));
            iterables[4] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "g" } ));
            iterables[5] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { } ));
            iterables[6] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "h", "i" } ));
            iterables[7] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "z" } ));
            CheckResults(iterables, EventFactoryHelper.MakeArray(events, new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "z" }));
    
            iterables = new Iterable[0][];
            CheckResults(iterables, null);
        }
    
        [Test]
        public void TestRemove()
        {
            IEnumerable<EventBean>[][] iterables = new IEnumerable<EventBean>[1][];
            iterables[0] = MakeArray(EventFactoryHelper.MakeList(events, new string[] { "a", "b", "c" } ));
            IterablesArrayIterator iterator = new IterablesArrayIterator(iterables);
    
            try
            {
                iterator.Remove();
                Assert.IsTrue(false);
            }
            catch (UnsupportedOperationException ex)
            {
                // Expected
            }
        }
    
        private void CheckResults(IEnumerable<EventBean>[][] iterables, EventBean[] expectedValues)
        {
            IterablesArrayIterator iterator = new IterablesArrayIterator(iterables);
            EPAssertionUtil.AssertEqualsExactOrder(expectedValues, iterator);
        }
    
        private IEnumerable<EventBean>[] MakeArray(IList<EventBean> eventBeans) {
            return (IEnumerable<EventBean>[])new IEnumerable<EventBean>[] { eventBeans };
        }
    }
}

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
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;
using com.espertech.esper.util;
using com.espertech.esper.view.window;

using NUnit.Framework;

namespace com.espertech.esper.view.ext
{
    [TestFixture]
    public class TestIStreamSortedRandomAccess
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            RandomAccessByIndexObserver updateObserver = new MyRandomAccessByIndexObserver();

            _access = new IStreamSortRankRandomAccessImpl(updateObserver);
            _sortedEvents = new OrderedDictionary<Object, Object>(
                new MultiKeyCastingComparator(
                    new MultiKeyComparator(new bool[]
                    {
                        false
                    })));

            _events = new EventBean[100];
            for (int i = 0; i < _events.Length; i++)
            {
                _events[i] = SupportEventBeanFactory.CreateObject(new SupportBean());
            }
        }

        #endregion

        private IStreamSortRankRandomAccess _access;
        private OrderedDictionary<Object, Object> _sortedEvents;
        private EventBean[] _events;

        public class MyRandomAccessByIndexObserver : RandomAccessByIndexObserver
        {
            #region RandomAccessByIndexObserver Members

            public void Updated(RandomAccessByIndex randomAccessByIndex)
            {
            }

            #endregion
        }

        private void AssertData(EventBean[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                Assert.AreSame(events[i], _access.GetNewData(i), "Failed for index " + i);
            }
            Assert.IsNull(_access.GetNewData(events.Length));
        }

        private void Add(String key, EventBean theEvent)
        {
            ((SupportBean) theEvent.Underlying).TheString = key;
            var mkey = new MultiKeyUntyped(new Object[]
            {
                key
            }
                );
            var eventList = (List<EventBean>) _sortedEvents.Get(mkey);

            if (eventList == null)
            {
                eventList = new List<EventBean>();
            }
            eventList.Insert(0, theEvent);
            _sortedEvents.Put(mkey, eventList);
        }

        [Test]
        public void TestGet()
        {
            _access.Refresh(_sortedEvents, 0, 10);
            Assert.IsNull(_access.GetNewData(0));
            Assert.IsNull(_access.GetNewData(1));

            Add("C", _events[0]);
            _access.Refresh(_sortedEvents, 1, 10);
            AssertData(new EventBean[]
            {
                _events[0]
            }
                );

            Add("E", _events[1]);
            _access.Refresh(_sortedEvents, 2, 10);
            AssertData(new EventBean[]
            {
                _events[0], _events[1]
            }
                );

            Add("A", _events[2]);
            _access.Refresh(_sortedEvents, 3, 10);
            AssertData(new EventBean[]
            {
                _events[2], _events[0], _events[1]
            }
                );

            Add("C", _events[4]);
            _access.Refresh(_sortedEvents, 4, 10);
            AssertData(new EventBean[]
            {
                _events[2], _events[4], _events[0], _events[1]
            }
                );

            Add("E", _events[5]);
            _access.Refresh(_sortedEvents, 5, 10);
            AssertData(new EventBean[]
            {
                _events[2], _events[4], _events[0], _events[5], _events[1]
            }
                );

            Add("A", _events[6]);
            _access.Refresh(_sortedEvents, 6, 10);
            AssertData(new EventBean[]
            {
                _events[6], _events[2], _events[4], _events[0], _events[5], _events[1]
            }
                );

            Add("B", _events[7]);
            _access.Refresh(_sortedEvents, 7, 10);
            AssertData(
                new EventBean[]
                {
                    _events[6], _events[2], _events[7], _events[4], _events[0], _events[5],
                    _events[1]
                }
                );

            Add("F", _events[8]);
            _access.Refresh(_sortedEvents, 8, 10);
            AssertData(
                new EventBean[]
                {
                    _events[6], _events[2], _events[7], _events[4], _events[0], _events[5],
                    _events[1], _events[8]
                }
                );
            // A           A           B       C           C           E           E           F

            Add("D", _events[9]);
            _access.Refresh(_sortedEvents, 9, 10);
            Assert.AreSame(_events[9], _access.GetNewData(5));
        }
    }
}
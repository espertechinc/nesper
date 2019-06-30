///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.@event.core
{
    [TestFixture]
    public class TestEventBeanUtility : CommonTest
    {
        [Test]
        public void TestArrayOp()
        {
            var testEvent = MakeEventArray(new[] { "a1", "a2", "a3" });

            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { testEvent[0] },
                    EventBeanUtility.AddToArray(new EventBean[0], testEvent[0]));

            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { testEvent[0], testEvent[1] },
                    EventBeanUtility.AddToArray(new[] { testEvent[0] }, testEvent[1]));

            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { testEvent[0], testEvent[1], testEvent[2] },
                    EventBeanUtility.AddToArray(new[] { testEvent[0], testEvent[1] }, testEvent[2]));

            Console.Out.WriteLine(EventBeanUtility.PrintEvents(testEvent));
        }

        [Test]
        public void TestArrayOpAdd()
        {
            var testEvent = MakeEventArray(new[] { "a1", "a2", "a3" });

            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { testEvent[0], testEvent[1], testEvent[2] },
                    EventBeanUtility.AddToArray(new[] { testEvent[0] }, Arrays.AsList(new[] { testEvent[1], testEvent[2] })));

            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { testEvent[1], testEvent[2] },
                    EventBeanUtility.AddToArray(new EventBean[] { }, Arrays.AsList(new[] { testEvent[1], testEvent[2] })));

            EPAssertionUtil.AssertEqualsAnyOrder(new object[] { testEvent[0] },
                    EventBeanUtility.AddToArray(new[] { testEvent[0] }, Arrays.AsList(new EventBean[0])));
        }

        [Test]
        public void TestFlattenList()
        {
            // test many arrays
            var testEvents = MakeEventArray(new[] { "a1", "a2", "b1", "b2", "b3", "c1", "c2" });
            var eventVector = new ArrayDeque<UniformPair<EventBean[]>>();

            eventVector.Add(new UniformPair<EventBean[]>(null, new[] { testEvents[0], testEvents[1] }));
            eventVector.Add(new UniformPair<EventBean[]>(new[] { testEvents[2] }, null));
            eventVector.Add(new UniformPair<EventBean[]>(null, new[] { testEvents[3], testEvents[4], testEvents[5] }));
            eventVector.Add(new UniformPair<EventBean[]>(new[] { testEvents[6] }, null));

            var events = EventBeanUtility.FlattenList(eventVector);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { testEvents[2], testEvents[6] }, events.First);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { testEvents[0], testEvents[1], testEvents[3], testEvents[4], testEvents[5] }, events.Second);

            // test just one array
            eventVector.Clear();
            eventVector.Add(new UniformPair<EventBean[]>(new[] { testEvents[2] }, null));
            events = EventBeanUtility.FlattenList(eventVector);
            EPAssertionUtil.AssertEqualsExactOrder(new[] { testEvents[2] }, events.First);
            EPAssertionUtil.AssertEqualsExactOrder((object[]) null, events.Second);

            // test empty vector
            eventVector.Clear();
            events = EventBeanUtility.FlattenList(eventVector);
            Assert.IsNull(events);
        }

        [Test]
        public void TestFlatten()
        {
            // test many arrays
            var testEvents = MakeEventArray(new[] { "a1", "a2", "b1", "b2", "b3", "c1", "c2" });
            var eventVector = new ArrayDeque<EventBean[]>();
            eventVector.Add(new[] { testEvents[0], testEvents[1] });
            eventVector.Add(new[] { testEvents[2] });
            eventVector.Add(new[] { testEvents[3], testEvents[4], testEvents[5] });
            eventVector.Add(new[] { testEvents[6] });

            var events = EventBeanUtility.Flatten(eventVector);
            Assert.AreEqual(7, events.Length);
            for (var i = 0; i < testEvents.Length; i++)
            {
                Assert.AreEqual(events[i], testEvents[i]);
            }

            // test just one array
            eventVector.Clear();
            eventVector.Add(new[] { testEvents[2] });
            events = EventBeanUtility.Flatten(eventVector);
            Assert.AreEqual(events[0], testEvents[2]);

            // test empty vector
            eventVector.Clear();
            events = EventBeanUtility.Flatten(eventVector);
            Assert.IsNull(events);
        }

        [Test]
        public void TestAppend()
        {
            var setOne = MakeEventArray(new[] { "a1", "a2" });
            var setTwo = MakeEventArray(new[] { "b1", "b2", "b3" });
            var total = EventBeanUtility.Append(setOne, setTwo);

            Assert.AreEqual(setOne[0], total[0]);
            Assert.AreEqual(setOne[1], total[1]);
            Assert.AreEqual(setTwo[0], total[2]);
            Assert.AreEqual(setTwo[1], total[3]);
            Assert.AreEqual(setTwo[2], total[4]);

            setOne = MakeEventArray(new[] { "a1" });
            setTwo = MakeEventArray(new[] { "b1" });
            total = EventBeanUtility.Append(setOne, setTwo);

            Assert.AreEqual(setOne[0], total[0]);
            Assert.AreEqual(setTwo[0], total[1]);
        }

        [Test]
        public void TestToArray()
        {
            // Test list with 2 elements
            var eventList = MakeEventList(new[] { "a1", "a2" });
            var array = eventList.ToArrayOrNull();
            Assert.AreEqual(2, array.Length);
            Assert.AreEqual(eventList[0], array[0]);
            Assert.AreEqual(eventList[1], array[1]);

            // Test list with 1 element
            eventList = MakeEventList(new[] { "a1" });
            array = eventList.ToArrayOrNull();
            Assert.AreEqual(1, array.Length);
            Assert.AreEqual(eventList[0], array[0]);

            // Test empty list
            eventList = MakeEventList(new string[0]);
            array = eventList.ToArrayOrNull();
            Assert.IsNull(array);

            // Test null
            array = eventList.ToArrayOrNull();
            Assert.IsNull(array);
        }

        [Test]
        public void TestGetPropertyArray()
        {
            // try 2 properties
            var getters = MakeGetters();
            var theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean("a", 10));
            var properties = EventBeanUtility.GetPropertyArray(theEvent, getters);
            Assert.AreEqual(2, properties.Length);
            Assert.AreEqual("a", properties[0]);
            Assert.AreEqual(10, properties[1]);

            // try no properties
            properties = EventBeanUtility.GetPropertyArray(theEvent, new EventPropertyGetter[0]);
            Assert.AreEqual(0, properties.Length);
        }

        [Test]
        public void TestMultiKey()
        {
            // try 2 properties
            var getters = MakeGetters();
            var theEvent = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean("a", 10));
            var multikey = EventBeanUtility.GetMultiKey(theEvent, getters);
            Assert.AreEqual(2, multikey.Keys.Length);
            Assert.AreEqual("a", multikey.Keys[0]);
            Assert.AreEqual(10, multikey.Keys[1]);

            // try no properties
            multikey = EventBeanUtility.GetMultiKey(theEvent, new EventPropertyGetter[0]);
            Assert.AreEqual(0, multikey.Keys.Length);
        }

        private EventPropertyGetter[] MakeGetters()
        {
            EventType eventType = supportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var getters = new EventPropertyGetter[2];
            getters[0] = eventType.GetGetter("theString");
            getters[1] = eventType.GetGetter("intPrimitive");
            return getters;
        }

        private EventBean[] MakeEventArray(string[] texts)
        {
            var events = new EventBean[texts.Length];
            for (var i = 0; i < texts.Length; i++)
            {
                events[i] = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean(texts[i], -1));
            }
            return events;
        }

        private IList<EventBean> MakeEventList(string[] texts)
        {
            IList<EventBean> events = new List<EventBean>();
            for (var i = 0; i < texts.Length; i++)
            {
                events.Add(SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean(texts[i], 0)));
            }
            return events;
        }
    }
} // end of namespace
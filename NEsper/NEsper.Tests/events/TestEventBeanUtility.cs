///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.events
{
    [TestFixture]
    public class TestEventBeanUtility
    {
        private static EventPropertyGetter[] MakeGetters()
        {
            EventType eventType = SupportEventTypeFactory.CreateBeanType(typeof(SupportBean));
            var getters = new EventPropertyGetter[2];
            getters[0] = eventType.GetGetter("TheString");
            getters[1] = eventType.GetGetter("IntPrimitive");
            return getters;
        }

        private static EventBean[] MakeEventArray(String[] texts)
        {
            var events = new EventBean[texts.Length];
            for (int i = 0; i < texts.Length; i++) {
                events[i] = SupportEventBeanFactory.CreateObject(texts[i]);
            }
            return events;
        }

        private static IList<EventBean> MakeEventList(String[] texts)
        {
            IList<EventBean> events = new List<EventBean>();
            for (int i = 0; i < texts.Length; i++) {
                events.Add(SupportEventBeanFactory.CreateObject(texts[i]));
            }
            return events;
        }

        [Test]
        public void TestAppend()
        {
            EventBean[] setOne = MakeEventArray(new[] {"a1", "a2"});
            EventBean[] setTwo = MakeEventArray(new[] {"b1", "b2", "b3"});
            EventBean[] total = setOne.Concat(setTwo).ToArray();

            Assert.AreEqual(setOne[0], total[0]);
            Assert.AreEqual(setOne[1], total[1]);
            Assert.AreEqual(setTwo[0], total[2]);
            Assert.AreEqual(setTwo[1], total[3]);
            Assert.AreEqual(setTwo[2], total[4]);

            setOne = MakeEventArray(new[] {"a1"});
            setTwo = MakeEventArray(new[] {"b1"});
            total = setOne.Concat(setTwo).ToArray();

            Assert.AreEqual(setOne[0], total[0]);
            Assert.AreEqual(setTwo[0], total[1]);
        }

        [Test]
        public void TestArrayOp()
        {
            EventBean[] testEvent = MakeEventArray(new[] {"a1", "a2", "a3"});

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] {testEvent[0]},
                                                    EventBeanUtility.AddToArray(new EventBean[0], testEvent[0]));

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] {testEvent[0], testEvent[1]},
                                                    EventBeanUtility.AddToArray(new[] {testEvent[0]}, testEvent[1]));

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] {testEvent[0], testEvent[1], testEvent[2]},
                                                    EventBeanUtility.AddToArray(new[] {testEvent[0], testEvent[1]},
                                                                                testEvent[2]));
        }

        [Test]
        public void TestArrayOpAdd()
        {
            EventBean[] testEvent = MakeEventArray(new[] {"a1", "a2", "a3"});

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] {testEvent[0], testEvent[1], testEvent[2]},
                                                    EventBeanUtility.AddToArray(new[] {testEvent[0]},
                                                                                new[] {testEvent[1], testEvent[2]}));

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] {testEvent[1], testEvent[2]},
                                                    EventBeanUtility.AddToArray(new EventBean[] {},
                                                                                new[] {testEvent[1], testEvent[2]}));

            EPAssertionUtil.AssertEqualsAnyOrder(new Object[] {testEvent[0]},
                                                    EventBeanUtility.AddToArray(new[] {testEvent[0]}, new EventBean[0]));
        }

        [Test]
        public void TestFlatten()
        {
            // test many arrays
            EventBean[] testEvents = MakeEventArray(new[] {"a1", "a2", "b1", "b2", "b3", "c1", "c2"});
            var eventVector = new LinkedList<EventBean[]>();
            eventVector.AddLast(new[] {testEvents[0], testEvents[1]});
            eventVector.AddLast(new[] { testEvents[2] });
            eventVector.AddLast(new[] { testEvents[3], testEvents[4], testEvents[5] });
            eventVector.AddLast(new[] { testEvents[6] });

            EventBean[] events = EventBeanUtility.Flatten(eventVector);
            Assert.AreEqual(7, events.Length);
            for (int i = 0; i < testEvents.Length; i++) {
                Assert.AreEqual(events[i], testEvents[i]);
            }

            // test just one array
            eventVector.Clear();
            eventVector.AddLast(new[] { testEvents[2] });
            events = EventBeanUtility.Flatten(eventVector);
            Assert.AreEqual(events[0], testEvents[2]);

            // test empty vector
            eventVector.Clear();
            events = EventBeanUtility.Flatten(eventVector);
            Assert.IsNull(events);
        }

        [Test]
        public void TestFlattenList()
        {
            // test many arrays
            EventBean[] testEvents = MakeEventArray(new[] {"a1", "a2", "b1", "b2", "b3", "c1", "c2"});
            var eventVector = new LinkedList<UniformPair<EventBean[]>>();

            eventVector.AddLast(new UniformPair<EventBean[]>(null, new[] { testEvents[0], testEvents[1] }));
            eventVector.AddLast(new UniformPair<EventBean[]>(new[] { testEvents[2] }, null));
            eventVector.AddLast(new UniformPair<EventBean[]>(null, new[] { testEvents[3], testEvents[4], testEvents[5] }));
            eventVector.AddLast(new UniformPair<EventBean[]>(new[] { testEvents[6] }, null));

            UniformPair<EventBean[]> events = EventBeanUtility.FlattenList(eventVector);
            EPAssertionUtil.AssertEqualsExactOrder(events.First, new[] { testEvents[2], testEvents[6] });
            EPAssertionUtil.AssertEqualsExactOrder(events.Second,
                                                      new[]
                                                      {
                                                          testEvents[0], testEvents[1], testEvents[3], testEvents[4],
                                                          testEvents[5]
                                                      });

            // test just one array
            eventVector.Clear();
            eventVector.AddLast(new UniformPair<EventBean[]>(new[] {testEvents[2]}, null));
            events = EventBeanUtility.FlattenList(eventVector);
            EPAssertionUtil.AssertEqualsExactOrder(events.First, new[] {testEvents[2]});
            EPAssertionUtil.AssertEqualsExactOrder(null, events.Second);

            // test empty vector
            eventVector.Clear();
            events = EventBeanUtility.FlattenList(eventVector);
            Assert.IsNull(events);
        }

        [Test]
        public void TestGetPropertyArray()
        {
            // try 2 properties
            EventPropertyGetter[] getters = MakeGetters();
            EventBean theEvent = SupportEventBeanFactory.CreateObject(new SupportBean("a", 10));
            Object[] properties = EventBeanUtility.GetPropertyArray(theEvent, getters);
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
            EventPropertyGetter[] getters = MakeGetters();
            EventBean theEvent = SupportEventBeanFactory.CreateObject(new SupportBean("a", 10));
            MultiKeyUntyped multikey = EventBeanUtility.GetMultiKey(theEvent, getters);
            Assert.AreEqual(2, multikey.Keys.Length);
            Assert.AreEqual("a", multikey.Keys[0]);
            Assert.AreEqual(10, multikey.Keys[1]);

            // try no properties
            multikey = EventBeanUtility.GetMultiKey(theEvent, new EventPropertyGetter[0]);
            Assert.AreEqual(0, multikey.Keys.Length);
        }

        [Test]
        public void TestToArray()
        {
            // Test list with 2 elements
            IList<EventBean> eventList = MakeEventList(new[] {"a1", "a2"});
            EventBean[] array = eventList.ToArray();
            Assert.AreEqual(2, array.Length);
            Assert.AreEqual(eventList[0], array[0]);
            Assert.AreEqual(eventList[1], array[1]);

            // Test list with 1 element
            eventList = MakeEventList(new[] {"a1"});
            array = eventList.ToArray();
            Assert.AreEqual(1, array.Length);
            Assert.AreEqual(eventList[0], array[0]);

            // Test empty list
            eventList = MakeEventList(new String[0]);
            array = eventList.ToArray();
            Assert.That(array.Length, Is.EqualTo(0));
        }
    }
}

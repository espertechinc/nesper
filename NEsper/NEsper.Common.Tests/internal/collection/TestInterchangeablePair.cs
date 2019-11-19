///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat.collections;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestInterchangeablePair : AbstractCommonTest
    {
        private InterchangeablePair<string, string> pair1a = new InterchangeablePair<string, string>("a", "b");
        private readonly InterchangeablePair<string, string> pair1b = new InterchangeablePair<string, string>("a", "c");
        private readonly InterchangeablePair<string, string> pair1c = new InterchangeablePair<string, string>("c", "b");
        private InterchangeablePair<string, string> pair1d = new InterchangeablePair<string, string>("a", "b");
        private InterchangeablePair<string, string> pair1e = new InterchangeablePair<string, string>("b", "a");

        private InterchangeablePair<string, string> pair2a = new InterchangeablePair<string, string>("a", null);
        private InterchangeablePair<string, string> pair2b = new InterchangeablePair<string, string>("b", null);
        private InterchangeablePair<string, string> pair2c = new InterchangeablePair<string, string>("a", null);

        private InterchangeablePair<string, string> pair3a = new InterchangeablePair<string, string>(null, "b");
        private InterchangeablePair<string, string> pair3b = new InterchangeablePair<string, string>(null, "c");
        private InterchangeablePair<string, string> pair3c = new InterchangeablePair<string, string>(null, "b");

        private InterchangeablePair<string, string> pair4a = new InterchangeablePair<string, string>(null, null);
        private InterchangeablePair<string, string> pair4b = new InterchangeablePair<string, string>(null, null);

        [Test]
        public void TestEquals()
        {
            Assert.IsTrue(pair1a.Equals(pair1d) && pair1d.Equals(pair1a));
            Assert.IsTrue(pair1a.Equals(pair1e) && pair1e.Equals(pair1a));
            Assert.IsFalse(pair1a.Equals(pair1b));
            Assert.IsFalse(pair1a.Equals(pair1c));
            Assert.IsFalse(pair1a.Equals(pair2a));
            Assert.IsFalse(pair1a.Equals(pair3a));
            Assert.IsFalse(pair1a.Equals(pair4a));

            Assert.IsTrue(pair2a.Equals(pair2c) && pair2c.Equals(pair2a));
            Assert.IsTrue(pair2b.Equals(pair3a) && pair3a.Equals(pair2b));
            Assert.IsFalse(pair2a.Equals(pair2b));
            Assert.IsFalse(pair2a.Equals(pair1a));
            Assert.IsFalse(pair2b.Equals(pair1e));
            Assert.IsFalse(pair2b.Equals(pair3b));
            Assert.IsFalse(pair2a.Equals(pair4a));

            Assert.IsTrue(pair3a.Equals(pair3c) && pair3c.Equals(pair3a));
            Assert.IsTrue(pair3c.Equals(pair2b) && pair2b.Equals(pair3c));
            Assert.IsFalse(pair3a.Equals(pair3b));
            Assert.IsFalse(pair3b.Equals(pair3a));
            Assert.IsFalse(pair3a.Equals(pair1a));
            Assert.IsFalse(pair3a.Equals(pair2a));
            Assert.IsFalse(pair3a.Equals(pair4a));

            Assert.IsTrue(pair4a.Equals(pair4b) && pair4b.Equals(pair4a));
            Assert.IsFalse(pair4a.Equals(pair1b) || pair4a.Equals(pair2a) || pair4a.Equals(pair3a));
        }

        [Test]
        public void TestHashCode()
        {
            if ("a".GetHashCode() > "b".GetHashCode()) {
                Assert.IsTrue(pair1a.GetHashCode() == ("a".GetHashCode() * 397 ^ "b".GetHashCode()));
            }
            else {
                Assert.IsTrue(pair1a.GetHashCode() == ("b".GetHashCode() * 397 ^ "a".GetHashCode()));
            }

            Assert.IsTrue(pair2a.GetHashCode() == "a".GetHashCode());
            Assert.IsTrue(pair3a.GetHashCode() == "b".GetHashCode());
            Assert.IsTrue(pair4a.GetHashCode() == 0);

            Assert.IsTrue(pair1a.GetHashCode() != pair2a.GetHashCode());
            Assert.IsTrue(pair1a.GetHashCode() != pair3a.GetHashCode());
            Assert.IsTrue(pair1a.GetHashCode() != pair4a.GetHashCode());

            Assert.IsTrue(pair1a.GetHashCode() == pair1d.GetHashCode());
            Assert.IsTrue(pair2a.GetHashCode() == pair2c.GetHashCode());
            Assert.IsTrue(pair3a.GetHashCode() == pair3c.GetHashCode());
            Assert.IsTrue(pair4a.GetHashCode() == pair4b.GetHashCode());

            Assert.IsTrue(pair2b.GetHashCode() == pair3a.GetHashCode());
        }

        [Test]
        public void TestSetBehavior()
        {
            ISet<InterchangeablePair<EventBean, EventBean>> eventPairs = new HashSet<InterchangeablePair<EventBean, EventBean>>();

            EventBean[] events = new EventBean[4];
            for (int i = 0; i < events.Length; i++)
            {
                events[i] = SupportEventBeanFactory.CreateObject(
                    supportEventTypeFactory, new SupportBean("E" + i, i));
            }

            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[0], events[1]));
            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[0], events[2]));
            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[1], events[2]));
            Assert.AreEqual(3, eventPairs.Count);

            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[0], events[1]));
            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[1], events[2]));
            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[2], events[0]));
            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[2], events[1]));
            eventPairs.Add(new InterchangeablePair<EventBean, EventBean>(events[1], events[0]));
            Assert.AreEqual(3, eventPairs.Count);

            Assert.IsTrue(eventPairs.Contains(new InterchangeablePair<EventBean, EventBean>(events[1], events[0])));
            Assert.IsFalse(eventPairs.Contains(new InterchangeablePair<EventBean, EventBean>(events[3], events[0])));
            Assert.IsTrue(eventPairs.Contains(new InterchangeablePair<EventBean, EventBean>(events[1], events[2])));
            Assert.IsTrue(eventPairs.Contains(new InterchangeablePair<EventBean, EventBean>(events[2], events[0])));

            eventPairs.Remove(new InterchangeablePair<EventBean, EventBean>(events[2], events[0]));
            Assert.IsFalse(eventPairs.Contains(new InterchangeablePair<EventBean, EventBean>(events[2], events[0])));
            eventPairs.Remove(new InterchangeablePair<EventBean, EventBean>(events[1], events[2]));
            eventPairs.Remove(new InterchangeablePair<EventBean, EventBean>(events[1], events[0]));

            Assert.IsTrue(eventPairs.IsEmpty());
        }
    }
} // end of namespace
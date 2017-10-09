///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Collections.Generic;
using System.Diagnostics;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;


namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestInterchangeablePair 
    {
        private readonly InterchangeablePair<String, String> _pair1A = new InterchangeablePair<String, String>("a", "b");
        private readonly InterchangeablePair<String, String> _pair1B = new InterchangeablePair<String, String>("a", "c");
        private readonly InterchangeablePair<String, String> _pair1C = new InterchangeablePair<String, String>("c", "b");
        private readonly InterchangeablePair<String, String> _pair1D = new InterchangeablePair<String, String>("a", "b");
        private readonly InterchangeablePair<String, String> _pair1E = new InterchangeablePair<String, String>("b", "a");
    
        private readonly InterchangeablePair<String, String> _pair2A = new InterchangeablePair<String, String>("a", null);
        private readonly InterchangeablePair<String, String> _pair2B = new InterchangeablePair<String, String>("b", null);
        private readonly InterchangeablePair<String, String> _pair2C = new InterchangeablePair<String, String>("a", null);
    
        private readonly InterchangeablePair<String, String> _pair3A = new InterchangeablePair<String, String>(null, "b");
        private readonly InterchangeablePair<String, String> _pair3B = new InterchangeablePair<String, String>(null, "c");
        private readonly InterchangeablePair<String, String> _pair3C = new InterchangeablePair<String, String>(null, "b");
    
        private readonly InterchangeablePair<String, String> _pair4A = new InterchangeablePair<String, String>(null, null);
        private readonly InterchangeablePair<String, String> _pair4B = new InterchangeablePair<String, String>(null, null);
    
        [Test]
        public void TestEquals()
        {
            Assert.IsTrue(_pair1A.Equals(_pair1D) && _pair1D.Equals(_pair1A));
            Assert.IsTrue(_pair1A.Equals(_pair1E) && _pair1E.Equals(_pair1A));
            Assert.IsFalse(_pair1A.Equals(_pair1B));
            Assert.IsFalse(_pair1A.Equals(_pair1C));
            Assert.IsFalse(_pair1A.Equals(_pair2A));
            Assert.IsFalse(_pair1A.Equals(_pair3A));
            Assert.IsFalse(_pair1A.Equals(_pair4A));
    
            Assert.IsTrue(_pair2A.Equals(_pair2C) && _pair2C.Equals(_pair2A));
            Assert.IsTrue(_pair2B.Equals(_pair3A) && _pair3A.Equals(_pair2B));
            Assert.IsFalse(_pair2A.Equals(_pair2B));
            Assert.IsFalse(_pair2A.Equals(_pair1A));
            Assert.IsFalse(_pair2B.Equals(_pair1E));
            Assert.IsFalse(_pair2B.Equals(_pair3B));
            Assert.IsFalse(_pair2A.Equals(_pair4A));
    
            Assert.IsTrue(_pair3A.Equals(_pair3C) && _pair3C.Equals(_pair3A));
            Assert.IsTrue(_pair3C.Equals(_pair2B) && _pair2B.Equals(_pair3C));
            Assert.IsFalse(_pair3A.Equals(_pair3B));
            Assert.IsFalse(_pair3B.Equals(_pair3A));
            Assert.IsFalse(_pair3A.Equals(_pair1A));
            Assert.IsFalse(_pair3A.Equals(_pair2A));
            Assert.IsFalse(_pair3A.Equals(_pair4A));
    
            Assert.IsTrue(_pair4A.Equals(_pair4B) && _pair4B.Equals(_pair4A));
            Assert.IsFalse(_pair4A.Equals(_pair1B) || _pair4A.Equals(_pair2A) || _pair4A.Equals(_pair3A));
        }
    
        [Test]
        public void TestHashCode()
        {
            Assert.That(_pair1A.GetHashCode(), Is.EqualTo(("a".GetHashCode() * 397) ^ ("b".GetHashCode())));

            Assert.IsTrue(_pair2A.GetHashCode() == "a".GetHashCode());
            Assert.IsTrue(_pair3A.GetHashCode() == "b".GetHashCode());
            Assert.IsTrue(_pair4A.GetHashCode() == 0);
    
            Assert.IsTrue(_pair1A.GetHashCode() != _pair2A.GetHashCode());
            Assert.IsTrue(_pair1A.GetHashCode() != _pair3A.GetHashCode());
            Assert.IsTrue(_pair1A.GetHashCode() != _pair4A.GetHashCode());
    
            Assert.IsTrue(_pair1A.GetHashCode() == _pair1D.GetHashCode());
            Assert.IsTrue(_pair2A.GetHashCode() == _pair2C.GetHashCode());
            Assert.IsTrue(_pair3A.GetHashCode() == _pair3C.GetHashCode());
            Assert.IsTrue(_pair4A.GetHashCode() == _pair4B.GetHashCode());
    
            Assert.IsTrue(_pair2B.GetHashCode() == _pair3A.GetHashCode());
        }
    
        [Test]
        public void TestSetBehavior()
        {
            ICollection<InterchangeablePair<EventBean, EventBean>> eventPairs = new HashSet<InterchangeablePair<EventBean, EventBean>>();
    
            var events = new EventBean[4];
            for (int i = 0; i < events.Length; i++)
            {
                events[i] = SupportEventBeanFactory.CreateObject(i);
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
}

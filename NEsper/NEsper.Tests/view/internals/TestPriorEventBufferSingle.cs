///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.view.internals
{
    [TestFixture]
    public class TestPriorEventBufferSingle 
    {
        private PriorEventBufferSingle _buffer;
        private EventBean[] _events;
    
        [SetUp]
        public void SetUp()
        {
            _buffer = new PriorEventBufferSingle(3);
    
            _events = new EventBean[100];
            for (int i = 0; i < _events.Length; i++)
            {
                SupportBean_S0 bean = new SupportBean_S0(i);
                _events[i] = SupportEventBeanFactory.CreateObject(bean);
            }
        }
    
        [Test]
        public void TestFlow()
        {
            _buffer.Update(new [] {_events[0], _events[1]}, null);
            AssertEvents0And1();
    
            _buffer.Update(new [] {_events[2]}, null);
            AssertEvents0And1();
            AssertEvents2();
    
            _buffer.Update(new [] {_events[3], _events[4]}, null);
            AssertEvents0And1();
            AssertEvents2();
            AssertEvents3And4();
    
            _buffer.Update(null, new [] {_events[0]});
            AssertEvents0And1();
            AssertEvents2();
            AssertEvents3And4();
    
            _buffer.Update(null, new [] {_events[1], _events[3]});
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[1], 0));
            AssertEvents2();
            AssertEvents3And4();
    
            _buffer.Update(new [] {_events[5]}, null);
            AssertEvents2();
            Assert.AreEqual(_events[1], _buffer.GetRelativeToEvent(_events[4], 0));
            Assert.AreEqual(_events[2], _buffer.GetRelativeToEvent(_events[5], 0));
        }
    
        private void AssertEvents0And1()
        {
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[0], 0));     // getting 0 is getting prior 1 (see indexes)
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[1], 0));
        }
    
        private void AssertEvents2()
        {
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[2], 0));
        }
    
        private void AssertEvents3And4()
        {
            Assert.AreEqual(_events[0], _buffer.GetRelativeToEvent(_events[3], 0));
            Assert.AreEqual(_events[1], _buffer.GetRelativeToEvent(_events[4], 0));
        }
    
        public void TryInvalid(EventBean theEvent, int index)
        {
            try
            {
                _buffer.GetRelativeToEvent(theEvent, index);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
        }
    
        [Test]
        public void TestInvalid()
        {
            try
            {
                _buffer.GetRelativeToEvent(_events[1], 2);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
}

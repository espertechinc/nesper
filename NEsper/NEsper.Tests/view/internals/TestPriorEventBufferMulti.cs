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
    public class TestPriorEventBufferMulti 
    {
        private PriorEventBufferMulti _buffer;
        private EventBean[] _events;
    
        [SetUp]
        public void SetUp()
        {
            int[] indexes = new int[] {1, 3};
            _buffer = new PriorEventBufferMulti(indexes);
    
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
            _buffer.Update(new EventBean[] {_events[0], _events[1]}, null);
            AssertEvents0And1();
    
            _buffer.Update(new EventBean[] {_events[2]}, null);
            AssertEvents0And1();
            AssertEvents2();
    
            _buffer.Update(new EventBean[] {_events[3], _events[4]}, null);
            AssertEvents0And1();
            AssertEvents2();
            AssertEvents3And4();
    
            _buffer.Update(null, new EventBean[] {_events[0]});
            AssertEvents0And1();
            AssertEvents2();
            AssertEvents3And4();
    
            _buffer.Update(null, new EventBean[] {_events[1], _events[3]});
            TryInvalid(_events[0], 0);
            TryInvalid(_events[0], 1);
            Assert.AreEqual(_events[0], _buffer.GetRelativeToEvent(_events[1], 0));
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[1], 1));
            AssertEvents2();
            AssertEvents3And4();
    
            _buffer.Update(new EventBean[] {_events[5]}, null);
            TryInvalid(_events[0], 0);
            TryInvalid(_events[1], 0);
            TryInvalid(_events[3], 0);
            AssertEvents2();
            Assert.AreEqual(_events[3], _buffer.GetRelativeToEvent(_events[4], 0));
            Assert.AreEqual(_events[1], _buffer.GetRelativeToEvent(_events[4], 1));
            Assert.AreEqual(_events[4], _buffer.GetRelativeToEvent(_events[5], 0));
            Assert.AreEqual(_events[2], _buffer.GetRelativeToEvent(_events[5], 1));
        }
    
        private void AssertEvents0And1()
        {
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[0], 0));     // getting 0 is getting prior 1 (see indexes)
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[0], 1));     // getting 1 is getting prior 3 (see indexes)
            Assert.AreEqual(_events[0], _buffer.GetRelativeToEvent(_events[1], 0));
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[1], 1));
        }
    
        private void AssertEvents2()
        {
            Assert.AreEqual(_events[1], _buffer.GetRelativeToEvent(_events[2], 0));
            Assert.IsNull(_buffer.GetRelativeToEvent(_events[2], 1));
        }
    
        private void AssertEvents3And4()
        {
            Assert.AreEqual(_events[2], _buffer.GetRelativeToEvent(_events[3], 0));
            Assert.AreEqual(_events[0], _buffer.GetRelativeToEvent(_events[3], 1));
            Assert.AreEqual(_events[3], _buffer.GetRelativeToEvent(_events[4], 0));
            Assert.AreEqual(_events[1], _buffer.GetRelativeToEvent(_events[4], 1));
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

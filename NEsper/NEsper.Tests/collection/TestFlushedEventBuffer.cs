///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.support.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestFlushedEventBuffer 
    {
        private FlushedEventBuffer _buffer;
        private EventBean[] _events;
    
        [SetUp]
        public void SetUp()
        {
            _buffer = new FlushedEventBuffer();
            _events = new EventBean[10];
    
            for (int i = 0; i < _events.Length; i++)
            {
                _events[i] = SupportEventBeanFactory.CreateObject(i);
            }
        }
    
        [Test]
        public void TestFlow()
        {
            // test empty buffer
            _buffer.Add(null);
            Assert.IsNull(_buffer.GetAndFlush());
            _buffer.Flush();
    
            // test add single events
            _buffer.Add(new [] { _events[0] });
            EventBean[] results = _buffer.GetAndFlush();
            Assert.IsTrue((results.Length == 1) && (results[0] == _events[0]));
    
            _buffer.Add(new [] { _events[0] });
            _buffer.Add(new [] { _events[1] });
            results = _buffer.GetAndFlush();
            Assert.IsTrue((results.Length == 2));
            Assert.AreSame(_events[0], results[0]);
            Assert.AreSame(_events[1], results[1]);
    
            _buffer.Flush();
            Assert.IsNull(_buffer.GetAndFlush());
    
            // Add multiple events
            _buffer.Add(new [] { _events[2], _events[3] });
            _buffer.Add(new [] { _events[4], _events[5] });
            results = _buffer.GetAndFlush();
            Assert.IsTrue((results.Length == 4));
            Assert.AreSame(_events[2], results[0]);
            Assert.AreSame(_events[3], results[1]);
            Assert.AreSame(_events[4], results[2]);
            Assert.AreSame(_events[5], results[3]);
    
            _buffer.Flush();
            Assert.IsNull(_buffer.GetAndFlush());
        }
    }
}

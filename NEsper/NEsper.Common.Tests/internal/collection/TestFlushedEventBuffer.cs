///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestFlushedEventBuffer : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            buffer = new FlushedEventBuffer();
            events = new EventBean[10];

            for (var i = 0; i < events.Length; i++)
            {
                events[i] = SupportEventBeanFactory.CreateObject(
                    supportEventTypeFactory, new SupportBean("a", i));
            }
        }

        private FlushedEventBuffer buffer;
        private EventBean[] events;

        [Test]
        public void TestFlow()
        {
            // test empty buffer
            buffer.Add(null);
            Assert.IsNull(buffer.GetAndFlush());
            buffer.Flush();

            // test add single events
            buffer.Add(new[] { events[0] });
            var results = buffer.GetAndFlush();
            Assert.IsTrue(results.Length == 1 && results[0] == events[0]);

            buffer.Add(new[] { events[0] });
            buffer.Add(new[] { events[1] });
            results = buffer.GetAndFlush();
            Assert.IsTrue(results.Length == 2);
            Assert.AreSame(events[0], results[0]);
            Assert.AreSame(events[1], results[1]);

            buffer.Flush();
            Assert.IsNull(buffer.GetAndFlush());

            // Add multiple events
            buffer.Add(new[] { events[2], events[3] });
            buffer.Add(new[] { events[4], events[5] });
            results = buffer.GetAndFlush();
            Assert.IsTrue(results.Length == 4);
            Assert.AreSame(events[2], results[0]);
            Assert.AreSame(events[3], results[1]);
            Assert.AreSame(events[4], results[2]);
            Assert.AreSame(events[5], results[3]);

            buffer.Flush();
            Assert.IsNull(buffer.GetAndFlush());
        }
    }
} // end of namespace
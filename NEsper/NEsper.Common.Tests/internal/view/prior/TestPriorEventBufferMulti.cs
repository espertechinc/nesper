///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.view.prior
{
    [TestFixture]
    public class TestPriorEventBufferMulti : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            int[] indexes = { 1, 3 };
            buffer = new PriorEventBufferMulti(indexes);

            events = new EventBean[100];
            for (var i = 0; i < events.Length; i++)
            {
                var bean = new SupportBean_S0(i);
                events[i] = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
            }
        }

        private PriorEventBufferMulti buffer;
        private EventBean[] events;

        private void AssertEvents0And1()
        {
            Assert.IsNull(buffer.GetRelativeToEvent(events[0], 0)); // getting 0 is getting prior 1 (see indexes)
            Assert.IsNull(buffer.GetRelativeToEvent(events[0], 1)); // getting 1 is getting prior 3 (see indexes)
            Assert.AreEqual(events[0], buffer.GetRelativeToEvent(events[1], 0));
            Assert.IsNull(buffer.GetRelativeToEvent(events[1], 1));
        }

        private void AssertEvents2()
        {
            Assert.AreEqual(events[1], buffer.GetRelativeToEvent(events[2], 0));
            Assert.IsNull(buffer.GetRelativeToEvent(events[2], 1));
        }

        private void AssertEvents3And4()
        {
            Assert.AreEqual(events[2], buffer.GetRelativeToEvent(events[3], 0));
            Assert.AreEqual(events[0], buffer.GetRelativeToEvent(events[3], 1));
            Assert.AreEqual(events[3], buffer.GetRelativeToEvent(events[4], 0));
            Assert.AreEqual(events[1], buffer.GetRelativeToEvent(events[4], 1));
        }

        public void TryInvalid(
            EventBean theEvent,
            int index)
        {
            try
            {
                buffer.GetRelativeToEvent(theEvent, index);
                Assert.Fail();
            }
            catch (IllegalStateException)
            {
                // expected
            }
        }

        [Test, RunInApplicationDomain]
        public void TestFlow()
        {
            buffer.Update(new[] { events[0], events[1] }, null);
            AssertEvents0And1();

            buffer.Update(new[] { events[2] }, null);
            AssertEvents0And1();
            AssertEvents2();

            buffer.Update(new[] { events[3], events[4] }, null);
            AssertEvents0And1();
            AssertEvents2();
            AssertEvents3And4();

            buffer.Update(null, new[] { events[0] });
            AssertEvents0And1();
            AssertEvents2();
            AssertEvents3And4();

            buffer.Update(null, new[] { events[1], events[3] });
            TryInvalid(events[0], 0);
            TryInvalid(events[0], 1);
            Assert.AreEqual(events[0], buffer.GetRelativeToEvent(events[1], 0));
            Assert.IsNull(buffer.GetRelativeToEvent(events[1], 1));
            AssertEvents2();
            AssertEvents3And4();

            buffer.Update(new[] { events[5] }, null);
            TryInvalid(events[0], 0);
            TryInvalid(events[1], 0);
            TryInvalid(events[3], 0);
            AssertEvents2();
            Assert.AreEqual(events[3], buffer.GetRelativeToEvent(events[4], 0));
            Assert.AreEqual(events[1], buffer.GetRelativeToEvent(events[4], 1));
            Assert.AreEqual(events[4], buffer.GetRelativeToEvent(events[5], 0));
            Assert.AreEqual(events[2], buffer.GetRelativeToEvent(events[5], 1));
        }

        [Test, RunInApplicationDomain]
        public void TestInvalid()
        {
            try
            {
                buffer.GetRelativeToEvent(events[1], 2);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
} // end of namespace

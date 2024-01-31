///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.common.@internal.supportunit.@event;
using com.espertech.esper.compat;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.view.prior
{
    [TestFixture]
    public class TestPriorEventBufferSingle : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            buffer = new PriorEventBufferSingle(3);

            events = new EventBean[100];
            for (var i = 0; i < events.Length; i++)
            {
                var bean = new SupportBean_S0(i);
                events[i] = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
            }
        }

        private PriorEventBufferSingle buffer;
        private EventBean[] events;

        private void AssertEvents0And1()
        {
            ClassicAssert.IsNull(buffer.GetRelativeToEvent(events[0], 0)); // getting 0 is getting prior 1 (see indexes)
            ClassicAssert.IsNull(buffer.GetRelativeToEvent(events[1], 0));
        }

        private void AssertEvents2()
        {
            ClassicAssert.IsNull(buffer.GetRelativeToEvent(events[2], 0));
        }

        private void AssertEvents3And4()
        {
            ClassicAssert.AreEqual(events[0], buffer.GetRelativeToEvent(events[3], 0));
            ClassicAssert.AreEqual(events[1], buffer.GetRelativeToEvent(events[4], 0));
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

        [Test]
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
            ClassicAssert.IsNull(buffer.GetRelativeToEvent(events[1], 0));
            AssertEvents2();
            AssertEvents3And4();

            buffer.Update(new[] { events[5] }, null);
            AssertEvents2();
            ClassicAssert.AreEqual(events[1], buffer.GetRelativeToEvent(events[4], 0));
            ClassicAssert.AreEqual(events[2], buffer.GetRelativeToEvent(events[5], 0));
        }
    }
} // end of namespace

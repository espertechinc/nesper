///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.supportunit.@event;

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestOneEventCollection : AbstractCommonTest
    {
        private OneEventCollection list;
        private EventBean[] events;

        [SetUp]
        public void SetUp()
        {
            list = new OneEventCollection();
            events = SupportEventBeanFactory.MakeEvents(
                supportEventTypeFactory, new string[] { "1", "2", "3", "4" });
        }

        [Test]
        public void TestFlow()
        {
            Assert.IsTrue(list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(list.ToArray(), new EventBean[0]);

            list.Add(events[0]);
            Assert.IsFalse(list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(list.ToArray(), new EventBean[] { events[0] });

            list.Add(events[1]);
            Assert.IsFalse(list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(list.ToArray(), new EventBean[] { events[0], events[1] });

            list.Add(events[2]);
            Assert.IsFalse(list.IsEmpty());
            EPAssertionUtil.AssertEqualsExactOrder(list.ToArray(), new EventBean[] { events[0], events[1], events[2] });
        }
    }
} // end of namespace

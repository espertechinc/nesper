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

using NUnit.Framework;

namespace com.espertech.esper.common.@internal.view.prior
{
    [TestFixture]
    public class TestPriorEventBufferUnbound : AbstractCommonTest
    {
        [SetUp]
        public void SetUp()
        {
            buffer = new PriorEventBufferUnbound(3);

            events = new EventBean[100];
            for (var i = 0; i < events.Length; i++)
            {
                var bean = new SupportBean_S0(i);
                events[i] = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, bean);
            }
        }

        private PriorEventBufferUnbound buffer;
        private EventBean[] events;

        [Test, RunInApplicationDomain]
        public void TestFlow()
        {
            buffer.Update(new[] { events[0], events[1] }, null);
            Assert.AreEqual(events[1], buffer.GetNewData(0));
            Assert.AreEqual(events[0], buffer.GetNewData(1));
            Assert.IsNull(buffer.GetNewData(2));
        }

        [Test, RunInApplicationDomain]
        public void TestInvalid()
        {
            try
            {
                buffer.GetNewData(6);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }
    }
} // end of namespace

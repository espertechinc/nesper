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
using com.espertech.esper.common.@internal.supportunit.util;
using com.espertech.esper.container;
using NUnit.Framework;

namespace com.espertech.esper.common.@internal.collection
{
    [TestFixture]
    public class TestRollingEventBuffer : AbstractCommonTest
    {
        private RollingEventBuffer bufferOne;
        private RollingEventBuffer bufferTwo;
        private RollingEventBuffer bufferFive;
        private static int eventId;

        [SetUp]
        public void SetUp()
        {
            bufferOne = new RollingEventBuffer(1);
            bufferTwo = new RollingEventBuffer(2);
            bufferFive = new RollingEventBuffer(5);
        }

        [Test]
        public void TestFlowSizeOne()
        {
            bufferOne.Add((EventBean[]) null);
            Assert.IsNull(bufferOne.Get(0));

            EventBean[] set1 = Make(2);
            bufferOne.Add(set1);
            Assert.AreSame(set1[1], bufferOne.Get(0));
            TryInvalid(bufferOne, 1);

            EventBean[] set2 = Make(1);
            bufferOne.Add(set2);
            Assert.AreSame(set2[0], bufferOne.Get(0));
            TryInvalid(bufferOne, 1);
        }

        [Test]
        public void TestFlowSizeTwo()
        {
            EventBean[] set1 = Make(2);
            bufferTwo.Add(set1);
            AssertEvents(new EventBean[] { set1[1], set1[0] }, bufferTwo);

            EventBean[] set2 = Make(1);
            bufferTwo.Add(set2);
            AssertEvents(new EventBean[] { set2[0], set1[1] }, bufferTwo);

            EventBean[] set3 = Make(1);
            bufferTwo.Add(set3);
            AssertEvents(new EventBean[] { set3[0], set2[0] }, bufferTwo);

            EventBean[] set4 = Make(3);
            bufferTwo.Add(set4);
            AssertEvents(new EventBean[] { set4[2], set4[1] }, bufferTwo);

            EventBean[] set5 = Make(5);
            bufferTwo.Add(set5);
            AssertEvents(new EventBean[] { set5[4], set5[3] }, bufferTwo);

            EventBean[] set6 = Make(1);
            bufferTwo.Add(set6);
            AssertEvents(new EventBean[] { set6[0], set5[4] }, bufferTwo);
            bufferTwo.Add(Make(0));
            AssertEvents(new EventBean[] { set6[0], set5[4] }, bufferTwo);

            EventBean[] set7 = Make(2);
            bufferTwo.Add(set7);
            AssertEvents(new EventBean[] { set7[1], set7[0] }, bufferTwo);
        }

        [Test]
        public void TestFlowSizeTen()
        {
            EventBean[] set1 = Make(3);
            bufferFive.Add(set1);
            AssertEvents(new EventBean[] { set1[2], set1[1], set1[0], null, null }, bufferFive);

            EventBean[] set2 = Make(1);
            bufferFive.Add(set2);
            AssertEvents(new EventBean[] { set2[0], set1[2], set1[1], set1[0], null }, bufferFive);

            EventBean[] set3 = Make(3);
            bufferFive.Add(set3);
            AssertEvents(new EventBean[] { set3[2], set3[1], set3[0], set2[0], set1[2] }, bufferFive);

            EventBean[] set4 = Make(5);
            bufferFive.Add(set4);
            AssertEvents(new EventBean[] { set4[4], set4[3], set4[2], set4[1], set4[0] }, bufferFive);

            EventBean[] set5 = Make(8);
            bufferFive.Add(set5);
            AssertEvents(new EventBean[] { set5[7], set5[6], set5[5], set5[4], set5[3] }, bufferFive);

            EventBean[] set6 = Make(2);
            bufferFive.Add(set6);
            AssertEvents(new EventBean[] { set6[1], set6[0], set5[7], set5[6], set5[5] }, bufferFive);
        }

        private void AssertEvents(EventBean[] expected, RollingEventBuffer buffer)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreSame(expected[i], buffer.Get(i));
            }
            TryInvalid(buffer, expected.Length);
        }

        private void TryInvalid(RollingEventBuffer buffer, int index)
        {
            try
            {
                buffer.Get(index);
                Assert.Fail();
            }
            catch (ArgumentException)
            {
                // expected
            }
        }

        private EventBean[] Make(int size)
        {
            EventBean[] events = new EventBean[size];
            for (int i = 0; i < events.Length; i++)
            {
                events[i] = SupportEventBeanFactory.CreateObject(supportEventTypeFactory, new SupportBean_S0(eventId++));
            }
            return events;
        }
    }
} // end of namespace

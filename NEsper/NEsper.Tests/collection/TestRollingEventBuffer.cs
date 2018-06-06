///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.supportunit.bean;
using com.espertech.esper.supportunit.events;

using NUnit.Framework;

namespace com.espertech.esper.collection
{
    [TestFixture]
    public class TestRollingEventBuffer 
    {
        private static int _eventId;

        private RollingEventBuffer _bufferOne;
        private RollingEventBuffer _bufferTwo;
        private RollingEventBuffer _bufferFive;
    
        [SetUp]
        public void SetUp()
        {
            _bufferOne = new RollingEventBuffer(1);
            _bufferTwo = new RollingEventBuffer(2);
            _bufferFive = new RollingEventBuffer(5);
        }
    
        [Test]
        public void TestFlowSizeOne()
        {
            _bufferOne.Add((EventBean[])null);
            Assert.IsNull(_bufferOne.Get(0));
    
            EventBean[] set1 = Make(2);
            _bufferOne.Add(set1);
            Assert.AreSame(set1[1], _bufferOne.Get(0));
            TryInvalid(_bufferOne, 1);
    
            EventBean[] set2 = Make(1);
            _bufferOne.Add(set2);
            Assert.AreSame(set2[0], _bufferOne.Get(0));
            TryInvalid(_bufferOne, 1);
        }
    
        [Test]
        public void TestFlowSizeTwo()
        {
            EventBean[] set1 = Make(2);
            _bufferTwo.Add(set1);
            AssertEvents(new [] {set1[1], set1[0]}, _bufferTwo);
    
            EventBean[] set2 = Make(1);
            _bufferTwo.Add(set2);
            AssertEvents(new [] {set2[0], set1[1]}, _bufferTwo);
    
            EventBean[] set3 = Make(1);
            _bufferTwo.Add(set3);
            AssertEvents(new [] {set3[0], set2[0]}, _bufferTwo);
    
            EventBean[] set4 = Make(3);
            _bufferTwo.Add(set4);
            AssertEvents(new [] {set4[2], set4[1]}, _bufferTwo);
    
            EventBean[] set5 = Make(5);
            _bufferTwo.Add(set5);
            AssertEvents(new [] {set5[4], set5[3]}, _bufferTwo);
    
            EventBean[] set6 = Make(1);
            _bufferTwo.Add(set6);
            AssertEvents(new [] {set6[0], set5[4]}, _bufferTwo);
            _bufferTwo.Add(Make(0));
            AssertEvents(new [] {set6[0], set5[4]}, _bufferTwo);
    
            EventBean[] set7 = Make(2);
            _bufferTwo.Add(set7);
            AssertEvents(new [] {set7[1], set7[0]}, _bufferTwo);
        }
    
        [Test]
        public void TestFlowSizeTen()
        {
            EventBean[] set1 = Make(3);
            _bufferFive.Add(set1);
            AssertEvents(new [] {set1[2], set1[1], set1[0], null, null}, _bufferFive);
    
            EventBean[] set2 = Make(1);
            _bufferFive.Add(set2);
            AssertEvents(new [] {set2[0], set1[2], set1[1], set1[0], null}, _bufferFive);
    
            EventBean[] set3 = Make(3);
            _bufferFive.Add(set3);
            AssertEvents(new [] {set3[2], set3[1], set3[0], set2[0], set1[2]}, _bufferFive);
    
            EventBean[] set4 = Make(5);
            _bufferFive.Add(set4);
            AssertEvents(new [] {set4[4], set4[3], set4[2], set4[1], set4[0]}, _bufferFive);
    
            EventBean[] set5 = Make(8);
            _bufferFive.Add(set5);
            AssertEvents(new [] {set5[7], set5[6], set5[5], set5[4], set5[3]}, _bufferFive);
    
            EventBean[] set6 = Make(2);
            _bufferFive.Add(set6);
            AssertEvents(new [] {set6[1], set6[0], set5[7], set5[6], set5[5]}, _bufferFive);
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
                events[i] = SupportEventBeanFactory.CreateObject(new SupportBean_S0(_eventId++));
            }
            return events;
        }
    }
}

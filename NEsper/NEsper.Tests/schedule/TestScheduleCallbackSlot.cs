///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using NUnit.Framework;

namespace com.espertech.esper.schedule
{
    [TestFixture]
    public class TestScheduleCallbackSlot 
    {
        [Test]
        public void TestCompare()
        {
            ScheduleSlot[] slots = new ScheduleSlot[10];
            slots[0] = new ScheduleSlot(1, 1);
            slots[1] = new ScheduleSlot(1, 2);
            slots[2] = new ScheduleSlot(2, 1);
            slots[3] = new ScheduleSlot(2, 2);
    
            Assert.AreEqual(-1, slots[0].CompareTo(slots[1]));
            Assert.AreEqual(1, slots[1].CompareTo(slots[0]));
            Assert.AreEqual(0, slots[0].CompareTo(slots[0]));
    
            Assert.AreEqual(-1, slots[0].CompareTo(slots[2]));
            Assert.AreEqual(-1, slots[1].CompareTo(slots[2]));
            Assert.AreEqual(1, slots[2].CompareTo(slots[0]));
            Assert.AreEqual(1, slots[2].CompareTo(slots[1]));
        }
    }
}

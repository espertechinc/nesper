///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
            long[] slots = new long[10];
            slots[0] = ScheduleBucket.ToLong(1, 1);
            slots[1] = ScheduleBucket.ToLong(1, 2);
            slots[2] = ScheduleBucket.ToLong(2, 1);
            slots[3] = ScheduleBucket.ToLong(2, 2);
    
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

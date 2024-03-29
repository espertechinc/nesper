///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace com.espertech.esper.common.@internal.schedule
{
    [TestFixture]
    public class TestScheduleCallbackSlot : AbstractCommonTest
    {
        [Test]
        public void TestCompare()
        {
            long[] slots = new long[10];
            slots[0] = ScheduleBucket.ToLong(1, 1);
            slots[1] = ScheduleBucket.ToLong(1, 2);
            slots[2] = ScheduleBucket.ToLong(2, 1);
            slots[3] = ScheduleBucket.ToLong(2, 2);

            ClassicAssert.AreEqual(-1, Compare(slots[0], slots[1]));
            ClassicAssert.AreEqual(1, Compare(slots[1], slots[0]));
            ClassicAssert.AreEqual(0, Compare(slots[0], slots[0]));

            ClassicAssert.AreEqual(-1, Compare(slots[0], slots[2]));
            ClassicAssert.AreEqual(-1, Compare(slots[1], slots[2]));
            ClassicAssert.AreEqual(1, Compare(slots[2], slots[0]));
            ClassicAssert.AreEqual(1, Compare(slots[2], slots[1]));
        }

        private int Compare(
            long first,
            long second)
        {
            return first.CompareTo(second);
        }
    }
} // end of namespace

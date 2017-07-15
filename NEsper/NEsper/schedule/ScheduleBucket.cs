///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.schedule
{
    /// <summary>
    /// This class acts as a buckets for sorting schedule service callbacks that are scheduled to occur at the same
    /// time. Each buckets constists of slots that callbacks are assigned to.
    /// <para>
    /// At the time of timer evaluation, callbacks that become triggerable are ordered using the bucket
    /// as the first-level order, and slot as the second-level order.
    /// </para>
    /// <para>
    /// Each statement at statement creation time allocates a buckets, and each timer within the
    /// statement allocates a slot. Thus statements that depend on other statements (such as for insert-into),
    /// and timers within their statement (such as time window or output rate limit timers) behave
    /// deterministically.
    /// </para>
    /// </summary>
    public class ScheduleBucket {
        private readonly int bucketNum;
        private int lastSlot;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="bucketNum">is a simple integer number for this bucket by which buckets can be sorted</param>
        public ScheduleBucket(int bucketNum) {
            this.bucketNum = bucketNum;
            lastSlot = 0;
        }
    
        public static long ToLong(int bucket, int slot) {
            return ((long) bucket << 32) | slot & 0xFFFFFFFFL;
        }
    
        public long AllocateSlot() {
            return ToLong(bucketNum, lastSlot++);
        }
    
        public long AllocateSlot(int slotNumber) {
            return ToLong(bucketNum, slotNumber);
        }
    }
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.schedule
{
    /// <summary>
    /// This class acts as a buckets for sorting schedule service callbacks that are scheduled 
    /// to occur at the same time. Each buckets constists of <seealso cref="ScheduleSlot" /> 
    /// slots that callbacks are assigned to. 
    /// <para/> 
    /// At the time of timer evaluation, callbacks that become triggerable are ordered using the 
    /// bucket as the first-level order, and slot as the second-level order. 
    /// <para/> 
    /// Each statement at statement creation time allocates a buckets, and each timer within the 
    /// statement allocates a slot. Thus statements that depend on other statements (such as for 
    /// insert-into), and timers within their statement (such as time window or output rate limit 
    /// timers) behave deterministically. 
    /// </summary>
    public class ScheduleBucket
    {
        private readonly int _bucketNum;
        private int _lastSlot;
    
        /// <summary>Ctor. </summary>
        /// <param name="bucketNum">is a simple integer number for this bucket by which buckets can be sorted</param>
        public ScheduleBucket(int bucketNum)
        {
            _bucketNum = bucketNum;
            _lastSlot = 0;
        }
    
        /// <summary>Restart bucket slot numbering wuch as when a statement is restarted and new slots are allocated. </summary>
        public void Restart()
        {
            _lastSlot = 0;
        }
    
        /// <summary>Returns a new slot in the bucket. </summary>
        /// <returns>slot</returns>
        public ScheduleSlot AllocateSlot()
        {
            return new ScheduleSlot(_bucketNum, _lastSlot++);
        }
    
        /// <summary>Returns a new slot in the bucket, given a slot number </summary>
        /// <returns>slot</returns>
        public ScheduleSlot AllocateSlot(int slotNumber)
        {
            return new ScheduleSlot(_bucketNum, slotNumber);
        }
    }
}

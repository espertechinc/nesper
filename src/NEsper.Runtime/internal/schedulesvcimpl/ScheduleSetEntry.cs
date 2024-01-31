///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.runtime.@internal.schedulesvcimpl
{
    /// <summary>Record for a schedule item. </summary>
    public class ScheduleSetEntry
    {
        /// <summary>Ctor. </summary>
        /// <param name="time">of schedule</param>
        /// <param name="scheduleSlot">slot</param>
        /// <param name="handle">handle to use</param>
        public ScheduleSetEntry(
            long time,
            long scheduleSlot,
            ScheduleHandle handle)
        {
            Time = time;
            ScheduleSlot = scheduleSlot;
            Handle = handle;
        }

        /// <summary>Gets or sets the time. </summary>
        /// <value>time</value>
        public long Time { get; set; }

        /// <summary>Returns schedule slot. </summary>
        /// <value>slot</value>
        public long ScheduleSlot { get; }

        /// <summary>Returns the schedule handle. </summary>
        /// <value>handle</value>
        public ScheduleHandle Handle { get; }
    }
}
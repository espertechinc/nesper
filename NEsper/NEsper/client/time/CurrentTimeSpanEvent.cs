///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.time
{
    /// <summary>
    /// Event for externally controlling the time within an 
    /// <seealso cref="com.espertech.esper.client.EPRuntime"/> or
    /// <seealso cref="com.espertech.esper.client.EPRuntimeIsolated" /> instance, 
    /// advancing time over a span of time.
    /// <para/>
    /// The engine advances time according to the resolution passed in, completing at 
    /// the target time provided. 
    /// <para/>
    /// When used without a resolution or with a negative or zero value for resolution 
    /// the engine advances time according to any statement schedules that may be present. 
    /// If no statement schedules are present, the engine simply advances time to the 
    /// target time provided.
    /// <para/>
    /// External clocking must be enabled via <seealso cref="TimerControlEvent" /> before 
    /// this class can be used to externally feed time.
    /// </summary>
    [Serializable]
    public sealed class CurrentTimeSpanEvent
        : TimerEvent
    {
        /// <summary>
        /// Constructor taking only a target time to advance to. 
        /// <para/>
        /// Use this constructor to have the engine decide the resolution at which time advances, 
        /// according to present statement schedules.
        /// </summary>
        /// <param name="targetTimeInMillis">target time</param>
        public CurrentTimeSpanEvent(long targetTimeInMillis)
        {
            TargetTimeInMillis = targetTimeInMillis;
        }

        /// <summary>
        /// Constructor taking a target time to advance to and a resoultion to use to advance time.
        /// <para/>
        /// Use this constructor to dictate a resolution at which time advances.
        /// </summary>
        /// <param name="targetTimeInMillis">target time</param>
        /// <param name="optionalResolution">should be a positive value</param>
        public CurrentTimeSpanEvent(long targetTimeInMillis, long optionalResolution)
        {
            TargetTimeInMillis = targetTimeInMillis;
            OptionalResolution = optionalResolution;
        }

        /// <summary>Returns the target time to advance engine time to. </summary>
        /// <value>target time</value>
        public long TargetTimeInMillis { get; set; }

        /// <summary>Returns the resolution for advancing time, or null if none provided. </summary>
        /// <value>resolution</value>
        public long? OptionalResolution { get; set; }
    }
}

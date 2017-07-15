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

namespace com.espertech.esper.timer
{
    /// <summary>
    /// Allow for different strategies for getting VM (wall clock) time.
    /// See JIRA issue ESPER-191 Support nano/microsecond resolution for more
    /// information on Java system time-call performance, accuracy and drift.
    /// </summary>
    /// <author>Jerry Shea</author>
    public class TimeSourceServiceImpl : TimeSourceService {
        private static readonly long MICROS_TO_MILLIS = 1000;
        private static readonly long NANOS_TO_MICROS = 1000;
    
        /// <summary>
        /// A public variable indicating whether to use the System millisecond time or
        /// nano time, to be configured through the engine settings.
        /// </summary>
        public static bool isSystemCurrentTime = true;
    
        private readonly long wallClockOffset;
        private readonly string description;
    
        /// <summary>Ctor.</summary>
        public TimeSourceServiceImpl() {
            this.wallClockOffset = System.CurrentTimeMillis() * MICROS_TO_MILLIS - this.TimeMicros;
            this.description = string.Format("%s: resolution %d microsecs",
                    this.GetType().Name, this.CalculateResolution());
        }
    
        /// <summary>
        /// Convenience method to get time in milliseconds
        /// </summary>
        /// <returns>wall-clock time in milliseconds</returns>
        public long GetTimeMillis() {
            if (isSystemCurrentTime) {
                return System.CurrentTimeMillis();
            }
            return GetTimeMicros() / MICROS_TO_MILLIS;
        }
    
        private long GetTimeMicros() {
            return (System.NanoTime() / NANOS_TO_MICROS) + wallClockOffset;
        }
    
    
        /// <summary>
        /// Calculate resolution of this timer in microseconds i.e. what is the resolution
        /// of the underlying platform's timer.
        /// </summary>
        /// <returns>timer resolution</returns>
        protected long CalculateResolution() {
            int loops = 5;
            long totalResolution = 0;
            long time = this.TimeMicros, prevTime = time;
            for (int i = 0; i < loops; i++) {
                // wait until time changes
                while (time == prevTime)
                    time = this.TimeMicros;
                totalResolution += time - prevTime;
                prevTime = time;
            }
            return totalResolution / loops;
        }
    
        public override string ToString() {
            return description;
        }
    }
} // end of namespace

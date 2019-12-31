///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.timer
{
    /// <summary>
    /// Allow for different strategies for getting VM (wall clock) time.
    /// See JIRA issue ESPER-191 Support nano/microsecond resolution for more
    /// information on Java system time-call performance, accuracy and drift.
    /// </summary>
    /// <author>Jerry Shea</author>
    public class TimeSourceServiceImpl : TimeSourceService
    {
        private const long MICROS_TO_MILLIS = 1000;
        private const long NANOS_TO_MICROS = 1000;

        /// <summary>
        /// A public variable indicating whether to use the System millisecond time or
        /// nano time, to be configured through the engine settings.
        /// </summary>
        public static bool IsSystemCurrentTime = true;

        private readonly long _wallClockOffset;
        private readonly string _description;

        /// <summary>Ctor.</summary>
        public TimeSourceServiceImpl()
        {
            _wallClockOffset = DateTimeHelper.CurrentTimeMillis * MICROS_TO_MILLIS - TimeMicros;
            _description = string.Format("{0}: resolution {1} microsecs", GetType().FullName, CalculateResolution());
        }

        /// <summary>
        /// Convenience method to get time in milliseconds
        /// </summary>
        /// <value>wall-clock time in milliseconds</value>
        public long TimeMillis {
            get {
                if (IsSystemCurrentTime) {
                    return DateTimeHelper.CurrentTimeMillis;
                }

                return TimeMicros / MICROS_TO_MILLIS;
            }
        }

        private long TimeMicros {
            get { return (DateTimeHelper.CurrentTimeNanos / NANOS_TO_MICROS) + _wallClockOffset; }
        }

        /// <summary>
        /// Calculate resolution of this timer in microseconds i.e. what is the resolution
        /// of the underlying platform's timer.
        /// </summary>
        /// <returns>timer resolution</returns>
        protected long CalculateResolution()
        {
            const int loops = 5;
            long totalResolution = 0;
            long time = this.TimeMicros, prevTime = time;
            for (int i = 0; i < loops; i++)
            {
                // wait until time changes
                while (time == prevTime)
                {
                    time = TimeMicros;
                }

                totalResolution += time - prevTime;
                prevTime = time;
            }
            return totalResolution / loops;
        }

        public override string ToString()
        {
            return _description;
        }
    }
} // end of namespace
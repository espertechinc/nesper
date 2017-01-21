///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.timer
{
    /// <summary>
    /// Allow for different strategies for getting VM (wall clock) time. See JIRA issue
    /// ESPER-191 Support nano/microsecond resolution for more information on system
    /// time-call performance, accuracy and drift.
    /// </summary>
    public class TimeSourceServiceImpl : TimeSourceService
    {
    	private const long MICROS_TO_MILLIS = 1000;
    	private const long NANOS_TO_MICROS = 1000;
    
        /// <summary>
        /// A public variable indicating whether to use the System millisecond time or nano
        /// time, to be configured through the engine settings.
        /// </summary>
        public static bool IS_SYSTEM_CURRENT_TIME = true;
    
        private readonly long _wallClockOffset;
        private readonly string _description;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        public TimeSourceServiceImpl()
        {
            _wallClockOffset = DateTimeHelper.CurrentTimeMillis * MICROS_TO_MILLIS - GetTimeMicros();
            _description = String.Format("{0}: resolution {1} microsecs",
                               GetType().FullName, CalculateResolution());
        }

        /// <summary>
        /// Get time in milliseconds.
        /// </summary>
        /// <returns>wall-clock time in milliseconds</returns>
        public long GetTimeMillis()
        {
            if (IS_SYSTEM_CURRENT_TIME)
            {
                return DateTimeHelper.CurrentTimeMillis;
            }
            return GetTimeMicros() / MICROS_TO_MILLIS;
        }

        /// <summary>
        /// Get time in microseconds.
        /// </summary>
        /// <returns></returns>
        private long GetTimeMicros()
        {
            return (DateTimeHelper.CurrentTimeNanos / NANOS_TO_MICROS) + _wallClockOffset;
        }


        /// <summary>
        /// Calculate resolution of this timer in microseconds i.e. what is the resolution
        /// of the underlying platform's timer.
        /// </summary>
        /// <returns>
        /// timer resolution
        /// </returns>
    	protected long CalculateResolution()
        {
    		const int LOOPS = 5;
            long totalResolution = 0;
    		long time = GetTimeMicros(), prevTime = time;
            for (int i = 0; i < LOOPS; i++) {
                // wait until time changes
                while (time == prevTime)
                    time = GetTimeMicros();
                totalResolution += (time - prevTime);
    			prevTime = time;
            }
    		return totalResolution / LOOPS;
    	}

        /// <summary>
        /// Provides a string rendering of the object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}, Description: {1}", base.ToString(), _description);
        }
    }
}

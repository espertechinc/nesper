///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    /// Service for advancing and controlling time.
    /// </summary>
    public interface EPEventServiceTimeControl
    {
        /// <summary>
        /// Advance time by jumping to the given time in milliseconds (or nanoseconds if so configured).
        /// <para />For externally controlling the time within a runtime.
        /// <para />External clocking must be first be enabled by configuration {@link com.espertech.esper.common.client.configuration.runtime.ConfigurationRuntimeThreading#setInternalTimerEnabled(boolean)} passing false
        /// or by calling {@link #clockExternal()}.
        /// <para />Time should never move backwards (unless for testing purposes where previous results can be thrown away)
        /// </summary>
        /// <param name="time">time</param>
        void AdvanceTime(long time);

        /// <summary>
        /// Advance time by continually-sliding to the given time in milliseconds (or nanoseconds if so configured) at the smallest resolution (non-hopping).
        /// <para />For externally controlling the time within a runtime.
        /// <para />External clocking must be first be enabled by configuration {@link com.espertech.esper.common.client.configuration.runtime.ConfigurationRuntimeThreading#setInternalTimerEnabled(boolean)} passing false
        /// or by calling {@link #clockExternal()}.
        /// <para />Time should never move backwards (unless for testing purposes where previous results can be thrown away)
        /// </summary>
        /// <param name="time">time</param>
        void AdvanceTimeSpan(long time);

        /// <summary>
        /// Advance time by continually-sliding to the given time in milliseconds (or nanoseconds if so configured) at the provided resolution (hopping).
        /// <para />For externally controlling the time within a runtime.
        /// <para />External clocking must be first be enabled by configuration {@link com.espertech.esper.common.client.configuration.runtime.ConfigurationRuntimeThreading#setInternalTimerEnabled(boolean)} passing false
        /// or by calling {@link #clockExternal()}.
        /// <para />Time should never move backwards (unless for testing purposes where previous results can be thrown away)
        /// </summary>
        /// <param name="time">time</param>
        /// <param name="resolution">the resolution to use</param>
        void AdvanceTimeSpan(long time, long resolution);

        /// <summary>
        /// Returns current engine time.
        /// <para />If time is provided externally via timer events, the function returns current time as externally provided.
        /// </summary>
        /// <value>current engine time</value>
        long CurrentTime { get; }

        /// <summary>
        /// Returns the time at which the next schedule execution is expected, returns null if no schedule execution is
        /// outstanding.
        /// </summary>
        /// <value>time of next schedule if any</value>
        long? NextScheduledTime { get; }

        /// <summary>
        /// Switches on the internal timer which tracks system time. There is no effect if the runtime is already
        /// on internal time.
        /// <para />Your application may not want to use {@link #advanceTime(long)}, {@link #advanceTimeSpan(long)} or {@link #advanceTimeSpan(long, long)}
        /// after calling this method, since time advances according to JVM time.
        /// </summary>
        void ClockInternal();

        /// <summary>
        /// Switches off the internal timer which tracks system time. There is no effect if the runtime is already
        /// on external internal time.
        /// <para />Your application may want to use {@link #advanceTime(long)}, {@link #advanceTimeSpan(long)} or {@link #advanceTimeSpan(long, long)}
        /// after calling this method to set or advance time.
        /// <para />Its generally preferable to turn off internal clocking (and thus turn on external clocking) by configuration {@link com.espertech.esper.common.client.configuration.runtime.ConfigurationRuntimeThreading#setInternalTimerEnabled(boolean)} passing false.
        /// </summary>
        void ClockExternal();

        /// <summary>
        /// Returns true for external clocking, false for internal clocking.
        /// </summary>
        /// <returns>clocking indicator</returns>
        bool IsExternalClockingEnabled();
    }
} // end of namespace
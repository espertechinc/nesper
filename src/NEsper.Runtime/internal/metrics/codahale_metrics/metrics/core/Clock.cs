///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// An abstraction for how time passes. It is passed to <seealso cref="Timer" /> to track timing.
    /// </summary>
    public abstract partial class Clock
    {
        private static readonly Clock DEFAULT = new UserTimeClock();

        /// <summary>
        /// Returns the current time tick.
        /// </summary>
        /// <value>time tick in nanoseconds</value>
        public abstract long Tick { get; }

        /// <summary>
	    /// Returns the current time in milliseconds.
	    /// </summary>
	    /// <returns>time in milliseconds</returns>
	    public long Time()
        {
            return DateTimeHelper.CurrentTimeMillis;
        }

        /// <summary>
        /// The default clock to use.
        /// </summary>
        /// <value>the default <seealso cref="Clock" /> instance
        /// </value>
        public static Clock DefaultClock {
            get { return DEFAULT; }
        }
    }
} // end of namespace
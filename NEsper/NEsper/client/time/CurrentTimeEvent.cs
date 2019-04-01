///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.client.time
{
    /// <summary>
    /// Event for externally controlling the TimeInMillis within an <seealso cref="com.espertech.esper.client.EPRuntime" />
    /// or <seealso cref="com.espertech.esper.client.EPRuntimeIsolated" /> instance.
    /// External clocking must be enabled via <seealso cref="TimerControlEvent" /> before this class can be used
    /// to externally feed TimeInMillis.
    /// </summary>
    public sealed class CurrentTimeEvent : TimerEvent
    {
        /// <summary>
        /// Returns the TimeInMillis in milliseconds.
        /// </summary>
        /// <value>TimeInMillis in milliseconds</value>
        public long Time { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeEvent"/> class.
        /// </summary>
        /// <param name="time">The time in millis.</param>
        public CurrentTimeEvent(long time)
        {
            Time = time;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeEvent"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public CurrentTimeEvent(DateTime dateTime)
        {
            Time = (new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime))).TimeInMillis();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeEvent"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public CurrentTimeEvent(DateTimeOffset dateTime)
        {
            Time = dateTime.TimeInMillis();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override String ToString()
        {
            return DateTimeOffsetHelper.TimeFromMillis(Time, TimeZoneInfo.Utc).ToString();
        }
    }
} // end of namespace

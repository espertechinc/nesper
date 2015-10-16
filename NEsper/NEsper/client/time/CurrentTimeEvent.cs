///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
    /// Event for externally controlling the time within an <see cref="com.espertech.esper.client.EPRuntime"/> instance.
    /// External clocking must be enabled via <see cref="TimerControlEvent"/> before this class can be used
    /// to externally feed time.
    /// </summary>
	
	[Serializable]
    public sealed class CurrentTimeEvent : TimerEvent
	{
	    /// <summary> Returns the time in milliseconds.</summary>
	    /// <returns> time in milliseconds
	    /// </returns>
	    public long TimeInMillis { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeEvent"/> class.
        /// </summary>
        /// <param name="timeInMillis">The time in millis.</param>
		public CurrentTimeEvent(long timeInMillis)
		{
			TimeInMillis = timeInMillis;
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeEvent"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public CurrentTimeEvent(DateTime dateTime)
        {
            TimeInMillis = (new DateTimeOffset(dateTime, TimeZoneInfo.Local.GetUtcOffset(dateTime))).TimeInMillis();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeEvent"/> class.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        public CurrentTimeEvent(DateTimeOffset dateTime)
        {
            TimeInMillis = dateTime.TimeInMillis();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
		public override String ToString()
        {
            return DateTimeOffsetHelper.TimeFromMillis(TimeInMillis, TimeZoneInfo.Utc).ToString();
		}
	}
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace com.espertech.esper.type
{
	/// <summary>
	/// Enumeration of units in a specification of schedule, which contains elements for each of the following units:
	/// minute, hour, day of month, month, day of week and seconds.
	/// Notice: value ranges are the same as the "crontab" standard values.
	/// </summary>
    [Serializable]
    public class ScheduleUnit
    {
        /// <summary> Second.</summary>
        public static readonly ScheduleUnit SECONDS = new ScheduleUnit(0, 59);
        /// <summary> Minute.</summary>
        public static readonly ScheduleUnit MINUTES = new ScheduleUnit(0, 59);
        /// <summary> Hour.</summary>
        public static readonly ScheduleUnit HOURS = new ScheduleUnit(0, 23);
        /// <summary> Day of month.</summary>
        public static readonly ScheduleUnit DAYS_OF_MONTH = new ScheduleUnit(1, 31);
        /// <summary> Month.</summary>
        public static readonly ScheduleUnit MONTHS = new ScheduleUnit(1, 12);
        /// <summary> Day of week.</summary>
        public static readonly ScheduleUnit DAYS_OF_WEEK = new ScheduleUnit(0, 6);

        /// <summary>
        /// Available constant values from this class
        /// </summary>
        
        public static readonly ScheduleUnit[] Values = new ScheduleUnit[]
        {
        	SECONDS,
        	MINUTES,
        	HOURS,
        	DAYS_OF_MONTH,
        	MONTHS,
        	DAYS_OF_WEEK
        };
        
        private readonly int min_;
        private readonly int max_;

        ScheduleUnit(int min, int max)
        {
            this.min_ = min;
            this.max_ = max;
        }

        /// <summary> Returns minimum valid value for the unit.</summary>
        /// <returns> minimum unit value
        /// </returns>
        public int Min()
        {
            return min_;
        }

        /// <summary> Returns minimum valid value for the unit.</summary>
        /// <returns> maximum unit value
        /// </returns>
        public int Max()
        {
            return max_;
        }
    }
}

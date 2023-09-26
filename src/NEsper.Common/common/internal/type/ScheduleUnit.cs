///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.type
{
    /// <summary>
    /// Enumeration of units in a specification of schedule, which contains elements for each of the following units:
    /// minute, hour, day of month, month, day of week and seconds.
    /// Notice: value ranges are the same as the "crontab" standard values.
    /// </summary>
    [Serializable]
    public enum ScheduleUnit
    {
        /// <summary>Microsecond</summary>
        MICROSECONDS,

        /// <summary>Millisecond</summary>
        MILLISECONDS,

        /// <summary> Second.</summary>
        SECONDS,

        /// <summary> Minute.</summary>
        MINUTES,

        /// <summary> Hour.</summary>
        HOURS,

        /// <summary> Day of month.</summary>
        DAYS_OF_MONTH,

        /// <summary> Month.</summary>
        MONTHS,

        /// <summary> Day of week.</summary>
        DAYS_OF_WEEK
    }

    public static class ScheduleUnitExtensions
    {
        /// <summary> Returns minimum valid value for the unit.</summary>
        /// <returns> minimum unit value
        /// </returns>
        public static int Min(this ScheduleUnit value)
        {
            switch (value) {
                case ScheduleUnit.MICROSECONDS:
                    return 0;

                case ScheduleUnit.MILLISECONDS:
                    return 0;

                case ScheduleUnit.SECONDS:
                    return 0;

                case ScheduleUnit.MINUTES:
                    return 0;

                case ScheduleUnit.HOURS:
                    return 0;

                case ScheduleUnit.DAYS_OF_MONTH:
                    return 1;

                case ScheduleUnit.MONTHS:
                    return 1;

                case ScheduleUnit.DAYS_OF_WEEK:
                    return 0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        /// <summary> Returns minimum valid value for the unit.</summary>
        /// <returns> maximum unit value
        /// </returns>
        public static int Max(this ScheduleUnit value)
        {
            switch (value) {
                case ScheduleUnit.MICROSECONDS:
                    return 999;

                case ScheduleUnit.MILLISECONDS:
                    return 999;

                case ScheduleUnit.SECONDS:
                    return 59;

                case ScheduleUnit.MINUTES:
                    return 59;

                case ScheduleUnit.HOURS:
                    return 23;

                case ScheduleUnit.DAYS_OF_MONTH:
                    return 31;

                case ScheduleUnit.MONTHS:
                    return 12;

                case ScheduleUnit.DAYS_OF_WEEK:
                    return 6;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
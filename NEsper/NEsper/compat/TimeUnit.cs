///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    public enum TimeUnit
    {
        DAYS,
        HOURS,
        MINUTES,
        SECONDS,
        MILLISECONDS,
        MICROSECONDS,
        NANOSECONDS
    }

    public static class TimeUnitHelper
    {
        public static TimeSpan ToTimeSpan(int units, TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return TimeSpan.FromDays(units);
                case TimeUnit.HOURS:
                    return TimeSpan.FromHours(units);
                case TimeUnit.MILLISECONDS:
                    return TimeSpan.FromMilliseconds(units);
                case TimeUnit.MINUTES:
                    return TimeSpan.FromMinutes(units);
                case TimeUnit.SECONDS:
                    return TimeSpan.FromSeconds(units);
                case TimeUnit.MICROSECONDS:
                    return TimeSpan.FromTicks(DateTimeHelper.MicrosToTicks(units));
                case TimeUnit.NANOSECONDS:
                    throw new ArgumentException("unsupported value for unit");
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }
    }
}

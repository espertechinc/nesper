///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        public static TimeSpan ToTimeSpan(long units, TimeUnit unit)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return TimeSpan.FromDays(units);
                case TimeUnit.HOURS:
                    return TimeSpan.FromHours(units);
                case TimeUnit.MINUTES:
                    return TimeSpan.FromMinutes(units);
                case TimeUnit.SECONDS:
                    return TimeSpan.FromSeconds(units);
                case TimeUnit.MILLISECONDS:
                    return TimeSpan.FromMilliseconds(units);
                case TimeUnit.MICROSECONDS:
                    return TimeSpan.FromTicks(DateTimeHelper.MicrosToTicks(units));
                case TimeUnit.NANOSECONDS:
                    return TimeSpan.FromTicks(DateTimeHelper.NanosToTicks(units));
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long Convert(
            long sourceUnits,
            TimeUnit sourceUnit,
            TimeUnit targetUnit)
        {
            switch (targetUnit)
            {
                case TimeUnit.DAYS:
                    return ToDays(sourceUnit, sourceUnits);
                case TimeUnit.HOURS:
                    return ToHours(sourceUnit, sourceUnits);
                case TimeUnit.MINUTES:
                    return ToMinutes(sourceUnit, sourceUnits);
                case TimeUnit.SECONDS:
                    return ToSeconds(sourceUnit, sourceUnits);
                case TimeUnit.MILLISECONDS:
                    return ToMillis(sourceUnit, sourceUnits);
                case TimeUnit.MICROSECONDS:
                    return ToMicros(sourceUnit, sourceUnits);
                case TimeUnit.NANOSECONDS:
                    return ToNanos(sourceUnit, sourceUnits);
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToDays(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units;
                case TimeUnit.HOURS:
                    return units / 24L;
                case TimeUnit.MINUTES:
                    return units / 24L / 60L;
                case TimeUnit.SECONDS:
                    return units / 24L / 60L / 60L;
                case TimeUnit.MILLISECONDS:
                    return units / 24L / 60L / 60L / 1000L;
                case TimeUnit.MICROSECONDS:
                    return units / 24L / 60L / 60L / 1000L / 1000L;
                case TimeUnit.NANOSECONDS:
                    return units / 24L / 60L / 60L / 1000L / 1000L / 1000L;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToHours(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units * 24L;
                case TimeUnit.HOURS:
                    return units;
                case TimeUnit.MINUTES:
                    return units / 60L;
                case TimeUnit.SECONDS:
                    return units / 60L / 60L;
                case TimeUnit.MILLISECONDS:
                    return units / 60L / 60L / 1000L;
                case TimeUnit.MICROSECONDS:
                    return units / 60L / 60L / 1000L / 1000L;
                case TimeUnit.NANOSECONDS:
                    return units / 60L / 60L / 1000L / 1000L / 1000L;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToMinutes(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units * 60L * 24L;
                case TimeUnit.HOURS:
                    return units * 60L;
                case TimeUnit.MINUTES:
                    return units;
                case TimeUnit.SECONDS:
                    return units / 60L;
                case TimeUnit.MILLISECONDS:
                    return units / 60L / 1000L;
                case TimeUnit.MICROSECONDS:
                    return units / 60L / 1000L / 1000L;
                case TimeUnit.NANOSECONDS:
                    return units / 60L / 1000L / 1000L / 1000L;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToSeconds(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units * 60L * 60L * 24L;
                case TimeUnit.HOURS:
                    return units * 60L * 60L;
                case TimeUnit.MINUTES:
                    return units * 60L;
                case TimeUnit.SECONDS:
                    return units;
                case TimeUnit.MILLISECONDS:
                    return units / 1000L;
                case TimeUnit.MICROSECONDS:
                    return units / 1000L / 1000L;
                case TimeUnit.NANOSECONDS:
                    return units / 1000L / 1000L / 1000L;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToMillis(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units * 1000L * 60L * 60L * 24L;
                case TimeUnit.HOURS:
                    return units * 1000L * 60L * 60L;
                case TimeUnit.MINUTES:
                    return units * 1000L * 60L;
                case TimeUnit.SECONDS:
                    return units * 1000L;
                case TimeUnit.MILLISECONDS:
                    return units * 1000L;
                case TimeUnit.MICROSECONDS:
                    return units / 1000L;
                case TimeUnit.NANOSECONDS:
                    return units / 1000L  / 1000L;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToMicros(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units * 1000L * 1000L * 60L * 60L * 24L;
                case TimeUnit.HOURS:
                    return units * 1000L * 1000L * 60L * 60L;
                case TimeUnit.MINUTES:
                    return units * 1000L * 1000L * 60L;
                case TimeUnit.SECONDS:
                    return units * 1000L * 1000L;
                case TimeUnit.MILLISECONDS:
                    return units * 1000L;
                case TimeUnit.MICROSECONDS:
                    return units;
                case TimeUnit.NANOSECONDS:
                    return units / 1000L;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }

        public static long ToNanos(this TimeUnit unit, long units)
        {
            switch (unit)
            {
                case TimeUnit.DAYS:
                    return units * 1000L * 1000L * 1000L * 60L * 60L * 24L;
                case TimeUnit.HOURS:
                    return units * 1000L * 1000L * 1000L * 60L * 60L;
                case TimeUnit.MINUTES:
                    return units * 1000L * 1000L * 1000L * 60L;
                case TimeUnit.SECONDS:
                    return units * 1000L * 1000L * 1000L;
                case TimeUnit.MILLISECONDS:
                    return units * 1000L * 1000L;
                case TimeUnit.MICROSECONDS:
                    return units * 1000L;
                case TimeUnit.NANOSECONDS:
                    return units;
                default:
                    throw new ArgumentException("invalid value for unit");
            }
        }
    }
}

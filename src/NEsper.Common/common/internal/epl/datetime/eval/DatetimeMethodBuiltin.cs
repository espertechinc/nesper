///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public enum DatetimeMethodBuiltin
    {
        // calendar op
        WITHTIME,
        WITHDATE,
        PLUS,
        MINUS,
        WITHMAX,
        WITHMIN,
        SET,
        ROUNDCEILING,
        ROUNDFLOOR,
        ROUNDHALF,

        // reformat op
        GET,
        FORMAT,
        TODATETIMEEX,
        TODATETIMEOFFSET,
        TODATETIME,
        TOMILLISEC,
        GETMINUTEOFHOUR,
        GETMONTHOFYEAR,
        GETDAYOFMONTH,
        GETDAYOFWEEK,
        GETDAYOFYEAR,
        GETERA,
        GETHOUROFDAY,
        GETMILLISOFSECOND,
        GETSECONDOFMINUTE,
        GETWEEKYEAR,
        GETYEAR,
        BETWEEN,

        // interval op
        BEFORE,
        AFTER,
        COINCIDES,
        DURING,
        INCLUDES,
        FINISHES,
        FINISHEDBY,
        MEETS,
        METBY,
        OVERLAPS,
        OVERLAPPEDBY,
        STARTS,
        STARTEDBY
    }

    public static class DatetimeMethodBuiltinExtensions
    {
        public static DatetimeMethodProviderForgeFactory GetForgeFactory(this DatetimeMethodBuiltin value)
        {
            return value switch {
                DatetimeMethodBuiltin.WITHTIME => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.WITHDATE => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.PLUS => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.MINUS => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.WITHMAX => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.WITHMIN => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.SET => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.ROUNDCEILING => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.ROUNDFLOOR => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.ROUNDHALF => DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY,
                DatetimeMethodBuiltin.GET => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.FORMAT => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.TODATETIMEEX => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.TODATETIMEOFFSET => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.TODATETIME => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.TOMILLISEC => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETMINUTEOFHOUR => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETMONTHOFYEAR => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETDAYOFMONTH => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETDAYOFWEEK => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETDAYOFYEAR => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETERA => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETHOUROFDAY => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETMILLISOFSECOND => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETSECONDOFMINUTE => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETWEEKYEAR => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.GETYEAR => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.BETWEEN => DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY,
                DatetimeMethodBuiltin.BEFORE => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.AFTER => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.COINCIDES => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.DURING => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.INCLUDES => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.FINISHES => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.FINISHEDBY => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.MEETS => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.METBY => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.OVERLAPS => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.OVERLAPPEDBY => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.STARTS => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                DatetimeMethodBuiltin.STARTEDBY => DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static string GetNameCamel(this DatetimeMethodBuiltin value)
        {
            return value switch {
                DatetimeMethodBuiltin.WITHTIME => "withTime",
                DatetimeMethodBuiltin.WITHDATE => "withDate",
                DatetimeMethodBuiltin.PLUS => "plus",
                DatetimeMethodBuiltin.MINUS => "minus",
                DatetimeMethodBuiltin.WITHMAX => "withMax",
                DatetimeMethodBuiltin.WITHMIN => "withMin",
                DatetimeMethodBuiltin.SET => "set",
                DatetimeMethodBuiltin.ROUNDCEILING => "roundCeiling",
                DatetimeMethodBuiltin.ROUNDFLOOR => "roundFloor",
                DatetimeMethodBuiltin.ROUNDHALF => "roundHalf",
                DatetimeMethodBuiltin.GET => "Get",
                DatetimeMethodBuiltin.FORMAT => "format",
                DatetimeMethodBuiltin.TODATETIMEEX => "toDateTimeEx",
                DatetimeMethodBuiltin.TODATETIMEOFFSET => "toDateTimeOffset",
                DatetimeMethodBuiltin.TODATETIME => "toDateTime",
                DatetimeMethodBuiltin.TOMILLISEC => "toMillisec",
                DatetimeMethodBuiltin.GETMINUTEOFHOUR => "getMinuteOfHour",
                DatetimeMethodBuiltin.GETMONTHOFYEAR => "getMonthOfYear",
                DatetimeMethodBuiltin.GETDAYOFMONTH => "getDayOfMonth",
                DatetimeMethodBuiltin.GETDAYOFWEEK => "getDayOfWeek",
                DatetimeMethodBuiltin.GETDAYOFYEAR => "getDayOfYear",
                DatetimeMethodBuiltin.GETERA => "getEra",
                DatetimeMethodBuiltin.GETHOUROFDAY => "getHourOfDay",
                DatetimeMethodBuiltin.GETMILLISOFSECOND => "getMillisOfSecond",
                DatetimeMethodBuiltin.GETSECONDOFMINUTE => "getSecondOfMinute",
                DatetimeMethodBuiltin.GETWEEKYEAR => "getWeekyear",
                DatetimeMethodBuiltin.GETYEAR => "getYear",
                DatetimeMethodBuiltin.BETWEEN => "between",
                DatetimeMethodBuiltin.BEFORE => "before",
                DatetimeMethodBuiltin.AFTER => "after",
                DatetimeMethodBuiltin.COINCIDES => "coincides",
                DatetimeMethodBuiltin.DURING => "during",
                DatetimeMethodBuiltin.INCLUDES => "includes",
                DatetimeMethodBuiltin.FINISHES => "finishes",
                DatetimeMethodBuiltin.FINISHEDBY => "finishedBy",
                DatetimeMethodBuiltin.MEETS => "meets",
                DatetimeMethodBuiltin.METBY => "metBy",
                DatetimeMethodBuiltin.OVERLAPS => "overlaps",
                DatetimeMethodBuiltin.OVERLAPPEDBY => "overlappedBy",
                DatetimeMethodBuiltin.STARTS => "starts",
                DatetimeMethodBuiltin.STARTEDBY => "startedBy",
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static DotMethodFP[] GetFootprints(this DatetimeMethodBuiltin value)
        {
            return value switch {
                DatetimeMethodBuiltin.WITHTIME => DatetimeMethodEnumParams.WITHTIME,
                DatetimeMethodBuiltin.WITHDATE => DatetimeMethodEnumParams.WITHDATE,
                DatetimeMethodBuiltin.PLUS => DatetimeMethodEnumParams.PLUSMINUS,
                DatetimeMethodBuiltin.MINUS => DatetimeMethodEnumParams.PLUSMINUS,
                DatetimeMethodBuiltin.WITHMAX => DatetimeMethodEnumParams.CALFIELD,
                DatetimeMethodBuiltin.WITHMIN => DatetimeMethodEnumParams.CALFIELD,
                DatetimeMethodBuiltin.SET => DatetimeMethodEnumParams.CALFIELD_PLUS_INT,
                DatetimeMethodBuiltin.ROUNDCEILING => DatetimeMethodEnumParams.CALFIELD,
                DatetimeMethodBuiltin.ROUNDFLOOR => DatetimeMethodEnumParams.CALFIELD,
                DatetimeMethodBuiltin.ROUNDHALF => DatetimeMethodEnumParams.CALFIELD,
                DatetimeMethodBuiltin.GET => DatetimeMethodEnumParams.CALFIELD,
                DatetimeMethodBuiltin.FORMAT => DatetimeMethodEnumParams.FORMAT,
                DatetimeMethodBuiltin.TODATETIMEEX => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.TODATETIMEOFFSET => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.TODATETIME => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.TOMILLISEC => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETMINUTEOFHOUR => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETMONTHOFYEAR => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETDAYOFMONTH => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETDAYOFWEEK => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETDAYOFYEAR => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETERA => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETHOUROFDAY => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETMILLISOFSECOND => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETSECONDOFMINUTE => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETWEEKYEAR => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.GETYEAR => DatetimeMethodEnumParams.NOPARAM,
                DatetimeMethodBuiltin.BETWEEN => DatetimeMethodEnumParams.BETWEEN,
                DatetimeMethodBuiltin.BEFORE => DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER,
                DatetimeMethodBuiltin.AFTER => DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER,
                DatetimeMethodBuiltin.COINCIDES => DatetimeMethodEnumParams.INTERVAL_COINCIDES,
                DatetimeMethodBuiltin.DURING => DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES,
                DatetimeMethodBuiltin.INCLUDES => DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES,
                DatetimeMethodBuiltin.FINISHES => DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY,
                DatetimeMethodBuiltin.FINISHEDBY => DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY,
                DatetimeMethodBuiltin.MEETS => DatetimeMethodEnumParams.INTERVAL_MEETS_METBY,
                DatetimeMethodBuiltin.METBY => DatetimeMethodEnumParams.INTERVAL_MEETS_METBY,
                DatetimeMethodBuiltin.OVERLAPS => DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY,
                DatetimeMethodBuiltin.OVERLAPPEDBY => DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY,
                DatetimeMethodBuiltin.STARTS => DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY,
                DatetimeMethodBuiltin.STARTEDBY => DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static DateTimeMethodEnum GetDatetimeMethod(this DatetimeMethodBuiltin value)
        {
            var nameCamel = GetNameCamel(value);
            return EnumHelper.Parse<DateTimeMethodEnum>(nameCamel);
        }

        public static DatetimeMethodDesc GetDescriptor(this DatetimeMethodBuiltin value)
        {
            return new DatetimeMethodDesc(
                GetDatetimeMethod(value),
                GetForgeFactory(value),
                GetFootprints(value));
        }
    }
} // end of namespace
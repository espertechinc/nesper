///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.methodbase;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public enum DateTimeMethodEnum
    {
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
        GETHOUROFDAY,
        GETMILLISOFSECOND,
        GETSECONDOFMINUTE,
        GETWEEKYEAR,
        GETYEAR,
        BETWEEN,
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

    public static class DatetimeMethodEnumHelper
    {
        private static IEnumerable<DateTimeMethodEnum> GetValues()
        {
            return EnumHelper.GetValues<DateTimeMethodEnum>();
        }

        public static bool IsDateTimeMethod(string name)
        {
            foreach (var e in GetValues()) {
                if (e.GetNameCamel().Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        public static DateTimeMethodEnum FromName(string name)
        {
            foreach (var e in GetValues()) {
                if (e.GetNameCamel().Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return e;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(name), name, null);
        }

        public static ForgeFactory GetForgeFactory(this DateTimeMethodEnum value)
        {
            switch (value)
            {
                case DateTimeMethodEnum.WITHTIME:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.WITHDATE:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.PLUS:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.MINUS:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.WITHMAX:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.WITHMIN:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.SET:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.ROUNDCEILING:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.ROUNDHALF:
                    return DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY;
                case DateTimeMethodEnum.GET:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.FORMAT:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.TODATETIMEEX:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.TODATETIMEOFFSET:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.TODATETIME:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.TOMILLISEC:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETMINUTEOFHOUR:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETMONTHOFYEAR:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETDAYOFMONTH:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETDAYOFWEEK:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETDAYOFYEAR:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETHOUROFDAY:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETMILLISOFSECOND:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETSECONDOFMINUTE:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETWEEKYEAR:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.GETYEAR:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.BETWEEN:
                    return DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY;
                case DateTimeMethodEnum.BEFORE:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.AFTER:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.COINCIDES:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.DURING:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.INCLUDES:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.FINISHES:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.FINISHEDBY:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.MEETS:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.METBY:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.OVERLAPS:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.OVERLAPPEDBY:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.STARTS:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                case DateTimeMethodEnum.STARTEDBY:
                    return DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static string GetNameCamel(this DateTimeMethodEnum value)
        {
            switch (value)
            {
                case DateTimeMethodEnum.WITHTIME:
                    return "withTime";
                case DateTimeMethodEnum.WITHDATE:
                    return "withDate";
                case DateTimeMethodEnum.PLUS:
                    return "plus";
                case DateTimeMethodEnum.MINUS:
                    return "minus";
                case DateTimeMethodEnum.WITHMAX:
                    return "withMax";
                case DateTimeMethodEnum.WITHMIN:
                    return "withMin";
                case DateTimeMethodEnum.SET:
                    return "set";
                case DateTimeMethodEnum.ROUNDCEILING:
                    return "roundCeiling";
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return "roundFloor";
                case DateTimeMethodEnum.ROUNDHALF:
                    return "roundHalf";
                case DateTimeMethodEnum.GET:
                    return "Get";
                case DateTimeMethodEnum.FORMAT:
                    return "format";
                case DateTimeMethodEnum.TODATETIMEEX:
                    return "toDateTimeEx";
                case DateTimeMethodEnum.TODATETIMEOFFSET:
                    return "toDateTimeOffset";
                case DateTimeMethodEnum.TODATETIME:
                    return "toDateTime";
                case DateTimeMethodEnum.TOMILLISEC:
                    return "toMillisec";
                case DateTimeMethodEnum.GETMINUTEOFHOUR:
                    return "getMinuteOfHour";
                case DateTimeMethodEnum.GETMONTHOFYEAR:
                    return "getMonthOfYear";
                case DateTimeMethodEnum.GETDAYOFMONTH:
                    return "getDayOfMonth";
                case DateTimeMethodEnum.GETDAYOFWEEK:
                    return "getDayOfWeek";
                case DateTimeMethodEnum.GETDAYOFYEAR:
                    return "getDayOfYear";
                case DateTimeMethodEnum.GETHOUROFDAY:
                    return "getHourOfDay";
                case DateTimeMethodEnum.GETMILLISOFSECOND:
                    return "getMillisOfSecond";
                case DateTimeMethodEnum.GETSECONDOFMINUTE:
                    return "getSecondOfMinute";
                case DateTimeMethodEnum.GETWEEKYEAR:
                    return "getWeekyear";
                case DateTimeMethodEnum.GETYEAR:
                    return "getYear";
                case DateTimeMethodEnum.BETWEEN:
                    return "between";
                case DateTimeMethodEnum.BEFORE:
                    return "before";
                case DateTimeMethodEnum.AFTER:
                    return "after";
                case DateTimeMethodEnum.COINCIDES:
                    return "coincides";
                case DateTimeMethodEnum.DURING:
                    return "during";
                case DateTimeMethodEnum.INCLUDES:
                    return "includes";
                case DateTimeMethodEnum.FINISHES:
                    return "finishes";
                case DateTimeMethodEnum.FINISHEDBY:
                    return "finishedBy";
                case DateTimeMethodEnum.MEETS:
                    return "meets";
                case DateTimeMethodEnum.METBY:
                    return "metBy";
                case DateTimeMethodEnum.OVERLAPS:
                    return "overlaps";
                case DateTimeMethodEnum.OVERLAPPEDBY:
                    return "overlappedBy";
                case DateTimeMethodEnum.STARTS:
                    return "starts";
                case DateTimeMethodEnum.STARTEDBY:
                    return "startedBy";
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }

        public static DotMethodFP[] GetFootprints(this DateTimeMethodEnum value)
        {
            switch (value)
            {
                case DateTimeMethodEnum.WITHTIME:
                    return DatetimeMethodEnumParams.WITHTIME;
                case DateTimeMethodEnum.WITHDATE:
                    return DatetimeMethodEnumParams.WITHDATE;
                case DateTimeMethodEnum.PLUS:
                    return DatetimeMethodEnumParams.PLUSMINUS;
                case DateTimeMethodEnum.MINUS:
                    return DatetimeMethodEnumParams.PLUSMINUS;
                case DateTimeMethodEnum.WITHMAX:
                    return DatetimeMethodEnumParams.CALFIELD;
                case DateTimeMethodEnum.WITHMIN:
                    return DatetimeMethodEnumParams.CALFIELD;
                case DateTimeMethodEnum.SET:
                    return DatetimeMethodEnumParams.CALFIELD_PLUS_INT;
                case DateTimeMethodEnum.ROUNDCEILING:
                    return DatetimeMethodEnumParams.CALFIELD;
                case DateTimeMethodEnum.ROUNDFLOOR:
                    return DatetimeMethodEnumParams.CALFIELD;
                case DateTimeMethodEnum.ROUNDHALF:
                    return DatetimeMethodEnumParams.CALFIELD;
                case DateTimeMethodEnum.GET:
                    return DatetimeMethodEnumParams.CALFIELD;
                case DateTimeMethodEnum.FORMAT:
                    return DatetimeMethodEnumParams.FORMAT;
                case DateTimeMethodEnum.TODATETIMEEX:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.TODATETIMEOFFSET:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.TODATETIME:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.TOMILLISEC:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETMINUTEOFHOUR:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETMONTHOFYEAR:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETDAYOFMONTH:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETDAYOFWEEK:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETDAYOFYEAR:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETHOUROFDAY:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETMILLISOFSECOND:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETSECONDOFMINUTE:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETWEEKYEAR:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.GETYEAR:
                    return DatetimeMethodEnumParams.NOPARAM;
                case DateTimeMethodEnum.BETWEEN:
                    return DatetimeMethodEnumParams.BETWEEN;
                case DateTimeMethodEnum.BEFORE:
                    return DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER;
                case DateTimeMethodEnum.AFTER:
                    return DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER;
                case DateTimeMethodEnum.COINCIDES:
                    return DatetimeMethodEnumParams.INTERVAL_COINCIDES;
                case DateTimeMethodEnum.DURING:
                    return DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES;
                case DateTimeMethodEnum.INCLUDES:
                    return DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES;
                case DateTimeMethodEnum.FINISHES:
                    return DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY;
                case DateTimeMethodEnum.FINISHEDBY:
                    return DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY;
                case DateTimeMethodEnum.MEETS:
                    return DatetimeMethodEnumParams.INTERVAL_MEETS_METBY;
                case DateTimeMethodEnum.METBY:
                    return DatetimeMethodEnumParams.INTERVAL_MEETS_METBY;
                case DateTimeMethodEnum.OVERLAPS:
                    return DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY;
                case DateTimeMethodEnum.OVERLAPPEDBY:
                    return DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY;
                case DateTimeMethodEnum.STARTS:
                    return DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY;
                case DateTimeMethodEnum.STARTEDBY:
                    return DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.epl.methodbase;

namespace com.espertech.esper.common.@internal.epl.datetime.eval
{
    public class DateTimeMethodEnum
    {
        // calendar op
        public static readonly DateTimeMethodEnum WITHTIME = new DateTimeMethodEnum
            ("withTime", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.WITHTIME);

        public static readonly DateTimeMethodEnum WITHDATE = new DateTimeMethodEnum
            ("withDate", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.WITHDATE);

        public static readonly DateTimeMethodEnum PLUS = new DateTimeMethodEnum
            ("plus", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.PLUSMINUS);

        public static readonly DateTimeMethodEnum MINUS = new DateTimeMethodEnum
            ("minus", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.PLUSMINUS);

        public static readonly DateTimeMethodEnum WITHMAX = new DateTimeMethodEnum
            ("withMax", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DateTimeMethodEnum WITHMIN = new DateTimeMethodEnum
            ("withMin", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DateTimeMethodEnum SET = new DateTimeMethodEnum
            ("set", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD_PLUS_INT);

        public static readonly DateTimeMethodEnum ROUNDCEILING = new DateTimeMethodEnum
            ("roundCeiling", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DateTimeMethodEnum ROUNDFLOOR = new DateTimeMethodEnum
            ("roundFloor", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DateTimeMethodEnum ROUNDHALF = new DateTimeMethodEnum
            ("roundHalf", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        // reformat op
        public static readonly DateTimeMethodEnum GET = new DateTimeMethodEnum
            ("get", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DateTimeMethodEnum FORMAT = new DateTimeMethodEnum
            ("format", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.FORMAT);

        public static readonly DateTimeMethodEnum TODATETIMEEX = new DateTimeMethodEnum
            ("toDateTimeEx", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum TODATETIMEOFFSET = new DateTimeMethodEnum
            ("toDateTimeOffset", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum TODATETIME = new DateTimeMethodEnum
            ("toDateTime", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum TOMILLISEC = new DateTimeMethodEnum
            ("toMillisec", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETMINUTEOFHOUR = new DateTimeMethodEnum
            ("getMinuteOfHour", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETMONTHOFYEAR = new DateTimeMethodEnum
            ("getMonthOfYear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETDAYOFMONTH = new DateTimeMethodEnum
            ("getDayOfMonth", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETDAYOFWEEK = new DateTimeMethodEnum
            ("getDayOfWeek", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETDAYOFYEAR = new DateTimeMethodEnum
            ("getDayOfYear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETHOUROFDAY = new DateTimeMethodEnum
            ("getHourOfDay", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETMILLISOFSECOND = new DateTimeMethodEnum
            ("getMillisOfSecond", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETSECONDOFMINUTE = new DateTimeMethodEnum
            ("getSecondOfMinute", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETWEEKYEAR = new DateTimeMethodEnum
            ("getWeekyear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum GETYEAR = new DateTimeMethodEnum
            ("getYear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DateTimeMethodEnum BETWEEN = new DateTimeMethodEnum
            ("between", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.BETWEEN);

        // interval op
        public static readonly DateTimeMethodEnum BEFORE = new DateTimeMethodEnum
            ("before", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER);

        public static readonly DateTimeMethodEnum AFTER = new DateTimeMethodEnum
            ("after", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER);

        public static readonly DateTimeMethodEnum COINCIDES = new DateTimeMethodEnum
            ("coincides", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_COINCIDES);

        public static readonly DateTimeMethodEnum DURING = new DateTimeMethodEnum
            ("during", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES);

        public static readonly DateTimeMethodEnum INCLUDES = new DateTimeMethodEnum
            ("includes", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES);

        public static readonly DateTimeMethodEnum FINISHES = new DateTimeMethodEnum
            ("finishes", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY);

        public static readonly DateTimeMethodEnum FINISHEDBY = new DateTimeMethodEnum
            ("finishedBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY);

        public static readonly DateTimeMethodEnum MEETS = new DateTimeMethodEnum
            ("meets", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_MEETS_METBY);

        public static readonly DateTimeMethodEnum METBY = new DateTimeMethodEnum
            ("metBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_MEETS_METBY);

        public static readonly DateTimeMethodEnum OVERLAPS = new DateTimeMethodEnum
            ("overlaps", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY);

        public static readonly DateTimeMethodEnum OVERLAPPEDBY = new DateTimeMethodEnum
            ("overlappedBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY);

        public static readonly DateTimeMethodEnum STARTS = new DateTimeMethodEnum
            ("starts", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY);

        public static readonly DateTimeMethodEnum STARTEDBY = new DateTimeMethodEnum
            ("startedBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY);

        public static readonly DateTimeMethodEnum[] Values = {
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
            GETERA,
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
        };

        private DateTimeMethodEnum(
            string nameCamel,
            ForgeFactory forgeFactory,
            DotMethodFP[] footprints)
        {
            NameCamel = nameCamel;
            ForgeFactory = forgeFactory;
            Footprints = footprints;
        }

        public ForgeFactory ForgeFactory { get; }

        public string NameCamel { get; }

        public DotMethodFP[] Footprints { get; }

        public static bool IsDateTimeMethod(string name)
        {
            foreach (var e in Values) {
                if (e.NameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return true;
                }
            }

            return false;
        }

        public static DateTimeMethodEnum FromName(string name)
        {
            foreach (var e in Values) {
                if (e.NameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase)) {
                    return e;
                }
            }

            return null;
        }
    }
} // end of namespace
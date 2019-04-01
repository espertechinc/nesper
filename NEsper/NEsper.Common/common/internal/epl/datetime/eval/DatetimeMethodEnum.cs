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
    public class DatetimeMethodEnum
    {
        // calendar op
        public static readonly DatetimeMethodEnum WITHTIME = new DatetimeMethodEnum
            ("withTime", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.WITHTIME);

        public static readonly DatetimeMethodEnum WITHDATE = new DatetimeMethodEnum
            ("withDate", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.WITHDATE);

        public static readonly DatetimeMethodEnum PLUS = new DatetimeMethodEnum
            ("plus", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.PLUSMINUS);

        public static readonly DatetimeMethodEnum MINUS = new DatetimeMethodEnum
            ("minus", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.PLUSMINUS);

        public static readonly DatetimeMethodEnum WITHMAX = new DatetimeMethodEnum
            ("withMax", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DatetimeMethodEnum WITHMIN = new DatetimeMethodEnum
            ("withMin", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DatetimeMethodEnum SET = new DatetimeMethodEnum
            ("set", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD_PLUS_INT);

        public static readonly DatetimeMethodEnum ROUNDCEILING = new DatetimeMethodEnum
            ("roundCeiling", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DatetimeMethodEnum ROUNDFLOOR = new DatetimeMethodEnum
            ("roundFloor", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DatetimeMethodEnum ROUNDHALF = new DatetimeMethodEnum
            ("roundHalf", DatetimeMethodEnumStatics.CALENDAR_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        // reformat op
        public static readonly DatetimeMethodEnum GET = new DatetimeMethodEnum
            ("get", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.CALFIELD);

        public static readonly DatetimeMethodEnum FORMAT = new DatetimeMethodEnum
            ("format", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.FORMAT);

        public static readonly DatetimeMethodEnum TODATETIMEEX = new DatetimeMethodEnum
            ("toDateTimeEx", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum TODATETIMEOFFSET = new DatetimeMethodEnum
            ("toDateTimeOffset", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum TODATETIME = new DatetimeMethodEnum
            ("toDateTime", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum TOMILLISEC = new DatetimeMethodEnum
            ("toMillisec", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETMINUTEOFHOUR = new DatetimeMethodEnum
            ("getMinuteOfHour", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETMONTHOFYEAR = new DatetimeMethodEnum
            ("getMonthOfYear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETDAYOFMONTH = new DatetimeMethodEnum
            ("getDayOfMonth", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETDAYOFWEEK = new DatetimeMethodEnum
            ("getDayOfWeek", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETDAYOFYEAR = new DatetimeMethodEnum
            ("getDayOfYear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETHOUROFDAY = new DatetimeMethodEnum
            ("getHourOfDay", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETMILLISOFSECOND = new DatetimeMethodEnum
            ("getMillisOfSecond", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETSECONDOFMINUTE = new DatetimeMethodEnum
            ("getSecondOfMinute", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETWEEKYEAR = new DatetimeMethodEnum
            ("getWeekyear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum GETYEAR = new DatetimeMethodEnum
            ("getYear", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.NOPARAM);

        public static readonly DatetimeMethodEnum BETWEEN = new DatetimeMethodEnum
            ("between", DatetimeMethodEnumStatics.REFORMAT_FORGE_FACTORY, DatetimeMethodEnumParams.BETWEEN);

        // interval op
        public static readonly DatetimeMethodEnum BEFORE = new DatetimeMethodEnum
            ("before", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER);

        public static readonly DatetimeMethodEnum AFTER = new DatetimeMethodEnum
            ("after", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER);

        public static readonly DatetimeMethodEnum COINCIDES = new DatetimeMethodEnum
            ("coincides", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_COINCIDES);

        public static readonly DatetimeMethodEnum DURING = new DatetimeMethodEnum
            ("during", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES);

        public static readonly DatetimeMethodEnum INCLUDES = new DatetimeMethodEnum
            ("includes", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES);

        public static readonly DatetimeMethodEnum FINISHES = new DatetimeMethodEnum
            ("finishes", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY);

        public static readonly DatetimeMethodEnum FINISHEDBY = new DatetimeMethodEnum
            ("finishedBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY);

        public static readonly DatetimeMethodEnum MEETS = new DatetimeMethodEnum
            ("meets", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_MEETS_METBY);

        public static readonly DatetimeMethodEnum METBY = new DatetimeMethodEnum
            ("metBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_MEETS_METBY);

        public static readonly DatetimeMethodEnum OVERLAPS = new DatetimeMethodEnum
            ("overlaps", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY);

        public static readonly DatetimeMethodEnum OVERLAPPEDBY = new DatetimeMethodEnum
            ("overlappedBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY);

        public static readonly DatetimeMethodEnum STARTS = new DatetimeMethodEnum
            ("starts", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY);

        public static readonly DatetimeMethodEnum STARTEDBY = new DatetimeMethodEnum
            ("startedBy", DatetimeMethodEnumStatics.INTERVAL_FORGE_FACTORY, DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY);

        public static readonly DatetimeMethodEnum[] Values = {
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

        private DatetimeMethodEnum(
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
            foreach (var e in Values)
            {
                if (e.NameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static DatetimeMethodEnum FromName(string name)
        {
            foreach (var e in Values)
            {
                if (e.NameCamel.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return e;
                }
            }

            return null;
        }
    }
} // end of namespace
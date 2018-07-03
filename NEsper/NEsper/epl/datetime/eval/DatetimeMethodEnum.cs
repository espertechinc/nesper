///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.methodbase;

namespace com.espertech.esper.epl.datetime.eval
{
    public enum DatetimeMethodEnum
    {
        // datetime ops
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

        // reformat ops
        GET,
        FORMAT,
        TOCALENDAR,
        TODATE,
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

        // interval ops
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

    public class DatetimeMethodEnumMetaData
    {

        public string NameCamel { get; private set; }

        public OpFactory OpFactory { get; private set; }

        public DotMethodFP[] Footprints { get; private set; }

        internal DatetimeMethodEnumMetaData(string nameCamel, OpFactory opFactory, DotMethodFP[] footprints)
        {
            NameCamel = nameCamel;
            OpFactory = opFactory;
            Footprints = footprints;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override string ToString()
        {
            return string.Format("NameCamel: {0}, OpFactory: {1}, Footprints: {2}", NameCamel, OpFactory, Footprints);
        }
    }

    public static class DatetimeMethodEnumExtensions
    {
        private static readonly IDictionary<DatetimeMethodEnum, DatetimeMethodEnumMetaData> MetaDataTable;

        static DatetimeMethodEnumExtensions()
        {
            MetaDataTable = new Dictionary<DatetimeMethodEnum, DatetimeMethodEnumMetaData>();
            MetaDataTable.Put(
                DatetimeMethodEnum.WITHTIME,
                new DatetimeMethodEnumMetaData("withTime", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.WITHTIME));
            MetaDataTable.Put(
                DatetimeMethodEnum.WITHDATE,
                new DatetimeMethodEnumMetaData("withDate", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.WITHDATE));
            MetaDataTable.Put(
                DatetimeMethodEnum.PLUS,
                new DatetimeMethodEnumMetaData("plus", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.PLUSMINUS));
            MetaDataTable.Put(
                DatetimeMethodEnum.MINUS,
                new DatetimeMethodEnumMetaData("minus", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.PLUSMINUS));
            MetaDataTable.Put(
                DatetimeMethodEnum.WITHMAX,
                new DatetimeMethodEnumMetaData("withMax", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD));
            MetaDataTable.Put(
                DatetimeMethodEnum.WITHMIN,
                new DatetimeMethodEnumMetaData("withMin", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD));
            MetaDataTable.Put(
                DatetimeMethodEnum.SET,
                new DatetimeMethodEnumMetaData("set", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD_PLUS_INT));
            MetaDataTable.Put(
                DatetimeMethodEnum.ROUNDCEILING,
                new DatetimeMethodEnumMetaData("roundCeiling", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD));
            MetaDataTable.Put(
                DatetimeMethodEnum.ROUNDFLOOR,
                new DatetimeMethodEnumMetaData("roundFloor", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD));
            MetaDataTable.Put(
                DatetimeMethodEnum.ROUNDHALF,
                new DatetimeMethodEnumMetaData("roundHalf", DatetimeMethodEnumStatics.CALENDAR_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD));
            MetaDataTable.Put(
                DatetimeMethodEnum.GET,
                new DatetimeMethodEnumMetaData("get", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.CALFIELD));
            MetaDataTable.Put(
                DatetimeMethodEnum.FORMAT,
                new DatetimeMethodEnumMetaData("format", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.FORMAT));
            MetaDataTable.Put(
                DatetimeMethodEnum.TOCALENDAR,
                new DatetimeMethodEnumMetaData("toCalendar", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.TODATE,
                new DatetimeMethodEnumMetaData("toDate", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.TOMILLISEC,
                new DatetimeMethodEnumMetaData("toMillisec", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETMINUTEOFHOUR,
                new DatetimeMethodEnumMetaData("getMinuteOfHour", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETMONTHOFYEAR,
                new DatetimeMethodEnumMetaData("getMonthOfYear", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETDAYOFMONTH,
                new DatetimeMethodEnumMetaData("getDayOfMonth", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETDAYOFWEEK,
                new DatetimeMethodEnumMetaData("getDayOfWeek", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETDAYOFYEAR,
                new DatetimeMethodEnumMetaData("getDayOfYear", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETERA,
                new DatetimeMethodEnumMetaData("getEra", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETHOUROFDAY,
                new DatetimeMethodEnumMetaData("getHourOfDay", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETMILLISOFSECOND,
                new DatetimeMethodEnumMetaData("getMillisOfSecond", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETSECONDOFMINUTE,
                new DatetimeMethodEnumMetaData("getSecondOfMinute", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETWEEKYEAR,
                new DatetimeMethodEnumMetaData("getWeekyear", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.GETYEAR,
                new DatetimeMethodEnumMetaData("getYear", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.NOPARAM));
            MetaDataTable.Put(
                DatetimeMethodEnum.BETWEEN,
                new DatetimeMethodEnumMetaData("between", DatetimeMethodEnumStatics.REFORMAT_OP_FACTORY,
                                               DatetimeMethodEnumParams.BETWEEN));
            MetaDataTable.Put(
                DatetimeMethodEnum.BEFORE,
                new DatetimeMethodEnumMetaData("before", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER));
            MetaDataTable.Put(
                DatetimeMethodEnum.AFTER,
                new DatetimeMethodEnumMetaData("after", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_BEFORE_AFTER));
            MetaDataTable.Put(
                DatetimeMethodEnum.COINCIDES,
                new DatetimeMethodEnumMetaData("coincides", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_COINCIDES));
            MetaDataTable.Put(
                DatetimeMethodEnum.DURING,
                new DatetimeMethodEnumMetaData("during", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES));
            MetaDataTable.Put(
                DatetimeMethodEnum.INCLUDES,
                new DatetimeMethodEnumMetaData("includes", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_DURING_INCLUDES));
            MetaDataTable.Put(
                DatetimeMethodEnum.FINISHES,
                new DatetimeMethodEnumMetaData("finishes", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.FINISHEDBY,
                new DatetimeMethodEnumMetaData("finishedBy", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_FINISHES_FINISHEDBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.MEETS,
                new DatetimeMethodEnumMetaData("meets", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_MEETS_METBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.METBY,
                new DatetimeMethodEnumMetaData("metBy", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_MEETS_METBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.OVERLAPS,
                new DatetimeMethodEnumMetaData("overlaps", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.OVERLAPPEDBY,
                new DatetimeMethodEnumMetaData("overlappedBy", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_DURING_OVERLAPS_OVERLAPBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.STARTS,
                new DatetimeMethodEnumMetaData("starts", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY));
            MetaDataTable.Put(
                DatetimeMethodEnum.STARTEDBY,
                new DatetimeMethodEnumMetaData("startedBy", DatetimeMethodEnumStatics.INTERVAL_OP_FACTORY,
                                               DatetimeMethodEnumParams.INTERVAL_STARTS_STARTEDBY));
        }

        public static DotMethodFP[] Footprints(this DatetimeMethodEnum datetimeMethodEnum)
        {
            var metaData = MetaData(datetimeMethodEnum);
            if (metaData != null)
                return metaData.Footprints;

            throw new ArgumentException();
        }

        public static DatetimeMethodEnumMetaData MetaData(this DatetimeMethodEnum datetimeMethodEnum)
        {
            return MetaDataTable.Get(datetimeMethodEnum);
        }

        public static bool IsDateTimeMethod(this string name)
        {
            name = name.ToLower();
            return MetaDataTable.Values.Any(metaData => metaData.NameCamel.ToLower() == name);
        }

        public static DatetimeMethodEnum FromName(string name)
        {
            name = name.ToLower();

            foreach (var keyValuePair in MetaDataTable)
            {
                if (keyValuePair.Value.NameCamel.ToLower() == name)
                {
                    return keyValuePair.Key;
                }
            }

            throw new ArgumentException("Enumeration identified by name \"" + name + "\" was not found");
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportDateTime
    {
        public SupportDateTime(
            long? longDate,
            DateTimeOffset? dtoDate,
            DateTimeEx dtxDate)
        {
            LongDate = longDate;
            DtoDate = dtoDate;
            DtxDate = dtxDate;
        }

        public long? LongDate { get; }

        public DateTimeOffset? DtoDate { get; }

        public DateTimeEx DtxDate { get; }

        public string Key { get; set; }

        public static DateTimeEx ToDateTimeEx(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Local, value);
        }

        public static DateTimeOffset ToDateTimeOffset(long value)
        {
            return value.TimeFromMillis(null);
        }

        public static DateTime ToDate(long value)
        {
            return value.TimeFromMillis();
        }

        public static SupportDateTime Make(string datestr)
        {
            if (datestr == null) {
                return new SupportDateTime(null, null, null);
            }

            // expected : 2002-05-30T09:00:00
            var dateTimeEx = DateTimeParsingFunctions.ParseDefaultEx(datestr);

            return new SupportDateTime(
                dateTimeEx.TimeInMillis,
                dateTimeEx.UtcDateTime,
                dateTimeEx);
        }

        public static SupportDateTime Make(
            string key,
            string datestr)
        {
            var bean = Make(datestr);
            bean.Key = key;
            return bean;
        }

        public static object GetValueCoerced(
            string expectedTime,
            string format)
        {
            var msec = DateTimeParsingFunctions.ParseDefaultMSec(expectedTime);
            return Coerce(msec, format);
        }

        private static object Coerce(
            long msec,
            string format)
        {
            switch (format.ToLower()) {
                case "long":
                    return msec;

                case "datetime":
                    return msec.TimeFromMillis();

                case "dto":
                case "util":
                    return msec.TimeFromMillis(null);

                case "dtx":
                case "cal":
                    return DateTimeEx.GetInstance(TimeZoneInfo.Local, msec.TimeFromMillis(null));

                case "str[utc]":
                    return msec.TimeFromMillis(TimeZoneInfo.Utc).ToString();

                case "str":
                    return msec.TimeFromMillis(TimeZoneInfo.Local).ToString();

                case "null":
                    return null;

                default:
                    throw new Exception("Unrecognized format abbreviation '" + format + "'");
            }
        }

        public static object[] GetArrayCoerced(
            string expectedTime,
            params string[] desc)
        {
            var result = new object[desc.Length];
            var msec = DateTimeParsingFunctions.ParseDefaultMSec(expectedTime);
            for (var i = 0; i < desc.Length; i++) {
                result[i] = Coerce(msec, desc[i]);
            }

            return result;
        }

        public static object[] GetArrayCoerced(
            string[] expectedTimes,
            string desc)
        {
            var result = new object[expectedTimes.Length];
            for (var i = 0; i < expectedTimes.Length; i++) {
                var msec = DateTimeParsingFunctions.ParseDefaultMSec(expectedTimes[i]);
                result[i] = Coerce(msec, desc);
            }

            return result;
        }
    }
} // end of namespace
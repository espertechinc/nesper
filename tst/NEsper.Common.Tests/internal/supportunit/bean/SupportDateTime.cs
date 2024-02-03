///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public class SupportDateTime
    {
        public SupportDateTime(
            long? longDate,
            DateTimeEx dateTimeEx,
            DateTimeOffset? dateTimeOffset,
            DateTime? dateTime)
        {
            LongDate = longDate;
            DateTimeEx = dateTimeEx;
            DateTimeOffset = dateTimeOffset;
            DateTime = dateTime;
        }

        public long? LongDate { get; }

        public DateTimeEx DateTimeEx { get; }

        public DateTimeOffset? DateTimeOffset { get; }

        public DateTime? DateTime { get; }

        public string Key { get; set; }

        public static DateTimeEx ToDateTimeEx(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Utc, value);
        }

        public static DateTimeOffset ToDateTimeOffset(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Utc, value)
                .DateTime;
        }

        public static DateTimeOffset ToDateTime(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Utc, value)
                .DateTime
                .DateTime;
        }

        public static SupportDateTime Make(string datestr)
        {
            if (datestr == null)
            {
                return new SupportDateTime(null, null, null, null);
            }

            // expected : 2002-05-30T09:00:00
            var dateTimeEx = DateTimeParsingFunctions.ParseDefaultEx(datestr);

            return new SupportDateTime(
                dateTimeEx.UtcMillis,
                dateTimeEx,
                dateTimeEx.UtcDateTime,
                dateTimeEx.UtcDateTime.DateTime);
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
            long msec = DateTimeParsingFunctions.ParseDefaultMSec(expectedTime);
            return Coerce(msec, format);
        }

        private static object Coerce(
            long msec,
            string format)
        {
            if (format.Equals("long", StringComparison.InvariantCultureIgnoreCase))
            {
                return msec;
            }

            if (format.Equals("dtx", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTimeEx.GetInstance(TimeZoneInfo.Utc, msec);
            }

            if (format.Equals("dto", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTimeEx.GetInstance(TimeZoneInfo.Utc, msec)
                    .DateTime;
            }

            if (format.Equals("datetime", StringComparison.InvariantCultureIgnoreCase) ||
                format.Equals("date", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTimeEx.GetInstance(TimeZoneInfo.Utc, msec)
                    .DateTime
                    .DateTime;
            }

            if (format.Equals("str", StringComparison.InvariantCultureIgnoreCase) ||
                format.Equals("iso", StringComparison.InvariantCultureIgnoreCase))
            {
                var dtx = DateTimeEx.GetInstance(TimeZoneInfo.Utc, msec);
                var sdf = DateTimeFormat.ISO_DATE_TIME;
                return sdf.Format(dtx);
            }

            if (format.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            throw new EPRuntimeException("Unrecognized format abbreviation '" + format + "'");
        }

        public static object[] GetArrayCoerced(
            string expectedTime,
            params string[] desc)
        {
            var result = new object[desc.Length];
            long msec = DateTimeParsingFunctions.ParseDefaultMSec(expectedTime);
            for (var i = 0; i < desc.Length; i++)
            {
                result[i] = Coerce(msec, desc[i]);
            }

            return result;
        }

        public static object[] GetArrayCoerced(
            string[] expectedTimes,
            string desc)
        {
            var result = new object[expectedTimes.Length];
            for (var i = 0; i < expectedTimes.Length; i++)
            {
                long msec = DateTimeParsingFunctions.ParseDefaultMSec(expectedTimes[i]);
                result[i] = Coerce(msec, desc);
            }

            return result;
        }
    }
} // end of namespace

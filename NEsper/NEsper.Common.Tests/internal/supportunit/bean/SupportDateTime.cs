///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
            long? longdate,
            DateTimeEx dtxDate,
            DateTimeOffset? offsetDateTime,
            DateTime? dateTime)
        {
            Longdate = longdate;
            DtxDate = dtxDate;
            OffsetDateTime = offsetDateTime;
            DateTime = dateTime;
        }

        public long? Longdate { get; }

        public DateTimeEx DtxDate { get; }

        public DateTimeOffset? OffsetDateTime { get; }

        public DateTime? DateTime { get; }

        public string Key { get; set; }

        public static DateTimeEx ToDateTimeEx(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Local, value);
        }

        public static DateTimeOffset ToDateTimeOffset(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Local, value)
                .DateTime;
        }

        public static DateTimeOffset ToDateTime(long value)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Local, value)
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
            var dto = DateTimeParsingFunctions.ParseDefaultDateTimeOffset(datestr);
            var dtx = DateTimeEx.GetInstance(TimeZoneInfo.Local, dto);
            dtx.SetMillis(0);

            return new SupportDateTime(
                dto.TimeInMillis(),
                dtx,
                dto,
                dto.DateTime);
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
            long time,
            string format)
        {
            if (format.Equals("long", StringComparison.InvariantCultureIgnoreCase))
            {
                return time;
            }

            if (format.Equals("dtx", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTimeEx.GetInstance(TimeZoneInfo.Local, time);
            }

            if (format.Equals("dto", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTimeEx.GetInstance(TimeZoneInfo.Local, time)
                    .DateTime;
            }

            if (format.Equals("datetime", StringComparison.InvariantCultureIgnoreCase))
            {
                return DateTimeEx.GetInstance(TimeZoneInfo.Local, time)
                    .DateTime
                    .DateTime;
            }

            if (format.Equals("sdf", StringComparison.InvariantCultureIgnoreCase))
            {
                var dtx = DateTimeEx.GetInstance(TimeZoneInfo.Local, time);
                var sdf = new SimpleDateFormat();
                return sdf.Format(dtx);
            }

#if FALSE
            if (format.Equals("dtf_isodt", StringComparison.InvariantCultureIgnoreCase))
            {
                var date = LocalDateTime.OfInstant(Instant.OfEpochMilli(time), TimeZoneInfo.Local);
                return DateTimeFormatter.ISO_DATE_TIME.Format(date);
            }

            if (format.Equals("dtf_isozdt", StringComparison.InvariantCultureIgnoreCase))
            {
                var date = ZonedDateTime.OfInstant(Instant.OfEpochMilli(time), TimeZoneInfo.Local);
                return DateTimeFormatter.ISO_ZONED_DATE_TIME.Format(date);
            }
#endif

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

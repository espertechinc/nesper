///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;

namespace com.espertech.esper.supportregression.bean
{
    public class SupportDateTime
    {
        private string _key;
        private readonly long? _longdate;
        private readonly DateTimeOffset? _utildate;
        private readonly DateTimeEx _caldate;

        public SupportDateTime(long? longdate, DateTimeOffset? utildate, DateTimeEx caldate)
        {
            _longdate = longdate;
            _utildate = utildate;
            _caldate = caldate;
        }

        public long? Longdate => _longdate;

        public DateTimeOffset? Utildate => _utildate;

        public DateTimeEx Caldate => _caldate;

        public string Key
        {
            get => _key;
            set => _key = value;
        }

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
            return DateTimeHelper.FromMillis(value);
        }

        public static SupportDateTime Make(string datestr)
        {
            if (datestr == null)
            {
                return new SupportDateTime(null, null, null);
            }
            // expected : 2002-05-30T09:00:00
            var dateTimeEx = DateTimeParser.ParseDefaultEx(datestr);

            return new SupportDateTime(
                dateTimeEx.TimeInMillis,
                dateTimeEx.UtcDateTime,
                dateTimeEx);
        }

        public static SupportDateTime Make(string key, string datestr)
        {
            SupportDateTime bean = Make(datestr);
            bean.Key = key;
            return bean;
        }

        public static Object GetValueCoerced(string expectedTime, string format)
        {
            long msec = DateTimeParser.ParseDefaultMSec(expectedTime);
            return Coerce(msec, format);
        }

        private static Object Coerce(long msec, string format)
        {
            switch (format.ToLower())
            {
                case "long":
                    return msec;
                case "util":
                    return DateTimeOffsetHelper.TimeFromMillis(msec, null);
                case "cal":
                    return DateTimeEx.GetInstance(TimeZoneInfo.Local, DateTimeOffsetHelper.TimeFromMillis(msec, null));
                case "str[utc]":
                    return DateTimeOffsetHelper.TimeFromMillis(msec, TimeZoneInfo.Utc).ToString();
                case "str":
                    return DateTimeOffsetHelper.TimeFromMillis(msec, TimeZoneInfo.Local).ToString();
                case "null":
                    return null;
                default:
                    throw new Exception("Unrecognized format abbreviation '" + format + "'");
            }
        }

        public static object[] GetArrayCoerced(string expectedTime, params string[] desc)
        {
            var result = new Object[desc.Length];
            var msec = DateTimeParser.ParseDefaultMSec(expectedTime);
            for (int i = 0; i < desc.Length; i++)
            {
                result[i] = Coerce(msec, desc[i]);
            }
            return result;
        }

        public static object[] GetArrayCoerced(string[] expectedTimes, string desc)
        {
            var result = new Object[expectedTimes.Length];
            for (int i = 0; i < expectedTimes.Length; i++)
            {
                var msec = DateTimeParser.ParseDefaultMSec(expectedTimes[i]);
                result[i] = Coerce(msec, desc);
            }
            return result;
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.support.bean
{
    public class SupportDateTime
    {
        private string _key;
        private readonly long? _msecdate;
        private readonly DateTimeOffset? _utildate;

        public SupportDateTime(string key, long? msecdate, DateTimeOffset? utildate)
        {
            _key = key;
            _msecdate = msecdate;
            _utildate = utildate;
        }

        public SupportDateTime(long? msecdate, DateTimeOffset? utildate)
        {
            _msecdate = msecdate;
            _utildate = utildate;
        }

        public long? Msecdate
        {
            get { return _msecdate; }
        }

        public DateTimeOffset? Utildate
        {
            get { return _utildate; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public static DateTimeOffset ToCalendar(long value)
        {
            return value.TimeFromMillis(null);
        }

        public static DateTimeOffset ToDate(long value)
        {
            return value.TimeFromMillis(null);
        }

        public static SupportDateTime Make(string datestr)
        {
            if (datestr == null)
            {
                return new SupportDateTime(null, null);
            }
            // expected : 2002-05-30T09:00:00
            var date = DateTimeParser.ParseDefaultEx(datestr);
            return new SupportDateTime(date.TimeInMillis, date.DateTime);
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
                case "msec":
                    return msec;
                case "util":
                    return DateTimeOffsetHelper.TimeFromMillis(msec, null);
                case "cal":
                    return DateTimeOffsetHelper.TimeFromMillis(msec, null);
                case "sdf":
                    return DateTimeOffsetHelper.TimeFromMillis(msec, null).ToString();
                case "null":
                    return null;
                default:
                    throw new Exception("Unrecognized format abbreviation '" + format + "'");
            }
        }

        public static Object[] GetArrayCoerced(string expectedTime, params string[] desc)
        {
            var result = new Object[desc.Length];
            var msec = DateTimeParser.ParseDefaultMSec(expectedTime);
            for (int i = 0; i < desc.Length; i++)
            {
                result[i] = Coerce(msec, desc[i]);
            }
            return result;
        }

        public static Object[] GetArrayCoerced(string[] expectedTimes, string desc)
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

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
        private readonly DateTime? _utildate;

        public SupportDateTime(string key, long? msecdate, DateTime? utildate)
        {
            _key = key;
            _msecdate = msecdate;
            _utildate = utildate;
        }

        public SupportDateTime(long? msecdate, DateTime? utildate)
        {
            _msecdate = msecdate;
            _utildate = utildate;
        }

        public long? Msecdate
        {
            get { return _msecdate; }
        }

        public DateTime? Utildate
        {
            get { return _utildate; }
        }

        public string Key
        {
            get { return _key; }
            set { _key = value; }
        }

        public static DateTime ToCalendar(long value)
        {
            return value.TimeFromMillis();
        }

        public static DateTime ToDate(long value)
        {
            return value.TimeFromMillis();
        }

        public static SupportDateTime Make(string datestr)
        {
            if (datestr == null)
            {
                return new SupportDateTime(null, null);
            }
            // expected : 2002-05-30T09:00:00
            DateTime date = DateTimeHelper.ParseDefault(datestr);
            return new SupportDateTime(date.TimeInMillis(), date);
        }

        public static SupportDateTime Make(string key, string datestr)
        {
            SupportDateTime bean = Make(datestr);
            bean.Key = key;
            return bean;
        }

        public static Object GetValueCoerced(string expectedTime, string format)
        {
            long msec = DateTimeHelper.ParseDefaultMSec(expectedTime);
            return Coerce(msec, format);
        }

        private static Object Coerce(long msec, string format)
        {

            switch (format.ToLower())
            {
                case "msec":
                    return msec;
                case "util":
                    return msec.TimeFromMillis();
                case "cal":
                    return msec.TimeFromMillis();
                case "sdf":
                    return msec.TimeFromMillis().ToString();
                case "null":
                    return null;
                default:
                    throw new Exception("Unrecognized format abbreviation '" + format + "'");
            }
        }

        public static Object[] GetArrayCoerced(string expectedTime, params string[] desc)
        {
            var result = new Object[desc.Length];
            var msec = DateTimeHelper.ParseDefaultMSec(expectedTime);
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
                var msec = DateTimeHelper.ParseDefaultMSec(expectedTimes[i]);
                result[i] = Coerce(msec, desc);
            }
            return result;
        }
    }
}

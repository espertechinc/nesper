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
    public class SupportTimeStartEndB
    {
        public SupportTimeStartEndB(String key,
                                    long? longdateStart,
                                    DateTimeOffset? utildateStart,
                                    DateTimeEx caldateStart,
                                    long? longdateEnd,
                                    DateTimeOffset? utildateEnd,
                                    DateTimeEx caldateEnd)
        {
            Key = key;
            LongdateStart = longdateStart;
            UtildateStart = utildateStart;
            CaldateStart = caldateStart;
            LongdateEnd = longdateEnd;
            UtildateEnd = utildateEnd;
            CaldateEnd = caldateEnd;
        }

        [PropertyName("longdateStart")]
        public long? LongdateStart { get; private set; }

        [PropertyName("utildateStart")]
        public DateTimeOffset? UtildateStart { get; private set; }

        [PropertyName("caldateStart")]
        public DateTimeEx CaldateStart { get; private set; }

        [PropertyName("longdateEnd")]
        public long? LongdateEnd { get; private set; }

        [PropertyName("utildateEnd")]
        public DateTimeOffset? UtildateEnd { get; private set; }

        [PropertyName("caldateEnd")]
        public DateTimeEx CaldateEnd { get; private set; }

        public string Key { get; set; }

        public static SupportTimeStartEndB Make(String key, String datestr, long duration)
        {
            if (datestr == null)
            {
                return new SupportTimeStartEndB(key, null, null, null, null, null, null);
            }
            // expected : 2002-05-30T09:00:00.000
            long start = DateTimeParser.ParseDefaultMSec(datestr);
            long end = start + duration;

            return new SupportTimeStartEndB(
                key, start, 
                SupportDateTime.ToDate(start), 
                SupportDateTime.ToDateTimeEx(start),
                end,
                SupportDateTime.ToDate(end),
                SupportDateTime.ToDateTimeEx(end));
        }
    }
}
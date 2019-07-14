///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

namespace com.espertech.esper.regressionlib.support.bean
{
    public abstract class SupportTimeStartBase
    {
        public SupportTimeStartBase(
            string key,
            string datestr,
            long duration)
        {
            Key = key;

            if (datestr != null) {
                // expected : 2002-05-30T09:00:00.000
                var start = DateTimeParsingFunctions.ParseDefaultMSec(datestr);
                var end = start + duration;

                LongdateStart = start;
                UtildateStart = SupportDateTime.ToDate(start);
                CaldateStart = SupportDateTime.ToDateTimeEx(start);
                LongdateEnd = end;
                UtildateEnd = SupportDateTime.ToDate(end);
                CaldateEnd = SupportDateTime.ToDateTimeEx(end);
            }
        }

        [PropertyName("longdateStart")]
        public long? LongdateStart { get; }

        [PropertyName("utildateStart")]
        public DateTime UtildateStart { get; }

        [PropertyName("caldateStart")]
        public DateTimeEx CaldateStart { get; }

        [PropertyName("longdateEnd")]
        public long? LongdateEnd { get; }

        [PropertyName("utildateEnd")]
        public DateTime UtildateEnd { get; }

        [PropertyName("caldateEnd")]
        public DateTimeEx CaldateEnd { get; }

        public string Key { get; set; }
    }
} // end of namespace
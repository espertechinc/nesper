///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.supportunit.bean
{
    public abstract class SupportTimeStartBase
    {
        public SupportTimeStartBase(string key, string datestr, long duration)
        {
            Key = key;

            if (datestr != null) {
                // expected : 2002-05-30T09:00:00.000
                var start = DateTimeParser.ParseDefaultMSec(datestr);
                var end = start + duration;

                LongdateStart = start;
                UtildateStart = SupportDateTime.ToDate(start);
                DateTimeExStart = SupportDateTime.ToDateTimeEx(start);
                LongdateEnd = end;
                UtildateEnd = SupportDateTime.ToDate(end);
                DateTimeExEnd = SupportDateTime.ToDateTimeEx(end);
            }
        }

        public long? LongdateStart { get; }

        public DateTimeOffset UtildateStart { get; }

        public DateTimeEx DateTimeExStart { get; }

        public long? LongdateEnd { get; }

        public DateTimeOffset UtildateEnd { get; }

        public DateTimeEx DateTimeExEnd { get; }

        public string Key { get; set; }
    }
} // end of namespace
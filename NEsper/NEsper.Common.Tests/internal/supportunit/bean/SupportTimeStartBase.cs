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

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    public abstract class SupportTimeStartBase
    {
        public SupportTimeStartBase(
            string key,
            string datestr,
            long duration)
        {
            Key = key;

            if (datestr != null)
            {
                // expected : 2002-05-30T09:00:00.000
                long start = DateTimeParsingFunctions.ParseDefaultMSec(datestr);
                var end = start + duration;

                LongdateStart = start;
                DtxStart = SupportDateTime.ToDateTimeEx(start);
                DtoStart = DateTimeParsingFunctions.ParseDefaultDate(datestr);
                DateTimeStart = DtoStart.TranslateTo(TimeZoneInfo.Local).DateTime;
                LongdateEnd = end;
                DtxEnd = SupportDateTime.ToDateTimeEx(end);
                DtoEnd = DtoStart.AddMilliseconds(duration);
                DateTimeEnd = DtoEnd.TranslateTo(TimeZoneInfo.Local).DateTime;
            }
        }

        public long? LongdateStart { get; }

        public DateTimeEx DtxStart { get; }

        public long? LongdateEnd { get; }

        public DateTimeEx DtxEnd { get; }

        public DateTimeOffset DtoStart { get; }

        public DateTime DateTimeStart { get; }

        public DateTimeOffset DtoEnd { get; }

        public DateTime DateTimeEnd { get; }

        public string Key { get; set; }
    }
} // end of namespace

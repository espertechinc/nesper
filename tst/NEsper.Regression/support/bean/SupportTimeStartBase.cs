///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public abstract class SupportTimeStartBase
    {
        protected SupportTimeStartBase(
            string key,
            string datestr,
            long duration)
        {
            Key = key;

            if (datestr != null)
            {
                // expected : 2002-05-30T09:00:00.000
                var start = DateTimeParsingFunctions.ParseDefaultEx(datestr);

                DateTimeExStart = start;
                DateTimeOffsetStart = DateTimeExStart.DateTime;
                DateTimeStart = DateTimeOffsetStart.DateTime;
                LongdateStart = start.UtcMillis;

                var end = start.Clone().AddTimeSpan(TimeSpan.FromMilliseconds(duration));

                DateTimeExEnd = end;
                DateTimeOffsetEnd = DateTimeExEnd.DateTime;
                DateTimeEnd = DateTimeOffsetEnd.DateTime;
                LongdateEnd = end.UtcMillis;

#if false
                Console.WriteLine("DateTimeExStart: " + DateTimeExStart + " / " + DateTimeExStart.Millisecond);
                Console.WriteLine("DateTimeOffsetStart: " + DateTimeOffsetStart + " / " + DateTimeOffsetStart.Millisecond);
                Console.WriteLine("DateTimeStart: " + DateTimeStart + " / " + DateTimeStart.Millisecond);

                Console.WriteLine("DateTimeExEnd: " + DateTimeExEnd + " / " + DateTimeExEnd.Millisecond);
                Console.WriteLine("DateTimeOffsetEnd: " + DateTimeOffsetEnd + " / " + DateTimeOffsetEnd.Millisecond);
                Console.WriteLine("DateTimeEnd: " + DateTimeEnd + " / " + DateTimeEnd.Millisecond);
#endif
            }
        }

        public long? LongdateStart { get; }

        public DateTime DateTimeStart { get; }

        public DateTimeOffset DateTimeOffsetStart { get; }

        public DateTimeEx DateTimeExStart { get; }

        public long? LongdateEnd { get; }

        public DateTime DateTimeEnd { get; }

        public DateTimeOffset DateTimeOffsetEnd { get; }

        public DateTimeEx DateTimeExEnd { get; }

        public string Key { get; set; }
    }
} // end of namespace
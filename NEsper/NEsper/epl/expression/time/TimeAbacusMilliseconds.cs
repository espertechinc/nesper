///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.time
{
    public class TimeAbacusMilliseconds : TimeAbacus
    {
        public static readonly TimeAbacusMilliseconds INSTANCE = new TimeAbacusMilliseconds();

        private TimeAbacusMilliseconds()
        {
        }

        public long DeltaForSecondsDouble(double seconds)
        {
            return (long) Math.Round(1000d*seconds);
        }

        public long DeltaForSecondsNumber(object timeInSeconds)
        {
            if (timeInSeconds.IsFloatingPointNumber())
            {
                return DeltaForSecondsDouble(timeInSeconds.AsDouble());
            }
            return 1000*timeInSeconds.AsLong();
        }

        public long CalendarSet(long fromTime, DateTimeEx dt)
        {
            dt.SetUtcMillis(fromTime);
            return 0;
        }

        public long CalendarGet(DateTimeEx dt, long remainder)
        {
            return dt.TimeInMillis;
        }

        public long OneSecond
        {
            get { return 1000; }
        }

        public DateTimeEx ToDate(long ts)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Utc, ts);
        }
    }
} // end of namespace

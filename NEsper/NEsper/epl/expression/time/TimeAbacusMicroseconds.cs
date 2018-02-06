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
    public class TimeAbacusMicroseconds : TimeAbacus
    {
        public static readonly TimeAbacusMicroseconds INSTANCE = new TimeAbacusMicroseconds();

        private TimeAbacusMicroseconds()
        {
        }

        public long DeltaForSecondsDouble(double seconds)
        {
            return (long) Math.Round(1000000d*seconds);
        }

        public long DeltaForSecondsNumber(object timeInSeconds)
        {
            if (timeInSeconds.IsFloatingPointNumber())
            {
                return DeltaForSecondsDouble(timeInSeconds.AsDouble());
            }
            return 1000000*timeInSeconds.AsLong();
        }

        public long CalendarSet(long fromTime, DateTimeEx dt)
        {
            long millis = fromTime/1000;
            dt.SetUtcMillis(millis);
            return fromTime - millis*1000;
        }

        public long CalendarGet(DateTimeEx dt, long remainder)
        {
            return dt.TimeInMillis*1000 + remainder;
        }

        public long OneSecond
        {
            get { return 1000000; }
        }

        public DateTimeEx ToDate(long ts)
        {
            return DateTimeEx.GetInstance(TimeZoneInfo.Utc, ts / 1000);
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ZonedDateTimeEvalStatics
    {
        public static readonly ZonedDateTimeEval MINUTE_OF_HOUR = zdt => zdt.Minute;

        public static readonly ZonedDateTimeEval MONTH_OF_YEAR = zdt => zdt.Month;

        public static readonly ZonedDateTimeEval DAY_OF_MONTH = zdt => zdt.Day;

        public static readonly ZonedDateTimeEval DAY_OF_WEEK = zdt => zdt.DayOfWeek;

        public static readonly ZonedDateTimeEval DAY_OF_YEAR = zdt => zdt.DayOfYear;
    
        public static readonly ZonedDateTimeEval ERA = zdt =>
        {
            throw new NotImplementedException();
        };

        public static readonly ZonedDateTimeEval HOUR_OF_DAY = zdt => zdt.Hour;

        public static readonly ZonedDateTimeEval MILLIS_OF_SECOND = zdt => zdt.Millisecond;

        public static readonly ZonedDateTimeEval SECOND_OF_MINUTE = zdt => zdt.Second;

        public static readonly ZonedDateTimeEval WEEKYEAR = zdt => zdt.WeekOfYear;

        public static readonly ZonedDateTimeEval YEAR = zdt => zdt.Year;
    }
} // end of namespace

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
    public class LocalDateTimeEvalStatics
    {
        public static readonly LocalDateTimeEval MINUTE_OF_HOUR = ldt => ldt.Minute;

        public static readonly LocalDateTimeEval MONTH_OF_YEAR = ldt => ldt.Month;

        public static readonly LocalDateTimeEval DAY_OF_MONTH = ldt => ldt.Day;

        public static readonly LocalDateTimeEval DAY_OF_WEEK = ldt => ldt.DayOfWeek;

        public static readonly LocalDateTimeEval DAY_OF_YEAR = ldt => ldt.DayOfYear;
    
        public static readonly LocalDateTimeEval ERA = ldt =>
        {
            throw new NotImplementedException();
        };

        public static readonly LocalDateTimeEval HOUR_OF_DAY = ldt => ldt.Hour;

        public static readonly LocalDateTimeEval MILLIS_OF_SECOND = ldt => ldt.Millisecond;

        public static readonly LocalDateTimeEval SECOND_OF_MINUTE = ldt => ldt.Second;

        public static readonly LocalDateTimeEval WEEKYEAR = ldt => ldt.WeekOfYear;

        public static readonly LocalDateTimeEval YEAR = ldt => ldt.Year;
    }
} // end of namespace

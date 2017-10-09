///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.datetime.reformatop
{
    using CalendarIntEval = Func<DateTimeEx, int>;
    using CalendarDowEval = Func<DateTimeEx, DayOfWeek>;

    public class CalendarEvalStatics
    {
        public static readonly CalendarIntEval MINUTE_OF_HOUR = dtx => dtx.Minute;
    
        public static readonly CalendarIntEval MONTH_OF_YEAR = dtx => dtx.Month;
    
        public static readonly CalendarIntEval DAY_OF_MONTH = dtx => dtx.Day;
    
        public static readonly CalendarDowEval DAY_OF_WEEK = dtx => dtx.DayOfWeek;

        public static readonly CalendarIntEval DAY_OF_YEAR = dtx => dtx.DayOfYear;
    
        public static readonly CalendarIntEval ERA = dtx => {
                throw new NotSupportedException();
            };
    
        public static readonly CalendarIntEval HOUR_OF_DAY = dtx => dtx.Hour;
    
        public static readonly CalendarIntEval MILLIS_OF_SECOND = dtx => dtx.Millisecond;
    
        public static readonly CalendarIntEval SECOND_OF_MINUTE = dtx => dtx.Second;
    
        public static readonly CalendarIntEval WEEKYEAR = dtx => dtx.DateTime.GetWeekOfYear();
    
        public static readonly CalendarIntEval YEAR = dtx => dtx.Year;
    }
} // end of namespace

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
    using CalendarEval = Func<DateTimeEx, object>;

    public class CalendarEvalStatics
    {
        public static readonly CalendarEval MINUTE_OF_HOUR = dtx => dtx.Minute;
    
        public static readonly CalendarEval MONTH_OF_YEAR = dtx => dtx.Month;
    
        public static readonly CalendarEval DAY_OF_MONTH = dtx => dtx.Day;
    
        public static readonly CalendarEval DAY_OF_WEEK = dtx => dtx.DayOfWeek;
    
        public static readonly CalendarEval DAY_OF_YEAR = dtx => dtx.Year;
    
        public static readonly CalendarEval ERA = dtx => {
                throw new NotSupportedException();
            };
    
        public static readonly CalendarEval HOUR_OF_DAY = dtx => dtx.Hour;
    
        public static readonly CalendarEval MILLIS_OF_SECOND = dtx => dtx.Millisecond;
    
        public static readonly CalendarEval SECOND_OF_MINUTE = dtx => dtx.Second;
    
        public static readonly CalendarEval WEEKYEAR = dtx => dtx.DateTime.GetWeekOfYear();
    
        public static readonly CalendarEval YEAR = dtx => dtx.Year;
    }
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class DateTimeEvalStatics
    {
        public static DateTimeEval MinuteOfHour = 
            dateTime => dateTime.Minute;
        public static DateTimeEval MonthOfYear =
            dateTime => dateTime.Month;
        public static DateTimeEval DayOfMonth =
            dateTime => dateTime.Day;
        public static DateTimeEval DayOfWeek = 
            dateTime => dateTime.DayOfWeek;
        public static DateTimeEval DayOfYear = 
            dateTime => dateTime.DayOfYear;
        public static DateTimeEval HourOfDay =
            dateTime => dateTime.Hour;
        public static DateTimeEval MillisOfSecond = 
            dateTime => dateTime.Millisecond;
        public static DateTimeEval SecondOfMinute =
            dateTime => dateTime.Second;
        public static DateTimeEval Year =
            dateTime => dateTime.Year;
        public static DateTimeEval Weekyear =
            dateTime => dateTime.GetWeekOfYear();
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.datetime.reformatop;



namespace com.espertech.esper.epl.datetime.reformatop
{
    public class CalendarEvalStatics {
    
        public static CalendarEval MinuteOfHour = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.MINUTE);
                }
            };
    
        public static CalendarEval MonthOfYear = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.MONTH);
                }
            };
    
        public static CalendarEval DayOfMonth = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.DATE);
                }
            };
    
        public static CalendarEval DayOfWeek = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.DAY_OF_WEEK);
                }
            };
    
        public static CalendarEval DayOfYear = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.DAY_OF_YEAR);
                }
            };    
    
        public static CalendarEval Era = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.ERA);
                }
            };
    
        public static CalendarEval HourOfDay = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.HOUR_OF_DAY);
                }
            };
    
        public static CalendarEval MillisOfSecond = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.MILLISECOND);
                }
            };
    
        public static CalendarEval SecondOfMinute = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.SECOND);
                }
            };
    
        public static CalendarEval Weekyear = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.WEEK_OF_YEAR);
                }
            };
    
        public static CalendarEval Year = new CalendarEval() {
                public Object EvaluateInternal(Calendar cal) {
                    return cal.Get(Calendar.YEAR);
                }
            };
    }
}

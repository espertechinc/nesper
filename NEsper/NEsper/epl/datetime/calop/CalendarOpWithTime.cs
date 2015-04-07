///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpWithTime : CalendarOp
    {
        private readonly ExprEvaluator _hour;
        private readonly ExprEvaluator _min;
        private readonly ExprEvaluator _sec;
        private readonly ExprEvaluator _msec;
    
        public CalendarOpWithTime(ExprEvaluator hour, ExprEvaluator min, ExprEvaluator sec, ExprEvaluator msec)
        {
            _hour = hour;
            _min = min;
            _sec = sec;
            _msec = msec;
        }
    
        public void Evaluate(ref DateTime dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) 
        {
            int? hourNum = CalendarOpWithDate.GetInt(_hour, eventsPerStream, isNewData, context);
            int? minNum = CalendarOpWithDate.GetInt(_min, eventsPerStream, isNewData, context);
            int? secNum = CalendarOpWithDate.GetInt(_sec, eventsPerStream, isNewData, context);
            int? msecNum = CalendarOpWithDate.GetInt(_msec, eventsPerStream, isNewData, context);
            dateTime = Action(dateTime, hourNum, minNum, secNum, msecNum);
        }

        private static DateTime Action(DateTime dateTime, int? hour, int? minute, int? second, int? msec)
        {
             return new DateTime(
                    dateTime.Year, dateTime.Month, dateTime.Day,
                    hour ?? dateTime.Hour, 
                    minute ?? dateTime.Minute, 
                    second ?? dateTime.Second,
                    msec ?? dateTime.Millisecond,
                    dateTime.Kind);
        }
    }
}

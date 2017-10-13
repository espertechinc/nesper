///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.calop
{
    public class CalendarOpWithDate : CalendarOp
    {
        private readonly ExprEvaluator _year;
        private readonly ExprEvaluator _month;
        private readonly ExprEvaluator _day;

        public CalendarOpWithDate(ExprEvaluator year, ExprEvaluator month, ExprEvaluator day)
        {
            _year = year;
            _month = month;
            _day = day;
        }

        public void Evaluate(DateTimeEx dateTime, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            int? yearNum = GetInt(_year, eventsPerStream, isNewData, context);
            int? monthNum = GetInt(_month, eventsPerStream, isNewData, context);
            int? dayNum = GetInt(_day, eventsPerStream, isNewData, context);
            Action(dateTime, yearNum, monthNum, dayNum);
        }

        public static int? GetInt(ExprEvaluator expr, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var result = expr.Evaluate(new EvaluateParams(eventsPerStream, isNewData, context));
            if (result == null)
            {
                return null;
            }
            return (int?)result;
        }

        private static DateTimeEx Action(DateTimeEx dateTime, int? year, int? month, int? day)
        {
            return dateTime.Set(
                year ?? dateTime.Year,
                month ?? dateTime.Month,
                day ?? dateTime.Day,
                dateTime.Hour,
                dateTime.Minute,
                dateTime.Second,
                dateTime.Millisecond);
        }
    }
}

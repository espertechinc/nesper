///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.epl.datetime.calop;
using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ReformatOpFactory : OpFactory 
    {
        private static readonly ReformatOp FormatString = new ReformatOpStringFormat();
        private static readonly ReformatOp ToMsec = new ReformatOpToMillisec();
    
        public ReformatOp GetOp(TimeZoneInfo timeZone, DatetimeMethodEnum method, String methodNameUsed, IList<ExprNode> parameters)
        {
            switch (method)
            {
                case DatetimeMethodEnum.GET:
                    return new ReformatOpGetField(CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]), timeZone);
                case DatetimeMethodEnum.FORMAT:
                    return FormatString;
                case DatetimeMethodEnum.TOMILLISEC:
                    return ToMsec;
                case DatetimeMethodEnum.TODATE:
                    return new ReformatOpToDateTime(timeZone);
                case DatetimeMethodEnum.TOCALENDAR:
                    return new ReformatOpToDateTime(timeZone);
                case DatetimeMethodEnum.GETDAYOFMONTH:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.DayOfMonth, typeof(int), timeZone);
                case DatetimeMethodEnum.GETMINUTEOFHOUR:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.MinuteOfHour, typeof(int), timeZone);
                case DatetimeMethodEnum.GETMONTHOFYEAR:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.MonthOfYear, typeof(int), timeZone);
                case DatetimeMethodEnum.GETDAYOFWEEK:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.DayOfWeek, typeof(DayOfWeek), timeZone);
                case DatetimeMethodEnum.GETDAYOFYEAR:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.DayOfYear, typeof(int), timeZone);
                case DatetimeMethodEnum.GETHOUROFDAY:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.HourOfDay, typeof(int), timeZone);
                case DatetimeMethodEnum.GETMILLISOFSECOND:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.MillisOfSecond, typeof(int), timeZone);
                case DatetimeMethodEnum.GETSECONDOFMINUTE:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.SecondOfMinute, typeof(int), timeZone);
                case DatetimeMethodEnum.GETWEEKYEAR:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.Weekyear, typeof(int), timeZone);
                case DatetimeMethodEnum.GETYEAR:
                    return new ReformatOpDateTimeEval(DateTimeEvalStatics.Year, typeof(int), timeZone);
                case DatetimeMethodEnum.BETWEEN:
                    if (ExprNodeUtility.IsAllConstants(parameters))
                    {
                        return new ReformatOpBetweenConstantParams(parameters);
                    }
                    return new ReformatOpBetweenNonConstantParams(parameters);
            }

            throw new IllegalStateException("Unrecognized date-time method code '" + method + "'");
        }
    }
}

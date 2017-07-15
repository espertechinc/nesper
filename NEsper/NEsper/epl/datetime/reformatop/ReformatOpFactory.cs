///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.datetime.reformatop
{
    public class ReformatOpFactory : OpFactory {
    
        private static readonly ReformatOp FORMAT_STRING = new ReformatOpStringFormat();
    
        public ReformatOp GetOp(TimeZoneInfo timeZone, TimeAbacus timeAbacus, DatetimeMethodEnum method, string methodNameUsed, List<ExprNode> parameters) {
            if (method == DatetimeMethodEnum.GET) {
                CalendarFieldEnum fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                return new ReformatOpGetField(fieldNum, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.FORMAT) {
                return FORMAT_STRING;
            }
            if (method == DatetimeMethodEnum.TOCALENDAR) {
                return new ReformatOpToCalendar(timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.TOMILLISEC) {
                return new ReformatOpToMillisec(timeZone);
            }
            if (method == DatetimeMethodEnum.TODATE) {
                return new ReformatOpToDateTimeOffset(timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETDAYOFMONTH) {
                return new ReformatOpEval(CalendarEvalStatics.DAY_OF_MONTH, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETMINUTEOFHOUR) {
                return new ReformatOpEval(CalendarEvalStatics.MINUTE_OF_HOUR, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETMONTHOFYEAR) {
                return new ReformatOpEval(CalendarEvalStatics.MONTH_OF_YEAR, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETDAYOFWEEK) {
                return new ReformatOpEval(CalendarEvalStatics.DAY_OF_WEEK, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETDAYOFYEAR) {
                return new ReformatOpEval(CalendarEvalStatics.DAY_OF_YEAR, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETERA) {
                return new ReformatOpEval(CalendarEvalStatics.ERA, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETHOUROFDAY) {
                return new ReformatOpEval(CalendarEvalStatics.HOUR_OF_DAY, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETMILLISOFSECOND) {
                return new ReformatOpEval(CalendarEvalStatics.MILLIS_OF_SECOND, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETSECONDOFMINUTE) {
                return new ReformatOpEval(CalendarEvalStatics.SECOND_OF_MINUTE, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETWEEKYEAR) {
                return new ReformatOpEval(CalendarEvalStatics.WEEKYEAR, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.GETYEAR) {
                return new ReformatOpEval(CalendarEvalStatics.YEAR, timeZone, timeAbacus);
            }
            if (method == DatetimeMethodEnum.BETWEEN) {
                if (ExprNodeUtility.IsAllConstants(parameters)) {
                    return new ReformatOpBetweenConstantParams(parameters, timeZone);
                }
                return new ReformatOpBetweenNonConstantParams(parameters, timeZone);
            }
            throw new IllegalStateException("Unrecognized date-time method code '" + method + "'");
        }
    }
} // end of namespace

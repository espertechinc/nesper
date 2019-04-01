///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.datetime.reformatop
{
    public class ReformatForgeFactory : ForgeFactory
    {
        private static readonly ReformatForge FORMAT_STRING = new ReformatStringFormatForge();

        public ReformatForge GetForge(
            EPType inputType,
            TimeAbacus timeAbacus,
            DatetimeMethodEnum method,
            string methodNameUsed,
            IList<ExprNode> parameters,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (method == DatetimeMethodEnum.GET) {
                var fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                return new ReformatGetFieldForge(fieldNum, timeAbacus);
            }

            if (method == DatetimeMethodEnum.FORMAT) {
                if (parameters.IsEmpty()) {
                    return FORMAT_STRING;
                }

                var formatterType = CalendarOpUtil.ValidateGetFormatterType(inputType, methodNameUsed, parameters[0]);
                return new ReformatFormatForge(formatterType, parameters[0].Forge, timeAbacus);
            }

            if (method == DatetimeMethodEnum.TODATETIMEEX) {
                return new ReformatToDateTimeExForge(timeAbacus);
            }

            if (method == DatetimeMethodEnum.TODATETIMEOFFSET) {
                return new ReformatToDateTimeOffsetForge(timeAbacus);
            }

            if (method == DatetimeMethodEnum.TODATETIMEOFFSET) {
                return new ReformatToDateTimeForge(timeAbacus);
            }

            if (method == DatetimeMethodEnum.TOMILLISEC) {
                return new ReformatToMillisecForge();
            }

            if (method == DatetimeMethodEnum.GETDAYOFMONTH) {
                return new ReformatEvalForge(DateTimeExEvalStatics.DAY_OF_MONTH, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETMINUTEOFHOUR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.MINUTE_OF_HOUR, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETMONTHOFYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.MONTH_OF_YEAR, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETDAYOFWEEK) {
                return new ReformatEvalForge(DateTimeExEvalStatics.DAY_OF_WEEK, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETDAYOFYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.DAY_OF_YEAR, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETHOUROFDAY) {
                return new ReformatEvalForge(DateTimeExEvalStatics.HOUR_OF_DAY, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETMILLISOFSECOND) {
                return new ReformatEvalForge(DateTimeExEvalStatics.MILLIS_OF_SECOND, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETSECONDOFMINUTE) {
                return new ReformatEvalForge(DateTimeExEvalStatics.SECOND_OF_MINUTE, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETWEEKYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.WEEKYEAR, timeAbacus);
            }

            if (method == DatetimeMethodEnum.GETYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.YEAR, timeAbacus);
            }

            if (method == DatetimeMethodEnum.BETWEEN) {
                if (ExprNodeUtilityQuery.IsAllConstants(parameters)) {
                    return new ReformatBetweenConstantParamsForge(parameters);
                }

                return new ReformatBetweenNonConstantParamsForge(parameters);
            }

            throw new IllegalStateException("Unrecognized date-time method code '" + method + "'");
        }
    }
} // end of namespace
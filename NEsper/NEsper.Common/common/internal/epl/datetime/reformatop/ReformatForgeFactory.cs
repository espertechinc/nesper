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
            DateTimeMethodEnum method,
            string methodNameUsed,
            IList<ExprNode> parameters,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (method == DateTimeMethodEnum.GET) {
                var fieldNum = CalendarOpUtil.GetEnum(methodNameUsed, parameters[0]);
                return new ReformatGetFieldForge(fieldNum, timeAbacus);
            }

            if (method == DateTimeMethodEnum.FORMAT) {
                if (parameters.IsEmpty()) {
                    return FORMAT_STRING;
                }

                var formatterType = CalendarOpUtil.ValidateGetFormatterType(inputType, methodNameUsed, parameters[0]);
                return new ReformatFormatForge(formatterType, parameters[0].Forge, timeAbacus);
            }

            if (method == DateTimeMethodEnum.TODATETIMEEX) {
                return new ReformatToDateTimeExForge(timeAbacus);
            }

            if (method == DateTimeMethodEnum.TODATETIMEOFFSET) {
                return new ReformatToDateTimeOffsetForge(timeAbacus);
            }

            if (method == DateTimeMethodEnum.TODATETIME) {
                return new ReformatToDateTimeForge(timeAbacus);
            }

            if (method == DateTimeMethodEnum.TOMILLISEC) {
                return new ReformatToMillisecForge();
            }

            if (method == DateTimeMethodEnum.GETDAYOFMONTH) {
                return new ReformatEvalForge(DateTimeExEvalStatics.DAY_OF_MONTH, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETMINUTEOFHOUR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.MINUTE_OF_HOUR, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETMONTHOFYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.MONTH_OF_YEAR, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETDAYOFWEEK) {
                return new ReformatEvalForge(DateTimeExEvalStatics.DAY_OF_WEEK, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETDAYOFYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.DAY_OF_YEAR, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETHOUROFDAY) {
                return new ReformatEvalForge(DateTimeExEvalStatics.HOUR_OF_DAY, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETMILLISOFSECOND) {
                return new ReformatEvalForge(DateTimeExEvalStatics.MILLIS_OF_SECOND, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETSECONDOFMINUTE) {
                return new ReformatEvalForge(DateTimeExEvalStatics.SECOND_OF_MINUTE, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETWEEKYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.WEEKYEAR, timeAbacus);
            }

            if (method == DateTimeMethodEnum.GETYEAR) {
                return new ReformatEvalForge(DateTimeExEvalStatics.YEAR, timeAbacus);
            }

            if (method == DateTimeMethodEnum.BETWEEN) {
                if (ExprNodeUtilityQuery.IsAllConstants(parameters)) {
                    return new ReformatBetweenConstantParamsForge(parameters);
                }

                return new ReformatBetweenNonConstantParamsForge(parameters);
            }

            throw new IllegalStateException("Unrecognized date-time method code '" + method + "'");
        }
    }
} // end of namespace
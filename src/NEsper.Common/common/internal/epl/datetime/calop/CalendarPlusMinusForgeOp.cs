///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarPlusMinusForgeOp : CalendarOp
    {
        private readonly int factor;
        private readonly ExprEvaluator param;

        public CalendarPlusMinusForgeOp(
            ExprEvaluator param,
            int factor)
        {
            this.param = param;
            this.factor = factor;
        }

        public DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = param.Evaluate(eventsPerStream, isNewData, context);
            if (value.IsNumber()) {
                return ActionCalendarPlusMinusNumber(dateTimeEx, factor, value.AsInt64());
            }
            else {
                return ActionCalendarPlusMinusTimePeriod(dateTimeEx, factor, (TimePeriod)value);
            }
        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = param.Evaluate(eventsPerStream, isNewData, context);
            if (value.IsNumber()) {
                return ActionLDTPlusMinusNumber(dateTimeOffset, factor, value.AsInt64());
            }

            return ActionLDTPlusMinusTimePeriod(dateTimeOffset, factor, (TimePeriod)value);
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var value = param.Evaluate(eventsPerStream, isNewData, context);
            if (value.IsNumber()) {
                return ActionZDTPlusMinusNumber(dateTime, factor, value.AsInt64());
            }

            return ActionZDTPlusMinusTimePeriod(dateTime, factor, (TimePeriod)value);
        }

        public static CodegenExpression CodegenCalendar(
            CalendarPlusMinusForge forge,
            CodegenExpression dtx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = forge.param.EvaluationType;
            if (evaluationType.IsTypeNumeric()) {
                var longDuration = SimpleNumberCoercerFactory.CoercerLong.CodegenLong(
                    forge.param.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                    evaluationType);
                return StaticMethod(
                    typeof(CalendarPlusMinusForgeOp),
                    "ActionCalendarPlusMinusNumber",
                    dtx,
                    Constant(forge.factor),
                    longDuration);
            }

            return StaticMethod(
                typeof(CalendarPlusMinusForgeOp),
                "ActionCalendarPlusMinusTimePeriod",
                dtx,
                Constant(forge.factor),
                forge.param.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope));
        }

        public static CodegenExpression CodegenDateTimeOffset(
            CalendarPlusMinusForge forge,
            CodegenExpression dto,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = forge.param.EvaluationType;
            if (evaluationType.IsTypeNumeric()) {
                var longDuration = SimpleNumberCoercerFactory.CoercerLong.CodegenLongMayNullBox(
                    forge.param.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                    evaluationType,
                    codegenMethodScope,
                    codegenClassScope);
                return StaticMethod(
                    typeof(CalendarPlusMinusForgeOp),
                    "ActionLDTPlusMinusNumber",
                    dto,
                    Constant(forge.factor),
                    longDuration);
            }

            return StaticMethod(
                typeof(CalendarPlusMinusForgeOp),
                "ActionLDTPlusMinusTimePeriod",
                dto,
                Constant(forge.factor),
                forge.param.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope));
        }

        public static CodegenExpression CodegenDateTime(
            CalendarPlusMinusForge forge,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var evaluationType = forge.param.EvaluationType;
            if (evaluationType.IsTypeNumeric()) {
                var longDuration = SimpleNumberCoercerFactory.CoercerLong.CodegenLongMayNullBox(
                    forge.param.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope),
                    evaluationType,
                    codegenMethodScope,
                    codegenClassScope);
                return StaticMethod(
                    typeof(CalendarPlusMinusForgeOp),
                    "ActionZDTPlusMinusNumber",
                    dateTime,
                    Constant(forge.factor),
                    longDuration);
            }

            return StaticMethod(
                typeof(CalendarPlusMinusForgeOp),
                "ActionZDTPlusMinusTimePeriod",
                dateTime,
                Constant(forge.factor),
                forge.param.EvaluateCodegen(evaluationType, codegenMethodScope, exprSymbol, codegenClassScope));
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dtx">calendar</param>
        /// <param name="factor">factor</param>
        /// <param name="duration">duration</param>
        public static DateTimeEx ActionCalendarPlusMinusNumber(
            DateTimeEx dtx,
            int factor,
            long? duration)
        {
            if (duration == null) {
                return dtx;
            }

            if (duration < int.MaxValue) {
                dtx.AddMilliseconds((int)(factor * duration));
                return dtx;
            }

            var days = (int)(duration / (1000L * 60 * 60 * 24));
            var msec = (int)(duration - days * 1000L * 60 * 60 * 24);
            dtx.AddMilliseconds(factor * msec);
            dtx.AddDays(factor * days);

            return dtx;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dto">dto</param>
        /// <param name="factor">factor</param>
        /// <param name="duration">duration</param>
        /// <returns>dto</returns>
        public static DateTimeOffset ActionLDTPlusMinusNumber(
            DateTimeOffset dto,
            int factor,
            long? duration)
        {
            if (duration == null) {
                return dto;
            }

            if (duration < int.MaxValue) {
                return dto.AddMilliseconds(factor * duration.Value);
            }

            var days = (int)(duration / (1000L * 60 * 60 * 24));
            var msec = (int)(duration - days * 1000L * 60 * 60 * 24);
            dto = dto.AddMilliseconds(factor * msec);
            return dto.AddDays(factor * days);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dateTime">dto</param>
        /// <param name="factor">factor</param>
        /// <param name="duration">duration</param>
        /// <returns>dateTime</returns>
        public static DateTime ActionZDTPlusMinusNumber(
            DateTime dateTime,
            int factor,
            long? duration)
        {
            if (duration == null) {
                return dateTime;
            }

            if (duration < int.MaxValue) {
                return dateTime.AddMilliseconds(factor * duration.Value);
            }

            var days = (int)(duration / (1000L * 60 * 60 * 24));
            var msec = (int)(duration - days * 1000L * 60 * 60 * 24);
            dateTime = dateTime.AddMilliseconds(factor * msec);
            return dateTime.AddDays(factor * days);
        }

        public static void ActionSafeOverflow(
            DateTimeEx dtx,
            int factor,
            TimePeriod tp)
        {
            if (Math.Abs(factor) == 1) {
                ActionCalendarPlusMinusTimePeriod(dtx, factor, tp);
                return;
            }

            var max = tp.LargestAbsoluteValue();
            if (max == null || max == 0) {
                return;
            }

            ActionHandleOverflow(dtx, factor, tp, max.Value);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dtx">calendar</param>
        /// <param name="factor">factor</param>
        /// <param name="tp">duration</param>
        public static DateTimeEx ActionCalendarPlusMinusTimePeriod(
            DateTimeEx dtx,
            int factor,
            TimePeriod tp)
        {
            if (tp == null) {
                return dtx;
            }

            if (tp.Years != null) {
                dtx.AddYears(factor * tp.Years.Value);
            }

            if (tp.Months != null) {
                dtx.AddMonths(factor * tp.Months.Value);
            }

            if (tp.Weeks != null) {
                dtx.AddDays(factor * tp.Weeks.Value * 7);
            }

            if (tp.Days != null) {
                dtx.AddDays(factor * tp.Days.Value);
            }

            if (tp.Hours != null) {
                dtx.AddHours(factor * tp.Hours.Value);
            }

            if (tp.Minutes != null) {
                dtx.AddMinutes(factor * tp.Minutes.Value);
            }

            if (tp.Seconds != null) {
                dtx.AddSeconds(factor * tp.Seconds.Value);
            }

            if (tp.Milliseconds != null) {
                dtx.AddMilliseconds(factor * tp.Milliseconds.Value);
            }

            return dtx;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dto">dto</param>
        /// <param name="factor">factor</param>
        /// <param name="tp">duration</param>
        /// <returns>dto</returns>
        public static DateTimeOffset ActionLDTPlusMinusTimePeriod(
            DateTimeOffset dto,
            int factor,
            TimePeriod tp)
        {
            if (tp == null) {
                return dto;
            }

            if (tp.Years != null) {
                dto = dto.AddYears(factor * tp.Years.Value);
            }

            if (tp.Months != null) {
                dto = dto.AddMonths(factor * tp.Months.Value);
            }

            if (tp.Weeks != null) {
                dto = dto.AddDays(factor * tp.Weeks.Value * 7);
            }

            if (tp.Days != null) {
                dto = dto.AddDays(factor * tp.Days.Value);
            }

            if (tp.Hours != null) {
                dto = dto.AddHours(factor * tp.Hours.Value);
            }

            if (tp.Minutes != null) {
                dto = dto.AddMinutes(factor * tp.Minutes.Value);
            }

            if (tp.Seconds != null) {
                dto = dto.AddSeconds(factor * tp.Seconds.Value);
            }

            if (tp.Milliseconds != null) {
                dto = dto.AddMilliseconds(factor * tp.Milliseconds.Value);
            }

            return dto;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dateTime">dateTime</param>
        /// <param name="factor">factor</param>
        /// <param name="tp">duration</param>
        /// <returns>dateTime</returns>
        public static DateTime ActionZDTPlusMinusTimePeriod(
            DateTime dateTime,
            int factor,
            TimePeriod tp)
        {
            if (tp == null) {
                return dateTime;
            }

            if (tp.Years != null) {
                dateTime = dateTime.AddYears(factor * tp.Years.Value);
            }

            if (tp.Months != null) {
                dateTime = dateTime.AddMonths(factor * tp.Months.Value);
            }

            if (tp.Weeks != null) {
                dateTime = dateTime.AddDays(factor * tp.Weeks.Value * 7);
            }

            if (tp.Days != null) {
                dateTime = dateTime.AddDays(factor * tp.Days.Value);
            }

            if (tp.Hours != null) {
                dateTime = dateTime.AddHours(factor * tp.Hours.Value);
            }

            if (tp.Minutes != null) {
                dateTime = dateTime.AddMinutes(factor * tp.Minutes.Value);
            }

            if (tp.Seconds != null) {
                dateTime = dateTime.AddSeconds(factor * tp.Seconds.Value);
            }

            if (tp.Milliseconds != null) {
                dateTime = dateTime.AddMilliseconds(factor * tp.Milliseconds.Value);
            }

            return dateTime;
        }

        private static void ActionHandleOverflow(
            DateTimeEx dtx,
            int factor,
            TimePeriod tp,
            int max)
        {
            if (max != 0 && factor > int.MaxValue / max) {
                // overflow
                var first = factor / 2;
                var second = factor - first * 2 + first;
                ActionHandleOverflow(dtx, first, tp, max);
                ActionHandleOverflow(dtx, second, tp, max);
            }
            else {
                // no overflow
                ActionCalendarPlusMinusTimePeriod(dtx, factor, tp);
            }
        }
    }
} // end of namespace
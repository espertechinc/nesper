///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.datetime;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarWithDateForgeOp : CalendarOp
    {
        public const string METHOD_ACTIONSETYMDCALENDAR = "ActionSetYMDCalendar";

        private readonly ExprEvaluator day;
        private readonly ExprEvaluator month;
        private readonly ExprEvaluator year;

        public CalendarWithDateForgeOp(
            ExprEvaluator year,
            ExprEvaluator month,
            ExprEvaluator day)
        {
            this.year = year;
            this.month = month;
            this.day = day;
        }

        public DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var yearNum = GetInt(year, eventsPerStream, isNewData, context);
            var monthNum = GetInt(month, eventsPerStream, isNewData, context);
            var dayNum = GetInt(day, eventsPerStream, isNewData, context);
            ActionSetYMDCalendar(dateTimeEx, yearNum, monthNum, dayNum);
            return dateTimeEx;
        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var yearNum = GetInt(year, eventsPerStream, isNewData, context);
            var monthNum = GetInt(month, eventsPerStream, isNewData, context);
            var dayNum = GetInt(day, eventsPerStream, isNewData, context);
            return ActionSetYMDDateTimeOffset(dateTimeOffset, yearNum, monthNum, dayNum);
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var yearNum = GetInt(year, eventsPerStream, isNewData, context);
            var monthNum = GetInt(month, eventsPerStream, isNewData, context);
            var dayNum = GetInt(day, eventsPerStream, isNewData, context);
            return ActionSetYMDDateTime(dateTime, yearNum, monthNum, dayNum);
        }

        public static CodegenExpression CodegenCalendar(
            CalendarWithDateForge forge,
            CodegenExpression dtx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeEx), typeof(CalendarWithDateForgeOp), codegenClassScope)
                .AddParam(typeof(DateTimeEx), "value");

            var block = methodNode.Block;
            CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(
                StaticMethod(
                    typeof(CalendarWithDateForgeOp),
                    METHOD_ACTIONSETYMDCALENDAR,
                    Ref("value"),
                    Ref("year"),
                    Ref("month"),
                    Ref("day")));

            return LocalMethod(methodNode, dtx);
        }

        public static CodegenExpression CodegenDateTimeOffset(
            CalendarWithDateForge forge,
            CodegenExpression dto,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(CalendarWithDateForgeOp), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "value");

            var block = methodNode.Block;
            CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(
                StaticMethod(
                    typeof(CalendarWithDateForgeOp),
                    "ActionSetYMDDateTimeOffset",
                    Ref("value"),
                    Ref("year"),
                    Ref("month"),
                    Ref("day")));
            return LocalMethod(methodNode, dto);
        }

        public static CodegenExpression CodegenDateTime(
            CalendarWithDateForge forge,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(CalendarWithDateForgeOp), codegenClassScope)
                .AddParam(typeof(DateTime), "value");

            var block = methodNode.Block;
            CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(
                StaticMethod(
                    typeof(CalendarWithDateForgeOp),
                    "ActionSetYMDDateTime",
                    Ref("value"),
                    Ref("year"),
                    Ref("month"),
                    Ref("day")));
            return LocalMethod(methodNode, dateTime);
        }

        protected internal static int? GetInt(
            ExprEvaluator expr,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var result = expr.Evaluate(eventsPerStream, isNewData, context);
            if (result == null) {
                return null;
            }

            return (int?) result;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dtx">date-time</param>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <param name="day">day</param>
        public static DateTimeEx ActionSetYMDCalendar(
            DateTimeEx dtx,
            int? year,
            int? month,
            int? day)
        {
            if (year != null) {
                dtx = dtx.SetYear(year.Value);
            }

            if (month != null) {
                dtx = dtx.SetMonth(month.Value);
            }

            if (day != null) {
                dtx = dtx.SetDay(month.Value);
            }

            return dtx;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dto">date time offset</param>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <param name="day">day</param>
        /// <returns>dto</returns>
        public static DateTimeOffset ActionSetYMDDateTimeOffset(
            DateTimeOffset dto,
            int? year,
            int? month,
            int? day)
        {
            if (year != null) {
                dto = dto.WithYear(year.Value);
            }

            if (month != null) {
                dto = dto.WithMonth(month.Value);
            }

            if (day != null) {
                dto = dto.WithDay(day.Value);
            }

            return dto;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dateTime">dateTime</param>
        /// <param name="year">year</param>
        /// <param name="month">month</param>
        /// <param name="day">day</param>
        /// <returns>dto</returns>
        public static DateTime ActionSetYMDDateTime(
            DateTime dateTime,
            int? year,
            int? month,
            int? day)
        {
            if (year != null) {
                dateTime = dateTime.WithYear(year.Value);
            }

            if (month != null) {
                dateTime = dateTime.WithMonth(month.Value);
            }

            if (day != null) {
                dateTime = dateTime.WithDay(day.Value);
            }

            return dateTime;
        }

        private static void CodegenDeclareInts(
            CodegenBlock block,
            CalendarWithDateForge forge,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var yearType = forge.year.EvaluationType;
            var monthType = forge.month.EvaluationType;
            var dayType = forge.day.EvaluationType;
            block.DeclareVar<int?>(
                    "year",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.year.EvaluateCodegen(yearType, methodNode, exprSymbol, codegenClassScope),
                        yearType,
                        methodNode,
                        codegenClassScope))
                .DeclareVar<int?>(
                    "month",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.month.EvaluateCodegen(monthType, methodNode, exprSymbol, codegenClassScope),
                        monthType,
                        methodNode,
                        codegenClassScope))
                .DeclareVar<int?>(
                    "day",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.day.EvaluateCodegen(dayType, methodNode, exprSymbol, codegenClassScope),
                        dayType,
                        methodNode,
                        codegenClassScope));
        }
    }
} // end of namespace
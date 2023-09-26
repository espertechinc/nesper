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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.calop
{
    public class CalendarWithTimeForgeOp : CalendarOp
    {
        private readonly ExprEvaluator _hour;
        private readonly ExprEvaluator _min;
        private readonly ExprEvaluator _msec;
        private readonly ExprEvaluator _sec;

        public CalendarWithTimeForgeOp(
            ExprEvaluator hour,
            ExprEvaluator min,
            ExprEvaluator sec,
            ExprEvaluator msec)
        {
            _hour = hour;
            _min = min;
            _sec = sec;
            _msec = msec;
        }

        public DateTimeEx Evaluate(
            DateTimeEx dateTimeEx,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var hourNum = CalendarWithDateForgeOp.GetInt(_hour, eventsPerStream, isNewData, context);
            var minNum = CalendarWithDateForgeOp.GetInt(_min, eventsPerStream, isNewData, context);
            var secNum = CalendarWithDateForgeOp.GetInt(_sec, eventsPerStream, isNewData, context);
            var msecNum = CalendarWithDateForgeOp.GetInt(_msec, eventsPerStream, isNewData, context);
            return ActionSetHMSMDateTimeEx(dateTimeEx, hourNum, minNum, secNum, msecNum);
        }

        public DateTimeOffset Evaluate(
            DateTimeOffset dateTimeOffset,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var hourNum = CalendarWithDateForgeOp.GetInt(_hour, eventsPerStream, isNewData, context);
            var minNum = CalendarWithDateForgeOp.GetInt(_min, eventsPerStream, isNewData, context);
            var secNum = CalendarWithDateForgeOp.GetInt(_sec, eventsPerStream, isNewData, context);
            var msecNum = CalendarWithDateForgeOp.GetInt(_msec, eventsPerStream, isNewData, context);
            return ActionSetHMSMDateTimeOffset(dateTimeOffset, hourNum, minNum, secNum, msecNum);
        }

        public DateTime Evaluate(
            DateTime dateTime,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var hourNum = CalendarWithDateForgeOp.GetInt(_hour, eventsPerStream, isNewData, context);
            var minNum = CalendarWithDateForgeOp.GetInt(_min, eventsPerStream, isNewData, context);
            var secNum = CalendarWithDateForgeOp.GetInt(_sec, eventsPerStream, isNewData, context);
            var msecNum = CalendarWithDateForgeOp.GetInt(_msec, eventsPerStream, isNewData, context);
            return ActionSetHMSMDateTime(dateTime, hourNum, minNum, secNum, msecNum);
        }

        public static CodegenExpression CodegenDateTimeEx(
            CalendarWithTimeForge forge,
            CodegenExpression dtx,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeEx), typeof(CalendarWithTimeForgeOp), codegenClassScope)
                .AddParam<DateTimeEx>("dtx");

            var block = methodNode.Block;
            CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(
                StaticMethod(
                    typeof(CalendarWithTimeForgeOp),
                    "ActionSetHMSMDateTimeEx",
                    Ref("dtx"),
                    Ref("hour"),
                    Ref("minute"),
                    Ref("second"),
                    Ref("msec")));
            return LocalMethod(methodNode, dtx);
        }

        public static CodegenExpression CodegenDateTimeOffset(
            CalendarWithTimeForge forge,
            CodegenExpression dto,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTimeOffset), typeof(CalendarWithTimeForgeOp), codegenClassScope)
                .AddParam<DateTimeOffset>("dto");

            var block = methodNode.Block;
            CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(
                StaticMethod(
                    typeof(CalendarWithTimeForgeOp),
                    "ActionSetHMSMDateTimeOffset",
                    Ref("dto"),
                    Ref("hour"),
                    Ref("minute"),
                    Ref("second"),
                    Ref("msec")));
            return LocalMethod(methodNode, dto);
        }

        public static CodegenExpression CodegenDateTime(
            CalendarWithTimeForge forge,
            CodegenExpression dateTime,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(DateTime), typeof(CalendarWithTimeForgeOp), codegenClassScope)
                .AddParam<DateTime>("dateTime");

            var block = methodNode.Block;
            CodegenDeclareInts(block, forge, methodNode, exprSymbol, codegenClassScope);
            block.MethodReturn(
                StaticMethod(
                    typeof(CalendarWithTimeForgeOp),
                    "ActionSetHMSMDateTime",
                    Ref("dateTime"),
                    Ref("hour"),
                    Ref("minute"),
                    Ref("second"),
                    Ref("msec")));
            return LocalMethod(methodNode, dateTime);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dateTime">the date time</param>
        /// <param name="hour">hour</param>
        /// <param name="minute">min</param>
        /// <param name="second">sec</param>
        /// <param name="msec">msec</param>
        public static DateTimeEx ActionSetHMSMDateTimeEx(
            DateTimeEx dateTime,
            int? hour,
            int? minute,
            int? second,
            int? msec)
        {
            if (hour != null) {
                dateTime.SetHour(hour.Value);
            }

            if (minute != null) {
                dateTime.SetMinute(minute.Value);
            }

            if (second != null) {
                dateTime.SetSecond(second.Value);
            }

            if (msec != null) {
                dateTime.SetMillis(msec.Value);
            }

            return dateTime;
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dto">dto</param>
        /// <param name="hour">hour</param>
        /// <param name="minute">min</param>
        /// <param name="second">sec</param>
        /// <param name="msec">msec</param>
        /// <returns>dto</returns>
        public static DateTimeOffset ActionSetHMSMDateTimeOffset(
            DateTimeOffset dto,
            int? hour,
            int? minute,
            int? second,
            int? msec)
        {
            return new DateTimeOffset(
                dto.Year,
                dto.Month,
                dto.Day,
                hour ?? dto.Hour,
                minute ?? dto.Minute,
                second ?? dto.Second,
                msec ?? dto.Millisecond,
                dto.Offset);
        }

        /// <summary>
        ///     NOTE: Code-generation-invoked method, method name and parameter order matters
        /// </summary>
        /// <param name="dateTime">dateTime</param>
        /// <param name="hour">hour</param>
        /// <param name="minute">min</param>
        /// <param name="second">sec</param>
        /// <param name="msec">msec</param>
        /// <returns>dto</returns>
        public static DateTime ActionSetHMSMDateTime(
            DateTime dateTime,
            int? hour,
            int? minute,
            int? second,
            int? msec)
        {
            return new DateTime(
                dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                hour ?? dateTime.Hour,
                minute ?? dateTime.Minute,
                second ?? dateTime.Second,
                msec ?? dateTime.Millisecond,
                dateTime.Kind);
        }

        private static void CodegenDeclareInts(
            CodegenBlock block,
            CalendarWithTimeForge forge,
            CodegenMethod methodNode,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var hourType = forge.Hour.EvaluationType;
            var minType = forge.Min.EvaluationType;
            var secType = forge.Sec.EvaluationType;
            var msecType = forge.Msec.EvaluationType;
            block
                .DeclareVar<int?>(
                    "hour",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.Hour.EvaluateCodegen(hourType, methodNode, exprSymbol, codegenClassScope),
                        hourType,
                        methodNode,
                        codegenClassScope))
                .DeclareVar<int?>(
                    "minute",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.Min.EvaluateCodegen(minType, methodNode, exprSymbol, codegenClassScope),
                        minType,
                        methodNode,
                        codegenClassScope))
                .DeclareVar<int?>(
                    "second",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.Sec.EvaluateCodegen(secType, methodNode, exprSymbol, codegenClassScope),
                        secType,
                        methodNode,
                        codegenClassScope))
                .DeclareVar<int?>(
                    "msec",
                    SimpleNumberCoercerFactory.CoercerInt.CoerceCodegenMayNull(
                        forge.Msec.EvaluateCodegen(msecType, methodNode, exprSymbol, codegenClassScope),
                        msecType,
                        methodNode,
                        codegenClassScope));
        }
    }
} // end of namespace
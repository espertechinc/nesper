///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.calop;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.settings;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtoOpsIntervalEval : DTLocalEvaluatorCalOpsIntervalBase
    {
        public DTLocalDtoOpsIntervalEval(
            IList<CalendarOp> calendarOps,
            IntervalOp intervalOp)
            : base(calendarOps, intervalOp)
        {
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var dto = (DateTimeOffset) target;
            dto = EvaluateCalOpsLDT(calendarOps, dto, eventsPerStream, isNewData, exprEvaluatorContext);
            var time = DatetimeLongCoercerDateTimeOffset.CoerceToMillis(dto);
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtoOpsIntervalForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtxOpsIntervalEval), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "target");

            var block = methodNode.Block;
            EvaluateCalOpsLDTCodegen(block, "target", forge.calendarForges, methodNode, exprSymbol, codegenClassScope);
            block.DeclareVar(
                typeof(long), "time",
                StaticMethod(
                    typeof(DatetimeLongCoercerDateTimeOffset), "CoerceToMillis", Ref("target"), timeZoneField));
            block.MethodReturn(
                forge.intervalForge.Codegen(Ref("time"), Ref("time"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public override object Evaluate(
            object startTimestamp,
            object endTimestamp,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var start = (DateTimeOffset) startTimestamp;
            var end = (DateTimeOffset) endTimestamp;
            var deltaMSec = DatetimeLongCoercerDateTimeOffset.CoerceToMillis(end) -
                            DatetimeLongCoercerDateTimeOffset.CoerceToMillis(start);
            var result = EvaluateCalOpsLDT(calendarOps, start, eventsPerStream, isNewData, exprEvaluatorContext);
            var startLong = DatetimeLongCoercerDateTimeOffset.CoerceToMillis(result);
            var endTime = startLong + deltaMSec;
            return intervalOp.Evaluate(startLong, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtoOpsIntervalForge forge,
            CodegenExpressionRef start,
            CodegenExpressionRef end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenExpression timeZoneField =
                codegenClassScope.AddOrGetFieldSharable(RuntimeSettingsTimeZoneField.INSTANCE);
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtxOpsIntervalEval), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "start").AddParam(typeof(DateTimeOffset), "end");

            var block = methodNode.Block
                .DeclareVar(
                    typeof(long), "startMs",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset), "CoerceToMillis", Ref("start"), timeZoneField))
                .DeclareVar(
                    typeof(long), "endMs",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset), "CoerceToMillis", Ref("end"), timeZoneField))
                .DeclareVar(typeof(long), "deltaMSec", Op(Ref("endMs"), "-", Ref("startMs")))
                .DeclareVar(typeof(DateTimeOffset), "result", start);
            EvaluateCalOpsLDTCodegen(block, "result", forge.calendarForges, methodNode, exprSymbol, codegenClassScope);
            block.DeclareVar(
                typeof(long), "startLong",
                StaticMethod(
                    typeof(DatetimeLongCoercerDateTimeOffset), "CoerceToMillis", Ref("result"), timeZoneField));
            block.DeclareVar(typeof(long), "endTime", Op(Ref("startLong"), "+", Ref("deltaMSec")));
            block.MethodReturn(
                forge.intervalForge.Codegen(
                    Ref("startLong"), Ref("endTime"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace
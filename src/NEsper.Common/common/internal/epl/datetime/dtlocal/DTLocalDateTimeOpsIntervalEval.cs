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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.datetime.dtlocal.DTLocalUtil;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDateTimeOpsIntervalEval : DTLocalEvaluatorCalOpsIntervalBase
    {
        public DTLocalDateTimeOpsIntervalEval(
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
            var dt = (DateTime)target;
            dt = EvaluateCalOpsDateTime(calendarOps, dt, eventsPerStream, isNewData, exprEvaluatorContext);
            var time = DatetimeLongCoercerDateTime.CoerceToMillis(dt);
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDateTimeOpsIntervalForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDateTimeOpsIntervalEval), codegenClassScope)
                .AddParam<DateTime>("target");

            var block = methodNode.Block;
            EvaluateCalOpsDateTimeCodegen(
                block,
                "target",
                forge.calendarForges,
                methodNode,
                exprSymbol,
                codegenClassScope);
            block.DeclareVar<long>(
                "time",
                StaticMethod(
                    typeof(DatetimeLongCoercerDateTime),
                    "CoerceToMillis",
                    Ref("target")));
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
            var start = (DateTime)startTimestamp;
            var end = (DateTime)endTimestamp;
            var deltaMSec = DatetimeLongCoercerDateTime.CoerceToMillis(end) -
                            DatetimeLongCoercerDateTime.CoerceToMillis(start);
            var result = EvaluateCalOpsDateTime(calendarOps, start, eventsPerStream, isNewData, exprEvaluatorContext);
            var startLong = DatetimeLongCoercerDateTime.CoerceToMillis(result);
            var endTime = startLong + deltaMSec;
            return intervalOp.Evaluate(startLong, endTime, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDateTimeOpsIntervalForge forge,
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDateTimeOpsIntervalEval), codegenClassScope)
                .AddParam<DateTime>("start")
                .AddParam<DateTime>("end");

            var block = methodNode
                .Block
                .DebugStack()
                .DeclareVar<long>(
                    "startMs",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTime),
                        "CoerceToMillis",
                        Ref("start")))
                .DeclareVar<long>(
                    "endMs",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTime),
                        "CoerceToMillis",
                        Ref("end")))
                .DeclareVar<long>(
                    "deltaMSec",
                    Op(Ref("endMs"), "-", Ref("startMs")))
                .DeclareVar<DateTime>(
                    "result",
                    Ref("start"));

            EvaluateCalOpsDateTimeCodegen(
                block,
                "result",
                forge.calendarForges,
                methodNode,
                exprSymbol,
                codegenClassScope);

            block.DeclareVar<long>(
                "startLong",
                StaticMethod(
                    typeof(DatetimeLongCoercerDateTime),
                    "CoerceToMillis",
                    Ref("result")));
            block.DeclareVar<long>(
                "endTime",
                Op(Ref("startLong"), "+", Ref("deltaMSec")));
            block.MethodReturn(
                forge.intervalForge.Codegen(
                    Ref("startLong"),
                    Ref("endTime"),
                    methodNode,
                    exprSymbol,
                    codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace
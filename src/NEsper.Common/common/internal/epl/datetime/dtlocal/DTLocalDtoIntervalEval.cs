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
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    internal class DTLocalDtoIntervalEval : DTLocalEvaluatorIntervalBase
    {
        private readonly TimeZoneInfo timeZone;

        public DTLocalDtoIntervalEval(
            IntervalOp intervalOp,
            TimeZoneInfo timeZone)
            : base(intervalOp)
        {
            this.timeZone = timeZone;
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            long time = DatetimeLongCoercerDateTimeOffset.CoerceToMillis((DateTimeOffset) target);
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtoIntervalForge forge,
            CodegenExpression inner,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtoIntervalEval), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "target");

            methodNode.Block
                .DeclareVar<long>(
                    "time",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset),
                        "CoerceToMillis",
                        Ref("target")))
                .MethodReturn(
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
            long start = DatetimeLongCoercerDateTimeOffset.CoerceToMillis((DateTimeOffset) startTimestamp);
            long end = DatetimeLongCoercerDateTimeOffset.CoerceToMillis((DateTimeOffset) endTimestamp);
            return intervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtoIntervalForge forge,
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            CodegenMethod methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtoIntervalEval), codegenClassScope)
                .AddParam(typeof(DateTimeOffset), "startTimestamp")
                .AddParam(typeof(DateTimeOffset), "endTimestamp");

            methodNode.Block
                .DeclareVar<long>(
                    "start",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset),
                        "CoerceToMillis",
                        Ref("startTimestamp")))
                .DeclareVar<long>(
                    "end",
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset),
                        "CoerceToMillis",
                        Ref("endTimestamp")))
                .MethodReturn(
                    forge.intervalForge.Codegen(Ref("start"), Ref("end"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace
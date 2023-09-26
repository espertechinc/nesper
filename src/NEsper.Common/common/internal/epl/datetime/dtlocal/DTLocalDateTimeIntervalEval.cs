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
    internal class DTLocalDateTimeIntervalEval : DTLocalEvaluatorIntervalBase
    {
        public DTLocalDateTimeIntervalEval(IntervalOp intervalOp)
            : base(intervalOp)
        {
        }

        public override object Evaluate(
            object target,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var time = DatetimeLongCoercerDateTime.CoerceToMillis((DateTime)target);
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDateTimeIntervalForge forge,
            CodegenExpression inner,
            Type innerType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDateTimeIntervalEval), codegenClassScope)
                .AddParam<DateTime>("target");

            methodNode.Block
                .DeclareVar<long>(
                    "time",
                    StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", Ref("target")))
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
            var start = DatetimeLongCoercerDateTime.CoerceToMillis((DateTime)startTimestamp);
            var end = DatetimeLongCoercerDateTime.CoerceToMillis((DateTime)endTimestamp);
            return intervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDateTimeIntervalForge forge,
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDateTimeIntervalEval), codegenClassScope)
                .AddParam<DateTime>("startTimestamp")
                .AddParam<DateTime>("endTimestamp");

            methodNode.Block
                .DeclareVar<long>(
                    "start",
                    StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", Ref("startTimestamp")))
                .DeclareVar<long>(
                    "end",
                    StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", Ref("endTimestamp")))
                .MethodReturn(
                    forge.intervalForge.Codegen(Ref("start"), Ref("end"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace
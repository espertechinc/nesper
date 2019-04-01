///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.interval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.dtlocal
{
    public class DTLocalDtxIntervalEval : DTLocalEvaluatorIntervalBase
    {
        public DTLocalDtxIntervalEval(IntervalOp intervalOp) : base(intervalOp)
        {
        }

        public override object Evaluate(
            object target, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var time = ((DateTimeEx) target).TimeInMillis;
            return intervalOp.Evaluate(time, time, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtxIntervalForge forge, CodegenExpression inner, CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol, CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtxIntervalEval), codegenClassScope)
                .AddParam(typeof(DateTimeEx), "target");

            methodNode.Block
                .DeclareVar(typeof(long), "time", ExprDotMethod(Ref("target"), "getTimeInMillis"))
                .MethodReturn(
                    forge.intervalForge.Codegen(Ref("time"), Ref("time"), methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, inner);
        }

        public override object Evaluate(
            object startTimestamp, object endTimestamp, EventBean[] eventsPerStream, bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var start = ((DateTimeEx) startTimestamp).TimeInMillis;
            var end = ((DateTimeEx) endTimestamp).TimeInMillis;
            return intervalOp.Evaluate(start, end, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public static CodegenExpression Codegen(
            DTLocalDtxIntervalForge forge, CodegenExpressionRef start, CodegenExpressionRef end,
            CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope
                .MakeChild(typeof(bool?), typeof(DTLocalDtxIntervalEval), codegenClassScope)
                .AddParam(typeof(DateTimeEx), "start").AddParam(typeof(DateTimeEx), "end");

            methodNode.Block.MethodReturn(
                forge.intervalForge.Codegen(
                    ExprDotMethod(Ref("start"), "getTimeInMillis"), ExprDotMethod(Ref("end"), "getTimeInMillis"),
                    methodNode, exprSymbol, codegenClassScope));
            return LocalMethod(methodNode, start, end);
        }
    }
} // end of namespace
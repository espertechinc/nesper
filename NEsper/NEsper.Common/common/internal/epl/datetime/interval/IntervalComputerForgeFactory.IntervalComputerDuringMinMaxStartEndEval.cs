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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalComputerForgeFactory
    {
        public class IntervalComputerDuringMinMaxStartEndEval : IntervalComputerEval
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprEvaluator maxEndEval;
            internal readonly IntervalDeltaExprEvaluator maxStartEval;
            internal readonly IntervalDeltaExprEvaluator minEndEval;
            internal readonly IntervalDeltaExprEvaluator minStartEval;

            public IntervalComputerDuringMinMaxStartEndEval(
                bool during,
                IntervalDeltaExprEvaluator minStartEval,
                IntervalDeltaExprEvaluator maxStartEval,
                IntervalDeltaExprEvaluator minEndEval,
                IntervalDeltaExprEvaluator maxEndEval)
            {
                this.during = during;
                this.minStartEval = minStartEval;
                this.maxStartEval = maxStartEval;
                this.minEndEval = minEndEval;
                this.maxEndEval = maxEndEval;
            }

            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                long minStart = minStartEval.Evaluate(rightStart, eventsPerStream, newData, context);
                long maxStart = maxStartEval.Evaluate(rightStart, eventsPerStream, newData, context);
                long minEnd = minEndEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                long maxEnd = maxEndEval.Evaluate(rightEnd, eventsPerStream, newData, context);

                if (during) {
                    return IntervalComputerDuringAndIncludesMinMaxEval.ComputeIntervalDuring(
                        leftStart,
                        leftEnd,
                        rightStart,
                        rightEnd,
                        minStart,
                        maxStart,
                        minEnd,
                        maxEnd);
                }

                return IntervalComputerDuringAndIncludesMinMaxEval.ComputeIntervalIncludes(
                    leftStart,
                    leftEnd,
                    rightStart,
                    rightEnd,
                    minStart,
                    maxStart,
                    minEnd,
                    maxEnd);
            }

            public static CodegenExpression Codegen(
                IntervalComputerDuringMinMaxStartEndForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool),
                        typeof(IntervalComputerDuringMinMaxStartEndEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "minStart",
                        forge.minStartEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "maxStart",
                        forge.maxStartEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "minEnd",
                        forge.minEndEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTEND,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "maxEnd",
                        forge.maxEndEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTEND,
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(IntervalComputerDuringAndIncludesMinMaxEval),
                        forge.during ? "computeIntervalDuring" : "computeIntervalIncludes",
                        IntervalForgeCodegenNames.REF_LEFTSTART,
                        IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART,
                        IntervalForgeCodegenNames.REF_RIGHTEND,
                        CodegenExpressionBuilder.Ref("minStart"),
                        CodegenExpressionBuilder.Ref("maxStart"),
                        CodegenExpressionBuilder.Ref("minEnd"),
                        CodegenExpressionBuilder.Ref("maxEnd")));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
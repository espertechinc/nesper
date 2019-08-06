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
        public class IntervalComputerOverlapsAndByMinMaxEval : IntervalComputerEval
        {
            internal readonly IntervalDeltaExprEvaluator maxEval;
            internal readonly IntervalDeltaExprEvaluator minEval;

            internal readonly bool overlaps;

            public IntervalComputerOverlapsAndByMinMaxEval(
                bool overlaps,
                IntervalDeltaExprEvaluator minEval,
                IntervalDeltaExprEvaluator maxEval)
            {
                this.overlaps = overlaps;
                this.minEval = minEval;
                this.maxEval = maxEval;
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
                if (overlaps) {
                    long minThreshold = minEval.Evaluate(leftStart, eventsPerStream, newData, context);
                    long maxThreshold = maxEval.Evaluate(leftEnd, eventsPerStream, newData, context);
                    return IntervalComputerOverlapsAndByThresholdEval.ComputeIntervalOverlaps(
                        leftStart,
                        leftEnd,
                        rightStart,
                        rightEnd,
                        minThreshold,
                        maxThreshold);
                }
                else {
                    long minThreshold = minEval.Evaluate(rightStart, eventsPerStream, newData, context);
                    long maxThreshold = maxEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                    return IntervalComputerOverlapsAndByThresholdEval.ComputeIntervalOverlaps(
                        rightStart,
                        rightEnd,
                        leftStart,
                        leftEnd,
                        minThreshold,
                        maxThreshold);
                }
            }

            public static CodegenExpression Codegen(
                IntervalComputerOverlapsAndByMinMaxForge forge,
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
                        typeof(IntervalComputerOverlapsAndByMinMaxEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "minThreshold",
                        forge.minEval.Codegen(
                            forge.overlaps
                                ? IntervalForgeCodegenNames.REF_LEFTSTART
                                : IntervalForgeCodegenNames.REF_RIGHTSTART,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "maxThreshold",
                        forge.maxEval.Codegen(
                            forge.overlaps
                                ? IntervalForgeCodegenNames.REF_LEFTEND
                                : IntervalForgeCodegenNames.REF_RIGHTEND,
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                if (forge.overlaps) {
                    block.MethodReturn(
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(IntervalComputerOverlapsAndByThresholdEval),
                            "ComputeIntervalOverlaps",
                            IntervalForgeCodegenNames.REF_LEFTSTART,
                            IntervalForgeCodegenNames.REF_LEFTEND,
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
                            IntervalForgeCodegenNames.REF_RIGHTEND,
                            CodegenExpressionBuilder.Ref("minThreshold"),
                            CodegenExpressionBuilder.Ref("maxThreshold")));
                }
                else {
                    block.MethodReturn(
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(IntervalComputerOverlapsAndByThresholdEval),
                            "ComputeIntervalOverlaps",
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
                            IntervalForgeCodegenNames.REF_RIGHTEND,
                            IntervalForgeCodegenNames.REF_LEFTSTART,
                            IntervalForgeCodegenNames.REF_LEFTEND,
                            CodegenExpressionBuilder.Ref("minThreshold"),
                            CodegenExpressionBuilder.Ref("maxThreshold")));
                }

                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
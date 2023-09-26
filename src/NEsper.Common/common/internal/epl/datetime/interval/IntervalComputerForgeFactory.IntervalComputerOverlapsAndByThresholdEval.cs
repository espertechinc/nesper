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
        public class IntervalComputerOverlapsAndByThresholdEval : IntervalComputerEval
        {
            internal readonly bool overlaps;
            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerOverlapsAndByThresholdEval(
                bool overlaps,
                IntervalDeltaExprEvaluator thresholdExpr)
            {
                this.overlaps = overlaps;
                this.thresholdExpr = thresholdExpr;
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
                    var threshold = thresholdExpr.Evaluate(leftStart, eventsPerStream, newData, context);
                    return ComputeIntervalOverlaps(leftStart, leftEnd, rightStart, rightEnd, 0, threshold);
                }
                else {
                    var threshold = thresholdExpr.Evaluate(rightStart, eventsPerStream, newData, context);
                    return ComputeIntervalOverlaps(rightStart, rightEnd, leftStart, leftEnd, 0, threshold);
                }
            }

            public static CodegenExpression Codegen(
                IntervalComputerOverlapsAndByThreshold forge,
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
                        typeof(IntervalComputerOverlapsAndByThresholdEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "threshold",
                        forge.thresholdExpr.Codegen(
                            forge.overlaps
                                ? IntervalForgeCodegenNames.REF_LEFTSTART
                                : IntervalForgeCodegenNames.REF_RIGHTSTART,
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
                            CodegenExpressionBuilder.Constant(0),
                            CodegenExpressionBuilder.Ref("threshold")));
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
                            CodegenExpressionBuilder.Constant(0),
                            CodegenExpressionBuilder.Ref("threshold")));
                }

                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="left">left start</param>
            /// <param name="leftEnd">left end</param>
            /// <param name="right">right start</param>
            /// <param name="rightEnd">right end</param>
            /// <param name="min">min</param>
            /// <param name="max">max</param>
            /// <returns>flag</returns>
            public static bool ComputeIntervalOverlaps(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long min,
                long max)
            {
                var match = left < right &&
                            right < leftEnd &&
                            leftEnd < rightEnd;
                if (!match) {
                    return false;
                }

                var delta = leftEnd - right;
                return min <= delta && delta <= max;
            }
        }
    }
}
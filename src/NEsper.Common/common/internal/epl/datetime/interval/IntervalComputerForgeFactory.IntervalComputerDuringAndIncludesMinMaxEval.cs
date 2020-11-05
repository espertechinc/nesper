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
        public class IntervalComputerDuringAndIncludesMinMaxEval : IntervalComputerEval
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprEvaluator maxEval;
            internal readonly IntervalDeltaExprEvaluator minEval;

            public IntervalComputerDuringAndIncludesMinMaxEval(
                bool during,
                IntervalDeltaExprEvaluator minEval,
                IntervalDeltaExprEvaluator maxEval)
            {
                this.during = during;
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
                long min = minEval.Evaluate(leftStart, eventsPerStream, newData, context);
                long max = maxEval.Evaluate(rightEnd, eventsPerStream, newData, context);
                if (during) {
                    return ComputeIntervalDuring(leftStart, leftEnd, rightStart, rightEnd, min, max, min, max);
                }

                return ComputeIntervalIncludes(leftStart, leftEnd, rightStart, rightEnd, min, max, min, max);
            }

            public static CodegenExpression Codegen(
                IntervalComputerDuringAndIncludesMinMax forge,
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
                        typeof(IntervalComputerDuringAndIncludesMinMaxEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "min",
                        forge.minEval.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTSTART,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "max",
                        forge.maxEval.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTEND,
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(IntervalComputerDuringAndIncludesMinMaxEval),
                        forge.during ? "ComputeIntervalDuring" : "ComputeIntervalIncludes",
                        IntervalForgeCodegenNames.REF_LEFTSTART,
                        IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART,
                        IntervalForgeCodegenNames.REF_RIGHTEND,
                        CodegenExpressionBuilder.Ref("min"),
                        CodegenExpressionBuilder.Ref("max"),
                        CodegenExpressionBuilder.Ref("min"),
                        CodegenExpressionBuilder.Ref("max")));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }

            public static bool ComputeIntervalDuring(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startMin,
                long startMax,
                long endMin,
                long endMax)
            {
                if (startMin <= 0) {
                    startMin = 1;
                }

                var deltaStart = left - right;
                if (deltaStart < startMin || deltaStart > startMax) {
                    return false;
                }

                var deltaEnd = rightEnd - leftEnd;
                return !(deltaEnd < endMin || deltaEnd > endMax);
            }

            public static bool ComputeIntervalIncludes(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startMin,
                long startMax,
                long endMin,
                long endMax)
            {
                if (startMin <= 0) {
                    startMin = 1;
                }

                var deltaStart = right - left;
                if (deltaStart < startMin || deltaStart > startMax) {
                    return false;
                }

                var deltaEnd = leftEnd - rightEnd;
                return !(deltaEnd < endMin || deltaEnd > endMax);
            }
        }
    }
}
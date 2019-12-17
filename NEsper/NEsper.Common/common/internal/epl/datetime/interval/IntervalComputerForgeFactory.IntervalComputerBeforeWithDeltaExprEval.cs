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
        public class IntervalComputerBeforeWithDeltaExprEval : IntervalComputerEval
        {
            private readonly IntervalDeltaExprEvaluator finish;

            private readonly IntervalDeltaExprEvaluator start;

            public IntervalComputerBeforeWithDeltaExprEval(
                IntervalDeltaExprEvaluator start,
                IntervalDeltaExprEvaluator finish)
            {
                this.start = start;
                this.finish = finish;
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
                long rangeStartDelta = start.Evaluate(leftEnd, eventsPerStream, newData, context);
                long rangeEndDelta = finish.Evaluate(leftEnd, eventsPerStream, newData, context);
                if (rangeStartDelta > rangeEndDelta) {
                    return IntervalComputerConstantBefore.ComputeIntervalBefore(
                        leftEnd,
                        rightStart,
                        rangeEndDelta,
                        rangeStartDelta);
                }

                return IntervalComputerConstantBefore.ComputeIntervalBefore(
                    leftEnd,
                    rightStart,
                    rangeStartDelta,
                    rangeEndDelta);
            }

            public static CodegenExpression Codegen(
                IntervalComputerBeforeWithDeltaExprForge forge,
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
                        typeof(IntervalComputerBeforeWithDeltaExprEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "RangeStartDelta",
                        forge.start.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTEND,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "RangeEndDelta",
                        forge.finish.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTEND,
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                block.IfCondition(
                        CodegenExpressionBuilder.Relational(
                            CodegenExpressionBuilder.Ref("RangeStartDelta"),
                            CodegenExpressionRelational.CodegenRelational.GT,
                            CodegenExpressionBuilder.Ref("RangeEndDelta")))
                    .BlockReturn(
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(IntervalComputerConstantBefore),
                            "ComputeIntervalBefore",
                            IntervalForgeCodegenNames.REF_LEFTEND,
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
                            CodegenExpressionBuilder.Ref("RangeEndDelta"),
                            CodegenExpressionBuilder.Ref("RangeStartDelta")));
                block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(IntervalComputerConstantBefore),
                        "ComputeIntervalBefore",
                        IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART,
                        CodegenExpressionBuilder.Ref("RangeStartDelta"),
                        CodegenExpressionBuilder.Ref("RangeEndDelta")));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
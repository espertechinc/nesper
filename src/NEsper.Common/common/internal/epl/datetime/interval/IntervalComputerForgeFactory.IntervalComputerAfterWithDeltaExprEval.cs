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
        public class IntervalComputerAfterWithDeltaExprEval : IntervalComputerEval
        {
            private readonly IntervalDeltaExprEvaluator finish;
            private readonly IntervalDeltaExprEvaluator start;

            public IntervalComputerAfterWithDeltaExprEval(
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
                var rangeStartDelta = start.Evaluate(rightStart, eventsPerStream, newData, context);
                var rangeEndDelta = finish.Evaluate(rightStart, eventsPerStream, newData, context);
                if (rangeStartDelta > rangeEndDelta) {
                    return IntervalComputerConstantAfter.ComputeIntervalAfter(
                        leftStart,
                        rightEnd,
                        rangeEndDelta,
                        rangeStartDelta);
                }

                return IntervalComputerConstantAfter.ComputeIntervalAfter(
                    leftStart,
                    rightEnd,
                    rangeStartDelta,
                    rangeEndDelta);
            }

            public static CodegenExpression Codegen(
                IntervalComputerAfterWithDeltaExprForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool), typeof(IntervalComputerAfterWithDeltaExprEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "RangeStartDelta",
                        forge.start.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "RangeEndDelta",
                        forge.finish.Codegen(
                            IntervalForgeCodegenNames.REF_RIGHTSTART,
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
                            typeof(IntervalComputerConstantAfter),
                            "ComputeIntervalAfter",
                            IntervalForgeCodegenNames.REF_LEFTSTART,
                            IntervalForgeCodegenNames.REF_RIGHTEND,
                            CodegenExpressionBuilder.Ref("RangeEndDelta"),
                            CodegenExpressionBuilder.Ref("RangeStartDelta")));
                block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(IntervalComputerConstantAfter),
                        "ComputeIntervalAfter",
                        IntervalForgeCodegenNames.REF_LEFTSTART,
                        IntervalForgeCodegenNames.REF_RIGHTEND,
                        CodegenExpressionBuilder.Ref("RangeStartDelta"),
                        CodegenExpressionBuilder.Ref("RangeEndDelta")));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
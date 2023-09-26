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
        public class IntervalComputerDuringAndIncludesThresholdEval : IntervalComputerEval
        {
            internal readonly bool during;
            internal readonly IntervalDeltaExprEvaluator threshold;

            public IntervalComputerDuringAndIncludesThresholdEval(
                bool during,
                IntervalDeltaExprEvaluator threshold)
            {
                this.during = during;
                this.threshold = threshold;
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
                var thresholdValue = threshold.Evaluate(leftStart, eventsPerStream, newData, context);

                if (during) {
                    var deltaStart = leftStart - rightStart;
                    if (deltaStart <= 0 || deltaStart > thresholdValue) {
                        return false;
                    }

                    var deltaEnd = rightEnd - leftEnd;
                    return !(deltaEnd <= 0 || deltaEnd > thresholdValue);
                }
                else {
                    var deltaStart = rightStart - leftStart;
                    if (deltaStart <= 0 || deltaStart > thresholdValue) {
                        return false;
                    }

                    var deltaEnd = leftEnd - rightEnd;
                    return !(deltaEnd <= 0 || deltaEnd > thresholdValue);
                }
            }

            public static CodegenExpression Codegen(
                IntervalComputerDuringAndIncludesThresholdForge forge,
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
                        typeof(IntervalComputerDuringAndIncludesThresholdEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "thresholdValue",
                        forge.threshold.Codegen(
                            IntervalForgeCodegenNames.REF_LEFTSTART,
                            methodNode,
                            exprSymbol,
                            codegenClassScope));

                if (forge.during) {
                    block.DeclareVar<long>(
                            "deltaStart",
                            CodegenExpressionBuilder.Op(
                                IntervalForgeCodegenNames.REF_LEFTSTART,
                                "-",
                                IntervalForgeCodegenNames.REF_RIGHTSTART))
                        .IfConditionReturnConst(
                            CodegenExpressionBuilder.Or(
                                CodegenExpressionBuilder.Relational(
                                    CodegenExpressionBuilder.Ref("deltaStart"),
                                    CodegenExpressionRelational.CodegenRelational.LE,
                                    CodegenExpressionBuilder.Constant(0)),
                                CodegenExpressionBuilder.Relational(
                                    CodegenExpressionBuilder.Ref("deltaStart"),
                                    CodegenExpressionRelational.CodegenRelational.GT,
                                    CodegenExpressionBuilder.Ref("thresholdValue"))),
                            false)
                        .DeclareVar<long>(
                            "deltaEnd",
                            CodegenExpressionBuilder.Op(
                                IntervalForgeCodegenNames.REF_RIGHTEND,
                                "-",
                                IntervalForgeCodegenNames.REF_LEFTEND))
                        .MethodReturn(
                            CodegenExpressionBuilder.Not(
                                CodegenExpressionBuilder.Or(
                                    CodegenExpressionBuilder.Relational(
                                        CodegenExpressionBuilder.Ref("deltaEnd"),
                                        CodegenExpressionRelational.CodegenRelational.LE,
                                        CodegenExpressionBuilder.Constant(0)),
                                    CodegenExpressionBuilder.Relational(
                                        CodegenExpressionBuilder.Ref("deltaEnd"),
                                        CodegenExpressionRelational.CodegenRelational.GT,
                                        CodegenExpressionBuilder.Ref("thresholdValue")))));
                }
                else {
                    block.DeclareVar<long>(
                            "deltaStart",
                            CodegenExpressionBuilder.Op(
                                IntervalForgeCodegenNames.REF_RIGHTSTART,
                                "-",
                                IntervalForgeCodegenNames.REF_LEFTSTART))
                        .IfConditionReturnConst(
                            CodegenExpressionBuilder.Or(
                                CodegenExpressionBuilder.Relational(
                                    CodegenExpressionBuilder.Ref("deltaStart"),
                                    CodegenExpressionRelational.CodegenRelational.LE,
                                    CodegenExpressionBuilder.Constant(0)),
                                CodegenExpressionBuilder.Relational(
                                    CodegenExpressionBuilder.Ref("deltaStart"),
                                    CodegenExpressionRelational.CodegenRelational.GT,
                                    CodegenExpressionBuilder.Ref("thresholdValue"))),
                            false)
                        .DeclareVar<long>(
                            "deltaEnd",
                            CodegenExpressionBuilder.Op(
                                IntervalForgeCodegenNames.REF_LEFTEND,
                                "-",
                                IntervalForgeCodegenNames.REF_RIGHTEND))
                        .MethodReturn(
                            CodegenExpressionBuilder.Not(
                                CodegenExpressionBuilder.Or(
                                    CodegenExpressionBuilder.Relational(
                                        CodegenExpressionBuilder.Ref("deltaEnd"),
                                        CodegenExpressionRelational.CodegenRelational.LE,
                                        CodegenExpressionBuilder.Constant(0)),
                                    CodegenExpressionBuilder.Relational(
                                        CodegenExpressionBuilder.Ref("deltaEnd"),
                                        CodegenExpressionRelational.CodegenRelational.GT,
                                        CodegenExpressionBuilder.Ref("thresholdValue")))));
                }

                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
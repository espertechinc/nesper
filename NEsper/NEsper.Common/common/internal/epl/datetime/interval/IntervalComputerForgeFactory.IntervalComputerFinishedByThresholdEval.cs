///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public partial class IntervalComputerForgeFactory
    {
        public class IntervalComputerFinishedByThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALFINISHEDBYTHRESHOLD = "logWarningIntervalFinishedByThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerFinishedByThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
            {
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
                long threshold = thresholdExpr.Evaluate(Math.Min(rightEnd, leftEnd), eventsPerStream, newData, context);
                if (threshold < 0) {
                    LogWarningIntervalFinishedByThreshold();
                    return null;
                }

                if (leftStart >= rightStart) {
                    return false;
                }

                var delta = Math.Abs(leftEnd - rightEnd);
                return delta <= threshold;
            }

            public static void LogWarningIntervalFinishedByThreshold()
            {
                Log.Warn("The 'finishes' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerFinishedByThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?),
                        typeof(IntervalComputerFinishedByThresholdEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar<long>(
                        "threshold",
                        forge.thresholdExpr.Codegen(
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(Math),
                                "min",
                                IntervalForgeCodegenNames.REF_RIGHTEND,
                                IntervalForgeCodegenNames.REF_LEFTEND),
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .IfCondition(
                        CodegenExpressionBuilder.Relational(
                            CodegenExpressionBuilder.Ref("threshold"),
                            CodegenExpressionRelational.CodegenRelational.LT,
                            CodegenExpressionBuilder.Constant(0)))
                    .StaticMethod(
                        typeof(IntervalComputerFinishedByThresholdEval),
                        METHOD_LOGWARNINGINTERVALFINISHEDBYTHRESHOLD)
                    .BlockReturn(CodegenExpressionBuilder.ConstantNull())
                    .IfConditionReturnConst(
                        CodegenExpressionBuilder.Relational(
                            IntervalForgeCodegenNames.REF_LEFTSTART,
                            CodegenExpressionRelational.CodegenRelational.GE,
                            IntervalForgeCodegenNames.REF_RIGHTSTART),
                        false)
                    .DeclareVar<long>(
                        "delta",
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(Math),
                            "abs",
                            CodegenExpressionBuilder.Op(
                                IntervalForgeCodegenNames.REF_LEFTEND,
                                "-",
                                IntervalForgeCodegenNames.REF_RIGHTEND)))
                    .MethodReturn(
                        CodegenExpressionBuilder.Relational(
                            CodegenExpressionBuilder.Ref("delta"),
                            CodegenExpressionRelational.CodegenRelational.LE,
                            CodegenExpressionBuilder.Ref("threshold")));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
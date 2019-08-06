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
        public class IntervalComputerMeetsThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALMEETSTHRESHOLD = "logWarningIntervalMeetsThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerMeetsThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
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
                long threshold = thresholdExpr.Evaluate(
                    Math.Min(leftEnd, rightStart),
                    eventsPerStream,
                    newData,
                    context);
                if (threshold < 0) {
                    LogWarningIntervalMeetsThreshold();
                    return null;
                }

                var delta = Math.Abs(rightStart - leftEnd);
                return delta <= threshold;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalMeetsThreshold()
            {
                Log.Warn("The 'meets' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerMeetsThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalComputerMeetsThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar<long>(
                        "threshold",
                        forge.thresholdExpr.Codegen(
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(Math),
                                "Min",
                                IntervalForgeCodegenNames.REF_LEFTEND,
                                IntervalForgeCodegenNames.REF_RIGHTSTART),
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .IfCondition(
                        CodegenExpressionBuilder.Relational(
                            CodegenExpressionBuilder.Ref("threshold"),
                            CodegenExpressionRelational.CodegenRelational.LT,
                            CodegenExpressionBuilder.Constant(0)))
                    .StaticMethod(typeof(IntervalComputerMeetsThresholdEval), METHOD_LOGWARNINGINTERVALMEETSTHRESHOLD)
                    .BlockReturn(CodegenExpressionBuilder.ConstantNull())
                    .DeclareVar<long>(
                        "delta",
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(Math),
                            "Abs",
                            CodegenExpressionBuilder.Op(
                                IntervalForgeCodegenNames.REF_RIGHTSTART,
                                "-",
                                IntervalForgeCodegenNames.REF_LEFTEND)))
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
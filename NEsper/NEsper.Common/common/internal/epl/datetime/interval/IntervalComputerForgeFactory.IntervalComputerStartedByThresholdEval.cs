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
        public class IntervalComputerStartedByThresholdEval : IntervalComputerEval
        {
            public const string METHOD_LOGWARNINGINTERVALSTARTEDBYTHRESHOLD = "logWarningIntervalStartedByThreshold";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            internal readonly IntervalDeltaExprEvaluator thresholdExpr;

            public IntervalComputerStartedByThresholdEval(IntervalDeltaExprEvaluator thresholdExpr)
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
                    Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                if (threshold < 0)
                {
                    LogWarningIntervalStartedByThreshold();
                    return null;
                }

                var delta = Math.Abs(leftStart - rightStart);
                return delta <= threshold && leftEnd > rightEnd;
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void LogWarningIntervalStartedByThreshold()
            {
                Log.Warn("The 'started-by' date-time method does not allow negative threshold");
            }

            public static CodegenExpression Codegen(
                IntervalComputerStartedByThresholdForge forge,
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?), typeof(IntervalComputerStartedByThresholdEval), codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                methodNode.Block
                    .DeclareVar(
                        typeof(long), "threshold",
                        forge.thresholdExpr.Codegen(
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(Math), "min", IntervalForgeCodegenNames.REF_LEFTSTART,
                                IntervalForgeCodegenNames.REF_RIGHTSTART), methodNode, exprSymbol, codegenClassScope))
                    .IfCondition(CodegenExpressionBuilder.Relational(CodegenExpressionBuilder.Ref("threshold"), CodegenExpressionRelational.CodegenRelational.LT, CodegenExpressionBuilder.Constant(0)))
                    .StaticMethod(
                        typeof(IntervalComputerStartedByThresholdEval), METHOD_LOGWARNINGINTERVALSTARTEDBYTHRESHOLD)
                    .BlockReturn(CodegenExpressionBuilder.ConstantNull())
                    .DeclareVar(
                        typeof(long), "delta",
                        CodegenExpressionBuilder.StaticMethod(
                            typeof(Math), "abs",
                            CodegenExpressionBuilder.Op(IntervalForgeCodegenNames.REF_LEFTSTART, "-", IntervalForgeCodegenNames.REF_RIGHTSTART)))
                    .MethodReturn(
                        CodegenExpressionBuilder.And(
                            CodegenExpressionBuilder.Relational(CodegenExpressionBuilder.Ref("delta"), CodegenExpressionRelational.CodegenRelational.LE, CodegenExpressionBuilder.Ref("threshold")),
                            CodegenExpressionBuilder.Relational(
                                IntervalForgeCodegenNames.REF_LEFTEND, CodegenExpressionRelational.CodegenRelational.GT, IntervalForgeCodegenNames.REF_RIGHTEND)));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }
        }
    }
}
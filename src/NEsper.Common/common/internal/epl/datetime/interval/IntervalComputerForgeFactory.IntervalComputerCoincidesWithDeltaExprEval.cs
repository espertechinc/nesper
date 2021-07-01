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
        public class IntervalComputerCoincidesWithDeltaExprEval : IntervalComputerEval
        {
            public const string METHOD_WARNCOINCIDESTARTENDLESSZERO = "WarnCoincideStartEndLessZero";

            private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

            private readonly IntervalDeltaExprEvaluator finish;

            private readonly IntervalDeltaExprEvaluator start;

            public IntervalComputerCoincidesWithDeltaExprEval(
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
                long startValue = start.Evaluate(Math.Min(leftStart, rightStart), eventsPerStream, newData, context);
                long endValue = finish.Evaluate(Math.Min(leftEnd, rightEnd), eventsPerStream, newData, context);

                if (startValue < 0 || endValue < 0) {
                    Log.Warn("The coincides date-time method does not allow negative start and end values");
                    return null;
                }

                return IntervalComputerConstantCoincides.ComputeIntervalCoincides(
                    leftStart,
                    leftEnd,
                    rightStart,
                    rightEnd,
                    startValue,
                    endValue);
            }

            public static CodegenExpression Codegen(
                IntervalComputerCoincidesWithDeltaExprForge forge,
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
                        typeof(IntervalComputerCoincidesWithDeltaExprEval),
                        codegenClassScope)
                    .AddParam(IntervalForgeCodegenNames.PARAMS);

                var block = methodNode.Block
                    .DeclareVar<long>(
                        "startValue",
                        forge.start.Codegen(
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(Math),
                                "Min",
                                IntervalForgeCodegenNames.REF_LEFTSTART,
                                IntervalForgeCodegenNames.REF_RIGHTSTART),
                            methodNode,
                            exprSymbol,
                            codegenClassScope))
                    .DeclareVar<long>(
                        "endValue",
                        forge.finish.Codegen(
                            CodegenExpressionBuilder.StaticMethod(
                                typeof(Math),
                                "Min",
                                IntervalForgeCodegenNames.REF_LEFTEND,
                                IntervalForgeCodegenNames.REF_RIGHTEND),
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                block.IfCondition(
                        CodegenExpressionBuilder.Or(
                            CodegenExpressionBuilder.Relational(
                                CodegenExpressionBuilder.Ref("startValue"),
                                CodegenExpressionRelational.CodegenRelational.LT,
                                CodegenExpressionBuilder.Constant(0)),
                            CodegenExpressionBuilder.Relational(
                                CodegenExpressionBuilder.Ref("endValue"),
                                CodegenExpressionRelational.CodegenRelational.LT,
                                CodegenExpressionBuilder.Constant(0))))
                    .StaticMethod(
                        typeof(IntervalComputerCoincidesWithDeltaExprEval),
                        METHOD_WARNCOINCIDESTARTENDLESSZERO)
                    .BlockReturn(CodegenExpressionBuilder.ConstantNull());
                block.MethodReturn(
                    CodegenExpressionBuilder.StaticMethod(
                        typeof(IntervalComputerConstantCoincides),
                        "ComputeIntervalCoincides",
                        IntervalForgeCodegenNames.REF_LEFTSTART,
                        IntervalForgeCodegenNames.REF_LEFTEND,
                        IntervalForgeCodegenNames.REF_RIGHTSTART,
                        IntervalForgeCodegenNames.REF_RIGHTEND,
                        CodegenExpressionBuilder.Ref("startValue"),
                        CodegenExpressionBuilder.Ref("endValue")));
                return CodegenExpressionBuilder.LocalMethod(methodNode, leftStart, leftEnd, rightStart, rightEnd);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            public static void WarnCoincideStartEndLessZero()
            {
                Log.Warn("The coincides date-time method does not allow negative start and end values");
            }
        }
    }
}
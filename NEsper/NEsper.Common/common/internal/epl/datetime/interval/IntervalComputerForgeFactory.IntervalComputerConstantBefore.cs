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
        /// <summary>
        ///     Before.
        /// </summary>
        public class IntervalComputerConstantBefore : IntervalComputerConstantBase,
            IntervalComputerForge,
            IntervalComputerEval
        {
            public IntervalComputerConstantBefore(IntervalStartEndParameterPairForge pair)
                : base(pair, true)
            {
            }

            public IntervalComputerEval MakeComputerEval()
            {
                return this;
            }

            public CodegenExpression Codegen(
                CodegenExpression leftStart,
                CodegenExpression leftEnd,
                CodegenExpression rightStart,
                CodegenExpression rightEnd,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return CodegenExpressionBuilder.StaticMethod(
                    typeof(IntervalComputerConstantBefore), "computeIntervalBefore", leftEnd, rightStart,
                    CodegenExpressionBuilder.Constant(start), CodegenExpressionBuilder.Constant(end));
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
                return ComputeIntervalBefore(leftEnd, rightStart, start, end);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="leftEnd">left end</param>
            /// <param name="right">right</param>
            /// <param name="start">start</param>
            /// <param name="end">end</param>
            /// <returns>flag</returns>
            public static bool ComputeIntervalBefore(
                long leftEnd,
                long right,
                long start,
                long end)
            {
                var delta = right - leftEnd;
                return start <= delta && delta <= end;
            }
        }
    }
}
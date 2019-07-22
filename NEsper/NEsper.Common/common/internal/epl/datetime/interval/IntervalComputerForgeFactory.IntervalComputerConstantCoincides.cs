///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

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
        ///     Coincides.
        /// </summary>
        public class IntervalComputerConstantCoincides : IntervalComputerForge,
            IntervalComputerEval
        {
            internal readonly long end;
            internal readonly long start;

            public IntervalComputerConstantCoincides(IntervalStartEndParameterPairForge pair)
            {
                start = pair.Start.OptionalConstant.GetValueOrDefault();
                end = pair.End.OptionalConstant.GetValueOrDefault();
                if (start < 0 || end < 0) {
                    throw new ExprValidationException(
                        "The coincides date-time method does not allow negative start and end values");
                }
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
                    typeof(IntervalComputerConstantCoincides),
                    "computeIntervalCoincides",
                    leftStart,
                    leftEnd,
                    rightStart,
                    rightEnd,
                    CodegenExpressionBuilder.Constant(start),
                    CodegenExpressionBuilder.Constant(end));
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
                return ComputeIntervalCoincides(leftStart, leftEnd, rightStart, rightEnd, start, end);
            }

            /// <summary>
            ///     NOTE: Code-generation-invoked method, method name and parameter order matters
            /// </summary>
            /// <param name="left">left start</param>
            /// <param name="leftEnd">left end</param>
            /// <param name="right">right start</param>
            /// <param name="rightEnd">right end</param>
            /// <param name="startThreshold">start th</param>
            /// <param name="endThreshold">end th</param>
            /// <returns>flag</returns>
            public static bool ComputeIntervalCoincides(
                long left,
                long leftEnd,
                long right,
                long rightEnd,
                long startThreshold,
                long endThreshold)
            {
                return Math.Abs(left - right) <= startThreshold &&
                       Math.Abs(leftEnd - rightEnd) <= endThreshold;
            }
        }
    }
}
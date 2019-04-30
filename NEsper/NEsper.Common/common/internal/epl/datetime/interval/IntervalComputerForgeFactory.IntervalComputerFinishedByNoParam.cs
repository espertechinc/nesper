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
        ///     Finishes-By.
        /// </summary>
        public class IntervalComputerFinishedByNoParam : IntervalComputerForge,
            IntervalComputerEval
        {
            public bool? Compute(
                long leftStart,
                long leftEnd,
                long rightStart,
                long rightEnd,
                EventBean[] eventsPerStream,
                bool newData,
                ExprEvaluatorContext context)
            {
                return leftStart < rightStart && leftEnd == rightEnd;
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
                return CodegenExpressionBuilder.And(CodegenExpressionBuilder.Relational(leftStart, CodegenExpressionRelational.CodegenRelational.LT, rightStart), CodegenExpressionBuilder.EqualsIdentity(leftEnd, rightEnd));
            }
        }
    }
}
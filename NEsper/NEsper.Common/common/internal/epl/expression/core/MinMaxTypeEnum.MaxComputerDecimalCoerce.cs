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
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public partial class MinMaxTypeEnum
    {
        /// <summary>
        ///     Determines maximum using AsDecimal.
        /// </summary>
        public class MaxComputerDecimalCoerce : Computer
        {
            private readonly ExprEvaluator[] childNodes;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="childNodes">array of expression nodes</param>
            public MaxComputerDecimalCoerce(ExprEvaluator[] childNodes)
            {
                this.childNodes = childNodes;
            }

            public object Execute(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null)
                {
                    return null;
                }

                object result;
                if (valueChildOne.AsDecimal() > valueChildTwo.AsDecimal())
                {
                    result = valueChildOne;
                }
                else
                {
                    result = valueChildTwo;
                }

                for (var i = 2; i < childNodes.Length; i++)
                {
                    var valueChild = childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null)
                    {
                        return null;
                    }

                    if (valueChild.AsDecimal() > result.AsDecimal())
                    {
                        result = valueChild;
                    }
                }

                return result;
            }

            public static CodegenExpression Codegen(
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope,
                ExprNode[] nodes,
                Type returnType)
            {
                return CodegenMinMax(false, codegenMethodScope, exprSymbol, codegenClassScope, nodes, returnType);
            }
        }
    }
}
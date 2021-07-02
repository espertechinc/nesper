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
    public partial class MinMaxType
    {
        /// <summary>
        ///     Determines maximum using AsDouble.
        /// </summary>
        public class MaxComputerDoubleCoerce : Computer
        {
            private readonly ExprEvaluator[] _childNodes;

            /// <summary>
            ///     Ctor.
            /// </summary>
            /// <param name="childNodes">array of expression nodes</param>
            public MaxComputerDoubleCoerce(ExprEvaluator[] childNodes)
            {
                this._childNodes = childNodes;
            }

            public object Execute(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = _childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = _childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null) {
                    return null;
                }

                object result;
                if (valueChildOne.AsDouble() > valueChildTwo.AsDouble()) {
                    result = valueChildOne;
                }
                else {
                    result = valueChildTwo;
                }

                for (var i = 2; i < _childNodes.Length; i++) {
                    var valueChild = _childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null) {
                        return null;
                    }

                    if (valueChild.AsDouble() > result.AsDouble()) {
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
                return MinMaxTypeCodegen
                    .CodegenMinMax(false, codegenMethodScope, exprSymbol, codegenClassScope, nodes, returnType);
            }
        }
    }
}
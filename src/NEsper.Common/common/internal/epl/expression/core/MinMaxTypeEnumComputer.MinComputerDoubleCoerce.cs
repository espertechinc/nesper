using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public partial interface MinMaxTypeEnumComputer
    {
        /// <summary>
        /// Determines minimum using object.doubleValue().
        /// </summary>
        public class MinComputerDoubleCoerce : MinMaxTypeEnumComputer
        {
            private readonly ExprEvaluator[] childNodes;

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="childNodes">array of expression nodes</param>
            public MinComputerDoubleCoerce(ExprEvaluator[] childNodes)
            {
                this.childNodes = childNodes;
            }

            public object Execute(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0]
                    .Evaluate(eventsPerStream, isNewData, exprEvaluatorContext)
                    .AsBoxedDouble();
                var valueChildTwo = childNodes[1]
                    .Evaluate(eventsPerStream, isNewData, exprEvaluatorContext)
                    .AsBoxedDouble();

                if (valueChildOne == null || valueChildTwo == null) {
                    return null;
                }

                var result = valueChildOne.Value > valueChildTwo.Value ? valueChildTwo : valueChildOne;

                for (var i = 2; i < childNodes.Length; i++) {
                    var valueChild = childNodes[i]
                        .Evaluate(eventsPerStream, isNewData, exprEvaluatorContext)
                        .AsBoxedDouble();
                    if (valueChild == null) {
                        return null;
                    }

                    if (valueChild.Value < result.Value) {
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
                return CodegenMinMax(
                    true,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope,
                    nodes,
                    returnType);
            }
        }
    }
}
using System.Numerics;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public partial interface MinMaxTypeEnumComputer
    {
        /// <summary>
        /// Determines minimum/maximum using BigInteger.compareTo.
        /// </summary>
        public class MinMaxComputerBigIntCoerce : MinMaxTypeEnumComputer
        {
            private readonly ExprEvaluator[] childNodes;
            private readonly BigIntegerCoercer[] convertors;
            private readonly bool isMax;

            /// <summary>
            /// Ctor.
            /// </summary>
            /// <param name="childNodes">expressions</param>
            /// <param name="convertors">convertors to BigInteger</param>
            /// <param name="isMax">true if max, false if min</param>
            public MinMaxComputerBigIntCoerce(
                ExprEvaluator[] childNodes,
                BigIntegerCoercer[] convertors,
                bool isMax)
            {
                this.childNodes = childNodes;
                this.convertors = convertors;
                this.isMax = isMax;
            }

            public object Execute(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var valueChildOne = childNodes[0].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                var valueChildTwo = childNodes[1].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

                if (valueChildOne == null || valueChildTwo == null) {
                    return null;
                }

                var bigIntOne = convertors[0].CoerceBoxedBigInt(valueChildOne);
                var bigIntTwo = convertors[1].CoerceBoxedBigInt(valueChildTwo);

                BigInteger result;
                if ((isMax && bigIntOne.CompareTo(bigIntTwo) > 0) ||
                    (!isMax && bigIntOne.CompareTo(bigIntTwo) < 0)) {
                    result = bigIntOne;
                }
                else {
                    result = bigIntTwo;
                }

                for (var i = 2; i < childNodes.Length; i++) {
                    var valueChild =
                        childNodes[i].Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
                    if (valueChild == null) {
                        return null;
                    }

                    var bigInt = convertors[i].CoerceBoxedBigInt(valueChild);
                    if ((isMax && result.CompareTo(bigInt) < 0) ||
                        (!isMax && result.CompareTo(bigInt) > 0)) {
                        result = bigInt;
                    }
                }

                return result;
            }

            public static CodegenExpression Codegen(
                bool max,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope,
                ExprNode[] nodes,
                BigIntegerCoercer[] convertors)
            {
                var r0Type = nodes[0].Forge.EvaluationType;
                var r1Type = nodes[1].Forge.EvaluationType;
                if (r0Type == null || r1Type == null) {
                    return CodegenExpressionBuilder.ConstantNull();
                }

                var methodNode = codegenMethodScope.MakeChild(
                    typeof(BigInteger),
                    typeof(MinMaxType.ComputerBigIntCoerce),
                    codegenClassScope);
                var block = methodNode.Block;

                var r0TypeClass = r0Type;
                var r1TypeClass = r1Type;

                block.DeclareVar(
                    r0TypeClass,
                    "r0",
                    nodes[0].Forge.EvaluateCodegen(r0TypeClass, methodNode, exprSymbol, codegenClassScope));
                if (!r0TypeClass.IsPrimitive) {
                    block.IfRefNullReturnNull("r0");
                }

                block.DeclareVar(
                    r1TypeClass,
                    "r1",
                    nodes[1].Forge.EvaluateCodegen(r1TypeClass, methodNode, exprSymbol, codegenClassScope));
                if (!r1TypeClass.IsPrimitive) {
                    block.IfRefNullReturnNull("r1");
                }

                block.DeclareVar<BigInteger>(
                    "bi0",
                    convertors[0].CoerceBoxedBigIntCodegen(CodegenExpressionBuilder.Ref("r0"), r0TypeClass, codegenMethodScope, codegenClassScope));
                block.DeclareVar<BigInteger>(
                    "bi1",
                    convertors[1].CoerceBoxedBigIntCodegen(CodegenExpressionBuilder.Ref("r1"), r1TypeClass, codegenMethodScope, codegenClassScope));

                block.DeclareVarNoInit(typeof(BigInteger), "result");
                block.IfCondition(CodegenCompareCompareTo(CodegenExpressionBuilder.Ref("bi0"), CodegenExpressionBuilder.Ref("bi1"), max))
                    .AssignRef("result", CodegenExpressionBuilder.Ref("bi0"))
                    .IfElse()
                    .AssignRef("result", CodegenExpressionBuilder.Ref("bi1"))
                    .BlockEnd();

                for (var i = 2; i < nodes.Length; i++) {
                    var nodeType = nodes[i].Forge.EvaluationType;
                    var refnameNumber = "r" + i;
                    block.DeclareVar(
                        nodeType,
                        refnameNumber,
                        nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                    if (!nodeType.IsPrimitive) {
                        block.IfRefNullReturnNull(refnameNumber);
                    }

                    var refnameBigint = "bi" + i;
                    block.DeclareVar<BigInteger>(
                        refnameBigint,
                        convertors[i].CoerceBoxedBigIntCodegen(CodegenExpressionBuilder.Ref(refnameNumber), nodeType, codegenMethodScope, codegenClassScope));
                    block.IfCondition(CodegenExpressionBuilder.Not(CodegenCompareCompareTo(CodegenExpressionBuilder.Ref("result"), CodegenExpressionBuilder.Ref(refnameBigint), max)))
                        .AssignRef("result", CodegenExpressionBuilder.Ref(refnameBigint))
                        .BlockEnd();
                }

                block.MethodReturn(CodegenExpressionBuilder.Ref("result"));
                return CodegenExpressionBuilder.LocalMethod(methodNode);
            }
        }
    }
}
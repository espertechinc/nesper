using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Executes child expression nodes and compares results.
    /// </summary>
    public partial interface MinMaxTypeEnumComputer
    {
        /// <summary>
        /// Executes child expression nodes and compares results, returning the min/max.
        /// </summary>
        /// <param name="eventsPerStream">events per stream</param>
        /// <param name="isNewData">true if new data</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        /// <returns>result</returns>
        object Execute(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        private static CodegenExpression CodegenMinMax(
            bool min,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprNode[] nodes,
            Type returnType)
        {
            var r0Type = nodes[0].Forge.EvaluationType;
            var r1Type = nodes[1].Forge.EvaluationType;
            if (r0Type == null || r1Type == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                returnType,
                typeof(MinMaxType.MaxComputerDoubleCoerce),
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

            var codegenRelational = min ? RelationalOpEnum.LT : RelationalOpEnum.GT;
            
            block.DeclareVarNoInit(returnType, "result");
            block.IfCondition(
                    CodegenCompareRelop(
                        returnType,
                        codegenRelational,
                        Ref("r0"),
                        r0TypeClass,
                        Ref("r1"),
                        r1TypeClass))
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref("r0"), r0TypeClass, returnType))
                .IfElse()
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref("r1"), r1TypeClass, returnType))
                .BlockEnd();

            for (var i = 2; i < nodes.Length; i++) {
                var nodeType = nodes[i].Forge.EvaluationType;
                var refname = "r" + i;
                block.DeclareVar(
                    nodeType,
                    refname,
                    nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                if (!nodeType.IsPrimitive) {
                    block.IfRefNullReturnNull(refname);
                }
                
                block.IfCondition(
                        Not(
                            CodegenCompareRelop(
                                returnType,
                                codegenRelational,
                                Ref("result"),
                                returnType,
                                Ref(refname),
                                r1TypeClass)))
                    .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref(refname), nodeType, returnType))
                    .BlockEnd();
            }

            block.MethodReturn(Ref("result"));
            return LocalMethod(methodNode);
        }

        private static CodegenExpression CodegenCompareRelop(
            Type resultType,
            RelationalOpEnum op,
            CodegenExpressionRef lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType)
        {
            return CodegenExpressionBuilder.Op(lhs, op.GetExpressionText(), rhs);
        }

        private static CodegenExpression CodegenCompareCompareTo(
            CodegenExpression lhs,
            CodegenExpression rhs,
            bool max)
        {
            return Relational(
                ExprDotMethod(lhs, "CompareTo", rhs),
                max
                    ? CodegenExpressionRelational.CodegenRelational.GT
                    : CodegenExpressionRelational.CodegenRelational.LT,
                Constant(0));
        }
    }
}
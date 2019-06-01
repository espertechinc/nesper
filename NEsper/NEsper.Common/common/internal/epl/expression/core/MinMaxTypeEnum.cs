///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    ///     Enumeration for the type of arithmetic to use.
    /// </summary>
    public partial class MinMaxTypeEnum
    {
        /// <summary>
        ///     Max.
        /// </summary>
        public static readonly MinMaxTypeEnum MAX = new MinMaxTypeEnum("max");

        /// <summary>
        ///     Min.
        /// </summary>
        public static readonly MinMaxTypeEnum MIN = new MinMaxTypeEnum("min");

        private MinMaxTypeEnum(string expressionText)
        {
            ExpressionText = expressionText;
        }

        /// <summary>
        ///     Returns textual representation of enum.
        /// </summary>
        /// <returns>text for enum</returns>
        public string ExpressionText { get; }

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
            if (r0Type == null || r1Type == null)
            {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                returnType, typeof(MaxComputerDoubleCoerce), codegenClassScope);
            var block = methodNode.Block;

            block.DeclareVar(
                r0Type, "r0", nodes[0].Forge.EvaluateCodegen(r0Type, methodNode, exprSymbol, codegenClassScope));
            if (!r0Type.IsPrimitive)
            {
                block.IfRefNullReturnNull("r0");
            }

            block.DeclareVar(
                r1Type, "r1", nodes[1].Forge.EvaluateCodegen(r1Type, methodNode, exprSymbol, codegenClassScope));
            if (!r1Type.IsPrimitive)
            {
                block.IfRefNullReturnNull("r1");
            }

            block.DeclareVarNoInit(returnType, "result");
            block.IfCondition(
                    CodegenCompareRelop(
                        returnType, min ? RelationalOpEnum.LT : RelationalOpEnum.GT, Ref("r0"), r0Type, Ref("r1"),
                        r1Type))
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref("r0"), r0Type, returnType))
                .IfElse()
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(Ref("r1"), r1Type, returnType))
                .BlockEnd();

            for (var i = 2; i < nodes.Length; i++)
            {
                var nodeType = nodes[i].Forge.EvaluationType;
                var refname = "r" + i;
                block.DeclareVar(
                    nodeType, refname,
                    nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                if (!nodeType.IsPrimitive)
                {
                    block.IfRefNullReturnNull(refname);
                }

                block.IfCondition(
                        Not(
                            CodegenCompareRelop(
                                returnType, min ? RelationalOpEnum.LT : RelationalOpEnum.GT, Ref("result"), returnType,
                                Ref(refname), r1Type)))
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
            return Op(lhs, op.ExpressionText, rhs);
        }

        private static CodegenExpression CodegenCompareCompareTo(
            CodegenExpression lhs,
            CodegenExpression rhs,
            bool max)
        {
            return Relational(
                ExprDotMethod(lhs, "compareTo", rhs),
                max
                    ? CodegenExpressionRelational.CodegenRelational.GT
                    : CodegenExpressionRelational.CodegenRelational.LT, Constant(0));
        }
    }
} // end of namespace
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

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public static class MinMaxTypeCodegen
    {
        public static CodegenExpression CodegenMinMax(
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
                return CodegenExpressionBuilder.ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                returnType,
                typeof(MinMaxType.MaxComputerDoubleCoerce),
                codegenClassScope);
            var block = methodNode.Block;

            block.DeclareVar(
                r0Type,
                "r0",
                nodes[0].Forge.EvaluateCodegen(r0Type, methodNode, exprSymbol, codegenClassScope));
            if (!r0Type.IsPrimitive)
            {
                block.IfRefNullReturnNull("r0");
            }

            block.DeclareVar(
                r1Type,
                "r1",
                nodes[1].Forge.EvaluateCodegen(r1Type, methodNode, exprSymbol, codegenClassScope));
            if (!r1Type.IsPrimitive)
            {
                block.IfRefNullReturnNull("r1");
            }

            block.DeclareVarNoInit(returnType, "result");
            block.IfCondition(
                    CodegenCompareRelop(
                        returnType,
                        min ? RelationalOpEnum.LT : RelationalOpEnum.GT,
                        CodegenExpressionBuilder.Ref("r0"),
                        r0Type,
                        CodegenExpressionBuilder.Ref("r1"),
                        r1Type))
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(CodegenExpressionBuilder.Ref("r0"), r0Type, returnType))
                .IfElse()
                .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(CodegenExpressionBuilder.Ref("r1"), r1Type, returnType))
                .BlockEnd();

            for (var i = 2; i < nodes.Length; i++)
            {
                var nodeType = nodes[i].Forge.EvaluationType;
                var refname = "r" + i;
                block.DeclareVar(
                    nodeType,
                    refname,
                    nodes[i].Forge.EvaluateCodegen(nodeType, methodNode, exprSymbol, codegenClassScope));
                if (!nodeType.IsPrimitive)
                {
                    block.IfRefNullReturnNull(refname);
                }

                block.IfCondition(
                        CodegenExpressionBuilder.Not(
                            CodegenCompareRelop(
                                returnType,
                                min ? RelationalOpEnum.LT : RelationalOpEnum.GT,
                                CodegenExpressionBuilder.Ref("result"),
                                returnType,
                                CodegenExpressionBuilder.Ref(refname),
                                r1Type)))
                    .AssignRef("result", TypeHelper.CoerceNumberToBoxedCodegen(CodegenExpressionBuilder.Ref(refname), nodeType, returnType))
                    .BlockEnd();
            }

            block.MethodReturn(CodegenExpressionBuilder.Ref("result"));
            return CodegenExpressionBuilder.LocalMethod(methodNode);
        }

        public static CodegenExpression CodegenCompareRelop(
            Type resultType,
            RelationalOpEnum op,
            CodegenExpressionRef lhs,
            Type lhsType,
            CodegenExpression rhs,
            Type rhsType)
        {
            return CodegenExpressionBuilder.Op(lhs, op.GetExpressionText(), rhs);
        }

        public static CodegenExpression CodegenCompareCompareTo(
            CodegenExpression lhs,
            CodegenExpression rhs,
            bool max)
        {
            return CodegenExpressionBuilder.Relational(
                CodegenExpressionBuilder.ExprDotMethod(lhs, "CompareTo", rhs),
                max
                    ? CodegenExpressionRelational.CodegenRelational.GT
                    : CodegenExpressionRelational.CodegenRelational.LT,
                CodegenExpressionBuilder.Constant(0));
        }
    }
}
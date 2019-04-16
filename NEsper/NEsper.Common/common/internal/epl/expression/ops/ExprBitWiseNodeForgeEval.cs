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
using com.espertech.esper.common.@internal.util;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprBitWiseNodeForgeEval : ExprEvaluator
    {
        private readonly ExprBitWiseNodeForge forge;
        private readonly ExprEvaluator lhs;
        private readonly ExprEvaluator rhs;

        internal ExprBitWiseNodeForgeEval(
            ExprBitWiseNodeForge forge,
            ExprEvaluator lhs,
            ExprEvaluator rhs)
        {
            this.forge = forge;
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var left = lhs.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            var right = rhs.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);

            if (left == null || right == null) {
                return null;
            }

            var result = forge.Computer.Compute(left, right);

            return result;
        }

        public static CodegenExpression Codegen(
            ExprBitWiseNodeForge forge,
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprNode lhs,
            ExprNode rhs)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType, typeof(ExprBitWiseNodeForgeEval), codegenClassScope);

            var leftType = lhs.Forge.EvaluationType;
            var rightType = rhs.Forge.EvaluationType;
            var block = methodNode.Block
                .DeclareVar(
                    leftType, "left", lhs.Forge.EvaluateCodegen(leftType, methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(
                    rightType, "right",
                    rhs.Forge.EvaluateCodegen(rightType, methodNode, exprSymbol, codegenClassScope));
            if (!leftType.IsPrimitive) {
                block.IfRefNullReturnNull("left");
            }

            if (!rhs.Forge.EvaluationType.IsPrimitive) {
                block.IfRefNullReturnNull("right");
            }

            var primitive = forge.EvaluationType.GetPrimitiveType();
            block.DeclareVar(primitive, "l", Ref("left"))
                .DeclareVar(primitive, "r", Ref("right"));

            block.MethodReturn(
                Cast(primitive, Op(Ref("l"), forge.ForgeRenderable.BitWiseOpEnum.ExpressionText, Ref("r"))));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace
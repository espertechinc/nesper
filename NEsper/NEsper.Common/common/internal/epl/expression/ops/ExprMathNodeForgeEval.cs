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
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprMathNodeForgeEval : ExprEvaluator
    {
        private readonly ExprEvaluator evaluatorLeft;
        private readonly ExprEvaluator evaluatorRight;
        private readonly ExprMathNodeForge forge;

        public ExprMathNodeForgeEval(
            ExprMathNodeForge forge,
            ExprEvaluator evaluatorLeft,
            ExprEvaluator evaluatorRight)
        {
            this.forge = forge;
            this.evaluatorLeft = evaluatorLeft;
            this.evaluatorRight = evaluatorRight;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var left = evaluatorLeft.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (left == null) {
                return null;
            }

            var right = evaluatorRight.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (right == null) {
                return null;
            }

            return forge.ArithTypeEnumComputer.Compute(left, right);
        }

        public static CodegenMethod Codegen(
            ExprMathNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprNode lhs,
            ExprNode rhs)
        {
            var methodNode = codegenMethodScope.MakeChild(
                forge.EvaluationType,
                typeof(ExprMathNodeForgeEval),
                codegenClassScope);
            var lhsType = lhs.Forge.EvaluationType.GetBoxedType();
            var rhsType = rhs.Forge.EvaluationType.GetBoxedType();
            var block = methodNode.Block
                .DeclareVar(
                    lhsType,
                    "left",
                    lhs.Forge.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope));
            if (!lhsType.IsPrimitive) {
                block.IfRefNullReturnNull("left");
            }

            block.DeclareVar(
                rhsType,
                "right",
                rhs.Forge.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));
            if (!rhsType.IsPrimitive) {
                block.IfRefNullReturnNull("right");
            }

            block.MethodReturn(
                forge.ArithTypeEnumComputer.Codegen(
                    methodNode,
                    codegenClassScope,
                    Ref("left"),
                    Ref("right"),
                    lhsType,
                    rhsType));
            return methodNode;
        }
    }
} // end of namespace
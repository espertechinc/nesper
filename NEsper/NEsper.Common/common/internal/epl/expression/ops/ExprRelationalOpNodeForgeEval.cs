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

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRelationalOpNodeForgeEval : ExprEvaluator
    {
        private readonly ExprRelationalOpNodeForge forge;
        private readonly ExprEvaluator left;
        private readonly ExprEvaluator right;

        public ExprRelationalOpNodeForgeEval(
            ExprRelationalOpNodeForge forge,
            ExprEvaluator left,
            ExprEvaluator right)
        {
            this.forge = forge;
            this.left = left;
            this.right = right;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var lvalue = left.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (lvalue == null) {
                return null;
            }

            var rvalue = right.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (rvalue == null) {
                return null;
            }

            return forge.Computer.Compare(lvalue, rvalue);
        }

        public static CodegenExpression Codegen(
            ExprRelationalOpNodeForge forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var lhs = forge.ForgeRenderable.ChildNodes[0].Forge;
            var rhs = forge.ForgeRenderable.ChildNodes[1].Forge;
            var lhsType = lhs.EvaluationType;
            if (lhsType == null) {
                return ConstantNull();
            }

            var rhsType = rhs.EvaluationType;
            if (rhsType == null) {
                return ConstantNull();
            }

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprRelationalOpNodeForgeEval),
                codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(lhsType, "left", lhs.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope));
            if (!lhsType.IsPrimitive) {
                block.IfRefNullReturnNull("left");
            }

            block.DeclareVar(rhsType, "right", rhs.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));
            if (!rhsType.IsPrimitive) {
                block.IfRefNullReturnNull("right");
            }

            block.MethodReturn(forge.Computer.Codegen(Ref("left"), lhsType, Ref("right"), rhsType));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace
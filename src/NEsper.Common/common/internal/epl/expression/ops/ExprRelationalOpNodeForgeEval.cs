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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprRelationalOpNodeForgeEval : ExprEvaluator
    {
        private readonly ExprRelationalOpNodeForge _forge;
        private readonly ExprEvaluator _left;
        private readonly ExprEvaluator _right;

        public ExprRelationalOpNodeForgeEval(
            ExprRelationalOpNodeForge forge,
            ExprEvaluator left,
            ExprEvaluator right)
        {
            this._forge = forge;
            this._left = left;
            this._right = right;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var lvalue = _left.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (lvalue == null) {
                return null;
            }

            var rvalue = _right.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext);
            if (rvalue == null) {
                return null;
            }

            return _forge.Computer.Compare(lvalue, rvalue);
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

            var chsType = forge.CoercionType;
            
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprRelationalOpNodeForgeEval),
                codegenClassScope);

            CodegenExpression lhsRef = Ref("left");
            CodegenExpression rhsRef = Ref("right");
            
            var block = methodNode.Block;

            block.DeclareVar(chsType, "left", lhs.EvaluateCodegen(chsType, methodNode, exprSymbol, codegenClassScope));
            if (chsType.CanBeNull()) {
                block.IfRefNullReturnNull("left");
                if (chsType.IsNullable()) {
                    lhsRef = Unbox(lhsRef);
                    lhsType = chsType.GetUnboxedType();
                }
            }

            block.DeclareVar(chsType, "right", rhs.EvaluateCodegen(chsType, methodNode, exprSymbol, codegenClassScope));
            if (chsType.CanBeNull()) {
                block.IfRefNullReturnNull("right");
                if (chsType.IsNullable()) {
                    rhsRef = Unbox(rhsRef);
                    rhsType = chsType.GetUnboxedType();
                }
            }

            block.MethodReturn(forge.Computer.Codegen(lhsRef, lhsType, rhsRef, rhsType));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace
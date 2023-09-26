///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeCoercionEval : ExprEvaluator
    {
        private readonly ExprEqualsNodeImpl _parent;
        private readonly ExprEvaluator _lhs;
        private readonly ExprEvaluator _rhs;
        private readonly Coercer _coercerLhs;
        private readonly Coercer _coercerRhs;

        public ExprEqualsNodeForgeCoercionEval(
            ExprEqualsNodeImpl parent,
            ExprEvaluator lhs,
            ExprEvaluator rhs,
            Coercer coercerLhs,
            Coercer coercerRhs)
        {
            _parent = parent;
            _lhs = lhs;
            _rhs = rhs;
            _coercerLhs = coercerLhs;
            _coercerRhs = coercerRhs;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            return EvaluateInternal(eventsPerStream, isNewData, context);
        }

        private bool? EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var leftResult = _lhs.Evaluate(eventsPerStream, isNewData, context);
            var rightResult = _rhs.Evaluate(eventsPerStream, isNewData, context);

            if (!_parent.IsIs) {
                if (leftResult == null || rightResult == null) {
                    // null comparison
                    return null;
                }
            }
            else {
                if (leftResult == null) {
                    return rightResult == null;
                }

                if (rightResult == null) {
                    return false;
                }
            }

            var left = _coercerLhs.CoerceBoxed(leftResult);
            var right = _coercerRhs.CoerceBoxed(rightResult);
            return left.Equals(right) ^ _parent.IsNotEquals;
        }

        public static CodegenMethod Codegen(
            ExprEqualsNodeForgeCoercion forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprNode lhs,
            ExprNode rhs)
        {
            var lhsType = lhs.Forge.EvaluationType;
            var rhsType = rhs.Forge.EvaluationType;

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprEqualsNodeForgeNCForgeEquals),
                codegenClassScope);
            var block = methodNode.Block;

            block
                .DeclareVar(lhsType, "l", lhs.Forge.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(
                    rhsType,
                    "r",
                    rhs.Forge.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));

            if (!forge.ForgeRenderable.IsIs) {
                if (lhsType.CanBeNull()) {
                    block.IfRefNullReturnNull("l");
                }

                if (rhsType.CanBeNull()) {
                    block.IfRefNullReturnNull("r");
                }
            }
            else {
                if (lhsType.CanBeNull() && rhsType.CanBeNull()) {
                    block.IfRefNull("l").BlockReturn(EqualsNull(Ref("r")));
                }

                if (rhsType.CanBeNull()) {
                    block.IfRefNull("r").BlockReturn(ConstantFalse());
                }
            }

            block.DeclareVar(
                forge.CoercerLHS.ReturnType,
                "left",
                forge.CoercerLHS.CoerceCodegen(Ref("l"), lhsType));
            block.DeclareVar(
                forge.CoercerRHS.ReturnType,
                "right",
                forge.CoercerRHS.CoerceCodegen(Ref("r"), rhsType));

            //var compare = StaticMethod(typeof(DebugExtensions), "DebugEquals", Ref("left"), Ref("right"));

            var compare = StaticMethod(typeof(object), "Equals", Ref("left"), Ref("right"));
            if (!forge.ForgeRenderable.IsNotEquals) {
                block.MethodReturn(compare);
            }
            else {
                block.MethodReturn(Not(compare));
            }

            return methodNode;
        }
    }
} // end of namespace
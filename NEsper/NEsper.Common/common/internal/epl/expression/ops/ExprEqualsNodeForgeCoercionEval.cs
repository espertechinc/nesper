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
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeCoercionEval : ExprEvaluator
    {
        private readonly ExprEvaluator lhs;
        private readonly SimpleNumberCoercer numberCoercerLHS;
        private readonly SimpleNumberCoercer numberCoercerRHS;
        private readonly ExprEqualsNodeImpl parent;
        private readonly ExprEvaluator rhs;

        public ExprEqualsNodeForgeCoercionEval(
            ExprEqualsNodeImpl parent,
            ExprEvaluator lhs,
            ExprEvaluator rhs,
            SimpleNumberCoercer numberCoercerLHS,
            SimpleNumberCoercer numberCoercerRHS)
        {
            this.parent = parent;
            this.lhs = lhs;
            this.rhs = rhs;
            this.numberCoercerLHS = numberCoercerLHS;
            this.numberCoercerRHS = numberCoercerRHS;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var result = EvaluateInternal(eventsPerStream, isNewData, context);
            return result;
        }

        private bool? EvaluateInternal(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var leftResult = lhs.Evaluate(eventsPerStream, isNewData, context);
            var rightResult = rhs.Evaluate(eventsPerStream, isNewData, context);

            if (!parent.IsIs) {
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

            var left = numberCoercerLHS.CoerceBoxed(leftResult);
            var right = numberCoercerRHS.CoerceBoxed(rightResult);
            return left.Equals(right) ^ parent.IsNotEquals;
        }

        public static CodegenMethod Codegen(
            ExprEqualsNodeForgeCoercion forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprNode lhs,
            ExprNode rhs)
        {
            var lhsType = lhs.Forge.EvaluationType.GetBoxedType();
            var rhsType = rhs.Forge.EvaluationType.GetBoxedType();

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?),
                typeof(ExprEqualsNodeForgeNCEvalEquals),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(lhsType, "l", lhs.Forge.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(rhsType, "r", rhs.Forge.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));

            if (!forge.ForgeRenderable.IsIs) {
                if (!lhsType.IsPrimitive) {
                    block.IfRefNullReturnNull("l");
                }

                if (!rhsType.IsPrimitive) {
                    block.IfRefNullReturnNull("r");
                }
            }
            else {
                if (!lhsType.IsPrimitive && !rhsType.IsPrimitive) {
                    block.IfRefNull("l").BlockReturn(EqualsNull(Ref("r")));
                }

                if (!rhsType.IsPrimitive) {
                    block.IfRefNull("r").BlockReturn(ConstantFalse());
                }
            }

            block.DeclareVar<object>(
                "left",
                forge.NumberCoercerLHS.CoerceCodegen(Ref("l"), lhs.Forge.EvaluationType));
            block.DeclareVar<object>(
                "right",
                forge.NumberCoercerRHS.CoerceCodegen(Ref("r"), rhs.Forge.EvaluationType));
            var compare = ExprDotMethod(Ref("left"), "Equals", Ref("right"));
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
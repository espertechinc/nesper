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
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.expression.codegen.CodegenLegoCompareEquals;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCEvalEquals : ExprEvaluator
    {
        private readonly ExprEvaluator lhs;
        private readonly ExprEqualsNodeImpl parent;
        private readonly ExprEvaluator rhs;

        internal ExprEqualsNodeForgeNCEvalEquals(
            ExprEqualsNodeImpl parent,
            ExprEvaluator lhs,
            ExprEvaluator rhs)
        {
            this.parent = parent;
            this.lhs = lhs;
            this.rhs = rhs;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var left = lhs.Evaluate(eventsPerStream, isNewData, context);
            var right = rhs.Evaluate(eventsPerStream, isNewData, context);

            if (left == null || right == null) { // null comparison
                return null;
            }

            var result = left.Equals(right) ^ parent.IsNotEquals;
            return result;
        }

        public static CodegenMethod Codegen(
            ExprEqualsNodeForgeNC forge,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope,
            ExprForge lhs,
            ExprForge rhs)
        {
            var lhsType = lhs.EvaluationType;
            var rhsType = rhs.EvaluationType;

            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool?), typeof(ExprEqualsNodeForgeNCEvalEquals), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(lhsType, "left", lhs.EvaluateCodegen(lhsType, methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(rhsType, "right", rhs.EvaluateCodegen(rhsType, methodNode, exprSymbol, codegenClassScope));

            if (!lhsType.IsPrimitive) {
                block.IfRefNullReturnNull("left");
            }

            if (!rhsType.IsPrimitive) {
                block.IfRefNullReturnNull("right");
            }

            var compare = CodegenEqualsNonNullNoCoerce(Ref("left"), lhsType, Ref("right"), rhsType);
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
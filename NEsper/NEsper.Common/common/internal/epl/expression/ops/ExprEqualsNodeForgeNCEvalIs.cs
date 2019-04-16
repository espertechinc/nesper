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

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprEqualsNodeForgeNCEvalIs : ExprEvaluator
    {
        private readonly ExprEvaluator lhs;
        private readonly ExprEqualsNodeImpl parent;
        private readonly ExprEvaluator rhs;

        public ExprEqualsNodeForgeNCEvalIs(
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

            bool result;
            if (left == null) {
                result = right == null;
            }
            else {
                result = right != null && left.Equals(right);
            }

            result = result ^ parent.IsNotEquals;

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
            var methodNode = codegenMethodScope.MakeChild(
                typeof(bool), typeof(ExprEqualsNodeForgeNCEvalIs), codegenClassScope);
            var block = methodNode.Block
                .DeclareVar(
                    typeof(object), "left",
                    lhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                .DeclareVar(
                    typeof(object), "right",
                    rhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope));
            block.DeclareVarNoInit(typeof(bool), "result")
                .IfRefNull("left")
                .AssignRef("result", EqualsNull(Ref("right")))
                .IfElse()
                .AssignRef(
                    "result", And(NotEqualsNull(Ref("right")), ExprDotMethod(Ref("left"), "equals", Ref("right"))))
                .BlockEnd();
            if (!forge.ForgeRenderable.IsNotEquals) {
                block.MethodReturn(Ref("result"));
            }
            else {
                block.MethodReturn(Not(Ref("result")));
            }

            return methodNode;
        }
    }
} // end of namespace
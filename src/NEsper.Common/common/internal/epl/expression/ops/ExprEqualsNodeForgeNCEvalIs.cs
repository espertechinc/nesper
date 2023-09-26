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
        private readonly ExprEvaluator _lhs;
        private readonly ExprEqualsNodeImpl _parent;
        private readonly ExprEvaluator _rhs;

        public ExprEqualsNodeForgeNCEvalIs(
            ExprEqualsNodeImpl parent,
            ExprEvaluator lhs,
            ExprEvaluator rhs)
        {
            _parent = parent;
            _lhs = lhs;
            _rhs = rhs;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var left = _lhs.Evaluate(eventsPerStream, isNewData, context);
            var right = _rhs.Evaluate(eventsPerStream, isNewData, context);

            bool result;
            if (left == null) {
                result = right == null;
            }
            else {
                result = right != null && left.Equals(right);
            }

            result = result ^ _parent.IsNotEquals;

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
                typeof(bool),
                typeof(ExprEqualsNodeForgeNCEvalIs),
                codegenClassScope);
            var block = methodNode.Block
                .DeclareVar<object>(
                    "left",
                    lhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                .DeclareVar<object>(
                    "right",
                    rhs.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope));
            block.DeclareVarNoInit(typeof(bool), "result")
                .IfRefNull("left")
                .AssignRef("result", EqualsNull(Ref("right")))
                .IfElse()
                .AssignRef(
                    "result",
                    And(NotEqualsNull(Ref("right")), StaticMethod<object>("Equals", Ref("left"), Ref("right"))))
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
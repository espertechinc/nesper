///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Text;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    public class ExprConcatNodeForgeEvalWNew : ExprEvaluator
    {
        private readonly ExprEvaluator[] evaluators;
        private readonly ExprConcatNodeForge forge;

        internal ExprConcatNodeForgeEvalWNew(ExprConcatNodeForge forge, ExprEvaluator[] evaluators)
        {
            this.forge = forge;
            this.evaluators = evaluators;
        }

        public object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var buffer = new StringBuilder();
            return Evaluate(eventsPerStream, isNewData, context, buffer, evaluators, forge);
        }

        protected internal static string Evaluate(
            EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context, StringBuilder buffer,
            ExprEvaluator[] evaluators, ExprConcatNodeForge form)
        {
            foreach (var child in evaluators) {
                var result = (string) child.Evaluate(eventsPerStream, isNewData, context);
                if (result == null) {
                    return null;
                }

                buffer.Append(result);
            }

            return buffer.ToString();
        }

        public static CodegenExpression Codegen(
            ExprConcatNodeForge forge, CodegenMethodScope codegenMethodScope, ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(string), typeof(ExprConcatNodeForgeEvalWNew), codegenClassScope);

            var block = methodNode.Block
                .DeclareVar(typeof(StringBuilder), "buf", NewInstance(typeof(StringBuilder)))
                .DeclareVarNoInit(typeof(string), "value");
            var chain = ExprDotMethodChain(Ref("buf"));
            foreach (ExprNode expr in forge.ForgeRenderable.ChildNodes) {
                block.AssignRef(
                        "value", expr.Forge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("value")
                    .ExprDotMethod(Ref("buf"), "append", Ref("value"));
            }

            block.MethodReturn(ExprDotMethod(chain, "toString"));
            return LocalMethod(methodNode);
        }
    }
} // end of namespace
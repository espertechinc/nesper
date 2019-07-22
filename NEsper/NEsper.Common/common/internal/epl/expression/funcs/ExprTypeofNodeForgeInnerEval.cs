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
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.funcs
{
    public class ExprTypeofNodeForgeInnerEval : ExprTypeofNodeForge
    {
        private readonly ExprTypeofNode parent;

        public ExprTypeofNodeForgeInnerEval(ExprTypeofNode parent)
        {
            this.parent = parent;
        }

        public override ExprEvaluator ExprEvaluator => new InnerEvaluator(parent.ChildNodes[0].Forge.ExprEvaluator);

        public override ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNodeRenderable ExprForgeRenderable => parent;

        public override CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprTypeof",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public override CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                typeof(string),
                typeof(ExprTypeofNodeForgeInnerEval),
                codegenClassScope);
            methodNode.Block
                .DeclareVar<object>(
                    "result",
                    parent.ChildNodes[0].Forge.EvaluateCodegen(requiredType, methodNode, exprSymbol, codegenClassScope))
                .IfRefNullReturnNull("result")
                .MethodReturn(ExprDotMethodChain(Ref("result")).Add("getClass").Add("getSimpleName"));
            return LocalMethod(methodNode);
        }

        private class InnerEvaluator : ExprEvaluator
        {
            private readonly ExprEvaluator evaluator;

            internal InnerEvaluator(ExprEvaluator evaluator)
            {
                this.evaluator = evaluator;
            }

            public virtual object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var result = evaluator.Evaluate(eventsPerStream, isNewData, context);
                if (result == null) {
                    return null;
                }

                return result.GetType().GetSimpleName();
            }
        }
    }
} // end of namespace
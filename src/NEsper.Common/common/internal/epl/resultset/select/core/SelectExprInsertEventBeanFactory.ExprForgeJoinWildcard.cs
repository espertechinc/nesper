///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class ExprForgeJoinWildcard : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly int streamNum;

            public ExprForgeJoinWildcard(
                int streamNum,
                Type returnType)
            {
                this.streamNum = streamNum;
                EvaluationType = returnType;
            }

            public object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var bean = eventsPerStream[streamNum];

                return bean?.Underlying;
            }

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public ExprEvaluator ExprEvaluator => this;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    EvaluationType,
                    typeof(ExprForgeJoinWildcard),
                    codegenClassScope);
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                methodNode.Block
                    .DeclareVar<EventBean>(
                        "bean",
                        CodegenExpressionBuilder.ArrayAtIndex(refEPS, CodegenExpressionBuilder.Constant(streamNum)))
                    .IfRefNullReturnNull("bean")
                    .MethodReturn(
                        CodegenExpressionBuilder.Cast(
                            EvaluationType,
                            CodegenExpressionBuilder.ExprDotUnderlying(CodegenExpressionBuilder.Ref("bean"))));
                return CodegenExpressionBuilder.LocalMethod(methodNode);
            }

            public Type EvaluationType { get; }

            public ExprNodeRenderable ExprForgeRenderable => this;

            public void ToEPL(
                TextWriter writer,
                ExprPrecedenceEnum parentPrecedence,
                ExprNodeRenderableFlags flags)
            {
                writer.Write(GetType().GetSimpleName());
            }
        }
    }
}
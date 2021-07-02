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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class ExprForgeStreamUnderlying : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly Type _returnType;

            private readonly int _streamNumEval;

            public ExprForgeStreamUnderlying(
                int streamNumEval,
                Type returnType)
            {
                this._streamNumEval = streamNumEval;
                this._returnType = returnType;
            }

            public object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var theEvent = eventsPerStream[_streamNumEval];
                return theEvent?.Underlying;
            }

            public ExprEvaluator ExprEvaluator => this;

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(_returnType, GetType(), codegenClassScope);

                var refEPS = exprSymbol.GetAddEPS(methodNode);
                methodNode.Block
                    .DeclareVar<EventBean>(
                        "theEvent",
                        CodegenExpressionBuilder.ArrayAtIndex(refEPS, CodegenExpressionBuilder.Constant(_streamNumEval)))
                    .IfRefNullReturnNull("theEvent")
                    .MethodReturn(
                        CodegenExpressionBuilder.Cast(
                            _returnType,
                            CodegenExpressionBuilder.ExprDotUnderlying(CodegenExpressionBuilder.Ref("theEvent"))));
                return CodegenExpressionBuilder.LocalMethod(methodNode);
            }

            public Type EvaluationType => typeof(object);

            public ExprNodeRenderable ExprForgeRenderable => this;

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public void ToEPL(TextWriter writer,
                ExprPrecedenceEnum parentPrecedence,
                ExprNodeRenderableFlags flags)
            {
                writer.Write(typeof(ExprForgeStreamUnderlying).GetSimpleName());
            }
        }
    }
}
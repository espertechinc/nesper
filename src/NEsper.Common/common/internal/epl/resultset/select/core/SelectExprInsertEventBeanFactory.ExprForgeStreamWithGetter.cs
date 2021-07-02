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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class ExprForgeStreamWithGetter : ExprForge,
            ExprEvaluator,
            ExprNodeRenderable
        {
            private readonly EventPropertyGetterSPI _getter;

            public ExprForgeStreamWithGetter(EventPropertyGetterSPI getter)
            {
                this._getter = getter;
            }

            public object Evaluate(
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext exprEvaluatorContext)
            {
                var theEvent = eventsPerStream[0];
                if (theEvent != null) {
                    return _getter.Get(theEvent);
                }

                return null;
            }

            public CodegenExpression EvaluateCodegen(
                Type requiredType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                    typeof(object),
                    typeof(ExprForgeStreamWithGetter),
                    codegenClassScope);
                var refEPS = exprSymbol.GetAddEPS(methodNode);
                methodNode.Block
                    .DeclareVar<EventBean>(
                        "theEvent",
                        CodegenExpressionBuilder.ArrayAtIndex(refEPS, CodegenExpressionBuilder.Constant(0)))
                    .IfRefNotNull("theEvent")
                    .BlockReturn(
                        _getter.EventBeanGetCodegen(
                            CodegenExpressionBuilder.Ref("theEvent"),
                            methodNode,
                            codegenClassScope))
                    .MethodReturn(CodegenExpressionBuilder.ConstantNull());
                return CodegenExpressionBuilder.LocalMethod(methodNode);
            }

            public ExprEvaluator ExprEvaluator => this;

            public Type EvaluationType => typeof(object);

            public ExprNodeRenderable ExprForgeRenderable => this;

            public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

            public void ToEPL(TextWriter writer,
                ExprPrecedenceEnum parentPrecedence,
                ExprNodeRenderableFlags flags)
            {
                writer.Write(typeof(ExprForgeStreamWithGetter).GetSimpleName());
            }
        }
    }
}
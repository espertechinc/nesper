///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.json.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class SelectExprInsertNativeExpressionCoerceJson : SelectExprInsertNativeExpressionCoerceBase
        {
            public SelectExprInsertNativeExpressionCoerceJson(
                EventType eventType,
                ExprForge exprForge)
                : base(eventType, exprForge)
            {
            }

            public override CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                methodNode
                    .Block
                    .DeclareVar<string>(
                        "result",
                        exprForge.EvaluateCodegen(typeof(string), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("result")
                    .DeclareVar<object>(
                        "und",
                        ExprDotMethod(Cast(typeof(JsonEventType), resultEventType), "Parse", Ref("result")))
                    .MethodReturn(ExprDotMethod(eventBeanFactory, "AdapterForTypedJson", Ref("und"), resultEventType));
                return methodNode;
            }
        }
    }
}
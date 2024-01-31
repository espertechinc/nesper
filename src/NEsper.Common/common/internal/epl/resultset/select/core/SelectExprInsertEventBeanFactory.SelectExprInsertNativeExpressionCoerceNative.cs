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

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class SelectExprInsertNativeExpressionCoerceNative : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceNative(
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
                var methodNode = codegenMethodScope.MakeChild(typeof(EventBean), GetType(), codegenClassScope);
                methodNode.Block
                    .DeclareVar<object>(
                        "result",
                        exprForge.EvaluateCodegen(typeof(object), methodNode, exprSymbol, codegenClassScope))
                    .IfRefNullReturnNull("result")
                    .MethodReturn(
                        CodegenExpressionBuilder.ExprDotMethod(
                            eventBeanFactory,
                            "AdapterForTypedObject",
                            CodegenExpressionBuilder.Ref("result"),
                            resultEventType));
                return methodNode;
            }
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
        public class SelectExprInsertNativeExpressionCoerceMap : SelectExprInsertNativeExpressionCoerceBase
        {
            internal SelectExprInsertNativeExpressionCoerceMap(
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
                var expr = exprForge.EvaluateCodegen(
                    typeof(IDictionary<string, object>),
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                if (!TypeHelper.IsSubclassOrImplementsInterface(
                    exprForge.EvaluationType,
                    typeof(IDictionary<string, object>))) {
                    expr = CodegenExpressionBuilder.Cast(typeof(IDictionary<string, object>), expr);
                }

                methodNode.Block.DeclareVar<IDictionary<string, object>>("result", expr)
                    .IfRefNullReturnNull("result")
                    .MethodReturn(
                        CodegenExpressionBuilder.ExprDotMethod(
                            eventBeanFactory,
                            "AdapterForTypedMap",
                            CodegenExpressionBuilder.Ref("result"),
                            resultEventType));
                return methodNode;
            }
        }
    }
}
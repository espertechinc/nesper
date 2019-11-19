///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.epl.resultset.@select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class SelectExprInsertNativeNoWiden : SelectExprInsertNativeBase
        {
            public SelectExprInsertNativeNoWiden(
                EventType eventType,
                EventBeanManufacturerForge eventManufacturer,
                ExprForge[] exprForges)
                : base(eventType, eventManufacturer, exprForges)
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
                var manufacturer = codegenClassScope.AddDefaultFieldUnshared(
                    true,
                    typeof(EventBeanManufacturer),
                    eventManufacturer.Make(methodNode.Block, codegenMethodScope, codegenClassScope));
                var block = methodNode.Block
                    .DeclareVar<object[]>(
                        "values",
                        CodegenExpressionBuilder.NewArrayByLength(
                            typeof(object),
                            CodegenExpressionBuilder.Constant(exprForges.Length)));
                for (var i = 0; i < exprForges.Length; i++) {
                    var expression = CodegenLegoMayVoid.ExpressionMayVoid(
                        typeof(object),
                        exprForges[i],
                        methodNode,
                        exprSymbol,
                        codegenClassScope);
                    block.AssignArrayElement("values", CodegenExpressionBuilder.Constant(i), expression);
                }

                block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        manufacturer,
                        "Make",
                        CodegenExpressionBuilder.Ref("values")));
                return methodNode;
            }
        }
    }
}
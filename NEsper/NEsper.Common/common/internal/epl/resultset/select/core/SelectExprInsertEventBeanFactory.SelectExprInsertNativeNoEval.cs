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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public partial class SelectExprInsertEventBeanFactory
    {
        public class SelectExprInsertNativeNoEval : SelectExprProcessorForge
        {
            private readonly EventBeanManufacturerForge eventManufacturer;

            public SelectExprInsertNativeNoEval(
                EventType eventType,
                EventBeanManufacturerForge eventManufacturer)
            {
                ResultEventType = eventType;
                this.eventManufacturer = eventManufacturer;
            }

            public EventType ResultEventType { get; }

            public CodegenMethod ProcessCodegen(
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
                methodNode.Block.MethodReturn(
                    CodegenExpressionBuilder.ExprDotMethod(
                        manufacturer,
                        "Make",
                        CodegenExpressionBuilder.PublicConstValue(typeof(CollectionUtil), "OBJECTARRAY_EMPTY")));
                return methodNode;
            }
        }
    }
}
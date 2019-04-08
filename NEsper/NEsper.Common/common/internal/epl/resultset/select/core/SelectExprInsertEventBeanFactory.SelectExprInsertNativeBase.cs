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
        public abstract class SelectExprInsertNativeBase : SelectExprProcessorForge
        {
            internal readonly EventBeanManufacturerForge eventManufacturer;
            internal readonly ExprForge[] exprForges;

            internal SelectExprInsertNativeBase(
                EventType eventType, EventBeanManufacturerForge eventManufacturer, ExprForge[] exprForges)
            {
                ResultEventType = eventType;
                this.eventManufacturer = eventManufacturer;
                this.exprForges = exprForges;
            }

            public EventType ResultEventType { get; }

            public abstract CodegenMethod ProcessCodegen(
                CodegenExpression resultEventType,
                CodegenExpression eventBeanFactory,
                CodegenMethodScope codegenMethodScope,
                SelectExprProcessorCodegenSymbol selectSymbol,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }
    }
}
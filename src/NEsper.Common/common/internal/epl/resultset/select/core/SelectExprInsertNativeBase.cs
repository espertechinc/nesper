///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.resultset.select.core
{
    public abstract class SelectExprInsertNativeBase : SelectExprProcessorForge
    {
        private readonly EventType eventType;
        protected readonly EventBeanManufacturerForge eventManufacturer;
        protected readonly ExprForge[] exprForges;

        public abstract CodegenMethod ProcessCodegen(
            CodegenExpression resultEventType,
            CodegenExpression eventBeanFactory,
            CodegenMethodScope codegenMethodScope,
            SelectExprProcessorCodegenSymbol selectSymbol,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope);

        protected SelectExprInsertNativeBase(
            EventType eventType,
            EventBeanManufacturerForge eventManufacturer,
            ExprForge[] exprForges)
        {
            this.eventType = eventType;
            this.eventManufacturer = eventManufacturer;
            this.exprForges = exprForges;
        }

        public static SelectExprInsertNativeBase MakeInsertNative(
            EventType eventType,
            EventBeanManufacturerForge eventManufacturer,
            ExprForge[] exprForges,
            TypeWidenerSPI[] wideners)
        {
            var hasWidener = false;
            foreach (var widener in wideners) {
                if (widener != null) {
                    hasWidener = true;
                    break;
                }
            }

            if (!hasWidener) {
                return new SelectExprInsertNativeNoWiden(eventType, eventManufacturer, exprForges);
            }

            return new SelectExprInsertNativeWidening(eventType, eventManufacturer, exprForges, wideners);
        }

        public EventType ResultEventType => eventType;
    }
} // end of namespace
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
        public abstract class SelectExprInsertNativeExpressionCoerceBase : SelectExprProcessorForge
        {
            internal readonly EventType eventType;
            internal readonly ExprForge exprForge;

            protected SelectExprInsertNativeExpressionCoerceBase(
                EventType eventType,
                ExprForge exprForge)
            {
                this.eventType = eventType;
                this.exprForge = exprForge;
            }

            public EventType ResultEventType => eventType;

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
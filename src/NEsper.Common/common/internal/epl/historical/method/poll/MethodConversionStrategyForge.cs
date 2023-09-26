///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.historical.method.poll
{
    public class MethodConversionStrategyForge
    {
        private readonly EventType eventType;
        private readonly Type implementation;

        public MethodConversionStrategyForge(
            EventType eventType,
            Type implementation)
        {
            this.eventType = eventType;
            this.implementation = implementation;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(implementation, GetType(), classScope);
            method.Block
                .DeclareVarNewInstance(implementation, "conv")
                .SetProperty(
                    Ref("conv"),
                    "EventType",
                    EventTypeUtility.ResolveTypeCodegen(eventType, symbols.GetAddInitSvc(method)))
                .MethodReturn(Ref("conv"));
            return LocalMethod(method);
        }
    }
} // end of namespace
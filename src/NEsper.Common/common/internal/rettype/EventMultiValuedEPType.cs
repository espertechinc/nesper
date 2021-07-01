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
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.rettype
{
    /// <summary>
    /// Clazz can be either - Collection - Array i.e. "EventType[].class"
    /// </summary>
    public class EventMultiValuedEPType : EPType
    {
        public EventMultiValuedEPType(
            Type container,
            EventType component)
        {
            Container = container;
            Component = component;
        }

        public Type Container { get; private set; }

        public EventType Component { get; private set; }

        public CodegenExpression Codegen(
            CodegenMethod method,
            CodegenClassScope classScope,
            CodegenExpression typeInitSvcRef)
        {
            return CodegenExpressionBuilder.NewInstance<EventMultiValuedEPType>(
                CodegenExpressionBuilder.Constant(Container),
                EventTypeUtility.ResolveTypeCodegen(Component, typeInitSvcRef));
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedValidationItem // : Supplier<EventType>
    {
        public ContextControllerKeyedValidationItem(
            EventType eventType,
            string[] propertyNames)
        {
            EventType = eventType;
            PropertyNames = propertyNames;
        }

        public EventType EventType { get; }

        public string[] PropertyNames { get; }

        public EventType Get()
        {
            return EventType;
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<ContextControllerKeyedValidationItem>(
                EventTypeUtility.ResolveTypeCodegen(EventType, addInitSvc),
                Constant(PropertyNames));
        }
    }
} // end of namespace
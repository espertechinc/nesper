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

namespace com.espertech.esper.common.@internal.context.controller.hash
{
    public class ContextControllerHashValidationItem
    {
        public ContextControllerHashValidationItem(EventType eventType)
        {
            EventType = eventType;
        }

        public EventType EventType { get; }

        public EventType Get()
        {
            return EventType;
        }

        public CodegenExpression Make(CodegenExpressionRef addInitSvc)
        {
            return NewInstance<ContextControllerHashValidationItem>(
                EventTypeUtility.ResolveTypeCodegen(EventType, addInitSvc));
        }
    }
} // end of namespace
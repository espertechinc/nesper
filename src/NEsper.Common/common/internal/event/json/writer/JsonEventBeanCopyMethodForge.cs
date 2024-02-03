///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder; // cast

// newInstance

namespace com.espertech.esper.common.@internal.@event.json.writer
{
    /// <summary>
    ///     Copy method for Json-underlying events.
    /// </summary>
    public class JsonEventBeanCopyMethodForge : EventBeanCopyMethodForge
    {
        private readonly JsonEventType _eventType;

        public JsonEventBeanCopyMethodForge(JsonEventType eventType)
        {
            _eventType = eventType;
        }

        public CodegenExpression MakeCopyMethodClassScoped(CodegenClassScope classScope)
        {
            var factory = classScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            return NewInstance(
                typeof(JsonEventBeanCopyMethod),
                Cast(
                    typeof(JsonEventType),
                    EventTypeUtility.ResolveTypeCodegen(_eventType, EPStatementInitServicesConstants.REF)),
                factory);
        }

        public EventBeanCopyMethod GetCopyMethod(EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new JsonEventBeanCopyMethod(_eventType, eventBeanTypedEventFactory);
        }
    }
} // end of namespace
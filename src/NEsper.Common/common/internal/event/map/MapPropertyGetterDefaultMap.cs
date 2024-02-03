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
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.@event.core;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.@event.map
{
    /// <summary>
    ///     Getter for map entry.
    /// </summary>
    public class MapPropertyGetterDefaultMap : MapPropertyGetterDefaultBase
    {
        public MapPropertyGetterDefaultMap(
            string propertyName,
            EventType fragmentEventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
            :
            base(propertyName, fragmentEventType, eventBeanTypedEventFactory)
        {
        }

        internal override object HandleCreateFragment(object value)
        {
            return BaseNestableEventUtil.HandleBNCreateFragmentMap(
                value,
                fragmentEventType,
                eventBeanTypedEventFactory);
        }

        internal override CodegenExpression HandleCreateFragmentCodegen(
            CodegenExpression value,
            CodegenClassScope codegenClassScope)
        {
            var factory = codegenClassScope.AddOrGetDefaultFieldSharable(EventBeanTypedEventFactoryCodegenField.INSTANCE);
            var eventType = codegenClassScope.AddDefaultFieldUnshared<EventType>(
                true, EventTypeUtility.ResolveTypeCodegen(fragmentEventType, EPStatementInitServicesConstants.REF));
            return StaticMethod(typeof(BaseNestableEventUtil), "HandleBNCreateFragmentMap", value, eventType, factory);
        }
    }
} // end of namespace
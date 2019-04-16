///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.bean.service;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class WrapperEventTypeUtil
    {
        /// <summary>
        ///     Make a wrapper type.
        ///     A wrapper event type may indeed wrap another wrapper event type. This is so that a wrapper event bean can wrap
        ///     another wrapper event bean.
        ///     If there were unwrapped the nesting information such as what is the nested wrapper type and how they are nested
        ///     would be lost.
        /// </summary>
        /// <param name="metadata">metadata</param>
        /// <param name="underlyingEventType">underlying event type</param>
        /// <param name="propertyTypesMayPrimitive">property types</param>
        /// <param name="eventBeanTypedEventFactory">factory for instances</param>
        /// <param name="beanEventTypeFactory">bean event type factory</param>
        /// <param name="eventTypeNameResolver">type name resolver</param>
        /// <returns>wrapper type</returns>
        public static WrapperEventType MakeWrapper(
            EventTypeMetadata metadata,
            EventType underlyingEventType,
            IDictionary<string, object> propertyTypesMayPrimitive,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            IDictionary<string, object> verified = BaseNestableEventUtil.ResolvePropertyTypes(
                propertyTypesMayPrimitive, eventTypeNameResolver);
            return new WrapperEventType(
                metadata, underlyingEventType, verified, eventBeanTypedEventFactory, beanEventTypeFactory);
        }
    }
} // end of namespace
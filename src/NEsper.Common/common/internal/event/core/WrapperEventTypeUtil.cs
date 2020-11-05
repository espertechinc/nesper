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
using com.espertech.esper.compat.collections;

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
            // If we are wrapping an underlying type that is itself a wrapper, then this is a special case
            if (underlyingEventType is WrapperEventType underlyingWrapperType) {
                // the underlying type becomes the type already wrapped
                // properties are a superset of the wrapped properties and the additional properties
                underlyingEventType = underlyingWrapperType.UnderlyingEventType;
                var propertiesSuperset = new Dictionary<string, object>();
                propertiesSuperset.PutAll(underlyingWrapperType.UnderlyingMapType.Types);
                propertiesSuperset.PutAll(propertyTypesMayPrimitive);
                propertyTypesMayPrimitive = propertiesSuperset;
            }
            
            IDictionary<string, object> verified = BaseNestableEventUtil.ResolvePropertyTypes(
                propertyTypesMayPrimitive,
                eventTypeNameResolver);
            return new WrapperEventType(
                metadata,
                underlyingEventType,
                verified,
                eventBeanTypedEventFactory,
                beanEventTypeFactory);
        }
    }
} // end of namespace
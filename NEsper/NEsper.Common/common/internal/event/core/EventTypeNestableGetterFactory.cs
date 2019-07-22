///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.property;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventTypeNestableGetterFactory
    {
        EventPropertyGetterSPI GetPropertyProvidedGetter(
            IDictionary<string, object> nestableTypes,
            string propertyName,
            Property prop,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        EventPropertyGetterMappedSPI GetPropertyProvidedGetterMap(
            IDictionary<string, object> nestableTypes,
            string mappedPropertyName,
            MappedProperty mappedProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        EventPropertyGetterIndexedSPI GetPropertyProvidedGetterIndexed(
            IDictionary<string, object> nestableTypes,
            string indexedPropertyName,
            IndexedProperty indexedProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory);

        EventPropertyGetterSPI GetGetterProperty(
            string name,
            BeanEventType nativeFragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        EventPropertyGetterSPI GetGetterEventBean(
            string name,
            Type underlyingType);

        EventPropertyGetterSPI GetGetterEventBeanArray(
            string name,
            EventType eventType);

        EventPropertyGetterSPI GetGetterBeanNested(
            string name,
            EventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        EventPropertyGetterSPI GetGetterBeanNestedArray(
            string name,
            EventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        EventPropertyGetterSPI GetGetterIndexedEventBean(
            string propertyNameAtomic,
            int index);

        EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType innerType);

        EventPropertyGetterSPI GetGetterIndexedPOJO(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            Type componentType,
            BeanEventTypeFactory beanEventTypeFactory);

        EventPropertyGetterSPI GetGetterMappedProperty(
            string propertyNameAtomic,
            string key);

        EventPropertyGetterSPI GetGetterNestedEntryBeanArray(
            string propertyNameAtomic,
            int index,
            EventPropertyGetter getter,
            EventType innerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(
            string propertyNameAtomic,
            int index,
            EventPropertyGetterSPI nestedGetter);

        EventPropertyGetterSPI GetGetterIndexedEntryPOJO(
            string propertyNameAtomic,
            int index,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type propertyTypeGetter);

        EventPropertyGetterSPI GetGetterNestedMapProp(
            string propertyName,
            MapEventPropertyGetter getterNestedMap);

        EventPropertyGetterSPI GetGetterNestedPOJOProp(
            string propertyName,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type nestedReturnType,
            Type nestedComponentType);

        EventPropertyGetterSPI GetGetterNestedEventBean(
            string propertyName,
            EventPropertyGetterSPI nestedGetter);

        EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(
            IDictionary<string, object> nestableTypes,
            string propertyName,
            EventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        EventPropertyGetterSPI GetGetterNestedEntryBean(
            string propertyName,
            EventPropertyGetter innerGetter,
            EventType innerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);
    }
} // end of namespace
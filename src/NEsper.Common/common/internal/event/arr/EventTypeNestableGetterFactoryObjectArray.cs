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
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.arr
{
    public class EventTypeNestableGetterFactoryObjectArray : EventTypeNestableGetterFactory
    {
        private readonly string _eventTypeName;

        public EventTypeNestableGetterFactoryObjectArray(
            string eventTypeName,
            IDictionary<string, int> propertiesIndex)
        {
            _eventTypeName = eventTypeName;
            PropertiesIndex = propertiesIndex;
        }

        public IDictionary<string, int> PropertiesIndex { get; }

        public EventPropertyGetterSPI GetPropertyDynamicGetter(
            IDictionary<string, object> nestableTypes,
            string propertyExpression,
            DynamicProperty prop,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return prop.GetGetterObjectArray(
                PropertiesIndex,
                nestableTypes,
                eventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public EventPropertyGetterSPI GetGetterProperty(
            string name,
            BeanEventType nativeFragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            bool canFragment)
        {
            var index = GetAssertIndex(name);
            return new ObjectArrayEntryPropertyGetter(index, nativeFragmentType, eventBeanTypedEventFactory);
        }

        public EventPropertyGetterSPI GetGetterEventBean(
            string name,
            Type underlyingType)
        {
            var index = GetAssertIndex(name);
            return new ObjectArrayEventBeanPropertyGetter(index, underlyingType);
        }

        public EventPropertyGetterSPI GetGetterEventBeanArray(
            string name,
            EventType eventType)
        {
            var index = GetAssertIndex(name);
            return new ObjectArrayEventBeanArrayPropertyGetter(index, eventType.UnderlyingType);
        }

        public EventPropertyGetterSPI GetGetterBeanNestedArray(
            string name,
            EventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var index = GetAssertIndex(name);
            return new ObjectArrayFragmentArrayPropertyGetter(index, eventType, eventBeanTypedEventFactory);
        }

        public EventPropertyGetterSPI GetGetterIndexedEventBean(
            string propertyNameAtomic,
            int index)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayEventBeanArrayIndexedPropertyGetter(propertyIndex, index);
        }

        public EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType innerType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPropertyGetter(propertyIndex, index, eventBeanTypedEventFactory, innerType);
        }

        public EventPropertyGetterSPI GetGetterIndexedClassArray(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            Type componentType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPONOEntryIndexedPropertyGetter(
                propertyIndex,
                index,
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                componentType);
        }

        public EventPropertyGetterSPI GetGetterMappedProperty(
            string propertyNameAtomic,
            string key)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayMappedPropertyGetter(propertyIndex, key);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(
            string propertyNameAtomic,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayEventBeanArrayIndexedElementPropertyGetter(propertyIndex, index, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryPONO(
            string propertyNameAtomic,
            int index,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type propertyTypeGetter)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPONOBeanEntryIndexedPropertyGetter(
                propertyIndex,
                index,
                nestedGetter,
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                propertyTypeGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedMapProp(
            string propertyName,
            MapEventPropertyGetter getterNested)
        {
            var index = GetAssertIndex(propertyName);
            return new ObjectArrayMapPropertyGetter(index, getterNested);
        }

        public EventPropertyGetterSPI GetGetterNestedPONOProp(
            string propertyName,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type nestedReturnType)
        {
            var index = GetAssertIndex(propertyName);
            return new ObjectArrayPONOEntryPropertyGetter(
                index,
                nestedGetter,
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                nestedReturnType);
        }

        public EventPropertyGetterSPI GetGetterNestedEventBean(
            string propertyName,
            EventPropertyGetterSPI nestedGetter)
        {
            var index = GetAssertIndex(propertyName);
            return new ObjectArrayEventBeanEntryPropertyGetter(index, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedEntryBean(
            string propertyName,
            EventPropertyGetter getter,
            EventType innerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var propertyIndex = GetAssertIndex(propertyName);
            if (getter is ObjectArrayEventPropertyGetter propertyGetter) {
                return new ObjectArrayNestedEntryPropertyGetterObjectArray(
                    propertyIndex,
                    innerType,
                    eventBeanTypedEventFactory,
                    propertyGetter);
            }

            return new ObjectArrayNestedEntryPropertyGetterMap(
                propertyIndex,
                innerType,
                eventBeanTypedEventFactory,
                (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetterSPI GetGetterNestedEntryBeanArray(
            string propertyNameAtomic,
            int index,
            EventPropertyGetter getter,
            EventType innerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var propertyIndex = GetAssertIndex(propertyNameAtomic);
            if (getter is ObjectArrayEventPropertyGetter propertyGetter) {
                return new ObjectArrayNestedEntryPropertyGetterArrayObjectArray(
                    propertyIndex,
                    innerType,
                    eventBeanTypedEventFactory,
                    index,
                    propertyGetter);
            }

            return new ObjectArrayNestedEntryPropertyGetterArrayMap(
                propertyIndex,
                innerType,
                eventBeanTypedEventFactory,
                index,
                (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetterSPI GetGetterBeanNested(
            string name,
            EventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            var index = GetAssertIndex(name);
            if (eventType is ObjectArrayEventType) {
                return new ObjectArrayPropertyGetterDefaultObjectArray(index, eventType, eventBeanTypedEventFactory);
            }

            return new ObjectArrayPropertyGetterDefaultMap(index, eventType, eventBeanTypedEventFactory);
        }

        public EventPropertyGetterMappedSPI GetPropertyProvidedGetterMap(
            IDictionary<string, object> nestableTypes,
            string mappedPropertyName,
            MappedProperty mappedProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return (EventPropertyGetterMappedSPI)mappedProperty.GetGetterObjectArray(
                PropertiesIndex,
                nestableTypes,
                eventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public EventPropertyGetterIndexedSPI GetPropertyProvidedGetterIndexed(
            IDictionary<string, object> nestableTypes,
            string indexedPropertyName,
            IndexedProperty indexedProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return (EventPropertyGetterIndexedSPI)indexedProperty.GetGetterObjectArray(
                PropertiesIndex,
                nestableTypes,
                eventBeanTypedEventFactory,
                beanEventTypeFactory);
        }

        public EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(
            IDictionary<string, object> nestableTypes,
            string propertyName,
            EventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return null; // this case is not supported
        }

        private int GetAssertIndex(string propertyName)
        {
            if (!PropertiesIndex.TryGetValue(propertyName, out var index)) {
                throw new PropertyAccessException(
                    "Property '" +
                    propertyName +
                    "' could not be found as a property of type '" +
                    _eventTypeName +
                    "'");
            }

            return index;
        }

        public EventPropertyGetterSPI GetGetterRootedDynamicNested(
            Property prop,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            throw new IllegalStateException("This getter is not available for object-array events");
        }
    }
} // end of namespace
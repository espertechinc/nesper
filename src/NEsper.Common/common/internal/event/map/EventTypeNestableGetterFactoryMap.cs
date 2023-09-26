///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.property;


namespace com.espertech.esper.common.@internal.@event.map
{
    public class EventTypeNestableGetterFactoryMap : EventTypeNestableGetterFactory
    {
        public EventPropertyGetterSPI GetPropertyDynamicGetter(
            IDictionary<string, object> nestableTypes,
            string propertyExpression,
            DynamicProperty prop,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return prop.GetGetterMap(nestableTypes, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        public EventPropertyGetterMappedSPI GetPropertyProvidedGetterMap(
            IDictionary<string, object> nestableTypes,
            string mappedPropertyName,
            MappedProperty mappedProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return (EventPropertyGetterMappedSPI) mappedProperty.GetGetterMap(nestableTypes, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        public EventPropertyGetterIndexedSPI GetPropertyProvidedGetterIndexed(
            IDictionary<string, object> nestableTypes,
            string indexedPropertyName,
            IndexedProperty indexedProperty,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return (EventPropertyGetterIndexedSPI) indexedProperty.GetGetterMap(nestableTypes, eventBeanTypedEventFactory, beanEventTypeFactory);
        }

        public EventPropertyGetterSPI GetGetterProperty(
            string name,
            BeanEventType nativeFragmentType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            bool canFragment)
        {
            return new MapEntryPropertyGetter(name, nativeFragmentType, eventBeanTypedEventFactory, canFragment);
        }

        public EventPropertyGetterSPI GetGetterEventBean(
            string name,
            Type underlyingType)
        {
            return new MapEventBeanPropertyGetter(name, underlyingType);
        }

        public EventPropertyGetterSPI GetGetterEventBeanArray(
            string name,
            EventType eventType)
        {
            return new MapEventBeanArrayPropertyGetter(name, eventType.UnderlyingType);
        }

        public EventPropertyGetterSPI GetGetterBeanNestedArray(
            string name,
            EventType eventType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new MapFragmentArrayPropertyGetter(name, eventType, eventBeanTypedEventFactory);
        }

        public EventPropertyGetterSPI GetGetterIndexedEventBean(
            string propertyNameAtomic,
            int index)
        {
            return new MapEventBeanArrayIndexedPropertyGetter(propertyNameAtomic, index);
        }

        public EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventType innerType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new MapArrayPropertyGetter(propertyNameAtomic, index, eventBeanTypedEventFactory, innerType);
        }

        public EventPropertyGetterSPI GetGetterIndexedClassArray(
            string propertyNameAtomic,
            int index,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            Type componentType,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return new MapArrayPONOEntryIndexedPropertyGetter(
                propertyNameAtomic,
                index,
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                componentType);
        }

        public EventPropertyGetterSPI GetGetterMappedProperty(
            string propertyNameAtomic,
            string key)
        {
            return new MapMappedPropertyGetter(propertyNameAtomic, key);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(
            string propertyNameAtomic,
            int index,
            EventPropertyGetterSPI nestedGetter)
        {
            return new MapEventBeanArrayIndexedElementPropertyGetter(propertyNameAtomic, index, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryPONO(
            string propertyNameAtomic,
            int index,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type propertyTypeGetter)
        {
            return new MapArrayPONOBeanEntryIndexedPropertyGetter(
                propertyNameAtomic,
                index,
                nestedGetter,
                eventBeanTypedEventFactory,
                beanEventTypeFactory,
                propertyTypeGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedMapProp(
            string propertyName,
            MapEventPropertyGetter getterNestedMap)
        {
            return new MapMapPropertyGetter(propertyName, getterNestedMap);
        }

        public EventPropertyGetterSPI GetGetterNestedPONOProp(
            string propertyName,
            BeanEventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory,
            Type nestedValueType)
        {
            return new MapPONOEntryPropertyGetter(
                propertyName,
                nestedGetter,
                eventBeanTypedEventFactory,
                nestedValueType,
                beanEventTypeFactory);
        }

        public EventPropertyGetterSPI GetGetterNestedEventBean(
            string propertyName,
            EventPropertyGetterSPI nestedGetter)
        {
            return new MapEventBeanEntryPropertyGetter(propertyName, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedEntryBean(
            string propertyName,
            EventPropertyGetter getter,
            EventType innerType,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            if (getter is ObjectArrayEventPropertyGetter propertyGetter) {
                return new MapNestedEntryPropertyGetterObjectArray(
                    propertyName,
                    innerType,
                    eventBeanTypedEventFactory,
                    propertyGetter);
            }

            return new MapNestedEntryPropertyGetterMap(
                propertyName,
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
            if (getter is ObjectArrayEventPropertyGetter propertyGetter) {
                return new MapNestedEntryPropertyGetterArrayObjectArray(
                    propertyNameAtomic,
                    innerType,
                    eventBeanTypedEventFactory,
                    index,
                    propertyGetter);
            }

            return new MapNestedEntryPropertyGetterArrayMap(
                propertyNameAtomic,
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
            if (eventType is ObjectArrayEventType) {
                return new MapPropertyGetterDefaultObjectArray(name, eventType, eventBeanTypedEventFactory);
            }

            return new MapPropertyGetterDefaultMap(name, eventType, eventBeanTypedEventFactory);
        }

        public EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(
            IDictionary<string, object> nestableTypes,
            string propertyName,
            EventPropertyGetter nestedGetter,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return new MapNestedEntryPropertyGetterPropertyProvidedDynamic(
                propertyName,
                null,
                eventBeanTypedEventFactory,
                nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterRootedDynamicNested(
            Property prop,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            BeanEventTypeFactory beanEventTypeFactory)
        {
            return prop.GetGetterMap(null, eventBeanTypedEventFactory, beanEventTypeFactory);
        }
    }
} // end of namespace
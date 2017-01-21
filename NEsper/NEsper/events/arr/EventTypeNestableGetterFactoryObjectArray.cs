///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.property;

namespace com.espertech.esper.events.arr
{
    public class EventTypeNestableGetterFactoryObjectArray : EventTypeNestableGetterFactory
    {
        private readonly String _eventTypeName;

        public EventTypeNestableGetterFactoryObjectArray(String eventTypeName, IDictionary<String, int> propertiesIndex)
        {
            _eventTypeName = eventTypeName;
            PropertiesIndex = propertiesIndex;
        }

        public IDictionary<string, int> PropertiesIndex { get; private set; }

        public EventPropertyGetter GetPropertyProvidedGetter(IDictionary<String, Object> nestableTypes, String propertyName, Property prop, EventAdapterService eventAdapterService)
        {
            return prop.GetGetterObjectArray(PropertiesIndex, nestableTypes, eventAdapterService);
        }

        public EventPropertyGetter GetGetterProperty(String name, BeanEventType nativeFragmentType, EventAdapterService eventAdapterService)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayEntryPropertyGetter(index, nativeFragmentType, eventAdapterService);
        }

        public EventPropertyGetter GetGetterEventBean(String name)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayEventBeanPropertyGetter(index);
        }

        public EventPropertyGetter GetGetterEventBeanArray(String name, EventType eventType)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayEventBeanArrayPropertyGetter(index, eventType.UnderlyingType);
        }

        public EventPropertyGetter GetGetterBeanNestedArray(String name, EventType eventType, EventAdapterService eventAdapterService)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayFragmentArrayPropertyGetter(index, eventType, eventAdapterService);
        }

        public EventPropertyGetter GetGetterIndexedEventBean(String propertyNameAtomic, int index)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayEventBeanArrayIndexedPropertyGetter(propertyIndex, index);
        }

        public EventPropertyGetter GetGetterIndexedUnderlyingArray(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, EventType innerType)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPropertyGetter(propertyIndex, index, eventAdapterService, innerType);
        }

        public EventPropertyGetter GetGetterIndexedPONO(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, Type componentType)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPONOEntryIndexedPropertyGetter(propertyIndex, index, eventAdapterService, componentType);
        }

        public EventPropertyGetter GetGetterMappedProperty(String propertyNameAtomic, String key)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayMappedPropertyGetter(propertyIndex, key);
        }

        public EventPropertyGetter GetGetterIndexedEntryEventBeanArrayElement(String propertyNameAtomic, int index, EventPropertyGetter nestedGetter)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayEventBeanArrayIndexedElementPropertyGetter(propertyIndex, index, nestedGetter);
        }

        public EventPropertyGetter GetGetterIndexedEntryPONO(String propertyNameAtomic, int index, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type propertyTypeGetter)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPONOBeanEntryIndexedPropertyGetter(propertyIndex, index, nestedGetter, eventAdapterService, propertyTypeGetter);
        }

        public EventPropertyGetter GetGetterNestedMapProp(String propertyName, MapEventPropertyGetter getterNested)
        {
            int index = GetAssertIndex(propertyName);
            return new ObjectArrayMapPropertyGetter(index, getterNested);
        }

        public EventPropertyGetter GetGetterNestedPONOProp(String propertyName, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type nestedReturnType, Type nestedComponentType)
        {
            int index = GetAssertIndex(propertyName);
            return new ObjectArrayPONOEntryPropertyGetter(index, nestedGetter, eventAdapterService, nestedReturnType, nestedComponentType);
        }

        public EventPropertyGetter GetGetterNestedEventBean(String propertyName, EventPropertyGetter nestedGetter)
        {
            int index = GetAssertIndex(propertyName);
            return new ObjectArrayEventBeanEntryPropertyGetter(index, nestedGetter);
        }

        public EventPropertyGetter GetGetterNestedEntryBean(String propertyName, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService)
        {
            int propertyIndex = GetAssertIndex(propertyName);
            if (getter is ObjectArrayEventPropertyGetter)
            {
                return new ObjectArrayNestedEntryPropertyGetterObjectArray(propertyIndex, innerType, eventAdapterService, (ObjectArrayEventPropertyGetter)getter);
            }
            return new ObjectArrayNestedEntryPropertyGetterMap(propertyIndex, innerType, eventAdapterService, (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetter GetGetterNestedEntryBeanArray(String propertyNameAtomic, int index, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            if (getter is ObjectArrayEventPropertyGetter)
            {
                return new ObjectArrayNestedEntryPropertyGetterArrayObjectArray(propertyIndex, innerType, eventAdapterService, index, (ObjectArrayEventPropertyGetter)getter);
            }
            return new ObjectArrayNestedEntryPropertyGetterArrayMap(propertyIndex, innerType, eventAdapterService, index, (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetter GetGetterBeanNested(String name, EventType eventType, EventAdapterService eventAdapterService)
        {
            int index = GetAssertIndex(name);
            if (eventType is ObjectArrayEventType)
            {
                return new ObjectArrayPropertyGetterDefaultObjectArray(index, eventType, eventAdapterService);
            }
            return new ObjectArrayPropertyGetterDefaultMap(index, eventType, eventAdapterService);
        }

        private int GetAssertIndex(String propertyName)
        {
            int index;
            if (!PropertiesIndex.TryGetValue(propertyName, out index))
            {
                throw new PropertyAccessException("Property '" + propertyName + "' could not be found as a property of type '" + _eventTypeName + "'");
            }
            return index;
        }

        public EventPropertyGetterMapped GetPropertyProvidedGetterMap(IDictionary<String, Object> nestableTypes, String mappedPropertyName, MappedProperty mappedProperty, EventAdapterService eventAdapterService)
        {
            return (EventPropertyGetterMapped) mappedProperty.GetGetterObjectArray(PropertiesIndex, nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterIndexed GetPropertyProvidedGetterIndexed(IDictionary<String, Object> nestableTypes, String indexedPropertyName, IndexedProperty indexedProperty, EventAdapterService eventAdapterService)
        {
            return (EventPropertyGetterIndexed) indexedProperty.GetGetterObjectArray(PropertiesIndex, nestableTypes, eventAdapterService);
        }
    }
}

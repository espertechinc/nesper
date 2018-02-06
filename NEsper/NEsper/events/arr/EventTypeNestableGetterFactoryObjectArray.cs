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
using com.espertech.esper.events;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.map;
using com.espertech.esper.events.property;

namespace com.espertech.esper.events.arr
{
    public class EventTypeNestableGetterFactoryObjectArray : EventTypeNestableGetterFactory
    {
        private readonly string _eventTypeName;

        public EventTypeNestableGetterFactoryObjectArray(string eventTypeName, IDictionary<string, int> propertiesIndex)
        {
            _eventTypeName = eventTypeName;
            PropertiesIndex = propertiesIndex;
        }

        public IDictionary<string, int> PropertiesIndex { get; private set; }

        public EventPropertyGetterSPI GetPropertyProvidedGetter(IDictionary<string, object> nestableTypes, string propertyName, Property prop, EventAdapterService eventAdapterService)
        {
            return prop.GetGetterObjectArray(PropertiesIndex, nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterProperty(string name, BeanEventType nativeFragmentType, EventAdapterService eventAdapterService)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayEntryPropertyGetter(index, nativeFragmentType, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterEventBean(string name)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayEventBeanPropertyGetter(index);
        }

        public EventPropertyGetterSPI GetGetterEventBeanArray(string name, EventType eventType)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayEventBeanArrayPropertyGetter(index, eventType.UnderlyingType);
        }

        public EventPropertyGetterSPI GetGetterBeanNestedArray(string name, EventType eventType, EventAdapterService eventAdapterService)
        {
            int index = GetAssertIndex(name);
            return new ObjectArrayFragmentArrayPropertyGetter(index, eventType, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterIndexedEventBean(string propertyNameAtomic, int index)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayEventBeanArrayIndexedPropertyGetter(propertyIndex, index);
        }

        public EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(string propertyNameAtomic, int index, EventAdapterService eventAdapterService, EventType innerType)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPropertyGetter(propertyIndex, index, eventAdapterService, innerType);
        }

        public EventPropertyGetterSPI GetGetterIndexedPono(string propertyNameAtomic, int index, EventAdapterService eventAdapterService, Type componentType)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPonoEntryIndexedPropertyGetter(propertyIndex, index, eventAdapterService, componentType);
        }

        public EventPropertyGetterSPI GetGetterMappedProperty(string propertyNameAtomic, string key)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayMappedPropertyGetter(propertyIndex, key);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(string propertyNameAtomic, int index, EventPropertyGetterSPI nestedGetter)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayEventBeanArrayIndexedElementPropertyGetter(propertyIndex, index, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryPono(string propertyNameAtomic, int index, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type propertyTypeGetter)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            return new ObjectArrayArrayPonoBeanEntryIndexedPropertyGetter(propertyIndex, index, nestedGetter, eventAdapterService, propertyTypeGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedMapProp(string propertyName, MapEventPropertyGetter getterNested)
        {
            int index = GetAssertIndex(propertyName);
            return new ObjectArrayMapPropertyGetter(index, getterNested);
        }

        public EventPropertyGetterSPI GetGetterNestedPonoProp(string propertyName, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type nestedReturnType, Type nestedComponentType)
        {
            int index = GetAssertIndex(propertyName);
            return new ObjectArrayPonoEntryPropertyGetter(index, nestedGetter, eventAdapterService, nestedReturnType, nestedComponentType);
        }

        public EventPropertyGetterSPI GetGetterNestedEventBean(string propertyName, EventPropertyGetterSPI nestedGetter)
        {
            int index = GetAssertIndex(propertyName);
            return new ObjectArrayEventBeanEntryPropertyGetter(index, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedEntryBean(string propertyName, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService)
        {
            int propertyIndex = GetAssertIndex(propertyName);
            if (getter is ObjectArrayEventPropertyGetter)
            {
                return new ObjectArrayNestedEntryPropertyGetterObjectArray(propertyIndex, innerType, eventAdapterService, (ObjectArrayEventPropertyGetter)getter);
            }
            return new ObjectArrayNestedEntryPropertyGetterMap(propertyIndex, innerType, eventAdapterService, (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetterSPI GetGetterNestedEntryBeanArray(string propertyNameAtomic, int index, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService)
        {
            int propertyIndex = GetAssertIndex(propertyNameAtomic);
            if (getter is ObjectArrayEventPropertyGetter)
            {
                return new ObjectArrayNestedEntryPropertyGetterArrayObjectArray(propertyIndex, innerType, eventAdapterService, index, (ObjectArrayEventPropertyGetter)getter);
            }
            return new ObjectArrayNestedEntryPropertyGetterArrayMap(propertyIndex, innerType, eventAdapterService, index, (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetterSPI GetGetterBeanNested(string name, EventType eventType, EventAdapterService eventAdapterService)
        {
            int index = GetAssertIndex(name);
            if (eventType is ObjectArrayEventType)
            {
                return new ObjectArrayPropertyGetterDefaultObjectArray(index, eventType, eventAdapterService);
            }
            return new ObjectArrayPropertyGetterDefaultMap(index, eventType, eventAdapterService);
        }

        private int GetAssertIndex(string propertyName)
        {
            int index;
            if (!PropertiesIndex.TryGetValue(propertyName, out index))
            {
                throw new PropertyAccessException("Property '" + propertyName + "' could not be found as a property of type '" + _eventTypeName + "'");
            }
            return index;
        }

        public EventPropertyGetterMapped GetPropertyProvidedGetterMap(IDictionary<string, object> nestableTypes, string mappedPropertyName, MappedProperty mappedProperty, EventAdapterService eventAdapterService)
        {
            return (EventPropertyGetterMapped)mappedProperty.GetGetterObjectArray(PropertiesIndex, nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterIndexed GetPropertyProvidedGetterIndexed(IDictionary<string, object> nestableTypes, string indexedPropertyName, IndexedProperty indexedProperty, EventAdapterService eventAdapterService)
        {
            return (EventPropertyGetterIndexed)indexedProperty.GetGetterObjectArray(PropertiesIndex, nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(IDictionary<string, object> nestableTypes, string propertyName, EventPropertyGetter nestedGetter, EventAdapterService eventAdapterService)
        {
            return null; // this case is not supported
        }
    }
}

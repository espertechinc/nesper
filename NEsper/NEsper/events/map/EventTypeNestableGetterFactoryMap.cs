///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.property;

namespace com.espertech.esper.events.map
{
    public class EventTypeNestableGetterFactoryMap : EventTypeNestableGetterFactory
    {
        public EventPropertyGetter GetPropertyProvidedGetter(IDictionary<String, Object> nestableTypes, String propertyName, Property prop, EventAdapterService eventAdapterService)
        {
            return prop.GetGetterMap(nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterMapped GetPropertyProvidedGetterMap(IDictionary<String, Object> nestableTypes, String mappedPropertyName, MappedProperty mappedProperty, EventAdapterService eventAdapterService)
        {
            return mappedProperty.GetGetterMap(nestableTypes, eventAdapterService) as EventPropertyGetterMapped;
        }

        public EventPropertyGetterIndexed GetPropertyProvidedGetterIndexed(IDictionary<String, Object> nestableTypes, String indexedPropertyName, IndexedProperty indexedProperty, EventAdapterService eventAdapterService)
        {
            return indexedProperty.GetGetterMap(nestableTypes, eventAdapterService) as EventPropertyGetterIndexed;
        }

        public EventPropertyGetter GetGetterProperty(String name, BeanEventType nativeFragmentType, EventAdapterService eventAdapterService)
        {
            return new MapEntryPropertyGetter(name, nativeFragmentType, eventAdapterService);
        }

        public EventPropertyGetter GetGetterEventBean(String name)
        {
            return new MapEventBeanPropertyGetter(name);
        }

        public EventPropertyGetter GetGetterEventBeanArray(String name, EventType eventType)
        {
            return new MapEventBeanArrayPropertyGetter(name, eventType.UnderlyingType);
        }

        public EventPropertyGetter GetGetterBeanNestedArray(String name, EventType eventType, EventAdapterService eventAdapterService)
        {
            return new MapFragmentArrayPropertyGetter(name, eventType, eventAdapterService);
        }

        public EventPropertyGetter GetGetterIndexedEventBean(String propertyNameAtomic, int index)
        {
            return new MapEventBeanArrayIndexedPropertyGetter(propertyNameAtomic, index);
        }

        public EventPropertyGetter GetGetterIndexedUnderlyingArray(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, EventType innerType)
        {
            return new MapArrayPropertyGetter(propertyNameAtomic, index, eventAdapterService, innerType);
        }

        public EventPropertyGetter GetGetterIndexedPONO(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, Type componentType)
        {
            return new MapArrayEntryIndexedPropertyGetter(propertyNameAtomic, index, eventAdapterService, componentType);
        }

        public EventPropertyGetter GetGetterMappedProperty(String propertyNameAtomic, String key)
        {
            return new MapMappedPropertyGetter(propertyNameAtomic, key);
        }

        public EventPropertyGetter GetGetterIndexedEntryEventBeanArrayElement(String propertyNameAtomic, int index, EventPropertyGetter nestedGetter)
        {
            return new MapEventBeanArrayIndexedElementPropertyGetter(propertyNameAtomic, index, nestedGetter);
        }

        public EventPropertyGetter GetGetterIndexedEntryPONO(String propertyNameAtomic, int index, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type propertyTypeGetter)
        {
            return new MapArrayBeanEntryIndexedPropertyGetter(propertyNameAtomic, index, nestedGetter, eventAdapterService, propertyTypeGetter);
        }

        public EventPropertyGetter GetGetterNestedMapProp(String propertyName, MapEventPropertyGetter getterNestedMap)
        {
            return new MapMapPropertyGetter(propertyName, getterNestedMap);
        }

        public EventPropertyGetter GetGetterNestedPONOProp(String propertyName, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type nestedReturnType, Type nestedComponentType)
        {
            return new MapObjectEntryPropertyGetter(propertyName, nestedGetter, eventAdapterService, nestedReturnType, nestedComponentType);
        }

        public EventPropertyGetter GetGetterNestedEventBean(String propertyName, EventPropertyGetter nestedGetter)
        {
            return new MapEventBeanEntryPropertyGetter(propertyName, nestedGetter);
        }

        public EventPropertyGetter GetGetterNestedEntryBean(String propertyName, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService)
        {
            if (getter is ObjectArrayEventPropertyGetter)
            {
                return new MapNestedEntryPropertyGetterObjectArray(propertyName, innerType, eventAdapterService, (ObjectArrayEventPropertyGetter)getter);
            }
            return new MapNestedEntryPropertyGetterMap(propertyName, innerType, eventAdapterService, (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetter GetGetterNestedEntryBeanArray(String propertyNameAtomic, int index, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService)
        {
            if (getter is ObjectArrayEventPropertyGetter)
            {
                return new MapNestedEntryPropertyGetterArrayObjectArray(propertyNameAtomic, innerType, eventAdapterService, index, (ObjectArrayEventPropertyGetter)getter);
            }
            return new MapNestedEntryPropertyGetterArrayMap(propertyNameAtomic, innerType, eventAdapterService, index, (MapEventPropertyGetter)getter);
        }

        public EventPropertyGetter GetGetterBeanNested(String name, EventType eventType, EventAdapterService eventAdapterService)
        {
            if (eventType is ObjectArrayEventType)
            {
                return new MapPropertyGetterDefaultObjectArray(name, eventType, eventAdapterService);
            }
            return new MapPropertyGetterDefaultMap(name, eventType, eventAdapterService);
        }
    }
}

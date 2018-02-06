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
using com.espertech.esper.events.arr;
using com.espertech.esper.events.bean;
using com.espertech.esper.events.property;

using static com.espertech.esper.codegen.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.events.map
{
    public class EventTypeNestableGetterFactoryMap : EventTypeNestableGetterFactory
    {
        public EventPropertyGetterMapped GetPropertyProvidedGetterMap(
            IDictionary<string, object> nestableTypes,
            string mappedPropertyName, 
            MappedProperty mappedProperty,
            EventAdapterService eventAdapterService)
        {
            return (EventPropertyGetterMapped) mappedProperty.GetGetterMap(nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterIndexed GetPropertyProvidedGetterIndexed(
            IDictionary<string, object> nestableTypes,
            string indexedPropertyName,
            IndexedProperty indexedProperty, 
            EventAdapterService eventAdapterService)
        {
            return (EventPropertyGetterIndexed) indexedProperty.GetGetterMap(nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterSPI GetPropertyProvidedGetter(
            IDictionary<string, object> nestableTypes,
            string propertyName,
            Property prop, 
            EventAdapterService eventAdapterService)
        {
            return prop.GetGetterMap(nestableTypes, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterProperty(
            string name,
            BeanEventType nativeFragmentType,
            EventAdapterService eventAdapterService)
        {
            return new MapEntryPropertyGetter(name, nativeFragmentType, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterEventBean(
            string name)
        {
            return new MapEventBeanPropertyGetter(name);
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
            EventAdapterService eventAdapterService)
        {
            return new MapFragmentArrayPropertyGetter(name, eventType, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterIndexedEventBean(
            string propertyNameAtomic, int index)
        {
            return new MapEventBeanArrayIndexedPropertyGetter(propertyNameAtomic, index);
        }

        public EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(
            string propertyNameAtomic, int index,
            EventAdapterService eventAdapterService,
            EventType innerType)
        {
            return new MapArrayPropertyGetter(propertyNameAtomic, index, eventAdapterService, innerType);
        }

        public EventPropertyGetterSPI GetGetterIndexedPono(
            string propertyNameAtomic, int index,
            EventAdapterService eventAdapterService,
            Type componentType)
        {
            return new MapArrayPonoEntryIndexedPropertyGetter(propertyNameAtomic, index, eventAdapterService,
                componentType);
        }

        public EventPropertyGetterSPI GetGetterMappedProperty(
            string propertyNameAtomic, string key)
        {
            return new MapMappedPropertyGetter(propertyNameAtomic, key);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(
            string propertyNameAtomic, int index,
            EventPropertyGetterSPI nestedGetter)
        {
            return new MapEventBeanArrayIndexedElementPropertyGetter(propertyNameAtomic, index, nestedGetter);
        }

        public EventPropertyGetterSPI GetGetterIndexedEntryPono(
            string propertyNameAtomic, int index,
            BeanEventPropertyGetter nestedGetter,
            EventAdapterService eventAdapterService, 
            Type propertyTypeGetter)
        {
            return new MapArrayPonoBeanEntryIndexedPropertyGetter(propertyNameAtomic, index, nestedGetter,
                eventAdapterService, propertyTypeGetter);
        }

        public EventPropertyGetterSPI GetGetterNestedMapProp(
            string propertyName,
            MapEventPropertyGetter getterNestedMap)
        {
            return new MapMapPropertyGetter(propertyName, getterNestedMap);
        }

        public EventPropertyGetterSPI GetGetterNestedPonoProp(
            string propertyName,
            BeanEventPropertyGetter nestedGetter,
            EventAdapterService eventAdapterService,
            Type nestedReturnType,
            Type nestedComponentType)
        {
            return new MapPonoEntryPropertyGetter(propertyName, nestedGetter, eventAdapterService, nestedReturnType,
                nestedComponentType);
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
            EventAdapterService eventAdapterService)
        {
            if (getter is ObjectArrayEventPropertyGetter)
                return new MapNestedEntryPropertyGetterObjectArray(propertyName, innerType, eventAdapterService,
                    (ObjectArrayEventPropertyGetter) getter);
            return new MapNestedEntryPropertyGetterMap(propertyName, innerType, eventAdapterService,
                (MapEventPropertyGetter) getter);
        }

        public EventPropertyGetterSPI GetGetterNestedEntryBeanArray(
            string propertyNameAtomic, int index, 
            EventPropertyGetter getter, 
            EventType innerType, 
            EventAdapterService eventAdapterService)
        {
            if (getter is ObjectArrayEventPropertyGetter)
                return new MapNestedEntryPropertyGetterArrayObjectArray(propertyNameAtomic, innerType,
                    eventAdapterService, index, (ObjectArrayEventPropertyGetter) getter);
            return new MapNestedEntryPropertyGetterArrayMap(propertyNameAtomic, innerType, eventAdapterService, index,
                (MapEventPropertyGetter) getter);
        }

        public EventPropertyGetterSPI GetGetterBeanNested(
            string name,
            EventType eventType,
            EventAdapterService eventAdapterService)
        {
            if (eventType is ObjectArrayEventType)
                return new MapPropertyGetterDefaultObjectArray(name, eventType, eventAdapterService);
            return new MapPropertyGetterDefaultMap(name, eventType, eventAdapterService);
        }

        public EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(
            IDictionary<string, object> nestableTypes,
            string propertyName,
            EventPropertyGetter nestedGetter,
            EventAdapterService eventAdapterService)
        {
            return new MapNestedEntryPropertyGetterPropertyProvidedDynamic(propertyName, null, eventAdapterService,
                nestedGetter);
        }
    }
} // end of namespace
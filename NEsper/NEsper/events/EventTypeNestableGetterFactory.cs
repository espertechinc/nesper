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

namespace com.espertech.esper.events
{
    public interface EventTypeNestableGetterFactory
    {
        EventPropertyGetter GetPropertyProvidedGetter(IDictionary<string, object> nestableTypes, String propertyName, Property prop, EventAdapterService eventAdapterService);
        EventPropertyGetterMapped GetPropertyProvidedGetterMap(IDictionary<string, object> nestableTypes, String mappedPropertyName, MappedProperty mappedProperty, EventAdapterService eventAdapterService);
        EventPropertyGetterIndexed GetPropertyProvidedGetterIndexed(IDictionary<string, object> nestableTypes, String indexedPropertyName, IndexedProperty indexedProperty, EventAdapterService eventAdapterService);
    
        EventPropertyGetter GetGetterProperty(String name, BeanEventType nativeFragmentType, EventAdapterService eventAdapterService);
        EventPropertyGetter GetGetterEventBean(String name);
        EventPropertyGetter GetGetterEventBeanArray(String name, EventType eventType);
        EventPropertyGetter GetGetterBeanNested(String name, EventType eventType, EventAdapterService eventAdapterService);
        EventPropertyGetter GetGetterBeanNestedArray(String name, EventType eventType, EventAdapterService eventAdapterService);
        EventPropertyGetter GetGetterIndexedEventBean(String propertyNameAtomic, int index);
    
        EventPropertyGetter GetGetterIndexedUnderlyingArray(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, EventType innerType);
        EventPropertyGetter GetGetterIndexedPONO(String propertyNameAtomic, int index, EventAdapterService eventAdapterService, Type componentType);
        EventPropertyGetter GetGetterMappedProperty(String propertyNameAtomic, String key);
    
        EventPropertyGetter GetGetterNestedEntryBeanArray(String propertyNameAtomic, int index, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService);
        EventPropertyGetter GetGetterIndexedEntryEventBeanArrayElement(String propertyNameAtomic, int index, EventPropertyGetter nestedGetter);
        EventPropertyGetter GetGetterIndexedEntryPONO(String propertyNameAtomic, int index, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type propertyTypeGetter);
        EventPropertyGetter GetGetterNestedMapProp(String propertyName, MapEventPropertyGetter getterNestedMap);
        EventPropertyGetter GetGetterNestedPONOProp(String propertyName, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type nTypeReturnType, Type nestedComponentType);
        EventPropertyGetter GetGetterNestedEventBean(String propertyName, EventPropertyGetter nestedGetter);
    
        EventPropertyGetter GetGetterNestedEntryBean(String propertyName, EventPropertyGetter innerGetter, EventType innerType, EventAdapterService eventAdapterService);
    }
}

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
        EventPropertyGetterSPI GetPropertyProvidedGetter(IDictionary<string, object> nestableTypes, string propertyName, Property prop, EventAdapterService eventAdapterService);

        EventPropertyGetterMapped GetPropertyProvidedGetterMap(IDictionary<string, object> nestableTypes, string mappedPropertyName, MappedProperty mappedProperty, EventAdapterService eventAdapterService);

        EventPropertyGetterIndexed GetPropertyProvidedGetterIndexed(IDictionary<string, object> nestableTypes, string indexedPropertyName, IndexedProperty indexedProperty, EventAdapterService eventAdapterService);

        EventPropertyGetterSPI GetGetterProperty(string name, BeanEventType nativeFragmentType, EventAdapterService eventAdapterService);

        EventPropertyGetterSPI GetGetterEventBean(string name);

        EventPropertyGetterSPI GetGetterEventBeanArray(string name, EventType eventType);

        EventPropertyGetterSPI GetGetterBeanNested(string name, EventType eventType, EventAdapterService eventAdapterService);

        EventPropertyGetterSPI GetGetterBeanNestedArray(string name, EventType eventType, EventAdapterService eventAdapterService);

        EventPropertyGetterSPI GetGetterIndexedEventBean(string propertyNameAtomic, int index);

        EventPropertyGetterSPI GetGetterIndexedUnderlyingArray(string propertyNameAtomic, int index, EventAdapterService eventAdapterService, EventType innerType);

        EventPropertyGetterSPI GetGetterIndexedPono(string propertyNameAtomic, int index, EventAdapterService eventAdapterService, Type componentType);

        EventPropertyGetterSPI GetGetterMappedProperty(string propertyNameAtomic, string key);

        EventPropertyGetterSPI GetGetterNestedEntryBeanArray(string propertyNameAtomic, int index, EventPropertyGetter getter, EventType innerType, EventAdapterService eventAdapterService);

        EventPropertyGetterSPI GetGetterIndexedEntryEventBeanArrayElement(string propertyNameAtomic, int index, EventPropertyGetterSPI nestedGetter);

        EventPropertyGetterSPI GetGetterIndexedEntryPono(string propertyNameAtomic, int index, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type propertyTypeGetter);

        EventPropertyGetterSPI GetGetterNestedMapProp(string propertyName, MapEventPropertyGetter getterNestedMap);

        EventPropertyGetterSPI GetGetterNestedPonoProp(string propertyName, BeanEventPropertyGetter nestedGetter, EventAdapterService eventAdapterService, Type nestedReturnType, Type nestedComponentType);

        EventPropertyGetterSPI GetGetterNestedEventBean(string propertyName, EventPropertyGetterSPI nestedGetter);

        EventPropertyGetterSPI GetGetterNestedPropertyProvidedGetterDynamic(IDictionary<string, object> nestableTypes, string propertyName, EventPropertyGetter nestedGetter, EventAdapterService eventAdapterService);

        EventPropertyGetterSPI GetGetterNestedEntryBean(string propertyName, EventPropertyGetter innerGetter, EventType innerType, EventAdapterService eventAdapterService);
    }
}

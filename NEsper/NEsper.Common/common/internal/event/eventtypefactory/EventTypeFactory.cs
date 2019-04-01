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
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.eventtypefactory
{
	public interface EventTypeFactory {
	    BeanEventType CreateBeanType(BeanEventTypeStem stem, EventTypeMetadata metadata, BeanEventTypeFactory beanEventTypeFactory, EventType[] superTypes, ISet<EventType> deepSuperTypes, string startTimestampPropertyName, string endTimestampPropertyName);

	    MapEventType CreateMap(EventTypeMetadata metadata, LinkedHashMap<string, object> properties, string[] superTypes, string startTimestampPropertyName, string endTimestampPropertyName, BeanEventTypeFactory beanEventTypeFactory, EventTypeNameResolver eventTypeNameResolver);

	    ObjectArrayEventType CreateObjectArray(EventTypeMetadata metadata, LinkedHashMap<string, object> properties, string[] superTypes, string startTimestampPropertyName, string endTimestampPropertyName, BeanEventTypeFactory beanEventTypeFactory, EventTypeNameResolver eventTypeNameResolver);

	    WrapperEventType CreateWrapper(EventTypeMetadata metadata, EventType underlying, LinkedHashMap<string, object> properties, BeanEventTypeFactory beanEventTypeFactory, EventTypeNameResolver eventTypeNameResolver);

	    EventType CreateXMLType(EventTypeMetadata metadata, ConfigurationCommonEventTypeXMLDOM detail, SchemaModel schemaModel, string representsFragmentOfProperty, string representsOriginalTypeName, BeanEventTypeFactory beanEventTypeFactory, XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory, EventTypeNameResolver eventTypeNameResolver);

	    VariantEventType CreateVariant(EventTypeMetadata metadata, VariantSpec spec);
	}
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;

namespace com.espertech.esper.common.@internal.@event.eventtypefactory
{
    public class EventTypeFactoryImpl : EventTypeFactory
    {
        public static readonly EventTypeFactoryImpl INSTANCE = new EventTypeFactoryImpl();

        private EventTypeFactoryImpl()
        {
        }

        public BeanEventType CreateBeanType(
            BeanEventTypeStem stem,
            EventTypeMetadata metadata,
            BeanEventTypeFactory beanEventTypeFactory,
            EventType[] superTypes,
            ICollection<EventType> deepSuperTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName)
        {
            return new BeanEventType(
                stem, metadata, beanEventTypeFactory, superTypes, deepSuperTypes, startTimestampPropertyName, endTimestampPropertyName);
        }

        public MapEventType CreateMap(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            var st = EventTypeUtility.GetSuperTypesDepthFirst(superTypes, EventUnderlyingType.MAP, eventTypeNameResolver);
            properties = BaseNestableEventUtil.ResolvePropertyTypes(properties, eventTypeNameResolver);
            return new MapEventType(
                metadata, properties,
                st.First, st.Second, startTimestampPropertyName, endTimestampPropertyName,
                beanEventTypeFactory);
        }

        public ObjectArrayEventType CreateObjectArray(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            var st = EventTypeUtility.GetSuperTypesDepthFirst(superTypes, EventUnderlyingType.OBJECTARRAY, eventTypeNameResolver);
            properties = BaseNestableEventUtil.ResolvePropertyTypes(properties, eventTypeNameResolver);
            return new ObjectArrayEventType(
                metadata, properties, st.First, st.Second,
                startTimestampPropertyName, endTimestampPropertyName, beanEventTypeFactory);
        }

        public WrapperEventType CreateWrapper(
            EventTypeMetadata metadata,
            EventType underlying,
            IDictionary<string, object> properties,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            return WrapperEventTypeUtil.MakeWrapper(
                metadata, underlying, properties, beanEventTypeFactory.EventBeanTypedEventFactory, beanEventTypeFactory, eventTypeNameResolver);
        }

        public EventType CreateXMLType(
            EventTypeMetadata metadata,
            ConfigurationCommonEventTypeXMLDOM detail,
            SchemaModel schemaModel,
            string representsFragmentOfProperty,
            string representsOriginalTypeName,
            BeanEventTypeFactory beanEventTypeFactory,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            if (metadata.IsPropertyAgnostic) {
                return new SimpleXMLEventType(
                    metadata, detail, beanEventTypeFactory.EventBeanTypedEventFactory, eventTypeNameResolver, xmlFragmentEventTypeFactory);
            }

            return new SchemaXMLEventType(
                metadata, detail, schemaModel, representsFragmentOfProperty, representsOriginalTypeName,
                beanEventTypeFactory.EventBeanTypedEventFactory, eventTypeNameResolver, xmlFragmentEventTypeFactory);
        }

        public VariantEventType CreateVariant(
            EventTypeMetadata metadata,
            VariantSpec spec)
        {
            return new VariantEventType(metadata, spec);
        }
    }
} // end of namespace
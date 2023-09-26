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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.introspect;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.@event.eventtypefactory
{
    public class EventTypeFactoryImpl : EventTypeFactory
    {
        private EventTypeFactoryImpl(IContainer container)
        {
            Container = container;
        }

        public IContainer Container { get; }

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
                Container,
                stem,
                metadata,
                beanEventTypeFactory,
                superTypes,
                deepSuperTypes,
                startTimestampPropertyName,
                endTimestampPropertyName);
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
            var st = EventTypeUtility.GetSuperTypesDepthFirst(
                superTypes,
                EventUnderlyingType.MAP,
                eventTypeNameResolver);
            properties = BaseNestableEventUtil.ResolvePropertyTypes(properties, eventTypeNameResolver);
            return new MapEventType(
                metadata,
                properties,
                st.First,
                st.Second,
                startTimestampPropertyName,
                endTimestampPropertyName,
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
            var st = EventTypeUtility.GetSuperTypesDepthFirst(
                superTypes,
                EventUnderlyingType.OBJECTARRAY,
                eventTypeNameResolver);
            properties = BaseNestableEventUtil.ResolvePropertyTypes(properties, eventTypeNameResolver);
            return new ObjectArrayEventType(
                metadata,
                properties,
                st.First,
                st.Second,
                startTimestampPropertyName,
                endTimestampPropertyName,
                beanEventTypeFactory);
        }

        public WrapperEventType CreateWrapper(
            EventTypeMetadata metadata,
            EventType underlying,
            IDictionary<string, object> properties,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver)
        {
            return WrapperEventTypeUtil.MakeWrapper(
                metadata,
                underlying,
                properties,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                beanEventTypeFactory,
                eventTypeNameResolver);
        }

        public EventType CreateXMLType(
            EventTypeMetadata metadata,
            ConfigurationCommonEventTypeXMLDOM detail,
            SchemaModel schemaModel,
            string representsFragmentOfProperty,
            string representsOriginalTypeName,
            BeanEventTypeFactory beanEventTypeFactory,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver,
            EventTypeXMLXSDHandler xmlXsdHandler)
        {
            if (metadata.IsPropertyAgnostic) {
                return new SimpleXMLEventType(
                    metadata,
                    detail,
                    beanEventTypeFactory.EventBeanTypedEventFactory,
                    eventTypeNameResolver,
                    xmlFragmentEventTypeFactory);
            }

            return new SchemaXMLEventType(
                metadata,
                detail,
                schemaModel,
                representsFragmentOfProperty,
                representsOriginalTypeName,
                beanEventTypeFactory.EventBeanTypedEventFactory,
                eventTypeNameResolver,
                xmlFragmentEventTypeFactory,
                xmlXsdHandler);
        }

        public VariantEventType CreateVariant(
            EventTypeMetadata metadata,
            VariantSpec spec)
        {
            return new VariantEventType(metadata, spec);
        }

        public JsonEventType CreateJson(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeNameResolver eventTypeNameResolver,
            JsonEventTypeDetail detail)
        {
            var st = EventTypeUtility.GetSuperTypesDepthFirst(
                superTypes,
                EventUnderlyingType.JSON,
                eventTypeNameResolver);
            properties = BaseNestableEventUtil.ResolvePropertyTypes(properties, eventTypeNameResolver);
            var getterFactoryJson = new EventTypeNestableGetterFactoryJson(detail);
            // We use a null-stand-in class as the actual underlying class is provided later
            return new JsonEventType(
                metadata,
                properties,
                st.First,
                st.Second,
                startTimestampPropertyName,
                endTimestampPropertyName,
                getterFactoryJson,
                beanEventTypeFactory,
                detail,
                null,
                false);
        }

        public static EventTypeFactoryImpl GetInstance(IContainer container)
        {
            return container.ResolveSingleton(() => new EventTypeFactoryImpl(container));
        }
    }
} // end of namespace
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
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.path
{
    public class EventTypeCollectorImpl : EventTypeCollector
    {
        private readonly BeanEventTypeFactory beanEventTypeFactory;
        private readonly BeanEventTypeStemService beanEventTypeStemService;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly EventTypeAvroHandler eventTypeAvroHandler;
        private readonly EventTypeFactory eventTypeFactory;
        private readonly EventTypeNameResolver eventTypeNameResolver;
        private readonly IDictionary<string, EventType> moduleEventTypes;
        private readonly XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory;

        public EventTypeCollectorImpl(
            IDictionary<string, EventType> moduleEventTypes,
            BeanEventTypeFactory beanEventTypeFactory,
            EventTypeFactory eventTypeFactory,
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeNameResolver eventTypeNameResolver,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory,
            EventTypeAvroHandler eventTypeAvroHandler,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            this.moduleEventTypes = moduleEventTypes;
            this.beanEventTypeFactory = beanEventTypeFactory;
            this.eventTypeFactory = eventTypeFactory;
            this.beanEventTypeStemService = beanEventTypeStemService;
            this.eventTypeNameResolver = eventTypeNameResolver;
            this.xmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
            this.eventTypeAvroHandler = eventTypeAvroHandler;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
        }

        public void RegisterMap(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName)
        {
            var eventType = eventTypeFactory.CreateMap(
                metadata,
                properties,
                superTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                beanEventTypeFactory,
                eventTypeNameResolver);
            HandleRegister(eventType);
        }

        public void RegisterObjectArray(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName)
        {
            var eventType = eventTypeFactory.CreateObjectArray(
                metadata,
                properties,
                superTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                beanEventTypeFactory,
                eventTypeNameResolver);
            HandleRegister(eventType);
        }

        public void RegisterWrapper(
            EventTypeMetadata metadata,
            EventType underlying,
            IDictionary<string, object> properties)
        {
            var eventType = eventTypeFactory.CreateWrapper(
                metadata,
                underlying,
                properties,
                beanEventTypeFactory,
                eventTypeNameResolver);
            HandleRegister(eventType);
        }

        public void RegisterBean(
            EventTypeMetadata metadata,
            Type clazz,
            string startTimestampName,
            string endTimestampName,
            EventType[] superTypes,
            ISet<EventType> deepSuperTypes)
        {
            var stem = beanEventTypeStemService.GetCreateStem(clazz, null);
            var eventType = eventTypeFactory.CreateBeanType(
                stem,
                metadata,
                beanEventTypeFactory,
                superTypes,
                deepSuperTypes,
                startTimestampName,
                endTimestampName);
            HandleRegister(eventType);
        }

        public void RegisterXML(
            EventTypeMetadata metadata,
            string representsFragmentOfProperty,
            string representsOriginalTypeName)
        {
            var existing = xmlFragmentEventTypeFactory.GetTypeByName(metadata.Name);
            if (existing != null) {
                HandleRegister(existing);
                return;
            }

            var schemaType = xmlFragmentEventTypeFactory.GetRootTypeByName(representsOriginalTypeName);
            if (schemaType == null) {
                throw new EPException("Failed to find XML schema type '" + representsOriginalTypeName + "'");
            }

            var prop = PropertyParser.ParseAndWalkLaxToSimple(representsFragmentOfProperty);
            var schemaModelRoot = SchemaUtil.FindRootElement(
                schemaType.SchemaModel,
                schemaType.ConfigurationEventTypeXMLDOM.RootElementNamespace,
                schemaType.RootElementName);
            var item = prop.GetPropertyTypeSchema(schemaModelRoot);
            var complex = (SchemaElementComplex) item;
            var eventType = xmlFragmentEventTypeFactory.GetCreateXMLDOMType(
                representsOriginalTypeName,
                metadata.Name,
                metadata.ModuleName,
                complex,
                representsFragmentOfProperty);
            HandleRegister(eventType);
        }

        public void RegisterAvro(
            EventTypeMetadata metadata,
            string schemaJson)
        {
            EventType eventType = eventTypeAvroHandler.NewEventTypeFromJson(
                metadata,
                eventBeanTypedEventFactory,
                schemaJson,
                TODO,
                TODO);
            HandleRegister(eventType);
        }

        public void RegisterVariant(
            EventTypeMetadata metadata,
            EventType[] variants,
            bool any)
        {
            var spec = new VariantSpec(
                variants,
                any ? TypeVariance.ANY : TypeVariance.PREDEFINED);
            EventType eventType = eventTypeFactory.CreateVariant(metadata, spec);
            HandleRegister(eventType);
        }

        private void HandleRegister(EventType eventType)
        {
            if (moduleEventTypes.ContainsKey(eventType.Name)) {
                throw new IllegalStateException(
                    "Event type '" + eventType.Name + "' attempting to register multiple times");
            }

            moduleEventTypes.Put(eventType.Name, eventType);
        }
    }
} // end of namespace
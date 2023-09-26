///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.json.core;
using com.espertech.esper.common.@internal.@event.property;
using com.espertech.esper.common.@internal.@event.variant;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.path
{
    public class EventTypeCollectorImpl : EventTypeCollector
    {
        private readonly IDictionary<string, EventType> moduleEventTypes;
        private readonly BeanEventTypeFactory beanEventTypeFactory;
        private readonly TypeResolver typeResolver;
        private readonly EventTypeFactory eventTypeFactory;
        private readonly BeanEventTypeStemService beanEventTypeStemService;
        private readonly EventTypeNameResolver eventTypeNameResolver;
        private readonly XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory;
        private readonly EventTypeAvroHandler eventTypeAvroHandler;
        private readonly EventBeanTypedEventFactory eventBeanTypedEventFactory;
        private readonly IList<EventTypeCollectedSerde> serdes = new List<EventTypeCollectedSerde>();
        private readonly ImportService importService;
        private readonly EventTypeXMLXSDHandler eventTypeXMLXSDHandler;

        public EventTypeCollectorImpl(
            IDictionary<string, EventType> moduleEventTypes,
            BeanEventTypeFactory beanEventTypeFactory,
            TypeResolver typeResolver,
            EventTypeFactory eventTypeFactory,
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeNameResolver eventTypeNameResolver,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory,
            EventTypeAvroHandler eventTypeAvroHandler,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ImportService importService,
            EventTypeXMLXSDHandler eventTypeXMLXSDHandler)
        {
            this.moduleEventTypes = moduleEventTypes;
            this.beanEventTypeFactory = beanEventTypeFactory;
            this.typeResolver = typeResolver;
            this.eventTypeFactory = eventTypeFactory;
            this.beanEventTypeStemService = beanEventTypeStemService;
            this.eventTypeNameResolver = eventTypeNameResolver;
            this.xmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
            this.eventTypeAvroHandler = eventTypeAvroHandler;
            this.eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            this.importService = importService;
            this.eventTypeXMLXSDHandler = eventTypeXMLXSDHandler;
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

        public void RegisterJson(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName,
            JsonEventTypeDetail detail)
        {
            var eventType = eventTypeFactory.CreateJson(
                metadata,
                properties,
                superTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                beanEventTypeFactory,
                eventTypeNameResolver,
                detail);
            eventType.Initialize(typeResolver);
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
            var complex = (SchemaElementComplex)item;
            var eventType = xmlFragmentEventTypeFactory.GetCreateXMLDOMType(
                representsOriginalTypeName,
                metadata.Name,
                metadata.ModuleName,
                complex,
                representsFragmentOfProperty);
            HandleRegister(eventType);
        }

        public void RegisterXMLNewType(
            EventTypeMetadata metadata,
            ConfigurationCommonEventTypeXMLDOM config)
        {
            SchemaModel schemaModel = null;
            if (config.SchemaResource != null || config.SchemaText != null) {
                try {
                    schemaModel = eventTypeXMLXSDHandler.LoadAndMap(
                        config.SchemaResource,
                        config.SchemaText,
                        importService);
                }
                catch (Exception ex) {
                    throw new EPException(ex.Message, ex);
                }
            }

            var eventType = eventTypeFactory.CreateXMLType(
                metadata,
                config,
                schemaModel,
                null,
                metadata.Name,
                beanEventTypeFactory,
                xmlFragmentEventTypeFactory,
                eventTypeNameResolver,
                eventTypeXMLXSDHandler);
            HandleRegister(eventType);
            if (eventType is SchemaXMLEventType) {
                xmlFragmentEventTypeFactory.AddRootType((SchemaXMLEventType)eventType);
            }
        }

        public void RegisterAvro(
            EventTypeMetadata metadata,
            string schemaJson,
            string[] superTypes)
        {
            var st = EventTypeUtility.GetSuperTypesDepthFirst(
                superTypes,
                EventUnderlyingType.AVRO,
                eventTypeNameResolver);
            EventType eventType = eventTypeAvroHandler.NewEventTypeFromJson(
                metadata,
                eventBeanTypedEventFactory,
                schemaJson,
                st.First,
                st.Second);
            HandleRegister(eventType);
        }

        public void RegisterVariant(
            EventTypeMetadata metadata,
            EventType[] variants,
            bool any)
        {
            var spec = new VariantSpec(
                variants,
                any
                    ? TypeVariance.ANY
                    : TypeVariance.PREDEFINED);
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

        public void RegisterSerde(
            EventTypeMetadata metadata,
            DataInputOutputSerde underlyingSerde,
            Type underlyingClass)
        {
            serdes.Add(new EventTypeCollectedSerde(metadata, underlyingSerde, underlyingClass));
        }

        public IList<EventTypeCollectedSerde> Serdes => serdes;
    }
} // end of namespace
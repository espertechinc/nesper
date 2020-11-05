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
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.@event.path
{
    public class EventTypeCollectorImpl : EventTypeCollector
    {
        private readonly IContainer _container;
        private readonly IDictionary<string, EventType> _moduleEventTypes;
        private readonly BeanEventTypeFactory _beanEventTypeFactory;
        private readonly ClassLoader _classLoader;
        private readonly BeanEventTypeStemService _beanEventTypeStemService;
        private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
        private readonly EventTypeAvroHandler _eventTypeAvroHandler;
        private readonly EventTypeFactory _eventTypeFactory;
        private readonly EventTypeNameResolver _eventTypeNameResolver;
        private readonly XMLFragmentEventTypeFactory _xmlFragmentEventTypeFactory;
        private readonly ImportService _importService;
        private readonly IList<EventTypeCollectedSerde> _serdes = new List<EventTypeCollectedSerde>();
        
        public EventTypeCollectorImpl(
            IContainer container,
            IDictionary<string, EventType> moduleEventTypes,
            BeanEventTypeFactory beanEventTypeFactory,
            ClassLoader classLoader,
            EventTypeFactory eventTypeFactory,
            BeanEventTypeStemService beanEventTypeStemService,
            EventTypeNameResolver eventTypeNameResolver,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory,
            EventTypeAvroHandler eventTypeAvroHandler,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ImportService importService)
        {
            _container = container;
            _moduleEventTypes = moduleEventTypes;
            _beanEventTypeFactory = beanEventTypeFactory;
            _classLoader = classLoader;
            _eventTypeFactory = eventTypeFactory;
            _beanEventTypeStemService = beanEventTypeStemService;
            _eventTypeNameResolver = eventTypeNameResolver;
            _xmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
            _eventTypeAvroHandler = eventTypeAvroHandler;
            _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
            _importService = importService;
        }

        public void RegisterMap(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName)
        {
            var eventType = _eventTypeFactory.CreateMap(
                metadata,
                properties,
                superTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                _beanEventTypeFactory,
                _eventTypeNameResolver);
            HandleRegister(eventType);
        }

        public void RegisterObjectArray(
            EventTypeMetadata metadata,
            IDictionary<string, object> properties,
            string[] superTypes,
            string startTimestampPropertyName,
            string endTimestampPropertyName)
        {
            var eventType = _eventTypeFactory.CreateObjectArray(
                metadata,
                properties,
                superTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                _beanEventTypeFactory,
                _eventTypeNameResolver);
            HandleRegister(eventType);
        }

        public void RegisterWrapper(
            EventTypeMetadata metadata,
            EventType underlying,
            IDictionary<string, object> properties)
        {
            var eventType = _eventTypeFactory.CreateWrapper(
                metadata,
                underlying,
                properties,
                _beanEventTypeFactory,
                _eventTypeNameResolver);
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
            var stem = _beanEventTypeStemService.GetCreateStem(clazz, null);
            var eventType = _eventTypeFactory.CreateBeanType(
                stem,
                metadata,
                _beanEventTypeFactory,
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
            JsonEventType eventType = _eventTypeFactory.CreateJson(
                metadata,
                properties,
                superTypes,
                startTimestampPropertyName,
                endTimestampPropertyName,
                _beanEventTypeFactory,
                _eventTypeNameResolver,
                detail);
            eventType.Initialize(_classLoader);
            HandleRegister(eventType);
        }

        public void RegisterXML(
            EventTypeMetadata metadata,
            string representsFragmentOfProperty,
            string representsOriginalTypeName)
        {
            var existing = _xmlFragmentEventTypeFactory.GetTypeByName(metadata.Name);
            if (existing != null) {
                HandleRegister(existing);
                return;
            }

            var schemaType = _xmlFragmentEventTypeFactory.GetRootTypeByName(representsOriginalTypeName);
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
            var eventType = _xmlFragmentEventTypeFactory.GetCreateXMLDOMType(
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
            if ((config.SchemaResource != null) || (config.SchemaText != null)) {
                try {
                    schemaModel = XSDSchemaMapper.LoadAndMap(
                        config.SchemaResource,
                        config.SchemaText,
                        _container.ResourceManager());
                }
                catch (Exception ex) {
                    throw new EPException(ex.Message, ex);
                }
            }

            EventType eventType = _eventTypeFactory.CreateXMLType(
                metadata,
                config,
                schemaModel,
                null,
                metadata.Name,
                _beanEventTypeFactory,
                _xmlFragmentEventTypeFactory,
                _eventTypeNameResolver);
            HandleRegister(eventType);

            if (eventType is SchemaXMLEventType) {
                _xmlFragmentEventTypeFactory.AddRootType((SchemaXMLEventType) eventType);
            }
        }

        public void RegisterAvro(
            EventTypeMetadata metadata,
            string schemaJson,
            string[] superTypes)
        {
            var st = EventTypeUtility.GetSuperTypesDepthFirst(
                superTypes, EventUnderlyingType.AVRO, _eventTypeNameResolver);

            var eventType = _eventTypeAvroHandler.NewEventTypeFromJson(
                metadata,
                _eventBeanTypedEventFactory,
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
                any ? TypeVariance.ANY : TypeVariance.PREDEFINED);
            EventType eventType = _eventTypeFactory.CreateVariant(metadata, spec);
            HandleRegister(eventType);
        }

        private void HandleRegister(EventType eventType)
        {
            if (_moduleEventTypes.ContainsKey(eventType.Name)) {
                throw new IllegalStateException(
                    "Event type '" + eventType.Name + "' attempting to register multiple times");
            }

            _moduleEventTypes.Put(eventType.Name, eventType);
        }

        public void RegisterSerde(
            EventTypeMetadata metadata,
            DataInputOutputSerde underlyingSerde,
            Type underlyingClass)
        {
            _serdes.Add(new EventTypeCollectedSerde(metadata, underlyingSerde, underlyingClass));
        }

        public IList<EventTypeCollectedSerde> Serdes => _serdes;
    }
} // end of namespace
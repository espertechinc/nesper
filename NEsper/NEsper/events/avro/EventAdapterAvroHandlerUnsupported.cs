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
using com.espertech.esper.compat;
using com.espertech.esper.epl.core;
using com.espertech.esper.util;

namespace com.espertech.esper.events.avro
{
    public class EventAdapterAvroHandlerUnsupported : EventAdapterAvroHandler
    {
        public const string HANDLER_IMPL = "com.espertech.esper.avro.core.EventAdapterAvroHandlerImpl";

        public static readonly EventAdapterAvroHandlerUnsupported INSTANCE = new EventAdapterAvroHandlerUnsupported();

        public EventAdapterAvroHandlerUnsupported()
        {
        }

        public void Init(ConfigurationEngineDefaults.AvroSettings avroSettings, EngineImportService engineImportService)
        {
            // no action, init is always done
        }

        public AvroSchemaEventType NewEventTypeFromSchema(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            ConfigurationEventTypeAvro requiredConfig,
            EventType[] supertypes,
            ICollection<EventType> deepSupertypes)
        {
            throw GetUnsupported();
        }

        public EventBean AdapterForTypeAvro(Object avroGenericDataDotRecord, EventType existingType)
        {
            throw GetUnsupported();
        }

        public AvroSchemaEventType NewEventTypeFromNormalized(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            IDictionary<string, Object> properties,
            Attribute[] annotations,
            ConfigurationEventTypeAvro optionalConfig,
            EventType[] superTypes,
            ICollection<EventType> deepSuperTypes,
            string statementName,
            string engineURI)
        {
            throw GetUnsupported();
        }

        public EventBeanManufacturer GetEventBeanManufacturer(
            AvroSchemaEventType avroSchemaEventType,
            EventAdapterService eventAdapterService,
            IList<WriteablePropertyDescriptor> properties)
        {
            throw GetUnsupported();
        }

        public EventBeanFactory GetEventBeanFactory(EventType type, EventAdapterService eventAdapterService)
        {
            throw GetUnsupported();
        }

        public void ValidateExistingType(EventType existingType, AvroSchemaEventType proposedType)
        {
            throw GetUnsupported();
        }

        public SelectExprProcessorRepresentationFactory GetOutputFactory()
        {
            throw GetUnsupported();
        }

        public void AvroCompat(EventType existingType, IDictionary<string, Object> selPropertyTypes)
        {
            throw GetUnsupported();
        }

        public Object ConvertEvent(EventBean theEvent, AvroSchemaEventType targetType)
        {
            throw GetUnsupported();
        }

        public TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType eventType)
        {
            throw GetUnsupported();
        }

        private UnsupportedOperationException GetUnsupported()
        {
            throw new UnsupportedOperationException("Esper-Avro is not part of your classpath");
        }
    }
} // end of namespace

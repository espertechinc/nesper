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
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.@event.avro
{
    public class EventTypeAvroHandlerUnsupported : EventTypeAvroHandler
    {
        public static readonly EventTypeAvroHandlerUnsupported INSTANCE = new EventTypeAvroHandlerUnsupported();

        public void Init(
            ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettingsConfig,
            ImportService importService)
        {
            // no action, init is always done
        }

        public AvroSchemaEventType NewEventTypeFromSchema(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ConfigurationCommonEventTypeAvro requiredConfig,
            EventType[] supertypes,
            ISet<EventType> deepSupertypes)
        {
            throw new UnsupportedOperationException();
        }

        public EventBean AdapterForTypeAvro(
            object avroGenericDataDotRecord,
            EventType existingType)
        {
            throw new UnsupportedOperationException();
        }

        public AvroSchemaEventType NewEventTypeFromNormalized(
            EventTypeMetadata metadata,
            EventTypeNameResolver eventTypeNameResolver,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            IDictionary<string, object> properties,
            Attribute[] annotations,
            ConfigurationCommonEventTypeAvro optionalConfig,
            EventType[] superTypes,
            ISet<EventType> deepSuperTypes,
            string statementName)
        {
            throw new UnsupportedOperationException();
        }

        public EventBeanManufacturerForge GetEventBeanManufacturer(
            AvroSchemaEventType avroSchemaEventType,
            WriteablePropertyDescriptor[] properties)
        {
            throw new UnsupportedOperationException();
        }

        public EventBeanFactory GetEventBeanFactory(
            EventType type,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            throw new UnsupportedOperationException();
        }

        public void ValidateExistingType(
            EventType existingType,
            AvroSchemaEventType proposedType)
        {
            throw new UnsupportedOperationException();
        }

        public SelectExprProcessorRepresentationFactory OutputFactory => throw new UnsupportedOperationException();

        public void AvroCompat(
            EventType existingType,
            IDictionary<string, object> selPropertyTypes)
        {
            throw new UnsupportedOperationException();
        }

        public AvroSchemaEventType NewEventTypeFromJson(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            string schemaJson,
            IList<EventType> superTypes,
            ISet<EventType> deepSuperTypes)
        {
            throw new UnsupportedOperationException();
        }

        public object ConvertEvent(
            EventBean theEvent,
            AvroSchemaEventType targetType)
        {
            throw new UnsupportedOperationException();
        }

        public TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType eventType)
        {
            return null;
        }

        private UnsupportedOperationException GetUnsupported()
        {
            throw new UnsupportedOperationException(
                "Esper-Avro is not enabled in the configuration or is not part of your classpath");
        }
    }
} // end of namespace
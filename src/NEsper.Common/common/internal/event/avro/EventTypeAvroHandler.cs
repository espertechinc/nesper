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

namespace com.espertech.esper.common.@internal.@event.avro
{
    public interface EventTypeAvroHandler
    {
        SelectExprProcessorRepresentationFactory OutputFactory { get; }

        void Init(
            ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettingsConfig,
            ImportService importService);

        AvroSchemaEventType NewEventTypeFromSchema(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            ConfigurationCommonEventTypeAvro requiredConfig,
            EventType[] superTypes,
            ISet<EventType> deepSuperTypes);

        AvroSchemaEventType NewEventTypeFromNormalized(
            EventTypeMetadata metadata,
            EventTypeNameResolver eventTypeNameResolver,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            IDictionary<string, object> properties,
            Attribute[] annotations,
            ConfigurationCommonEventTypeAvro optionalConfig,
            EventType[] superTypes,
            ISet<EventType> deepSuperTypes,
            string statementName);

        AvroSchemaEventType NewEventTypeFromJson(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            string schemaJson,
            IList<EventType> superTypes,
            ISet<EventType> deepSuperTypes);

        EventBean AdapterForTypeAvro(
            object avroGenericDataDotRecord,
            EventType existingType);

        EventBeanManufacturerForge GetEventBeanManufacturer(
            AvroSchemaEventType avroSchemaEventType,
            WriteablePropertyDescriptor[] properties);

        EventBeanFactory GetEventBeanFactory(
            EventType type,
            EventBeanTypedEventFactory eventBeanTypedEventFactory);

        void ValidateExistingType(
            EventType existingType,
            AvroSchemaEventType proposedType);

        void AvroCompat(
            EventType existingType,
            IDictionary<string, object> selPropertyTypes);

        object ConvertEvent(
            EventBean theEvent,
            AvroSchemaEventType targetType);

        TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType eventType);
    }

    public class EventTypeAvroHandlerConstants
    {
        public const string RUNTIME_NONHA_HANDLER_IMPL = "NEsper.Avro.Core.EventTypeAvroHandlerImpl";
        public const string COMPILE_TIME_HANDLER_IMPL = RUNTIME_NONHA_HANDLER_IMPL;
    }
} // end of namespace
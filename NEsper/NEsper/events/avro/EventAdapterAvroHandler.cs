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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.events.avro
{
    public interface EventAdapterAvroHandler
    {
        void Init(ConfigurationEngineDefaults.AvroSettings avroSettings, EngineImportService engineImportService);

        SelectExprProcessorRepresentationFactory GetOutputFactory();

        AvroSchemaEventType NewEventTypeFromSchema(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            ConfigurationEventTypeAvro requiredConfig,
            EventType[] superTypes,
            ICollection<EventType> deepSuperTypes);

        AvroSchemaEventType NewEventTypeFromNormalized(
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
            string engineURI);

        EventBean AdapterForTypeAvro(Object avroGenericDataDotRecord, EventType existingType);

        EventBeanManufacturer GetEventBeanManufacturer(
            AvroSchemaEventType avroSchemaEventType,
            EventAdapterService eventAdapterService,
            IList<WriteablePropertyDescriptor> properties);

        EventBeanFactory GetEventBeanFactory(EventType type, EventAdapterService eventAdapterService);

        void ValidateExistingType(EventType existingType, AvroSchemaEventType proposedType);

        void AvroCompat(EventType existingType, IDictionary<string, Object> selPropertyTypes);

        Object ConvertEvent(EventBean theEvent, AvroSchemaEventType targetType);

        TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType eventType);
    }
} // end of namespace

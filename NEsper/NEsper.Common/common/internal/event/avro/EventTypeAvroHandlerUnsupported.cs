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
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.avro
{
	public class EventTypeAvroHandlerUnsupported : EventTypeAvroHandler {
	    public readonly static EventTypeAvroHandlerUnsupported INSTANCE = new EventTypeAvroHandlerUnsupported();

	    public EventTypeAvroHandlerUnsupported() {
	    }

	    public void Init(ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettingsConfig, ImportService importService) {
	        // no action, init is always done
	    }

	    public AvroSchemaEventType NewEventTypeFromSchema(EventTypeMetadata metadata, EventBeanTypedEventFactory eventBeanTypedEventFactory, ConfigurationCommonEventTypeAvro requiredConfig, EventType[] supertypes, ISet<EventType> deepSupertypes) {
	        throw Unsupported;
	    }

	    public EventBean AdapterForTypeAvro(object avroGenericDataDotRecord, EventType existingType) {
	        throw Unsupported;
	    }

	    public AvroSchemaEventType NewEventTypeFromNormalized(EventTypeMetadata metadata, EventTypeNameResolver eventTypeNameResolver, EventBeanTypedEventFactory eventBeanTypedEventFactory, IDictionary<string, object> properties, Attribute[] annotations, ConfigurationCommonEventTypeAvro optionalConfig, EventType[] superTypes, ISet<EventType> deepSuperTypes, string statementName) {
	        throw Unsupported;
	    }

	    public EventBeanManufacturerForge GetEventBeanManufacturer(AvroSchemaEventType avroSchemaEventType, WriteablePropertyDescriptor[] properties) {
	        throw Unsupported;
	    }

	    public EventBeanFactory GetEventBeanFactory(EventType type, EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        throw Unsupported;
	    }

	    public void ValidateExistingType(EventType existingType, AvroSchemaEventType proposedType) {
	        throw Unsupported;
	    }

	    public SelectExprProcessorRepresentationFactory OutputFactory {
	        get { throw Unsupported; }
	    }

	    public void AvroCompat(EventType existingType, IDictionary<string, object> selPropertyTypes) {
	        throw Unsupported;
	    }

	    public AvroSchemaEventType NewEventTypeFromJson(EventTypeMetadata metadata, EventBeanTypedEventFactory eventBeanTypedEventFactory, string schemaJson) {
	        throw Unsupported;
	    }

	    public object ConvertEvent(EventBean theEvent, AvroSchemaEventType targetType) {
	        throw Unsupported;
	    }

	    public TypeWidenerCustomizer GetTypeWidenerCustomizer(EventType eventType) {
	        return null;
	    }

	    private UnsupportedOperationException GetUnsupported() {
	        throw new UnsupportedOperationException("Esper-Avro is not enabled in the configuration or is not part of your classpath");
	    }
	}
} // end of namespace
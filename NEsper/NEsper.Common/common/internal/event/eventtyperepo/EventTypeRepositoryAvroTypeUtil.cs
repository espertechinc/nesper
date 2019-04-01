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
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
	public class EventTypeRepositoryAvroTypeUtil {
	    public static void BuildAvroTypes(EventTypeRepositoryImpl eventTypeRepositoryPreconfigured, IDictionary<string, ConfigurationCommonEventTypeAvro> eventTypesAvro, EventTypeAvroHandler eventTypeAvroHandler, EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        foreach (KeyValuePair<string, ConfigurationCommonEventTypeAvro> entry in eventTypesAvro) {
	            BuildAvroType(eventTypeRepositoryPreconfigured, entry.Key, entry.Value, eventTypeAvroHandler, eventBeanTypedEventFactory);
	        }
	    }

	    private static void BuildAvroType(EventTypeRepositoryImpl eventTypeRepositoryPreconfigured, string eventTypeName, ConfigurationCommonEventTypeAvro config, EventTypeAvroHandler eventTypeAvroHandler, EventBeanTypedEventFactory eventBeanTypedEventFactory) {
	        EventTypeMetadata metadata = new EventTypeMetadata(eventTypeName, null, EventTypeTypeClass.APPLICATION, EventTypeApplicationType.AVRO, NameAccessModifier.PRECONFIGURED, EventTypeBusModifier.NONBUS, false, new EventTypeIdPair(CRC32Util.ComputeCRC32(eventTypeName), -1));
	        Pair<EventType[], ISet<EventType>> avroSuperTypes = EventTypeUtility.GetSuperTypesDepthFirst(config.SuperTypes, EventUnderlyingType.AVRO, eventTypeRepositoryPreconfigured);
	        AvroSchemaEventType newEventType = eventTypeAvroHandler.NewEventTypeFromSchema(metadata, eventBeanTypedEventFactory, config, avroSuperTypes.First, avroSuperTypes.Second);
	        eventTypeRepositoryPreconfigured.AddType(newEventType);
	    }
	}
} // end of namespace
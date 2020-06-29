///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

namespace NEsper.Avro.Core
{
    public class EventTypeAvroHandlerImpl : EventTypeAvroHandlerBase
    {
        protected override AvroSchemaEventType MakeType(
            EventTypeMetadata metadata,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            Schema schema,
            ConfigurationCommonEventTypeAvro optionalConfig,
            IList<EventType> supertypes,
            ISet<EventType> deepSupertypes)
        {
            return new AvroEventType(
                metadata,
                schema,
                optionalConfig?.StartTimestampPropertyName,
                optionalConfig?.EndTimestampPropertyName,
                supertypes,
                deepSupertypes,
                eventBeanTypedEventFactory,
                this);
        }

        public override EventBean AdapterForTypeAvro(
            object avroGenericDataDotRecord,
            EventType existingType)
        {
            if (!(avroGenericDataDotRecord is GenericRecord)) {
                throw new EPException(
                    "Unexpected event object type '" +
                    (avroGenericDataDotRecord == null ? "null" : avroGenericDataDotRecord.GetType().CleanName()) +
                    "' encountered, please supply a GenericRecord");
            }

            var record = (GenericRecord) avroGenericDataDotRecord;
            return new AvroGenericDataEventBean(record, existingType);
        }
    }
} // end of namespace
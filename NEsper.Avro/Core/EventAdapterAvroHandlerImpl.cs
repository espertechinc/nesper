///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;

using com.espertech.esper.client;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    public class EventAdapterAvroHandlerImpl : EventAdapterAvroHandlerBase
    {
        protected override AvroSchemaEventType MakeType(
            EventTypeMetadata metadata,
            string eventTypeName,
            int typeId,
            EventAdapterService eventAdapterService,
            RecordSchema schema,
            ConfigurationEventTypeAvro optionalConfig,
            EventType[] supertypes,
            ICollection<EventType> deepSupertypes)
        {
            return new AvroEventType(
                metadata, eventTypeName, typeId, eventAdapterService, schema,
                optionalConfig == null ? null : optionalConfig.StartTimestampPropertyName,
                optionalConfig == null ? null : optionalConfig.EndTimestampPropertyName, supertypes, deepSupertypes);
        }
    }
} // end of namespace

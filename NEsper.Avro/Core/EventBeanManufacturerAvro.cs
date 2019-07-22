///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    /// <summary>
    ///     Factory for ObjectArray-underlying events.
    /// </summary>
    public class EventBeanManufacturerAvro : EventBeanManufacturer
    {
        private readonly EventBeanTypedEventFactory _eventAdapterService;
        private readonly AvroEventType _eventType;
        private readonly Field[] _indexPerWritable;
        private readonly Schema _schema;

        public EventBeanManufacturerAvro(
            AvroSchemaEventType eventType,
            EventBeanTypedEventFactory eventAdapterService,
            Field[] indexPerWritable)
        {
            _eventAdapterService = eventAdapterService;
            _eventType = (AvroEventType) eventType;
            _schema = _eventType.SchemaAvro;
            _indexPerWritable = indexPerWritable;
        }

        public EventBean Make(object[] properties)
        {
            var record = MakeUnderlying(properties);
            return _eventAdapterService.AdapterForTypedAvro(record, _eventType);
        }

        public object MakeUnderlying(object[] properties)
        {
            var record = new GenericRecord(_schema.AsRecordSchema());
            for (var i = 0; i < properties.Length; i++) {
                var indexToWrite = _indexPerWritable[i];
                record.Put(indexToWrite, properties[i]);
            }

            return record;
        }
    }
} // end of namespace
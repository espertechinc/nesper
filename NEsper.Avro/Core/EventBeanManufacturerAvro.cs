///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.events;
using com.espertech.esper.events.avro;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    /// <summary>Factory for ObjectArray-underlying events.</summary>
    public class EventBeanManufacturerAvro : EventBeanManufacturer
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly AvroEventType _eventType;
        private readonly Field[] _indexPerWritable;
        private readonly RecordSchema _schema;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="eventType">type to create</param>
        /// <param name="eventAdapterService">event factory</param>
        /// <param name="properties">written properties</param>
        public EventBeanManufacturerAvro(
            AvroSchemaEventType eventType,
            EventAdapterService eventAdapterService,
            IList<WriteablePropertyDescriptor> properties)
        {
            _eventAdapterService = eventAdapterService;
            _eventType = (AvroEventType) eventType;
            _schema = _eventType.SchemaAvro;

            _indexPerWritable = new Field[properties.Count];
            for (int i = 0; i < properties.Count; i++)
            {
                string propertyName = properties[i].PropertyName;

                var field = _schema.GetField(propertyName);
                if (field == null)
                {
                    throw new IllegalStateException(
                        "Failed to find property '" + propertyName + "' among the array indexes");
                }
                _indexPerWritable[i] = field;
            }
        }

        public EventBean Make(Object[] properties)
        {
            Object record = MakeUnderlying(properties);
            return _eventAdapterService.AdapterForTypedAvro(record, _eventType);
        }

        public Object MakeUnderlying(Object[] properties)
        {
            var record = new GenericRecord(_schema);
            for (int i = 0; i < properties.Length; i++)
            {
                var indexToWrite = _indexPerWritable[i];
                record.Put(indexToWrite, properties[i]);
            }
            return record;
        }
    }
} // end of namespace
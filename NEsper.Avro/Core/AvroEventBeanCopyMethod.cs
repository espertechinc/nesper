///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    /// <summary>Copy method for Map-underlying events.</summary>
    public class AvroEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly AvroEventType _avroEventType;
        private readonly EventAdapterService _eventAdapterService;

        public AvroEventBeanCopyMethod(AvroEventType avroEventType, EventAdapterService eventAdapterService)
        {
            _avroEventType = avroEventType;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var original = (GenericRecord) theEvent.Underlying;
            var copy = new GenericRecord(_avroEventType.SchemaAvro.AsRecordSchema());
            var fields = _avroEventType.SchemaAvro.GetFields();
            foreach (Field field in fields)
            {
                if (field.Schema.Tag == Schema.Type.Array)
                {
                    var originalColl = original.Get(field).UnwrapEnumerable<object>(true);
                    if (originalColl != null)
                    {
                        copy.Put(field, new List<object>(originalColl));
                    }
                }
                else if (field.Schema.Tag == Schema.Type.Map)
                {
                    var originalMap = original.Get(field).UnwrapStringDictionary();
                    if (originalMap != null)
                    {
                        copy.Put(field, new Dictionary<string, object>(originalMap));
                    }
                }
                else
                {
                    object originalValue;
                    if (original.TryGetValue(field.Name, out originalValue))
                    {
                        copy.Add(field.Name, originalValue);
                    }
                }
            }
            return _eventAdapterService.AdapterForTypedAvro(copy, _avroEventType);
        }
    }
} // end of namespace
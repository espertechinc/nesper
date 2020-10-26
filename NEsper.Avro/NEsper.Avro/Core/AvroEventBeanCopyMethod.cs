///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using Avro;
using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Core
{
    /// <summary>
    ///     Copy method for Map-underlying events.
    /// </summary>
    public class AvroEventBeanCopyMethod : EventBeanCopyMethod
    {
        private readonly AvroEventType _avroEventType;
        private readonly EventBeanTypedEventFactory _eventAdapterService;

        public AvroEventBeanCopyMethod(
            AvroEventType avroEventType,
            EventBeanTypedEventFactory eventAdapterService)
        {
            _avroEventType = avroEventType;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Copy(EventBean theEvent)
        {
            var original = (GenericRecord) theEvent.Underlying;
            var copy = new GenericRecord(_avroEventType.SchemaAvro.AsRecordSchema());
            var fields = _avroEventType.SchemaAvro.GetFields();
            foreach (var field in fields) {
                if (field.Schema.Tag == Schema.Type.Array) {
                    var originalValue = original.Get(field);
                    if (originalValue == null) {
                    } else if (originalValue is Array originalArray) {
                        var copyArray = Arrays.CreateInstanceChecked(
                            originalArray.GetType().GetElementType(),
                            originalArray.Length);
                        Array.Copy(originalArray, 0, copyArray, 0, copyArray.Length);
                        copy.Put(field, copyArray);
                    } else if (originalValue.GetType().IsGenericCollection()) {
                        var elementType = originalValue.GetType().GetCollectionItemType();
                        var copyList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                        throw new NotImplementedException();
                    }
                }
                else if (field.Schema.Tag == Schema.Type.Map) {
                    var originalMap = (IDictionary<string, object>) original.Get(field);
                    if (originalMap != null) {
                        copy.Put(field, new Dictionary<string, object>(originalMap));
                    }
                }
                else {
                    copy.Put(field, original.Get(field));
                }
            }

            return _eventAdapterService.AdapterForTypedAvro(copy, _avroEventType);
        }
    }
} // end of namespace
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
using com.espertech.esper.events;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterSimple : AvroEventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentType;
        private readonly Field _propertyIndex;

        public AvroEventBeanGetterSimple(
            Field propertyIndex,
            EventType fragmentType,
            EventAdapterService eventAdapterService)
        {
            _propertyIndex = propertyIndex;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            return record.Get(_propertyIndex);
        }

        public Object Get(EventBean theEvent)
        {
            return GetAvroFieldValue((GenericRecord) theEvent.Underlying);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true; // Property exists as the property is not dynamic (unchecked)
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public Object GetFragment(EventBean obj)
        {
            Object value = Get(obj);
            return GetFragmentInternal(value);
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            Object value = GetAvroFieldValue(record);
            return GetFragmentInternal(value);
        }

        private Object GetFragmentInternal(Object value)
        {
            if (_fragmentType == null)
            {
                return null;
            }
            if (value is GenericRecord)
            {
                return _eventAdapterService.AdapterForTypedAvro(value, _fragmentType);
            }
            if (value is ICollection<object>)
            {
                var coll = (ICollection<object>) value;
                var events = new EventBean[coll.Count];
                int index = 0;
                foreach (Object item in coll)
                {
                    events[index++] = _eventAdapterService.AdapterForTypedAvro(item, _fragmentType);
                }
                return events;
            }
            return null;
        }
    }
} // end of namespace
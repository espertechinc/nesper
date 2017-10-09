///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.events;

using NEsper.Avro.Core;
using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterIndexed : AvroEventPropertyGetter
    {
        private readonly Field _pos;
        private readonly int _index;
        private readonly EventType _fragmentEventType;
        private readonly EventAdapterService _eventAdapterService;

        public AvroEventBeanGetterIndexed(
            Field pos,
            int index,
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            _pos = pos;
            _index = index;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var values = record.Get(_pos).Unwrap<object>(true);
            return GetIndexedValue(values, _index);
        }

        public Object GetAvroFieldValue(GenericRecord record)
        {
            var values = record.Get(_pos).Unwrap<object>(true);
            return GetIndexedValue(values, _index);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public bool IsExistsPropertyAvro(GenericRecord record)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            return GetAvroFragment(record);
        }

        public Object GetAvroFragment(GenericRecord record)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }
            var value = GetAvroFieldValue(record);
            if (value == null)
            {
                return null;
            }
            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentEventType);
        }

        internal static Object GetIndexedValue(Array values, int index)
        {
            if (values == null)
            {
                return null;
            }

            return values.Length > index ? values.GetValue(index) : null;
        }

        internal static Object GetIndexedValue(IEnumerable<object> values, int index)
        {
            if (values == null)
            {
                return null;
            }
            if (values is IList<object>)
            {
                var list = (IList<object>)values;
                return list.Count > index ? list[index] : null;
            }

            return values.Skip(index).FirstOrDefault(null);
            //return values.ToArray()[index];
        }
    }
} // end of namespace

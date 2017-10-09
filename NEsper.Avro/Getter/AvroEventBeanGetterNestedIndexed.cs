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

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedIndexed : EventPropertyGetter
    {
        private readonly Field _top;
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentEventType;
        private readonly int _index;
        private readonly Field _pos;

        public AvroEventBeanGetterNestedIndexed(
            Field top,
            Field pos,
            int index,
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            _top = top;
            _pos = pos;
            _index = index;
            _fragmentEventType = fragmentEventType;
            _eventAdapterService = eventAdapterService;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_top);
            if (inner == null)
            {
                return null;
            }
            var collection = (ICollection<object>) inner.Get(_pos);
            return AvroEventBeanGetterIndexed.GetIndexedValue(collection, _index);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (_fragmentEventType == null)
            {
                return null;
            }
            Object value = Get(eventBean);
            if (value == null)
            {
                return null;
            }
            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentEventType);
        }
    }
} // end of namespace
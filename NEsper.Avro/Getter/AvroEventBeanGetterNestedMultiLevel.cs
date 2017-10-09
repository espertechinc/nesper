///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro;
using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events;

using NEsper.Avro.Extensions;

namespace NEsper.Avro.Getter
{
    public class AvroEventBeanGetterNestedMultiLevel : EventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentEventType;
        private readonly Field[] _path;
        private readonly Field _top;

        public AvroEventBeanGetterNestedMultiLevel(
            Field top,
            Field[] path,
            EventType fragmentEventType,
            EventAdapterService eventAdapterService)
        {
            _top = top;
            _path = path;
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
            for (int i = 0; i < _path.Length - 1; i++)
            {
                inner = (GenericRecord) inner.Get(_path[i]);
                if (inner == null)
                {
                    return null;
                }
            }
            return inner.Get(_path[_path.Length - 1]);
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
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
    public class AvroEventBeanGetterNestedSimple : EventPropertyGetter
    {
        private readonly EventAdapterService _eventAdapterService;
        private readonly EventType _fragmentType;
        private readonly Field _posInner;
        private readonly Field _posTop;

        public AvroEventBeanGetterNestedSimple(
            Field posTop,
            Field posInner,
            EventType fragmentType,
            EventAdapterService eventAdapterService)
        {
            _posTop = posTop;
            _posInner = posInner;
            _fragmentType = fragmentType;
            _eventAdapterService = eventAdapterService;
        }

        public Object Get(EventBean eventBean)
        {
            var record = (GenericRecord) eventBean.Underlying;
            var inner = (GenericRecord) record.Get(_posTop);
            if (inner == null)
            {
                return null;
            }
            return inner.Get(_posInner);
        }

        public bool IsExistsProperty(EventBean eventBean)
        {
            return true;
        }

        public Object GetFragment(EventBean eventBean)
        {
            if (_fragmentType == null)
            {
                return null;
            }
            Object value = Get(eventBean);
            if (value == null)
            {
                return null;
            }
            return _eventAdapterService.AdapterForTypedAvro(value, _fragmentType);
        }
    }
} // end of namespace
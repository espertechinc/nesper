///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace NEsper.Avro.Core
{
    public class EventBeanFactoryAvro : EventBeanFactory
    {
        private readonly EventType _type;
        private readonly EventBeanTypedEventFactory _eventAdapterService;

        public EventBeanFactoryAvro(
            EventType type,
            EventBeanTypedEventFactory eventAdapterService)
        {
            _type = type;
            _eventAdapterService = eventAdapterService;
        }

        public EventBean Wrap(object underlying)
        {
            return _eventAdapterService.AdapterForTypedAvro(underlying, _type);
        }

        public Type UnderlyingType {
            get => typeof(GenericRecord[]);
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Avro.Generic;

using com.espertech.esper.client;
using com.espertech.esper.events;

namespace NEsper.Avro.Core
{
    public class EventBeanFactoryAvro : EventBeanFactory
    {
        private readonly EventType _type;
        private readonly EventAdapterService _eventAdapterService;
    
        public EventBeanFactoryAvro(EventType type, EventAdapterService eventAdapterService) {
            this._type = type;
            this._eventAdapterService = eventAdapterService;
        }
    
        public EventBean Wrap(Object underlying) {
            return _eventAdapterService.AdapterForTypedAvro(underlying, _type);
        }
    
        public Type GetUnderlyingType()
        {
            return typeof (GenericRecord[]);
        }
    }
} // end of namespace

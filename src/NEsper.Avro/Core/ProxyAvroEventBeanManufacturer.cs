///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using Avro.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace NEsper.Avro.Core
{
    public class ProxyAvroEventBeanManufacturer : ProxyEventBeanManufacturer
    {
        public EventBeanTypedEventFactory EventFactory { get; set; }
        public EventType EventType { get; set; }

        public ProxyAvroEventBeanManufacturer(
            EventType eventType,
            EventBeanTypedEventFactory eventFactory,
            MakeUnderlyingFunc procMakeUnderlying)
        {
            EventFactory = eventFactory;
            EventType = eventType;
            ProcMakeUnderlying = procMakeUnderlying;
        }

        public override EventBean Make(object[] properties)
        {
            var und = (GenericRecord) MakeUnderlying(properties);
            return EventFactory.AdapterForTypedAvro(und, EventType);
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class ProxyObjectArrayEventBeanManufacturer : ProxyEventBeanManufacturer
    {
        public EventBeanTypedEventFactory EventFactory { get; set; }
        public EventType EventType { get; set; }

        public ProxyObjectArrayEventBeanManufacturer(
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
            var und = (object[])MakeUnderlying(properties);
            return EventFactory.AdapterForTypedObjectArray(und, EventType);
        }
    }
}
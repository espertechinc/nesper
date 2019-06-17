///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;

namespace com.espertech.esper.common.@internal.@event.core
{
    public interface EventTypeIdResolver
    {
        EventType GetTypeById(
            long eventTypeIdPublic,
            long eventTypeIdProtected);
    }

    public class ProxyEventTypeIdResolver : EventTypeIdResolver
    {
        public Func<long, long, EventType> ProcGetTypeById { get; set; }

        public EventType GetTypeById(
            long eventTypeIdPublic,
            long eventTypeIdProtected)
        {
            return ProcGetTypeById?.Invoke(eventTypeIdPublic, eventTypeIdProtected);
        }
    }
} // end of namespace
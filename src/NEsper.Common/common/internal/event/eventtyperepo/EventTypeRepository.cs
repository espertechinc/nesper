///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;

namespace com.espertech.esper.common.@internal.@event.eventtyperepo
{
    public interface EventTypeRepository : EventTypeNameResolver
    {
        ICollection<EventType> AllTypes { get; }

        IDictionary<string, EventType> NameToTypeMap { get; }

        EventType GetTypeById(long eventTypeIdPublic);

        void AddType(EventType eventType);
    }
} // end of namespace
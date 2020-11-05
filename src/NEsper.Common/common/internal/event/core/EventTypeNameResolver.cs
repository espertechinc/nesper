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
    public interface EventTypeNameResolver
    {
        EventType GetTypeByName(string typeName);
    }

    public class ProxyEventTypeNameResolver : EventTypeNameResolver
    {
        public Func<string, EventType> ProcGetTypeByName { get; set; }

        public ProxyEventTypeNameResolver()
        {
        }

        public ProxyEventTypeNameResolver(Func<string, EventType> procGetTypeByName)
        {
            ProcGetTypeByName = procGetTypeByName;
        }

        public EventType GetTypeByName(string typeName)
        {
            return ProcGetTypeByName(typeName);
        }
    }
} // end of namespace
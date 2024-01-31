///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
    public class SerdeEventTypeCompileTimeRegistryImpl : SerdeEventTypeCompileTimeRegistry
    {
        public SerdeEventTypeCompileTimeRegistryImpl(bool isTargetHa)
        {
            IsTargetHA = isTargetHa;
            if (isTargetHa) {
                EventTypes = new Dictionary<EventType, DataInputOutputSerdeForge>();
            }
            else {
                EventTypes = EmptyDictionary<EventType, DataInputOutputSerdeForge>.Instance;
            }
        }

        public bool IsTargetHA { get; }

        public void AddSerdeFor(
            EventType eventType,
            DataInputOutputSerdeForge forge)
        {
            if (IsTargetHA) {
                EventTypes.Put(eventType, forge);
            }
        }

        public IDictionary<EventType, DataInputOutputSerdeForge> EventTypes { get; }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.serde.compiletime.resolve;

namespace com.espertech.esper.common.@internal.serde.compiletime.eventtype
{
    public interface SerdeEventTypeCompileTimeRegistry
    {
        bool IsTargetHA { get; }
        IDictionary<EventType, DataInputOutputSerdeForge> EventTypes { get; }

        void AddSerdeFor(
            EventType eventType,
            DataInputOutputSerdeForge forge);
    }
} // end of namespace
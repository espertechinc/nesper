///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage3;

namespace com.espertech.esper.common.@internal.@event.core
{
    public class EventTypeForgeablesPair
    {
        public EventTypeForgeablesPair(
            EventType eventType,
            IList<StmtClassForgeableFactory> additionalForgeables)
        {
            EventType = eventType;
            AdditionalForgeables = additionalForgeables;
        }

        public EventType EventType { get; }
        public IList<StmtClassForgeableFactory> AdditionalForgeables { get; }
    }
}
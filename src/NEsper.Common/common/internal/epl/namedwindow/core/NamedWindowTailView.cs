///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;

namespace com.espertech.esper.common.@internal.epl.namedwindow.core
{
    public interface NamedWindowTailView
    {
        bool IsPrioritized { get; }

        bool IsParentBatchWindow { get; }

        StatementResultService StatementResultService { get; }

        EventType EventType { get; }
        NamedWindowConsumerLatchFactory MakeLatchFactory();

        void AddDispatches(
            NamedWindowConsumerLatchFactory latchFactory,
            IDictionary<EPStatementAgentInstanceHandle, IList<NamedWindowConsumerView>> consumersInContext,
            NamedWindowDeltaData delta,
            AgentInstanceContext agentInstanceContext);

        NamedWindowConsumerView AddConsumerNoContext(NamedWindowConsumerDesc consumerDesc);

        void RemoveConsumerNoContext(NamedWindowConsumerView namedWindowConsumerView);
    }
} // end of namespace
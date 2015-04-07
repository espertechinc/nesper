///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.context;

namespace com.espertech.esper.core.context.mgr
{
    public interface ContextController
    {
        int PathId { get; }
        void Activate(EventBean optionalTriggeringEvent, IDictionary<String, Object> optionalTriggeringPattern, ContextControllerState states, ContextInternalFilterAddendum filterAddendum, int? importPathId);
        ContextControllerFactory Factory { get; }
        void Deactivate();
        void VisitSelectedPartitions(ContextPartitionSelector contextPartitionSelector, ContextPartitionVisitor visitor);
        void ImportContextPartitions(ContextControllerState state, int pathIdToUse, ContextInternalFilterAddendum filterAddendum, AgentInstanceSelector agentInstanceSelector);
        void DeletePath(ContextPartitionIdentifier identifier);
    }
}

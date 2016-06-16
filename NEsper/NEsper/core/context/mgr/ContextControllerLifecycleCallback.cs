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
    public interface ContextControllerLifecycleCallback
    {
        ContextControllerInstanceHandle ContextPartitionInstantiate(
            int? optionalContextPartitionId,
            int subpath,
            int? importSubpathId,
            ContextController originator,
            EventBean optionalTriggeringEvent,
            IDictionary<String, Object> optionalTriggeringPattern,
            Object partitionKey,
            IDictionary<String, Object> contextProperties,
            ContextControllerState states,
            ContextInternalFilterAddendum filterAddendum,
            bool isRecoveringResilient,
            ContextPartitionState state);

        void ContextPartitionNavigate(
            ContextControllerInstanceHandle existingHandle,
            ContextController originator,
            ContextControllerState controllerState,
            int exportedCPOrPathId,
            ContextInternalFilterAddendum filterAddendum,
            AgentInstanceSelector agentInstanceSelector,
            byte[] payload,
            bool isRecoveringResilient);

        void ContextPartitionTerminate(
            ContextControllerInstanceHandle contextNestedHandle,
            IDictionary<String, Object> terminationProperties,
            bool leaveLocksAcquired,
            IList<AgentInstance> agentInstances);
    }
}

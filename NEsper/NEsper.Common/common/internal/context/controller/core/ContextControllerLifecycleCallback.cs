///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.controller.core
{
    public interface ContextControllerLifecycleCallback
    {
        ContextPartitionInstantiationResult ContextPartitionInstantiate(
            IntSeqKey controllerPathId,
            int subpathId,
            ContextController originator,
            EventBean optionalTriggeringEvent,
            IDictionary<string, object> optionalTriggeringPattern,
            object[] parentPartitionKeys,
            object partitionKey);

        void ContextPartitionTerminate(
            IntSeqKey controllerPath,
            int subpathIdOrCPId,
            ContextController originator,
            IDictionary<string, object> terminationProperties,
            bool leaveLocksAcquired,
            IList<AgentInstance> agentInstancesLocksHeld);

        void ContextPartitionRecursiveVisit(
            IntSeqKey controllerPath,
            int subpathId,
            ContextController originator,
            ContextPartitionVisitor visitor,
            ContextPartitionSelector[] selectorPerLevel);
    }
} // end of namespace
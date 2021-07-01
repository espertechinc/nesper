///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.mgr
{
    public class ContextPartitionInstantiationResult
    {
        public ContextPartitionInstantiationResult(
            int subpathOrCPId,
            IList<AgentInstance> agentInstances)
        {
            SubpathOrCPId = subpathOrCPId;
            AgentInstances = agentInstances;
        }

        public int SubpathOrCPId { get; }

        public IList<AgentInstance> AgentInstances { get; }
    }
} // end of namespace
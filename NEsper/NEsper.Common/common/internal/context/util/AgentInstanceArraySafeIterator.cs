///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.common.@internal.context.util
{
    public class AgentInstanceArraySafeIterator
    {
        public static IEnumerator<EventBean> Create(AgentInstance[] instances)
        {
            try {
                foreach (AgentInstance instance in instances) {
                    var instanceLock = instance
                        .AgentInstanceContext
                        .EpStatementAgentInstanceHandle
                        .StatementAgentInstanceLock;
                    instanceLock.AcquireWriteLock();
                }

                foreach (AgentInstance instance in instances) {
                    foreach (EventBean eventBean in instance.FinalView) {
                        yield return eventBean;
                    }
                }
            }
            finally {
                foreach (AgentInstance instance in instances) {
                    var agentInstanceContext = instance.AgentInstanceContext;
                    if (agentInstanceContext.StatementContext.EpStatementHandle.HasTableAccess) {
                        agentInstanceContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
                    }
                }
            }
        }
    }
} // end of namespace
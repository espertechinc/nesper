///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client.annotation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.util;

namespace com.espertech.esper.core.context.factory
{
    public abstract class StatementAgentInstanceFactoryBase : StatementAgentInstanceFactory
    {
        private readonly bool _audit;

        protected StatementAgentInstanceFactoryBase(Attribute[] annotations)
        {
            _audit = AuditEnum.CONTEXTPARTITION.GetAudit(annotations) != null;
        }
    
        protected abstract StatementAgentInstanceFactoryResult NewContextInternal(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient);
    
        public StatementAgentInstanceFactoryResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
        {
            if (!_audit || agentInstanceContext.AgentInstanceId == -1) {
                return NewContextInternal(agentInstanceContext, isRecoveringResilient);
            }
    
            AuditPath.AuditContextPartition(agentInstanceContext.EngineURI, agentInstanceContext.StatementName, true, agentInstanceContext.AgentInstanceId);
            var result = NewContextInternal(agentInstanceContext, isRecoveringResilient);
            var stopCallback = result.StopCallback;
            result.StopCallback = () =>
            {
                AuditPath.AuditContextPartition(agentInstanceContext.EngineURI, agentInstanceContext.StatementName, false, agentInstanceContext.AgentInstanceId);
                stopCallback.Invoke();
            };
            return result;
        }
    }
}

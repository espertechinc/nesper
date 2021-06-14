///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        public class NamedWindowConsumerPreloadDispatchNonJoin : StatementAgentInstancePreload
        {
            private readonly AgentInstanceContext _agentInstanceContext;

            public NamedWindowConsumerPreloadDispatchNonJoin(AgentInstanceContext agentInstanceContext)
            {
                this._agentInstanceContext = agentInstanceContext;
            }

            public void ExecutePreload()
            {
                _agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable?.Execute();
            }
        }
    }
}
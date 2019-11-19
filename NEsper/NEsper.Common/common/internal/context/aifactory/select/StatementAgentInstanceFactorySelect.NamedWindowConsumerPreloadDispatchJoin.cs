///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.@join.@base;

namespace com.espertech.esper.common.@internal.context.aifactory.@select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class NamedWindowConsumerPreloadDispatchJoin : StatementAgentInstancePreload
        {
            private readonly AgentInstanceContext agentInstanceContext;
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly int stream;

            public NamedWindowConsumerPreloadDispatchJoin(
                JoinPreloadMethod joinPreloadMethod,
                int stream,
                AgentInstanceContext agentInstanceContext)
            {
                this.joinPreloadMethod = joinPreloadMethod;
                this.stream = stream;
                this.agentInstanceContext = agentInstanceContext;
            }

            public void ExecutePreload()
            {
                joinPreloadMethod.PreloadFromBuffer(stream, agentInstanceContext);
            }
        }
    }
}
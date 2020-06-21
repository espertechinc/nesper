///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.@base;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        public class NamedWindowConsumerPreloadDispatchJoin : StatementAgentInstancePreload
        {
            private readonly JoinPreloadMethod _joinPreloadMethod;
            private readonly int _stream;
            private readonly AgentInstanceContext _agentInstanceContext;

            public NamedWindowConsumerPreloadDispatchJoin(
                JoinPreloadMethod joinPreloadMethod,
                int stream,
                AgentInstanceContext agentInstanceContext)
            {
                this._joinPreloadMethod = joinPreloadMethod;
                this._stream = stream;
                this._agentInstanceContext = agentInstanceContext;
            }

            public void ExecutePreload()
            {
                _joinPreloadMethod.PreloadFromBuffer(_stream, _agentInstanceContext);
            }
        }
    }
}
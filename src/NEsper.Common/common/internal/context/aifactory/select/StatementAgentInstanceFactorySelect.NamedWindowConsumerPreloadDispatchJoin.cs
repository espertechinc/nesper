using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.@base;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class NamedWindowConsumerPreloadDispatchJoin : StatementAgentInstancePreload
        {
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly int stream;
            private readonly AgentInstanceContext agentInstanceContext;

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
using com.espertech.esper.common.@internal.context.util;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class NamedWindowConsumerPreloadDispatchNonJoin : StatementAgentInstancePreload
        {
            private readonly AgentInstanceContext agentInstanceContext;

            public NamedWindowConsumerPreloadDispatchNonJoin(AgentInstanceContext agentInstanceContext)
            {
                this.agentInstanceContext = agentInstanceContext;
            }

            public void ExecutePreload()
            {
                if (agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable != null) {
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                }
            }
        }
    }
}
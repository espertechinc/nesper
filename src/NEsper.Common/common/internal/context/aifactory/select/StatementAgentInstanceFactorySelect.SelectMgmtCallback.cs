using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class SelectMgmtCallback : AgentInstanceMgmtCallback
        {
            private readonly ResultSetProcessor resultSetProcessor;
            private readonly AggregationService aggregationService;

            public SelectMgmtCallback(compat.collections.Pair<ResultSetProcessor, AggregationService> processorPair)
            {
                resultSetProcessor = processorPair.First;
                aggregationService = processorPair.Second;
            }

            public void Stop(AgentInstanceStopServices services)
            {
                resultSetProcessor.Stop();
                aggregationService.Stop();
            }

            public void Transfer(AgentInstanceTransferServices services)
            {
                // no action
            }
        }
    }
}
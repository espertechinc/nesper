using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.resultset.core;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class NamedWindowConsumerPreloadAggregationJoin : StatementAgentInstancePreload
        {
            private readonly JoinPreloadMethod joinPreloadMethod;
            private readonly ResultSetProcessor resultSetProcessor;

            public NamedWindowConsumerPreloadAggregationJoin(
                JoinPreloadMethod joinPreloadMethod,
                ResultSetProcessor resultSetProcessor)
            {
                this.joinPreloadMethod = joinPreloadMethod;
                this.resultSetProcessor = resultSetProcessor;
            }

            public void ExecutePreload()
            {
                joinPreloadMethod.PreloadAggregation(resultSetProcessor);
            }
        }
    }
}
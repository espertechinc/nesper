using System.Collections.Generic;
using System.Collections.ObjectModel;

using Antlr4.Runtime.Misc;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.context.aifactory.select
{
    public partial class StatementAgentInstanceFactorySelect
    {
        private class NamedWindowConsumerPreload : StatementAgentInstancePreload
        {
            private readonly ViewableActivatorNamedWindow nwActivator;
            private readonly NamedWindowConsumerView consumer;
            private readonly AgentInstanceContext agentInstanceContext;
            private readonly JoinPreloadMethod joinPreloadMethod;

            public NamedWindowConsumerPreload(
                ViewableActivatorNamedWindow nwActivator,
                NamedWindowConsumerView consumer,
                AgentInstanceContext agentInstanceContext,
                JoinPreloadMethod joinPreloadMethod)
            {
                this.nwActivator = nwActivator;
                this.consumer = consumer;
                this.agentInstanceContext = agentInstanceContext;
                this.joinPreloadMethod = joinPreloadMethod;
            }

            public void ExecutePreload()
            {
                if (nwActivator.NamedWindowContextName != null &&
                    !nwActivator.NamedWindowContextName.Equals(agentInstanceContext.StatementContext.ContextName)) {
                    return;
                }

                var snapshot = consumer
                    .ConsumerCallback
                    .Snapshot(nwActivator.FilterQueryGraph, agentInstanceContext.Annotations);

                EventBean[] events;
                if (consumer.Filter == null) {
                    events = CollectionUtil.ToArrayEvents(snapshot);
                }
                else {
                    var eventsInWindow = new List<EventBean>(snapshot.Count);
                    ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                        snapshot.GetEnumerator(),
                        consumer.Filter,
                        agentInstanceContext,
                        eventsInWindow);
                    events = eventsInWindow.ToArray();
                }

                if (events.Length == 0) {
                    return;
                }

                consumer.Update(events, null);

                if (joinPreloadMethod != null &&
                    !joinPreloadMethod.IsPreloading &&
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable != null) {
                    agentInstanceContext.EpStatementAgentInstanceHandle.OptionalDispatchable.Execute();
                }
            }
        }
    }
}
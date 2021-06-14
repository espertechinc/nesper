///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.client.dataflow.annotations;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.dataflow.interfaces;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.dataflow.op.eventbussource
{
    public class EventBusSourceOp : DataFlowSourceOperator,
        DataFlowOperatorLifecycle,
        FilterHandleCallback
    {
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly EPDataFlowEventBeanCollector collector;
        private readonly EventBusSourceFactory factory;

        protected LinkedBlockingQueue<object> emittables = new LinkedBlockingQueue<object>();

        [DataFlowContext] protected EPDataFlowEmitter graphContext;

        public EventBusSourceOp(
            EventBusSourceFactory factory,
            AgentInstanceContext agentInstanceContext,
            EPDataFlowEventBeanCollector collector)
        {
            this.factory = factory;
            this.agentInstanceContext = agentInstanceContext;
            this.collector = collector;
        }

        public void Next()
        {
            object next = emittables.Pop();
            graphContext.Submit(next);
        }

        public void Open(DataFlowOpOpenContext openContext)
        {
            var adapter = agentInstanceContext.DataFlowFilterServiceAdapter;
            var filterService = agentInstanceContext.FilterService;
            var filterValues = factory.FilterSpecActivatable.Plan.EvaluateValueSet(
                null,
                agentInstanceContext,
                agentInstanceContext.StatementContextFilterEvalEnv);
            if (filterValues != null) {
                adapter.AddFilterCallback(
                    this,
                    agentInstanceContext,
                    factory.FilterSpecActivatable.FilterForEventType,
                    filterValues,
                    factory.FilterSpecActivatable.FilterCallbackId);
                var filtersVersion = filterService.FiltersVersion;
                agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
            }
        }

        public void Close(DataFlowOpCloseContext closeContext)
        {
            lock (this) {
                var adapter = agentInstanceContext.DataFlowFilterServiceAdapter;
                var filterService = agentInstanceContext.FilterService;
                var filterValues = factory.FilterSpecActivatable.Plan.EvaluateValueSet(
                    null,
                    agentInstanceContext,
                    agentInstanceContext.StatementContextFilterEvalEnv);
                if (filterValues != null) {
                    adapter.RemoveFilterCallback(
                        this,
                        agentInstanceContext,
                        factory.FilterSpecActivatable.FilterForEventType,
                        filterValues,
                        factory.FilterSpecActivatable.FilterCallbackId);
                    var filtersVersion = filterService.FiltersVersion;
                    agentInstanceContext.EpStatementAgentInstanceHandle.StatementFilterVersion.StmtFilterVersion = filtersVersion;
                }
            }
        }

        public void MatchFound(
            EventBean theEvent,
            ICollection<FilterHandleCallback> allStmtMatches)
        {
            if (collector != null) {
                var holder = new EPDataFlowEventBeanCollectorContext(graphContext, factory.IsSubmitEventBean, theEvent);
                collector.Collect(holder);
            }
            else if (factory.IsSubmitEventBean) {
                emittables.Push(theEvent);
            }
            else {
                emittables.Push(theEvent.Underlying);
            }
        }

        public bool IsSubSelect => false;
    }
} // end of namespace
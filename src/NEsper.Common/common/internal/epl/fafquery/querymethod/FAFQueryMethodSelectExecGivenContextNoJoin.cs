///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;
using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecGivenContextNoJoin : FAFQueryMethodSelectExec
    {
        public static readonly FAFQueryMethodSelectExec INSTANCE = new FAFQueryMethodSelectExecGivenContextNoJoin();

        private FAFQueryMethodSelectExecGivenContextNoJoin()
        {
        }

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            var processor = select.Processors[0];

            var singleSelector = contextPartitionSelectors != null && contextPartitionSelectors.Length > 0
                ? contextPartitionSelectors[0]
                : null;
            var agentInstanceIds = AgentInstanceIds(processor, singleSelector, contextManagementService);

            IList<ContextPartitionResult> contextPartitionResults = new List<ContextPartitionResult>();
            foreach (var agentInstanceId in agentInstanceIds) {
                var processorInstance = processor.GetProcessorInstanceContextById(agentInstanceId);
                if (processorInstance != null) {
                    var coll = processorInstance.SnapshotBestEffort(select.QueryGraph, select.Annotations);
                    contextPartitionResults.Add(
                        new ContextPartitionResult(coll, processorInstance.AgentInstanceContext));
                }
            }

            // process context partitions
            var events = new ArrayDeque<EventBean[]>();
            ResultSetProcessor resultSetProcessor = null;
            foreach (var contextPartitionResult in contextPartitionResults) {
                resultSetProcessor = ProcessorWithAssign(
                    select.ResultSetProcessorFactoryProvider,
                    contextPartitionResult.Context,
                    assignerSetter,
                    select.TableAccesses,
                    select.Subselects);

                var snapshot = contextPartitionResult.Events;
                if (select.WhereClause != null) {
                    snapshot = Filtered(
                        snapshot,
                        select.WhereClause,
                        contextPartitionResult.Context);
                }

                var rows = snapshot.ToArray();
                resultSetProcessor.ExprEvaluatorContext = contextPartitionResult.Context;
                var results = resultSetProcessor.ProcessViewResult(rows, null, true);
                if (results != null && results.First != null && results.First.Length > 0) {
                    events.Add(results.First);
                }
            }

            var distinct = EventBeanUtility.GetDistinctByProp(
                EventBeanUtility.Flatten(events),
                select.DistinctKeyGetter);
            return new EPPreparedQueryResult(select.EventType, distinct);
        }

        public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            FAFQueryMethodSelectExecUtil.ReleaseTableLocks(processors);
        }

        internal class ContextPartitionResult
        {
            internal ContextPartitionResult(
                ICollection<EventBean> events,
                AgentInstanceContext context)
            {
                Events = events;
                Context = context;
            }

            public ICollection<EventBean> Events { get; }

            public AgentInstanceContext Context { get; }
        }
    }
} // end of namespace
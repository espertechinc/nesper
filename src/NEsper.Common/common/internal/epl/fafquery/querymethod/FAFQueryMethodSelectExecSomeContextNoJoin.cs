///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;
using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecSomeContextNoJoin : FAFQueryMethodSelectExec
    {
        public static readonly FAFQueryMethodSelectExec INSTANCE = new FAFQueryMethodSelectExecSomeContextNoJoin();

        private FAFQueryMethodSelectExecSomeContextNoJoin()
        {
        }

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            var processor = select.Processors[0];

            var singleSelector =
                contextPartitionSelectors != null && contextPartitionSelectors.Length > 0
                    ? contextPartitionSelectors[0]
                    : null;
            var agentInstanceIds = AgentInstanceIds(processor, singleSelector, contextManagementService);

            ICollection<EventBean> events = new ArrayDeque<EventBean>();
            AgentInstanceContext agentInstanceContext = null;
            foreach (var agentInstanceId in agentInstanceIds) {
                var processorInstance = processor.GetProcessorInstanceContextById(agentInstanceId);
                if (processorInstance != null) {
                    agentInstanceContext = processorInstance.AgentInstanceContext;
                    var coll = processorInstance.SnapshotBestEffort(
                        select.QueryGraph,
                        select.Annotations);
                    events.AddAll(coll);
                }
            }

            // get RSP
            var resultSetProcessor = ProcessorWithAssign(
                select.ResultSetProcessorFactoryProvider,
                agentInstanceContext,
                assignerSetter,
                select.TableAccesses,
                select.Subselects);

            if (select.WhereClause != null) {
                events = Filtered(events, select.WhereClause, agentInstanceContext);
            }

            return ProcessedNonJoin(resultSetProcessor, events, select.DistinctKeyGetter);
        }

        public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            processors[0].StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
        }
    }
} // end of namespace
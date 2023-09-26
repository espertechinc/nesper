///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.@base;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecNoContextJoin : FAFQueryMethodSelectExec
    {
        public static readonly FAFQueryMethodSelectExec INSTANCE = new FAFQueryMethodSelectExecNoContextJoin();

        private FAFQueryMethodSelectExecNoContextJoin()
        {
        }

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            var numStreams = select.Processors.Length;
            var snapshots = new ICollection<EventBean>[numStreams];

            AgentInstanceContext agentInstanceContext = null;
            var viewablePerStream = new Viewable[numStreams];

            for (var i = 0; i < numStreams; i++) {
                var processor = select.Processors[i];
                var processorInstance = processor.ProcessorInstanceNoContext;
                snapshots[i] = Snapshot(
                    select.ConsumerFilters[i],
                    processorInstance,
                    select.QueryGraph,
                    select.Annotations);
                agentInstanceContext = processorInstance.AgentInstanceContext;
                viewablePerStream[i] = processorInstance.TailViewInstance;
            }

            // get RSP
            var resultSetProcessor = ProcessorWithAssign(
                select.ResultSetProcessorFactoryProvider,
                agentInstanceContext,
                assignerSetter,
                select.TableAccesses,
                select.Subselects);

            // determine join
            var joinSetComposerDesc = select.JoinSetComposerPrototype.Create(
                viewablePerStream,
                true,
                agentInstanceContext,
                false);
            var joinComposer = joinSetComposerDesc.JoinSetComposer;

            var oldDataPerStream = new EventBean[numStreams][];
            var newDataPerStream = new EventBean[numStreams][];
            for (var i = 0; i < numStreams; i++) {
                newDataPerStream[i] = snapshots[i].ToArray();
            }

            var result = joinComposer.Join(
                newDataPerStream,
                oldDataPerStream,
                agentInstanceContext);
            if (joinSetComposerDesc.PostJoinFilterEvaluator != null) {
                JoinSetComposerUtil.Filter(
                    joinSetComposerDesc.PostJoinFilterEvaluator,
                    result.First,
                    true,
                    agentInstanceContext);
            }

            var results = resultSetProcessor.ProcessJoinResult(
                result.First,
                EmptySet<MultiKeyArrayOfKeys<EventBean>>.Instance,
                true);

            var distinct = EventBeanUtility.GetDistinctByProp(results?.First, select.DistinctKeyGetter);

            return new EPPreparedQueryResult(resultSetProcessor.ResultEventType, distinct);
        }

        public void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            FAFQueryMethodSelectExecUtil.ReleaseTableLocks(processors);
        }
    }
} // end of namespace
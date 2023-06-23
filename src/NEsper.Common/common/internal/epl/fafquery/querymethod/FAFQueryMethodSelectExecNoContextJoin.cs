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
            int numStreams = select.Processors.Length;
            ICollection<EventBean>[] snapshots = new ICollection<EventBean>[numStreams];

            AgentInstanceContext agentInstanceContext = null;
            Viewable[] viewablePerStream = new Viewable[numStreams];

            for (int i = 0; i < numStreams; i++) {
                FireAndForgetProcessor processor = select.Processors[i];
                FireAndForgetInstance processorInstance = processor.ProcessorInstanceNoContext;
                snapshots[i] = Snapshot(
                    select.ConsumerFilters[i],
                    processorInstance,
                    select.QueryGraph,
                    select.Annotations);
                agentInstanceContext = processorInstance.AgentInstanceContext;
                viewablePerStream[i] = processorInstance.TailViewInstance;
            }

            // get RSP
            ResultSetProcessor resultSetProcessor = ProcessorWithAssign(
                select.ResultSetProcessorFactoryProvider,
                agentInstanceContext,
                assignerSetter,
                select.TableAccesses,
                select.Subselects);

            // determine join
            JoinSetComposerDesc joinSetComposerDesc = select.JoinSetComposerPrototype.Create(
                viewablePerStream,
                true,
                agentInstanceContext,
                false);
            JoinSetComposer joinComposer = joinSetComposerDesc.JoinSetComposer;

            EventBean[][] oldDataPerStream = new EventBean[numStreams][];
            EventBean[][] newDataPerStream = new EventBean[numStreams][];
            for (int i = 0; i < numStreams; i++) {
                newDataPerStream[i] = snapshots[i].ToArray();
            }

            UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> result = joinComposer.Join(
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

            UniformPair<EventBean[]> results = resultSetProcessor.ProcessJoinResult(result.First, null, true);

            EventBean[] distinct = EventBeanUtility.GetDistinctByProp(results?.First, select.DistinctKeyGetter);

            return new EPPreparedQueryResult(resultSetProcessor.ResultEventType, distinct);
        }
    }
} // end of namespace
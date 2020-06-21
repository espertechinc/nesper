///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.context;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.epl.fafquery.querymethod.FAFQueryMethodSelectExecUtil;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecNoContextNoJoin : FAFQueryMethodSelectExec
    {
        public static readonly FAFQueryMethodSelectExec INSTANCE = new FAFQueryMethodSelectExecNoContextNoJoin();

        private FAFQueryMethodSelectExecNoContextNoJoin()
        {
        }

        public EPPreparedQueryResult Execute(
            FAFQueryMethodSelect select,
            ContextPartitionSelector[] contextPartitionSelectors,
            FAFQueryMethodAssignerSetter assignerSetter,
            ContextManagementService contextManagementService)
        {
            FireAndForgetProcessor processor = select.Processors[0];
            FireAndForgetInstance processorInstance = processor.ProcessorInstanceNoContext;

            ICollection<EventBean> events;
            AgentInstanceContext agentInstanceContext = null;
            if (processorInstance == null) {
                events = EmptyList<EventBean>.Instance;
            }
            else {
                agentInstanceContext = processorInstance.AgentInstanceContext;
                events = Snapshot(select.ConsumerFilters[0], processorInstance, select.QueryGraph, select.Annotations);
            }

            // get RSP
            ResultSetProcessor resultSetProcessor = ProcessorWithAssign(
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
    }
} // end of namespace
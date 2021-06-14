///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.fafquery.processor;
using com.espertech.esper.common.@internal.epl.join.querygraph;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    public class FAFQueryMethodSelectExecUtil
    {
        protected internal static ICollection<EventBean> Snapshot(
            ExprEvaluator filter,
            FireAndForgetInstance processorInstance,
            QueryGraph queryGraph,
            Attribute[] annotations)
        {
            var coll = processorInstance.SnapshotBestEffort(queryGraph, annotations);
            if (filter != null) {
                coll = Filtered(coll, filter, processorInstance.AgentInstanceContext);
            }

            return coll;
        }

        protected internal static ResultSetProcessor ProcessorWithAssign(
            ResultSetProcessorFactoryProvider processorProvider,
            AgentInstanceContext agentInstanceContext,
            FAFQueryMethodAssignerSetter assignerSetter,
            IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses,
            IDictionary<int, SubSelectFactory> subselects)
        {
            // start table-access
            var tableAccessEvals = ExprTableEvalHelperStart.StartTableAccess(tableAccesses, agentInstanceContext);

            // get RSP
            var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                processorProvider,
                agentInstanceContext,
                false,
                null);
            
            // start subselects
            var subselectStopCallbacks = new List<AgentInstanceMgmtCallback>(2);
            IDictionary<int, SubSelectFactoryResult> subselectActivations = SubSelectHelperStart.StartSubselects(
                subselects, agentInstanceContext, subselectStopCallbacks, false);

            // assign
            assignerSetter.Assign(
                new StatementAIFactoryAssignmentsImpl(
                    pair.Second,
                    null,
                    null,
                    subselectActivations,
                    tableAccessEvals,
                    null));

            return pair.First;
        }

        protected internal static ICollection<EventBean> Filtered(
            ICollection<EventBean> snapshot,
            ExprEvaluator filterExpressions,
            AgentInstanceContext agentInstanceContext)
        {
            ArrayDeque<EventBean> deque = new ArrayDeque<EventBean>(Math.Min(snapshot.Count, 16));
            ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                snapshot.GetEnumerator(),
                filterExpressions,
                agentInstanceContext,
                deque);
            return deque;
        }

        protected internal static EPPreparedQueryResult ProcessedNonJoin(
            ResultSetProcessor resultSetProcessor,
            ICollection<EventBean> events,
            EventPropertyValueGetter distinctKeyGetter)
        {
            var rows = events.ToArray();
            var results = resultSetProcessor.ProcessViewResult(rows, null, true);

            EventBean[] distinct;
            if (distinctKeyGetter == null) {
                distinct = results.First;
            }
            else {
                distinct = EventBeanUtility.GetDistinctByProp(results.First, distinctKeyGetter);
            }

            return new EPPreparedQueryResult(resultSetProcessor.ResultEventType, distinct);
        }
    }
} // end of namespace
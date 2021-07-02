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
        internal static ICollection<EventBean> Snapshot(
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

        internal static ResultSetProcessor ProcessorWithAssign(
            ResultSetProcessorFactoryProvider processorProvider,
            AgentInstanceContext agentInstanceContext,
            FAFQueryMethodAssignerSetter assignerSetter,
            IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses,
            IDictionary<int, SubSelectFactory> subselects)
        {
            return ProcessorWithAssign(processorProvider, agentInstanceContext, agentInstanceContext, assignerSetter, tableAccesses, subselects);
        }

        internal static ResultSetProcessor ProcessorWithAssign(
            ResultSetProcessorFactoryProvider processorProvider,
            ExprEvaluatorContext exprEvaluatorContext,
            AgentInstanceContext agentInstanceContextOpt,
            FAFQueryMethodAssignerSetter assignerSetter,
            IDictionary<int, ExprTableEvalStrategyFactory> tableAccesses,
            IDictionary<int, SubSelectFactory> subselects)
        {
            // start table-access
            var tableAccessEvals = ExprTableEvalHelperStart.StartTableAccess(tableAccesses, exprEvaluatorContext);

            // get RSP
            var pair = StatementAgentInstanceFactoryUtil.StartResultSetAndAggregation(
                processorProvider,
                exprEvaluatorContext,
                false,
                null);
            
            // start subselects
            var subselectStopCallbacks = new List<AgentInstanceMgmtCallback>(2);
            var subselectActivations = SubSelectHelperStart.StartSubselects(
                subselects,
                exprEvaluatorContext,
                agentInstanceContextOpt,
                subselectStopCallbacks,
                false);

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

        internal static ICollection<EventBean> Filtered(
            ICollection<EventBean> snapshot,
            ExprEvaluator filterExpressions,
            AgentInstanceContext agentInstanceContext)
        {
            var deque = new ArrayDeque<EventBean>(Math.Min(snapshot.Count, 16));
            ExprNodeUtilityEvaluate.ApplyFilterExpressionIterable(
                snapshot.GetEnumerator(),
                filterExpressions,
                agentInstanceContext,
                deque);
            return deque;
        }

        internal static EPPreparedQueryResult ProcessedNonJoin(
            ResultSetProcessor resultSetProcessor,
            ICollection<EventBean> events,
            EventPropertyValueGetter distinctKeyGetter)
        {
            var rows = events.ToArray();
            return ProcessedNonJoin(resultSetProcessor, rows, distinctKeyGetter);
        }

        internal static EPPreparedQueryResult ProcessedNonJoin(
            ResultSetProcessor resultSetProcessor,
            EventBean[] rows,
            EventPropertyValueGetter distinctKeyGetter)
        {
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

        internal static void ReleaseTableLocks(FireAndForgetProcessor[] processors)
        {
            foreach (FireAndForgetProcessor processor in processors) {
                processor.StatementContext.TableExprEvaluatorContext.ReleaseAcquiredLocks();
            }
        }
    }
} // end of namespace
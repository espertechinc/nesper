///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.table.strategy;
using com.espertech.esper.compat;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
    /// <summary>
    ///     Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllWTableImpl : AggregationService
    {
        private readonly AggregationMultiFunctionAgent[] accessAgents;
        private readonly int[] accessColumnsZeroOffset;
        private readonly TableColumnMethodPairEval[] methodPairs;
        private readonly TableInstanceUngrouped tableInstance;

        public AggSvcGroupAllWTableImpl(
            TableInstanceUngrouped tableInstance,
            TableColumnMethodPairEval[] methodPairs,
            AggregationMultiFunctionAgent[] accessAgents,
            int[] accessColumnsZeroOffset)
        {
            this.tableInstance = tableInstance;
            this.methodPairs = methodPairs;
            this.accessAgents = accessAgents;
            this.accessColumnsZeroOffset = accessColumnsZeroOffset;
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
            var @event = tableInstance.GetCreateRowIntoTable(exprEvaluatorContext);
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            for (var i = 0; i < methodPairs.Length; i++) {
                var methodPair = methodPairs[i];
                var columnResult = methodPair.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                row.EnterAgg(methodPair.Column, columnResult);
            }

            for (var i = 0; i < accessAgents.Length; i++) {
                accessAgents[i].ApplyEnter(eventsPerStream, exprEvaluatorContext, row, accessColumnsZeroOffset[i]);
            }

            tableInstance.HandleRowUpdated(@event);
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
            var @event = tableInstance.GetCreateRowIntoTable(exprEvaluatorContext);
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            for (var i = 0; i < methodPairs.Length; i++) {
                var methodPair = methodPairs[i];
                var columnResult = methodPair.Evaluator.Evaluate(eventsPerStream, false, exprEvaluatorContext);
                row.LeaveAgg(methodPair.Column, columnResult);
            }

            for (var i = 0; i < accessAgents.Length; i++) {
                accessAgents[i].ApplyLeave(eventsPerStream, exprEvaluatorContext, row, accessColumnsZeroOffset[i]);
            }

            tableInstance.HandleRowUpdated(@event);
        }

        public void SetCurrentAccess(
            object groupKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel)
        {
            // no action needed - this implementation does not group and the current row is the single group
        }

        public object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
            var @event = tableInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            return row.GetValue(column, eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            // acquire table-level write lock
            TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, context);
            var @event = tableInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            return row.GetCollectionOfEvents(column, eventsPerStream, isNewData, context);
        }

        public ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            // acquire table-level write lock
            TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, context);
            var @event = tableInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            return row.GetCollectionScalar(column, eventsPerStream, isNewData, context);
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            // acquire table-level write lock
            TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, context);
            var @event = tableInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }

            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            return row.GetEventBean(column, eventsPerStream, isNewData, context);
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            // clear not required
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }
        
        public void Accept(AggregationServiceVisitor visitor)
        {
            // not applicable
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
        }

        public bool IsGrouped => false;

        public object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }

        public void Stop()
        {
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return this;
        }

        public AggregationRow GetAggregationRow(
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public abstract class AggSvcGroupByWTableBase : AggregationService
    {
        protected readonly TableMetadata TableMetadata;
        protected readonly TableColumnMethodPair[] MethodPairs;
        protected readonly AggregationAccessorSlotPair[] Accessors;
        protected readonly bool IsJoin;
        protected readonly TableStateInstanceGrouped TableStateInstance;
        protected readonly int[] TargetStates;
        protected readonly ExprNode[] AccessStateExpr;
        private readonly AggregationAgent[] _agents;

        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        protected AggregationMethod[] CurrentAggregatorMethods;
        protected AggregationState[] CurrentAggregatorStates;
        protected object CurrentGroupKey;

        protected AggSvcGroupByWTableBase(
            TableMetadata tableMetadata,
            TableColumnMethodPair[] methodPairs,
            AggregationAccessorSlotPair[] accessors,
            bool join,
            TableStateInstanceGrouped tableStateInstance,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents)
        {
            TableMetadata = tableMetadata;
            MethodPairs = methodPairs;
            Accessors = accessors;
            IsJoin = join;
            TableStateInstance = tableStateInstance;
            TargetStates = targetStates;
            AccessStateExpr = accessStateExpr;
            _agents = agents;
        }

        public abstract void ApplyEnterInternal(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext);
        public abstract void ApplyLeaveInternal(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext);

        public void ApplyEnter(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(TableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
            ApplyEnterInternal(eventsPerStream, groupByKey, exprEvaluatorContext);
        }

        public void ApplyLeave(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(TableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
            ApplyLeaveInternal(eventsPerStream, groupByKey, exprEvaluatorContext);
        }

        protected void ApplyEnterGroupKey(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            var bean = TableStateInstance.GetCreateRowIntoTable(groupByKey, exprEvaluatorContext);
            var row = (AggregationRowPair)bean.Properties[0];

            CurrentAggregatorMethods = row.Methods;
            CurrentAggregatorStates = row.States;

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, MethodPairs.Length, TargetStates.Length, groupByKey); }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);

            for (var j = 0; j < MethodPairs.Length; j++)
            {
                var methodPair = MethodPairs[j];
                var method = CurrentAggregatorMethods[methodPair.TargetIndex];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, j, method, methodPair.AggregationNode); }
                var columnResult = methodPair.Evaluator.Evaluate(evaluateParams);
                method.Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, j, method); }
            }

            for (var i = 0; i < TargetStates.Length; i++)
            {
                var state = CurrentAggregatorStates[TargetStates[i]];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, state, AccessStateExpr[i]); }
                _agents[i].ApplyEnter(eventsPerStream, exprEvaluatorContext, state);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, state); }
            }

            TableStateInstance.HandleRowUpdated(bean);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true); }
        }

        protected void ApplyLeaveGroupKey(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            var bean = TableStateInstance.GetCreateRowIntoTable(groupByKey, exprEvaluatorContext);
            var row = (AggregationRowPair)bean.Properties[0];

            CurrentAggregatorMethods = row.Methods;
            CurrentAggregatorStates = row.States;

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, MethodPairs.Length, TargetStates.Length, groupByKey); }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            for (var j = 0; j < MethodPairs.Length; j++)
            {
                var methodPair = MethodPairs[j];
                var method = CurrentAggregatorMethods[methodPair.TargetIndex];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, j, method, methodPair.AggregationNode); }
                var columnResult = methodPair.Evaluator.Evaluate(evaluateParams);
                method.Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, j, method); }
            }

            for (var i = 0; i < TargetStates.Length; i++)
            {
                var state = CurrentAggregatorStates[TargetStates[i]];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, state, AccessStateExpr[i]); }
                _agents[i].ApplyLeave(eventsPerStream, exprEvaluatorContext, state);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, state); }
            }

            TableStateInstance.HandleRowUpdated(bean);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false); }
        }

        public virtual void SetCurrentAccess(object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            var bean = TableStateInstance.GetRowForGroupKey(groupByKey);

            if (bean != null)
            {
                var row = (AggregationRowPair)bean.Properties[0];
                CurrentAggregatorMethods = row.Methods;
                CurrentAggregatorStates = row.States;
            }
            else
            {
                CurrentAggregatorMethods = null;
            }

            CurrentGroupKey = groupByKey;
        }

        public object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams)
        {
            if (column < CurrentAggregatorMethods.Length)
            {
                return CurrentAggregatorMethods[column].Value;
            }
            else
            {
                var pair = Accessors[column - CurrentAggregatorMethods.Length];
                return pair.Accessor.GetValue(CurrentAggregatorStates[pair.Slot], evaluateParams);
            }
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams)
        {
            if (column < CurrentAggregatorMethods.Length)
            {
                return null;
            }
            else
            {
                var pair = Accessors[column - CurrentAggregatorMethods.Length];
                return pair.Accessor.GetEnumerableEvents(CurrentAggregatorStates[pair.Slot], evaluateParams);
            }
        }

        public ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams)
        {
            if (column < CurrentAggregatorMethods.Length)
            {
                return null;
            }
            else
            {
                var pair = Accessors[column - CurrentAggregatorMethods.Length];
                return pair.Accessor.GetEnumerableScalar(CurrentAggregatorStates[pair.Slot], evaluateParams);
            }
        }

        public EventBean GetEventBean(int column, EvaluateParams evaluateParams)
        {
            if (column < CurrentAggregatorMethods.Length)
            {
                return null;
            }
            else
            {
                var pair = Accessors[column - CurrentAggregatorMethods.Length];
                return pair.Accessor.GetEnumerableEvent(CurrentAggregatorStates[pair.Slot], evaluateParams);
            }
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
            // not applicable
        }

        public bool IsGrouped
        {
            get { return true; }
        }

        public object GetGroupKey(int agentInstanceId)
        {
            return CurrentGroupKey;
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return TableStateInstance.GroupKeys;
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            TableStateInstance.Clear();
        }

        public void Stop()
        {
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return this;
        }
    }
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.table.strategy;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllMixedAccessWTableImpl : AggregationService
    {
        private readonly TableStateInstanceUngrouped _tableStateInstance;
        private readonly TableColumnMethodPair[] _methodPairs;
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly int[] _targetStates;
        private readonly ExprNode[] _accessStateExpr;
        private readonly AggregationAgent[] _agents;
    
        public AggSvcGroupAllMixedAccessWTableImpl(TableStateInstanceUngrouped tableStateInstance, TableColumnMethodPair[] methodPairs, AggregationAccessorSlotPair[] accessors, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents)
        {
            _tableStateInstance = tableStateInstance;
            _methodPairs = methodPairs;
            _accessors = accessors;
            _targetStates = targetStates;
            _accessStateExpr = accessStateExpr;
            _agents = agents;
        }
    
        public void ApplyEnter(EventBean[] eventsPerStream, object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
    
            var @event = _tableStateInstance.GetCreateRowIntoTable(null, exprEvaluatorContext);
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(true, row.Methods.Length, row.States.Length);}
            for (var i = 0; i < _methodPairs.Length; i++)
            {
                var methodPair = _methodPairs[i];
                var method = row.Methods[methodPair.TargetIndex];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, method, methodPair.AggregationNode);}
                var columnResult = methodPair.Evaluator.Evaluate(evaluateParams);
                method.Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, method);}
            }
    
            for (var i = 0; i < _targetStates.Length; i++) {
                var state = row.States[_targetStates[i]];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, state, _accessStateExpr[i]);}
                _agents[i].ApplyEnter(eventsPerStream, exprEvaluatorContext, state);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, state);}
            }
    
            _tableStateInstance.HandleRowUpdated(@event);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(true);}
        }
    
        public void ApplyLeave(EventBean[] eventsPerStream, object optionalGroupKeyPerRow, ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
    
            var @event = _tableStateInstance.GetCreateRowIntoTable(null, exprEvaluatorContext);
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(false, row.Methods.Length, row.States.Length); }
    
            for (var i = 0; i < _methodPairs.Length; i++)
            {
                var methodPair = _methodPairs[i];
                var method = row.Methods[methodPair.TargetIndex];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, method, methodPair.AggregationNode);}
                var columnResult = methodPair.Evaluator.Evaluate(evaluateParams);
                method.Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, method);}
            }
    
            for (var i = 0; i < _targetStates.Length; i++) {
                var state = row.States[_targetStates[i]];
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, state, _accessStateExpr[i]);}
                _agents[i].ApplyLeave(eventsPerStream, exprEvaluatorContext, state);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, state);}
            }
    
            _tableStateInstance.HandleRowUpdated(@event);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(false);}
        }
    
        public void SetCurrentAccess(object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            // no action needed - this implementation does not group and the current row is the single group
        }
    
        public object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
    
            var @event = _tableStateInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var aggregators = row.Methods;
            if (column < aggregators.Length) {
                return aggregators[column].Value;
            }
            else {
                var pair = _accessors[column - aggregators.Length];
                return pair.Accessor.GetValue(row.States[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }
    
        public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, context);

            var @event = _tableStateInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var aggregators = row.Methods;
            if (column < aggregators.Length) {
                return null;
            }
            else {
                var pair = _accessors[column - aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(row.States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, context);

            var @event = _tableStateInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var aggregators = row.Methods;
            if (column < aggregators.Length) {
                return null;
            }
            else {
                var pair = _accessors[column - aggregators.Length];
                return pair.Accessor.GetEnumerableScalar(row.States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, context);

            var @event = _tableStateInstance.EventUngrouped;
            if (@event == null) {
                return null;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var aggregators = row.Methods;
    
            if (column < aggregators.Length) {
                return null;
            }
            else {
                var pair = _accessors[column - aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(row.States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            // acquire table-level write lock
            ExprTableEvalLockUtil.ObtainLockUnless(_tableStateInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);

            var @event = _tableStateInstance.EventUngrouped;
            if (@event == null) {
                return;
            }
            var row = ExprTableEvalStrategyUtil.GetRow(@event);
            var aggregators = row.Methods;
    
            foreach (var state in row.States) {
                state.Clear();
            }
            foreach (var aggregator in aggregators) {
                aggregator.Clear();
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
        }

        public bool IsGrouped
        {
            get { return false; }
        }

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
    }
}

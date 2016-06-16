///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByReclaimAgedImpl : AggregationServiceBaseGrouped
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const long DEFAULT_MAX_AGE_MSEC = 60000L;

        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly AggregationStateFactory[] _accessAggregations;
        private readonly bool _isJoin;
    
        private readonly AggSvcGroupByReclaimAgedEvalFunc _evaluationFunctionMaxAge;
        private readonly AggSvcGroupByReclaimAgedEvalFunc _evaluationFunctionFrequency;
        private readonly MethodResolutionService _methodResolutionService;
    
        // maintain for each group a row of aggregator states that the expression node can pull the data from via index
        private readonly IDictionary<Object, AggregationMethodRowAged> _aggregatorsPerGroup;
    
        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        private AggregationMethod[] _currentAggregatorMethods;
        private AggregationState[] _currentAggregatorStates;
        private Object _currentGroupKey;
    
        private readonly IList<Object> _removedKeys;
        private long? _nextSweepTime = null;
        private AggregationRowRemovedCallback _removedCallback;
        private long _currentMaxAge = DEFAULT_MAX_AGE_MSEC;
        private long _currentReclaimFrequency = DEFAULT_MAX_AGE_MSEC;
    
        public AggSvcGroupByReclaimAgedImpl(ExprEvaluator[] evaluators, AggregationMethodFactory[] aggregators, Object groupKeyBinding, AggregationAccessorSlotPair[] accessors, AggregationStateFactory[] accessAggregations, bool join, AggSvcGroupByReclaimAgedEvalFunc evaluationFunctionMaxAge, AggSvcGroupByReclaimAgedEvalFunc evaluationFunctionFrequency, MethodResolutionService methodResolutionService)
            : base(evaluators, aggregators, groupKeyBinding)
        {
            _accessors = accessors;
            _accessAggregations = accessAggregations;
            _isJoin = join;
            _evaluationFunctionMaxAge = evaluationFunctionMaxAge;
            _evaluationFunctionFrequency = evaluationFunctionFrequency;
            _methodResolutionService = methodResolutionService;
            _aggregatorsPerGroup = new Dictionary<Object, AggregationMethodRowAged>();
            _removedKeys = new List<Object>();
        }
    
        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            _aggregatorsPerGroup.Clear();
        }
    
        public override void ApplyEnter(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, Aggregators.Length, _accessAggregations.Length, groupByKey);}
            long currentTime = exprEvaluatorContext.TimeProvider.Time;
            if ((_nextSweepTime == null) || (_nextSweepTime <= currentTime))
            {
                _currentMaxAge = GetMaxAge(_currentMaxAge);
                _currentReclaimFrequency = GetReclaimFrequency(_currentReclaimFrequency);
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug("Reclaiming groups older then " + _currentMaxAge + " msec and every " + _currentReclaimFrequency + "msec in frequency");
                }
                _nextSweepTime = currentTime + _currentReclaimFrequency;
                Sweep(currentTime, _currentMaxAge);
            }
    
            HandleRemovedKeys(); // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
    
            AggregationMethodRowAged row = _aggregatorsPerGroup.Get(groupByKey);
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationMethod[] groupAggregators;
            AggregationState[] groupStates;
            if (row == null)
            {
                groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                groupStates = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, _isJoin, _accessAggregations, groupByKey, GroupKeyBinding, null, null);
                row = new AggregationMethodRowAged(_methodResolutionService.GetCurrentRowCount(groupAggregators, groupStates) + 1, currentTime, groupAggregators, groupStates);
                _aggregatorsPerGroup.Put(groupByKey, row);
            }
            else
            {
                groupAggregators = row.Methods;
                groupStates = row.States;
                row.IncreaseRefcount();
                row.LastUpdateTime = currentTime;
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorMethods = groupAggregators;
            _currentAggregatorStates = groupStates;
            for (int i = 0; i < Evaluators.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, _currentAggregatorMethods[i], Aggregators[i].AggregationExpression); }
                Object columnResult = Evaluators[i].Evaluate(evaluateParams);
                groupAggregators[i].Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, _currentAggregatorMethods[i]);}
            }
    
            for (int i = 0; i < _currentAggregatorStates.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, _currentAggregatorStates[i], _accessAggregations[i].AggregationExpression); }
                _currentAggregatorStates[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, _currentAggregatorStates[i]);}
            }
    
            InternalHandleUpdated(groupByKey, row);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true);}
        }
    
        private void Sweep(long currentTime, long currentMaxAge)
        {
            ArrayDeque<Object> removed = new ArrayDeque<Object>();
            foreach (var entry in _aggregatorsPerGroup)
            {
                long age = currentTime - entry.Value.LastUpdateTime;
                if (age > currentMaxAge)
                {
                    removed.Add(entry.Key);
                }
            }
    
            foreach (Object key in removed)
            {
                _aggregatorsPerGroup.Remove(key);
                InternalHandleRemoved(key);
                _removedCallback.Removed(key);
            }
        }
    
        private long GetMaxAge(long currentMaxAge)
        {
            var maxAge = _evaluationFunctionMaxAge.LongValue;
            if ((maxAge == null) || (maxAge <= 0))
            {
                return currentMaxAge;
            }
            return (long) Math.Round(maxAge.Value * 1000d);
        }
    
        private long GetReclaimFrequency(long currentReclaimFrequency)
        {
            var frequency = _evaluationFunctionFrequency.LongValue;
            if ((frequency == null) || (frequency <= 0))
            {
                return currentReclaimFrequency;
            }
            return (long) Math.Round(frequency.Value * 1000d);
        }
    
        public override void ApplyLeave(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, Aggregators.Length, _accessAggregations.Length, groupByKey);}
            AggregationMethodRowAged row = _aggregatorsPerGroup.Get(groupByKey);
            long currentTime = exprEvaluatorContext.TimeProvider.Time;
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationMethod[] groupAggregators;
            AggregationState[] groupStates;
            if (row != null)
            {
                groupAggregators = row.Methods;
                groupStates = row.States;
            }
            else
            {
                groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                groupStates = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, _isJoin, _accessAggregations, groupByKey, GroupKeyBinding, null, null);
                row = new AggregationMethodRowAged(_methodResolutionService.GetCurrentRowCount(groupAggregators, groupStates) + 1, currentTime, groupAggregators, groupStates);
                _aggregatorsPerGroup.Put(groupByKey, row);
            }
    
            // For this row, evaluate sub-expressions, enter result
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            _currentAggregatorMethods = groupAggregators;
            _currentAggregatorStates = groupStates;
            for (int i = 0; i < Evaluators.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, _currentAggregatorMethods[i], Aggregators[i].AggregationExpression); }
                Object columnResult = Evaluators[i].Evaluate(evaluateParams);
                groupAggregators[i].Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, _currentAggregatorMethods[i]);}
            }
    
            for (int i = 0; i < _currentAggregatorStates.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, _currentAggregatorStates[i], _accessAggregations[i].AggregationExpression); }
                _currentAggregatorStates[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, _currentAggregatorStates[i]);}
            }
    
            row.DecreaseRefcount();
            row.LastUpdateTime = currentTime;
            if (row.Refcount <= 0)
            {
                _removedKeys.Add(groupByKey);
                _methodResolutionService.RemoveAggregators(exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);  // allow persistence to remove keys already
            }
            InternalHandleUpdated(groupByKey, row);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false); }
        }

        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            AggregationMethodRowAged row = _aggregatorsPerGroup.Get(groupByKey);
    
            if (row != null) {
                _currentAggregatorMethods = row.Methods;
                _currentAggregatorStates = row.States;
            }
            else {
                _currentAggregatorMethods = null;
            }
    
            if (_currentAggregatorMethods == null) {
                _currentAggregatorMethods = _methodResolutionService.NewAggregators(Aggregators, agentInstanceId, groupByKey, GroupKeyBinding, null);
                _currentAggregatorStates = _methodResolutionService.NewAccesses(agentInstanceId, _isJoin, _accessAggregations, groupByKey, GroupKeyBinding, null, null);
            }
    
            _currentGroupKey = groupByKey;
        }
    
        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (column < Aggregators.Length) {
                return _currentAggregatorMethods[column].Value;
            }
            else {
                AggregationAccessorSlotPair pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetValue(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }
    
        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                AggregationAccessorSlotPair pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                AggregationAccessorSlotPair pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableScalar(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                AggregationAccessorSlotPair pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public override void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            _removedCallback = callback;
        }
    
        public void InternalHandleUpdated(Object groupByKey, AggregationMethodRowAged row) {
            // no action required
        }
    
        public void InternalHandleRemoved(Object key) {
            // no action required
        }
    
        public override void Accept(AggregationServiceVisitor visitor) {
            visitor.VisitAggregations(_aggregatorsPerGroup.Count, _aggregatorsPerGroup);
        }
    
        public override void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor) {
            visitor.VisitGrouped(_aggregatorsPerGroup.Count);
            foreach (var entry in _aggregatorsPerGroup) {
                visitor.VisitGroup(entry.Key, entry.Value);
            }
        }

        public override bool IsGrouped
        {
            get { return true; }
        }

        protected void HandleRemovedKeys() {
            if (_removedKeys.IsNotEmpty())     // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
            {
                foreach (Object removedKey in _removedKeys)
                {
                    _aggregatorsPerGroup.Remove(removedKey);
                    InternalHandleRemoved(removedKey);
                }
                _removedKeys.Clear();
            }
        }
    
        public override Object GetGroupKey(int agentInstanceId)
        {
            return _currentGroupKey;
        }
    
        public override ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return _aggregatorsPerGroup.Keys;
        }
    }
}

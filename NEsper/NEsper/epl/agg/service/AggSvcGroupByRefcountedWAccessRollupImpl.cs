///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByRefcountedWAccessRollupImpl : AggregationServiceBaseGrouped
    {
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly AggregationStateFactory[] _accessAggregations;
        private readonly bool _isJoin;
        private readonly AggregationGroupByRollupDesc _rollupLevelDesc;
    
        // maintain for each group a row of aggregator states that the expression node can pull the data from via index
        private readonly IDictionary<Object, AggregationMethodPairRow>[] _aggregatorsPerGroup;
        private readonly AggregationMethodPairRow _aggregatorTopGroup;
    
        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        private AggregationMethod[] _currentAggregatorMethods;
        private AggregationState[] _currentAggregatorStates;
        private Object _currentGroupKey;
    
        private readonly MethodResolutionService _methodResolutionService;

        private readonly Object[] _methodParameterValues;
        private bool _hasRemovedKey;
        private readonly IList<Object>[] _removedKeys;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="prototypes">collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregationsaggregation states for each group</param>
        /// <param name="groupKeyBinding">The group key binding.</param>
        /// <param name="methodResolutionService">factory for creating additional aggregation method instances per group key</param>
        /// <param name="accessors">accessor definitions</param>
        /// <param name="accessAggregations">access aggs</param>
        /// <param name="isJoin">true for join, false for single-stream</param>
        /// <param name="rollupLevelDesc">The rollup level desc.</param>
        /// <param name="topGroupAggregators">The top group aggregators.</param>
        /// <param name="topGroupStates">The top group states.</param>
        public AggSvcGroupByRefcountedWAccessRollupImpl(
            ExprEvaluator[] evaluators,
            AggregationMethodFactory[] prototypes,
            Object groupKeyBinding,
            MethodResolutionService methodResolutionService,
            AggregationAccessorSlotPair[] accessors,
            AggregationStateFactory[] accessAggregations,
            bool isJoin,
            AggregationGroupByRollupDesc rollupLevelDesc,
            AggregationMethod[] topGroupAggregators,
            AggregationState[] topGroupStates)
            : base(evaluators, prototypes, groupKeyBinding)
        {
            _methodResolutionService = methodResolutionService;
    
            _aggregatorsPerGroup = new IDictionary<object, AggregationMethodPairRow>[rollupLevelDesc.NumLevelsAggregation];
            _removedKeys = new List<object>[rollupLevelDesc.NumLevelsAggregation];
            for (var i = 0; i < rollupLevelDesc.NumLevelsAggregation; i++) {
                _aggregatorsPerGroup[i] = new Dictionary<Object, AggregationMethodPairRow>();
                _removedKeys[i] = new List<Object>(2);
            }
            _accessors = accessors;
            _accessAggregations = accessAggregations;
            _isJoin = isJoin;
            _rollupLevelDesc = rollupLevelDesc;
            _aggregatorTopGroup = new AggregationMethodPairRow(0, topGroupAggregators, topGroupStates);
            _methodParameterValues = new Object[evaluators.Length];
        }
    
        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var state in _aggregatorTopGroup.States) {
                state.Clear();
            }
            foreach (var aggregator in _aggregatorTopGroup.Methods) {
                aggregator.Clear();
            }
            for (var i = 0; i < _rollupLevelDesc.NumLevelsAggregation; i++) {
                _aggregatorsPerGroup[i].Clear();
            }
        }
    
        public override void ApplyEnter(EventBean[] eventsPerStream, Object compositeGroupKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            HandleRemovedKeys();

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);

            for (var i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedRollupEvalParam(true, _methodParameterValues.Length);}
                _methodParameterValues[i] = Evaluators[i].Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedRollupEvalParam(_methodParameterValues[i]);}
            }
    
            var groupKeyPerLevel = (Object[]) compositeGroupKey;
            for (var i = 0; i < groupKeyPerLevel.Length; i++) {
                var level = _rollupLevelDesc.Levels[i];
                var groupKey = groupKeyPerLevel[i];
                
                AggregationMethodPairRow row;
                if (!level.IsAggregationTop) {
                    row = _aggregatorsPerGroup[level.AggregationOffset].Get(groupKey);
                }
                else {
                    row = _aggregatorTopGroup;
                }
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, Aggregators.Length, _accessAggregations.Length, groupKey);}
    
                // The aggregators for this group do not exist, need to create them from the prototypes
                AggregationMethod[] groupAggregators;
                AggregationState[] groupStates;
                if (row == null)
                {
                    groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupKey, GroupKeyBinding, level);
                    groupStates = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, _isJoin, _accessAggregations, groupKey, GroupKeyBinding, level, null);
                    row = new AggregationMethodPairRow(_methodResolutionService.GetCurrentRowCount(groupAggregators, groupStates) + 1, groupAggregators, groupStates);
                    if (!level.IsAggregationTop) {
                        _aggregatorsPerGroup[level.AggregationOffset].Put(groupKey, row);
                    }
                }
                else
                {
                    groupAggregators = row.Methods;
                    groupStates = row.States;
                    row.IncreaseRefcount();
                }
    
                // For this row, evaluate sub-expressions, enter result
                _currentAggregatorMethods = groupAggregators;
                _currentAggregatorStates = groupStates;
                for (var j = 0; j < Evaluators.Length; j++)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, j, groupAggregators[j], Aggregators[j].AggregationExpression); }
                    groupAggregators[j].Enter(_methodParameterValues[j]);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, j, groupAggregators[j]);}
                }
    
                for (var j = 0; j < _currentAggregatorStates.Length; j++) {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, j, _currentAggregatorStates[j], _accessAggregations[j].AggregationExpression); }
                    _currentAggregatorStates[j].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, j, _currentAggregatorStates[j]);}
                }
    
                InternalHandleGroupUpdate(groupKey, row, level);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true);}
            }
        }
    
        public override void ApplyLeave(EventBean[] eventsPerStream, Object compositeGroupKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            for (var i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedRollupEvalParam(false, _methodParameterValues.Length);}
                _methodParameterValues[i] = Evaluators[i].Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedRollupEvalParam(_methodParameterValues[i]);}
            }
    
            var groupKeyPerLevel = (Object[]) compositeGroupKey;
            for (var i = 0; i < groupKeyPerLevel.Length; i++) {
                var level = _rollupLevelDesc.Levels[i];
                var groupKey = groupKeyPerLevel[i];
    
                AggregationMethodPairRow row;
                if (!level.IsAggregationTop) {
                    row = _aggregatorsPerGroup[level.AggregationOffset].Get(groupKey);
                }
                else {
                    row = _aggregatorTopGroup;
                }
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, Aggregators.Length, _accessAggregations.Length, groupKey);}
    
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
                    groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupKey, GroupKeyBinding, level);
                    groupStates = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, _isJoin, _accessAggregations, groupKey, GroupKeyBinding, level, null);
                    row = new AggregationMethodPairRow(_methodResolutionService.GetCurrentRowCount(groupAggregators, groupStates) + 1, groupAggregators, groupStates);
                    if (!level.IsAggregationTop) {
                        _aggregatorsPerGroup[level.AggregationOffset].Put(groupKey, row);
                    }
                }
    
                // For this row, evaluate sub-expressions, enter result
                _currentAggregatorMethods = groupAggregators;
                _currentAggregatorStates = groupStates;
                for (var j = 0; j < Evaluators.Length; j++)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, j, groupAggregators[j], Aggregators[j].AggregationExpression); }
                    groupAggregators[j].Leave(_methodParameterValues[j]);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, j, groupAggregators[j]);}
                }
    
                for (var j = 0; j < _currentAggregatorStates.Length; j++) {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, j, _currentAggregatorStates[j], _accessAggregations[j].AggregationExpression); }
                    _currentAggregatorStates[j].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, j, _currentAggregatorStates[j]);}
                }
    
                row.DecreaseRefcount();
                if (row.Refcount <= 0)
                {
                    _hasRemovedKey = true;
                    if (!level.IsAggregationTop) {
                        _removedKeys[level.AggregationOffset].Add(groupKey);
                    }
                    _methodResolutionService.RemoveAggregators(exprEvaluatorContext.AgentInstanceId, groupKey, GroupKeyBinding, level);  // allow persistence to remove keys already
                }
    
                InternalHandleGroupUpdate(groupKey, row, level);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false);}
            }
        }
    
        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            AggregationMethodPairRow row;
            if (rollupLevel.IsAggregationTop) {
                row = _aggregatorTopGroup;
            }
            else {
                row = _aggregatorsPerGroup[rollupLevel.AggregationOffset].Get(groupByKey);
            }
    
            if (row != null) {
                _currentAggregatorMethods = row.Methods;
                _currentAggregatorStates = row.States;
            }
            else {
                _currentAggregatorMethods = null;
            }
    
            if (_currentAggregatorMethods == null) {
                _currentAggregatorMethods = _methodResolutionService.NewAggregators(Aggregators, agentInstanceId, groupByKey, GroupKeyBinding, rollupLevel);
                _currentAggregatorStates = _methodResolutionService.NewAccesses(agentInstanceId, _isJoin, _accessAggregations, groupByKey, GroupKeyBinding, rollupLevel, null);
            }
    
            _currentGroupKey = groupByKey;
        }
    
        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (column < Aggregators.Length) {
                return _currentAggregatorMethods[column].Value;
            }
            else {
                var pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetValue(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }
    
        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                var pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
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
                var pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public override void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            // not applicable
        }
    
        public void InternalHandleGroupUpdate(Object groupByKey, AggregationMethodPairRow row, AggregationGroupByRollupLevel groupByRollupLevel) {
            // no action required
        }
    
        public void InternalHandleGroupRemove(Object groupByKey, AggregationGroupByRollupLevel groupByRollupLevel) {
            // no action required
        }
    
        public override void Accept(AggregationServiceVisitor visitor) {
            visitor.VisitAggregations(GetGroupKeyCount(), _aggregatorsPerGroup);
        }
    
        public override void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor) {
            visitor.VisitGrouped(GetGroupKeyCount());
            foreach (var anAggregatorsPerGroup in _aggregatorsPerGroup) {
                foreach (var entry in anAggregatorsPerGroup) {
                    visitor.VisitGroup(entry.Key, entry.Value);
                }
            }
            visitor.VisitGroup(new Object[0], _aggregatorTopGroup);
        }

        public override bool IsGrouped
        {
            get { return true; }
        }

        protected void HandleRemovedKeys() {
            if (!_hasRemovedKey) {
                return;
            }
            _hasRemovedKey = false;
            for (var i = 0; i < _removedKeys.Length; i++) {
                if (_removedKeys[i].IsEmpty()) {
                    continue;
                }
                foreach (var removedKey in _removedKeys[i])
                {
                    _aggregatorsPerGroup[i].Remove(removedKey);
                    InternalHandleGroupRemove(removedKey, _rollupLevelDesc.Levels[i]);
                }
                _removedKeys[i].Clear();
            }
        }
    
        public override Object GetGroupKey(int agentInstanceId)
        {
            return _currentGroupKey;
        }
    
        public override ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new NotSupportedException();
        }
    
        private int GetGroupKeyCount()
        {
            return 1 + _aggregatorsPerGroup.Sum(anAggregatorsPerGroup => anAggregatorsPerGroup.Count);
        }
    }
}

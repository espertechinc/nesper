///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
    public class AggSvcGroupByRefcountedWAccessImpl : AggregationServiceBaseGrouped
    {
        protected readonly AggregationAccessorSlotPair[] Accessors;
        protected readonly AggregationStateFactory[] AccessAggregations;
        protected readonly bool IsJoin;
    
        // maintain for each group a row of aggregator states that the expression node canb pull the data from via index
        protected IDictionary<Object, AggregationMethodPairRow> AggregatorsPerGroup;
    
        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        private AggregationMethod[] _currentAggregatorMethods;
        private AggregationState[] _currentAggregatorStates;
        private Object _currentGroupKey;
    
        private readonly MethodResolutionService _methodResolutionService;
    
        protected IList<Object> RemovedKeys;

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
        public AggSvcGroupByRefcountedWAccessImpl(ExprEvaluator[] evaluators,
                                                  AggregationMethodFactory[] prototypes,
                                                  Object groupKeyBinding,
                                                  MethodResolutionService methodResolutionService,
                                                  AggregationAccessorSlotPair[] accessors,
                                                  AggregationStateFactory[] accessAggregations,
                                                  bool isJoin)
            : base(evaluators, prototypes, groupKeyBinding)
        {
            _methodResolutionService = methodResolutionService;
            AggregatorsPerGroup = new Dictionary<Object, AggregationMethodPairRow>();
            Accessors = accessors;
            AccessAggregations = accessAggregations;
            IsJoin = isJoin;
            RemovedKeys = new List<Object>();
        }
    
        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregatorsPerGroup.Clear();
        }
    
        public override void ApplyEnter(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, Aggregators.Length, AccessAggregations.Length, groupByKey);}
            HandleRemovedKeys();
    
            var row = AggregatorsPerGroup.Get(groupByKey);
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationMethod[] groupAggregators;
            AggregationState[] groupStates;
            if (row == null)
            {
                groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                groupStates = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, AccessAggregations, groupByKey, GroupKeyBinding, null);
                row = new AggregationMethodPairRow(_methodResolutionService.GetCurrentRowCount(groupAggregators, groupStates) + 1, groupAggregators, groupStates);
                AggregatorsPerGroup.Put(groupByKey, row);
            }
            else
            {
                groupAggregators = row.Methods;
                groupStates = row.States;
                row.IncreaseRefcount();
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorMethods = groupAggregators;
            _currentAggregatorStates = groupStates;
            for (var j = 0; j < Evaluators.Length; j++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, j, groupAggregators[j], Aggregators[j].AggregationExpression); }
                var columnResult = Evaluators[j].Evaluate(evaluateParams);
                groupAggregators[j].Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, j, groupAggregators[j]);}
            }
    
            for (var i = 0; i < _currentAggregatorStates.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, _currentAggregatorStates[i], AccessAggregations[i].AggregationExpression); }
                _currentAggregatorStates[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, _currentAggregatorStates[i]);}
            }
    
            InternalHandleGroupUpdate(groupByKey, row);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true);}
        }
    
        public override void ApplyLeave(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, Aggregators.Length, AccessAggregations.Length, groupByKey);}
            var row = AggregatorsPerGroup.Get(groupByKey);
    
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
                groupStates = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, AccessAggregations, groupByKey, GroupKeyBinding, null);
                row = new AggregationMethodPairRow(_methodResolutionService.GetCurrentRowCount(groupAggregators, groupStates) + 1, groupAggregators, groupStates);
                AggregatorsPerGroup.Put(groupByKey, row);
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorMethods = groupAggregators;
            _currentAggregatorStates = groupStates;
            for (var j = 0; j < Evaluators.Length; j++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, j, groupAggregators[j], Aggregators[j].AggregationExpression); }
                var columnResult = Evaluators[j].Evaluate(evaluateParams);
                groupAggregators[j].Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, j, groupAggregators[j]);}
            }
    
            for (var i = 0; i < _currentAggregatorStates.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, _currentAggregatorStates[i], AccessAggregations[i].AggregationExpression); }
                _currentAggregatorStates[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, _currentAggregatorStates[i]);}
            }
    
            row.DecreaseRefcount();
            if (row.Refcount <= 0)
            {
                RemovedKeys.Add(groupByKey);
                _methodResolutionService.RemoveAggregators(exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);  // allow persistence to remove keys already
            }
    
            InternalHandleGroupUpdate(groupByKey, row);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false);}
        }

        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            var row = AggregatorsPerGroup.Get(groupByKey);
    
            if (row != null) {
                _currentAggregatorMethods = row.Methods;
                _currentAggregatorStates = row.States;
            }
            else {
                _currentAggregatorMethods = null;
            }
    
            if (_currentAggregatorMethods == null) {
                _currentAggregatorMethods = _methodResolutionService.NewAggregators(Aggregators, agentInstanceId, groupByKey, GroupKeyBinding, null);
                _currentAggregatorStates = _methodResolutionService.NewAccesses(agentInstanceId, IsJoin, AccessAggregations, groupByKey, GroupKeyBinding, null);
            }
    
            _currentGroupKey = groupByKey;
        }
    
        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (column < Aggregators.Length) {
                return _currentAggregatorMethods[column].Value;
            }
            else {
                var pair = Accessors[column - Aggregators.Length];
                return pair.Accessor.GetValue(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }
    
        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                var pair = Accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                AggregationAccessorSlotPair pair = Accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableScalar(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public override EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                var pair = Accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(_currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public override void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            // not applicable
        }
    
        public void InternalHandleGroupUpdate(Object groupByKey, AggregationMethodPairRow row) {
            // no action required
        }
    
        public void InternalHandleGroupRemove(Object groupByKey) {
            // no action required
        }
    
        public override void Accept(AggregationServiceVisitor visitor) {
            visitor.VisitAggregations(AggregatorsPerGroup.Count, AggregatorsPerGroup);
        }
    
        public override void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor) {
            visitor.VisitGrouped(AggregatorsPerGroup.Count);
            foreach (var entry in AggregatorsPerGroup) {
                visitor.VisitGroup(entry.Key, entry.Value);
            }
        }

        public override bool IsGrouped
        {
            get { return true; }
        }

        protected void HandleRemovedKeys() {
            if (RemovedKeys.IsNotEmpty())     // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
            {
                foreach (var removedKey in RemovedKeys)
                {
                    AggregatorsPerGroup.Remove(removedKey);
                    InternalHandleGroupRemove(removedKey);
                }
                RemovedKeys.Clear();
            }
        }
    
        public override Object GetGroupKey(int agentInstanceId) {
            return _currentGroupKey;
        }
    
        public override ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
            return AggregatorsPerGroup.Keys;
        }
    }
}

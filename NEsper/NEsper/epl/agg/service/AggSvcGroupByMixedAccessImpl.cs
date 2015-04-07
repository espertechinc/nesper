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
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>Implementation for handling aggregation with grouping by group-keys. </summary>
    public class AggSvcGroupByMixedAccessImpl : AggregationServiceBaseGrouped
    {
        private readonly AggregationAccessorSlotPair[] _accessorsFactory;
        protected readonly AggregationStateFactory[] AccessAggregations;
        protected readonly bool IsJoin;
    
        // maintain for each group a row of aggregator states that the expression node canb pull the data from via index
        protected IDictionary<Object, AggregationRowPair> AggregatorsPerGroup;
    
        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        private AggregationRowPair _currentAggregatorRow;
        private Object _currentGroupKey;
    
        private readonly MethodResolutionService _methodResolutionService;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="prototypes">collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregationsaggregation states for each group</param>
        /// <param name="groupKeyBinding">The group key binding.</param>
        /// <param name="methodResolutionService">factory for creating additional aggregation method instances per group key</param>
        /// <param name="accessorsFactory">accessor definitions</param>
        /// <param name="accessAggregations">access aggs</param>
        /// <param name="isJoin">true for join, false for single-stream</param>
        public AggSvcGroupByMixedAccessImpl(ExprEvaluator[] evaluators,
                                            AggregationMethodFactory[] prototypes,
                                            Object groupKeyBinding,
                                            MethodResolutionService methodResolutionService,
                                            AggregationAccessorSlotPair[] accessorsFactory,
                                            AggregationStateFactory[] accessAggregations,
                                            bool isJoin)

                    : base(evaluators, prototypes, groupKeyBinding)
        {
            _accessorsFactory = accessorsFactory;
            AccessAggregations = accessAggregations;
            IsJoin = isJoin;
            _methodResolutionService = methodResolutionService;
            AggregatorsPerGroup = new Dictionary<Object, AggregationRowPair>();
        }
    
        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregatorsPerGroup.Clear();
        }
    
        public override void ApplyEnter(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, Aggregators.Length, AccessAggregations.Length, groupByKey);}
            AggregationRowPair groupAggregators = AggregatorsPerGroup.Get(groupByKey);
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationState[] states;

            if (groupAggregators == null)
            {
                AggregationMethod[] methods = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                states = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, AccessAggregations, groupByKey, GroupKeyBinding, null);
                groupAggregators = new AggregationRowPair(methods, states);
                AggregatorsPerGroup.Put(groupByKey, groupAggregators);
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorRow = groupAggregators;
            AggregationMethod[] groupAggMethods = groupAggregators.Methods;
            for (int i = 0; i < Evaluators.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, groupAggMethods[i], Aggregators[i].AggregationExpression); }
                Object columnResult = Evaluators[i].Evaluate(evaluateParams);
                groupAggMethods[i].Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, groupAggMethods[i]);}
            }
    
            states = _currentAggregatorRow.States;
            for (int i = 0; i < states.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, states[i], AccessAggregations[i].AggregationExpression); }
                states[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, states[i]);}
            }
    
            InternalHandleUpdated(groupByKey, groupAggregators);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true);}
        }
    
        public override void ApplyLeave(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, Aggregators.Length, AccessAggregations.Length, groupByKey);}
            AggregationRowPair groupAggregators = AggregatorsPerGroup.Get(groupByKey);
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationState[] states;

            if (groupAggregators == null)
            {
                AggregationMethod[] methods = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                states = _methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, AccessAggregations, groupByKey, GroupKeyBinding, null);
                groupAggregators = new AggregationRowPair(methods, states);
                AggregatorsPerGroup.Put(groupByKey, groupAggregators);
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorRow = groupAggregators;
            AggregationMethod[] groupAggMethods = groupAggregators.Methods;
            for (int i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, groupAggMethods[i], Aggregators[i].AggregationExpression); }
                Object columnResult = Evaluators[i].Evaluate(evaluateParams);
                groupAggMethods[i].Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, groupAggMethods[i]);}
            }
    
            states = _currentAggregatorRow.States;
            for (int i = 0; i < states.Length; i++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, states[i], AccessAggregations[i].AggregationExpression); }
                states[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, states[i]);}
            }
    
            InternalHandleUpdated(groupByKey, groupAggregators);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false);}
        }

        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            _currentAggregatorRow = AggregatorsPerGroup.Get(groupByKey);
            _currentGroupKey = groupByKey;
    
            if (_currentAggregatorRow == null)
            {
                AggregationMethod[] methods = _methodResolutionService.NewAggregators(Aggregators, agentInstanceId, groupByKey, GroupKeyBinding, null);
                AggregationState[] states = _methodResolutionService.NewAccesses(agentInstanceId, IsJoin, AccessAggregations, groupByKey, GroupKeyBinding, null);
                _currentAggregatorRow = new AggregationRowPair(methods, states);
                AggregatorsPerGroup.Put(groupByKey, _currentAggregatorRow);
            }
        }
    
        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (column < Aggregators.Length) {
                return _currentAggregatorRow.Methods[column].Value;
            }
            else {
                AggregationAccessorSlotPair pair = _accessorsFactory[column - Aggregators.Length];
                return pair.Accessor.GetValue(_currentAggregatorRow.States[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }
        
        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                AggregationAccessorSlotPair pair = _accessorsFactory[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(_currentAggregatorRow.States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override ICollection<Object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (column < Aggregators.Length)
            {
                return null;
            }
            else
            {
                AggregationAccessorSlotPair pair = _accessorsFactory[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableScalar(_currentAggregatorRow.States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public override EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            if (column < Aggregators.Length) {
                return null;
            }
            else {
                AggregationAccessorSlotPair pair = _accessorsFactory[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(_currentAggregatorRow.States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }
    
        public override void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            // not applicable
        }
    
        public void InternalHandleUpdated(Object groupByKey, AggregationRowPair groupAggregators) {
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

        public override Object GetGroupKey(int agentInstanceId) {
            return _currentGroupKey;
        }
    
        public override ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
            return AggregatorsPerGroup.Keys;
        }
    }
}

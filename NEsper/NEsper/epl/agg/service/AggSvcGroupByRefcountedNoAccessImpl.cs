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
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByRefcountedNoAccessImpl : AggregationServiceBaseGrouped
    {
        // maintain for each group a row of aggregator states that the expression node canb pull the data from via index
        protected IDictionary<Object, AggregationMethodRow> AggregatorsPerGroup;
    
        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        private AggregationMethod[] _currentAggregatorRow;
        private Object _currentGroupKey;
    
        private readonly MethodResolutionService _methodResolutionService;
    
        private readonly IList<Object> _removedKeys;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="prototypes">collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregationsaggregation states for each group</param>
        /// <param name="groupKeyBinding">The group key binding.</param>
        /// <param name="methodResolutionService">factory for creating additional aggregation method instances per group key</param>
        public AggSvcGroupByRefcountedNoAccessImpl(ExprEvaluator[] evaluators,
                                           AggregationMethodFactory[] prototypes,
                                           Object groupKeyBinding,
                                           MethodResolutionService methodResolutionService)
            : base(evaluators, prototypes, groupKeyBinding)
        {
            _methodResolutionService = methodResolutionService;
            AggregatorsPerGroup = new Dictionary<Object, AggregationMethodRow>();
            _removedKeys = new List<Object>();
        }
    
        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregatorsPerGroup.Clear();
        }
    
        public override void ApplyEnter(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, Aggregators.Length, 0, groupByKey);}
            HandleRemovedKeys();
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationMethodRow row;
            AggregationMethod[] groupAggregators;
            if (!AggregatorsPerGroup.TryGetValue(groupByKey, out row))
            {
                groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                row = new AggregationMethodRow(_methodResolutionService.GetCurrentRowCount(groupAggregators, null) + 1, groupAggregators);
                AggregatorsPerGroup[groupByKey] = row;
            }
            else
            {
                groupAggregators = row.Methods;
                row.IncreaseRefcount();
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorRow = groupAggregators;

            var evaluators = Evaluators;
            var evaluatorsLength = evaluators.Length;

            for (var ii = 0; ii < evaluatorsLength; ii++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, ii, groupAggregators[ii], Aggregators[ii].AggregationExpression); }
                groupAggregators[ii].Enter(evaluators[ii].Evaluate(evaluateParams));
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, ii, groupAggregators[ii]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true);}
        }
    
        public override void ApplyLeave(EventBean[] eventsPerStream, Object groupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, Aggregators.Length, 0, groupByKey);}
            var row = AggregatorsPerGroup.Get(groupByKey);
    
            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationMethod[] groupAggregators;
            if (row != null)
            {
                groupAggregators = row.Methods;
            }
            else
            {
                groupAggregators = _methodResolutionService.NewAggregators(Aggregators, exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);
                row = new AggregationMethodRow(_methodResolutionService.GetCurrentRowCount(groupAggregators, null) + 1, groupAggregators);
                AggregatorsPerGroup[groupByKey] = row;
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorRow = groupAggregators;

            var evaluators = Evaluators;
            var evaluatorsLength = evaluators.Length;

            for (var ii = 0; ii < evaluatorsLength; ii++) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, ii, groupAggregators[ii], Aggregators[ii].AggregationExpression); }
                groupAggregators[ii].Leave(evaluators[ii].Evaluate(evaluateParams));
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, ii, groupAggregators[ii]);}
            }
    
            row.DecreaseRefcount();
            if (row.Refcount <= 0)
            {
                _removedKeys.Add(groupByKey);
                _methodResolutionService.RemoveAggregators(exprEvaluatorContext.AgentInstanceId, groupByKey, GroupKeyBinding, null);  // allow persistence to remove keys already
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false);}
        }

        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            var row = AggregatorsPerGroup.Get(groupByKey);
    
            if (row != null) {
                _currentAggregatorRow = row.Methods;
            }
            else {
                _currentAggregatorRow = null;
            }
    
            if (_currentAggregatorRow == null) {
                _currentAggregatorRow = _methodResolutionService.NewAggregators(Aggregators, agentInstanceId, groupByKey, GroupKeyBinding, null);
            }
            _currentGroupKey = groupByKey;
        }
    
        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return _currentAggregatorRow[column].Value;
        }
    
        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }

        public override ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }
    
        public override EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
            return null;
        }
    
        public override void SetRemovedCallback(AggregationRowRemovedCallback callback) {
            // not applicable
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
    
        protected void HandleRemovedKeys() {
            if (_removedKeys.IsNotEmpty())     // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
            {
                foreach (var removedKey in _removedKeys)
                {
                    AggregatorsPerGroup.Remove(removedKey);
                }
                _removedKeys.Clear();
            }
        }
    
        public override ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
            HandleRemovedKeys();
            return AggregatorsPerGroup.Keys;
        }
    }
}

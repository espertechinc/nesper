///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByNoAccessImpl : AggregationServiceBaseGrouped
    {
        // maintain for each group a row of aggregator states that the expression node canb pull the data from via index
        private readonly IDictionary<Object, AggregationMethod[]> _aggregatorsPerGroup;

        // maintain a current row for random access into the aggregator state table
        // (row=groups, columns=expression nodes that have aggregation functions)
        private AggregationMethod[] _currentAggregatorRow;
        private Object _currentGroupKey;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="prototypes">collect the aggregation state that evaluators evaluate to, act as prototypes for new aggregationsaggregation states for each group</param>
        public AggSvcGroupByNoAccessImpl(ExprEvaluator[] evaluators, AggregationMethodFactory[] prototypes)
            : base(evaluators, prototypes)
        {
            _aggregatorsPerGroup = new Dictionary<Object, AggregationMethod[]>();
        }

        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            _aggregatorsPerGroup.Clear();
        }

        public override void ApplyEnter(
            EventBean[] eventsPerStream,
            Object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, Aggregators.Length, 0, groupByKey); }
            var groupAggregators = _aggregatorsPerGroup.Get(groupByKey);

            // The aggregators for this group do not exist, need to create them from the prototypes
            if (groupAggregators == null)
            {
                groupAggregators = AggSvcGroupByUtil.NewAggregators(Aggregators);
                _aggregatorsPerGroup.Put(groupByKey, groupAggregators);
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorRow = groupAggregators;
            for (var i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, groupAggregators[i], Aggregators[i].AggregationExpression); }
                var columnResult = Evaluators[i].Evaluate(evaluateParams);
                groupAggregators[i].Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, groupAggregators[i]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true); }
        }

        public override void ApplyLeave(
            EventBean[] eventsPerStream,
            Object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, Aggregators.Length, 0, groupByKey); }
            var groupAggregators = _aggregatorsPerGroup.Get(groupByKey);

            // The aggregators for this group do not exist, need to create them from the prototypes
            if (groupAggregators == null)
            {
                groupAggregators = AggSvcGroupByUtil.NewAggregators(Aggregators);
                _aggregatorsPerGroup.Put(groupByKey, groupAggregators);
            }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorRow = groupAggregators;
            for (var i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, groupAggregators[i], Aggregators[i].AggregationExpression); }
                var columnResult = Evaluators[i].Evaluate(evaluateParams);
                groupAggregators[i].Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, groupAggregators[i]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false); }
        }

        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            _currentAggregatorRow = _aggregatorsPerGroup.Get(groupByKey);
            _currentGroupKey = groupByKey;

            if (_currentAggregatorRow == null)
            {
                _currentAggregatorRow = AggSvcGroupByUtil.NewAggregators(Aggregators);
                _aggregatorsPerGroup.Put(groupByKey, _currentAggregatorRow);
            }
        }

        public override object GetValue(int column, int agentInstanceId, EvaluateParams evaluateParams)
        {
            return _currentAggregatorRow[column].Value;
        }

        public override ICollection<EventBean> GetCollectionOfEvents(int column, EvaluateParams evaluateParams)
        {
            return null;
        }

        public override EventBean GetEventBean(int column, EvaluateParams evaluateParams)
        {
            return null;
        }

        public override ICollection<object> GetCollectionScalar(int column, EvaluateParams evaluateParams)
        {
            return null;
        }

        public override void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public override void Accept(AggregationServiceVisitor visitor)
        {
            visitor.VisitAggregations(_aggregatorsPerGroup.Count, _aggregatorsPerGroup);
        }

        public override void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
            visitor.VisitGrouped(_aggregatorsPerGroup.Count);
            foreach (var entry in _aggregatorsPerGroup)
            {
                visitor.VisitGroup(entry.Key, entry.Value);
            }
        }

        public override bool IsGrouped
        {
            get { return true; }
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
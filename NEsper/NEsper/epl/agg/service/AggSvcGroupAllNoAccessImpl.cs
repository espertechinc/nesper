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
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllNoAccessImpl : AggregationServiceBaseUngrouped
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="evaluators">evaluate the sub-expression within the aggregate function (ie. Sum(4*myNum))</param>
        /// <param name="aggregators">collect the aggregation state that evaluators evaluate to</param>
        /// <param name="aggregatorFactories">The aggregator factories.</param>
        public AggSvcGroupAllNoAccessImpl(
            ExprEvaluator[] evaluators,
            AggregationMethod[] aggregators,
            AggregationMethodFactory[] aggregatorFactories)
            : base(evaluators, aggregators, aggregatorFactories, new AggregationStateFactory[0])
        {
        }

        public override void ApplyEnter(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var aggregators = base.Aggregators;
            var aggregatorFactories = base.AggregatorFactories;

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(true, Evaluators.Length, 0); }
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            for (int j = 0; j < Evaluators.Length; j++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, j, aggregators[j], aggregatorFactories[j].AggregationExpression); }
                var columnResult = Evaluators[j].Evaluate(evaluateParams);
                aggregators[j].Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, j, aggregators[j]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(true); }
        }

        public override void ApplyLeave(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var aggregators = base.Aggregators;
            var aggregatorFactories = base.AggregatorFactories;

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(false, Evaluators.Length, 0); }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);
            for (int j = 0; j < Evaluators.Length; j++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, j, aggregators[j], aggregatorFactories[j].AggregationExpression); }
                var columnResult = Evaluators[j].Evaluate(evaluateParams);
                aggregators[j].Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, j, aggregators[j]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(false); }
        }

        public override void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            // no action needed - this implementation does not group and the current row is the single group
        }

        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            return base.Aggregators[column].Value;
        }

        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public override ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public override EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            return null;
        }

        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (AggregationMethod aggregator in base.Aggregators)
            {
                aggregator.Clear();
            }
        }

        public override void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public override void Accept(AggregationServiceVisitor visitor)
        {
            visitor.VisitAggregations(1, base.Aggregators);
        }

        public override void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
        }

        public override bool IsGrouped
        {
            get { return false; }
        }

        public override Object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public override ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
        }
    }
}
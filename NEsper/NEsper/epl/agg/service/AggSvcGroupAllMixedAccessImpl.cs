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
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllMixedAccessImpl : AggregationServiceBaseUngrouped
    {
        private readonly AggregationAccessorSlotPair[] _accessors;
        protected AggregationState[] States;

        public AggSvcGroupAllMixedAccessImpl(
            ExprEvaluator[] evaluators,
            AggregationMethod[] aggregators,
            AggregationAccessorSlotPair[] accessors,
            AggregationState[] states,
            AggregationMethodFactory[] aggregatorFactories,
            AggregationStateFactory[] accessAggregations)
            : base(evaluators, aggregators, aggregatorFactories, accessAggregations)
        {
            _accessors = accessors;
            States = states;
        }

        public override void ApplyEnter(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var aggregators = base.Aggregators;
            var accessAggregations = base.AccessAggregations;
            var aggregatorFactories = base.AggregatorFactories;

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(true, Evaluators.Length, accessAggregations.Length); }

            for (int i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, aggregators[i], aggregatorFactories[i].AggregationExpression); }
                Object columnResult =
                    Evaluators[i].Evaluate(new EvaluateParams(eventsPerStream, true, exprEvaluatorContext));
                aggregators[i].Enter(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, aggregators[i]); }
            }

            for (int i = 0; i < States.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, States[i], accessAggregations[i].AggregationExpression); }
                States[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, States[i]); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(true); }
        }

        public override void ApplyLeave(
            EventBean[] eventsPerStream,
            Object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var aggregators = base.Aggregators;
            var accessAggregations = base.AccessAggregations;
            var aggregatorFactories = base.AggregatorFactories;

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(false, Evaluators.Length, accessAggregations.Length); }

            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);
            for (int i = 0; i < Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, aggregators[i], aggregatorFactories[i].AggregationExpression); }
                var columnResult = Evaluators[i].Evaluate(evaluateParams);
                aggregators[i].Leave(columnResult);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, aggregators[i]); }
            }

            for (int i = 0; i < States.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, States[i], accessAggregations[i].AggregationExpression); }
                States[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, States[i]); }
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(false); }
        }

        public override void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            // no action needed - this implementation does not group and the current row is the single group
        }

        public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var aggregators = base.Aggregators;
            if (column < aggregators.Length)
            {
                return aggregators[column].Value;
            }
            else
            {
                AggregationAccessorSlotPair pair = _accessors[column - aggregators.Length];
                return pair.Accessor.GetValue(States[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        public override ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            var aggregators = base.Aggregators;
            if (column < aggregators.Length)
            {
                return null;
            }
            else
            {
                AggregationAccessorSlotPair pair = _accessors[column - aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (column < Aggregators.Length)
            {
                return null;
            }
            else
            {
                AggregationAccessorSlotPair pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableScalar(States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            if (column < Aggregators.Length)
            {
                return null;
            }
            else
            {
                AggregationAccessorSlotPair pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(States[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (AggregationState state in States)
            {
                state.Clear();
            }
            foreach (AggregationMethod aggregator in Aggregators)
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
            visitor.VisitAggregations(1, States, base.Aggregators);
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
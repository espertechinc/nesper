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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Aggregation service for use when only first/last/window aggregation functions are used an none other.
    /// </summary>
    public class AggSvcGroupAllAccessOnlyImpl
        : AggregationService
            ,
            AggregationResultFuture
    {
        private readonly AggregationState[] _states;
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly AggregationStateFactory[] _accessAggSpecs;

        public AggSvcGroupAllAccessOnlyImpl(
            AggregationAccessorSlotPair[] accessors,
            AggregationState[] states,
            AggregationStateFactory[] accessAggSpecs)
        {
            _accessors = accessors;
            _states = states;
            _accessAggSpecs = accessAggSpecs;
        }

        public void ApplyEnter(EventBean[] eventsPerStream, Object groupKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(true, 0, _accessAggSpecs.Length);
            }
            for (int i = 0; i < _states.Length; i++)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get()
                        .QAggAccessEnterLeave(true, i, _states[i], _accessAggSpecs[i].AggregationExpression);
                }
                _states[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, _states[i]);
                }
            }
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(true);
            }
        }

        public void ApplyLeave(EventBean[] eventsPerStream, Object groupKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QAggregationUngroupedApplyEnterLeave(false, 0, _accessAggSpecs.Length);
            }
            for (int i = 0; i < _states.Length; i++)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get()
                        .QAggAccessEnterLeave(false, i, _states[i], _accessAggSpecs[i].AggregationExpression);
                }
                _states[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, _states[i]);
                }
            }
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AAggregationUngroupedApplyEnterLeave(false);
            }
        }

        public void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            // no implementation required
        }

        public Object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetValue(_states[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetEnumerableEvent(_states[pair.Slot], eventsPerStream, isNewData, context);
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetEnumerableEvents(_states[pair.Slot], eventsPerStream, isNewData, context);
        }

        public ICollection<Object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetEnumerableScalar(_states[pair.Slot], eventsPerStream, isNewData, context);
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (AggregationState state in _states)
            {
                state.Clear();
            }
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public void Accept(AggregationServiceVisitor visitor)
        {
            visitor.VisitAggregations(1, _states);
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
        }

        public bool IsGrouped
        {
            get { return false; }
        }

        public Object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return null;
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

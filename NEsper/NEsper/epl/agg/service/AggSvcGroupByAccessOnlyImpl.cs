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
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Aggregation service for use when only first/last/window aggregation functions are used an none other.
    /// </summary>
    public class AggSvcGroupByAccessOnlyImpl : AggregationService, AggregationResultFuture
    {
        private readonly IDictionary<Object, AggregationState[]> _accessMap;
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly AggregationStateFactory[] _accessAggSpecs;
        private readonly bool _isJoin;

        private AggregationState[] _currentAccesses;
        private Object _currentGroupKey;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="accessors">accessor definitions</param>
        /// <param name="accessAggSpecs">access agg specs</param>
        /// <param name="isJoin">true for join, false for single-stream</param>
        public AggSvcGroupByAccessOnlyImpl(AggregationAccessorSlotPair[] accessors, AggregationStateFactory[] accessAggSpecs, bool isJoin)
        {
            _accessMap = new Dictionary<Object, AggregationState[]>();
            _accessors = accessors;
            _accessAggSpecs = accessAggSpecs;
            _isJoin = isJoin;
        }

        public void ApplyEnter(EventBean[] eventsPerStream, Object groupKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, 0, _accessAggSpecs.Length, groupKey); }
            AggregationState[] row = GetAssertRow(exprEvaluatorContext.AgentInstanceId, groupKey);
            for (int i = 0; i < row.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, row[i], _accessAggSpecs[i].AggregationExpression); }
                row[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, row[i]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true); }
        }

        public void ApplyLeave(EventBean[] eventsPerStream, Object groupKey, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, 0, _accessAggSpecs.Length, groupKey); }
            AggregationState[] row = GetAssertRow(exprEvaluatorContext.AgentInstanceId, groupKey);
            for (int i = 0; i < row.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, row[i], _accessAggSpecs[i].AggregationExpression); }
                row[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, row[i]); }
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false); }
        }

        public void SetCurrentAccess(Object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            _currentAccesses = GetAssertRow(agentInstanceId, groupKey);
            _currentGroupKey = groupKey;
        }

        public object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetValue(_currentAccesses[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
        }

        public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetEnumerableEvents(_currentAccesses[pair.Slot], eventsPerStream, isNewData, context);
        }

        public ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetEnumerableScalar(_currentAccesses[pair.Slot], eventsPerStream, isNewData, context);
        }

        public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context)
        {
            AggregationAccessorSlotPair pair = _accessors[column];
            return pair.Accessor.GetEnumerableEvent(_currentAccesses[pair.Slot], eventsPerStream, isNewData, context);
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            _accessMap.Clear();
        }

        private AggregationState[] GetAssertRow(int agentInstanceId, Object groupKey)
        {
            AggregationState[] row = _accessMap.Get(groupKey);
            if (row != null)
            {
                return row;
            }

            row = AggSvcGroupByUtil.NewAccesses(agentInstanceId, _isJoin, _accessAggSpecs, groupKey, null);
            _accessMap.Put(groupKey, row);
            return row;
        }

        public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public void Accept(AggregationServiceVisitor visitor)
        {
            visitor.VisitAggregations(_accessMap.Count, _accessMap);
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
            visitor.VisitGrouped(_accessMap.Count);
            foreach (var entry in _accessMap)
            {
                visitor.VisitGroup(entry.Key, entry.Value);
            }
        }

        public bool IsGrouped
        {
            get { return true; }
        }

        public Object GetGroupKey(int agentInstanceId)
        {
            return _currentGroupKey;
        }

        public ICollection<Object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            return _accessMap.Keys;
        }

        public void Stop()
        {
        }

        public AggregationService GetContextPartitionAggregationService(int agentInstanceId)
        {
            return this;
        }
    }
}

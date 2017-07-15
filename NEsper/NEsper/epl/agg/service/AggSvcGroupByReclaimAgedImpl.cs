///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
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

        public static readonly long DEFAULT_MAX_AGE_MSEC = 60000L;

        private readonly AggregationStateFactory[] _accessAggregations;
        private readonly bool _isJoin;
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly TimeAbacus _timeAbacus;

        private readonly AggSvcGroupByReclaimAgedEvalFunc _evaluationFunctionMaxAge;
        private readonly AggSvcGroupByReclaimAgedEvalFunc _evaluationFunctionFrequency;

        // maintain for each group a row of aggregator states that the expression node canb pull the data from via index
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

        public AggSvcGroupByReclaimAgedImpl(
            ExprEvaluator[] evaluators,
            AggregationMethodFactory[] aggregators,
            AggregationAccessorSlotPair[] accessors,
            AggregationStateFactory[] accessAggregations,
            bool join,
            AggSvcGroupByReclaimAgedEvalFunc evaluationFunctionMaxAge,
            AggSvcGroupByReclaimAgedEvalFunc evaluationFunctionFrequency,
            TimeAbacus timeAbacus)
            : base(evaluators, aggregators)
        {
            _accessors = accessors;
            _accessAggregations = accessAggregations;
            _isJoin = join;
            _evaluationFunctionMaxAge = evaluationFunctionMaxAge;
            _evaluationFunctionFrequency = evaluationFunctionFrequency;
            _aggregatorsPerGroup = new Dictionary<Object, AggregationMethodRowAged>();
            _timeAbacus = timeAbacus;
            _removedKeys = new List<Object>();
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
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, base.Aggregators.Length, _accessAggregations.Length, groupByKey);
            }
            var currentTime = exprEvaluatorContext.TimeProvider.Time;
            if ((_nextSweepTime == null) || (_nextSweepTime <= currentTime))
            {
                _currentMaxAge = GetMaxAge(_currentMaxAge);
                _currentReclaimFrequency = GetReclaimFrequency(_currentReclaimFrequency);
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
                {
                    Log.Debug(
                        "Reclaiming groups older then " + _currentMaxAge + " msec and every " + _currentReclaimFrequency +
                        "msec in frequency");
                }
                _nextSweepTime = currentTime + _currentReclaimFrequency;
                Sweep(currentTime, _currentMaxAge);
            }

            HandleRemovedKeys();
                // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily

            var row = _aggregatorsPerGroup.Get(groupByKey);

            // The aggregators for this group do not exist, need to create them from the prototypes
            AggregationMethod[] groupAggregators;
            AggregationState[] groupStates;
            if (row == null)
            {
                groupAggregators = AggSvcGroupByUtil.NewAggregators(base.Aggregators);
                groupStates = AggSvcGroupByUtil.NewAccesses(
                    exprEvaluatorContext.AgentInstanceId, _isJoin, _accessAggregations, groupByKey, null);
                row = new AggregationMethodRowAged(1, currentTime, groupAggregators, groupStates);
                _aggregatorsPerGroup.Put(groupByKey, row);
            }
            else
            {
                groupAggregators = row.Methods;
                groupStates = row.States;
                row.IncreaseRefcount();
                row.LastUpdateTime = currentTime;
            }

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorMethods = groupAggregators;
            _currentAggregatorStates = groupStates;
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            for (var i = 0; i < base.Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, _currentAggregatorMethods[i], base.Aggregators[i].AggregationExpression);
                }
                var columnResult = base.Evaluators[i].Evaluate(evaluateParams);
                groupAggregators[i].Enter(columnResult);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, _currentAggregatorMethods[i]);
                }
            }

            for (var i = 0; i < _currentAggregatorStates.Length; i++)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, _currentAggregatorStates[i], _accessAggregations[i].AggregationExpression);
                }
                _currentAggregatorStates[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, _currentAggregatorStates[i]);
                }
            }

            InternalHandleUpdated(groupByKey, row);
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true);
            }
        }

        private void Sweep(long currentTime, long currentMaxAge)
        {
            var removed = new ArrayDeque<Object>();
            foreach (var entry in _aggregatorsPerGroup)
            {
                var age = currentTime - entry.Value.LastUpdateTime;
                if (age > currentMaxAge)
                {
                    removed.Add(entry.Key);
                }
            }

            foreach (var key in removed)
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
            return _timeAbacus.DeltaForSecondsDouble(maxAge.Value);
        }

        private long GetReclaimFrequency(long currentReclaimFrequency)
        {
            var frequency = _evaluationFunctionFrequency.LongValue;
            if ((frequency == null) || (frequency <= 0))
            {
                return currentReclaimFrequency;
            }
            return _timeAbacus.DeltaForSecondsDouble(frequency.Value);
        }

        public override void ApplyLeave(
            EventBean[] eventsPerStream,
            Object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, base.Aggregators.Length, _accessAggregations.Length, groupByKey);
            }
            var row = _aggregatorsPerGroup.Get(groupByKey);
            var currentTime = exprEvaluatorContext.TimeProvider.Time;

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
                groupAggregators = AggSvcGroupByUtil.NewAggregators(base.Aggregators);
                groupStates = AggSvcGroupByUtil.NewAccesses(
                    exprEvaluatorContext.AgentInstanceId, _isJoin, _accessAggregations, groupByKey, null);
                row = new AggregationMethodRowAged(1, currentTime, groupAggregators, groupStates);
                _aggregatorsPerGroup.Put(groupByKey, row);
            }

            // For this row, evaluate sub-expressions, enter result
            _currentAggregatorMethods = groupAggregators;
            _currentAggregatorStates = groupStates;
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);
            for (var i = 0; i < base.Evaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, _currentAggregatorMethods[i], base.Aggregators[i].AggregationExpression);
                }
                var columnResult = base.Evaluators[i].Evaluate(evaluateParams);
                groupAggregators[i].Leave(columnResult);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, _currentAggregatorMethods[i]);
                }
            }

            for (var i = 0; i < _currentAggregatorStates.Length; i++)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, _currentAggregatorStates[i], _accessAggregations[i].AggregationExpression);
                }
                _currentAggregatorStates[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, _currentAggregatorStates[i]);
                }
            }

            row.DecreaseRefcount();
            row.LastUpdateTime = currentTime;
            if (row.Refcount <= 0)
            {
                _removedKeys.Add(groupByKey);
            }
            InternalHandleUpdated(groupByKey, row);
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false);
            }
        }

        public override void SetCurrentAccess(Object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
        {
            var row = _aggregatorsPerGroup.Get(groupByKey);

            if (row != null)
            {
                _currentAggregatorMethods = row.Methods;
                _currentAggregatorStates = row.States;
            }
            else
            {
                _currentAggregatorMethods = null;
            }

            if (_currentAggregatorMethods == null)
            {
                _currentAggregatorMethods = AggSvcGroupByUtil.NewAggregators(base.Aggregators);
                _currentAggregatorStates = AggSvcGroupByUtil.NewAccesses(
                    agentInstanceId, _isJoin, _accessAggregations, groupByKey, null);
            }

            _currentGroupKey = groupByKey;
        }

        public override Object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (column < base.Aggregators.Length)
            {
                return _currentAggregatorMethods[column].Value;
            }
            else
            {
                var pair = _accessors[column - base.Aggregators.Length];
                return pair.Accessor.GetValue(
                    _currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
            }
        }

        public override ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (column < base.Aggregators.Length)
            {
                return null;
            }
            else
            {
                var pair = _accessors[column - base.Aggregators.Length];
                return pair.Accessor.GetEnumerableEvents(
                    _currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override ICollection<Object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (column < base.Aggregators.Length)
            {
                return null;
            }
            else
            {
                var pair = _accessors[column - base.Aggregators.Length];
                return pair.Accessor.GetEnumerableScalar(
                    _currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            if (column < Aggregators.Length)
            {
                return null;
            }
            else
            {
                var pair = _accessors[column - Aggregators.Length];
                return pair.Accessor.GetEnumerableEvent(
                    _currentAggregatorStates[pair.Slot], eventsPerStream, isNewData, context);
            }
        }

        public override void SetRemovedCallback(AggregationRowRemovedCallback value)
        {
            _removedCallback = value;
        }

        public void InternalHandleUpdated(Object groupByKey, AggregationMethodRowAged row)
        {
            // no action required
        }

        public void InternalHandleRemoved(Object key)
        {
            // no action required
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

        protected void HandleRemovedKeys()
        {
            if (!_removedKeys.IsEmpty())
            {
                // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
                foreach (var removedKey in _removedKeys)
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
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public abstract class AggSvcGroupLocalGroupByBase : AggregationService
    {
        protected internal readonly bool IsJoin;
        protected internal readonly AggregationLocalGroupByPlan LocalGroupByPlan;

        public abstract object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        public abstract void SetCurrentAccess(object groupKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel);

        // state

        protected abstract object ComputeGroupKey(
            AggregationLocalGroupByLevel level,
            object groupKey,
            ExprEvaluator[] partitionEval,
            EventBean[] eventsPerStream,
            bool newData,
            ExprEvaluatorContext exprEvaluatorContext);

        protected AggSvcGroupLocalGroupByBase(bool isJoin, AggregationLocalGroupByPlan localGroupByPlan)
        {
            IsJoin = isJoin;
            LocalGroupByPlan = localGroupByPlan;

            AggregatorsPerLevelAndGroup = new IDictionary<object, AggregationMethodPairRow>[localGroupByPlan.AllLevels.Length];
            for (var i = 0; i < localGroupByPlan.AllLevels.Length; i++)
            {
                AggregatorsPerLevelAndGroup[i] = new Dictionary<object, AggregationMethodPairRow>();
            }
            RemovedKeys = new List<Pair<int, object>>();
        }

        public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
        {
            ClearResults(AggregatorsPerLevelAndGroup, AggregatorsTopLevel, StatesTopLevel);
        }

        public void ApplyEnter(
            EventBean[] eventsPerStream,
            object groupByKeyProvided,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, LocalGroupByPlan.NumMethods, LocalGroupByPlan.NumAccess, groupByKeyProvided); }

            HandleRemovedKeys();

            if (LocalGroupByPlan.OptionalLevelTop != null)
            {
                if (AggregatorsTopLevel == null)
                {
                    AggregatorsTopLevel = AggSvcGroupByUtil.NewAggregators(LocalGroupByPlan.OptionalLevelTop.MethodFactories);
                    StatesTopLevel = AggSvcGroupByUtil.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, LocalGroupByPlan.OptionalLevelTop.StateFactories, null, null);
                }
                AggregateIntoEnter(
                    LocalGroupByPlan.OptionalLevelTop, AggregatorsTopLevel, StatesTopLevel, eventsPerStream,
                    exprEvaluatorContext);
                InternalHandleUpdatedTop();
            }

            for (var levelNum = 0; levelNum < LocalGroupByPlan.AllLevels.Length; levelNum++)
            {
                var level = LocalGroupByPlan.AllLevels[levelNum];
                var partitionEval = level.PartitionEvaluators;
                var groupByKey = ComputeGroupKey(
                    level, groupByKeyProvided, partitionEval, eventsPerStream, true, exprEvaluatorContext);
                var row = AggregatorsPerLevelAndGroup[levelNum].Get(groupByKey);
                if (row == null)
                {
                    var rowAggregators = AggSvcGroupByUtil.NewAggregators(level.MethodFactories);
                    AggregationState[] rowStates = AggSvcGroupByUtil.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, level.StateFactories, groupByKey, null);
                    row = new AggregationMethodPairRow(1, rowAggregators, rowStates);
                    AggregatorsPerLevelAndGroup[levelNum].Put(groupByKey, row);
                }
                else
                {
                    row.IncreaseRefcount();
                }

                AggregateIntoEnter(level, row.Methods, row.States, eventsPerStream, exprEvaluatorContext);
                InternalHandleUpdatedGroup(levelNum, groupByKey, row);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true); }
        }

        public void ApplyLeave(
            EventBean[] eventsPerStream,
            object groupByKeyProvided,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, LocalGroupByPlan.NumMethods, LocalGroupByPlan.NumAccess, groupByKeyProvided); }

            if (LocalGroupByPlan.OptionalLevelTop != null)
            {
                if (AggregatorsTopLevel == null)
                {
                    AggregatorsTopLevel = AggSvcGroupByUtil.NewAggregators(LocalGroupByPlan.OptionalLevelTop.MethodFactories);
                    StatesTopLevel = AggSvcGroupByUtil.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, LocalGroupByPlan.OptionalLevelTop.StateFactories, null, null);
                }
                AggregateIntoLeave(
                    LocalGroupByPlan.OptionalLevelTop, AggregatorsTopLevel, StatesTopLevel, eventsPerStream,
                    exprEvaluatorContext);
                InternalHandleUpdatedTop();
            }

            for (var levelNum = 0; levelNum < LocalGroupByPlan.AllLevels.Length; levelNum++)
            {
                var level = LocalGroupByPlan.AllLevels[levelNum];
                var partitionEval = level.PartitionEvaluators;
                var groupByKey = ComputeGroupKey(
                    level, groupByKeyProvided, partitionEval, eventsPerStream, true, exprEvaluatorContext);
                var row = AggregatorsPerLevelAndGroup[levelNum].Get(groupByKey);
                if (row == null)
                {
                    var rowAggregators = AggSvcGroupByUtil.NewAggregators(level.MethodFactories);
                    AggregationState[] rowStates = AggSvcGroupByUtil.NewAccesses(exprEvaluatorContext.AgentInstanceId, IsJoin, level.StateFactories, groupByKey, null);
                    row = new AggregationMethodPairRow(1, rowAggregators, rowStates);
                    AggregatorsPerLevelAndGroup[levelNum].Put(groupByKey, row);
                }
                else
                {
                    row.DecreaseRefcount();
                    if (row.Refcount <= 0)
                    {
                        RemovedKeys.Add(new Pair<int, object>(levelNum, groupByKey));
                    }
                }
                AggregateIntoLeave(level, row.Methods, row.States, eventsPerStream, exprEvaluatorContext);
                InternalHandleUpdatedGroup(levelNum, groupByKey, row);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false); }
        }

        public ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var col = LocalGroupByPlan.Columns[column];
            if (col.PartitionEvaluators.Length == 0)
            {
                return col.Pair.Accessor.GetEnumerableEvents(
                    StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, context);
            }
            var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, isNewData, context);
            var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
            return col.Pair.Accessor.GetEnumerableEvents(row.States[col.Pair.Slot], eventsPerStream, isNewData, context);
        }

        public ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var col = LocalGroupByPlan.Columns[column];
            if (col.PartitionEvaluators.Length == 0)
            {
                return col.Pair.Accessor.GetEnumerableScalar(
                    StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, context);
            }
            var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, isNewData, context);
            var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
            return col.Pair.Accessor.GetEnumerableScalar(row.States[col.Pair.Slot], eventsPerStream, isNewData, context);
        }

        public EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context)
        {
            var col = LocalGroupByPlan.Columns[column];
            if (col.PartitionEvaluators.Length == 0)
            {
                return col.Pair.Accessor.GetEnumerableEvent(
                    StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, context);
            }
            var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, isNewData, context);
            var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
            return col.Pair.Accessor.GetEnumerableEvent(row.States[col.Pair.Slot], eventsPerStream, isNewData, context);
        }

        public virtual bool IsGrouped
        {
            get { return true; }
        }

        public virtual void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
            // not applicable
        }

        public virtual void Accept(AggregationServiceVisitor visitor)
        {
            visitor.VisitAggregations(NumGroups, AggregatorsTopLevel, StatesTopLevel, AggregatorsPerLevelAndGroup);
        }

        public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
            visitor.VisitGrouped(NumGroups);
            if (AggregatorsTopLevel != null)
            {
                visitor.VisitGroup(null, AggregatorsTopLevel, StatesTopLevel);
            }
            for (var i = 0; i < LocalGroupByPlan.AllLevels.Length; i++)
            {
                foreach (var entry in AggregatorsPerLevelAndGroup[i])
                {
                    visitor.VisitGroup(entry.Key, entry.Value);
                }
            }
        }

        public void InternalHandleUpdatedGroup(int level, object groupByKey, AggregationMethodPairRow row)
        {
            // no action required
        }

        public void InternalHandleUpdatedTop()
        {
            // no action required
        }

        public void InternalHandleGroupRemove(Pair<int, object> groupByKey)
        {
            // no action required
        }

        public void HandleRemovedKeys()
        {
            if (!RemovedKeys.IsEmpty())
                // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
            {
                foreach (var removedKey in RemovedKeys)
                {
                    AggregatorsPerLevelAndGroup[removedKey.First].Remove(removedKey.Second);
                    InternalHandleGroupRemove(removedKey);
                }
                RemovedKeys.Clear();
            }
        }

        public object GetGroupKey(int agentInstanceId)
        {
            return null;
        }

        public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new UnsupportedOperationException();
        }

        public static object ComputeGroupKey(
            ExprEvaluator[] partitionEval,
            EventBean[] eventsPerStream,
            bool b,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            if (partitionEval.Length == 1)
            {
                return partitionEval[0].Evaluate(evaluateParams);
            }
            var keys = new object[partitionEval.Length];
            for (var i = 0; i < keys.Length; i++)
            {
                keys[i] = partitionEval[i].Evaluate(evaluateParams);
            }
            return new MultiKeyUntyped(keys);
        }

        public static void AggregateIntoEnter(
            AggregationLocalGroupByLevel level,
            AggregationMethod[] methods,
            AggregationState[] states,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            for (var i = 0; i < level.MethodEvaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, methods[i], level.MethodFactories[i].AggregationExpression); }
                var value = level.MethodEvaluators[i].Evaluate(evaluateParams);
                methods[i].Enter(value);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, methods[i]); }
            }
            for (var i = 0; i < states.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, states[i], level.StateFactories[i].AggregationExpression); }
                states[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, states[i]); }
            }
        }

        public static void AggregateIntoLeave(
            AggregationLocalGroupByLevel level,
            AggregationMethod[] methods,
            AggregationState[] states,
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);
            for (var i = 0; i < level.MethodEvaluators.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, methods[i], level.MethodFactories[i].AggregationExpression); }
                var value = level.MethodEvaluators[i].Evaluate(evaluateParams);
                methods[i].Leave(value);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, methods[i]); }
            }
            for (var i = 0; i < states.Length; i++)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, states[i], level.StateFactories[i].AggregationExpression); }
                states[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, states[i]); }
            }
        }

        public static void ClearResults(
            IDictionary<object, AggregationMethodPairRow>[] aggregatorsPerLevelAndGroup,
            AggregationMethod[] aggregatorsTopLevel,
            AggregationState[] statesTopLevel)
        {
            foreach (var aggregatorsPerGroup in aggregatorsPerLevelAndGroup)
            {
                aggregatorsPerGroup.Clear();
            }
            if (aggregatorsTopLevel != null)
            {
                foreach (var method in aggregatorsTopLevel)
                {
                    method.Clear();
                }
                foreach (var state in statesTopLevel)
                {
                    state.Clear();
                }
            }
        }

        public AggregationMethod[] AggregatorsTopLevel { get; set; }

        public AggregationState[] StatesTopLevel { get; set; }

        public IDictionary<object, AggregationMethodPairRow>[] AggregatorsPerLevelAndGroup { get; set; }

        public IList<Pair<int, object>> RemovedKeys { get; set; }

        public void Stop()
        {
        }

        private int NumGroups
        {
            get
            {
                var size = AggregatorsTopLevel != null ? 1 : 0;
                for (var i = 0; i < LocalGroupByPlan.AllLevels.Length; i++)
                {
                    size += AggregatorsPerLevelAndGroup[i].Count;
                }
                return size;
            }
        }
    }
} // end of namespace

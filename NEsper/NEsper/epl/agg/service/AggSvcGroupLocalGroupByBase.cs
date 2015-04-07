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
	    protected readonly MethodResolutionService methodResolutionService;
	    protected readonly bool isJoin;
	    protected readonly AggregationLocalGroupByPlan localGroupByPlan;
	    protected readonly object groupKeyBinding;

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

	    public AggSvcGroupLocalGroupByBase(
	        MethodResolutionService methodResolutionService,
	        bool isJoin,
	        AggregationLocalGroupByPlan localGroupByPlan,
	        object groupKeyBinding)
	    {
	        this.methodResolutionService = methodResolutionService;
	        this.isJoin = isJoin;
	        this.localGroupByPlan = localGroupByPlan;
	        this.groupKeyBinding = groupKeyBinding;

            this.AggregatorsPerLevelAndGroup = new IDictionary<object, AggregationMethodPairRow>[localGroupByPlan.AllLevels.Length];
	        for (var i = 0; i < localGroupByPlan.AllLevels.Length; i++) {
	            this.AggregatorsPerLevelAndGroup[i] = new Dictionary<object, AggregationMethodPairRow>();
	        }
	        RemovedKeys = new List<Pair<int, object>>();
	    }

	    public void ClearResults(ExprEvaluatorContext exprEvaluatorContext)
	    {
	        ClearResults(AggregatorsPerLevelAndGroup, AggregatorsTopLevel, StatesTopLevel);
	    }

	    public void ApplyEnter(EventBean[] eventsPerStream, object groupByKeyProvided, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(true, localGroupByPlan.NumMethods, localGroupByPlan.NumAccess, groupByKeyProvided);}
	        HandleRemovedKeys();

	        if (localGroupByPlan.OptionalLevelTop != null) {
	            if (AggregatorsTopLevel == null) {
	                AggregatorsTopLevel = methodResolutionService.NewAggregators(localGroupByPlan.OptionalLevelTop.MethodFactories, exprEvaluatorContext.AgentInstanceId, null, null, null);
	                StatesTopLevel = methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, isJoin, localGroupByPlan.OptionalLevelTop.StateFactories, null, null, null);
	            }
	            AggregateIntoEnter(localGroupByPlan.OptionalLevelTop, AggregatorsTopLevel, StatesTopLevel, eventsPerStream, exprEvaluatorContext);
	            InternalHandleUpdatedTop();
	        }

	        for (var levelNum = 0; levelNum < localGroupByPlan.AllLevels.Length; levelNum++) {
	            var level = localGroupByPlan.AllLevels[levelNum];
	            var partitionEval = level.PartitionEvaluators;
	            var groupByKey = ComputeGroupKey(level, groupByKeyProvided, partitionEval, eventsPerStream, true, exprEvaluatorContext);
	            var row = AggregatorsPerLevelAndGroup[levelNum].Get(groupByKey);
	            if (row == null) {
	                var rowAggregators = methodResolutionService.NewAggregators(level.MethodFactories, exprEvaluatorContext.AgentInstanceId, groupByKey, null, null);
	                var rowStates = methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, isJoin, level.StateFactories, groupByKey, null, null);
	                row = new AggregationMethodPairRow(methodResolutionService.GetCurrentRowCount(rowAggregators, rowStates) + 1, rowAggregators, rowStates);
	                AggregatorsPerLevelAndGroup[levelNum].Put(groupByKey, row);
	            }
	            else {
	                row.IncreaseRefcount();
	            }

	            AggregateIntoEnter(level, row.Methods, row.States, eventsPerStream, exprEvaluatorContext);
	            InternalHandleUpdatedGroup(levelNum, groupByKey, row);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(true); }
	    }

	    public void ApplyLeave(EventBean[] eventsPerStream, object groupByKeyProvided, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggregationGroupedApplyEnterLeave(false, localGroupByPlan.NumMethods, localGroupByPlan.NumAccess, groupByKeyProvided);}
	        if (localGroupByPlan.OptionalLevelTop != null) {
	            if (AggregatorsTopLevel == null) {
	                AggregatorsTopLevel = methodResolutionService.NewAggregators(localGroupByPlan.OptionalLevelTop.MethodFactories, exprEvaluatorContext.AgentInstanceId, null, null, null);
	                StatesTopLevel = methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, isJoin, localGroupByPlan.OptionalLevelTop.StateFactories, null, null, null);
	            }
	            AggregateIntoLeave(localGroupByPlan.OptionalLevelTop, AggregatorsTopLevel, StatesTopLevel, eventsPerStream, exprEvaluatorContext);
	            InternalHandleUpdatedTop();
	        }

	        for (var levelNum = 0; levelNum < localGroupByPlan.AllLevels.Length; levelNum++) {
	            var level = localGroupByPlan.AllLevels[levelNum];
	            var partitionEval = level.PartitionEvaluators;
	            var groupByKey = ComputeGroupKey(level, groupByKeyProvided, partitionEval, eventsPerStream, true, exprEvaluatorContext);
	            var row = AggregatorsPerLevelAndGroup[levelNum].Get(groupByKey);
	            if (row == null) {
	                var rowAggregators = methodResolutionService.NewAggregators(level.MethodFactories, exprEvaluatorContext.AgentInstanceId, groupByKey, null, null);
	                var rowStates = methodResolutionService.NewAccesses(exprEvaluatorContext.AgentInstanceId, isJoin, level.StateFactories, groupByKey, null, null);
	                row = new AggregationMethodPairRow(methodResolutionService.GetCurrentRowCount(rowAggregators, rowStates) + 1, rowAggregators, rowStates);
	                AggregatorsPerLevelAndGroup[levelNum].Put(groupByKey, row);
	            }
	            else {
	                row.DecreaseRefcount();
	                if (row.Refcount <= 0) {
	                    RemovedKeys.Add(new Pair<int, object>(levelNum, groupByKey));
	                }
	            }
	            AggregateIntoLeave(level, row.Methods, row.States, eventsPerStream, exprEvaluatorContext);
	            InternalHandleUpdatedGroup(levelNum, groupByKey, row);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggregationGroupedApplyEnterLeave(false);}
	    }

	    public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        var col = localGroupByPlan.Columns[column];
	        if (col.PartitionEvaluators.Length == 0) {
	            return col.Pair.Accessor.GetEnumerableEvents(StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, context);
	        }
	        var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, isNewData, context);
	        var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
	        return col.Pair.Accessor.GetEnumerableEvents(row.States[col.Pair.Slot], eventsPerStream, isNewData, context);
	    }

	    public ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        var col = localGroupByPlan.Columns[column];
	        if (col.PartitionEvaluators.Length == 0) {
	            return col.Pair.Accessor.GetEnumerableScalar(StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, context);
	        }
	        var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, isNewData, context);
	        var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
	        return col.Pair.Accessor.GetEnumerableScalar(row.States[col.Pair.Slot], eventsPerStream, isNewData, context);
	    }

	    public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        var col = localGroupByPlan.Columns[column];
	        if (col.PartitionEvaluators.Length == 0) {
	            return col.Pair.Accessor.GetEnumerableEvent(StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, context);
	        }
	        var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, isNewData, context);
	        var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
	        return col.Pair.Accessor.GetEnumerableEvent(row.States[col.Pair.Slot], eventsPerStream, isNewData, context);
	    }

	    public bool IsGrouped
	    {
	        get { return true; }
	    }

	    public void SetRemovedCallback(AggregationRowRemovedCallback callback)
        {
	        // not applicable
	    }

	    public void Accept(AggregationServiceVisitor visitor)
        {
	        visitor.VisitAggregations(NumGroups, AggregatorsTopLevel, StatesTopLevel, AggregatorsPerLevelAndGroup);
	    }

	    public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor)
        {
	        visitor.VisitGrouped(NumGroups);
	        if (AggregatorsTopLevel != null) {
	            visitor.VisitGroup(null, AggregatorsTopLevel, StatesTopLevel);
	        }
	        for (var i = 0; i < localGroupByPlan.AllLevels.Length; i++) {
	            foreach (var entry in AggregatorsPerLevelAndGroup[i]) {
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
	        if (!RemovedKeys.IsEmpty())     // we collect removed keys lazily on the next enter to reduce the chance of empty-group queries creating empty aggregators temporarily
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

	    protected static object ComputeGroupKey(ExprEvaluator[] partitionEval, EventBean[] eventsPerStream, bool b, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            
            if (partitionEval.Length == 1)
	        {
	            return partitionEval[0].Evaluate(evaluateParams);
	        }
	        var keys = new object[partitionEval.Length];
	        for (var i = 0; i < keys.Length; i++) {
	            keys[i] = partitionEval[i].Evaluate(evaluateParams);
	        }
	        return new MultiKeyUntyped(keys);
	    }

	    protected static void AggregateIntoEnter(AggregationLocalGroupByLevel level, AggregationMethod[] methods, AggregationState[] states, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, true, exprEvaluatorContext);
            for (var i = 0; i < level.MethodEvaluators.Length; i++) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(true, i, methods[i], level.MethodFactories[i].AggregationExpression);}
	            var value = level.MethodEvaluators[i].Evaluate(evaluateParams);
	            methods[i].Enter(value);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(true, i, methods[i]);}
	        }
	        for (var i = 0; i < states.Length; i++) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(true, i, states[i], level.StateFactories[i].AggregationExpression);}
	            states[i].ApplyEnter(eventsPerStream, exprEvaluatorContext);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(true, i, states[i]);}
	        }
	    }

	    protected static void AggregateIntoLeave(AggregationLocalGroupByLevel level, AggregationMethod[] methods, AggregationState[] states, EventBean[] eventsPerStream, ExprEvaluatorContext exprEvaluatorContext)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, false, exprEvaluatorContext);
            for (var i = 0; i < level.MethodEvaluators.Length; i++) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggNoAccessEnterLeave(false, i, methods[i], level.MethodFactories[i].AggregationExpression);}
	            var value = level.MethodEvaluators[i].Evaluate(evaluateParams);
	            methods[i].Leave(value);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggNoAccessEnterLeave(false, i, methods[i]);}
	        }
	        for (var i = 0; i < states.Length; i++) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QAggAccessEnterLeave(false, i, states[i], level.StateFactories[i].AggregationExpression);}
	            states[i].ApplyLeave(eventsPerStream, exprEvaluatorContext);
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AAggAccessEnterLeave(false, i, states[i]);}
	        }
	    }

	    protected static void ClearResults(IDictionary<object, AggregationMethodPairRow>[] aggregatorsPerLevelAndGroup, AggregationMethod[] aggregatorsTopLevel, AggregationState[] statesTopLevel)
        {
	        foreach (var aggregatorsPerGroup in aggregatorsPerLevelAndGroup) {
	            aggregatorsPerGroup.Clear();
	        }
	        if (aggregatorsTopLevel != null) {
	            foreach (var method in aggregatorsTopLevel) {
	                method.Clear();
	            }
	            foreach (var state in statesTopLevel) {
	                state.Clear();
	            }
	        }
	    }

	    public AggregationMethod[] AggregatorsTopLevel { get; set; }

	    public AggregationState[] StatesTopLevel { get; set; }

	    public IDictionary<object, AggregationMethodPairRow>[] AggregatorsPerLevelAndGroup { get; set; }

	    public IList<Pair<int, object>> RemovedKeys { get; set; }

	    private int NumGroups
	    {
	        get
	        {
	            var size = AggregatorsTopLevel != null ? 1 : 0;
	            for (var i = 0; i < localGroupByPlan.AllLevels.Length; i++)
	            {
	                size += AggregatorsPerLevelAndGroup[i].Count;
	            }
	            return size;
	        }
	    }
	}
} // end of namespace

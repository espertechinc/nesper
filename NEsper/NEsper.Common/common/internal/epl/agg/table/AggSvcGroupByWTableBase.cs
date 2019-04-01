///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
	/// <summary>
	/// Implementation for handling aggregation with grouping by group-keys.
	/// </summary>
	public abstract class AggSvcGroupByWTableBase : AggregationService {
	    internal readonly TableInstanceGrouped tableInstance;
	    internal readonly TableColumnMethodPairEval[] methodPairs;
	    private readonly AggregationMultiFunctionAgent[] accessAgents;
	    private readonly int[] accessColumnsZeroOffset;

	    protected AggregationRow currentAggregationRow;
	    protected object currentGroupKey;

	    public AggSvcGroupByWTableBase(TableInstanceGrouped tableInstance, TableColumnMethodPairEval[] methodPairs, AggregationMultiFunctionAgent[] accessAgents, int[] accessColumnsZeroOffset) {
	        this.tableInstance = tableInstance;
	        this.methodPairs = methodPairs;
	        this.accessAgents = accessAgents;
	        this.accessColumnsZeroOffset = accessColumnsZeroOffset;
	    }

	    public abstract void ApplyEnterInternal(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext);

	    public abstract void ApplyLeaveInternal(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext);

	    public void ApplyEnter(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext) {
	        // acquire tableInstance-level write lock
	        TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock(), exprEvaluatorContext);
	        ApplyEnterInternal(eventsPerStream, groupByKey, exprEvaluatorContext);
	    }

	    public void ApplyLeave(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext) {
	        // acquire tableInstance-level write lock
	        TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock(), exprEvaluatorContext);
	        ApplyLeaveInternal(eventsPerStream, groupByKey, exprEvaluatorContext);
	    }

	    protected void ApplyEnterGroupKey(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext) {
	        ObjectArrayBackedEventBean bean = tableInstance.GetCreateRowIntoTable(groupByKey, exprEvaluatorContext);
	        currentAggregationRow = (AggregationRow) bean.Properties[0];

	        InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
	        instrumentationCommon.QAggregationGroupedApplyEnterLeave(true, methodPairs.Length, accessAgents.Length, groupByKey);

	        for (int i = 0; i < methodPairs.Length; i++) {
	            TableColumnMethodPairEval methodPair = methodPairs[i];
	            instrumentationCommon.QAggNoAccessEnterLeave(true, i, null, null);
	            object columnResult = methodPair.Evaluator.Evaluate(eventsPerStream, true, exprEvaluatorContext);
	            currentAggregationRow.EnterAgg(methodPair.Column, columnResult);
	            instrumentationCommon.AAggNoAccessEnterLeave(true, i, null);
	        }

	        for (int i = 0; i < accessAgents.Length; i++) {
	            instrumentationCommon.QAggAccessEnterLeave(true, i, null);
	            accessAgents[i].ApplyEnter(eventsPerStream, exprEvaluatorContext, currentAggregationRow, accessColumnsZeroOffset[i]);
	            instrumentationCommon.AAggAccessEnterLeave(true, i);
	        }

	        tableInstance.HandleRowUpdated(bean);

	        instrumentationCommon.AAggregationGroupedApplyEnterLeave(true);
	    }

	    protected void ApplyLeaveGroupKey(EventBean[] eventsPerStream, object groupByKey, ExprEvaluatorContext exprEvaluatorContext) {
	        ObjectArrayBackedEventBean bean = tableInstance.GetCreateRowIntoTable(groupByKey, exprEvaluatorContext);
	        currentAggregationRow = (AggregationRow) bean.Properties[0];

	        InstrumentationCommon instrumentationCommon = exprEvaluatorContext.InstrumentationProvider;
	        instrumentationCommon.QAggregationGroupedApplyEnterLeave(false, methodPairs.Length, accessAgents.Length, groupByKey);

	        for (int i = 0; i < methodPairs.Length; i++) {
	            TableColumnMethodPairEval methodPair = methodPairs[i];
	            instrumentationCommon.QAggNoAccessEnterLeave(false, i, null, null);
	            object columnResult = methodPair.Evaluator.Evaluate(eventsPerStream, false, exprEvaluatorContext);
	            currentAggregationRow.LeaveAgg(methodPair.Column, columnResult);
	            instrumentationCommon.AAggNoAccessEnterLeave(false, i, null);
	        }

	        for (int i = 0; i < accessAgents.Length; i++) {
	            instrumentationCommon.QAggAccessEnterLeave(false, i, null);
	            accessAgents[i].ApplyLeave(eventsPerStream, exprEvaluatorContext, currentAggregationRow, accessColumnsZeroOffset[i]);
	            instrumentationCommon.AAggAccessEnterLeave(false, i);
	        }

	        tableInstance.HandleRowUpdated(bean);

	        instrumentationCommon.AAggregationGroupedApplyEnterLeave(false);
	    }

	    public virtual void SetCurrentAccess(object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel) {
	        ObjectArrayBackedEventBean bean = tableInstance.GetRowForGroupKey(groupByKey);
	        if (bean != null) {
	            currentAggregationRow = (AggregationRow) bean.Properties[0];
	        } else {
	            currentAggregationRow = null;
	        }
	        this.currentGroupKey = groupByKey;
	    }

	    public object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext) {
	        return currentAggregationRow.GetValue(column, eventsPerStream, isNewData, exprEvaluatorContext);
	    }

	    public ICollection<EventBean> GetCollectionOfEvents(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return currentAggregationRow.GetCollectionOfEvents(column, eventsPerStream, isNewData, context);
	    }

	    public ICollection<object> GetCollectionScalar(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return currentAggregationRow.GetCollectionScalar(column, eventsPerStream, isNewData, context);
	    }

	    public EventBean GetEventBean(int column, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext context) {
	        return currentAggregationRow.GetEventBean(column, eventsPerStream, isNewData, context);
	    }

	    public void SetRemovedCallback(AggregationRowRemovedCallback callback) {
	        // not applicable
	    }

	    public void Accept(AggregationServiceVisitor visitor) {
	        // not applicable
	    }

	    public void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor) {
	        // not applicable
	    }

	    public bool IsGrouped
	    {
	        get => true;
	    }

	    public object GetGroupKey(int agentInstanceId) {
	        return currentGroupKey;
	    }

	    public ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext) {
	        return tableInstance.GroupKeys;
	    }

	    public void ClearResults(ExprEvaluatorContext exprEvaluatorContext) {
	        // clear not required
	    }

	    public void Stop() {
	    }

	    public AggregationService GetContextPartitionAggregationService(int agentInstanceId) {
	        return this;
	    }
	}
} // end of namespace
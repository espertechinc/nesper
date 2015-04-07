///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.service
{
	/// <summary>
	/// Implementation for handling aggregation with grouping by group-keys.
	/// </summary>
	public class AggSvcGroupByWTableRollupMultiKeyImpl : AggSvcGroupByWTableBase
	{
	    private readonly AggregationGroupByRollupDesc groupByRollupDesc;

	    public AggSvcGroupByWTableRollupMultiKeyImpl(
	        TableMetadata tableMetadata,
	        TableColumnMethodPair[] methodPairs,
	        AggregationAccessorSlotPair[] accessors,
	        bool join,
	        TableStateInstanceGroupBy tableStateInstance,
	        int[] targetStates,
	        ExprNode[] accessStateExpr,
	        AggregationAgent[] agents,
	        AggregationGroupByRollupDesc groupByRollupDesc)
	        : base(tableMetadata, methodPairs, accessors, join, tableStateInstance, targetStates, accessStateExpr, agents)
        {
	        this.groupByRollupDesc = groupByRollupDesc;
	    }

	    public override void ApplyEnterInternal(EventBean[] eventsPerStream, object compositeGroupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
	        var groupKeyPerLevel = (object[]) compositeGroupByKey;
	        for (var i = 0; i < groupKeyPerLevel.Length; i++) {
	            var level = groupByRollupDesc.Levels[i];
	            object groupByKey = level.ComputeMultiKey(groupKeyPerLevel[i], TableMetadata.KeyTypes.Length);
	            ApplyEnterGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
	        }
	    }

	    public override void ApplyLeaveInternal(EventBean[] eventsPerStream, object compositeGroupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
	        var groupKeyPerLevel = (object[]) compositeGroupByKey;
	        for (var i = 0; i < groupKeyPerLevel.Length; i++) {
	            var level = groupByRollupDesc.Levels[i];
	            object groupByKey = level.ComputeMultiKey(groupKeyPerLevel[i], TableMetadata.KeyTypes.Length);
	            ApplyLeaveGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
	        }
	    }

	    public override void SetCurrentAccess(object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
	    {
	        var key = rollupLevel.ComputeMultiKey(groupByKey, TableMetadata.KeyTypes.Length);
	        var bean = TableStateInstance.Rows.Get(key);

	        if (bean != null) {
	            var row = (AggregationRowPair) bean.Properties[0];
	            CurrentAggregatorMethods = row.Methods;
	            CurrentAggregatorStates = row.States;
	        }
	        else {
	            CurrentAggregatorMethods = null;
	        }

	        this.CurrentGroupKey = key;
	    }
	}
} // end of namespace

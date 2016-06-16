///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
	/// <summary>
	/// Implementation for handling aggregation with grouping by group-keys.
	/// </summary>
	public class AggSvcGroupAllLocalGroupBy : AggSvcGroupLocalGroupByBase
	{
	    public AggSvcGroupAllLocalGroupBy(MethodResolutionService methodResolutionService, bool isJoin, AggregationLocalGroupByPlan localGroupByPlan, object groupKeyBinding)
            : base(methodResolutionService, isJoin, localGroupByPlan, groupKeyBinding)
        {
	        
	    }

	    protected override object ComputeGroupKey(AggregationLocalGroupByLevel level, object groupKey, ExprEvaluator[] partitionEval, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext)
        {
	        return AggSvcGroupLocalGroupByBase.ComputeGroupKey(partitionEval, eventsPerStream, newData, exprEvaluatorContext);
	    }

	    public override void SetCurrentAccess(object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
	    {
	    }

	    public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        AggregationLocalGroupByColumn col = LocalGroupByPlan.Columns[column];

	        if (col.PartitionEvaluators.Length == 0) {
	            if (col.IsMethodAgg) {
	                return AggregatorsTopLevel[col.MethodOffset].Value;
	            }
	            return col.Pair.Accessor.GetValue(StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
	        }

	        var groupByKey = ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, true, exprEvaluatorContext);
	        AggregationMethodPairRow row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
	        if (col.IsMethodAgg) {
	            return row.Methods[col.MethodOffset].Value;
	        }
	        return col.Pair.Accessor.GetValue(row.States[col.Pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
	    }
	}
} // end of namespace

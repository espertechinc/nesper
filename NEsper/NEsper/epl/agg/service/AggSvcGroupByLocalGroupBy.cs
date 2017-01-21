///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.agg.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;

namespace com.espertech.esper.epl.agg.service
{
	/// <summary>
	/// Implementation for handling aggregation with grouping by group-keys.
	/// </summary>
	public class AggSvcGroupByLocalGroupBy : AggSvcGroupLocalGroupByBase
	{
	    private AggregationMethod[] _currentAggregatorMethods;
	    private AggregationState[] _currentAggregatorStates;

	    public AggSvcGroupByLocalGroupBy(bool isJoin, AggregationLocalGroupByPlan localGroupByPlan)
	        : base (isJoin, localGroupByPlan)
        {
	    }

	    protected override object ComputeGroupKey(AggregationLocalGroupByLevel level, object groupKey, ExprEvaluator[] partitionEval, EventBean[] eventsPerStream, bool newData, ExprEvaluatorContext exprEvaluatorContext) {
	        if (level.IsDefaultLevel) {
	            return groupKey;
	        }
	        return AggSvcGroupAllLocalGroupBy.ComputeGroupKey(partitionEval, eventsPerStream, true, exprEvaluatorContext);
	    }

	    public override void SetCurrentAccess(object groupByKey, int agentInstanceId, AggregationGroupByRollupLevel rollupLevel)
	    {
	        if (!LocalGroupByPlan.AllLevels[0].IsDefaultLevel) {
	            return;
	        }
	        var row = AggregatorsPerLevelAndGroup[0].Get(groupByKey);

	        if (row != null) {
	            _currentAggregatorMethods = row.Methods;
	            _currentAggregatorStates = row.States;
	        }
	        else {
	            _currentAggregatorMethods = null;
	        }

	        if (_currentAggregatorMethods == null) {
                _currentAggregatorMethods = AggSvcGroupByUtil.NewAggregators(LocalGroupByPlan.AllLevels[0].MethodFactories);
                _currentAggregatorStates = AggSvcGroupByUtil.NewAccesses(agentInstanceId, IsJoin, LocalGroupByPlan.AllLevels[0].StateFactories, groupByKey, null);
	        }
	    }

	    public override object GetValue(int column, int agentInstanceId, EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
	    {
	        var col = LocalGroupByPlan.Columns[column];
	        if (col.IsDefaultGroupLevel) {
	            if (col.IsMethodAgg) {
	                return _currentAggregatorMethods[col.MethodOffset].Value;
	            }
	            return col.Pair.Accessor.GetValue(_currentAggregatorStates[col.Pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
	        }
	        if (col.PartitionEvaluators.Length == 0) {
	            if (col.IsMethodAgg) {
	                return AggregatorsTopLevel[col.MethodOffset].Value;
	            }
	            return col.Pair.Accessor.GetValue(StatesTopLevel[col.Pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
	        }
	        var groupByKey = AggSvcGroupAllLocalGroupBy.ComputeGroupKey(col.PartitionEvaluators, eventsPerStream, true, exprEvaluatorContext);
	        var row = AggregatorsPerLevelAndGroup[col.LevelNum].Get(groupByKey);
	        if (col.IsMethodAgg) {
	            return row.Methods[col.MethodOffset].Value;
	        }
	        return col.Pair.Accessor.GetValue(row.States[col.Pair.Slot], eventsPerStream, isNewData, exprEvaluatorContext);
	    }
	}
} // end of namespace

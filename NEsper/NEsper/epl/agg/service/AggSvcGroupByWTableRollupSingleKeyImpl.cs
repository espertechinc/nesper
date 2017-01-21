///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.service
{
	/// <summary>
	/// Implementation for handling aggregation with grouping by group-keys.
	/// </summary>
	public class AggSvcGroupByWTableRollupSingleKeyImpl : AggSvcGroupByWTableBase
	{
        public AggSvcGroupByWTableRollupSingleKeyImpl(TableMetadata tableMetadata, TableColumnMethodPair[] methodPairs, AggregationAccessorSlotPair[] accessors, bool join, TableStateInstanceGrouped tableStateInstance, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents)
            : base(tableMetadata, methodPairs, accessors, join, tableStateInstance, targetStates, accessStateExpr, agents)
        {
	    }

	    public override void ApplyEnterInternal(EventBean[] eventsPerStream, object compositeGroupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
	        var groupKeyPerLevel = (object[]) compositeGroupByKey;
	        foreach (var groupByKey in groupKeyPerLevel) {
	            ApplyEnterGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
	        }
	    }

	    public override void ApplyLeaveInternal(EventBean[] eventsPerStream, object compositeGroupByKey, ExprEvaluatorContext exprEvaluatorContext)
        {
	        var groupKeyPerLevel = (object[]) compositeGroupByKey;
	        foreach (var groupByKey in groupKeyPerLevel) {
	            ApplyLeaveGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
	        }
	    }
	}
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
	public class AggregationServiceFactoryTable : AggregationServiceFactory {
	    private Table table;
	    private TableColumnMethodPairEval[] methodPairs;
	    private AggregationMultiFunctionAgent[] accessAgents;
	    private int[] accessColumnsZeroOffset;
	    private AggregationGroupByRollupDesc groupByRollupDesc;

	    public void SetTable(Table table) {
	        this.table = table;
	    }

	    public void SetMethodPairs(TableColumnMethodPairEval[] methodPairs) {
	        this.methodPairs = methodPairs;
	    }

	    public void SetAccessAgents(AggregationMultiFunctionAgent[] accessAgents) {
	        this.accessAgents = accessAgents;
	    }

	    public void SetAccessColumnsZeroOffset(int[] accessColumnsZeroOffset) {
	        this.accessColumnsZeroOffset = accessColumnsZeroOffset;
	    }

	    public void SetGroupByRollupDesc(AggregationGroupByRollupDesc groupByRollupDesc) {
	        this.groupByRollupDesc = groupByRollupDesc;
	    }

	    public AggregationService MakeService(AgentInstanceContext agentInstanceContext, ImportServiceRuntime importService, bool isSubquery, int? subqueryNumber, int[] groupId) {
	        TableInstance tableInstance = table.GetTableInstance(agentInstanceContext.AgentInstanceId);
	        if (!table.MetaData.IsKeyed) {
	            TableInstanceUngrouped tableInstanceUngrouped = (TableInstanceUngrouped) tableInstance;
	            return new AggSvcGroupAllWTableImpl(tableInstanceUngrouped, methodPairs, accessAgents, accessColumnsZeroOffset);
	        }

	        TableInstanceGrouped tableInstanceGrouped = (TableInstanceGrouped) tableInstance;
	        if (groupByRollupDesc == null) {
	            return new AggSvcGroupByWTableImpl(tableInstanceGrouped, methodPairs, accessAgents, accessColumnsZeroOffset);
	        }

	        if (table.MetaData.KeyTypes.Length > 1) {
	            return new AggSvcGroupByWTableRollupMultiKeyImpl(tableInstanceGrouped, methodPairs, accessAgents, accessColumnsZeroOffset, groupByRollupDesc);
	        } else {
	            return new AggSvcGroupByWTableRollupSingleKeyImpl(tableInstanceGrouped, methodPairs, accessAgents, accessColumnsZeroOffset);
	        }
	    }
	}
} // end of namespace
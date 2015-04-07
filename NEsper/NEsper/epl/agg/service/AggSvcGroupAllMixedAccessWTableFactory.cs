///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.agg.service
{
    /// <summary>
    /// Implementation for handling aggregation without any grouping (no group-by).
    /// </summary>
    public class AggSvcGroupAllMixedAccessWTableFactory : AggregationServiceFactory
    {
        protected readonly AggregationAccessorSlotPair[] accessors;
        protected readonly bool isJoin;
        private readonly TableColumnMethodPair[] methodPairs;
        private readonly string tableName;
        private readonly int[] targetStates;
        private readonly ExprNode[] accessStateExpr;
        private readonly AggregationAgent[] agents;
    
        public AggSvcGroupAllMixedAccessWTableFactory(AggregationAccessorSlotPair[] accessors, bool join, TableColumnMethodPair[] methodPairs, string tableName, int[] targetStates, ExprNode[] accessStateExpr, AggregationAgent[] agents)
        {
            this.accessors = accessors;
            isJoin = join;
            this.methodPairs = methodPairs;
            this.tableName = tableName;
            this.targetStates = targetStates;
            this.accessStateExpr = accessStateExpr;
            this.agents = agents;
        }
    
        public AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService) {
            TableStateInstanceUngrouped tableState = (TableStateInstanceUngrouped) agentInstanceContext.StatementContext.TableService.GetState(tableName, agentInstanceContext.AgentInstanceId);
            return new AggSvcGroupAllMixedAccessWTableImpl(tableState, methodPairs,
                    accessors, targetStates, accessStateExpr, agents);
        }
    }
}

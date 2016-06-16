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
        protected internal readonly AggregationAccessorSlotPair[] Accessors;
        protected internal readonly bool IsJoin;

        private readonly TableColumnMethodPair[] _methodPairs;
        private readonly string _tableName;
        private readonly int[] _targetStates;
        private readonly ExprNode[] _accessStateExpr;
        private readonly AggregationAgent[] _agents;

        public AggSvcGroupAllMixedAccessWTableFactory(
            AggregationAccessorSlotPair[] accessors,
            bool join,
            TableColumnMethodPair[] methodPairs,
            string tableName,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents)
        {
            Accessors = accessors;
            IsJoin = join;
            _methodPairs = methodPairs;
            _tableName = tableName;
            _targetStates = targetStates;
            _accessStateExpr = accessStateExpr;
            _agents = agents;
        }

        public AggregationService MakeService(
            AgentInstanceContext agentInstanceContext,
            MethodResolutionService methodResolutionService,
            bool isSubquery,
            int? subqueryNumber)
        {
            var tableState = (TableStateInstanceUngrouped) agentInstanceContext.StatementContext.TableService.GetState(
                _tableName, agentInstanceContext.AgentInstanceId);
            return new AggSvcGroupAllMixedAccessWTableImpl(
                tableState, _methodPairs,
                Accessors, _targetStates, _accessStateExpr, _agents);
        }
    }
}

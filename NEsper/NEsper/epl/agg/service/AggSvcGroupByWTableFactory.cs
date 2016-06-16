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
    /// Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByWTableFactory : AggregationServiceFactory
    {
        private readonly TableMetadata _tableMetadata;
        private readonly TableColumnMethodPair[] _methodPairs;
        private readonly AggregationAccessorSlotPair[] _accessors;
        private readonly bool _isJoin;
        private readonly int[] _targetStates;
        private readonly ExprNode[] _accessStateExpr;
        private readonly AggregationAgent[] _agents;
        private readonly AggregationGroupByRollupDesc _groupByRollupDesc;

        public AggSvcGroupByWTableFactory(
            TableMetadata tableMetadata,
            TableColumnMethodPair[] methodPairs,
            AggregationAccessorSlotPair[] accessors,
            bool join,
            int[] targetStates,
            ExprNode[] accessStateExpr,
            AggregationAgent[] agents,
            AggregationGroupByRollupDesc groupByRollupDesc)
        {
            _tableMetadata = tableMetadata;
            _methodPairs = methodPairs;
            _accessors = accessors;
            _isJoin = join;
            _targetStates = targetStates;
            _accessStateExpr = accessStateExpr;
            _agents = agents;
            _groupByRollupDesc = groupByRollupDesc;
        }
    
        public AggregationService MakeService(AgentInstanceContext agentInstanceContext, MethodResolutionService methodResolutionService, bool isSubquery, int? subqueryNumber)
        {
            var tableState = (TableStateInstanceGrouped) agentInstanceContext.StatementContext.TableService.GetState(_tableMetadata.TableName, agentInstanceContext.AgentInstanceId);
            if (_groupByRollupDesc == null)
            {
                return new AggSvcGroupByWTableImpl(
                    _tableMetadata, _methodPairs, _accessors, _isJoin,
                    tableState, _targetStates, _accessStateExpr, _agents);
            }
            if (_tableMetadata.KeyTypes.Length > 1)
            {
                return new AggSvcGroupByWTableRollupMultiKeyImpl(
                    _tableMetadata, _methodPairs, _accessors, _isJoin,
                    tableState, _targetStates, _accessStateExpr, _agents, _groupByRollupDesc);
            }
            else
            {
                return new AggSvcGroupByWTableRollupSingleKeyImpl(
                    _tableMetadata, _methodPairs, _accessors, _isJoin,
                    tableState, _targetStates, _accessStateExpr, _agents);
            }
        }
    }
}

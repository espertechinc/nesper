///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.settings;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
    public class AggregationServiceFactoryTable : AggregationServiceFactory
    {
        private Table table;
        private TableColumnMethodPairEval[] methodPairs;
        private AggregationMultiFunctionAgent[] accessAgents;
        private int[] accessColumnsZeroOffset;
        private AggregationGroupByRollupDesc groupByRollupDesc;

        public Table Table
        {
            set => table = value;
        }

        public TableColumnMethodPairEval[] MethodPairs
        {
            set => methodPairs = value;
        }

        public AggregationMultiFunctionAgent[] AccessAgents
        {
            set => accessAgents = value;
        }

        public int[] AccessColumnsZeroOffset
        {
            set => accessColumnsZeroOffset = value;
        }

        public AggregationGroupByRollupDesc GroupByRollupDesc
        {
            set => groupByRollupDesc = value;
        }

        public AggregationService MakeService(
            ExprEvaluatorContext exprEvaluatorContext,
            bool isSubquery,
            int? subqueryNumber,
            int[] groupId)
        {
            var tableInstance = table.GetTableInstance(exprEvaluatorContext.AgentInstanceId);
            if (!table.MetaData.IsKeyed)
            {
                var tableInstanceUngrouped = (TableInstanceUngrouped) tableInstance;
                return new AggSvcGroupAllWTableImpl(
                    tableInstanceUngrouped,
                    methodPairs,
                    accessAgents,
                    accessColumnsZeroOffset);
            }

            var tableInstanceGrouped = (TableInstanceGrouped) tableInstance;
            if (groupByRollupDesc == null)
            {
                return new AggSvcGroupByWTableImpl(
                    tableInstanceGrouped,
                    methodPairs,
                    accessAgents,
                    accessColumnsZeroOffset);
            }

            if (table.MetaData.KeyTypes.Length > 1)
            {
                return new AggSvcGroupByWTableRollupMultiKeyImpl(
                    tableInstanceGrouped,
                    methodPairs,
                    accessAgents,
                    accessColumnsZeroOffset,
                    groupByRollupDesc);
            }
            else
            {
                return new AggSvcGroupByWTableRollupSingleKeyImpl(
                    tableInstanceGrouped,
                    methodPairs,
                    accessAgents,
                    accessColumnsZeroOffset);
            }
        }
    }
} // end of namespace
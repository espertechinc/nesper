///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
    public class AggregationServiceFactoryTable : AggregationServiceFactory
    {
        private Table _table;
        private TableColumnMethodPairEval[] _methodPairs;
        private AggregationMultiFunctionAgent[] _accessAgents;
        private int[] _accessColumnsZeroOffset;
        private AggregationGroupByRollupDesc _groupByRollupDesc;

        public Table Table {
            set => _table = value;
        }

        public TableColumnMethodPairEval[] MethodPairs {
            set => _methodPairs = value;
        }

        public AggregationMultiFunctionAgent[] AccessAgents {
            set => _accessAgents = value;
        }

        public int[] AccessColumnsZeroOffset {
            set => _accessColumnsZeroOffset = value;
        }

        public AggregationGroupByRollupDesc GroupByRollupDesc {
            set => _groupByRollupDesc = value;
        }

        public AggregationService MakeService(
            ExprEvaluatorContext exprEvaluatorContext,
            int? streamNum,
            int? subqueryNumber,
            int[] groupId)
        {
            var tableInstance = _table.GetTableInstance(exprEvaluatorContext.AgentInstanceId);
            if (!_table.MetaData.IsKeyed) {
                var tableInstanceUngrouped = (TableInstanceUngrouped)tableInstance;
                return new AggSvcGroupAllWTableImpl(
                    tableInstanceUngrouped,
                    _methodPairs,
                    _accessAgents,
                    _accessColumnsZeroOffset);
            }

            var tableInstanceGrouped = (TableInstanceGrouped)tableInstance;
            if (_groupByRollupDesc == null) {
                return new AggSvcGroupByWTableImpl(
                    tableInstanceGrouped,
                    _methodPairs,
                    _accessAgents,
                    _accessColumnsZeroOffset);
            }

            if (_table.MetaData.KeyTypes.Length > 1) {
                return new AggSvcGroupByWTableRollupMultiKeyImpl(
                    tableInstanceGrouped,
                    _methodPairs,
                    _accessAgents,
                    _accessColumnsZeroOffset,
                    _groupByRollupDesc);
            }
            else {
                return new AggSvcGroupByWTableRollupSingleKeyImpl(
                    tableInstanceGrouped,
                    _methodPairs,
                    _accessAgents,
                    _accessColumnsZeroOffset);
            }
        }
    }
} // end of namespace
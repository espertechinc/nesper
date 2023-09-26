///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByWTableRollupMultiKeyImpl : AggSvcGroupByWTableBase
    {
        private readonly AggregationGroupByRollupDesc groupByRollupDesc;
        private readonly int numKeys;

        public AggSvcGroupByWTableRollupMultiKeyImpl(
            TableInstanceGrouped tableInstance,
            TableColumnMethodPairEval[] methodPairs,
            AggregationMultiFunctionAgent[] accessAgents,
            int[] accessColumnsZeroOffset,
            AggregationGroupByRollupDesc groupByRollupDesc)
            : base(
                tableInstance,
                methodPairs,
                accessAgents,
                accessColumnsZeroOffset)
        {
            this.groupByRollupDesc = groupByRollupDesc;
            numKeys = tableInstance.Table.MetaData.KeyTypes.Length;
        }

        public override void ApplyEnterInternal(
            EventBean[] eventsPerStream,
            object compositeGroupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var groupKeyPerLevel = (object[])compositeGroupByKey;
            for (var i = 0; i < groupKeyPerLevel.Length; i++) {
                var level = groupByRollupDesc.Levels[i];
                var groupByKey = level.ComputeMultiKey(groupKeyPerLevel[i], numKeys);
                var tableKey = tableInstance.Table.PrimaryKeyObjectArrayTransform.From(groupByKey);
                ApplyEnterTableKey(eventsPerStream, tableKey, exprEvaluatorContext);
            }
        }

        public override void ApplyLeaveInternal(
            EventBean[] eventsPerStream,
            object compositeGroupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var groupKeyPerLevel = (object[])compositeGroupByKey;
            for (var i = 0; i < groupKeyPerLevel.Length; i++) {
                var level = groupByRollupDesc.Levels[i];
                var groupByKey = level.ComputeMultiKey(groupKeyPerLevel[i], numKeys);
                var tableKey = tableInstance.Table.PrimaryKeyObjectArrayTransform.From(groupByKey);
                ApplyLeaveTableKey(eventsPerStream, tableKey, exprEvaluatorContext);
            }
        }

        public override void SetCurrentAccess(
            object groupByKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel)
        {
            var key = rollupLevel.ComputeMultiKey(groupByKey, numKeys);
            var tableKey = tableInstance.Table.PrimaryKeyObjectArrayTransform.From(key);
            var bean = tableInstance.GetRowForGroupKey(tableKey);

            if (bean != null) {
                currentAggregationRow = (AggregationRow)bean.Properties[0];
            }
            else {
                currentAggregationRow = null;
            }

            currentGroupKey = key;
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.aggmultifunc;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.epl.agg.table
{
    /// <summary>
    ///     Implementation for handling aggregation with grouping by group-keys.
    /// </summary>
    public class AggSvcGroupByWTableRollupSingleKeyImpl : AggSvcGroupByWTableBase
    {
        public AggSvcGroupByWTableRollupSingleKeyImpl(
            TableInstanceGrouped tableInstance,
            TableColumnMethodPairEval[] methodPairs,
            AggregationMultiFunctionAgent[] accessAgents,
            int[] accessColumnsZeroOffset)
            : base(
                tableInstance,
                methodPairs,
                accessAgents,
                accessColumnsZeroOffset)
        {
        }

        public override void ApplyEnterInternal(
            EventBean[] eventsPerStream,
            object compositeGroupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var groupKeyPerLevel = (object[])compositeGroupByKey;
            foreach (var groupByKey in groupKeyPerLevel) {
                ApplyEnterTableKey(eventsPerStream, groupByKey, exprEvaluatorContext);
            }
        }

        public override void ApplyLeaveInternal(
            EventBean[] eventsPerStream,
            object compositeGroupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var groupKeyPerLevel = (object[])compositeGroupByKey;
            foreach (var groupByKey in groupKeyPerLevel) {
                ApplyLeaveTableKey(eventsPerStream, groupByKey, exprEvaluatorContext);
            }
        }
    }
} // end of namespace
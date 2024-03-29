///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    public class AggSvcGroupByWTableImpl : AggSvcGroupByWTableBase
    {
        public AggSvcGroupByWTableImpl(
            TableInstanceGrouped table,
            TableColumnMethodPairEval[] methodPairs,
            AggregationMultiFunctionAgent[] accessAgents,
            int[] accessColumnsZeroOffset)
            : base(
                table,
                methodPairs,
                accessAgents,
                accessColumnsZeroOffset)
        {
        }

        public override void ApplyEnterInternal(
            EventBean[] eventsPerStream,
            object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ApplyEnterGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
        }

        public override void ApplyLeaveInternal(
            EventBean[] eventsPerStream,
            object groupByKey,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            ApplyLeaveGroupKey(eventsPerStream, groupByKey, exprEvaluatorContext);
        }
    }
} // end of namespace
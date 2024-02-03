///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.client.hook.aggmultifunc
{
    /// <summary>
    ///     Agents change multi-function aggregation state.
    /// </summary>
    public interface AggregationMultiFunctionAgent
    {
        /// <summary>
        ///     Enter-into (add to) an aggregation
        /// </summary>
        /// <param name="eventsPerStream">events</param>
        /// <param name="exprEvaluatorContext">evaluation context</param>
        /// <param name="row">aggregation row</param>
        /// <param name="column">column assigned to the aggregation state</param>
        void ApplyEnter(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column);

        /// <summary>
        ///     Leave-from (remove from) an aggregation
        /// </summary>
        /// <param name="eventsPerStream">events</param>
        /// <param name="exprEvaluatorContext">evaluation context</param>
        /// <param name="row">aggregation row</param>
        /// <param name="column">column assigned to the aggregation state</param>
        void ApplyLeave(
            EventBean[] eventsPerStream,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationRow row,
            int column);
    }
} // end of namespace
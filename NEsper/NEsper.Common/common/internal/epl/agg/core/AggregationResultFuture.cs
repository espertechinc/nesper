///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    ///     Interface for use by aggregate expression nodes representing aggregate functions such as 'sum' or 'avg' to use
    ///     to obtain the current value for the function at time of expression evaluation.
    /// </summary>
    public interface AggregationResultFuture
    {
        object GetValue(
            int column,
            int agentInstanceId,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        ICollection<EventBean> GetCollectionOfEvents(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        EventBean GetEventBean(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        object GetGroupKey(int agentInstanceId);

        ICollection<object> GetGroupKeys(ExprEvaluatorContext exprEvaluatorContext);

        ICollection<object> GetCollectionScalar(
            int column,
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext context);

        AggregationService GetContextPartitionAggregationService(int agentInstanceId);

        /// <summary>
        ///     Set the current aggregation state row - for use when evaluation nodes are asked to evaluate.
        /// </summary>
        /// <param name="groupKey">single key identifying the row of aggregation states</param>
        /// <param name="agentInstanceId">context partition id</param>
        /// <param name="rollupLevel">rollup level</param>
        void SetCurrentAccess(
            object groupKey,
            int agentInstanceId,
            AggregationGroupByRollupLevel rollupLevel);
    }
} // end of namespace
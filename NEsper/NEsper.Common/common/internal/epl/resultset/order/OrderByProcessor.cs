///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.agg.rollup;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.resultset.order
{
    /// <summary>
    /// A processor for ordering output events according to the order specified in the order-by clause.
    /// </summary>
    public interface OrderByProcessor
    {
        /// <summary>
        /// Sort the output events. If the order-by processor needs group-by
        /// keys to evaluate the expressions in the order-by clause, these will
        /// be computed from the generating events.
        /// </summary>
        /// <param name="outgoingEvents">the events to be sorted</param>
        /// <param name="generatingEvents">the events that generated the output events (each event has a corresponding array of generating events per different event streams)</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="aggregationService">aggregation svc</param>
        /// <returns>an array containing the output events in sorted order</returns>
        EventBean[] SortPlain(
            EventBean[] outgoingEvents,
            EventBean[][] generatingEvents,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService);

        /// <summary>
        /// Sort the output events, using the provided group-by keys for
        /// evaluating grouped aggregation functions, and avoiding the cost of
        /// recomputing the keys.
        /// </summary>
        /// <param name="outgoingEvents">the events to sort</param>
        /// <param name="generatingEvents">the events that generated the output events (each event has a corresponding array of generating events per different event streams)</param>
        /// <param name="groupByKeys">the keys to use for determining the group-by group of output events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="exprEvaluatorContext">context for expression evaluation</param>
        /// <param name="aggregationService">aggregation svc</param>
        /// <returns>an array containing the output events in sorted order</returns>
        EventBean[] SortWGroupKeys(
            EventBean[] outgoingEvents,
            EventBean[][] generatingEvents,
            object[] groupByKeys,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationService aggregationService);

        /// <summary>
        /// Sort the output events, using the provided group-by keys for
        /// evaluating grouped aggregation functions, and avoiding the cost of
        /// recomputing the keys.
        /// </summary>
        /// <param name="outgoingEvents">the events to sort</param>
        /// <param name="currentGenerators">the events that generated the output events (each event has a corresponding array of generating events per different event streams)</param>
        /// <param name="newData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="agentInstanceContext">context for expression evaluation</param>
        /// <param name="aggregationService">aggregation svc</param>
        /// <returns>an array containing the output events in sorted order</returns>
        EventBean[] SortRollup(
            EventBean[] outgoingEvents,
            IList<GroupByRollupKey> currentGenerators,
            bool newData,
            AgentInstanceContext agentInstanceContext,
            AggregationService aggregationService);

        /// <summary>
        /// Returns the sort key for a given row.
        /// </summary>
        /// <param name="eventsPerStream">is the row consisting of one event per stream</param>
        /// <param name="isNewData">is true for new data</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>sort key</returns>
        object GetSortKey(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Returns the sort key for a given row for rollup.
        /// </summary>
        /// <param name="eventsPerStream">is the row consisting of one event per stream</param>
        /// <param name="isNewData">is true for new data</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <param name="level">rollup level</param>
        /// <returns>sort key</returns>
        object GetSortKeyRollup(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext,
            AggregationGroupByRollupLevel level);

        /// <summary>
        /// Sort a given array of outgoing events using the sort keys returning a sorted outgoing event array.
        /// </summary>
        /// <param name="outgoingEvents">is the events to sort</param>
        /// <param name="orderKeys">is the keys to sort by</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>sorted events</returns>
        EventBean[] SortWOrderKeys(
            EventBean[] outgoingEvents,
            object[] orderKeys,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Sort two keys and events
        /// </summary>
        /// <param name="first">first</param>
        /// <param name="sortKeyFirst">sort key first</param>
        /// <param name="second">second</param>
        /// <param name="sortKeySecond">sort key seconds</param>
        /// <returns>sorted</returns>
        EventBean[] SortTwoKeys(
            EventBean first,
            object sortKeyFirst,
            EventBean second,
            object sortKeySecond);
    }
} // end of namespace
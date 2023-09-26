///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

namespace com.espertech.esper.common.@internal.epl.agg.core
{
    /// <summary>
    /// Service for maintaining aggregation state. Processes events entering (a window, a join etc,)
    /// and events leaving. Answers questions about current aggregation state for a given row.
    /// </summary>
    public interface AggregationService : AggregationResultFuture,
        StopCallback
    {
        /// <summary>
        /// Apply events as entering a window (new events).
        /// </summary>
        /// <param name="eventsPerStream">events for each stream entering window</param>
        /// <param name="optionalGroupKeyPerRow">can be null if grouping without keys is desired, else the keys or array of keys to use for grouping, each distinct key value results in a new row of aggregation state.</param>
        /// <param name="exprEvaluatorContext">context for expression evaluatiom</param>
        void ApplyEnter(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Apply events as leaving a window (old events).
        /// </summary>
        /// <param name="eventsPerStream">events for each stream entering window</param>
        /// <param name="optionalGroupKeyPerRow">can be null if grouping without keys is desired, else the keys or array of keys to use for grouping, each distinct key value results in a new row of aggregation state.</param>
        /// <param name="exprEvaluatorContext">context for expression evaluatiom</param>
        void ApplyLeave(
            EventBean[] eventsPerStream,
            object optionalGroupKeyPerRow,
            ExprEvaluatorContext exprEvaluatorContext);

        /// <summary>
        /// Clear current aggregation state.
        /// </summary>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        void ClearResults(ExprEvaluatorContext exprEvaluatorContext);

        void SetRemovedCallback(AggregationRowRemovedCallback callback);

        void Accept(AggregationServiceVisitor visitor);
        void AcceptGroupDetail(AggregationServiceVisitorWGroupDetail visitor);
        bool IsGrouped { get; }
    }
}
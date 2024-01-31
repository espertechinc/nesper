///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    /// <summary>
    ///     Processor for result sets coming from 2 sources. First, out of a simple view (no join).
    ///     And second, out of a join of event streams. The processor must apply the select-clause, grou-by-clause and
    ///     having-clauses
    ///     as supplied. It must state what the event type of the result rows is.
    /// </summary>
    public interface ResultSetProcessor : StopCallback
    {
        /// <summary>
        ///     Returns the event type of processed results.
        /// </summary>
        /// <returns>event type of the resulting events posted by the processor.</returns>
        EventType ResultEventType { get; }

        /// <summary>
        ///     For use by views posting their result, process the event rows that are entered and removed (new and old events).
        ///     Processes according to select-clauses, group-by clauses and having-clauses and returns new events and
        ///     old events as specified.
        /// </summary>
        /// <param name="newData">new events posted by view</param>
        /// <param name="oldData">old events posted by view</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <returns>pair of new events and old events</returns>
        UniformPair<EventBean[]> ProcessViewResult(
            EventBean[] newData,
            EventBean[] oldData,
            bool isSynthesize);

        /// <summary>
        ///     For use by joins posting their result, process the event rows that are entered and removed (new and old events).
        ///     Processes according to select-clauses, group-by clauses and having-clauses and returns new events and
        ///     old events as specified.
        /// </summary>
        /// <param name="newEvents">new events posted by join</param>
        /// <param name="oldEvents">old events posted by join</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <returns>pair of new events and old events</returns>
        UniformPair<EventBean[]> ProcessJoinResult(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isSynthesize);

        /// <summary>
        ///     Returns the iterator implementing the group-by and aggregation and order-by logic
        ///     specific to each case of use of these construct.
        /// </summary>
        /// <param name="parent">is the parent view iterator</param>
        /// <returns>event iterator</returns>
        IEnumerator<EventBean> GetEnumerator(Viewable parent);

        /// <summary>
        ///     Returns the iterator for iterating over a join-result.
        /// </summary>
        /// <param name="joinSet">is the join result set</param>
        /// <returns>iterator over join results</returns>
        IEnumerator<EventBean> GetEnumerator(ISet<MultiKeyArrayOfKeys<EventBean>> joinSet);

        /// <summary>
        ///     Clear out current state.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Processes batched events in case of output-rate limiting.
        /// </summary>
        /// <param name="joinEventsSet">the join results</param>
        /// <param name="generateSynthetic">flag to indicate whether synthetic events must be generated</param>
        /// <returns>results for dispatch</returns>
        UniformPair<EventBean[]> ProcessOutputLimitedJoin(
            IList<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> joinEventsSet,
            bool generateSynthetic);

        /// <summary>
        ///     Processes batched events in case of output-rate limiting.
        /// </summary>
        /// <param name="viewEventsList">the view results</param>
        /// <param name="generateSynthetic">flag to indicate whether synthetic events must be generated</param>
        /// <returns>results for dispatch</returns>
        UniformPair<EventBean[]> ProcessOutputLimitedView(
            IList<UniformPair<EventBean[]>> viewEventsList,
            bool generateSynthetic);

        ExprEvaluatorContext ExprEvaluatorContext { get; set; }

        void ApplyViewResult(
            EventBean[] newData,
            EventBean[] oldData);

        void ApplyJoinResult(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents);

        void ProcessOutputLimitedLastAllNonBufferedView(
            EventBean[] newData,
            EventBean[] oldData,
            bool isGenerateSynthetic);

        void ProcessOutputLimitedLastAllNonBufferedJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            bool isGenerateSynthetic);

        UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize);

        UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize);

        void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor);
    }
} // end of namespace
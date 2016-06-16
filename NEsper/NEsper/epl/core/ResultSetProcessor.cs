///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.spec;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Processor for result sets coming from 2 sources. First, out of a simple view (no join). 
    /// And second, out of a join of event streams. The processor must apply the select-clause,
    /// group-by-clause and having-clauses as supplied. It must state what the event type of 
    /// the result rows is.
    /// </summary>
    public interface ResultSetProcessor : StopCallback
    {
        /// <summary>
        /// Returns the event type of processed results.
        /// </summary>
        /// <value>event type of the resulting events posted by the processor.</value>
        EventType ResultEventType { get; }

        /// <summary>
        /// For use by views posting their result, process the event rows that are entered and removed 
        /// (new and old events). Processes according to select-clauses, group-by clauses and having-clauses 
        /// and returns new events and old events as specified.
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
        /// For use by joins posting their result, process the event rows that are entered and removed
        /// (new and old events). Processes according to select-clauses, group-by clauses and having-clauses
        /// and returns new events and old events as specified.
        /// </summary>
        /// <param name="newEvents">new events posted by join</param>
        /// <param name="oldEvents">old events posted by join</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <returns>pair of new events and old events</returns>
        UniformPair<EventBean[]> ProcessJoinResult(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            bool isSynthesize);

        /// <summary>
        /// Returns the enumerator implementing the group-by and aggregation and order-by logic
        /// specific to each case of use of these construct.
        /// </summary>
        /// <param name="parent">is the parent view iterator</param>
        /// <returns>event iterator</returns>
        IEnumerator<EventBean> GetEnumerator(Viewable parent);
    
        /// <summary>Returns the iterator for iterating over a join-result. </summary>
        /// <param name="joinSet">is the join result set</param>
        /// <returns>iterator over join results</returns>
        IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet);
    
        /// <summary>Clear out current state. </summary>
        void Clear();

        /// <summary>Processes batched events in case of output-rate limiting. </summary>
        /// <param name="joinEventsSet">the join results</param>
        /// <param name="generateSynthetic">flag to indicate whether synthetic events must be generated</param>
        /// <param name="outputLimitLimitType">the type of output rate limiting</param>
        /// <returns>results for dispatch</returns>
        UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet,
                                                          bool generateSynthetic,
                                                          OutputLimitLimitType outputLimitLimitType);

        /// <summary>Processes batched events in case of output-rate limiting. </summary>
        /// <param name="viewEventsList">the view results</param>
        /// <param name="generateSynthetic">flag to indicate whether synthetic events must be generated</param>
        /// <param name="outputLimitLimitType">the type of output rate limiting</param>
        /// <returns>results for dispatch</returns>
        UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList,
                                                          bool generateSynthetic,
                                                          OutputLimitLimitType outputLimitLimitType);

        bool HasAggregation { get; }

        /// <summary>
        /// Sets the agent instance context.
        /// </summary>
        /// <value>The context.</value>
        AgentInstanceContext AgentInstanceContext { set; }

        void ApplyViewResult(EventBean[] newData, EventBean[] oldData);

        void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents);

        void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll);

        void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll);

        UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll);

        UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll);
    }
}

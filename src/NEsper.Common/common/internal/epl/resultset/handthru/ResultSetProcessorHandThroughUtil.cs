///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.select.core;

namespace com.espertech.esper.common.@internal.epl.resultset.handthru
{
    public class ResultSetProcessorHandThroughUtil
    {
        public const string METHOD_GETSELECTEVENTSNOHAVINGHANDTHRUVIEW = "GetSelectEventsNoHavingHandThruView";
        public const string METHOD_GETSELECTEVENTSNOHAVINGHANDTHRUJOIN = "GetSelectEventsNoHavingHandThruJoin";

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="agentInstanceContext">context</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectEventsNoHavingHandThruView(
            SelectExprProcessor exprProcessor,
            EventBean[] events,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext agentInstanceContext)
        {
            if (events == null)
            {
                return null;
            }

            EventBean[] result = new EventBean[events.Length];
            EventBean[] eventsPerStream = new EventBean[1];
            for (int i = 0; i < events.Length; i++)
            {
                eventsPerStream[0] = events[i];
                result[i] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, agentInstanceContext);
            }

            return result;
        }

        /// <summary>
        /// NOTE: Code-generation-invoked method, method name and parameter order matters
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the
        /// same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="agentInstanceContext">context</param>
        /// <returns>output events, one for each input event</returns>
        public static EventBean[] GetSelectEventsNoHavingHandThruJoin(
            SelectExprProcessor exprProcessor,
            ISet<MultiKeyArrayOfKeys<EventBean>> events,
            bool isNewData,
            bool isSynthesize,
            ExprEvaluatorContext agentInstanceContext)
        {
            int length = events.Count;
            if (length == 0)
            {
                return null;
            }

            EventBean[] result = new EventBean[length];
            int count = 0;
            foreach (MultiKeyArrayOfKeys<EventBean> key in events)
            {
                EventBean[] eventsPerStream = key.Array;
                result[count] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, agentInstanceContext);
                count++;
            }

            return result;
        }
    }
} // end of namespace
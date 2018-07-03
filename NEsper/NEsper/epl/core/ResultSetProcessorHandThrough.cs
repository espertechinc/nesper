///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor for the hand-through case: no aggregation functions used
    /// in the select clause, and no group-by, no having and ordering.
    /// </summary>
    public class ResultSetProcessorHandThrough : ResultSetProcessorBaseSimple
    {
        private readonly ResultSetProcessorHandThroughFactory _prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private AgentInstanceContext _agentInstanceContext;
    
        public ResultSetProcessorHandThrough(ResultSetProcessorHandThroughFactory prototype, SelectExprProcessor selectExprProcessor, AgentInstanceContext agentInstanceContext)
        {
            _prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _agentInstanceContext = agentInstanceContext;
        }

        public override AgentInstanceContext AgentInstanceContext
        {
            set { _agentInstanceContext = value; }
            get { return _agentInstanceContext; }
        }

        public override EventType ResultEventType
        {
            get { return _prototype.ResultEventType; }
        }

        public override UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;
    
            if (_prototype.IsSelectRStream)
            {
                selectOldEvents = GetSelectEventsNoHaving(_selectExprProcessor, oldEvents, false, isSynthesize, _agentInstanceContext);
            }
            selectNewEvents = GetSelectEventsNoHaving(_selectExprProcessor, newEvents, true, isSynthesize, _agentInstanceContext);
    
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        public override UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            EventBean[] selectOldEvents = null;
    
            if (_prototype.IsSelectRStream)
            {
                selectOldEvents = GetSelectEventsNoHaving(_selectExprProcessor, oldData, false, isSynthesize, _agentInstanceContext);
            }
            EventBean[] selectNewEvents = GetSelectEventsNoHaving(_selectExprProcessor, newData, true, isSynthesize, _agentInstanceContext);
    
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectEventsNoHaving(SelectExprProcessor exprProcessor, EventBean[] events, bool isNewData, bool isSynthesize, ExprEvaluatorContext agentInstanceContext)
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
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the same, i.e. this method does not filter it just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <returns>
        /// output events, one for each input event
        /// </returns>
        internal static EventBean[] GetSelectEventsNoHaving(SelectExprProcessor exprProcessor, ICollection<MultiKey<EventBean>> events, bool isNewData, bool isSynthesize, ExprEvaluatorContext agentInstanceContext)
        {
            int length = events.Count;
            if (length == 0)
            {
                return null;
            }
    
            EventBean[] result = new EventBean[length];
            int count = 0;
            foreach (MultiKey<EventBean> key in events)
            {
                EventBean[] eventsPerStream = key.Array;
                result[count] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, agentInstanceContext);
                count++;
            }
    
            return result;
        }
    
        public override void Clear()
        {
            // No need to clear state, there is no state held
        }
    
        public override IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            // Return an iterator that gives row-by-row a result
            var transform = new ResultSetProcessorSimpleTransform(this);
            return parent.Select(transform.Transform).GetEnumerator();
        }

        public override IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            // Process join results set as a regular join, includes sorting and having-clause filter
            UniformPair<EventBean[]> result = ProcessJoinResult(joinSet, CollectionUtil.EMPTY_ROW_SET, true);
            if ((result == null) || (result.First == null))
                return EnumerationHelper.Empty<EventBean>(); 
            return ((IEnumerable<EventBean>)result.First).GetEnumerator();
        }

        public override bool HasAggregation
        {
            get { return false; }
        }


        public override void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
        }

        public override void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
        }


        public override void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
        }

        public override void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll)
        {
        }

        public override UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
            return null;
        }

        public override UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
            return null;
        }

        public override void Stop()
        {
            // no action required
        }

        public override void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
        {
            // nothing to visit
        }
    }
}

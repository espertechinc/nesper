///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    public class ResultSetProcessorUtil
    {
        public static void ApplyAggViewResult(
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext,
            EventBean[] newData,
            EventBean[] oldData,
            EventBean[] eventsPerStream)
        {
            if (newData != null)
            {
                // apply new data to aggregates
                for (int i = 0; i < newData.Length; i++)
                {
                    eventsPerStream[0] = newData[i];
                    aggregationService.ApplyEnter(eventsPerStream, null, exprEvaluatorContext);
                }
            }
            if (oldData != null)
            {
                // apply old data to aggregates
                for (int i = 0; i < oldData.Length; i++)
                {
                    eventsPerStream[0] = oldData[i];
                    aggregationService.ApplyLeave(eventsPerStream, null, exprEvaluatorContext);
                }
            }
        }

        public static void ApplyAggJoinResult(
            AggregationService aggregationService,
            ExprEvaluatorContext exprEvaluatorContext,
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents)
        {
            if (!newEvents.IsEmpty())
            {
                // apply new data to aggregates
                foreach (MultiKey<EventBean> events in newEvents)
                {
                    aggregationService.ApplyEnter(events.Array, null, exprEvaluatorContext);
                }
            }
            if (!oldEvents.IsEmpty())
            {
                // apply old data to aggregates
                foreach (MultiKey<EventBean> events in oldEvents)
                {
                    aggregationService.ApplyLeave(events.Array, null, exprEvaluatorContext);
                }
            }
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of 
        /// events stays the same, i.e. this method does not filter e just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectEventsNoHaving(SelectExprProcessor exprProcessor, EventBean[] events, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }
    
            var result = new EventBean[events.Length];
            var eventsPerStream = new EventBean[1];
            for (var i = 0; i < events.Length; i++) {
                eventsPerStream[0] = events[i];
                result[i] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
            }
            return result;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays 
        /// the same, i.e. this method does not filter e just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">orders the outgoing events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectEventsNoHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, EventBean[] events, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }
    
            var result = new EventBean[events.Length];
            var eventGenerators = new EventBean[events.Length][];
    
            var eventsPerStream = new EventBean[1];
            for (var i = 0; i < events.Length; i++) {
                eventsPerStream[0] = events[i];
                result[i] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                eventGenerators[i] = new EventBean[] {events[i]};
            }
    
            return orderByProcessor.Sort(result, eventGenerators, isNewData, exprEvaluatorContext);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays 
        /// the same, i.e. this method does not filter e just transforms the result set.
        /// <para/>
        /// Also applies a having clause.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">for sorting output events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectEventsHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, EventBean[] events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }
    
            ArrayDeque<EventBean> result = null;
            ArrayDeque<EventBean[]> eventGenerators = null;
    
            var eventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);

            foreach (var theEvent in events)
            {
                eventsPerStream[0] = theEvent;
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean());}
                if ((passesHaving == null) || (false.Equals(passesHaving))) {
                    continue;
                }
    
                var generated = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (generated != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Length);
                        eventGenerators = new ArrayDeque<EventBean[]>(events.Length);
                    }
                    result.Add(generated);
                    eventGenerators.Add(new EventBean[]{theEvent});
                }
            }
    
            if (result != null) {
                return orderByProcessor.Sort(result.ToArray(), eventGenerators.ToArray(), isNewData, exprEvaluatorContext);
            }
            return null;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays 
        /// the same, i.e. this method does not filter e just transforms the result set.
        /// <para/>
        /// Also applies a having clause.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectEventsHaving(SelectExprProcessor exprProcessor,
                                                           EventBean[] events,
                                                           ExprEvaluator havingNode,
                                                           bool isNewData,
                                                           bool isSynthesize,
                                                           ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return null;
            }
    
            ArrayDeque<EventBean> result = null;
            var eventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
    
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving))) {
                    continue;
                }
    
                var generated = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (generated != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Length);
                    }
                    result.Add(generated);
                }
            }
    
            if (result != null) {
                return result.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the 
        /// same, i.e. this method does not filter e just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">for sorting output events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectJoinEventsNoHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, ICollection<MultiKey<EventBean>> events, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((events == null) || (events.IsEmpty())) {
                return null;
            }
    
            var result = new EventBean[events.Count];
            var eventGenerators = new EventBean[events.Count][];
    
            var count = 0;
            foreach (var key in events) {
                var eventsPerStream = key.Array;
                result[count] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                eventGenerators[count] = eventsPerStream;
                count++;
            }
    
            return orderByProcessor.Sort(result, eventGenerators, isNewData, exprEvaluatorContext);
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the 
        /// same, i.e. this method does not filter e just transforms the result set.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectJoinEventsNoHaving(SelectExprProcessor exprProcessor, ICollection<MultiKey<EventBean>> events, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((events == null) || (events.IsEmpty())) {
                return null;
            }
    
            var result = new EventBean[events.Count];
            var count = 0;
    
            foreach (var key in events) {
                var eventsPerStream = key.Array;
                result[count] = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                count++;
            }
    
            return result;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the 
        /// same, i.e. this method does not filter e just transforms the result set.
        /// <para/>
        /// Also applies a having clause.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectJoinEventsHaving(SelectExprProcessor exprProcessor, ICollection<MultiKey<EventBean>> events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((events == null) || (events.IsEmpty())) {
                return null;
            }
    
            ArrayDeque<EventBean> result = null;
    
            foreach (var key in events) {
                var eventsPerStream = key.Array;
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving)))
                {
                    continue;
                }
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Count);
                    }
                    result.Add(resultEvent);
                }
            }
    
            if (result != null) {
                return result.ToArray();
            }
            return null;
        }

        /// <summary>
        /// Applies the select-clause to the given events returning the selected events. The number of events stays the 
        /// same, i.e. this method does not filter e just transforms the result set. <para/>Also applies a having clause.
        /// </summary>
        /// <param name="exprProcessor">processes each input event and returns output event</param>
        /// <param name="orderByProcessor">for sorting output events according to the order-by clause</param>
        /// <param name="events">input events</param>
        /// <param name="havingNode">supplies the having-clause expression</param>
        /// <param name="isNewData">indicates whether we are dealing with new data (istream) or old data (rstream)</param>
        /// <param name="isSynthesize">set to true to indicate that synthetic events are required for an iterator result set</param>
        /// <param name="exprEvaluatorContext">context for expression evalauation</param>
        /// <returns>output events, one for each input event</returns>
        internal static EventBean[] GetSelectJoinEventsHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, ICollection<MultiKey<EventBean>> events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((events == null) || (events.IsEmpty())) {
                return null;
            }
    
            ArrayDeque<EventBean> result = null;
            ArrayDeque<EventBean[]> eventGenerators = null;
    
            foreach (var key in events) {
                var eventsPerStream = key.Array;
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving)))
                {
                    continue;
                }
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    if (result == null) {
                        result = new ArrayDeque<EventBean>(events.Count);
                        eventGenerators = new ArrayDeque<EventBean[]>(events.Count);
                    }
                    result.Add(resultEvent);
                    eventGenerators.Add(eventsPerStream);
                }
            }
    
            if (result != null) {
                return orderByProcessor.Sort(result.ToArray(), eventGenerators.ToArray(), isNewData, exprEvaluatorContext);
            }
            return null;
        }
    
        internal static void PopulateSelectEventsNoHaving(SelectExprProcessor exprProcessor, EventBean[] events, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }
    
            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        internal static void PopulateSelectEventsNoHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, EventBean[] events, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ICollection<Object> sortKeys, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }
    
            var eventsPerStream = new EventBean[1];
            foreach (var theEvent in events) {
                eventsPerStream[0] = theEvent;
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    sortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        internal static void PopulateSelectEventsHaving(SelectExprProcessor exprProcessor, EventBean[] events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }
    
            var eventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            foreach (var theEvent in events)
            {
                eventsPerStream[0] = theEvent;
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving)))
                {
                    continue;
                }
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        internal static void PopulateSelectEventsHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, EventBean[] events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ICollection<Object> optSortKeys, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }
    
            var eventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
            foreach (var theEvent in events)
            {
                eventsPerStream[0] = theEvent;
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(theEvent);}
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving)))
                {
                    continue;
                }
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    optSortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        internal static void PopulateSelectJoinEventsHaving(SelectExprProcessor exprProcessor, ICollection<MultiKey<EventBean>> events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }
    
            foreach (var key in events) {
                var eventsPerStream = key.Array;
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving))) {
                    continue;
                }
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        internal static void PopulateSelectJoinEventsHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, ICollection<MultiKey<EventBean>> events, ExprEvaluator havingNode, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ICollection<Object> sortKeys, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (events == null) {
                return;
            }
    
            foreach (var key in events) {
                var eventsPerStream = key.Array;
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, exprEvaluatorContext);
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var passesHaving = havingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(passesHaving.AsBoxedBoolean()); }
                if ((passesHaving == null) || (false.Equals(passesHaving))) {
                    continue;
                }
    
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    sortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        internal static void PopulateSelectJoinEventsNoHaving(SelectExprProcessor exprProcessor, ICollection<MultiKey<EventBean>> events, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ExprEvaluatorContext exprEvaluatorContext)
        {
            var length = (events != null) ? events.Count : 0;
            if (length == 0) {
                return;
            }
    
            foreach (var key in events) {
                var eventsPerStream = key.Array;
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                }
            }
        }

        internal static void PopulateSelectJoinEventsNoHavingWithOrderBy(SelectExprProcessor exprProcessor, OrderByProcessor orderByProcessor, ICollection<MultiKey<EventBean>> events, bool isNewData, bool isSynthesize, ICollection<EventBean> result, ICollection<Object> optSortKeys, ExprEvaluatorContext exprEvaluatorContext)
        {
            var length = (events != null) ? events.Count : 0;
            if (length == 0) {
                return;
            }
    
            foreach (var key in events)
            {
                var eventsPerStream = key.Array;
                var resultEvent = exprProcessor.Process(eventsPerStream, isNewData, isSynthesize, exprEvaluatorContext);
                if (resultEvent != null) {
                    result.Add(resultEvent);
                    optSortKeys.Add(orderByProcessor.GetSortKey(eventsPerStream, isNewData, exprEvaluatorContext));
                }
            }
        }

        public static void ClearAndAggregateUngrouped(ExprEvaluatorContext exprEvaluatorContext, AggregationService aggregationService, Viewable parent)
        {
            aggregationService.ClearResults(exprEvaluatorContext);
            var ee = parent.GetEnumerator();
            var eventsPerStream = new EventBean[1];
            while(ee.MoveNext())
            {
                eventsPerStream[0] = ee.Current;
                aggregationService.ApplyEnter(eventsPerStream, null, exprEvaluatorContext);
            }
        }

        public static ArrayDeque<EventBean> EnumeratorToDeque(IEnumerator<EventBean> e)
        {
            var deque = new ArrayDeque<EventBean>();
            while(e.MoveNext())
            {
                deque.Add(e.Current);
            }
            return deque;
        }
    }
}

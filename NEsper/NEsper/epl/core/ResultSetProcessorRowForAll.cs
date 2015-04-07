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
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor for the case: aggregation functions used in the select clause,
    /// and no group-by, and all properties in the select clause are under an aggregation function.
    /// <para/>
    /// This processor does not perform grouping, every event entering and leaving is in the same
    /// group. Produces one old event and one new event row every time either at least one old or 
    /// new event is received. Aggregation state is simply one row holding all the state.
    /// </summary>
    public class ResultSetProcessorRowForAll : ResultSetProcessor
    {
        private readonly ResultSetProcessorRowForAllFactory _prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly AggregationService _aggregationService;
        private ExprEvaluatorContext _exprEvaluatorContext;
    
        public ResultSetProcessorRowForAll(ResultSetProcessorRowForAllFactory prototype, SelectExprProcessor selectExprProcessor, OrderByProcessor orderByProcessor, AggregationService aggregationService, ExprEvaluatorContext exprEvaluatorContext)
        {
            _prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _orderByProcessor = orderByProcessor;
            _aggregationService = aggregationService;
            _exprEvaluatorContext = exprEvaluatorContext;
        }

        public AgentInstanceContext AgentInstanceContext
        {
            set { _exprEvaluatorContext = value; }
            get { return (AgentInstanceContext) _exprEvaluatorContext; }
        }

        public EventType ResultEventType
        {
            get { return _prototype.ResultEventType; }
        }

        public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessUngroupedFullyAgg();}
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;
    
            if (_prototype.IsUnidirectional)
            {
                Clear();
            }
    
            if (_prototype.IsSelectRStream)
            {
                selectOldEvents = GetSelectListEvents(false, isSynthesize, true);
            }

            ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);
    
            selectNewEvents = GetSelectListEvents(true, isSynthesize, true);
    
            if ((selectNewEvents == null) && (selectOldEvents == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedFullyAgg(null, null);}
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedFullyAgg(selectNewEvents, selectOldEvents);}
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        public UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessUngroupedFullyAgg();}
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;
    
            if (_prototype.IsSelectRStream)
            {
                selectOldEvents = GetSelectListEvents(false, isSynthesize, false);
            }
    
            EventBean[] eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, eventsPerStream);
    
            // generate new events using select expressions
            selectNewEvents = GetSelectListEvents(true, isSynthesize, false);
    
            if ((selectNewEvents == null) && (selectOldEvents == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedFullyAgg(null, null);}
                return null;
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedFullyAgg(selectNewEvents, selectOldEvents);}
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        private EventBean[] GetSelectListEvents(bool isNewData, bool isSynthesize, bool join)
        {
            if (_prototype.OptionalHavingNode != null)
            {
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(null); else InstrumentationHelper.Get().QHavingClauseJoin(null);}
                var result = _prototype.OptionalHavingNode.Evaluate(new EvaluateParams(null, isNewData, _exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                if ((result == null) || (false.Equals(result)))
                {
                    return null;
                }
            }
    
            // Since we are dealing with strictly aggregation nodes, there are no events required for evaluating
            EventBean theEvent = _selectExprProcessor.Process(CollectionUtil.EVENTBEANARRAY_EMPTY, isNewData, isSynthesize, _exprEvaluatorContext);
    
            // The result is always a single row
            return new EventBean[] {theEvent};
        }
    
        private EventBean GetSelectListEvent(bool isNewData, bool isSynthesize, bool join)
        {
            if (_prototype.OptionalHavingNode != null)
            {
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(null); else InstrumentationHelper.Get().QHavingClauseJoin(null);}
                var result = _prototype.OptionalHavingNode.Evaluate(new EvaluateParams(null, isNewData, _exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
    
                if ((result == null) || (false.Equals(result)))
                {
                    return null;
                }
            }
    
            // Since we are dealing with strictly aggregation nodes, there are no events required for evaluating
            EventBean theEvent = _selectExprProcessor.Process(CollectionUtil.EVENTBEANARRAY_EMPTY, isNewData, isSynthesize, _exprEvaluatorContext);
    
            // The result is always a single row
            return theEvent;
        }
    
        public IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (!_prototype.IsHistoricalOnly)
            {
                return ObtainEnumerator();
            }

            ResultSetProcessorUtil.ClearAndAggregateUngrouped(_exprEvaluatorContext, _aggregationService, parent);

            var enumerator = ObtainEnumerator();
            _aggregationService.ClearResults(_exprEvaluatorContext);
            return enumerator;
        }

        public IEnumerator<EventBean> ObtainEnumerator()
        {
            EventBean[] selectNewEvents = GetSelectListEvents(true, true, false);
            if (selectNewEvents != null)
            {
                return EnumerationHelper<EventBean>.CreateSingletonEnumerator(selectNewEvents[0]);
            }

            return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            EventBean[] result = GetSelectListEvents(true, true, true);
            return ((IEnumerable<EventBean>) result).GetEnumerator();
        }
    
        public void Clear()
        {
            _aggregationService.ClearResults(_exprEvaluatorContext);
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.LAST)
            {
                EventBean lastOldEvent = null;
                EventBean lastNewEvent = null;
    
                // if empty (nothing to post)
                if (joinEventsSet.IsEmpty())
                {
                    if (_prototype.IsSelectRStream)
                    {
                        lastOldEvent = GetSelectListEvent(false, generateSynthetic, true);
                        lastNewEvent = lastOldEvent;
                    }
                    else
                    {
                        lastNewEvent = GetSelectListEvent(false, generateSynthetic, true);
                    }
                }
    
                foreach (UniformPair<ISet<MultiKey<EventBean>>> pair in joinEventsSet)
                {
                    if (_prototype.IsUnidirectional)
                    {
                        Clear();
                    }
    
                    ICollection<MultiKey<EventBean>> newData = pair.First;
                    ICollection<MultiKey<EventBean>> oldData = pair.Second;
    
                    if ((lastOldEvent == null) && (_prototype.IsSelectRStream))
                    {
                        lastOldEvent = GetSelectListEvent(false, generateSynthetic, true);
                    }
    
                    if (newData != null)
                    {
                        // apply new data to aggregates
                        foreach (MultiKey<EventBean> eventsPerStream in newData)
                        {
                            _aggregationService.ApplyEnter(eventsPerStream.Array, null, _exprEvaluatorContext);
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        foreach (MultiKey<EventBean> eventsPerStream in oldData)
                        {
                            _aggregationService.ApplyLeave(eventsPerStream.Array, null, _exprEvaluatorContext);
                        }
                    }
    
                    lastNewEvent = GetSelectListEvent(true, generateSynthetic, true);
                }
    
                EventBean[] lastNew = (lastNewEvent != null) ? new EventBean[] {lastNewEvent} : null;
                EventBean[] lastOld = (lastOldEvent != null) ? new EventBean[] {lastOldEvent} : null;
    
                if ((lastNew == null) && (lastOld == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(lastNew, lastOld);
            }
            else
            {
                ICollection<EventBean> newEvents = new List<EventBean>();
                ICollection<EventBean> oldEvents = null;
                if (_prototype.IsSelectRStream)
                {
                    oldEvents = new LinkedList<EventBean>();
                }

                ICollection<Object> newEventsSortKey = null;
                ICollection<Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedList<Object>();
                    if (_prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedList<Object>();
                    }
                }
    
                foreach (UniformPair<ISet<MultiKey<EventBean>>> pair in joinEventsSet)
                {
                    if (_prototype.IsUnidirectional)
                    {
                        Clear();
                    }
    
                    ICollection<MultiKey<EventBean>> newData = pair.First;
                    ICollection<MultiKey<EventBean>> oldData = pair.Second;
    
                    if (_prototype.IsSelectRStream)
                    {
                        GetSelectListEvent(false, generateSynthetic, oldEvents, true);
                    }
    
                    if (newData != null)
                    {
                        // apply new data to aggregates
                        foreach (MultiKey<EventBean> row in newData)
                        {
                            _aggregationService.ApplyEnter(row.Array, null, _exprEvaluatorContext);
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        foreach (MultiKey<EventBean> row in oldData)
                        {
                            _aggregationService.ApplyLeave(row.Array, null, _exprEvaluatorContext);
                        }
                    }
    
                    GetSelectListEvent(false, generateSynthetic, newEvents, true);
                }
    
                EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
                EventBean[] oldEventsArr = null;
                if (_prototype.IsSelectRStream)
                {
                    oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
                }
    
                if (_orderByProcessor != null)
                {
                    Object[] sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _exprEvaluatorContext);
                    if (_prototype.IsSelectRStream)
                    {
                        Object[] sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
                        oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _exprEvaluatorContext);
                    }
                }
    
                if (joinEventsSet.IsEmpty())
                {
                    if (_prototype.IsSelectRStream)
                    {
                        oldEventsArr = GetSelectListEvents(false, generateSynthetic, true);
                    }
                    newEventsArr = GetSelectListEvents(true, generateSynthetic, true);
                }
    
                if ((newEventsArr == null) && (oldEventsArr == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
            }
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.LAST)
            {
                // For last, if there are no events:
                //   As insert stream, return the current value, if matching the having clause
                //   As remove stream, return the current value, if matching the having clause
                // For last, if there are events in the batch:
                //   As insert stream, return the newest value that is matching the having clause
                //   As remove stream, return the oldest value that is matching the having clause
    
                EventBean lastOldEvent = null;
                EventBean lastNewEvent = null;
                EventBean[] eventsPerStream = new EventBean[1];
    
                // if empty (nothing to post)
                if (viewEventsList.IsEmpty())
                {
                    if (_prototype.IsSelectRStream)
                    {
                        lastOldEvent = GetSelectListEvent(false, generateSynthetic, false);
                        lastNewEvent = lastOldEvent;
                    }
                    else
                    {
                        lastNewEvent = GetSelectListEvent(false, generateSynthetic, false);
                    }
                }
    
                foreach (UniformPair<EventBean[]> pair in viewEventsList)
                {
                    EventBean[] newData = pair.First;
                    EventBean[] oldData = pair.Second;
    
                    if ((lastOldEvent == null) && (_prototype.IsSelectRStream))
                    {
                        lastOldEvent = GetSelectListEvent(false, generateSynthetic, false);
                    }
    
                    if (newData != null)
                    {
                        // apply new data to aggregates
                        foreach (EventBean aNewData in newData)
                        {
                            eventsPerStream[0] = aNewData;
                            _aggregationService.ApplyEnter(eventsPerStream, null, _exprEvaluatorContext);
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        foreach (EventBean anOldData in oldData)
                        {
                            eventsPerStream[0] = anOldData;
                            _aggregationService.ApplyLeave(eventsPerStream, null, _exprEvaluatorContext);
                        }
                    }
    
                    lastNewEvent = GetSelectListEvent(false, generateSynthetic, false);
                }
    
                EventBean[] lastNew = (lastNewEvent != null) ? new EventBean[] {lastNewEvent} : null;
                EventBean[] lastOld = (lastOldEvent != null) ? new EventBean[] {lastOldEvent} : null;
    
                if ((lastNew == null) && (lastOld == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(lastNew, lastOld);
            }
            else
            {
                ICollection<EventBean> newEvents = new List<EventBean>();
                ICollection<EventBean> oldEvents = null;
                if (_prototype.IsSelectRStream)
                {
                    oldEvents = new LinkedList<EventBean>();
                }

                ICollection<Object> newEventsSortKey = null;
                ICollection<Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedList<Object>();
                    if (_prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedList<Object>();
                    }
                }
    
                foreach (UniformPair<EventBean[]> pair in viewEventsList)
                {
                    EventBean[] newData = pair.First;
                    EventBean[] oldData = pair.Second;
    
                    if (_prototype.IsSelectRStream)
                    {
                        GetSelectListEvent(false, generateSynthetic, oldEvents, false);
                    }
    
                    EventBean[] eventsPerStream = new EventBean[1];
                    if (newData != null)
                    {
                        // apply new data to aggregates
                        foreach (EventBean aNewData in newData)
                        {
                            eventsPerStream[0] = aNewData;
                            _aggregationService.ApplyEnter(eventsPerStream, null, _exprEvaluatorContext);
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        foreach (EventBean anOldData in oldData)
                        {
                            eventsPerStream[0] = anOldData;
                            _aggregationService.ApplyLeave(eventsPerStream, null, _exprEvaluatorContext);
                        }
                    }
    
                    GetSelectListEvent(true, generateSynthetic, newEvents, false);
                }
    
                EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
                EventBean[] oldEventsArr = null;
                if (_prototype.IsSelectRStream)
                {
                    oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
                }
                if (_orderByProcessor != null)
                {
                    Object[] sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _exprEvaluatorContext);
                    if (_prototype.IsSelectRStream)
                    {
                        Object[] sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
                        oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _exprEvaluatorContext);
                    }
                }
    
                if (viewEventsList.IsEmpty())
                {
                    if (_prototype.IsSelectRStream)
                    {
                        oldEventsArr = GetSelectListEvents(false, generateSynthetic, false);
                    }
                    newEventsArr = GetSelectListEvents(true, generateSynthetic, false);
                }
    
                if ((newEventsArr == null) && (oldEventsArr == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
            }
        }

        public bool HasAggregation
        {
            get { return true; }
        }

        public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            EventBean[] events = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, events);
        }

        public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);
        }
    
        private void GetSelectListEvent(bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, bool join)
        {
            if (_prototype.OptionalHavingNode != null)
            {
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(null); else InstrumentationHelper.Get().QHavingClauseJoin(null);}
                var result = _prototype.OptionalHavingNode.Evaluate(new EvaluateParams(null, isNewData, _exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                if ((result == null) || (false.Equals(result)))
                {
                    return;
                }
            }
    
            // Since we are dealing with strictly aggregation nodes, there are no events required for evaluating
            EventBean theEvent = _selectExprProcessor.Process(CollectionUtil.EVENTBEANARRAY_EMPTY, isNewData, isSynthesize, _exprEvaluatorContext);
    
            resultEvents.Add(theEvent);
        }
    }
}

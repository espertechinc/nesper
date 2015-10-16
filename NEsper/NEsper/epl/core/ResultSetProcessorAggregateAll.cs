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
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result set processor for the case: aggregation functions used in the select clause, 
    /// and no group-by, and not all of the properties in the select clause are under an 
    /// aggregation function.
    /// <para/>
    /// This processor does not perform grouping, every event entering and leaving is in the 
    /// same group. The processor generates one row for each event entering (new event) and 
    /// one row for each event leaving (old event). Aggregation state is simply one row holding 
    /// all the state.
    /// </summary>
    public class ResultSetProcessorAggregateAll : ResultSetProcessor
    {
        private readonly ResultSetProcessorAggregateAllFactory _prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly AggregationService _aggregationService; 
        private ExprEvaluatorContext _exprEvaluatorContext;
        private ResultSetProcessorAggregateAllOutputLastHelper _outputLastUnordHelper;
        private ResultSetProcessorAggregateAllOutputAllHelper _outputAllUnordHelper;
    
        public ResultSetProcessorAggregateAll(ResultSetProcessorAggregateAllFactory prototype, SelectExprProcessor selectExprProcessor, OrderByProcessor orderByProcessor, AggregationService aggregationService, ExprEvaluatorContext exprEvaluatorContext)
        {
            _prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _orderByProcessor = orderByProcessor;
            _aggregationService = aggregationService;
            _exprEvaluatorContext = exprEvaluatorContext;
            _outputLastUnordHelper = prototype.IsOutputLast ? new ResultSetProcessorAggregateAllOutputLastHelper(this) : null;
            _outputAllUnordHelper = prototype.IsOutputAll ? new ResultSetProcessorAggregateAllOutputAllHelper(this) : null;
        }

        public AgentInstanceContext AgentInstanceContext
        {
            set { _exprEvaluatorContext = value; }
        }

        /// <summary>Returns the select expression processor </summary>
        /// <value>select processor.</value>
        public SelectExprProcessor SelectExprProcessor
        {
            get { return _selectExprProcessor; }
        }

        /// <summary>Returns the optional having expression. </summary>
        /// <value>having expression node</value>
        public ExprEvaluator OptionalHavingNode
        {
            get { return _prototype.OptionalHavingNode; }
        }

        public EventType ResultEventType
        {
            get { return _prototype.ResultEventType; }
        }

        public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            EventBean[] eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, eventsPerStream);
        }

        public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);
        }

        public UniformPair<EventBean[]> ProcessJoinResult(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QResultSetProcessUngroupedNonfullyAgg();
            }
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;

            if (_prototype.IsUnidirectional)
            {
                Clear();
            }

            ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);
    
            if (_prototype.OptionalHavingNode == null)
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, oldEvents, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldEvents, false, isSynthesize, _exprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, newEvents, true, isSynthesize, _exprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newEvents, true, isSynthesize, _exprEvaluatorContext);
                }
            }
            else
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, oldEvents, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldEvents, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, newEvents, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newEvents, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
            }
    
            if ((selectNewEvents == null) && (selectOldEvents == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(null, null);}
                return null;
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(selectNewEvents, selectOldEvents);}
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        public UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessUngroupedNonfullyAgg();}
            EventBean[] selectOldEvents = null;
            EventBean[] selectNewEvents;
    
            EventBean[] eventsPerStream = new EventBean[1];
            ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, eventsPerStream);

            // generate new events using select expressions
            if (_prototype.OptionalHavingNode == null)
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, oldData, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, isSynthesize, _exprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, newData, true, isSynthesize, _exprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, isSynthesize, _exprEvaluatorContext);
                }
            }
            else
            {
                if (_prototype.IsSelectRStream)
                {
                    if (_orderByProcessor == null) {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                    else {
                        selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, _prototype.OptionalHavingNode, false, isSynthesize, _exprEvaluatorContext);
                    }
                }
    
                if (_orderByProcessor == null) {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
                else {
                    selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, _prototype.OptionalHavingNode, true, isSynthesize, _exprEvaluatorContext);
                }
            }
    
            if ((selectNewEvents == null) && (selectOldEvents == null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(null, null);}
                return null;
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessUngroupedNonfullyAgg(selectNewEvents, selectOldEvents);}
            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
        }
    
        public IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (!_prototype.IsHistoricalOnly)
            {
                return ObtainIterator(parent);
            }

            ResultSetProcessorUtil.ClearAndAggregateUngrouped(_exprEvaluatorContext, _aggregationService, parent);
            ArrayDeque<EventBean> deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainIterator(parent));
            _aggregationService.ClearResults(_exprEvaluatorContext);
            return deque.GetEnumerator();
        }

        public IEnumerator<EventBean> ObtainIterator(Viewable parent)
        {
            if (_orderByProcessor == null)
            {
                return ResultSetAggregateAllEnumerator.New(parent, this, _exprEvaluatorContext);
            }
    
            // Pull all parent events, generate order keys
            EventBean[] eventsPerStream = new EventBean[1];
            IList<EventBean> outgoingEvents = new List<EventBean>();
            IList<Object> orderKeys = new List<Object>();

            var evaluateParams = new EvaluateParams(eventsPerStream, true, _exprEvaluatorContext);
            foreach (EventBean candidate in parent)
            {
                eventsPerStream[0] = candidate;
    
                bool? pass = true;
                if (_prototype.OptionalHavingNode != null)
                {
                    pass = (bool?) _prototype.OptionalHavingNode.Evaluate(evaluateParams);
                }
                if ((pass == null) || (!pass.Value))
                {
                    continue;
                }
    
                outgoingEvents.Add(_selectExprProcessor.Process(eventsPerStream, true, true, _exprEvaluatorContext));
    
                Object orderKey = _orderByProcessor.GetSortKey(eventsPerStream, true, _exprEvaluatorContext);
                orderKeys.Add(orderKey);
            }
    
            // sort
            EventBean[] outgoingEventsArr = outgoingEvents.ToArray();
            Object[] orderKeysArr = orderKeys.ToArray();
            EventBean[] orderedEvents = _orderByProcessor.Sort(outgoingEventsArr, orderKeysArr, _exprEvaluatorContext);
    
            return ((IEnumerable<EventBean>) orderedEvents).GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            EventBean[] result;
            if (_prototype.OptionalHavingNode == null)
            {
                if (_orderByProcessor == null) {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, joinSet, true, true, _exprEvaluatorContext);
                }
                else {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, joinSet, true, true, _exprEvaluatorContext);
                }
            }
            else
            {
                if (_orderByProcessor == null) {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, joinSet, _prototype.OptionalHavingNode, true, true, _exprEvaluatorContext);
                }
                else {
                    result = ResultSetProcessorUtil.GetSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, joinSet, _prototype.OptionalHavingNode, true, true, _exprEvaluatorContext);
                }
            }
            if (result == null)
                return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
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
    
                foreach (UniformPair<ISet<MultiKey<EventBean>>> pair in joinEventsSet)
                {
                    ICollection<MultiKey<EventBean>> newData = pair.First;
                    ICollection<MultiKey<EventBean>> oldData = pair.Second;
    
                    if (_prototype.IsUnidirectional)
                    {
                        Clear();
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
    
                    EventBean[] selectOldEvents;
                    if (_prototype.IsSelectRStream)
                    {
                        if (_prototype.OptionalHavingNode == null)
                        {
                            selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, _exprEvaluatorContext);
                        }
                        else
                        {
                            selectOldEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, _exprEvaluatorContext);
                        }
                        if ((selectOldEvents != null) && (selectOldEvents.Length > 0))
                        {
                            lastOldEvent = selectOldEvents[selectOldEvents.Length - 1];
                        }
                    }
    
                    // generate new events using select expressions
                    EventBean[] selectNewEvents;
                    if (_prototype.OptionalHavingNode == null)
                    {
                        selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectNewEvents = ResultSetProcessorUtil.GetSelectJoinEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, _exprEvaluatorContext);
                    }
                    if ((selectNewEvents != null) && (selectNewEvents.Length > 0))
                    {
                        lastNewEvent = selectNewEvents[selectNewEvents.Length - 1];
                    }
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
                IList<EventBean> newEvents = new List<EventBean>();
                IList<EventBean> oldEvents = null;
                if (_prototype.IsSelectRStream)
                {
                    oldEvents = new List<EventBean>();
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
                    ICollection<MultiKey<EventBean>> newData = pair.First;
                    ICollection<MultiKey<EventBean>> oldData = pair.Second;
    
                    if (_prototype.IsUnidirectional)
                    {
                        Clear();
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
    
                    // generate old events using select expressions
                    if (_prototype.IsSelectRStream)
                    {
                        if (_prototype.OptionalHavingNode == null)
                        {
                            if (_orderByProcessor == null) {
                                ResultSetProcessorUtil.PopulateSelectJoinEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                            }
                            else {
                                ResultSetProcessorUtil.PopulateSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                            }
                        }
                        // generate old events using having then select
                        else
                        {
                            if (_orderByProcessor == null) {
                                ResultSetProcessorUtil.PopulateSelectJoinEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                            }
                            else {
                                ResultSetProcessorUtil.PopulateSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                            }
                        }
                    }
    
                    // generate new events using select expressions
                    if (_prototype.OptionalHavingNode == null)
                    {
                        if (_orderByProcessor == null) {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                        }
                        else {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                        }
                    }
                    else
                    {
                        if (_orderByProcessor == null) {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                        }
                        else {
                            ResultSetProcessorUtil.PopulateSelectJoinEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                        }
                    }
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
                EventBean lastOldEvent = null;
                EventBean lastNewEvent = null;
                EventBean[] eventsPerStream = new EventBean[1];
    
                foreach (UniformPair<EventBean[]> pair in viewEventsList)
                {
                    EventBean[] newData = pair.First;
                    EventBean[] oldData = pair.Second;
    
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
                            _aggregationService.ApplyLeave(eventsPerStream, null,_exprEvaluatorContext);
                        }
                    }
    
                    EventBean[] selectOldEvents;
                    if (_prototype.IsSelectRStream)
                    {
                        if (_prototype.OptionalHavingNode == null)
                        {
                            selectOldEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, _exprEvaluatorContext);
                        }
                        else
                        {
                            selectOldEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, _exprEvaluatorContext);
                        }
                        if ((selectOldEvents != null) && (selectOldEvents.Length > 0))
                        {
                            lastOldEvent = selectOldEvents[selectOldEvents.Length - 1];
                        }
                    }
    
                    // generate new events using select expressions
                    EventBean[] selectNewEvents;
                    if (_prototype.OptionalHavingNode == null)
                    {
                        selectNewEvents = ResultSetProcessorUtil.GetSelectEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, _exprEvaluatorContext);
                    }
                    else
                    {
                        selectNewEvents = ResultSetProcessorUtil.GetSelectEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, _exprEvaluatorContext);
                    }
                    if ((selectNewEvents != null) && (selectNewEvents.Length > 0))
                    {
                        lastNewEvent = selectNewEvents[selectNewEvents.Length - 1];
                    }
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
    
                    // generate old events using select expressions
                    if (_prototype.IsSelectRStream)
                    {
                        if (_prototype.OptionalHavingNode == null)
                        {
                            if (_orderByProcessor == null) {
                                ResultSetProcessorUtil.PopulateSelectEventsNoHaving(_selectExprProcessor, oldData, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                            }
                            else {
                                ResultSetProcessorUtil.PopulateSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                            }
                        }
                        // generate old events using having then select
                        else
                        {
                            if (_orderByProcessor == null) {
                                ResultSetProcessorUtil.PopulateSelectEventsHaving(_selectExprProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, _exprEvaluatorContext);
                            }
                            else {
                                ResultSetProcessorUtil.PopulateSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, oldData, _prototype.OptionalHavingNode, false, generateSynthetic, oldEvents, oldEventsSortKey, _exprEvaluatorContext);
                            }
                        }
                    }
    
                    // generate new events using select expressions
                    if (_prototype.OptionalHavingNode == null)
                    {
                        if (_orderByProcessor == null) {
                            ResultSetProcessorUtil.PopulateSelectEventsNoHaving(_selectExprProcessor, newData, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                        }
                        else {
                            ResultSetProcessorUtil.PopulateSelectEventsNoHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                        }
                    }
                    else
                    {
                        if (_orderByProcessor == null) {
                            ResultSetProcessorUtil.PopulateSelectEventsHaving(_selectExprProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, _exprEvaluatorContext);
                        }
                        else {
                            ResultSetProcessorUtil.PopulateSelectEventsHavingWithOrderBy(_selectExprProcessor, _orderByProcessor, newData, _prototype.OptionalHavingNode, true, generateSynthetic, newEvents, newEventsSortKey, _exprEvaluatorContext);
                        }
                    }
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


        public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllUnordHelper.ProcessView(newData, oldData, isGenerateSynthetic);
            }
            else
            {
                _outputLastUnordHelper.ProcessView(newData, oldData, isGenerateSynthetic);
            }
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllUnordHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
            }
            else
            {
                _outputLastUnordHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
            }
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllUnordHelper.Output();
            }
            return _outputLastUnordHelper.Output();
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllUnordHelper.Output();
            }
            return _outputLastUnordHelper.Output();
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Result set processor for the case: aggregation functions used in the select clause, and no group-by,
	/// and all properties in the select clause are under an aggregation function.
	/// <para />This processor does not perform grouping, every event entering and leaving is in the same group.
	/// Produces one old event and one new event row every time either at least one old or new event is received.
	/// Aggregation state is simply one row holding all the state.
	/// </summary>
	public class ResultSetProcessorRowForAll : ResultSetProcessor
	{
	    protected internal readonly ResultSetProcessorRowForAllFactory _prototype;
	    private readonly SelectExprProcessor _selectExprProcessor;
	    private readonly OrderByProcessor _orderByProcessor;
	    protected internal readonly AggregationService _aggregationService;
        protected internal ExprEvaluatorContext _exprEvaluatorContext;
	    private readonly ResultSetProcessorRowForAllOutputLastHelper _outputLastHelper;
        private readonly ResultSetProcessorRowForAllOutputAllHelper _outputAllHelper;

	    public ResultSetProcessorRowForAll(
	        ResultSetProcessorRowForAllFactory prototype,
	        SelectExprProcessor selectExprProcessor,
	        OrderByProcessor orderByProcessor,
	        AggregationService aggregationService,
	        AgentInstanceContext agentInstanceContext)
        {
	        _prototype = prototype;
	        _selectExprProcessor = selectExprProcessor;
	        _orderByProcessor = orderByProcessor;
	        _aggregationService = aggregationService;
	        _exprEvaluatorContext = agentInstanceContext;
	        if (prototype.IsOutputLast) {
	            _outputLastHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowForAllOutputLast(this, prototype, agentInstanceContext);
	        }
	        else if (prototype.IsOutputAll) {
	            _outputAllHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowForAllOutputAll(this, prototype, agentInstanceContext);
	        }
	    }

	    public ResultSetProcessorRowForAllFactory Prototype
	    {
	        get { return _prototype; }
	    }

	    public AgentInstanceContext AgentInstanceContext
	    {
	        set { _exprEvaluatorContext = value; }
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

	        var eventsPerStream = new EventBean[1];
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

	    public EventBean[] GetSelectListEvents(bool isNewData, bool isSynthesize, bool join)
	    {
	        if (_prototype.OptionalHavingNode != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(null); else InstrumentationHelper.Get().QHavingClauseJoin(null);}
	            var result = _prototype.OptionalHavingNode.Evaluate(new EvaluateParams(null, isNewData, _exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	            if ((result == null) || (false.Equals(result)))
	            {
	                return null;
	            }
	        }

	        // Since we are dealing with strictly aggregation nodes, there are no events required for evaluating
	        var theEvent = _selectExprProcessor.Process(CollectionUtil.EVENTBEANARRAY_EMPTY, isNewData, isSynthesize, _exprEvaluatorContext);

	        // The result is always a single row
	        return new EventBean[] {theEvent};
	    }

	    private EventBean GetSelectListEvent(bool isNewData, bool isSynthesize, bool join)
	    {
	        if (_prototype.OptionalHavingNode != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(null); else InstrumentationHelper.Get().QHavingClauseJoin(null);}
	            var result = _prototype.OptionalHavingNode.Evaluate(new EvaluateParams(null, isNewData, _exprEvaluatorContext));
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}

	            if ((result == null) || (false.Equals(result)))
	            {
	                return null;
	            }
	        }

	        // Since we are dealing with strictly aggregation nodes, there are no events required for evaluating
	        var theEvent = _selectExprProcessor.Process(CollectionUtil.EVENTBEANARRAY_EMPTY, isNewData, isSynthesize, _exprEvaluatorContext);

	        // The result is always a single row
	        return theEvent;
	    }

        public static readonly IList<EventBean> EMPTY_EVENT_BEAN_LIST = new EventBean[0]; 

	    public IEnumerator<EventBean> GetEnumerator(Viewable parent)
	    {
	        if (!_prototype.IsHistoricalOnly) {
	            return ObtainEnumerator();
	        }

	        ResultSetProcessorUtil.ClearAndAggregateUngrouped(_exprEvaluatorContext, _aggregationService, parent);

	        var iterator = ObtainEnumerator();
	        _aggregationService.ClearResults(_exprEvaluatorContext);
	        return iterator;
	    }

	    public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
	    {
	        var result = GetSelectListEvents(true, true, true) ?? EMPTY_EVENT_BEAN_LIST;
	        return result.GetEnumerator();
	    }

	    public void Clear()
	    {
	        _aggregationService.ClearResults(_exprEvaluatorContext);
	    }

	    public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
	    {
	        if (outputLimitLimitType == OutputLimitLimitType.LAST) {
	            return ProcessOutputLimitedJoinLast(joinEventsSet, generateSynthetic);
	        }
	        else {
	            return ProcessOutputLimitedJoinDefault(joinEventsSet, generateSynthetic);
	        }
	    }

	    public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
	    {
	        if (outputLimitLimitType == OutputLimitLimitType.LAST) {
	            return ProcessOutputLimitedViewLast(viewEventsList, generateSynthetic);
	        }
	        else {
	            return ProcessOutputLimitedViewDefault(viewEventsList, generateSynthetic);
	        }
	    }

	    public bool HasAggregation
	    {
	        get { return true; }
	    }

	    public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
	        var events = new EventBean[1];
	        ResultSetProcessorUtil.ApplyAggViewResult(_aggregationService, _exprEvaluatorContext, newData, oldData, events);
	    }

	    public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
	        ResultSetProcessorUtil.ApplyAggJoinResult(_aggregationService, _exprEvaluatorContext, newEvents, oldEvents);
	    }

	    public AggregationService AggregationService
	    {
	        get { return _aggregationService; }
	    }

	    public void Stop() {
	        if (_outputLastHelper != null) {
	            _outputLastHelper.Destroy();
	        }
	        if (_outputAllHelper != null) {
	            _outputAllHelper.Destroy();
	        }
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll) {
	        if (isAll) {
	            _outputAllHelper.ProcessView(newData, oldData, isGenerateSynthetic);
	        }
	        else {
	            _outputLastHelper.ProcessView(newData, oldData, isGenerateSynthetic);
	        }
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll) {
	        if (isAll) {
	            _outputAllHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
	        }
	        else {
	            _outputLastHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
	        }
	    }

	    public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
	        if (isAll) {
	            return _outputAllHelper.OutputView(isSynthesize);
	        }
	        return _outputLastHelper.OutputView(isSynthesize);
	    }

	    public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
	        if (isAll) {
	            return _outputAllHelper.OutputJoin(isSynthesize);
	        }
	        return _outputLastHelper.OutputJoin(isSynthesize);
	    }

        public void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
        {
            if (_outputLastHelper != null)
            {
                visitor.Visit(_outputLastHelper);
            }
            if (_outputAllHelper != null)
            {
                visitor.Visit(_outputAllHelper);
            }
        }

	    private void GetSelectListEvent(bool isNewData, bool isSynthesize, IList<EventBean> resultEvents, bool join)
	    {
	        if (_prototype.OptionalHavingNode != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(null); else InstrumentationHelper.Get().QHavingClauseJoin(null);}
	            var result = _prototype.OptionalHavingNode.Evaluate(new EvaluateParams(null, isNewData, _exprEvaluatorContext));
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	            if ((result == null) || (false.Equals(result)))
	            {
	                return;
	            }
	        }

	        // Since we are dealing with strictly aggregation nodes, there are no events required for evaluating
	        var theEvent = _selectExprProcessor.Process(CollectionUtil.EVENTBEANARRAY_EMPTY, isNewData, isSynthesize, _exprEvaluatorContext);

	        resultEvents.Add(theEvent);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedJoinDefault(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
	        IList<EventBean> newEvents = new List<EventBean>();
	        IList<EventBean> oldEvents = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEvents = new List<EventBean>();
	        }

	        IList<object> newEventsSortKey = null;
	        IList<object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new List<object>();
	            if (_prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        foreach (var pair in joinEventsSet)
	        {
	            if (_prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (_prototype.IsSelectRStream)
	            {
	                GetSelectListEvent(false, generateSynthetic, oldEvents, true);
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var row in newData)
	                {
	                    _aggregationService.ApplyEnter(row.Array, null, _exprEvaluatorContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var row in oldData)
	                {
	                    _aggregationService.ApplyLeave(row.Array, null, _exprEvaluatorContext);
	                }
	            }

	            GetSelectListEvent(false, generateSynthetic, newEvents, true);
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _exprEvaluatorContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
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

	    private UniformPair<EventBean[]> ProcessOutputLimitedJoinLast(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
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

	        foreach (var pair in joinEventsSet)
	        {
	            if (_prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            var newData = pair.First;
	            var oldData = pair.Second;

	            if ((lastOldEvent == null) && (_prototype.IsSelectRStream))
	            {
	                lastOldEvent = GetSelectListEvent(false, generateSynthetic, true);
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var eventsPerStream in newData)
	                {
	                    _aggregationService.ApplyEnter(eventsPerStream.Array, null, _exprEvaluatorContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var eventsPerStream in oldData)
	                {
	                    _aggregationService.ApplyLeave(eventsPerStream.Array, null, _exprEvaluatorContext);
	                }
	            }

	            lastNewEvent = GetSelectListEvent(true, generateSynthetic, true);
	        }

	        var lastNew = (lastNewEvent != null) ? new EventBean[] {lastNewEvent} : null;
	        var lastOld = (lastOldEvent != null) ? new EventBean[] {lastOldEvent} : null;

	        if ((lastNew == null) && (lastOld == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(lastNew, lastOld);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedViewDefault(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
	        IList<EventBean> newEvents = new List<EventBean>();
	        IList<EventBean> oldEvents = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEvents = new List<EventBean>();
	        }

	        IList<object> newEventsSortKey = null;
	        IList<object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new List<object>();
	            if (_prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (_prototype.IsSelectRStream)
	            {
	                GetSelectListEvent(false, generateSynthetic, oldEvents, false);
	            }

	            var eventsPerStream = new EventBean[1];
	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var aNewData in newData)
	                {
	                    eventsPerStream[0] = aNewData;
	                    _aggregationService.ApplyEnter(eventsPerStream, null, _exprEvaluatorContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var anOldData in oldData)
	                {
	                    eventsPerStream[0] = anOldData;
	                    _aggregationService.ApplyLeave(eventsPerStream, null, _exprEvaluatorContext);
	                }
	            }

	            GetSelectListEvent(true, generateSynthetic, newEvents, false);
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }
	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _exprEvaluatorContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
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

	    private UniformPair<EventBean[]> ProcessOutputLimitedViewLast(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
	        // For last, if there are no events:
	        //   As insert stream, return the current value, if matching the having clause
	        //   As remove stream, return the current value, if matching the having clause
	        // For last, if there are events in the batch:
	        //   As insert stream, return the newest value that is matching the having clause
	        //   As remove stream, return the oldest value that is matching the having clause

	        EventBean lastOldEvent = null;
	        EventBean lastNewEvent = null;
	        var eventsPerStream = new EventBean[1];

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

	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if ((lastOldEvent == null) && (_prototype.IsSelectRStream))
	            {
	                lastOldEvent = GetSelectListEvent(false, generateSynthetic, false);
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var aNewData in newData)
	                {
	                    eventsPerStream[0] = aNewData;
	                    _aggregationService.ApplyEnter(eventsPerStream, null, _exprEvaluatorContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var anOldData in oldData)
	                {
	                    eventsPerStream[0] = anOldData;
	                    _aggregationService.ApplyLeave(eventsPerStream, null, _exprEvaluatorContext);
	                }
	            }

	            lastNewEvent = GetSelectListEvent(false, generateSynthetic, false);
	        }

	        var lastNew = (lastNewEvent != null) ? new EventBean[] {lastNewEvent} : null;
	        var lastOld = (lastOldEvent != null) ? new EventBean[] {lastOldEvent} : null;

	        if ((lastNew == null) && (lastOld == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(lastNew, lastOld);
	    }

	    private IEnumerator<EventBean> ObtainEnumerator()
	    {
	        var selectNewEvents = GetSelectListEvents(true, true, false);
	        return selectNewEvents != null 
                ? EnumerationHelper.Singleton(selectNewEvents[0])
                : EnumerationHelper.Empty<EventBean>();
	    }
	}
} // end of namespace

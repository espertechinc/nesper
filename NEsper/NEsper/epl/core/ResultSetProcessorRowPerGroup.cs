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
using com.espertech.esper.epl.view;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Result set processor for the fully-grouped case:
	/// there is a group-by and all non-aggregation event properties in the select clause are listed in the group by,
	/// and there are aggregation functions.
	/// <para />Produces one row for each group that changed (and not one row per event). Computes MultiKey group-by keys for
	/// each event and uses a set of the group-by keys to generate the result rows, using the first (old or new, anyone) event
	/// for each distinct group-by key.
	/// </summary>
	public class ResultSetProcessorRowPerGroup 
        : ResultSetProcessor
        , AggregationRowRemovedCallback
    {
	    private readonly ResultSetProcessorRowPerGroupFactory _prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly AggregationService _aggregationService;
        private AgentInstanceContext _agentInstanceContext;

	    // For output rate limiting, keep a representative event for each group for
	    // representing each group in an output limit clause
        private ResultSetProcessorGroupedOutputAllGroupReps _outputAllGroupReps;

        private readonly ResultSetProcessorGroupedOutputFirstHelper _outputFirstHelper;
        private readonly ResultSetProcessorRowPerGroupOutputLastHelper _outputLastHelper;
	    private readonly ResultSetProcessorRowPerGroupOutputAllHelper _outputAllHelper;

	    public ResultSetProcessorRowPerGroup(
	        ResultSetProcessorRowPerGroupFactory prototype,
	        SelectExprProcessor selectExprProcessor,
	        OrderByProcessor orderByProcessor,
	        AggregationService aggregationService,
	        AgentInstanceContext agentInstanceContext)
        {
	        _prototype = prototype;
	        _selectExprProcessor = selectExprProcessor;
	        _orderByProcessor = orderByProcessor;
	        _aggregationService = aggregationService;
	        _agentInstanceContext = agentInstanceContext;

	        aggregationService.SetRemovedCallback(this);

	        if (prototype.IsOutputLast) {
	            _outputLastHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowPerGroupOutputLastOpt(agentInstanceContext, this, prototype);
	        }
	        else if (prototype.IsOutputAll) {
	            if (!prototype.IsEnableOutputLimitOpt) {
	                _outputAllGroupReps = prototype.ResultSetProcessorHelperFactory.MakeRSGroupedOutputAllNoOpt(agentInstanceContext, prototype.GroupKeyNodes, prototype.NumStreams);
	            }
	            else {
	                _outputAllHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowPerGroupOutputAllOpt(agentInstanceContext, this, prototype);
	            }
	        }
	        else if (prototype.IsOutputFirst) {
	            _outputFirstHelper = prototype.ResultSetProcessorHelperFactory.MakeRSGroupedOutputFirst(agentInstanceContext, prototype.GroupKeyNodes, prototype.OptionalOutputFirstConditionFactory, null, -1);
	        }
	    }

	    public OrderByProcessor OrderByProcessor
	    {
	        get { return _orderByProcessor; }
	    }

	    public ResultSetProcessorRowPerGroupFactory Prototype
	    {
	        get { return _prototype; }
	    }

	    public AgentInstanceContext AgentInstanceContext
	    {
            get { return _agentInstanceContext; }
	        set { _agentInstanceContext = value; }
	    }

	    public EventType ResultEventType
	    {
	        get { return _prototype.ResultEventType; }
	    }

	    public virtual void ApplyViewResult(EventBean[] newData, EventBean[] oldData) {
	        var eventsPerStream = new EventBean[1];
	        if (newData != null) {
	            // apply new data to aggregates
	            foreach (var aNewData in newData) {
	                eventsPerStream[0] = aNewData;
	                var mk = GenerateGroupKey(eventsPerStream, true);
	                _aggregationService.ApplyEnter(eventsPerStream, mk, _agentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            // apply old data to aggregates
	            foreach (var anOldData in oldData) {
	                eventsPerStream[0] = anOldData;
	                var mk = GenerateGroupKey(eventsPerStream, false);
	                _aggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
	            }
	        }
	    }

	    public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        if (!newEvents.IsEmpty()) {
	            // apply old data to aggregates
	            foreach (var eventsPerStream in newEvents) {
	                var mk = GenerateGroupKey(eventsPerStream.Array, true);
	                _aggregationService.ApplyEnter(eventsPerStream.Array, mk, _agentInstanceContext);
	            }
	        }
	        if (oldEvents != null && !oldEvents.IsEmpty()) {
	            // apply old data to aggregates
	            foreach (var eventsPerStream in oldEvents) {
	                var mk = GenerateGroupKey(eventsPerStream.Array, false);
	                _aggregationService.ApplyLeave(eventsPerStream.Array, mk, _agentInstanceContext);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}
	        // Generate group-by keys for all events, collect all keys in a set for later event generation
            IDictionary<object, EventBean[]> keysAndEvents = new NullableDictionary<object, EventBean[]>();
	        var newDataMultiKey = GenerateGroupKeys(newEvents, keysAndEvents, true);
	        var oldDataMultiKey = GenerateGroupKeys(oldEvents, keysAndEvents, false);

	        if (_prototype.IsUnidirectional)
	        {
	            Clear();
	        }

	        // generate old events
	        EventBean[] selectOldEvents = null;
	        if (_prototype.IsSelectRStream)
	        {
	            selectOldEvents = GenerateOutputEventsJoin(keysAndEvents, false, isSynthesize);
	        }

	        // update aggregates
	        if (!newEvents.IsEmpty())
	        {
	            // apply old data to aggregates
	            var count = 0;
	            foreach (var eventsPerStream in newEvents)
	            {
	                _aggregationService.ApplyEnter(eventsPerStream.Array, newDataMultiKey[count], _agentInstanceContext);
	                count++;
	            }
	        }
	        if (oldEvents != null && !oldEvents.IsEmpty())
	        {
	            // apply old data to aggregates
	            var count = 0;
	            foreach (var eventsPerStream in oldEvents)
	            {
	                _aggregationService.ApplyLeave(eventsPerStream.Array, oldDataMultiKey[count], _agentInstanceContext);
	                count++;
	            }
	        }

	        // generate new events using select expressions
	        var selectNewEvents = GenerateOutputEventsJoin(keysAndEvents, true, isSynthesize);

	        if ((selectNewEvents != null) || (selectOldEvents != null))
	        {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
	            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
	        return null;
	    }

	    public virtual UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}
	        // Generate group-by keys for all events, collect all keys in a set for later event generation
            IDictionary<object, EventBean> keysAndEvents = new NullableDictionary<object, EventBean>();

	        var newDataMultiKey = GenerateGroupKeys(newData, keysAndEvents, true);
	        var oldDataMultiKey = GenerateGroupKeys(oldData, keysAndEvents, false);

	        EventBean[] selectOldEvents = null;
	        if (_prototype.IsSelectRStream)
	        {
	            selectOldEvents = GenerateOutputEventsView(keysAndEvents, false, isSynthesize);
	        }

	        // update aggregates
	        var eventsPerStream = new EventBean[1];
	        if (newData != null)
	        {
	            // apply new data to aggregates
	            for (var i = 0; i < newData.Length; i++)
	            {
	                eventsPerStream[0] = newData[i];
	                _aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], _agentInstanceContext);
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            for (var i = 0; i < oldData.Length; i++)
	            {
	                eventsPerStream[0] = oldData[i];
	                _aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], _agentInstanceContext);
	            }
	        }

	        // generate new events using select expressions
	        var selectNewEvents = GenerateOutputEventsView(keysAndEvents, true, isSynthesize);
	        if ((selectNewEvents != null) || (selectOldEvents != null))
	        {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
	            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
	        return null;
	    }

	    protected EventBean[] GenerateOutputEventsView(IDictionary<object, EventBean> keysAndEvents, bool isNewData, bool isSynthesize)
	    {
	        var eventsPerStream = new EventBean[1];
	        var events = new EventBean[keysAndEvents.Count];
	        var keys = new object[keysAndEvents.Count];
	        EventBean[][] currentGenerators = null;
	        if(_prototype.IsSorting)
	        {
	            currentGenerators = new EventBean[keysAndEvents.Count][];
	        }

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
            var count = 0;
	        foreach (var entry in keysAndEvents)
	        {
	            // Set the current row of aggregation states
	            _aggregationService.SetCurrentAccess(entry.Key, _agentInstanceContext.AgentInstanceId, null);

	            eventsPerStream[0] = entry.Value;

	            // Filter the having clause
	            if (_prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(entry.Value);}
	                var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            events[count] = _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
	            keys[count] = entry.Key;
	            if(_prototype.IsSorting)
	            {
	                var currentEventsPerStream = new EventBean[] { entry.Value };
	                currentGenerators[count] = currentEventsPerStream;
	            }

	            count++;
	        }

	        // Resize if some rows were filtered out
	        if (count != events.Length)
	        {
	            if (count == 0)
	            {
	                return null;
	            }
	            var outEvents = new EventBean[count];
	            Array.Copy(events, 0, outEvents, 0, count);
	            events = outEvents;

	            if(_prototype.IsSorting)
	            {
	                var outKeys = new object[count];
                    Array.Copy(keys, 0, outKeys, 0, count);
	                keys = outKeys;

	                var outGens = new EventBean[count][];
                    Array.Copy(currentGenerators, 0, outGens, 0, count);
	                currentGenerators = outGens;
	            }
	        }

	        if(_prototype.IsSorting)
	        {
	            events = _orderByProcessor.Sort(events, currentGenerators, keys, isNewData, _agentInstanceContext);
	        }

	        return events;
	    }

	    private void GenerateOutputBatchedRow(IDictionary<object, EventBean> keysAndEvents, bool isNewData, bool isSynthesize, IList<EventBean> resultEvents, IList<object> optSortKeys, AgentInstanceContext agentInstanceContext)
	    {
	        var eventsPerStream = new EventBean[1];
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, agentInstanceContext);

	        foreach (var entry in keysAndEvents)
	        {
	            // Set the current row of aggregation states
	            _aggregationService.SetCurrentAccess(entry.Key, agentInstanceContext.AgentInstanceId, null);

	            eventsPerStream[0] = entry.Value;

	            // Filter the having clause
	            if (_prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(entry.Value);}
	                var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, agentInstanceContext));

	            if(_prototype.IsSorting)
	            {
	                optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, agentInstanceContext));
	            }
	        }
	    }

	    public void GenerateOutputBatchedArr(bool join, IEnumerator<KeyValuePair<object, EventBean[]>> keysAndEvents, bool isNewData, bool isSynthesize, IList<EventBean> resultEvents, IList<object> optSortKeys)
	    {
	        while (keysAndEvents.MoveNext())
            {
	            var entry = keysAndEvents.Current;
	            GenerateOutputBatchedRow(join, entry.Key, entry.Value, isNewData, isSynthesize, resultEvents, optSortKeys);
	        }
	    }

	    private void GenerateOutputBatchedRow(bool join, object mk, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, IList<EventBean> resultEvents, IList<object> optSortKeys)
	    {
	        // Set the current row of aggregation states
	        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);

	        // Filter the having clause
	        if (_prototype.OptionalHavingNode != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
	            var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	            if ((result == null) || (false.Equals(result)))
	            {
	                return;
	            }
	        }

	        resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));

	        if(_prototype.IsSorting)
	        {
	            optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext));
	        }
	    }

	    public EventBean GenerateOutputBatchedNoSortWMap(bool join, object mk, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize)
	    {
	        // Set the current row of aggregation states
	        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);

	        // Filter the having clause
	        if (_prototype.OptionalHavingNode != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
	            var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	            if ((result == null) || (false.Equals(result))) {
	                return null;
	            }
	        }

	        return _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
	    }

	    private EventBean[] GenerateOutputEventsJoin(IDictionary<object, EventBean[]> keysAndEvents, bool isNewData, bool isSynthesize)
	    {
	        var events = new EventBean[keysAndEvents.Count];
	        var keys = new object[keysAndEvents.Count];
	        EventBean[][] currentGenerators = null;
	        if(_prototype.IsSorting)
	        {
	            currentGenerators = new EventBean[keysAndEvents.Count][];
	        }

	        var count = 0;
	        foreach (var entry in keysAndEvents)
	        {
	            _aggregationService.SetCurrentAccess(entry.Key, _agentInstanceContext.AgentInstanceId, null);
	            var eventsPerStream = entry.Value;

	            // Filter the having clause
	            if (_prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
	                var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            events[count] = _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
	            keys[count] = entry.Key;
	            if(_prototype.IsSorting)
	            {
	                currentGenerators[count] = eventsPerStream;
	            }

	            count++;
	        }

	        // Resize if some rows were filtered out
	        if (count != events.Length)
	        {
	            if (count == 0)
	            {
	                return null;
	            }
	            var outEvents = new EventBean[count];
	            Array.Copy(events, 0, outEvents, 0, count);
	            events = outEvents;

	            if(_prototype.IsSorting)
	            {
	                var outKeys = new object[count];
	                Array.Copy(keys, 0, outKeys, 0, count);
	                keys = outKeys;

	                var outGens = new EventBean[count][];
	                Array.Copy(currentGenerators, 0, outGens, 0, count);
	                currentGenerators = outGens;
	            }
	        }

	        if(_prototype.IsSorting)
	        {
	            events =  _orderByProcessor.Sort(events, currentGenerators, keys, isNewData, _agentInstanceContext);
	        }

	        return events;
	    }

	    private object[] GenerateGroupKeys(EventBean[] events, bool isNewData)
	    {
	        if (events == null)
	        {
	            return null;
	        }

	        var eventsPerStream = new EventBean[1];
	        var keys = new object[events.Length];

	        for (var i = 0; i < events.Length; i++)
	        {
	            eventsPerStream[0] = events[i];
	            keys[i] = GenerateGroupKey(eventsPerStream, isNewData);
	        }

	        return keys;
	    }

	    protected object[] GenerateGroupKeys(EventBean[] events, IDictionary<object, EventBean> eventPerKey, bool isNewData)
	    {
	        if (events == null) {
	            return null;
	        }

	        var eventsPerStream = new EventBean[1];
	        var keys = new object[events.Length];

	        for (var i = 0; i < events.Length; i++)
	        {
	            eventsPerStream[0] = events[i];
	            keys[i] = GenerateGroupKey(eventsPerStream, isNewData);
	            eventPerKey.Put(keys[i], events[i]);
	        }

	        return keys;
	    }

	    private object[] GenerateGroupKeys(ISet<MultiKey<EventBean>> resultSet, IDictionary<object, EventBean[]> eventPerKey, bool isNewData)
	    {
	        if (resultSet == null || resultSet.IsEmpty())
	        {
	            return null;
	        }

	        var keys = new object[resultSet.Count];

	        var count = 0;
	        foreach (var eventsPerStream in resultSet)
	        {
	            keys[count] = GenerateGroupKey(eventsPerStream.Array, isNewData);
	            eventPerKey.Put(keys[count], eventsPerStream.Array);

	            count++;
	        }

	        return keys;
	    }

	    /// <summary>
	    /// Returns the optional having expression.
	    /// </summary>
	    /// <value>having expression node</value>
	    public ExprEvaluator OptionalHavingNode
	    {
	        get { return _prototype.OptionalHavingNode; }
	    }

	    /// <summary>
	    /// Returns the select expression processor
	    /// </summary>
	    /// <value>select processor.</value>
	    public SelectExprProcessor SelectExprProcessor
	    {
	        get { return _selectExprProcessor; }
	    }

	    public virtual IEnumerator<EventBean> GetEnumerator(Viewable parent)
	    {
	        if (!_prototype.IsHistoricalOnly) {
	            return ObtainEnumerator(parent);
	        }

	        _aggregationService.ClearResults(_agentInstanceContext);
	        var it = parent.GetEnumerator();
	        var eventsPerStream = new EventBean[1];
	        while(it.MoveNext()) {
	            eventsPerStream[0] = it.Current;
	            var groupKey = GenerateGroupKey(eventsPerStream, true);
	            _aggregationService.ApplyEnter(eventsPerStream, groupKey, _agentInstanceContext);
	        }

	        ArrayDeque<EventBean> deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainEnumerator(parent));
	        _aggregationService.ClearResults(_agentInstanceContext);
	        return deque.GetEnumerator();
	    }

	    public IEnumerator<EventBean> ObtainEnumerator(Viewable parent)
	    {
	        if (_orderByProcessor == null)
	        {
	            return ResultSetRowPerGroupEnumerator.New(parent, this, _aggregationService, _agentInstanceContext);
	        }
	        return GetEnumeratorSorted(parent.GetEnumerator());
	    }

	    private static readonly IList<EventBean> EMPTY_EVENT_BEAN_LIST = new EventBean[0]; 

	    protected IEnumerator<EventBean> GetEnumeratorSorted(IEnumerator<EventBean> parentIter)
        {
	        // Pull all parent events, generate order keys
	        var eventsPerStream = new EventBean[1];
	        IList<EventBean> outgoingEvents = new List<EventBean>();
	        IList<object> orderKeys = new List<object>();
	        ISet<object> priorSeenGroups = new HashSet<object>();

            var evaluateParams = new EvaluateParams(eventsPerStream, true, _agentInstanceContext);
            while (parentIter.MoveNext())
	        {
	            var candidate = parentIter.Current;
	            eventsPerStream[0] = candidate;

	            var groupKey = GenerateGroupKey(eventsPerStream, true);
	            _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, null);

	            if (_prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(candidate);}
	                var pass = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(pass.AsBoolean());}
	                if ((pass == null) || (false.Equals(pass))) {
	                    continue;
	                }
	            }
	            if (priorSeenGroups.Contains(groupKey))
	            {
	                continue;
	            }
	            priorSeenGroups.Add(groupKey);

	            outgoingEvents.Add(_selectExprProcessor.Process(eventsPerStream, true, true, _agentInstanceContext));

	            var orderKey = _orderByProcessor.GetSortKey(eventsPerStream, true, _agentInstanceContext);
	            orderKeys.Add(orderKey);
	        }

	        // sort
	        var outgoingEventsArr = outgoingEvents.ToArray();
	        var orderKeysArr = orderKeys.ToArray();
	        var orderedEvents = _orderByProcessor.Sort(outgoingEventsArr, orderKeysArr, _agentInstanceContext) ?? EMPTY_EVENT_BEAN_LIST;

	        return orderedEvents.GetEnumerator();
        }

	    public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
	    {
            IDictionary<object, EventBean[]> keysAndEvents = new NullableDictionary<object, EventBean[]>();
	        GenerateGroupKeys(joinSet, keysAndEvents, true);
	        var selectNewEvents = GenerateOutputEventsJoin(keysAndEvents, true, true) ?? EMPTY_EVENT_BEAN_LIST;
	        return selectNewEvents.GetEnumerator();
	    }

	    public void Clear()
	    {
	        _aggregationService.ClearResults(_agentInstanceContext);
	    }

	    public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
	    {
	        if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
	            return ProcessOutputLimitedJoinDefault(joinEventsSet, generateSynthetic);
	        }
	        else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
	            return ProcessOutputLimitedJoinAll(joinEventsSet, generateSynthetic);
	        }
	        else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
	            return ProcessOutputLimitedJoinFirst(joinEventsSet, generateSynthetic);
	        }
	        else if (outputLimitLimitType == OutputLimitLimitType.LAST) {
	            return ProcessOutputLimitedJoinLast(joinEventsSet, generateSynthetic);
	        }
	        throw new IllegalStateException("Unrecognized output limit type " + outputLimitLimitType);
	    }

	    public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
	    {
	        if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
	            return ProcessOutputLimitedViewDefault(viewEventsList, generateSynthetic);
	        }
	        else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
	            return ProcessOutputLimitedViewAll(viewEventsList, generateSynthetic);
	        }
	        else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
	            return ProcessOutputLimitedViewFirst(viewEventsList, generateSynthetic);
	        }
	        else if (outputLimitLimitType == OutputLimitLimitType.LAST) {
	            return ProcessOutputLimitedViewLast(viewEventsList, generateSynthetic);
	        }
	        throw new IllegalStateException("Unrecognized output limit type " + outputLimitLimitType);
	    }

	    public bool HasAggregation
	    {
	        get { return true; }
	    }

	    public void Removed(object key) {
	        if (_outputAllGroupReps != null) {
	            _outputAllGroupReps.Remove(key);
	        }
	        if (_outputLastHelper != null) {
	            _outputLastHelper.Remove(key);
	        }
	        if (_outputAllGroupReps != null) {
	            _outputAllGroupReps.Remove(key);
	        }
	        if (_outputFirstHelper != null) {
	            _outputFirstHelper.Remove(key);
	        }
	    }

	    public object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData)
        {
	        var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
	        if (InstrumentationHelper.ENABLED) {
	            InstrumentationHelper.Get().QResultSetProcessComputeGroupKeys(isNewData, _prototype.GroupKeyNodeExpressions, eventsPerStream);
	            object keyObject;
	            if (_prototype.GroupKeyNode != null) {
	                keyObject = _prototype.GroupKeyNode.Evaluate(evaluateParams);
	            }
	            else {
	                var evals = _prototype.GroupKeyNodes;
	                var keys = new object[evals.Length];
	                for (var i = 0; i < evals.Length; i++) {
	                    keys[i] = evals[i].Evaluate(evaluateParams);
	                }
	                keyObject = new MultiKeyUntyped(keys);
	            }

	            InstrumentationHelper.Get().AResultSetProcessComputeGroupKeys(isNewData, keyObject);
	            return keyObject;
	        }

	        if (_prototype.GroupKeyNode != null) {
	            return _prototype.GroupKeyNode.Evaluate(evaluateParams);
	        }
	        else {
	            var evals = _prototype.GroupKeyNodes;
	            var keys = new object[evals.Length];
	            for (var i = 0; i < evals.Length; i++) {
	                keys[i] = evals[i].Evaluate(evaluateParams);
	            }
	            return new MultiKeyUntyped(keys);
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

	    public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic, bool isAll) {
	        if (isAll) {
	            _outputAllHelper.ProcessJoin(newData, oldData, isGenerateSynthetic);
	        }
	        else {
	            _outputLastHelper.ProcessJoin(newData, oldData, isGenerateSynthetic);
	        }
	    }

	    public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll) {
	        if (isAll) {
	            return _outputAllHelper.OutputView(isSynthesize);
	        }
	        return _outputLastHelper.OutputView(isSynthesize);
	    }

	    public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll) {
	        if (isAll) {
	            return _outputAllHelper.OutputJoin(isSynthesize);
	        }
	        return _outputLastHelper.OutputJoin(isSynthesize);
	    }

	    public virtual void Stop()
        {
	        if (_outputAllGroupReps != null) {
	            _outputAllGroupReps.Destroy();
	        }
	        if (_outputAllHelper != null) {
	            _outputAllHelper.Destroy();
	        }
	        if (_outputLastHelper != null) {
	            _outputLastHelper.Destroy();
	        }
	        if (_outputFirstHelper != null) {
	            _outputFirstHelper.Destroy();
	        }
	    }

	    public AggregationService AggregationService
	    {
	        get { return _aggregationService; }
	    }

        public void AcceptHelperVisitor(ResultSetProcessorOutputHelperVisitor visitor)
        {
            if (_outputAllGroupReps != null)
            {
                visitor.Visit(_outputAllGroupReps);
            }
            if (_outputFirstHelper != null)
            {
                visitor.Visit(_outputFirstHelper);
            }
            if (_outputLastHelper != null)
            {
                visitor.Visit(_outputLastHelper);
            }
            if (_outputAllHelper != null)
            {
                visitor.Visit(_outputAllHelper);
            }
        }

	    private UniformPair<EventBean[]> ProcessOutputLimitedJoinLast(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
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

	        IDictionary<object, EventBean[]> groupRepsView = new LinkedHashMap<object, EventBean[]>();
	        foreach (var pair in joinEventsSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (_prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var aNewData in newData)
	                {
	                    var mk = GenerateGroupKey(aNewData.Array, true);

	                    // if this is a newly encountered group, generate the remove stream event
	                    if (groupRepsView.Push(mk, aNewData.Array) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }
	                    _aggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var anOldData in oldData)
	                {
	                    var mk = GenerateGroupKey(anOldData.Array, true);

	                    if (groupRepsView.Push(mk, anOldData.Array) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }

	                    _aggregationService.ApplyLeave(anOldData.Array, mk, _agentInstanceContext);
	                }
	            }
	        }

	        GenerateOutputBatchedArr(true, groupRepsView.GetEnumerator(), true, generateSynthetic, newEvents, newEventsSortKey);

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);

	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedJoinFirst(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
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

	        IDictionary<object, EventBean[]> groupRepsView = new LinkedHashMap<object, EventBean[]>();
	        if (_prototype.OptionalHavingNode == null) {
	            foreach (var pair in joinEventsSet)
	            {
	                var newData = pair.First;
	                var oldData = pair.Second;

	                if (newData != null)
	                {
	                    // apply new data to aggregates
	                    foreach (var aNewData in newData)
	                    {
	                        var mk = GenerateGroupKey(aNewData.Array, true);
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            // if this is a newly encountered group, generate the remove stream event
	                            if (groupRepsView.Push(mk, aNewData.Array) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }
	                        _aggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
	                    }
	                }
	                if (oldData != null)
	                {
	                    // apply old data to aggregates
	                    foreach (var anOldData in oldData)
	                    {
	                        var mk = GenerateGroupKey(anOldData.Array, true);
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            if (groupRepsView.Push(mk, anOldData.Array) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }

	                        _aggregationService.ApplyLeave(anOldData.Array, mk, _agentInstanceContext);
	                    }
	                }
	            }
	        }
	        else {
	            groupRepsView.Clear();
	            foreach (var pair in joinEventsSet)
	            {
	                var newData = pair.First;
	                var oldData = pair.Second;

	                var newDataMultiKey = GenerateGroupKeys(newData, true);
	                var oldDataMultiKey = GenerateGroupKeys(oldData, false);

	                if (newData != null)
	                {
	                    // apply new data to aggregates
	                    var count = 0;
	                    foreach (var aNewData in newData)
	                    {
	                        _aggregationService.ApplyEnter(aNewData.Array, newDataMultiKey[count], _agentInstanceContext);
	                        count++;
	                    }
	                }
	                if (oldData != null)
	                {
	                    // apply old data to aggregates
	                    var count = 0;
	                    foreach (var anOldData in oldData)
	                    {
	                        _aggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _agentInstanceContext);
	                        count++;
	                    }
	                }

	                // evaluate having-clause
	                if (newData != null)
	                {
	                    var count = 0;
	                    foreach (var aNewData in newData)
	                    {
	                        var mk = newDataMultiKey[count];
	                        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aNewData.Array);}
	                        var evaluateParams = new EvaluateParams(aNewData.Array, true, _agentInstanceContext);
	                        var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            count++;
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            if (groupRepsView.Push(mk, aNewData.Array) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }
	                        count++;
	                    }
	                }

	                // evaluate having-clause
	                if (oldData != null)
	                {
	                    var count = 0;
	                    foreach (var anOldData in oldData)
	                    {
	                        var mk = oldDataMultiKey[count];
	                        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(anOldData.Array);}
	                        var evaluateParams = new EvaluateParams(anOldData.Array, false, _agentInstanceContext);
	                        var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            count++;
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            if (groupRepsView.Push(mk, anOldData.Array) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }
	                        count++;
	                    }
	                }
	            }
	        }

	        GenerateOutputBatchedArr(true, groupRepsView.GetEnumerator(), true, generateSynthetic, newEvents, newEventsSortKey);

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedJoinAll(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
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

	        if (_prototype.IsSelectRStream)
	        {
	            GenerateOutputBatchedArr(true, _outputAllGroupReps.EntryIterator(), false, generateSynthetic, oldEvents, oldEventsSortKey);
	        }

	        foreach (var pair in joinEventsSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (_prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var aNewData in newData)
	                {
	                    var mk = GenerateGroupKey(aNewData.Array, true);

	                    // if this is a newly encountered group, generate the remove stream event
                        if (_outputAllGroupReps.Put(mk, aNewData.Array) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }
	                    _aggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var anOldData in oldData)
	                {
	                    var mk = GenerateGroupKey(anOldData.Array, true);

                        if (_outputAllGroupReps.Put(mk, anOldData.Array) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }

	                    _aggregationService.ApplyLeave(anOldData.Array, mk, _agentInstanceContext);
	                }
	            }
	        }

	        GenerateOutputBatchedArr(true, _outputAllGroupReps.EntryIterator(), true, generateSynthetic, newEvents, newEventsSortKey);

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
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

            IDictionary<object, EventBean[]> keysAndEvents = new NullableDictionary<object, EventBean[]>();

	        foreach (var pair in joinEventsSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (_prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            var newDataMultiKey = GenerateGroupKeys(newData, keysAndEvents, true);
	            var oldDataMultiKey = GenerateGroupKeys(oldData, keysAndEvents, false);

	            if (_prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedArr(true, keysAndEvents.GetEnumerator(), false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                var count = 0;
	                foreach (var aNewData in newData)
	                {
	                    _aggregationService.ApplyEnter(aNewData.Array, newDataMultiKey[count], _agentInstanceContext);
	                    count++;
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    _aggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _agentInstanceContext);
	                    count++;
	                }
	            }

	            GenerateOutputBatchedArr(true, keysAndEvents.GetEnumerator(), true, generateSynthetic, newEvents, newEventsSortKey);

	            keysAndEvents.Clear();
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
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedViewLast(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
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

	        IDictionary<object, EventBean[]> groupRepsView = new LinkedHashMap<object, EventBean[]>();
	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var aNewData in newData)
	                {
	                    var eventsPerStream = new EventBean[] {aNewData};
	                    var mk = GenerateGroupKey(eventsPerStream, true);

	                    // if this is a newly encountered group, generate the remove stream event
	                    if (groupRepsView.Push(mk, eventsPerStream) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }
	                    _aggregationService.ApplyEnter(eventsPerStream, mk, _agentInstanceContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var anOldData in oldData)
	                {
	                    var eventsPerStream = new EventBean[] {anOldData};
	                    var mk = GenerateGroupKey(eventsPerStream, true);

	                    if (groupRepsView.Push(mk, eventsPerStream) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }

	                    _aggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
	                }
	            }
	        }

	        GenerateOutputBatchedArr(false, groupRepsView.GetEnumerator(), true, generateSynthetic, newEvents, newEventsSortKey);

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedViewFirst(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
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

	        IDictionary<object, EventBean[]> groupRepsView = new LinkedHashMap<object, EventBean[]>();
	        if (_prototype.OptionalHavingNode == null) {
	            foreach (var pair in viewEventsList)
	            {
	                var newData = pair.First;
	                var oldData = pair.Second;

	                if (newData != null)
	                {
	                    // apply new data to aggregates
	                    foreach (var aNewData in newData)
	                    {
	                        var eventsPerStream = new EventBean[] {aNewData};
	                        var mk = GenerateGroupKey(eventsPerStream, true);
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            // if this is a newly encountered group, generate the remove stream event
	                            if (groupRepsView.Push(mk, eventsPerStream) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }
	                        _aggregationService.ApplyEnter(eventsPerStream, mk, _agentInstanceContext);
	                    }
	                }
	                if (oldData != null)
	                {
	                    // apply old data to aggregates
	                    foreach (var anOldData in oldData)
	                    {
	                        var eventsPerStream = new EventBean[] {anOldData};
	                        var mk = GenerateGroupKey(eventsPerStream, true);
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            if (groupRepsView.Push(mk, eventsPerStream) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }

	                        _aggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
	                    }
	                }
	            }
	        }
	        else { // having clause present, having clause evaluates at the level of individual posts
	            var eventsPerStreamOneStream = new EventBean[1];
	            foreach (var pair in viewEventsList)
	            {
	                var newData = pair.First;
	                var oldData = pair.Second;

	                var newDataMultiKey = GenerateGroupKeys(newData, true);
	                var oldDataMultiKey = GenerateGroupKeys(oldData, false);

	                if (newData != null)
	                {
	                    // apply new data to aggregates
	                    for (var i = 0; i < newData.Length; i++)
	                    {
	                        eventsPerStreamOneStream[0] = newData[i];
	                        _aggregationService.ApplyEnter(eventsPerStreamOneStream, newDataMultiKey[i], _agentInstanceContext);
	                    }
	                }
	                if (oldData != null)
	                {
	                    // apply old data to aggregates
	                    for (var i = 0; i < oldData.Length; i++)
	                    {
	                        eventsPerStreamOneStream[0] = oldData[i];
	                        _aggregationService.ApplyLeave(eventsPerStreamOneStream, oldDataMultiKey[i], _agentInstanceContext);
	                    }
	                }

	                // evaluate having-clause
	                if (newData != null)
	                {
                        var evaluateParams = new EvaluateParams(eventsPerStreamOneStream, true, _agentInstanceContext);
                        for (var i = 0; i < newData.Length; i++)
	                    {
	                        var mk = newDataMultiKey[i];
	                        eventsPerStreamOneStream[0] = newData[i];
	                        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(newData[i]);}
	                        var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            var eventsPerStream = new EventBean[] {newData[i]};
	                            if (groupRepsView.Push(mk, eventsPerStream) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(false, mk, eventsPerStream, true, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }
	                    }
	                }

	                // evaluate having-clause
	                if (oldData != null)
	                {
                        var evaluateParams = new EvaluateParams(eventsPerStreamOneStream, false, _agentInstanceContext);
                        for (var i = 0; i < oldData.Length; i++)
	                    {
	                        var mk = oldDataMultiKey[i];
	                        eventsPerStreamOneStream[0] = oldData[i];
	                        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(oldData[i]);}
	                        var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            var eventsPerStream = new EventBean[] {oldData[i]};
	                            if (groupRepsView.Push(mk, eventsPerStream) == null)
	                            {
	                                if (_prototype.IsSelectRStream)
	                                {
	                                    GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                                }
	                            }
	                        }
	                    }
	                }
	            }
	        }

	        GenerateOutputBatchedArr(false, groupRepsView.GetEnumerator(), true, generateSynthetic, newEvents, newEventsSortKey);

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedViewAll(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic) {
	        var eventsPerStream = new EventBean[1];

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

	        if (_prototype.IsSelectRStream)
	        {
	            GenerateOutputBatchedArr(false, _outputAllGroupReps.EntryIterator(), false, generateSynthetic, oldEvents, oldEventsSortKey);
	        }

	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                foreach (var aNewData in newData)
	                {
	                    eventsPerStream[0] = aNewData;
	                    var mk = GenerateGroupKey(eventsPerStream, true);

	                    // if this is a newly encountered group, generate the remove stream event
	                    if (_outputAllGroupReps.Put(mk, new EventBean[] {aNewData}) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }
	                    _aggregationService.ApplyEnter(eventsPerStream, mk, _agentInstanceContext);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                foreach (var anOldData in oldData)
	                {
	                    eventsPerStream[0] = anOldData;
	                    var mk = GenerateGroupKey(eventsPerStream, true);

                        if (_outputAllGroupReps.Put(mk, new EventBean[] { anOldData }) == null)
	                    {
	                        if (_prototype.IsSelectRStream)
	                        {
	                            GenerateOutputBatchedRow(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
	                        }
	                    }

	                    _aggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
	                }
	            }
	        }

	        GenerateOutputBatchedArr(false, _outputAllGroupReps.EntryIterator(), true, generateSynthetic, newEvents, newEventsSortKey);

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
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

            IDictionary<object, EventBean> keysAndEvents = new NullableDictionary<object, EventBean>();

	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            var newDataMultiKey = GenerateGroupKeys(newData, keysAndEvents, true);
	            var oldDataMultiKey = GenerateGroupKeys(oldData, keysAndEvents, false);

	            if (_prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedRow(keysAndEvents, false, generateSynthetic, oldEvents, oldEventsSortKey, _agentInstanceContext);
	            }

	            var eventsPerStream = new EventBean[1];
	            if (newData != null)
	            {
	                // apply new data to aggregates
	                var count = 0;
	                foreach (var aNewData in newData)
	                {
	                    eventsPerStream[0] = aNewData;
	                    _aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[count], _agentInstanceContext);
	                    count++;
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    eventsPerStream[0] = anOldData;
	                    _aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[count], _agentInstanceContext);
	                    count++;
	                }
	            }

	            GenerateOutputBatchedRow(keysAndEvents, true, generateSynthetic, newEvents, newEventsSortKey, _agentInstanceContext);

	            keysAndEvents.Clear();
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
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private object[] GenerateGroupKeys(ISet<MultiKey<EventBean>> resultSet, bool isNewData)
	    {
	        if (resultSet.IsEmpty())
	        {
	            return null;
	        }

	        var keys = new object[resultSet.Count];

	        var count = 0;
	        foreach (var eventsPerStream in resultSet)
	        {
	            keys[count] = GenerateGroupKey(eventsPerStream.Array, isNewData);
	            count++;
	        }

	        return keys;
	    }
	}
} // end of namespace

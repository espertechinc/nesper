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
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
	/// <summary>
	/// Result-set processor for the aggregate-grouped case:
	/// there is a group-by and one or more non-aggregation event properties in the select clause are not listed in the group by,
	/// and there are aggregation functions.
	/// <para />This processor does perform grouping by computing MultiKey group-by keys for each row.
	/// The processor generates one row for each event entering (new event) and one row for each event leaving (old event).
	/// <para />Aggregation state is a table of rows held by <seealso cref="agg.service.AggregationService" /> where the row key is the group-by MultiKey.
	/// </summary>
	public class ResultSetProcessorAggregateGrouped 
        : ResultSetProcessor
        , AggregationRowRemovedCallback
    {
	    protected internal readonly ResultSetProcessorAggregateGroupedFactory Prototype;
	    private readonly SelectExprProcessor _selectExprProcessor;
	    private readonly OrderByProcessor _orderByProcessor;

	    protected internal AggregationService _aggregationService;

	    protected readonly EventBean[] _eventsPerStreamOneStream = new EventBean[1];

	    // For output limiting, keep a representative of each group-by group
	    private readonly ResultSetProcessorGroupedOutputAllGroupReps _outputAllGroupReps;
	    private readonly IDictionary<object, EventBean[]> _workCollection = new LinkedHashMap<object, EventBean[]>();
	    private readonly IDictionary<object, EventBean[]> _workCollectionTwo = new LinkedHashMap<object, EventBean[]>();

	    private readonly ResultSetProcessorAggregateGroupedOutputLastHelper _outputLastHelper;
	    private readonly ResultSetProcessorAggregateGroupedOutputAllHelper _outputAllHelper;
	    private readonly ResultSetProcessorGroupedOutputFirstHelper _outputFirstHelper;

	    public ResultSetProcessorAggregateGrouped(
	        ResultSetProcessorAggregateGroupedFactory prototype,
	        SelectExprProcessor selectExprProcessor,
	        OrderByProcessor orderByProcessor,
	        AggregationService aggregationService,
	        AgentInstanceContext agentInstanceContext)
        {
	        Prototype = prototype;
	        _selectExprProcessor = selectExprProcessor;
	        _orderByProcessor = orderByProcessor;
	        _aggregationService = aggregationService;
	        AgentInstanceContext = agentInstanceContext;

	        aggregationService.SetRemovedCallback(this);

	        if (prototype.IsOutputLast && prototype.IsEnableOutputLimitOpt) {
	            _outputLastHelper = prototype.ResultSetProcessorHelperFactory.MakeRSAggregateGroupedOutputLastOpt(agentInstanceContext, this, prototype);
	        }
	        else if (prototype.IsOutputAll) {
	            if (!prototype.IsEnableOutputLimitOpt) {
	                _outputAllGroupReps = prototype.ResultSetProcessorHelperFactory.MakeRSGroupedOutputAllNoOpt(agentInstanceContext, prototype.GroupKeyNodes, prototype.NumStreams);
	            }
	            else {
	                _outputAllHelper = prototype.ResultSetProcessorHelperFactory.MakeRSAggregateGroupedOutputAll(agentInstanceContext, this, prototype);
	            }
	        }
	        else if (prototype.IsOutputFirst) {
	            _outputFirstHelper = prototype.ResultSetProcessorHelperFactory.MakeRSGroupedOutputFirst(agentInstanceContext, prototype.GroupKeyNodes, prototype.OptionalOutputFirstConditionFactory, null, -1);
	        }
	    }

	    public AgentInstanceContext AgentInstanceContext { get; set; }

	    public EventType ResultEventType
	    {
	        get { return Prototype.ResultEventType; }
	    }

	    public EventBean[] EventsPerStreamOneStream
	    {
	        get { return _eventsPerStreamOneStream; }
	    }

	    public AggregationService AggregationService
	    {
	        get { return _aggregationService; }
	    }

	    public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
	    {
	        var eventsPerStream = new EventBean[1];
	        if (newData != null)
	        {
	            // apply new data to aggregates
	            foreach (var aNewData in newData)
	            {
	                eventsPerStream[0] = aNewData;
	                var mk = GenerateGroupKey(eventsPerStream, true);
	                _aggregationService.ApplyEnter(eventsPerStream, mk, AgentInstanceContext);
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            foreach (var anOldData in oldData)
	            {
	                eventsPerStream[0] = anOldData;
	                var mk = GenerateGroupKey(eventsPerStream, false);
	                _aggregationService.ApplyLeave(eventsPerStream, mk, AgentInstanceContext);
	            }
	        }
	    }

	    public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
	    {
	        if (!newEvents.IsEmpty())
	        {
	            // apply old data to aggregates
	            foreach (var eventsPerStream in newEvents)
	            {
	                var mk = GenerateGroupKey(eventsPerStream.Array, true);
	                _aggregationService.ApplyEnter(eventsPerStream.Array, mk, AgentInstanceContext);
	            }
	        }
	        if (oldEvents != null && !oldEvents.IsEmpty())
	        {
	            // apply old data to aggregates
	            foreach (var eventsPerStream in oldEvents)
	            {
	                var mk = GenerateGroupKey(eventsPerStream.Array, false);
	                _aggregationService.ApplyLeave(eventsPerStream.Array, mk, AgentInstanceContext);
	            }
	        }
	    }

	    public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerEvent();}
	        // Generate group-by keys for all events
	        var newDataGroupByKeys = GenerateGroupKeys(newEvents, true);
	        var oldDataGroupByKeys = GenerateGroupKeys(oldEvents, false);

	        // generate old events
	        if (Prototype.IsUnidirectional)
	        {
	            Clear();
	        }

	        // update aggregates
	        if (!newEvents.IsEmpty())
	        {
	            // apply old data to aggregates
	            var count = 0;
	            foreach (var eventsPerStream in newEvents)
	            {
	                _aggregationService.ApplyEnter(eventsPerStream.Array, newDataGroupByKeys[count], AgentInstanceContext);
	                count++;
	            }
	        }
	        if (!oldEvents.IsEmpty())
	        {
	            // apply old data to aggregates
	            var count = 0;
	            foreach (var eventsPerStream in oldEvents)
	            {
	                _aggregationService.ApplyLeave(eventsPerStream.Array, oldDataGroupByKeys[count], AgentInstanceContext);
	                count++;
	            }
	        }

	        EventBean[] selectOldEvents = null;
	        if (Prototype.IsSelectRStream)
	        {
	            selectOldEvents = GenerateOutputEventsJoin(oldEvents, oldDataGroupByKeys, false, isSynthesize);
	        }
	        var selectNewEvents = GenerateOutputEventsJoin(newEvents, newDataGroupByKeys, true, isSynthesize);

	        if ((selectNewEvents != null) || (selectOldEvents != null))
	        {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerEvent(selectNewEvents, selectOldEvents);}
	            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerEvent(null, null);}
	        return null;
	    }

	    public UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerEvent();}

	        // Generate group-by keys for all events
	        var newDataGroupByKeys = GenerateGroupKeys(newData, true);
	        var oldDataGroupByKeys = GenerateGroupKeys(oldData, false);

	        // update aggregates
	        var eventsPerStream = new EventBean[1];
	        if (newData != null)
	        {
	            // apply new data to aggregates
	            for (var i = 0; i < newData.Length; i++)
	            {
	                eventsPerStream[0] = newData[i];
	                _aggregationService.ApplyEnter(eventsPerStream, newDataGroupByKeys[i], AgentInstanceContext);
	            }
	        }
	        if (oldData != null)
	        {
	            // apply old data to aggregates
	            for (var i = 0; i < oldData.Length; i++)
	            {
	                eventsPerStream[0] = oldData[i];
	                _aggregationService.ApplyLeave(eventsPerStream, oldDataGroupByKeys[i], AgentInstanceContext);
	            }
	        }

	        EventBean[] selectOldEvents = null;
	        if (Prototype.IsSelectRStream)
	        {
	            selectOldEvents = GenerateOutputEventsView(oldData, oldDataGroupByKeys, false, isSynthesize);
	        }
	        var selectNewEvents = GenerateOutputEventsView(newData, newDataGroupByKeys, true, isSynthesize);

	        if ((selectNewEvents != null) || (selectOldEvents != null))
	        {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerEvent(selectNewEvents, selectOldEvents);}
	            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerEvent(null, null);}
	        return null;
	    }

		private EventBean[] GenerateOutputEventsView(EventBean[] outputEvents, object[] groupByKeys, bool isNewData, bool isSynthesize)
	    {
	        if (outputEvents == null)
	        {
	            return null;
	        }

	        var eventsPerStream = new EventBean[1];
	        var events = new EventBean[outputEvents.Length];
	        var keys = new object[outputEvents.Length];
	        EventBean[][] currentGenerators = null;
	        if(Prototype.IsSorting)
	        {
	        	currentGenerators = new EventBean[outputEvents.Length][];
	        }

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
            var countOutputRows = 0;
	        for (var countInputRows = 0; countInputRows < outputEvents.Length; countInputRows++)
	        {
	            _aggregationService.SetCurrentAccess(groupByKeys[countInputRows], AgentInstanceContext.AgentInstanceId, null);
	            eventsPerStream[0] = outputEvents[countInputRows];

	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(outputEvents[countInputRows]);}
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); }
	                if ((result == null) || (false.Equals(result))) {
	                    continue;
	                }
	            }

	            events[countOutputRows] = _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext);
	            keys[countOutputRows] = groupByKeys[countInputRows];
	            if(Prototype.IsSorting)
	            {
	            	var currentEventsPerStream = new EventBean[] { outputEvents[countInputRows] };
	            	currentGenerators[countOutputRows] = currentEventsPerStream;
	            }

	            countOutputRows++;
	        }

	        // Resize if some rows were filtered out
	        if (countOutputRows != events.Length)
	        {
	            if (countOutputRows == 0)
	            {
	                return null;
	            }
	            var outEvents = new EventBean[countOutputRows];
	            Array.Copy(events, 0, outEvents, 0, countOutputRows);
	            events = outEvents;

	            if(Prototype.IsSorting)
	            {
	            	var outKeys = new object[countOutputRows];
	            	Array.Copy(keys, 0, outKeys, 0, countOutputRows);
	            	keys = outKeys;

	            	var outGens = new EventBean[countOutputRows][];
	            	Array.Copy(currentGenerators, 0, outGens, 0, countOutputRows);
	            	currentGenerators = outGens;
	            }
	        }

	        if(Prototype.IsSorting)
	        {
	            events = _orderByProcessor.Sort(events, currentGenerators, keys, isNewData, AgentInstanceContext);
	        }

	        return events;
	    }

	    public object[] GenerateGroupKeys(ISet<MultiKey<EventBean>> resultSet, bool isNewData)
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

	    public object[] GenerateGroupKeys(EventBean[] events, bool isNewData)
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
	        }

	        return keys;
	    }

	    /// <summary>
	    /// Generates the group-by key for the row
	    /// </summary>
	    /// <param name="eventsPerStream">is the row of events</param>
	    /// <param name="isNewData">is true for new data</param>
	    /// <returns>grouping keys</returns>
	    protected internal object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData)
	    {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
            
            if (InstrumentationHelper.ENABLED)
            {
	            InstrumentationHelper.Get().QResultSetProcessComputeGroupKeys(isNewData, Prototype.GroupKeyNodeExpressions, eventsPerStream);

	            object keyObject;
	            if (Prototype.GroupKeyNode != null) {
	                keyObject = Prototype.GroupKeyNode.Evaluate(evaluateParams);
	            }
	            else {
	                var keysX = new object[Prototype.GroupKeyNodes.Length];
	                var countX = 0;
	                foreach (var exprNode in Prototype.GroupKeyNodes) {
	                    keysX[countX] = exprNode.Evaluate(evaluateParams);
	                    countX++;
	                }
	                keyObject = new MultiKeyUntyped(keysX);
	            }
	            InstrumentationHelper.Get().AResultSetProcessComputeGroupKeys(isNewData, keyObject);
	            return keyObject;
	        }

	        if (Prototype.GroupKeyNode != null) {
	            return Prototype.GroupKeyNode.Evaluate(evaluateParams);
	        }

	        var keys = new object[Prototype.GroupKeyNodes.Length];
	        var count = 0;
	        foreach (var exprNode in Prototype.GroupKeyNodes) {
	            keys[count] = exprNode.Evaluate(evaluateParams);
	            count++;
	        }
	        return new MultiKeyUntyped(keys);
	    }

	    private EventBean[] GenerateOutputEventsJoin(ISet<MultiKey<EventBean>> resultSet, object[] groupByKeys, bool isNewData, bool isSynthesize)
	    {
	        if (resultSet.IsEmpty())
	        {
	            return null;
	        }

	        var events = new EventBean[resultSet.Count];
	        var keys = new object[resultSet.Count];
	        EventBean[][] currentGenerators = null;
	        if(Prototype.IsSorting)
	        {
	        	currentGenerators = new EventBean[resultSet.Count][];
	        }

	        var countOutputRows = 0;
	        var countInputRows = -1;
	        foreach (var row in resultSet)
	        {
	            countInputRows++;
	            var eventsPerStream = row.Array;
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);

	            _aggregationService.SetCurrentAccess(groupByKeys[countInputRows], AgentInstanceContext.AgentInstanceId, null);

	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            events[countOutputRows] = _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext);
	            keys[countOutputRows] = groupByKeys[countInputRows];
	            if(Prototype.IsSorting)
	            {
	            	currentGenerators[countOutputRows] = eventsPerStream;
	            }

	            countOutputRows++;
	        }

	        // Resize if some rows were filtered out
	        if (countOutputRows != events.Length)
	        {
	            if (countOutputRows == 0)
	            {
	                return null;
	            }
	            var outEvents = new EventBean[countOutputRows];
	            Array.Copy(events, 0, outEvents, 0, countOutputRows);
	            events = outEvents;

	            if(Prototype.IsSorting)
	            {
	            	var outKeys = new object[countOutputRows];
	            	Array.Copy(keys, 0, outKeys, 0, countOutputRows);
	            	keys = outKeys;

	            	var outGens = new EventBean[countOutputRows][];
	            	Array.Copy(currentGenerators, 0, outGens, 0, countOutputRows);
	            	currentGenerators = outGens;
	            }
	        }

	        if(Prototype.IsSorting)
	        {
	            events = _orderByProcessor.Sort(events, currentGenerators, keys, isNewData, AgentInstanceContext);
	        }
	        return events;
	    }

        public static readonly IList<EventBean> EMPTY_EVENT_BEAN_LIST = new EventBean[0]; 

	    public IEnumerator<EventBean> GetEnumerator(Viewable parent)
	    {
	        if (!Prototype.IsHistoricalOnly) {
	            return ObtainEnumerator(parent);
	        }

	        _aggregationService.ClearResults(AgentInstanceContext);
            var it = parent.GetEnumerator();
	        var eventsPerStream = new EventBean[1];
	        while (it.MoveNext()) {
	            eventsPerStream[0] = it.Current;
	            var groupKey = GenerateGroupKey(eventsPerStream, true);
	            _aggregationService.ApplyEnter(eventsPerStream, groupKey, AgentInstanceContext);
	        }

	        var deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainEnumerator(parent));
	        _aggregationService.ClearResults(AgentInstanceContext);
            return deque.GetEnumerator();
	    }

	    private IEnumerator<EventBean> ObtainEnumerator(Viewable parent)
        {
	        if (_orderByProcessor == null)
	        {
	            return new ResultSetAggregateGroupedIterator(parent.GetEnumerator(), this, _aggregationService,AgentInstanceContext);
	        }

	        // Pull all parent events, generate order keys
	        var eventsPerStream = new EventBean[1];
	        IList<EventBean> outgoingEvents = new List<EventBean>();
	        IList<object> orderKeys = new List<object>();

            var evaluateParams = new EvaluateParams(eventsPerStream, true, AgentInstanceContext);
            
            foreach (var candidate in parent)
            {
	            eventsPerStream[0] = candidate;

	            var groupKey = GenerateGroupKey(eventsPerStream, true);
	            _aggregationService.SetCurrentAccess(groupKey, AgentInstanceContext.AgentInstanceId, null);

	            if (Prototype.OptionalHavingNode != null) {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(candidate);}
	                var pass = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(pass.AsBoolean()); }
	                if ((pass == null) || (false.Equals(pass)))
	                {
	                    continue;
	                }
	            }

	            outgoingEvents.Add(_selectExprProcessor.Process(eventsPerStream, true, true, AgentInstanceContext));

	            var orderKey = _orderByProcessor.GetSortKey(eventsPerStream, true, AgentInstanceContext);
	            orderKeys.Add(orderKey);
	        }

	        // sort
	        var outgoingEventsArr = outgoingEvents.ToArray();
	        var orderKeysArr = orderKeys.ToArray();
	        var orderedEvents = _orderByProcessor.Sort(outgoingEventsArr, orderKeysArr, AgentInstanceContext) ?? EMPTY_EVENT_BEAN_LIST;

	        return orderedEvents.GetEnumerator();
	    }

	    /// <summary>
	    /// Returns the select expression processor
	    /// </summary>
	    /// <value>select processor.</value>
	    public SelectExprProcessor SelectExprProcessor
	    {
	        get { return _selectExprProcessor; }
	    }

	    /// <summary>
	    /// Returns the having node.
	    /// </summary>
	    /// <value>having expression</value>
	    public ExprEvaluator OptionalHavingNode
	    {
	        get { return Prototype.OptionalHavingNode; }
	    }

	    public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
	    {
	        // Generate group-by keys for all events
	        var groupByKeys = GenerateGroupKeys(joinSet, true);
            var result = GenerateOutputEventsJoin(joinSet, groupByKeys, true, true) ?? EMPTY_EVENT_BEAN_LIST;
	        return result.GetEnumerator();
	    }

	    public void Clear()
	    {
	        _aggregationService.ClearResults(AgentInstanceContext);
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
	        else {
	            throw new IllegalStateException("Unrecognized output limit " + outputLimitLimitType);
	        }
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
	        else {
	            throw new IllegalStateException("Unrecognized output limited type " + outputLimitLimitType);
	        }
	    }

	    public void Stop()
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

	    public void GenerateOutputBatchedJoinUnkeyed(ISet<MultiKey<EventBean>> outputEvents, object[] groupByKeys, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, IList<object> optSortKeys)
	    {
	        if (outputEvents == null)
	        {
	            return;
	        }

	        EventBean[] eventsPerStream;

	        var count = 0;
	        foreach (var row in outputEvents)
	        {
	            _aggregationService.SetCurrentAccess(groupByKeys[count], AgentInstanceContext.AgentInstanceId, null);
	            eventsPerStream = row.Array;

                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext));
	            if(Prototype.IsSorting)
	            {
	                optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, AgentInstanceContext));
	            }

	            count++;
	        }
	    }

	    public EventBean GenerateOutputBatchedSingle(object groupByKey, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize)
	    {
	        _aggregationService.SetCurrentAccess(groupByKey, AgentInstanceContext.AgentInstanceId, null);

            // Filter the having clause
	        if (Prototype.OptionalHavingNode != null)
	        {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	            if ((result == null) || (false.Equals(result))) {
	                return null;
	            }
	        }

	        return _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext);
	    }

	    public void GenerateOutputBatchedViewPerKey(EventBean[] outputEvents, object[] groupByKeys, bool isNewData, bool isSynthesize, IDictionary<object, EventBean> resultEvents, IDictionary<object, object> optSortKeys)
	    {
	        if (outputEvents == null)
	        {
	            return;
	        }

	        var eventsPerStream = new EventBean[1];

	        var count = 0;
	        for (var i = 0; i < outputEvents.Length; i++)
	        {
	            var groupKey = groupByKeys[count];
	            _aggregationService.SetCurrentAccess(groupKey, AgentInstanceContext.AgentInstanceId, null);
	            eventsPerStream[0] = outputEvents[count];

	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(outputEvents[count]);}
	                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); }
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            resultEvents.Put(groupKey, _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext));
	            if(Prototype.IsSorting)
	            {
	                optSortKeys.Put(groupKey, _orderByProcessor.GetSortKey(eventsPerStream, isNewData, AgentInstanceContext));
	            }

	            count++;
	        }
	    }

	    public void GenerateOutputBatchedJoinPerKey(ISet<MultiKey<EventBean>> outputEvents, object[] groupByKeys, bool isNewData, bool isSynthesize, IDictionary<object, EventBean> resultEvents, IDictionary<object, object> optSortKeys)
	    {
	        if (outputEvents == null)
	        {
	            return;
	        }

	        var count = 0;
	        foreach (var row in outputEvents)
	        {
	            var groupKey = groupByKeys[count];
	            _aggregationService.SetCurrentAccess(groupKey, AgentInstanceContext.AgentInstanceId, null);

	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(row.Array);}
	                var evaluateParams = new EvaluateParams(row.Array, isNewData, AgentInstanceContext);
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            resultEvents.Put(groupKey, _selectExprProcessor.Process(row.Array, isNewData, isSynthesize, AgentInstanceContext));
	            if(Prototype.IsSorting)
	            {
	                optSortKeys.Put(groupKey, _orderByProcessor.GetSortKey(row.Array, isNewData, AgentInstanceContext));
	            }

	            count++;
	        }
	    }

	    public bool HasAggregation
	    {
	        get { return true; }
	    }

	    public void Removed(object key)
        {
	        if (_outputAllGroupReps != null) {
	            _outputAllGroupReps.Remove(key);
	        }
	        if (_outputAllHelper != null) {
	            _outputAllHelper.Remove(key);
	        }
	        if (_outputLastHelper != null) {
	            _outputLastHelper.Remove(key);
	        }
	        if (_outputFirstHelper != null) {
	            _outputFirstHelper.Remove(key);
	        }
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
	        if (isAll) {
	            _outputAllHelper.ProcessView(newData, oldData, isGenerateSynthetic);
	        }
	        else {
	            _outputLastHelper.ProcessView(newData, oldData, isGenerateSynthetic);
	        }
	    }

	    public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic, bool isAll)
        {
	        if (isAll) {
	            _outputAllHelper.ProcessJoin(newData, oldData, isGenerateSynthetic);
	        }
	        else {
	            _outputLastHelper.ProcessJoin(newData, oldData, isGenerateSynthetic);
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

	    private UniformPair<EventBean[]> ProcessOutputLimitedJoinLast(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
	        IDictionary<object, EventBean> lastPerGroupNew = new LinkedHashMap<object, EventBean>();
	        IDictionary<object, EventBean> lastPerGroupOld = null;
	        if (Prototype.IsSelectRStream)
	        {
	            lastPerGroupOld = new LinkedHashMap<object, EventBean>();
	        }

	        IDictionary<object, object> newEventsSortKey = null; // group key to sort key
	        IDictionary<object, object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new LinkedHashMap<object, object>();
	            if (Prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new LinkedHashMap<object, object>();
	            }
	        }

	        foreach (var pair in joinEventsSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            var newDataMultiKey = GenerateGroupKeys(newData, true);
	            var oldDataMultiKey = GenerateGroupKeys(oldData, false);

	            if (Prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                var count = 0;
	                foreach (var aNewData in newData)
	                {
	                    var mk = newDataMultiKey[count];
	                    _aggregationService.ApplyEnter(aNewData.Array, mk, AgentInstanceContext);
	                    count++;
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    _aggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }

	            if (Prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedJoinPerKey(oldData, oldDataMultiKey, false, generateSynthetic, lastPerGroupOld, oldEventsSortKey);
	            }
	            GenerateOutputBatchedJoinPerKey(newData, newDataMultiKey, false, generateSynthetic, lastPerGroupNew, newEventsSortKey);
	        }

	        var newEventsArr = (lastPerGroupNew.IsEmpty()) ? null : lastPerGroupNew.Values.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEventsArr = (lastPerGroupOld.IsEmpty()) ? null : lastPerGroupOld.Values.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.Values.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.Values.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
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
	        IList<EventBean> resultNewEvents = new List<EventBean>();
	        IList<object> resultNewSortKeys = null;
	        IList<EventBean> resultOldEvents = null;
	        IList<object> resultOldSortKeys = null;

	        if (_orderByProcessor != null) {
	            resultNewSortKeys = new List<object>();
	        }
	        if (Prototype.IsSelectRStream) {
	            resultOldEvents = new List<EventBean>();
	            resultOldSortKeys = new List<object>();
	        }

	        _workCollection.Clear();

	        if (Prototype.OptionalHavingNode == null)
            {
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
	                        var mk = newDataMultiKey[count];
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            _workCollection.Put(mk, aNewData.Array);
	                        }
	                        _aggregationService.ApplyEnter(aNewData.Array, mk, AgentInstanceContext);
	                        count++;
	                    }
	                }

	                if (oldData != null)
	                {
	                    // apply new data to aggregates
	                    var count = 0;
	                    foreach (var aOldData in oldData)
	                    {
	                        var mk = oldDataMultiKey[count];
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            _workCollection.Put(mk, aOldData.Array);
	                        }
	                        _aggregationService.ApplyLeave(aOldData.Array, mk, AgentInstanceContext);
	                        count++;
	                    }
	                }

	                // there is no remove stream currently for output first
	                GenerateOutputBatchedArr(_workCollection, false, generateSynthetic, resultNewEvents, resultNewSortKeys);
	            }
	        }
	        else {// there is a having-clause, apply after aggregations
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
	                        var mk = newDataMultiKey[count];
	                        _aggregationService.ApplyEnter(aNewData.Array, mk, AgentInstanceContext);
	                        count++;
	                    }
	                }

	                if (oldData != null)
	                {
	                    var count = 0;
	                    foreach (var aOldData in oldData)
	                    {
	                        var mk = oldDataMultiKey[count];
	                        _aggregationService.ApplyLeave(aOldData.Array, mk, AgentInstanceContext);
	                        count++;
	                    }
	                }

	                if (newData != null)
	                {
	                    // check having clause and first-condition
	                    var count = 0;
	                    foreach (var aNewData in newData)
	                    {
	                        var mk = newDataMultiKey[count];
	                        _aggregationService.SetCurrentAccess(mk, AgentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aNewData.Array);}
	                        var evaluateParams = new EvaluateParams(aNewData.Array, true, AgentInstanceContext);
	                        var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            count++;
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            _workCollection.Put(mk, aNewData.Array);
	                        }
	                        count++;
	                    }
	                }

	                if (oldData != null)
	                {
	                    // apply new data to aggregates
	                    var count = 0;
	                    foreach (var aOldData in oldData)
	                    {
	                        var mk = oldDataMultiKey[count];
	                        _aggregationService.SetCurrentAccess(mk, AgentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aOldData.Array);}
	                        var evaluateParams = new EvaluateParams(aOldData.Array, true, AgentInstanceContext);
	                        var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            count++;
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            _workCollection.Put(mk, aOldData.Array);
	                        }
	                    }
	                }

	                // there is no remove stream currently for output first
	                GenerateOutputBatchedArr(_workCollection, false, generateSynthetic, resultNewEvents, resultNewSortKeys);
	            }
	        }

	        EventBean[] newEventsArr = null;
	        EventBean[] oldEventsArr = null;
	        if (!resultNewEvents.IsEmpty()) {
	            newEventsArr = resultNewEvents.ToArray();
	        }
	        if ((resultOldEvents != null) && (!resultOldEvents.IsEmpty())) {
	            oldEventsArr = resultOldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (resultNewSortKeys.IsEmpty()) ? null : resultNewSortKeys.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (resultOldSortKeys.IsEmpty()) ? null : resultOldSortKeys.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
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
	        if (Prototype.IsSelectRStream)
	        {
	            oldEvents = new List<EventBean>();
	        }

	        IList<object> newEventsSortKey = null;
	        IList<object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new List<object>();
	            if (Prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        _workCollection.Clear();

	        foreach (var pair in joinEventsSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            var newDataMultiKey = GenerateGroupKeys(newData, true);
	            var oldDataMultiKey = GenerateGroupKeys(oldData, false);

	            if (Prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                var count = 0;
	                foreach (var aNewData in newData)
	                {
	                    var mk = newDataMultiKey[count];
	                    _aggregationService.ApplyEnter(aNewData.Array, mk, AgentInstanceContext);
	                    count++;

	                    // keep the new event as a representative for the group
	                    _workCollection.Put(mk, aNewData.Array);
	                    _outputAllGroupReps.Put(mk, aNewData.Array);
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    _aggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }

	            if (Prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedJoinUnkeyed(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }
	            GenerateOutputBatchedJoinUnkeyed(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
	        }

	        // For any group representatives not in the work collection, generate a row
	        var entryIterator = _outputAllGroupReps.EntryIterator();
	        while (entryIterator.MoveNext())
	        {
	            var entry = entryIterator.Current;
	            if (!_workCollection.ContainsKey(entry.Key))
	            {
	                _workCollectionTwo.Put(entry.Key, entry.Value);
	                GenerateOutputBatchedArr(_workCollectionTwo, true, generateSynthetic, newEvents, newEventsSortKey);
	                _workCollectionTwo.Clear();
	            }
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
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
	        if (Prototype.IsSelectRStream)
	        {
	            oldEvents = new List<EventBean>();
	        }

	        IList<object> newEventsSortKey = null;
	        IList<object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new List<object>();
	            if (Prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        foreach (var pair in joinEventsSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            var newDataMultiKey = GenerateGroupKeys(newData, true);
	            var oldDataMultiKey = GenerateGroupKeys(oldData, false);

	            if (Prototype.IsUnidirectional)
	            {
	                Clear();
	            }

	            if (newData != null)
	            {
	                // apply new data to aggregates
	                var count = 0;
	                foreach (var aNewData in newData)
	                {
	                    _aggregationService.ApplyEnter(aNewData.Array, newDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    _aggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }

	            if (Prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedJoinUnkeyed(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }
	            GenerateOutputBatchedJoinUnkeyed(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
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
	        IDictionary<object, EventBean> lastPerGroupNew = new LinkedHashMap<object, EventBean>();
	        IDictionary<object, EventBean> lastPerGroupOld = null;
	        if (Prototype.IsSelectRStream)
	        {
	            lastPerGroupOld = new LinkedHashMap<object, EventBean>();
	        }

	        IDictionary<object, object> newEventsSortKey = null; // group key to sort key
	        IDictionary<object, object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new LinkedHashMap<object, object>();
	            if (Prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new LinkedHashMap<object, object>();
	            }
	        }

	        foreach (var pair in viewEventsList)
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
	                    var mk = newDataMultiKey[count];
	                    _eventsPerStreamOneStream[0] = aNewData;
	                    _aggregationService.ApplyEnter(_eventsPerStreamOneStream, mk, AgentInstanceContext);
	                    count++;
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    _eventsPerStreamOneStream[0] = anOldData;
	                    _aggregationService.ApplyLeave(_eventsPerStreamOneStream, oldDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }

	            if (Prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedViewPerKey(oldData, oldDataMultiKey, false, generateSynthetic, lastPerGroupOld, oldEventsSortKey);
	            }
	            GenerateOutputBatchedViewPerKey(newData, newDataMultiKey, false, generateSynthetic, lastPerGroupNew, newEventsSortKey);
	        }

	        var newEventsArr = (lastPerGroupNew.IsEmpty()) ? null : lastPerGroupNew.Values.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEventsArr = (lastPerGroupOld.IsEmpty()) ? null : lastPerGroupOld.Values.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.Values.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.Values.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
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
	        IList<EventBean> resultNewEvents = new List<EventBean>();
	        IList<object> resultNewSortKeys = null;
	        IList<EventBean> resultOldEvents = null;
	        IList<object> resultOldSortKeys = null;

	        if (_orderByProcessor != null) {
	            resultNewSortKeys = new List<object>();
	        }
	        if (Prototype.IsSelectRStream) {
	            resultOldEvents = new List<EventBean>();
	            resultOldSortKeys = new List<object>();
	        }

	        _workCollection.Clear();
	        if (Prototype.OptionalHavingNode == null)
            {
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
	                        _eventsPerStreamOneStream[0] = newData[i];
	                        var mk = newDataMultiKey[i];
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            _workCollection.Put(mk, new EventBean[]{newData[i]});
	                        }
	                        _aggregationService.ApplyEnter(_eventsPerStreamOneStream, mk, AgentInstanceContext);
	                    }
	                }

	                if (oldData != null)
	                {
	                    // apply new data to aggregates
	                    for (var i = 0; i < oldData.Length; i++)
	                    {
	                        _eventsPerStreamOneStream[0] = oldData[i];
	                        var mk = oldDataMultiKey[i];
	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            _workCollection.Put(mk, new EventBean[]{oldData[i]});
	                        }
	                        _aggregationService.ApplyLeave(_eventsPerStreamOneStream, mk, AgentInstanceContext);
	                    }
	                }

	                // there is no remove stream currently for output first
	                GenerateOutputBatchedArr(_workCollection, false, generateSynthetic, resultNewEvents, resultNewSortKeys);
	            }
	        }
	        else {  // has a having-clause
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
	                        _eventsPerStreamOneStream[0] = newData[i];
	                        var mk = newDataMultiKey[i];
	                        _aggregationService.ApplyEnter(_eventsPerStreamOneStream, mk, AgentInstanceContext);
	                    }
	                }

	                if (oldData != null)
	                {
	                    for (var i = 0; i < oldData.Length; i++)
	                    {
	                        _eventsPerStreamOneStream[0] = oldData[i];
	                        var mk = oldDataMultiKey[i];
	                        _aggregationService.ApplyLeave(_eventsPerStreamOneStream, mk, AgentInstanceContext);
	                    }
	                }

	                if (newData != null)
	                {
                        var evaluateParams = new EvaluateParams(_eventsPerStreamOneStream, true, AgentInstanceContext);
                        
                        // check having clause and first-condition
	                    for (var i = 0; i < newData.Length; i++)
	                    {
	                        _eventsPerStreamOneStream[0] = newData[i];
	                        var mk = newDataMultiKey[i];
	                        _aggregationService.SetCurrentAccess(mk, AgentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(newData[i]);}
	                        var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); }
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
	                            _workCollection.Put(mk, new EventBean[]{newData[i]});
	                        }
	                    }
	                }

	                if (oldData != null)
	                {
                        var evaluateParams = new EvaluateParams(_eventsPerStreamOneStream, true, AgentInstanceContext);
                        
                        // apply new data to aggregates
	                    for (var i = 0; i < oldData.Length; i++)
	                    {
	                        _eventsPerStreamOneStream[0] = oldData[i];
	                        var mk = oldDataMultiKey[i];
	                        _aggregationService.SetCurrentAccess(mk, AgentInstanceContext.AgentInstanceId, null);

	                        // Filter the having clause
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(oldData[i]);}
	                        var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); }
	                        if ((result == null) || (false.Equals(result)))
	                        {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelper.GetOrAllocate(mk, AgentInstanceContext, Prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(0, 1);
	                        if (pass) {
	                            _workCollection.Put(mk, new EventBean[]{oldData[i]});
	                        }
	                    }
	                }

	                // there is no remove stream currently for output first
	                GenerateOutputBatchedArr(_workCollection, false, generateSynthetic, resultNewEvents, resultNewSortKeys);
	            }
	        }

	        EventBean[] newEventsArr = null;
	        EventBean[] oldEventsArr = null;
	        if (!resultNewEvents.IsEmpty()) {
	            newEventsArr = resultNewEvents.ToArray();
	        }
	        if ((resultOldEvents != null) && (!resultOldEvents.IsEmpty())) {
	            oldEventsArr = resultOldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (resultNewSortKeys.IsEmpty()) ? null : resultNewSortKeys.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (resultOldSortKeys.IsEmpty()) ? null : resultOldSortKeys.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private UniformPair<EventBean[]> ProcessOutputLimitedViewAll(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
	        IList<EventBean> newEvents = new List<EventBean>();
	        IList<EventBean> oldEvents = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEvents = new List<EventBean>();
	        }

	        IList<object> newEventsSortKey = null;
	        IList<object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new List<object>();
	            if (Prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        _workCollection.Clear();

	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            var newDataMultiKey = GenerateGroupKeys(newData, true);
	            var oldDataMultiKey = GenerateGroupKeys(oldData, false);

	            var eventsPerStream = new EventBean[1];
	            if (newData != null)
	            {
	                // apply new data to aggregates
	                var count = 0;
	                foreach (var aNewData in newData)
	                {
	                    var mk = newDataMultiKey[count];
	                    eventsPerStream[0] = aNewData;
	                    _aggregationService.ApplyEnter(eventsPerStream, mk, AgentInstanceContext);
	                    count++;

	                    // keep the new event as a representative for the group
	                    _workCollection.Put(mk, eventsPerStream);
	                    _outputAllGroupReps.Put(mk, new EventBean[] {aNewData});
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    eventsPerStream[0] = anOldData;
	                    _aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }

	            if (Prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedViewUnkeyed(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }
	            GenerateOutputBatchedViewUnkeyed(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
	        }

	        // For any group representatives not in the work collection, generate a row
	        var entryIterator = _outputAllGroupReps.EntryIterator();
	        while (entryIterator.MoveNext())
	        {
	            var entry = entryIterator.Current;
	            if (!_workCollection.ContainsKey(entry.Key))
	            {
	                _workCollectionTwo.Put(entry.Key, entry.Value);
	                GenerateOutputBatchedArr(_workCollectionTwo, true, generateSynthetic, newEvents, newEventsSortKey);
	                _workCollectionTwo.Clear();
	            }
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
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
	        if (Prototype.IsSelectRStream)
	        {
	            oldEvents = new List<EventBean>();
	        }

	        IList<object> newEventsSortKey = null;
	        IList<object> oldEventsSortKey = null;
	        if (_orderByProcessor != null)
	        {
	            newEventsSortKey = new List<object>();
	            if (Prototype.IsSelectRStream)
	            {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        foreach (var pair in viewEventsList)
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
	                    _eventsPerStreamOneStream[0] = aNewData;
	                    _aggregationService.ApplyEnter(_eventsPerStreamOneStream, newDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }
	            if (oldData != null)
	            {
	                // apply old data to aggregates
	                var count = 0;
	                foreach (var anOldData in oldData)
	                {
	                    _eventsPerStreamOneStream[0] = anOldData;
	                    _aggregationService.ApplyLeave(_eventsPerStreamOneStream, oldDataMultiKey[count], AgentInstanceContext);
	                    count++;
	                }
	            }

	            if (Prototype.IsSelectRStream)
	            {
	                GenerateOutputBatchedViewUnkeyed(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }
	            GenerateOutputBatchedViewUnkeyed(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (Prototype.IsSelectRStream)
	        {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null)
	        {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, AgentInstanceContext);
	            if (Prototype.IsSelectRStream)
	            {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, AgentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null))
	        {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private void GenerateOutputBatchedArr(IDictionary<object, EventBean[]> keysAndEvents, bool isNewData, bool isSynthesize, IList<EventBean> resultEvents, IList<object> optSortKeys)
	    {
	        foreach (var entry in keysAndEvents)
	        {
	            var eventsPerStream = entry.Value;

	            // Set the current row of aggregation states
	            _aggregationService.SetCurrentAccess(entry.Key, AgentInstanceContext.AgentInstanceId, null);

	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(entry.Value);}
	                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext));

	            if(Prototype.IsSorting)
	            {
	                optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, AgentInstanceContext));
	            }
	        }
	    }

	    public void GenerateOutputBatchedViewUnkeyed(EventBean[] outputEvents, object[] groupByKeys, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, IList<object> optSortKeys)
	    {
	        if (outputEvents == null)
	        {
	            return;
	        }

	        var eventsPerStream = new EventBean[1];

            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, AgentInstanceContext);
            
            var count = 0;
	        for (var i = 0; i < outputEvents.Length; i++)
	        {
	            _aggregationService.SetCurrentAccess(groupByKeys[count], AgentInstanceContext.AgentInstanceId, null);
	            eventsPerStream[0] = outputEvents[count];

	            // Filter the having clause
	            if (Prototype.OptionalHavingNode != null)
	            {
	                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(outputEvents[count]);}
	                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); }
	                if ((result == null) || (false.Equals(result)))
	                {
	                    continue;
	                }
	            }

	            resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, AgentInstanceContext));
	            if(Prototype.IsSorting)
	            {
	                optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, AgentInstanceContext));
	            }

	            count++;
	        }
	    }
	}
} // end of namespace

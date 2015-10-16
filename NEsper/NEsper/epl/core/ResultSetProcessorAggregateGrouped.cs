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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Result-set processor for the aggregate-grouped case: there is a group-by and one or 
    /// more non-aggregation event properties in the select clause are not listed in the group 
    /// by, and there are aggregation functions. <para/>This processor does perform grouping 
    /// by computing MultiKey group-by keys for each row. The processor generates one row for 
    /// each event entering (new event) and one row for each event leaving (old event).
    /// <para/>
    /// Aggregation state is a table of rows held by <seealso cref="agg.service.AggregationService"/> 
    /// where the row key is the group-by MultiKey.
    /// </summary>
    public class ResultSetProcessorAggregateGrouped
        : ResultSetProcessor
        , AggregationRowRemovedCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        internal readonly ResultSetProcessorAggregateGroupedFactory Prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        internal readonly AggregationService AggregationService;
        private AgentInstanceContext _agentInstanceContext;
    
        internal readonly EventBean[] EventsPerStreamOneStream = new EventBean[1];
    
        // For output limiting, keep a representative of each group-by group
        private readonly IDictionary<Object, EventBean[]> _eventGroupReps = new Dictionary<Object, EventBean[]>();
        private readonly IDictionary<Object, EventBean[]> _workCollection = new LinkedHashMap<Object, EventBean[]>();
        private readonly IDictionary<Object, EventBean[]> _workCollectionTwo = new LinkedHashMap<Object, EventBean[]>();
    
        // For sorting, keep the generating events for each outgoing event
        private readonly IDictionary<Object, EventBean[]> _newGenerators = new Dictionary<Object, EventBean[]>();
        private readonly IDictionary<Object, EventBean[]> _oldGenerators = new Dictionary<Object, EventBean[]>();

        private readonly IDictionary<Object, OutputConditionPolled> _outputState = new Dictionary<Object, OutputConditionPolled>();

        private ResultSetProcessorAggregateGroupedOutputLastHelper _outputLastHelper;
        private ResultSetProcessorAggregateGroupedOutputAllHelper _outputAllHelper;

        public ResultSetProcessorAggregateGrouped(ResultSetProcessorAggregateGroupedFactory prototype, SelectExprProcessor selectExprProcessor, OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            Prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _orderByProcessor = orderByProcessor;
            AggregationService = aggregationService;
            _agentInstanceContext = agentInstanceContext;
            aggregationService.SetRemovedCallback(this);
        
            if (prototype.IsOutputLast)
            {
                _outputLastHelper = new ResultSetProcessorAggregateGroupedOutputLastHelper(this);
            }
            else if (prototype.IsOutputAll)
            {
                _outputAllHelper = new ResultSetProcessorAggregateGroupedOutputAllHelper(this);
            }
        }

        public AgentInstanceContext AgentInstanceContext
        {
            get { return _agentInstanceContext; }
            set { _agentInstanceContext = value; }
        }

        public EventType ResultEventType
        {
            get { return Prototype.ResultEventType; }
        }

        public void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            var eventsPerStream = new EventBean[1];
            if (newData != null)
            {
                // apply new data to aggregates
                foreach (var aNewData in newData) {
                    eventsPerStream[0] = aNewData;
                    var mk = GenerateGroupKey(eventsPerStream, true);
                    AggregationService.ApplyEnter(eventsPerStream, mk, _agentInstanceContext);
                }
            }
            if (oldData != null) {
                // apply old data to aggregates
                foreach (var anOldData in oldData) {
                    eventsPerStream[0] = anOldData;
                    var mk = GenerateGroupKey(eventsPerStream, false);
                    AggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
                }
            }
        }

        public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            if (!newEvents.IsEmpty()) {
                // apply old data to aggregates
                foreach (var eventsPerStream in newEvents) {
                    var mk = GenerateGroupKey(eventsPerStream.Array, true);
                    AggregationService.ApplyEnter(eventsPerStream.Array, mk, _agentInstanceContext);
                }
            }
            if (oldEvents != null && !oldEvents.IsEmpty()) {
                // apply old data to aggregates
                foreach (var eventsPerStream in oldEvents) {
                    var mk = GenerateGroupKey(eventsPerStream.Array, false);
                    AggregationService.ApplyLeave(eventsPerStream.Array, mk, _agentInstanceContext);
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
    
            // Update aggregates
            if (newEvents.IsNotEmpty())
            {
                // apply old data to aggregates
                var count = 0;
                foreach (var eventsPerStream in newEvents)
                {
                    AggregationService.ApplyEnter(eventsPerStream.Array, newDataGroupByKeys[count], _agentInstanceContext);
                    count++;
                }
            }
            if (oldEvents.IsNotEmpty())
            {
                // apply old data to aggregates
                var count = 0;
                foreach (var eventsPerStream in oldEvents)
                {
                    AggregationService.ApplyLeave(eventsPerStream.Array, oldDataGroupByKeys[count], _agentInstanceContext);
                    count++;
                }
            }
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream)
            {
                selectOldEvents = GenerateOutputEventsJoin(oldEvents, oldDataGroupByKeys, _oldGenerators, false, isSynthesize);
            }
            var selectNewEvents = GenerateOutputEventsJoin(newEvents, newDataGroupByKeys, _newGenerators, true, isSynthesize);
    
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
    
            // Update aggregates
            var eventsPerStream = new EventBean[1];
            if (newData != null)
            {
                // apply new data to aggregates
                for (var i = 0; i < newData.Length; i++)
                {
                    eventsPerStream[0] = newData[i];
                    AggregationService.ApplyEnter(eventsPerStream, newDataGroupByKeys[i], _agentInstanceContext);
                }
            }
            if (oldData != null)
            {
                // apply old data to aggregates
                for (var i = 0; i < oldData.Length; i++)
                {
                    eventsPerStream[0] = oldData[i];
                    AggregationService.ApplyLeave(eventsPerStream, oldDataGroupByKeys[i], _agentInstanceContext);
                }
            }
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream)
            {
                selectOldEvents = GenerateOutputEventsView(oldData, oldDataGroupByKeys, _oldGenerators, false, isSynthesize);
            }
            var selectNewEvents = GenerateOutputEventsView(newData, newDataGroupByKeys, _newGenerators, true, isSynthesize);
    
            if ((selectNewEvents != null) || (selectOldEvents != null))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerEvent(selectNewEvents, selectOldEvents);}
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerEvent(null, null);}
            return null;
        }
    
    	private EventBean[] GenerateOutputEventsView(EventBean[] outputEvents, Object[] groupByKeys, IDictionary<Object, EventBean[]> generators, bool isNewData, bool isSynthesize)
        {
            if (outputEvents == null)
            {
                return null;
            }
    
            var eventsPerStream = new EventBean[1];
            var events = new EventBean[outputEvents.Length];
            var keys = new Object[outputEvents.Length];
            EventBean[][] currentGenerators = null;
            if(Prototype.IsSorting)
            {
            	currentGenerators = new EventBean[outputEvents.Length][];
            }
    
            var countOutputRows = 0;
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
            for (var countInputRows = 0; countInputRows < outputEvents.Length; countInputRows++)
            {
                AggregationService.SetCurrentAccess(groupByKeys[countInputRows], _agentInstanceContext.AgentInstanceId, null);
                eventsPerStream[0] = outputEvents[countInputRows];
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(outputEvents[countOutputRows]);}
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
                    if ((result == null) || (false.Equals(result))) {
                        continue;
                    }
                }

                events[countOutputRows] = _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
                keys[countOutputRows] = groupByKeys[countInputRows];
                if(Prototype.IsSorting)
                {
                    var currentEventsPerStream = new EventBean[] { outputEvents[countInputRows] };
                	generators.Put(keys[countOutputRows], currentEventsPerStream);
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
                	var outKeys = new Object[countOutputRows];
                	Array.Copy(keys, 0, outKeys, 0, countOutputRows);
                	keys = outKeys;
    
                	var outGens = new EventBean[countOutputRows][];
                	Array.Copy(currentGenerators, 0, outGens, 0, countOutputRows);
                	currentGenerators = outGens;
                }
            }
    
            if(Prototype.IsSorting)
            {
                events = _orderByProcessor.Sort(events, currentGenerators, keys, isNewData, _agentInstanceContext);
            }
    
            return events;
        }
    
        internal Object[] GenerateGroupKeys(ISet<MultiKey<EventBean>> resultSet, bool isNewData)
        {
            if (resultSet.IsEmpty())
            {
                return null;
            }
    
            var keys = new Object[resultSet.Count];
    
            var count = 0;
            foreach (var eventsPerStream in resultSet)
            {
                keys[count] = GenerateGroupKey(eventsPerStream.Array, isNewData);
                count++;
            }
    
            return keys;
        }

        internal Object[] GenerateGroupKeys(EventBean[] events, bool isNewData)
        {
            if (events == null) {
                return null;
            }
    
            var eventsPerStream = new EventBean[1];
            var keys = new Object[events.Length];
    
            for (var i = 0; i < events.Length; i++)
            {
                eventsPerStream[0] = events[i];
                keys[i] = GenerateGroupKey(eventsPerStream, isNewData);
            }
    
            return keys;
        }
    
        /// <summary>Generates the group-by key for the row </summary>
        /// <param name="eventsPerStream">is the row of events</param>
        /// <param name="isNewData">is true for new data</param>
        /// <returns>grouping keys</returns>
        public Object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QResultSetProcessComputeGroupKeys(isNewData, Prototype.GroupKeyNodeExpressions, eventsPerStream);
    
                Object keyObject;
                if (Prototype.GroupKeyNode != null)
                {
                    keyObject = Prototype.GroupKeyNode.Evaluate(evaluateParams);
                }
                else
                {
                    var keysX = new Object[Prototype.GroupKeyNodes.Length];
                    var countX = 0;
                    foreach (var exprNode in Prototype.GroupKeyNodes)
                    {
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
    
            var keys = new Object[Prototype.GroupKeyNodes.Length];
            var count = 0;
            foreach (var exprNode in Prototype.GroupKeyNodes) {
                keys[count] = exprNode.Evaluate(evaluateParams);
                count++;
            }
            return new MultiKeyUntyped(keys);
        }
    
        private EventBean[] GenerateOutputEventsJoin(ISet<MultiKey<EventBean>> resultSet, Object[] groupByKeys, IDictionary<Object, EventBean[]> generators, bool isNewData, bool isSynthesize)
        {
            if (resultSet.IsEmpty())
            {
                return null;
            }
    
            var events = new EventBean[resultSet.Count];
            var keys = new Object[resultSet.Count];
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
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);

                AggregationService.SetCurrentAccess(groupByKeys[countInputRows], _agentInstanceContext.AgentInstanceId, null);
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                    if ((result == null) || (false.Equals(result)))
                    {
                        continue;
                    }
                }

                events[countOutputRows] = _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
                keys[countOutputRows] = groupByKeys[countOutputRows];
                if(Prototype.IsSorting)
                {
                	generators.Put(keys[countOutputRows], eventsPerStream);
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
                	var outKeys = new Object[countOutputRows];
                	Array.Copy(keys, 0, outKeys, 0, countOutputRows);
                	keys = outKeys;
    
                	var outGens = new EventBean[countOutputRows][];
                	Array.Copy(currentGenerators, 0, outGens, 0, countOutputRows);
                	currentGenerators = outGens;
                }
            }
    
            if(Prototype.IsSorting)
            {
                events = _orderByProcessor.Sort(events, currentGenerators, keys, isNewData, _agentInstanceContext);
            }
            return events;
        }

        private IEnumerator<EventBean> AggregateGroupedEnumerator(Viewable baseEnum)
        {
            var eventsPerStream = new EventBean[1];

            foreach (var candidate in baseEnum)
            {
                eventsPerStream[0] = candidate;

                var groupKey = GenerateGroupKey(eventsPerStream, true);
                AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, null);

                bool? pass = true;
                if (OptionalHavingNode != null)
                {
                    pass = (bool?)OptionalHavingNode.Evaluate(new EvaluateParams(eventsPerStream, true, _agentInstanceContext));
                }

                if (pass.GetValueOrDefault(false))
                {
                    yield return SelectExprProcessor.Process(eventsPerStream, true, true, _agentInstanceContext);
                }
            }
        }
        public IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (!Prototype.IsHistoricalOnly)
            {
                return ObtainIterator(parent);
            }

            AggregationService.ClearResults(_agentInstanceContext);
            var it = parent.GetEnumerator();
            var eventsPerStream = new EventBean[1];
            for (; it.MoveNext(); )
            {
                eventsPerStream[0] = it.Current;
                var groupKey = GenerateGroupKey(eventsPerStream, true);
                AggregationService.ApplyEnter(eventsPerStream, groupKey, _agentInstanceContext);
            }

            var deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainIterator(parent));
            AggregationService.ClearResults(_agentInstanceContext);
            return deque.GetEnumerator();
        }

        private IEnumerator<EventBean> ObtainIterator(Viewable parent)
        {
            if (_orderByProcessor == null)
            {
                return AggregateGroupedEnumerator(parent);
            }
    
            // Pull all parent events, generate order keys
            var eventsPerStream = new EventBean[1];
            var outgoingEvents = new List<EventBean>();
            var orderKeys = new List<Object>();
            var evaluateParams = new EvaluateParams(eventsPerStream, true, _agentInstanceContext);
    
            foreach (var candidate in parent) {
                eventsPerStream[0] = candidate;
    
                var groupKey = GenerateGroupKey(eventsPerStream, true);
                AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, null);
    
                if (Prototype.OptionalHavingNode != null) {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(candidate);}
                    var pass = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(pass.AsBoxedBoolean()); }
                    if ((pass == null) || (false.Equals(pass)))
                    {
                        continue;
                    }
                }

                outgoingEvents.Add(_selectExprProcessor.Process(eventsPerStream, true, true, _agentInstanceContext));

                var orderKey = _orderByProcessor.GetSortKey(eventsPerStream, true, _agentInstanceContext);
                orderKeys.Add(orderKey);
            }
    
            // sort
            var outgoingEventsArr = outgoingEvents.ToArray();
            var orderKeysArr = orderKeys.ToArray();
            var orderedEvents = _orderByProcessor.Sort(outgoingEventsArr, orderKeysArr, _agentInstanceContext);
    
            return ((IEnumerable<EventBean>) orderedEvents).GetEnumerator();
        }

        /// <summary>Returns the select expression processor </summary>
        /// <value>select processor.</value>
        public SelectExprProcessor SelectExprProcessor
        {
            get { return _selectExprProcessor; }
        }

        /// <summary>Returns the having node. </summary>
        /// <value>having expression</value>
        public ExprEvaluator OptionalHavingNode
        {
            get { return Prototype.OptionalHavingNode; }
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            // Generate group-by keys for all events
            var groupByKeys = GenerateGroupKeys(joinSet, true);
            var result = GenerateOutputEventsJoin(joinSet, groupByKeys, _newGenerators, true, true);
            if (result == null)
                return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
            return ((IEnumerable<EventBean>)result).GetEnumerator();
        }
    
        public void Clear()
        {
            AggregationService.ClearResults(_agentInstanceContext);
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT)
            {
                var newEvents = new List<EventBean>();
                LinkedList<EventBean> oldEvents = null;
                if (Prototype.IsSelectRStream)
                {
                     oldEvents = new LinkedList<EventBean>();
                }

                LinkedList<Object> newEventsSortKey = null;
                LinkedList<Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedList<Object>();
                    if (Prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedList<Object>();
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
                            AggregationService.ApplyEnter(aNewData.Array, newDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        var count = 0;
                        foreach (var anOldData in oldData)
                        {
                            AggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
    
                    if (Prototype.IsSelectRStream)
                    {
                        GenerateOutputBatchedJoin(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
                    }
                    GenerateOutputBatchedJoin(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
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
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
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
            else if (outputLimitLimitType == OutputLimitLimitType.ALL)
            {
                var newEvents = new List<EventBean>();
                LinkedList<EventBean> oldEvents = null;
                if (Prototype.IsSelectRStream)
                {
                    oldEvents = new LinkedList<EventBean>();
                }

                LinkedList<Object> newEventsSortKey = null;
                LinkedList<Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedList<Object>();
                    if (Prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedList<Object>();
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
                            AggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
                            count++;
    
                            // keep the new event as a representative for the group
                            _workCollection.Put(mk, aNewData.Array);
                            _eventGroupReps.Put(mk, aNewData.Array);
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        var count = 0;
                        foreach (var anOldData in oldData)
                        {
                            AggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
    
                    if (Prototype.IsSelectRStream)
                    {
                        GenerateOutputBatchedJoin(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
                    }
                    GenerateOutputBatchedJoin(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
                }
    
                // For any group representatives not in the work collection, generate a row
                foreach (var entry in _eventGroupReps)
                {
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
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
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
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                var resultNewEvents = new List<EventBean>();
                List<Object> resultNewSortKeys = null;
                List<EventBean> resultOldEvents = null;
                List<Object> resultOldSortKeys = null;
    
                if (_orderByProcessor != null) {
                    resultNewSortKeys = new List<Object>();
                }
                if (Prototype.IsSelectRStream) {
                    resultOldEvents = new List<EventBean>();
                    resultOldSortKeys = new List<Object>();
                }
    
                _workCollection.Clear();
    
                if (Prototype.OptionalHavingNode == null) {
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
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(newDataMultiKey[count], outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                                if (pass) {
                                    _workCollection.Put(mk, aNewData.Array);
                                }
                                AggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
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
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(oldDataMultiKey[count], outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    _workCollection.Put(mk, aOldData.Array);
                                }
                                AggregationService.ApplyLeave(aOldData.Array, mk, _agentInstanceContext);
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
                                AggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
                                count++;
                            }
                        }
    
                        if (oldData != null)
                        {
                            var count = 0;
                            foreach (var aOldData in oldData)
                            {
                                var mk = oldDataMultiKey[count];
                                AggregationService.ApplyLeave(aOldData.Array, mk, _agentInstanceContext);
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
                                AggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);
    
                                // Filter the having clause
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aNewData.Array);}
                                var evaluateParams = new EvaluateParams(aNewData.Array, true, _agentInstanceContext);
                                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                                if ((result == null) || (false.Equals(result)))
                                {
                                    count++;
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
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
                                AggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);
    
                                // Filter the having clause
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aOldData.Array);}
                                var evaluateParams = new EvaluateParams(aOldData.Array, true, _agentInstanceContext);
                                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                                if ((result == null) || (false.Equals(result)))
                                {
                                    count++;
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
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
                if (resultNewEvents.IsNotEmpty()) {
                    newEventsArr = resultNewEvents.ToArray();
                }
                if ((resultOldEvents != null) && (resultOldEvents.IsNotEmpty())) {
                    oldEventsArr = resultOldEvents.ToArray();
                }
    
                if (_orderByProcessor != null)
                {
                    var sortKeysNew = (resultNewSortKeys.IsEmpty()) ? null : resultNewSortKeys.ToArray();
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
                    {
                        var sortKeysOld = (resultOldSortKeys.IsEmpty()) ? null : resultOldSortKeys.ToArray();
                        oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
                    }
                }
    
                if ((newEventsArr == null) && (oldEventsArr == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
            }
            else // (outputLimitLimitType == OutputLimitLimitType.LAST) Compute last per group
            {
                IDictionary<Object, EventBean> lastPerGroupNew = new LinkedHashMap<Object, EventBean>();
                IDictionary<Object, EventBean> lastPerGroupOld = null;
                if (Prototype.IsSelectRStream)
                {
                    lastPerGroupOld = new LinkedHashMap<Object, EventBean>();
                }
    
                IDictionary<Object, Object> newEventsSortKey = null; // group key to sort key
                IDictionary<Object, Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedHashMap<Object, Object>();
                    if (Prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedHashMap<Object, Object>();
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
                            AggregationService.ApplyEnter(aNewData.Array, mk, _agentInstanceContext);
                            count++;
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        var count = 0;
                        foreach (var anOldData in oldData)
                        {
                            _workCollection.Put(oldDataMultiKey[count], anOldData.Array);
                            AggregationService.ApplyLeave(anOldData.Array, oldDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
    
                    if (Prototype.IsSelectRStream)
                    {
                        GenerateOutputBatchedJoin(oldData, oldDataMultiKey, false, generateSynthetic, lastPerGroupOld, oldEventsSortKey);
                    }
                    GenerateOutputBatchedJoin(newData, newDataMultiKey, false, generateSynthetic, lastPerGroupNew, newEventsSortKey);
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
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
                    {
                        var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.Values.ToArray();
                        oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
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
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT)
            {
                var newEvents = new LinkedList<EventBean>();
                LinkedList<EventBean> oldEvents = null;
                if (Prototype.IsSelectRStream)
                {
                    oldEvents = new LinkedList<EventBean>();
                }

                LinkedList<Object> newEventsSortKey = null;
                LinkedList<Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedList<Object>();
                    if (Prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedList<Object>();
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
                            EventsPerStreamOneStream[0] = aNewData;
                            AggregationService.ApplyEnter(EventsPerStreamOneStream, newDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        var count = 0;
                        foreach (var anOldData in oldData)
                        {
                            EventsPerStreamOneStream[0] = anOldData;
                            AggregationService.ApplyLeave(EventsPerStreamOneStream, oldDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
    
                    if (Prototype.IsSelectRStream)
                    {
                        GenerateOutputBatchedView(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
                    }
                    GenerateOutputBatchedView(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
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
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
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
            else if (outputLimitLimitType == OutputLimitLimitType.ALL)
            {
                var newEvents = new LinkedList<EventBean>();
                LinkedList<EventBean> oldEvents = null;
                if (Prototype.IsSelectRStream)
                {
                    oldEvents = new LinkedList<EventBean>();
                }

                LinkedList<Object> newEventsSortKey = null;
                LinkedList<Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedList<Object>();
                    if (Prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedList<Object>();
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
                            AggregationService.ApplyEnter(eventsPerStream, mk, _agentInstanceContext);
                            count++;
    
                            // keep the new event as a representative for the group
                            _workCollection.Put(mk, eventsPerStream);
                            _eventGroupReps.Put(mk, new EventBean[] {aNewData});
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        var count = 0;
                        foreach (var anOldData in oldData)
                        {
                            eventsPerStream[0] = anOldData;
                            AggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
    
                    if (Prototype.IsSelectRStream)
                    {
                        GenerateOutputBatchedView(oldData, oldDataMultiKey, false, generateSynthetic, oldEvents, oldEventsSortKey);
                    }
                    GenerateOutputBatchedView(newData, newDataMultiKey, true, generateSynthetic, newEvents, newEventsSortKey);
                }
    
                // For any group representatives not in the work collection, generate a row
                foreach (var entry in _eventGroupReps)
                {
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
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
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
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST)
            {
                var resultNewEvents = new List<EventBean>();
                List<Object> resultNewSortKeys = null;
                List<EventBean> resultOldEvents = null;
                List<Object> resultOldSortKeys = null;
    
                if (_orderByProcessor != null) {
                    resultNewSortKeys = new List<Object>();
                }
                if (Prototype.IsSelectRStream) {
                    resultOldEvents = new List<EventBean>();
                    resultOldSortKeys = new List<Object>();
                }
    
                _workCollection.Clear();
                if (Prototype.OptionalHavingNode == null) {
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
                                EventsPerStreamOneStream[0] = newData[i];
                                var mk = newDataMultiKey[i];
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException e) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(newDataMultiKey[i], outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                                if (pass) {
                                    _workCollection.Put(mk, new EventBean[]{newData[i]});
                                }
                                AggregationService.ApplyEnter(EventsPerStreamOneStream, mk, _agentInstanceContext);
                            }
                        }
    
                        if (oldData != null)
                        {
                            // apply new data to aggregates
                            for (var i = 0; i < oldData.Length; i++)
                            {
                                EventsPerStreamOneStream[0] = oldData[i];
                                var mk = oldDataMultiKey[i];
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException e) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(oldDataMultiKey[i], outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    _workCollection.Put(mk, new EventBean[]{oldData[i]});
                                }
                                AggregationService.ApplyLeave(EventsPerStreamOneStream, mk, _agentInstanceContext);
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
                                EventsPerStreamOneStream[0] = newData[i];
                                var mk = newDataMultiKey[i];
                                AggregationService.ApplyEnter(EventsPerStreamOneStream, mk, _agentInstanceContext);
                            }
                        }
    
                        if (oldData != null)
                        {
                            for (var i = 0; i < oldData.Length; i++)
                            {
                                EventsPerStreamOneStream[0] = oldData[i];
                                var mk = oldDataMultiKey[i];
                                AggregationService.ApplyLeave(EventsPerStreamOneStream, mk, _agentInstanceContext);
                            }
                        }
    
                        if (newData != null)
                        {
                            // check having clause and first-condition
                            for (var i = 0; i < newData.Length; i++)
                            {
                                EventsPerStreamOneStream[0] = newData[i];
                                var mk = newDataMultiKey[i];
                                AggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);
    
                                // Filter the having clause
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(newData[i]);}
                                var evaluateParams = new EvaluateParams(EventsPerStreamOneStream, true, _agentInstanceContext);
                                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); }
                                if ((result == null) || (false.Equals(result)))
                                {
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                                if (pass) {
                                    _workCollection.Put(mk, new EventBean[]{newData[i]});
                                }
                            }
                        }
    
                        if (oldData != null)
                        {
                            // apply new data to aggregates
                            for (var i = 0; i < oldData.Length; i++)
                            {
                                EventsPerStreamOneStream[0] = oldData[i];
                                var mk = oldDataMultiKey[i];
                                AggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);
    
                                // Filter the having clause
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(oldData[i]);}
                                var evaluateParams = new EvaluateParams(EventsPerStreamOneStream, true, _agentInstanceContext);
                                var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); }
                                if ((result == null) || (false.Equals(result)))
                                {
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(oldDataMultiKey[i], outputStateGroup);
                                }
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
                if (resultNewEvents.IsNotEmpty()) {
                    newEventsArr = resultNewEvents.ToArray();
                }
                if ((resultOldEvents != null) && (resultOldEvents.IsNotEmpty())) {
                    oldEventsArr = resultOldEvents.ToArray();
                }
    
                if (_orderByProcessor != null)
                {
                    var sortKeysNew = (resultNewSortKeys.IsEmpty()) ? null : resultNewSortKeys.ToArray();
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
                    {
                        var sortKeysOld = (resultOldSortKeys.IsEmpty()) ? null : resultOldSortKeys.ToArray();
                        oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
                    }
                }
    
                if ((newEventsArr == null) && (oldEventsArr == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
            }
            else // (outputLimitLimitType == OutputLimitLimitType.LAST) Compute last per group
            {
                IDictionary<Object, EventBean> lastPerGroupNew = new LinkedHashMap<Object, EventBean>();
                IDictionary<Object, EventBean> lastPerGroupOld = null;
                if (Prototype.IsSelectRStream)
                {
                    lastPerGroupOld = new LinkedHashMap<Object, EventBean>();
                }
    
                IDictionary<Object, Object> newEventsSortKey = null; // group key to sort key
                IDictionary<Object, Object> oldEventsSortKey = null;
                if (_orderByProcessor != null)
                {
                    newEventsSortKey = new LinkedHashMap<Object, Object>();
                    if (Prototype.IsSelectRStream)
                    {
                        oldEventsSortKey = new LinkedHashMap<Object, Object>();
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
                            EventsPerStreamOneStream[0] = aNewData;
                            AggregationService.ApplyEnter(EventsPerStreamOneStream, mk, _agentInstanceContext);
                            count++;
                        }
                    }
                    if (oldData != null)
                    {
                        // apply old data to aggregates
                        var count = 0;
                        foreach (var anOldData in oldData)
                        {
                            EventsPerStreamOneStream[0] = anOldData;
                            AggregationService.ApplyLeave(EventsPerStreamOneStream, oldDataMultiKey[count], _agentInstanceContext);
                            count++;
                        }
                    }
    
                    if (Prototype.IsSelectRStream)
                    {
                        GenerateOutputBatchedView(oldData, oldDataMultiKey, false, generateSynthetic, lastPerGroupOld, oldEventsSortKey);
                    }
                    GenerateOutputBatchedView(newData, newDataMultiKey, false, generateSynthetic, lastPerGroupNew, newEventsSortKey);
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
                    newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                    if (Prototype.IsSelectRStream)
                    {
                        var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.Values.ToArray();
                        oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
                    }
                }
    
                if ((newEventsArr == null) && (oldEventsArr == null))
                {
                    return null;
                }
                return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
            }
        }
    
        private void GenerateOutputBatchedArr(IDictionary<Object, EventBean[]> keysAndEvents, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<Object> optSortKeys)
        {
            foreach (var entry in keysAndEvents)
            {
                var eventsPerStream = entry.Value;
    
                // Set the current row of aggregation states
                AggregationService.SetCurrentAccess(entry.Key, _agentInstanceContext.AgentInstanceId, null);
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(entry.Value);}
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                    if ((result == null) || (false.Equals(result)))
                    {
                        continue;
                    }
                }
    
                resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
    
                if(Prototype.IsSorting)
                {
                    optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext));
                }
            }
        }
    
        internal void GenerateOutputBatchedView(EventBean[] outputEvents, Object[] groupByKeys, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<object> optSortKeys)
        {
            if (outputEvents == null)
            {
                return;
            }
    
            var eventsPerStream = new EventBean[1];
    
            var count = 0;
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);

            for (var i = 0; i < outputEvents.Length; i++)
            {
                AggregationService.SetCurrentAccess(groupByKeys[count], _agentInstanceContext.AgentInstanceId, null);
                eventsPerStream[0] = outputEvents[count];
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(outputEvents[count]);}
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); }
                    if ((result == null) || (false.Equals(result)))
                    {
                        continue;
                    }
                }
    
                resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
                if(Prototype.IsSorting)
                {
                    optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext));
                }
    
                count++;
            }
        }

        internal void GenerateOutputBatchedJoin(ISet<MultiKey<EventBean>> outputEvents, Object[] groupByKeys, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<object> optSortKeys)
        {
            if (outputEvents == null)
            {
                return;
            }
    
            EventBean[] eventsPerStream;
    
            var count = 0;
            foreach (var row in outputEvents)
            {
                AggregationService.SetCurrentAccess(groupByKeys[count], _agentInstanceContext.AgentInstanceId, null);
                eventsPerStream = row.Array;
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                    if ((result == null) || (false.Equals(result)))
                    {
                        continue;
                    }
                }
    
                resultEvents.Add(_selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
                if(Prototype.IsSorting)
                {
                    optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext));
                }
    
                count++;
            }
        }

        internal EventBean GenerateOutputBatchedSingle(Object groupByKey, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize)
        {
            AggregationService.SetCurrentAccess(groupByKey, _agentInstanceContext.AgentInstanceId, null);

            // Filter the having clause
            if (Prototype.OptionalHavingNode != null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream); }
                var result = Prototype.OptionalHavingNode.Evaluate(new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext)).AsBoxedBoolean();
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result); }
                if ((result == null) || (false.Equals(result)))
                {
                    return null;
                }
            }

            return _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
        }

        internal void GenerateOutputBatchedView(EventBean[] outputEvents, Object[] groupByKeys, bool isNewData, bool isSynthesize, IDictionary<Object, EventBean> resultEvents, IDictionary<Object, Object> optSortKeys)
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
                AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, null);
                eventsPerStream[0] = outputEvents[count];
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(outputEvents[count]);}
                    var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext); 
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); }
                    if ((result == null) || (false.Equals(result)))
                    {
                        continue;
                    }
                }
    
                resultEvents.Put(groupKey, _selectExprProcessor.Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
                if(Prototype.IsSorting)
                {
                    optSortKeys.Put(groupKey, _orderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext));
                }
    
                count++;
            }
        }

        internal void GenerateOutputBatchedJoin(ISet<MultiKey<EventBean>> outputEvents, Object[] groupByKeys, bool isNewData, bool isSynthesize, IDictionary<Object, EventBean> resultEvents, IDictionary<Object, Object> optSortKeys)
        {
            if (outputEvents == null)
            {
                return;
            }
    
            var count = 0;
            foreach (var row in outputEvents)
            {
                var groupKey = groupByKeys[count];
                AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, null);
    
                // Filter the having clause
                if (Prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(row.Array);}
                    var evaluateParams = new EvaluateParams(row.Array, isNewData, _agentInstanceContext);
                    var result = Prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean()); }
                    if ((result == null) || (false.Equals(result)))
                    {
                        continue;
                    }
                }
    
                resultEvents.Put(groupKey, _selectExprProcessor.Process(row.Array, isNewData, isSynthesize, _agentInstanceContext));
                if(Prototype.IsSorting)
                {
                    optSortKeys.Put(groupKey, _orderByProcessor.GetSortKey(row.Array, isNewData, _agentInstanceContext));
                }
    
                count++;
            }
        }

        public bool HasAggregation
        {
            get { return true; }
        }

        public void Removed(Object key)
        {
            _eventGroupReps.Remove(key);
        }

        public void ProcessOutputLimitedLastAllNonBufferedView(EventBean[] newData, EventBean[] oldData, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllHelper.ProcessView(newData, oldData, isGenerateSynthetic);
            }
            else
            {
                _outputLastHelper.ProcessView(newData, oldData, isGenerateSynthetic);
            }
        }

        public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newData, ISet<MultiKey<EventBean>> oldData, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllHelper.ProcessJoin(newData, oldData, isGenerateSynthetic);
            }
            else
            {
                _outputLastHelper.ProcessJoin(newData, oldData, isGenerateSynthetic);
            }
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedView(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllHelper.OutputView(isSynthesize);
            }
            return _outputLastHelper.OutputView(isSynthesize);
        }

        public UniformPair<EventBean[]> ContinueOutputLimitedLastAllNonBufferedJoin(bool isSynthesize, bool isAll)
        {
            if (isAll)
            {
                return _outputAllHelper.OutputJoin(isSynthesize);
            }
            return _outputLastHelper.OutputJoin(isSynthesize);
        }
    }
}

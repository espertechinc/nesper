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
    /// Result set processor for the fully-grouped case: there is a group-by and all 
    /// non-aggregation event properties in the select clause are listed in the group by, 
    /// and there are aggregation functions. <para/>Produces one row for each group that 
    /// changed (and not one row per event). Computes MultiKey group-by keys for each 
    /// event and uses a set of the group-by keys to generate the result rows, using the 
    /// first (old or new, anyone) event for each distinct group-by key.
    /// </summary>
    public class ResultSetProcessorRowPerGroup : ResultSetProcessor, AggregationRowRemovedCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ResultSetProcessorRowPerGroupFactory _prototype;
        private readonly SelectExprProcessor _selectExprProcessor;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly AggregationService _aggregationService;
        private AgentInstanceContext _agentInstanceContext;

        private static int seq = 0;
        private readonly int id = seq++;

        // For output rate limiting, keep a representative event for each group for
        // representing each group in an output limit clause
        private readonly IDictionary<Object, EventBean[]> _groupRepsView = new LinkedHashMap<Object, EventBean[]>();

        private readonly IDictionary<Object, OutputConditionPolled> _outputState = new Dictionary<Object, OutputConditionPolled>();
    
        public ResultSetProcessorRowPerGroup(ResultSetProcessorRowPerGroupFactory prototype, SelectExprProcessor selectExprProcessor, OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            _prototype = prototype;
            _selectExprProcessor = selectExprProcessor;
            _orderByProcessor = orderByProcessor;
            _aggregationService = aggregationService;
            _agentInstanceContext = agentInstanceContext;
            aggregationService.SetRemovedCallback(this);
        }

        public AggregationService AggregationService
        {
            get { return _aggregationService; }
        }

        public SelectExprProcessor SelectExprProcessor
        {
            get { return _selectExprProcessor; }
        }

        public OrderByProcessor OrderByProcessor
        {
            get { return _orderByProcessor; }
        }

        public AgentInstanceContext AgentInstanceContext
        {
            get { return _agentInstanceContext; }
            set { _agentInstanceContext = value; }
        }

        public ResultSetProcessorRowPerGroupFactory Prototype
        {
            get { return _prototype; }
        }

        public virtual EventType ResultEventType
        {
            get { return _prototype.ResultEventType; }
        }

        public virtual void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            EventBean[] eventsPerStream = new EventBean[1];
            if (newData != null) {
                // apply new data to aggregates
                foreach (EventBean aNewData in newData) {
                    eventsPerStream[0] = aNewData;
                    Object mk = GenerateGroupKey(eventsPerStream, true);
                    AggregationService.ApplyEnter(eventsPerStream, mk, AgentInstanceContext);
                }
            }
            if (oldData != null) {
                // apply old data to aggregates
                foreach (EventBean anOldData in oldData) {
                    eventsPerStream[0] = anOldData;
                    Object mk = GenerateGroupKey(eventsPerStream, false);
                    AggregationService.ApplyLeave(eventsPerStream, mk, AgentInstanceContext);
                }
            }
        }

        public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            if (!newEvents.IsEmpty()) {
                // apply old data to aggregates
                foreach (MultiKey<EventBean> eventsPerStream in newEvents) {
                    Object mk = GenerateGroupKey(eventsPerStream.Array, true);
                    AggregationService.ApplyEnter(eventsPerStream.Array, mk, AgentInstanceContext);
                }
            }
            if (oldEvents != null && !oldEvents.IsEmpty()) {
                // apply old data to aggregates
                foreach (MultiKey<EventBean> eventsPerStream in oldEvents) {
                    Object mk = GenerateGroupKey(eventsPerStream.Array, false);
                    AggregationService.ApplyLeave(eventsPerStream.Array, mk, AgentInstanceContext);
                }
            }
        }

        public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}
            // Generate group-by keys for all events, collect all keys in a set for later event generation
            IDictionary<Object, EventBean[]> keysAndEvents = new Dictionary<Object, EventBean[]>();
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
    
            // Update aggregates
            if (newEvents.IsNotEmpty())
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
            var keysAndEvents = new Dictionary<Object, EventBean>();
    
            var newDataMultiKey = GenerateGroupKeys(newData, keysAndEvents, true);
            var oldDataMultiKey = GenerateGroupKeys(oldData, keysAndEvents, false);
    
            EventBean[] selectOldEvents = null;
            if (_prototype.IsSelectRStream)
            {
                selectOldEvents = GenerateOutputEventsView(keysAndEvents, false, isSynthesize);
            }
    
            // Update aggregates
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
    
        protected EventBean[] GenerateOutputEventsView(IDictionary<Object, EventBean> keysAndEvents, bool isNewData, bool isSynthesize)
        {
            var eventsPerStream = new EventBean[1];
            var events = new EventBean[keysAndEvents.Count];
            var keys = new Object[keysAndEvents.Count];
            EventBean[][] currentGenerators = null;
            if(_prototype.IsSorting)
            {
                currentGenerators = new EventBean[keysAndEvents.Count][];
            }
    
            var count = 0;
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
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
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
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
                    var outKeys = new Object[count];
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
    
        private void GenerateOutputBatched(IDictionary<Object, EventBean> keysAndEvents, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<object> optSortKeys, AgentInstanceContext agentInstanceContext)
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
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
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

        private void GenerateOutputBatchedArr(bool join, IDictionary<Object, EventBean[]> keysAndEvents, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<object> optSortKeys)
        {
            foreach (var entry in keysAndEvents)
            {
                GenerateOutputBatched(join, entry.Key, entry.Value, isNewData, isSynthesize, resultEvents, optSortKeys);
            }
        }
    
        private void GenerateOutputBatched(bool join, Object mk, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<object> optSortKeys)
        {
            // Set the current row of aggregation states
            _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, null);
    
            // Filter the having clause
            if (_prototype.OptionalHavingNode != null)
            {
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
                var result = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean());}
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
    
        private EventBean[] GenerateOutputEventsJoin(IDictionary<Object, EventBean[]> keysAndEvents, bool isNewData, bool isSynthesize)
        {
            var events = new EventBean[keysAndEvents.Count];
            var keys = new Object[keysAndEvents.Count];
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
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean());}
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
                    var outKeys = new Object[count];
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
    
        private Object[] GenerateGroupKeys(EventBean[] events, bool isNewData)
        {
            if (events == null)
            {
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
    
        protected Object[] GenerateGroupKeys(EventBean[] events, IDictionary<Object, EventBean> eventPerKey, bool isNewData)
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
                eventPerKey.Put(keys[i], events[i]);
            }
    
            return keys;
        }
    
        private Object[] GenerateGroupKeys(ISet<MultiKey<EventBean>> resultSet, IDictionary<Object, EventBean[]> eventPerKey, bool isNewData)
        {
            if (resultSet == null || resultSet.IsEmpty())
            {
                return null;
            }
    
            var keys = new Object[resultSet.Count];
    
            var count = 0;
            foreach (var eventsPerStream in resultSet)
            {
                keys[count] = GenerateGroupKey(eventsPerStream.Array, isNewData);
                eventPerKey.Put(keys[count], eventsPerStream.Array);
    
                count++;
            }
    
            return keys;
        }

        /// <summary>Returns the optional having expression. </summary>
        /// <value>having expression node</value>
        public ExprEvaluator OptionalHavingNode
        {
            get { return _prototype.OptionalHavingNode; }
        }

        /// <summary>Returns the select expression processor </summary>
        /// <returns>select processor.</returns>
        public SelectExprProcessor GetSelectExprProcessor()
        {
            return _selectExprProcessor;
        }

        public virtual IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (!Prototype.IsHistoricalOnly)
            {
                return ObtainIterator(parent);
            }

            AggregationService.ClearResults(AgentInstanceContext);
            var it = parent.GetEnumerator();
            EventBean[] eventsPerStream = new EventBean[1];
            for (; it.MoveNext(); )
            {
                eventsPerStream[0] = it.Current;
                Object groupKey = GenerateGroupKey(eventsPerStream, true);
                AggregationService.ApplyEnter(eventsPerStream, groupKey, AgentInstanceContext);
            }

            ArrayDeque<EventBean> deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainIterator(parent));
            AggregationService.ClearResults(AgentInstanceContext);
            return deque.GetEnumerator();
        }

        public virtual IEnumerator<EventBean> ObtainIterator(Viewable parent)
        {
            var parentEnum = parent.GetEnumerator();
            if (_orderByProcessor == null)
            {
                return ResultSetRowPerGroupEnumerator.New(parentEnum, this, _aggregationService, _agentInstanceContext);
            }
            return GetEnumeratorSorted(parentEnum);
        }

        protected IEnumerator<EventBean> GetEnumeratorSorted(IEnumerator<EventBean> parentIter)
        {
    
            // Pull all parent events, generate order keys
            var eventsPerStream = new EventBean[1];
            var outgoingEvents = new List<EventBean>();
            var orderKeys = new List<Object>();
            ISet<Object> priorSeenGroups = new HashSet<Object>();

            var evaluateParams = new EvaluateParams(eventsPerStream, true, _agentInstanceContext);
            for (; parentIter.MoveNext(); )
            {
                EventBean candidate = parentIter.Current;
                eventsPerStream[0] = candidate;
    
                var groupKey = GenerateGroupKey(eventsPerStream, true);
                _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, null);
    
                if (_prototype.OptionalHavingNode != null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(candidate);}
                    var pass = _prototype.OptionalHavingNode.Evaluate(evaluateParams);
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(pass.AsBoxedBoolean());}
                    if ((pass == null) || (false.Equals(pass)))
                    {
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
            var orderedEvents = _orderByProcessor.Sort(outgoingEventsArr, orderKeysArr, _agentInstanceContext);

            if (orderedEvents == null)
                return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
            return ((IEnumerable<EventBean>)orderedEvents).GetEnumerator();
        }

        public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            IDictionary<Object, EventBean[]> keysAndEvents = new Dictionary<Object, EventBean[]>();
            GenerateGroupKeys(joinSet, keysAndEvents, true);
            var selectNewEvents = GenerateOutputEventsJoin(keysAndEvents, true, true);
            if (selectNewEvents == null)
                return EnumerationHelper<EventBean>.CreateEmptyEnumerator();
            return ((IEnumerable<EventBean>) selectNewEvents).GetEnumerator();
        }
    
        public void Clear()
        {
            _aggregationService.ClearResults(_agentInstanceContext);
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT)
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

                IDictionary<Object, EventBean[]> keysAndEvents = new Dictionary<Object, EventBean[]>();
    
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
                        GenerateOutputBatchedArr(true, keysAndEvents, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                    GenerateOutputBatchedArr(true, keysAndEvents, true, generateSynthetic, newEvents, newEventsSortKey);
    
                    keysAndEvents.Clear();
                }
    
                EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
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
            else if (outputLimitLimitType == OutputLimitLimitType.ALL)
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
    
                if (_prototype.IsSelectRStream)
                {
                    GenerateOutputBatchedArr(true, _groupRepsView, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
                            if (_groupRepsView.Push(mk, aNewData.Array) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                            if (_groupRepsView.Push(mk, anOldData.Array) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                }
                            }
    
                            _aggregationService.ApplyLeave(anOldData.Array, mk, _agentInstanceContext);
                        }
                    }
                }
    
                GenerateOutputBatchedArr(true, _groupRepsView, true, generateSynthetic, newEvents, newEventsSortKey);
    
                EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
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
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {

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
    
                _groupRepsView.Clear();
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
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                                if (pass) {
                                    // if this is a newly encountered group, generate the remove stream event
                                    if (_groupRepsView.Push(mk, aNewData.Array) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    if (_groupRepsView.Push(mk, anOldData.Array) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                        }
                                    }
                                }
    
                                _aggregationService.ApplyLeave(anOldData.Array, mk, _agentInstanceContext);
                            }
                        }
                    }
                }
                else {
                    _groupRepsView.Clear();
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
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean());}
                                if ((result == null) || (false.Equals(result)))
                                {
                                    count++;
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                                if (pass) {
                                    if (_groupRepsView.Push(mk, aNewData.Array) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean());}
                                if ((result == null) || (false.Equals(result)))
                                {
                                    count++;
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    if (_groupRepsView.Push(mk, anOldData.Array) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                        }
                                    }
                                }
                                count++;
                            }
                        }
                    }
                }
    
                GenerateOutputBatchedArr(true, _groupRepsView, true, generateSynthetic, newEvents, newEventsSortKey);
    
                EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
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
            else // (outputLimitLimitType == OutputLimitLimitType.LAST)
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
    
                _groupRepsView.Clear();
                foreach (var pair in joinEventsSet)
                {
                    ISet<MultiKey<EventBean>> newData = pair.First;
                    ISet<MultiKey<EventBean>> oldData = pair.Second;
    
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
                            if (_groupRepsView.Push(mk, aNewData.Array) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(true, mk, aNewData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                            if (_groupRepsView.Push(mk, anOldData.Array) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(true, mk, anOldData.Array, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                }
                            }
    
                            _aggregationService.ApplyLeave(anOldData.Array, mk, _agentInstanceContext);
                        }
                    }
                }
    
                GenerateOutputBatchedArr(true, _groupRepsView, true, generateSynthetic, newEvents, newEventsSortKey);
    
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
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT)
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

                IDictionary<Object, EventBean> keysAndEvents = new Dictionary<Object, EventBean>();
    
                foreach (var pair in viewEventsList)
                {
                    var newData = pair.First;
                    var oldData = pair.Second;
    
                    var newDataMultiKey = GenerateGroupKeys(newData, keysAndEvents, true);
                    var oldDataMultiKey = GenerateGroupKeys(oldData, keysAndEvents, false);
    
                    if (_prototype.IsSelectRStream)
                    {
                        GenerateOutputBatched(keysAndEvents, false, generateSynthetic, oldEvents, oldEventsSortKey, _agentInstanceContext);
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
    
                    GenerateOutputBatched(keysAndEvents, true, generateSynthetic, newEvents, newEventsSortKey, _agentInstanceContext);
    
                    keysAndEvents.Clear();
                }
    
                EventBean[] newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
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
            else if (outputLimitLimitType == OutputLimitLimitType.ALL)
            {
                var eventsPerStream = new EventBean[1];

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
    
                if (_prototype.IsSelectRStream)
                {
                    GenerateOutputBatchedArr(false, _groupRepsView, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
                            if (_groupRepsView.Push(mk, new EventBean[] {aNewData}) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                            if (_groupRepsView.Push(mk, new EventBean[] {anOldData}) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                }
                            }
    
                            _aggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
                        }
                    }
                }
    
                GenerateOutputBatchedArr(false, _groupRepsView, true, generateSynthetic, newEvents, newEventsSortKey);
    
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
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST)
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
    
                if (_prototype.OptionalHavingNode == null) {
    
                    _groupRepsView.Clear();
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
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                                if (pass) {
                                    // if this is a newly encountered group, generate the remove stream event
                                    if (_groupRepsView.Push(mk, eventsPerStream) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    if (_groupRepsView.Push(mk, eventsPerStream) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
                    _groupRepsView.Clear();
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
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
                                if ((result == null) || (false.Equals(result)))
                                {
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException e) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    var eventsPerStream = new EventBean[] {newData[i]};
                                    if (_groupRepsView.Push(mk, eventsPerStream) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(false, mk, eventsPerStream, true, generateSynthetic, oldEvents, oldEventsSortKey);
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
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
                                if ((result == null) || (false.Equals(result)))
                                {
                                    continue;
                                }
    
                                var outputStateGroup = _outputState.Get(mk);
                                if (outputStateGroup == null) {
                                    try {
                                        outputStateGroup = OutputConditionPolledFactory.CreateCondition(_prototype.OutputLimitSpec, _agentInstanceContext);
                                    }
                                    catch (ExprValidationException) {
                                        Log.Error("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "'");
                                    }
                                    _outputState.Put(mk, outputStateGroup);
                                }
                                var pass = outputStateGroup.UpdateOutputCondition(0, 1);
                                if (pass) {
                                    var eventsPerStream = new EventBean[] {oldData[i]};
                                    if (_groupRepsView.Push(mk, eventsPerStream) == null)
                                    {
                                        if (_prototype.IsSelectRStream)
                                        {
                                            GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
    
                GenerateOutputBatchedArr(false, _groupRepsView, true, generateSynthetic, newEvents, newEventsSortKey);
    
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
            else // (outputLimitLimitType == OutputLimitLimitType.LAST)
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
    
                _groupRepsView.Clear();
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
                            if (_groupRepsView.Push(mk, eventsPerStream) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
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
    
                            if (_groupRepsView.Push(mk, eventsPerStream) == null)
                            {
                                if (_prototype.IsSelectRStream)
                                {
                                    GenerateOutputBatched(false, mk, eventsPerStream, false, generateSynthetic, oldEvents, oldEventsSortKey);
                                }
                            }
    
                            _aggregationService.ApplyLeave(eventsPerStream, mk, _agentInstanceContext);
                        }
                    }
                }
    
                GenerateOutputBatchedArr(false, _groupRepsView, true, generateSynthetic, newEvents, newEventsSortKey);
    
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
        }
    
        private Object[] GenerateGroupKeys(ISet<MultiKey<EventBean>> resultSet, bool isNewData)
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

        public bool HasAggregation
        {
            get { return true; }
        }

        public virtual void Removed(Object key)
        {
            _groupRepsView.Remove(key);
            _outputState.Remove(key);
        }
    
        protected internal object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData)
        {
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QResultSetProcessComputeGroupKeys(isNewData, _prototype.GroupKeyNodeExpressions, eventsPerStream);
                Object keyObject;
                if (_prototype.GroupKeyNode != null)
                {
                    keyObject = _prototype.GroupKeyNode.Evaluate(evaluateParams);
                }
                else
                {
                    var evals = _prototype.GroupKeyNodes;
                    var keys = new Object[evals.Length];
                    for (var i = 0; i < evals.Length; i++)
                    {
                        keys[i] = evals[i].Evaluate(evaluateParams);
                    }
                    keyObject = new MultiKeyUntyped(keys);
                }
    
                InstrumentationHelper.Get().AResultSetProcessComputeGroupKeys(isNewData, keyObject);
                return keyObject;
            }
    
            if (_prototype.GroupKeyNode != null)
            {
                return _prototype.GroupKeyNode.Evaluate(evaluateParams);
            }
            else
            {
                var evals = _prototype.GroupKeyNodes;
                var keys = new Object[evals.Length];
                for (var i = 0; i < evals.Length; i++)
                {
                    keys[i] = evals[i].Evaluate(evaluateParams);
                }
                return new MultiKeyUntyped(keys);
            }
        }
    }
}

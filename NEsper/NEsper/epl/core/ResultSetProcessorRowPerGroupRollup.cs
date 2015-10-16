///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.view;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
    public class ResultSetProcessorRowPerGroupRollup : ResultSetProcessor, AggregationRowRemovedCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        internal readonly ResultSetProcessorRowPerGroupRollupFactory Prototype;
        internal readonly OrderByProcessor OrderByProcessor;
        internal readonly AggregationService AggregationService;
        private AgentInstanceContext _agentInstanceContext;
    
        // For output rate limiting, as temporary buffer of to keep a representative event for each group
        internal readonly IDictionary<Object, EventBean[]>[] OutputLimitGroupRepsPerLevel;
    
        private readonly IDictionary<Object, OutputConditionPolled>[] _outputState;
    
        protected readonly IDictionary<Object, EventBean>[] EventPerGroupBuf;
        protected readonly IDictionary<Object, EventBean[]>[] EventPerGroupJoinBuf;
    
        private readonly EventArrayAndSortKeyArray _rstreamEventSortArrayPair;
        private readonly ResultSetProcessorRowPerGroupRollupOutputLastHelper _outputLastHelper;
        private readonly ResultSetProcessorRowPerGroupRollupOutputAllHelper _outputAllHelper;
    
        public ResultSetProcessorRowPerGroupRollup(ResultSetProcessorRowPerGroupRollupFactory prototype, OrderByProcessor orderByProcessor, AggregationService aggregationService, AgentInstanceContext agentInstanceContext)
        {
            Prototype = prototype;
            OrderByProcessor = orderByProcessor;
            AggregationService = aggregationService;
            _agentInstanceContext = agentInstanceContext;
            aggregationService.SetRemovedCallback(this);
    
            var levelCount = prototype.GroupByRollupDesc.Levels.Length;
    
            if (prototype.IsJoin) {
                EventPerGroupJoinBuf = new IDictionary<Object, EventBean[]>[levelCount];
                for (var i = 0; i < levelCount; i++) {
                    EventPerGroupJoinBuf[i] = new LinkedHashMap<Object, EventBean[]>();
                }
                EventPerGroupBuf = null;
            }
            else {
                EventPerGroupBuf = new IDictionary<Object, EventBean>[levelCount];
                for (var i = 0; i < levelCount; i++) {
                    EventPerGroupBuf[i] = new LinkedHashMap<Object, EventBean>();
                }
                EventPerGroupJoinBuf = null;
            }

            if (prototype.OutputLimitSpec != null)
            {
                OutputLimitGroupRepsPerLevel = new IDictionary<Object, EventBean[]>[levelCount];
                for (var i = 0; i < levelCount; i++)
                {
                    OutputLimitGroupRepsPerLevel[i] = new LinkedHashMap<Object, EventBean[]>();
                }

                if (prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST)
                {
                    _outputLastHelper = new ResultSetProcessorRowPerGroupRollupOutputLastHelper(
                        this, OutputLimitGroupRepsPerLevel.Length);
                    _outputAllHelper = null;
                }
                else if (prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL)
                {
                    _outputAllHelper = new ResultSetProcessorRowPerGroupRollupOutputAllHelper(
                        this, OutputLimitGroupRepsPerLevel.Length);
                    _outputLastHelper = null;
                }
                else
                {
                    _outputLastHelper = null;
                    _outputAllHelper = null;
                }
            }
            else
            {
                OutputLimitGroupRepsPerLevel = null;
                _outputLastHelper = null;
                _outputAllHelper = null;
            }

            // Allocate output state for output-first
            if (prototype.OutputLimitSpec != null && prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST) {
                _outputState = new IDictionary<Object, OutputConditionPolled>[levelCount];
                for (var i = 0; i < levelCount; i++) {
                    _outputState[i] = new HashMap<Object, OutputConditionPolled>();
                }
            }
            else {
                _outputState = null;
            }
    
            if (prototype.OutputLimitSpec != null && (prototype.IsSelectRStream || prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST)) {
                var eventsPerLevel = new List<EventBean>[prototype.GroupByRollupDesc.Levels.Length];
                List<Object>[] sortKeyPerLevel = null;
                if (orderByProcessor != null) {
                    sortKeyPerLevel = new List<Object>[prototype.GroupByRollupDesc.Levels.Length];
                }
                foreach (var level in prototype.GroupByRollupDesc.Levels) {
                    eventsPerLevel[level.LevelNumber] = new List<EventBean>();
                    if (orderByProcessor != null) {
                        sortKeyPerLevel[level.LevelNumber] = new List<Object>();
                    }
                }
                _rstreamEventSortArrayPair = new EventArrayAndSortKeyArray(eventsPerLevel, sortKeyPerLevel);
            }
            else {
                _rstreamEventSortArrayPair = null;
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

        public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}
    
            if (Prototype.IsUnidirectional) {
                Clear();
            }
    
            ResetEventPerGroupJoinBuf();
            var newDataMultiKey = GenerateGroupKeysJoin(newEvents, EventPerGroupJoinBuf, true);
            var oldDataMultiKey = GenerateGroupKeysJoin(oldEvents, EventPerGroupJoinBuf, false);
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream) {
                selectOldEvents = GenerateOutputEventsJoin(EventPerGroupJoinBuf, false, isSynthesize);
            }
    
            // update aggregates
            if (newEvents != null) {
                var count = 0;
                foreach (var mk in newEvents) {
                    AggregationService.ApplyEnter(mk.Array, newDataMultiKey[count++], _agentInstanceContext);
                }
            }
            if (oldEvents != null) {
                var count = 0;
                foreach (var mk in oldEvents) {
                    AggregationService.ApplyLeave(mk.Array, oldDataMultiKey[count++], _agentInstanceContext);
                }
            }
    
            // generate new events using select expressions
            var selectNewEvents = GenerateOutputEventsJoin(EventPerGroupJoinBuf, true, isSynthesize);
    
            if ((selectNewEvents != null) || (selectOldEvents != null)) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
            return null;
        }

        public virtual UniformPair<EventBean[]> ProcessViewResult(EventBean[] newData, EventBean[] oldData, bool isSynthesize)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}
    
            ResetEventPerGroupBuf();
            var newDataMultiKey = GenerateGroupKeysView(newData, EventPerGroupBuf, true);
            var oldDataMultiKey = GenerateGroupKeysView(oldData, EventPerGroupBuf, false);
    
            EventBean[] selectOldEvents = null;
            if (Prototype.IsSelectRStream) {
                selectOldEvents = GenerateOutputEventsView(EventPerGroupBuf, false, isSynthesize);
            }
    
            // update aggregates
            var eventsPerStream = new EventBean[1];
            if (newData != null) {
                for (var i = 0; i < newData.Length; i++) {
                    eventsPerStream[0] = newData[i];
                    AggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], _agentInstanceContext);
                }
            }
            if (oldData != null) {
                for (var i = 0; i < oldData.Length; i++) {
                    eventsPerStream[0] = oldData[i];
                    AggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], _agentInstanceContext);
                }
            }
    
            // generate new events using select expressions
            var selectNewEvents = GenerateOutputEventsView(EventPerGroupBuf, true, isSynthesize);
    
            if ((selectNewEvents != null) || (selectOldEvents != null)) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
                return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
            }
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
            return null;
        }
    
        protected EventBean[] GenerateOutputEventsView(IDictionary<Object, EventBean>[] keysAndEvents, bool isNewData, bool isSynthesize)
        {
            var eventsPerStream = new EventBean[1];
            var events = new List<EventBean>(1);
            List<GroupByRollupKey> currentGenerators = null;
            if(Prototype.IsSorting) {
                currentGenerators = new List<GroupByRollupKey>(4);
            }
    
            var levels = Prototype.GroupByRollupDesc.Levels;
            var selectExprProcessors = Prototype.PerLevelExpression.SelectExprProcessor;
            var optionalHavingClauses = Prototype.PerLevelExpression.OptionalHavingNodes;
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);

            foreach (var level in levels)
            {
                foreach (var entry in keysAndEvents[level.LevelNumber]) {
                    var groupKey = entry.Key;
    
                    // Set the current row of aggregation states
                    AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
                    eventsPerStream[0] = entry.Value;
    
                    // Filter the having clause
                    if (optionalHavingClauses != null)
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(entry.Value);}
                        var result = optionalHavingClauses[level.LevelNumber].Evaluate(evaluateParams);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
                        if ((result == null) || false.Equals(result)) {
                            continue;
                        }
                    }
                    events.Add(selectExprProcessors[level.LevelNumber].Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
    
                    if(Prototype.IsSorting) {
                        var currentEventsPerStream = new EventBean[] { entry.Value };
                        currentGenerators.Add(new GroupByRollupKey(currentEventsPerStream, level, groupKey));
                    }
                }
            }
    
            if (events.IsEmpty()) {
                return null;
            }
            var outgoing = events.ToArray();
            if (outgoing.Length > 1 && Prototype.IsSorting) {
                return OrderByProcessor.Sort(outgoing, currentGenerators, isNewData, _agentInstanceContext, Prototype.PerLevelExpression.OptionalOrderByElements);
            }
            return outgoing;
        }
    
        private EventBean[] GenerateOutputEventsJoin(IDictionary<Object, EventBean[]>[] eventPairs, bool isNewData, bool synthesize) {
            var events = new List<EventBean>(1);
            List<GroupByRollupKey> currentGenerators = null;
            if(Prototype.IsSorting) {
                currentGenerators = new List<GroupByRollupKey>(4);
            }
    
            var levels = Prototype.GroupByRollupDesc.Levels;
            var selectExprProcessors = Prototype.PerLevelExpression.SelectExprProcessor;
            var optionalHavingClauses = Prototype.PerLevelExpression.OptionalHavingNodes;
            foreach (var level in levels) {
                foreach (var entry in eventPairs[level.LevelNumber]) {
                    var groupKey = entry.Key;
    
                    // Set the current row of aggregation states
                    AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
    
                    // Filter the having clause
                    if (optionalHavingClauses != null)
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(entry.Value);}
                        var evaluateParams = new EvaluateParams(entry.Value, isNewData, _agentInstanceContext);
                        var result = optionalHavingClauses[level.LevelNumber].Evaluate(evaluateParams);
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
                        if ((result == null) || false.Equals(result)) {
                            continue;
                        }
                    }
                    events.Add(selectExprProcessors[level.LevelNumber].Process(entry.Value, isNewData, synthesize, _agentInstanceContext));
    
                    if(Prototype.IsSorting) {
                        currentGenerators.Add(new GroupByRollupKey(entry.Value, level, groupKey));
                    }
                }
            }
    
            if (events.IsEmpty()) {
                return null;
            }
            var outgoing = events.ToArray();
            if (outgoing.Length > 1 && Prototype.IsSorting) {
                return OrderByProcessor.Sort(outgoing, currentGenerators, isNewData, _agentInstanceContext, Prototype.PerLevelExpression.OptionalOrderByElements);
            }
            return outgoing;
        }

        public virtual IEnumerator<EventBean> GetEnumerator(Viewable parent)
        {
            if (!Prototype.IsHistoricalOnly)
            {
                return ObtainEnumerator(parent);
            }

            AggregationService.ClearResults(AgentInstanceContext);
            var it = parent.GetEnumerator();
            var eventsPerStream = new EventBean[1];
            var groupKeys = new Object[Prototype.GroupByRollupDesc.Levels.Length];
            var levels = Prototype.GroupByRollupDesc.Levels;
            while ( it.MoveNext() )
            {
                eventsPerStream[0] = it.Current;
                var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
                for (int j = 0; j < levels.Length; j++)
                {
                    var subkey = levels[j].ComputeSubkey(groupKeyComplete);
                    groupKeys[j] = subkey;
                }
                AggregationService.ApplyEnter(eventsPerStream, groupKeys, AgentInstanceContext);
            }

            ArrayDeque<EventBean> deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainEnumerator(parent));
            AggregationService.ClearResults(AgentInstanceContext);
            return deque.GetEnumerator();
        }

        public virtual IEnumerator<EventBean> ObtainEnumerator(Viewable parent)
        {
            ResetEventPerGroupBuf();
            EventBean[] events = EPAssertionUtil.EnumeratorToArray(parent.GetEnumerator());
            GenerateGroupKeysView(events, EventPerGroupBuf, true);
            var output = GenerateOutputEventsView(EventPerGroupBuf, true, true);
            return ((IEnumerable<EventBean>) output).GetEnumerator();
        }

        public virtual IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
        {
            ResetEventPerGroupJoinBuf();
            GenerateGroupKeysJoin(joinSet, EventPerGroupJoinBuf, true);
            var output = GenerateOutputEventsJoin(EventPerGroupJoinBuf, true, true);
            return ((IEnumerable<EventBean>) output).GetEnumerator();
        }
    
        public void Clear()
        {
            AggregationService.ClearResults(_agentInstanceContext);
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                return HandleOutputLimitDefaultJoin(joinEventsSet, generateSynthetic);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                return HandleOutputLimitAllJoin(joinEventsSet, generateSynthetic);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                return HandleOutputLimitFirstJoin(joinEventsSet, generateSynthetic);
            }
            // (outputLimitLimitType == OutputLimitLimitType.LAST) {
            return HandleOutputLimitLastJoin(joinEventsSet, generateSynthetic);
        }
    
        public UniformPair<EventBean[]> ProcessOutputLimitedView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, OutputLimitLimitType outputLimitLimitType)
        {
            if (outputLimitLimitType == OutputLimitLimitType.DEFAULT) {
                return HandleOutputLimitDefaultView(viewEventsList, generateSynthetic);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.ALL) {
                return HandleOutputLimitAllView(viewEventsList, generateSynthetic);
            }
            else if (outputLimitLimitType == OutputLimitLimitType.FIRST) {
                return HandleOutputLimitFirstView(viewEventsList, generateSynthetic);
            }
            // (outputLimitLimitType == OutputLimitLimitType.LAST) {
            return HandleOutputLimitLastView(viewEventsList, generateSynthetic);
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitFirstView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
            foreach (var aGroupRepsView in OutputLimitGroupRepsPerLevel) {
                aGroupRepsView.Clear();
            }
    
            _rstreamEventSortArrayPair.Reset();
    
            int oldEventCount;
            if (Prototype.PerLevelExpression.OptionalHavingNodes == null) {
                oldEventCount = HandleOutputLimitFirstViewNoHaving(viewEventsList, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
            }
            else {
                oldEventCount = HandleOutputLimitFirstViewHaving(viewEventsList, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
            }
    
            return GenerateAndSort(OutputLimitGroupRepsPerLevel, generateSynthetic, oldEventCount);
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitFirstJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
            foreach (var aGroupRepsView in OutputLimitGroupRepsPerLevel) {
                aGroupRepsView.Clear();
            }
    
            _rstreamEventSortArrayPair.Reset();
    
            int oldEventCount;
            if (Prototype.PerLevelExpression.OptionalHavingNodes == null) {
                oldEventCount = HandleOutputLimitFirstJoinNoHaving(joinEventsSet, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
            }
            else {
                oldEventCount = HandleOutputLimitFirstJoinHaving(joinEventsSet, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
            }
    
            return GenerateAndSort(OutputLimitGroupRepsPerLevel, generateSynthetic, oldEventCount);
        }
    
        private int HandleOutputLimitFirstViewHaving(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, IList<EventBean>[] oldEventsPerLevel, IList<Object>[] oldEventsSortKeyPerLevel)
        {
            var oldEventCount = 0;
    
            var havingPerLevel = Prototype.PerLevelExpression.OptionalHavingNodes;
    
            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                EventBean[] eventsPerStream;
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        eventsPerStream = new EventBean[] {aNewData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                        }
                        AggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        eventsPerStream = new EventBean[] {anOldData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                        }
                        AggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
    
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        eventsPerStream = new EventBean[] {aNewData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
                        var evaluateParams = new EvaluateParams(eventsPerStream, true, _agentInstanceContext);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels)
                        {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(aNewData);}
                            var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
                            if ((result == null) || false.Equals(result)) {
                                continue;
                            }
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        eventsPerStream = new EventBean[] {anOldData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
                        var evaluateParams = new EvaluateParams(eventsPerStream, false, _agentInstanceContext);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels)
                        {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
    
                            AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(anOldData);}
                            var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
                            if ((result == null) || false.Equals(result)) {
                                continue;
                            }
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, eventsPerStream, false, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return oldEventCount;
        }
    
        private int HandleOutputLimitFirstJoinNoHaving(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventSet, bool generateSynthetic, IList<EventBean>[] oldEventsPerLevel, IList<Object>[] oldEventsSortKeyPerLevel)
        {
            var oldEventCount = 0;
    
            // outer loop is the events
            foreach (var pair in joinEventSet)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                EventBean[] eventsPerStream;
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aNewData.Array) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                        AggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, anOldData.Array) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, anOldData.Array, false, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                        AggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
            }
            return oldEventCount;
        }
    
        private int HandleOutputLimitFirstJoinHaving(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventSet, bool generateSynthetic, IList<EventBean>[] oldEventsPerLevel, IList<Object>[] oldEventsSortKeyPerLevel)
        {
            var oldEventCount = 0;
    
            var havingPerLevel = Prototype.PerLevelExpression.OptionalHavingNodes;
    
            foreach (var pair in joinEventSet)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                EventBean[] eventsPerStream;
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                        }
                        AggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                        }
                        AggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
    
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
                        var evaluateParams = new EvaluateParams(aNewData.Array, true, _agentInstanceContext);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels)
                        {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aNewData.Array);}
                            var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
                            if ((result == null) || false.Equals(result)) {
                                continue;
                            }
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aNewData.Array) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
                        var evaluateParams = new EvaluateParams(anOldData.Array, false, _agentInstanceContext);

                        foreach (var level in Prototype.GroupByRollupDesc.Levels)
                        {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            AggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(anOldData.Array);}
                            var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
                            if ((result == null) || false.Equals(result)) {
                                continue;
                            }
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, anOldData.Array) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, anOldData.Array, false, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return oldEventCount;
        }

        private int HandleOutputLimitFirstViewNoHaving(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic, IList<EventBean>[] oldEventsPerLevel, IList<Object>[] oldEventsSortKeyPerLevel)
        {
            var oldEventCount = 0;
    
            // outer loop is the events
            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                EventBean[] eventsPerStream;
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        eventsPerStream = new EventBean[] {aNewData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                        AggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        eventsPerStream = new EventBean[] {anOldData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
    
                            var outputStateGroup = _outputState[level.LevelNumber].Get(groupKey);
                            if (outputStateGroup == null) {
                                try {
                                    outputStateGroup = OutputConditionPolledFactory.CreateCondition(Prototype.OutputLimitSpec, _agentInstanceContext);
                                }
                                catch (ExprValidationException e) {
                                    throw HandleConditionValidationException(e);
                                }
                                _outputState[level.LevelNumber].Put(groupKey, outputStateGroup);
                            }
                            var pass = outputStateGroup.UpdateOutputCondition(1, 0);
                            if (pass) {
                                if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                                    if (Prototype.IsSelectRStream) {
                                        GenerateOutputBatched(false, groupKey, level, eventsPerStream, false, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
                                        oldEventCount++;
                                    }
                                }
                            }
                        }
                        AggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
            }
            return oldEventCount;
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitDefaultView(IEnumerable<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
            var newEvents = new List<EventBean>();
            List<Object> newEventsSortKey = null;
            if (OrderByProcessor != null) {
                newEventsSortKey = new List<Object>();
            }
    
            List<EventBean> oldEvents = null;
            List<Object> oldEventsSortKey = null;
            if (Prototype.IsSelectRStream) {
                oldEvents = new List<EventBean>();
                if (OrderByProcessor != null) {
                    oldEventsSortKey = new List<Object>();
                }
            }
    
            foreach (var pair in viewEventsList) {
                var newData = pair.First;
                var oldData = pair.Second;
    
                ResetEventPerGroupBuf();
                var newDataMultiKey = GenerateGroupKeysView(newData, EventPerGroupBuf, true);
                var oldDataMultiKey = GenerateGroupKeysView(oldData, EventPerGroupBuf, false);
    
                if (Prototype.IsSelectRStream) {
                    GenerateOutputBatchedCollectNonJoin(EventPerGroupBuf, false, generateSynthetic, oldEvents, oldEventsSortKey);
                }
    
                // update aggregates
                var eventsPerStream = new EventBean[1];
                if (newData != null) {
                    for (var i = 0; i < newData.Length; i++) {
                        eventsPerStream[0] = newData[i];
                        AggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    for (var i = 0; i < oldData.Length; i++) {
                        eventsPerStream[0] = oldData[i];
                        AggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], _agentInstanceContext);
                    }
                }
    
                GenerateOutputBatchedCollectNonJoin(EventPerGroupBuf, true, generateSynthetic, newEvents, newEventsSortKey);
            }
    
            return ConvertToArrayMaySort(newEvents, newEventsSortKey, oldEvents, oldEventsSortKey);
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitDefaultJoin(IEnumerable<UniformPair<ISet<MultiKey<EventBean>>>> viewEventsList, bool generateSynthetic)
        {
            var newEvents = new List<EventBean>();
            List<Object> newEventsSortKey = null;
            if (OrderByProcessor != null) {
                newEventsSortKey = new List<Object>();
            }
    
            List<EventBean> oldEvents = null;
            List<Object> oldEventsSortKey = null;
            if (Prototype.IsSelectRStream) {
                oldEvents = new List<EventBean>();
                if (OrderByProcessor != null) {
                    oldEventsSortKey = new List<Object>();
                }
            }
    
            foreach (var pair in viewEventsList) {
                var newData = pair.First;
                var oldData = pair.Second;
    
                ResetEventPerGroupJoinBuf();
                var newDataMultiKey = GenerateGroupKeysJoin(newData, EventPerGroupJoinBuf, true);
                var oldDataMultiKey = GenerateGroupKeysJoin(oldData, EventPerGroupJoinBuf, false);
    
                if (Prototype.IsSelectRStream) {
                    GenerateOutputBatchedCollectJoin(EventPerGroupJoinBuf, false, generateSynthetic, oldEvents, oldEventsSortKey);
                }
    
                // update aggregates
                if (newData != null) {
                    var count = 0;
                    foreach (var newEvent in newData) {
                        AggregationService.ApplyEnter(newEvent.Array, newDataMultiKey[count++], _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    var count = 0;
                    foreach (var oldEvent in oldData) {
                        AggregationService.ApplyLeave(oldEvent.Array, oldDataMultiKey[count++], _agentInstanceContext);
                    }
                }
    
                GenerateOutputBatchedCollectJoin(EventPerGroupJoinBuf, true, generateSynthetic, newEvents, newEventsSortKey);
            }
    
            return ConvertToArrayMaySort(newEvents, newEventsSortKey, oldEvents, oldEventsSortKey);
        }

        public virtual bool HasAggregation
        {
            get { return true; }
        }

        public virtual void Removed(Object key)
        {
            throw new NotSupportedException();
        }
    
        internal Object GenerateGroupKey(EventBean[] eventsPerStream, bool isNewData)
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
                    var evals = Prototype.GroupKeyNodes;
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
    
            if (Prototype.GroupKeyNode != null)
            {
                return Prototype.GroupKeyNode.Evaluate(evaluateParams);
            }
            else {
                var evals = Prototype.GroupKeyNodes;
                var keys = new Object[evals.Length];
                for (var i = 0; i < evals.Length; i++)
                {
                    keys[i] = evals[i].Evaluate(evaluateParams);
                }
                return new MultiKeyUntyped(keys);
            }
        }
    
        private void GenerateOutputBatched(bool join, Object mk, AggregationGroupByRollupLevel level, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, IList<IList<EventBean>> resultEvents, IList<IList<object>> optSortKeys)
        {
            var resultList = resultEvents[level.LevelNumber];
            var sortKeys = optSortKeys == null ? null : optSortKeys[level.LevelNumber];
            GenerateOutputBatched(join, mk, level, eventsPerStream, isNewData, isSynthesize, resultList, sortKeys);
        }
    
        internal void GenerateOutputBatched(bool join, Object mk, AggregationGroupByRollupLevel level, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, ICollection<EventBean> resultEvents, ICollection<object> optSortKeys)
        {
            AggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, level);
    
            if (Prototype.PerLevelExpression.OptionalHavingNodes != null)
            {
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
                var result = Prototype.PerLevelExpression.OptionalHavingNodes[level.LevelNumber].Evaluate(new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext));
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
                if ((result == null) || false.Equals(result)) {
                    return;
                }
            }
    
            resultEvents.Add(Prototype.PerLevelExpression.SelectExprProcessor[level.LevelNumber].Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
    
            if (Prototype.IsSorting) {
                optSortKeys.Add(OrderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext, Prototype.PerLevelExpression.OptionalOrderByElements[level.LevelNumber]));
            }
        }

        internal void GenerateOutputBatchedMapUnsorted(bool join, Object mk, AggregationGroupByRollupLevel level, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, IDictionary<Object, EventBean> resultEvents)
        {
            AggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, level);

            if (Prototype.PerLevelExpression.OptionalHavingNodes!= null)
            {
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream); }
                var result = Prototype.PerLevelExpression.OptionalHavingNodes[level.LevelNumber].Evaluate(new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext)).AsBoxedBoolean();
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result); else InstrumentationHelper.Get().AHavingClauseJoin(result); }
                if ((result == null) || (false.Equals(result)))
                {
                    return;
                }
            }

            resultEvents[mk] = Prototype.PerLevelExpression.SelectExprProcessor[level.LevelNumber].Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext);
        }

        private UniformPair<EventBean[]> HandleOutputLimitLastView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
            var oldEventCount = 0;
            if (Prototype.IsSelectRStream) {
                _rstreamEventSortArrayPair.Reset();
            }
    
            foreach (var aGroupRepsView in OutputLimitGroupRepsPerLevel) {
                aGroupRepsView.Clear();
            }
    
            // outer loop is the events
            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                EventBean[] eventsPerStream;
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        eventsPerStream = new EventBean[] {aNewData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                            {
                                if (Prototype.IsSelectRStream) {
                                    GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                    oldEventCount++;
                                }
                            }
                        }
                        AggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        eventsPerStream = new EventBean[] {anOldData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream) == null) {
                                if (Prototype.IsSelectRStream) {
                                    GenerateOutputBatched(true, groupKey, level, eventsPerStream, true, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                    oldEventCount++;
                                }
                            }
                        }
                        AggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
            }
    
            return GenerateAndSort(OutputLimitGroupRepsPerLevel, generateSynthetic, oldEventCount);
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitLastJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> viewEventsList, bool generateSynthetic)
        {
            var oldEventCount = 0;
            if (Prototype.IsSelectRStream) {
                _rstreamEventSortArrayPair.Reset();
            }
    
            foreach (var aGroupRepsView in OutputLimitGroupRepsPerLevel) {
                aGroupRepsView.Clear();
            }
    
            // outer loop is the events
            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aNewData.Array) == null) {
                                if (Prototype.IsSelectRStream) {
                                    GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                    oldEventCount++;
                                }
                            }
                        }
                        AggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, anOldData.Array) == null) {
                                if (Prototype.IsSelectRStream) {
                                    GenerateOutputBatched(true, groupKey, level, anOldData.Array, true, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                    oldEventCount++;
                                }
                            }
                        }
                        AggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
            }
    
            return GenerateAndSort(OutputLimitGroupRepsPerLevel, generateSynthetic, oldEventCount);
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitAllView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
            var oldEventCount = 0;
            if (Prototype.IsSelectRStream) {
                _rstreamEventSortArrayPair.Reset();
    
                foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                    var groupGenerators = OutputLimitGroupRepsPerLevel[level.LevelNumber];
                    foreach (var entry in groupGenerators) {
                        GenerateOutputBatched(false, entry.Key, level, entry.Value, false, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                        oldEventCount++;
                    }
                }
            }
    
            // outer loop is the events
            foreach (var pair in viewEventsList)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        var eventsPerStream = new EventBean[] {aNewData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            Object existing = OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream);
    
                            if (existing == null && Prototype.IsSelectRStream) {
                                GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                oldEventCount++;
                            }
                        }
                        AggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        var eventsPerStream = new EventBean[] {anOldData};
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            Object existing = OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, eventsPerStream);
    
                            if (existing == null && Prototype.IsSelectRStream) {
                                GenerateOutputBatched(false, groupKey, level, eventsPerStream, false, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                oldEventCount++;
                            }
                        }
                        AggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
            }
    
            return GenerateAndSort(OutputLimitGroupRepsPerLevel, generateSynthetic, oldEventCount);
        }
    
        private UniformPair<EventBean[]> HandleOutputLimitAllJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
            var oldEventCount = 0;
            if (Prototype.IsSelectRStream) {
                _rstreamEventSortArrayPair.Reset();
    
                foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                    var groupGenerators = OutputLimitGroupRepsPerLevel[level.LevelNumber];
                    foreach (var entry in groupGenerators) {
                        GenerateOutputBatched(false, entry.Key, level, entry.Value, false, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                        oldEventCount++;
                    }
                }
            }
    
            // outer loop is the events
            foreach (var pair in joinEventsSet)
            {
                var newData = pair.First;
                var oldData = pair.Second;
    
                // apply to aggregates
                var groupKeysPerLevel = new Object[Prototype.GroupByRollupDesc.Levels.Length];
                if (newData != null) {
                    foreach (var aNewData in newData) {
                        var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            Object existing = OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, aNewData.Array);
    
                            if (existing == null && Prototype.IsSelectRStream) {
                                GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                oldEventCount++;
                            }
                        }
                        AggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
                if (oldData != null) {
                    foreach (var anOldData in oldData) {
                        var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
                        foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                            var groupKey = level.ComputeSubkey(groupKeyComplete);
                            groupKeysPerLevel[level.LevelNumber] = groupKey;
                            Object existing = OutputLimitGroupRepsPerLevel[level.LevelNumber].Push(groupKey, anOldData.Array);
    
                            if (existing == null && Prototype.IsSelectRStream) {
                                GenerateOutputBatched(false, groupKey, level, anOldData.Array, false, generateSynthetic, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                                oldEventCount++;
                            }
                        }
                        AggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
                    }
                }
            }
    
            return GenerateAndSort(OutputLimitGroupRepsPerLevel, generateSynthetic, oldEventCount);
        }

        private void GenerateOutputBatchedCollectNonJoin(IDictionary<Object, EventBean>[] eventPairs, bool isNewData, bool generateSynthetic, IList<EventBean> events, IList<Object> sortKey)
        {
            var levels = Prototype.GroupByRollupDesc.Levels;
            var eventsPerStream = new EventBean[1];
    
            foreach (var level in levels) {
                var eventsForLevel = eventPairs[level.LevelNumber];
                foreach (var pair in eventsForLevel) {
                    eventsPerStream[0] = pair.Value;
                    GenerateOutputBatched(false, pair.Key, level, eventsPerStream, isNewData, generateSynthetic, events, sortKey);
                }
            }
        }

        private void GenerateOutputBatchedCollectJoin(IDictionary<Object, EventBean[]>[] eventPairs, bool isNewData, bool generateSynthetic, List<EventBean> events, List<Object> sortKey)
        {
            var levels = Prototype.GroupByRollupDesc.Levels;
    
            foreach (var level in levels) {
                var eventsForLevel = eventPairs[level.LevelNumber];
                foreach (var pair in eventsForLevel) {
                    GenerateOutputBatched(false, pair.Key, level, pair.Value, isNewData, generateSynthetic, events, sortKey);
                }
            }
        }
    
        private void ResetEventPerGroupBuf() {
            foreach (var anEventPerGroupBuf in EventPerGroupBuf) {
                anEventPerGroupBuf.Clear();
            }
        }
    
        private void ResetEventPerGroupJoinBuf() {
            foreach (var anEventPerGroupBuf in EventPerGroupJoinBuf) {
                anEventPerGroupBuf.Clear();
            }
        }
    
        private EventsAndSortKeysPair GetOldEventsSortKeys(int oldEventCount, List<EventBean>[] oldEventsPerLevel, List<Object>[] oldEventsSortKeyPerLevel) {
            var oldEventsArr = new EventBean[oldEventCount];
            Object[] oldEventsSortKeys = null;
            if (OrderByProcessor != null) {
                oldEventsSortKeys = new Object[oldEventCount];
            }
            var countEvents = 0;
            var countSortKeys = 0;
            foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                var events = oldEventsPerLevel[level.LevelNumber];
                foreach (var @event in events) {
                    oldEventsArr[countEvents++] = @event;
                }
                if (OrderByProcessor != null) {
                    var sortKeys = oldEventsSortKeyPerLevel[level.LevelNumber];
                    foreach (var sortKey in sortKeys) {
                        oldEventsSortKeys[countSortKeys++] = sortKey;
                    }
                }
            }
            return new EventsAndSortKeysPair(oldEventsArr, oldEventsSortKeys);
        }
    
        public Object[][] GenerateGroupKeysView(EventBean[] events, IDictionary<Object, EventBean>[] eventPerKey, bool isNewData)
        {
            if (events == null) {
                return null;
            }
    
            var result = new Object[events.Length][];
            var eventsPerStream = new EventBean[1];
    
            for (var i = 0; i < events.Length; i++) {
                eventsPerStream[0] = events[i];
                var groupKeyComplete = GenerateGroupKey(eventsPerStream, isNewData);
                var levels = Prototype.GroupByRollupDesc.Levels;
                result[i] = new Object[levels.Length];
                for (var j = 0; j < levels.Length; j++) {
                    var subkey = levels[j].ComputeSubkey(groupKeyComplete);
                    result[i][j] = subkey;
                    eventPerKey[levels[j].LevelNumber].Push(subkey, events[i]);
                }
            }
    
            return result;
        }
    
        private Object[][] GenerateGroupKeysJoin(ISet<MultiKey<EventBean>> events, IDictionary<Object, EventBean[]>[] eventPerKey, bool isNewData)
        {
            if (events == null || events.IsEmpty()) {
                return null;
            }
    
            var result = new Object[events.Count][];
    
            var count = -1;
            foreach (var eventrow in events) {
                count++;
                var groupKeyComplete = GenerateGroupKey(eventrow.Array, isNewData);
                var levels = Prototype.GroupByRollupDesc.Levels;
                result[count] = new Object[levels.Length];
                for (var j = 0; j < levels.Length; j++) {
                    var subkey = levels[j].ComputeSubkey(groupKeyComplete);
                    result[count][j] = subkey;
                    eventPerKey[levels[j].LevelNumber].Push(subkey, eventrow.Array);
                }
            }
    
            return result;
        }
    
        private UniformPair<EventBean[]> GenerateAndSort(IDictionary<Object, EventBean[]>[] outputLimitGroupRepsPerLevel, bool generateSynthetic, int oldEventCount)
        {
            // generate old events: ordered by level by default
            EventBean[] oldEventsArr = null;
            Object[] oldEventSortKeys = null;
            if (Prototype.IsSelectRStream && oldEventCount > 0) {
                var pair = GetOldEventsSortKeys(oldEventCount, _rstreamEventSortArrayPair.EventsPerLevel, _rstreamEventSortArrayPair.SortKeyPerLevel);
                oldEventsArr = pair.Events;
                oldEventSortKeys = pair.SortKeys;
            }
    
            var newEvents = new List<EventBean>();
            List<Object> newEventsSortKey = null;
            if (OrderByProcessor != null) {
                newEventsSortKey = new List<Object>();
            }
    
            foreach (var level in Prototype.GroupByRollupDesc.Levels) {
                var groupGenerators = outputLimitGroupRepsPerLevel[level.LevelNumber];
                foreach (var entry in groupGenerators) {
                    GenerateOutputBatched(false, entry.Key, level, entry.Value, true, generateSynthetic, newEvents, newEventsSortKey);
                }
            }
    
            var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
            if (OrderByProcessor != null) {
                var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
                newEventsArr = OrderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                if (Prototype.IsSelectRStream) {
                    oldEventsArr = OrderByProcessor.Sort(oldEventsArr, oldEventSortKeys, _agentInstanceContext);
                }
            }
    
            if ((newEventsArr == null) && (oldEventsArr == null)) {
                return null;
            }
            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }

        public virtual void ApplyViewResult(EventBean[] newData, EventBean[] oldData)
        {
            EventBean[] eventsPerStream = new EventBean[1];
            if (newData != null) {
                foreach (EventBean aNewData in newData) {
                    eventsPerStream[0] = aNewData;
                    Object[] keys = GenerateGroupKeysNonJoin(eventsPerStream, true);
                    AggregationService.ApplyEnter(eventsPerStream, keys, AgentInstanceContext);
                }
            }
            if (oldData != null) {
                foreach (EventBean anOldData in oldData) {
                    eventsPerStream[0] = anOldData;
                    Object[] keys = GenerateGroupKeysNonJoin(eventsPerStream, false);
                    AggregationService.ApplyLeave(eventsPerStream, keys, AgentInstanceContext);
                }
            }
        }

        public virtual void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            if (newEvents != null) {
                foreach (MultiKey<EventBean> mk in newEvents) {
                    Object[] keys = GenerateGroupKeysNonJoin(mk.Array, true);
                    AggregationService.ApplyEnter(mk.Array, keys, AgentInstanceContext);
                }
            }
            if (oldEvents != null) {
                foreach (MultiKey<EventBean> mk in oldEvents) {
                    Object[] keys = GenerateGroupKeysNonJoin(mk.Array, false);
                    AggregationService.ApplyLeave(mk.Array, keys, AgentInstanceContext);
                }
            }
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

        public void ProcessOutputLimitedLastAllNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isGenerateSynthetic, bool isAll)
        {
            if (isAll)
            {
                _outputAllHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
            }
            else
            {
                _outputLastHelper.ProcessJoin(newEvents, oldEvents, isGenerateSynthetic);
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

        private UniformPair<EventBean[]> ConvertToArrayMaySort(List<EventBean> newEvents, List<Object> newEventsSortKey, List<EventBean> oldEvents, List<Object> oldEventsSortKey) {
            var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
            EventBean[] oldEventsArr = null;
            if (Prototype.IsSelectRStream) {
                oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
            }
    
            if (OrderByProcessor != null) {
                var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
                newEventsArr = OrderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
                if (Prototype.IsSelectRStream) {
                    var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
                    oldEventsArr = OrderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
                }
            }
    
            if ((newEventsArr == null) && (oldEventsArr == null)) {
                return null;
            }
            return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
        }
    
        private EPException HandleConditionValidationException(ExprValidationException e) {
            return new EPException("Error starting output limit for group for statement '" + _agentInstanceContext.StatementContext.StatementName + "': " + e.Message, e);
        }

        private Object[] GenerateGroupKeysNonJoin(EventBean[] eventsPerStream, bool isNewData)
        {
            var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
            var levels = Prototype.GroupByRollupDesc.Levels;
            var result = new Object[levels.Length];
            for (int j = 0; j < levels.Length; j++)
            {
                Object subkey = levels[j].ComputeSubkey(groupKeyComplete);
                result[j] = subkey;
            }
            return result;
        }
    
        public class EventArrayAndSortKeyArray
        {
            public EventArrayAndSortKeyArray(List<EventBean>[] eventsPerLevel, List<Object>[] sortKeyPerLevel)
            {
                EventsPerLevel = eventsPerLevel;
                SortKeyPerLevel = sortKeyPerLevel;
            }

            public List<EventBean>[] EventsPerLevel { get; private set; }

            public List<object>[] SortKeyPerLevel { get; private set; }

            public void Reset() {
               foreach (var anEventsPerLevel in EventsPerLevel) {
                   anEventsPerLevel.Clear();
               }
               if (SortKeyPerLevel != null) {
                   foreach (var anSortKeyPerLevel in SortKeyPerLevel) {
                       anSortKeyPerLevel.Clear();
                   }
               }
           }
        }
    
        public class EventsAndSortKeysPair
        {
            public EventsAndSortKeysPair(EventBean[] events, Object[] sortKeys) {
                Events = events;
                SortKeys = sortKeys;
            }

            public EventBean[] Events { get; private set; }

            public object[] SortKeys { get; private set; }
        }
    }
}

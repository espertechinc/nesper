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
using com.espertech.esper.client.scopetest;
using com.espertech.esper.collection;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.rollup;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.spec;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.view;

namespace com.espertech.esper.epl.core
{
	public class ResultSetProcessorRowPerGroupRollup
        : ResultSetProcessor
        , AggregationRowRemovedCallback
    {
        private readonly ResultSetProcessorRowPerGroupRollupFactory _prototype;
        private readonly OrderByProcessor _orderByProcessor;
        private readonly AggregationService _aggregationService;
        private AgentInstanceContext _agentInstanceContext;

	    private readonly IDictionary<object, EventBean[]>[] _groupRepsPerLevelBuf;
	    private readonly IDictionary<object, EventBean>[] _eventPerGroupBuf;
	    private readonly IDictionary<object, EventBean[]>[] _eventPerGroupJoinBuf;
	    private readonly EventArrayAndSortKeyArray _rstreamEventSortArrayBuf;

	    private readonly ResultSetProcessorRowPerGroupRollupOutputLastHelper _outputLastHelper;
	    private readonly ResultSetProcessorRowPerGroupRollupOutputAllHelper _outputAllHelper;
	    private readonly ResultSetProcessorGroupedOutputFirstHelper[] _outputFirstHelpers;

	    public ResultSetProcessorRowPerGroupRollup(
	        ResultSetProcessorRowPerGroupRollupFactory prototype,
	        OrderByProcessor orderByProcessor,
	        AggregationService aggregationService,
	        AgentInstanceContext agentInstanceContext)
        {
	        _prototype = prototype;
	        _orderByProcessor = orderByProcessor;
	        _aggregationService = aggregationService;
	        _agentInstanceContext = agentInstanceContext;
	        aggregationService.SetRemovedCallback(this);

	        var levelCount = prototype.GroupByRollupDesc.Levels.Length;

	        if (prototype.IsJoin) {
                _eventPerGroupJoinBuf = new IDictionary<object, EventBean[]>[levelCount];
	            for (var i = 0; i < levelCount; i++) {
	                _eventPerGroupJoinBuf[i] = new LinkedHashMap<object, EventBean[]>();
	            }
	            _eventPerGroupBuf = null;
	        }
	        else {
                _eventPerGroupBuf = new IDictionary<object, EventBean>[levelCount];
	            for (var i = 0; i < levelCount; i++) {
	                _eventPerGroupBuf[i] = new LinkedHashMap<object, EventBean>();
	            }
	            _eventPerGroupJoinBuf = null;
	        }

	        if (prototype.OutputLimitSpec != null) {
                _groupRepsPerLevelBuf = new IDictionary<object, EventBean[]>[levelCount];
	            for (var i = 0; i < levelCount; i++) {
	                _groupRepsPerLevelBuf[i] = new LinkedHashMap<object, EventBean[]>();
	            }

	            if (prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.LAST) {
	                _outputLastHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowPerGroupRollupLast(agentInstanceContext, this, prototype);
	                _outputAllHelper = null;
	            }
	            else if (prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.ALL) {
	                _outputAllHelper = prototype.ResultSetProcessorHelperFactory.MakeRSRowPerGroupRollupAll(agentInstanceContext, this, prototype);
	                _outputLastHelper = null;
	            }
	            else {
	                _outputLastHelper = null;
	                _outputAllHelper = null;
	            }
	        }
	        else {
	            _groupRepsPerLevelBuf = null;
	            _outputLastHelper = null;
	            _outputAllHelper = null;
	        }

	        // Allocate output state for output-first
	        if (prototype.OutputLimitSpec != null && prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST) {
	            _outputFirstHelpers = new ResultSetProcessorGroupedOutputFirstHelper[levelCount];
	            for (var i = 0; i < levelCount; i++) {
	                _outputFirstHelpers[i] = prototype.ResultSetProcessorHelperFactory.MakeRSGroupedOutputFirst(agentInstanceContext, prototype.GroupKeyNodes, prototype.OptionalOutputFirstConditionFactory, prototype.GroupByRollupDesc, i);
	            }
	        }
	        else {
	            _outputFirstHelpers = null;
	        }

	        if (prototype.OutputLimitSpec != null && (prototype.IsSelectRStream || prototype.OutputLimitSpec.DisplayLimit == OutputLimitLimitType.FIRST)) {
	            var eventsPerLevel = new IList<EventBean>[prototype.GroupByRollupDesc.Levels.Length];
	            IList<object>[] sortKeyPerLevel = null;
	            if (orderByProcessor != null) {
                    sortKeyPerLevel = new IList<object>[prototype.GroupByRollupDesc.Levels.Length];
	            }
	            foreach (var level in prototype.GroupByRollupDesc.Levels) {
	                eventsPerLevel[level.LevelNumber] = new List<EventBean>();
	                if (orderByProcessor != null) {
	                    sortKeyPerLevel[level.LevelNumber] = new List<object>();
	                }
	            }
	            _rstreamEventSortArrayBuf = new EventArrayAndSortKeyArray(eventsPerLevel, sortKeyPerLevel);
	        }
	        else {
	            _rstreamEventSortArrayBuf = null;
	        }
	    }

	    public ResultSetProcessorRowPerGroupRollupFactory Prototype
	    {
	        get { return _prototype; }
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

	    public AggregationService AggregationService
	    {
	        get { return _aggregationService; }
	    }

	    public EventType ResultEventType
	    {
	        get { return _prototype.ResultEventType; }
	    }

	    public UniformPair<EventBean[]> ProcessJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, bool isSynthesize)
	    {
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QResultSetProcessGroupedRowPerGroup();}

	        if (_prototype.IsUnidirectional) {
	            Clear();
	        }

	        ResetEventPerGroupJoinBuf();
	        var newDataMultiKey = GenerateGroupKeysJoin(newEvents, _eventPerGroupJoinBuf, true);
	        var oldDataMultiKey = GenerateGroupKeysJoin(oldEvents, _eventPerGroupJoinBuf, false);

	        EventBean[] selectOldEvents = null;
	        if (_prototype.IsSelectRStream) {
	            selectOldEvents = GenerateOutputEventsJoin(_eventPerGroupJoinBuf, false, isSynthesize);
	        }

	        // update aggregates
	        if (newEvents != null) {
	            var count = 0;
	            foreach (var mk in newEvents) {
	                _aggregationService.ApplyEnter(mk.Array, newDataMultiKey[count++], _agentInstanceContext);
	            }
	        }
	        if (oldEvents != null) {
	            var count = 0;
	            foreach (var mk in oldEvents) {
	                _aggregationService.ApplyLeave(mk.Array, oldDataMultiKey[count++], _agentInstanceContext);
	            }
	        }

	        // generate new events using select expressions
	        var selectNewEvents = GenerateOutputEventsJoin(_eventPerGroupJoinBuf, true, isSynthesize);

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
	        var newDataMultiKey = GenerateGroupKeysView(newData, _eventPerGroupBuf, true);
	        var oldDataMultiKey = GenerateGroupKeysView(oldData, _eventPerGroupBuf, false);

	        EventBean[] selectOldEvents = null;
	        if (_prototype.IsSelectRStream) {
	            selectOldEvents = GenerateOutputEventsView(_eventPerGroupBuf, false, isSynthesize);
	        }

	        // update aggregates
	        var eventsPerStream = new EventBean[1];
	        if (newData != null) {
	            for (var i = 0; i < newData.Length; i++) {
	                eventsPerStream[0] = newData[i];
	                _aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], _agentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            for (var i = 0; i < oldData.Length; i++) {
	                eventsPerStream[0] = oldData[i];
	                _aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], _agentInstanceContext);
	            }
	        }

	        // generate new events using select expressions
	        var selectNewEvents = GenerateOutputEventsView(_eventPerGroupBuf, true, isSynthesize);

	        if ((selectNewEvents != null) || (selectOldEvents != null)) {
	            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(selectNewEvents, selectOldEvents);}
	            return new UniformPair<EventBean[]>(selectNewEvents, selectOldEvents);
	        }
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AResultSetProcessGroupedRowPerGroup(null, null);}
	        return null;
	    }

	    protected EventBean[] GenerateOutputEventsView(IDictionary<object, EventBean>[] keysAndEvents, bool isNewData, bool isSynthesize)
	    {
	        var eventsPerStream = new EventBean[1];
	        var events = new List<EventBean>(1);
	        IList<GroupByRollupKey> currentGenerators = null;
	        if(_prototype.IsSorting) {
	            currentGenerators = new List<GroupByRollupKey>(4);
	        }

	        var levels = _prototype.GroupByRollupDesc.Levels;
	        var selectExprProcessors = _prototype.PerLevelExpression.SelectExprProcessor;
	        var optionalHavingClauses = _prototype.PerLevelExpression.OptionalHavingNodes;
            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
            foreach (var level in levels)
            {
	            foreach (var entry in keysAndEvents[level.LevelNumber]) {
	                var groupKey = entry.Key;

	                // Set the current row of aggregation states
	                _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
	                eventsPerStream[0] = entry.Value;

	                // Filter the having clause
	                if (optionalHavingClauses != null)
	                {
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(entry.Value);}
	                    var result = optionalHavingClauses[level.LevelNumber].Evaluate(evaluateParams);
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
	                    if ((result == null) || (false.Equals(result))) {
	                        continue;
	                    }
	                }
	                events.Add(selectExprProcessors[level.LevelNumber].Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));

	                if(_prototype.IsSorting) {
	                    var currentEventsPerStream = new EventBean[] { entry.Value };
	                    currentGenerators.Add(new GroupByRollupKey(currentEventsPerStream, level, groupKey));
	                }
	            }
	        }

	        if (events.IsEmpty()) {
	            return null;
	        }
	        var outgoing = events.ToArray();
	        if (outgoing.Length > 1 && _prototype.IsSorting) {
	            return _orderByProcessor.Sort(outgoing, currentGenerators, isNewData, _agentInstanceContext, _prototype.PerLevelExpression.OptionalOrderByElements);
	        }
	        return outgoing;
	    }

	    private EventBean[] GenerateOutputEventsJoin(IDictionary<object, EventBean[]>[] eventPairs, bool isNewData, bool synthesize) {
	        var events = new List<EventBean>(1);
	        IList<GroupByRollupKey> currentGenerators = null;
	        if(_prototype.IsSorting) {
	            currentGenerators = new List<GroupByRollupKey>(4);
	        }

	        var levels = _prototype.GroupByRollupDesc.Levels;
	        var selectExprProcessors = _prototype.PerLevelExpression.SelectExprProcessor;
	        var optionalHavingClauses = _prototype.PerLevelExpression.OptionalHavingNodes;
	        foreach (var level in levels) {
	            foreach (var entry in eventPairs[level.LevelNumber]) {
	                var groupKey = entry.Key;

	                // Set the current row of aggregation states
	                _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);

	                // Filter the having clause
	                if (optionalHavingClauses != null)
	                {
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(entry.Value);}
	                    var evaluateParams = new EvaluateParams(entry.Value, isNewData, _agentInstanceContext);
	                    var result = optionalHavingClauses[level.LevelNumber].Evaluate(evaluateParams);
	                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	                    if ((result == null) || (false.Equals(result))) {
	                        continue;
	                    }
	                }
	                events.Add(selectExprProcessors[level.LevelNumber].Process(entry.Value, isNewData, synthesize, _agentInstanceContext));

	                if(_prototype.IsSorting) {
	                    currentGenerators.Add(new GroupByRollupKey(entry.Value, level, groupKey));
	                }
	            }
	        }

	        if (events.IsEmpty()) {
	            return null;
	        }
	        var outgoing = events.ToArray();
	        if (outgoing.Length > 1 && _prototype.IsSorting) {
	            return _orderByProcessor.Sort(outgoing, currentGenerators, isNewData, _agentInstanceContext, _prototype.PerLevelExpression.OptionalOrderByElements);
	        }
	        return outgoing;
	    }

	    public virtual IEnumerator<EventBean> GetEnumerator(Viewable parent)
	    {
	        if (!_prototype.IsHistoricalOnly) {
	            return ObtainEnumerator(parent);
	        }

	        _aggregationService.ClearResults(_agentInstanceContext);
	        var it = parent.GetEnumerator();
	        var eventsPerStream = new EventBean[1];
	        var groupKeys = new object[_prototype.GroupByRollupDesc.Levels.Length];
	        var levels = _prototype.GroupByRollupDesc.Levels;
	        while (it.MoveNext()) {
	            eventsPerStream[0] = it.Current;
	            var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	            for (var j = 0; j < levels.Length; j++) {
	                var subkey = levels[j].ComputeSubkey(groupKeyComplete);
	                groupKeys[j] = subkey;
	            }
	            _aggregationService.ApplyEnter(eventsPerStream, groupKeys, _agentInstanceContext);
	        }

	        ArrayDeque<EventBean> deque = ResultSetProcessorUtil.EnumeratorToDeque(ObtainEnumerator(parent));
	        _aggregationService.ClearResults(_agentInstanceContext);
	        return deque.GetEnumerator();
	    }

	    private static readonly IList<EventBean> EMPTY_BEAN_ARRAY = new EventBean[0];

	    private IEnumerator<EventBean> ObtainEnumerator(Viewable parent)
	    {
	        ResetEventPerGroupBuf();
	        EventBean[] events = EPAssertionUtil.EnumeratorToArray(parent.GetEnumerator());
	        GenerateGroupKeysView(events, _eventPerGroupBuf, true);
	        var output = GenerateOutputEventsView(_eventPerGroupBuf, true, true) ?? EMPTY_BEAN_ARRAY;
            return output.GetEnumerator();
	    }

	    public IEnumerator<EventBean> GetEnumerator(ISet<MultiKey<EventBean>> joinSet)
	    {
	        ResetEventPerGroupJoinBuf();
	        GenerateGroupKeysJoin(joinSet, _eventPerGroupJoinBuf, true);
	        var output = GenerateOutputEventsJoin(_eventPerGroupJoinBuf, true, true) ?? EMPTY_BEAN_ARRAY;
	        return output.GetEnumerator();
	    }

	    public void Clear()
	    {
	        _aggregationService.ClearResults(_agentInstanceContext);
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

	    private UniformPair<EventBean[]> HandleOutputLimitFirstView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic) {

	        foreach (var aGroupRepsView in _groupRepsPerLevelBuf) {
	            aGroupRepsView.Clear();
	        }

	        _rstreamEventSortArrayBuf.Reset();

	        int oldEventCount;
	        if (_prototype.PerLevelExpression.OptionalHavingNodes == null) {
	            oldEventCount = HandleOutputLimitFirstViewNoHaving(viewEventsList, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	        }
	        else {
	            oldEventCount = HandleOutputLimitFirstViewHaving(viewEventsList, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	        }

	        return GenerateAndSort(_groupRepsPerLevelBuf, generateSynthetic, oldEventCount);
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitFirstJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic) {

	        foreach (var aGroupRepsView in _groupRepsPerLevelBuf) {
	            aGroupRepsView.Clear();
	        }

	        _rstreamEventSortArrayBuf.Reset();

	        int oldEventCount;
	        if (_prototype.PerLevelExpression.OptionalHavingNodes == null) {
	            oldEventCount = HandleOutputLimitFirstJoinNoHaving(joinEventsSet, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	        }
	        else {
	            oldEventCount = HandleOutputLimitFirstJoinHaving(joinEventsSet, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	        }

	        return GenerateAndSort(_groupRepsPerLevelBuf, generateSynthetic, oldEventCount);
	    }

	    private int HandleOutputLimitFirstViewHaving(
	        IList<UniformPair<EventBean[]>> viewEventsList,
	        bool generateSynthetic,
	        IList<EventBean>[] oldEventsPerLevel,
	        IList<object>[] oldEventsSortKeyPerLevel)
        {
	        var oldEventCount = 0;

	        var havingPerLevel = _prototype.PerLevelExpression.OptionalHavingNodes;

	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            // apply to aggregates
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            EventBean[] eventsPerStream;
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    eventsPerStream = new EventBean[] {aNewData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    }
	                    _aggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    eventsPerStream = new EventBean[] {anOldData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    }
	                    _aggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }

	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    eventsPerStream = new EventBean[] {aNewData};
                        var evaluateParams = new EvaluateParams(eventsPerStream, true, _agentInstanceContext);
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);

	                        _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(aNewData);}
	                        var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean());}
	                        if ((result == null) || (false.Equals(result))) {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
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
                        var evaluateParams = new EvaluateParams(eventsPerStream, false, _agentInstanceContext);
                        var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);

	                        _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseNonJoin(anOldData);}
	                        var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoxedBoolean());}
	                        if ((result == null) || (false.Equals(result))) {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
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

	    private int HandleOutputLimitFirstJoinNoHaving(
	        IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventSet,
	        bool generateSynthetic,
	        IList<EventBean>[] oldEventsPerLevel,
	        IList<object>[] oldEventsSortKeyPerLevel)
        {
	        var oldEventCount = 0;

	        // outer loop is the events
	        foreach (var pair in joinEventSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            // apply to aggregates
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, aNewData.Array) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
	                                    GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
	                                    oldEventCount++;
	                                }
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, anOldData.Array) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
	                                    GenerateOutputBatched(false, groupKey, level, anOldData.Array, false, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
	                                    oldEventCount++;
	                                }
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	        }
	        return oldEventCount;
	    }

	    private int HandleOutputLimitFirstJoinHaving(
	        IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventSet,
	        bool generateSynthetic,
	        IList<EventBean>[] oldEventsPerLevel,
	        IList<object>[] oldEventsSortKeyPerLevel)
        {
	        var oldEventCount = 0;

	        var havingPerLevel = _prototype.PerLevelExpression.OptionalHavingNodes;

	        foreach (var pair in joinEventSet)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            // apply to aggregates
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    }
	                    _aggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
	                    }
	                    _aggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }

	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
                        var evaluateParams = new EvaluateParams(aNewData.Array, true, _agentInstanceContext);
                        foreach (var level in _prototype.GroupByRollupDesc.Levels)
                        {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);

	                        _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(aNewData.Array);}
	                        var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean());}
	                        if ((result == null) || (false.Equals(result))) {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, aNewData.Array) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
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
                        foreach (var level in _prototype.GroupByRollupDesc.Levels)
                        {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);

	                        _aggregationService.SetCurrentAccess(groupKey, _agentInstanceContext.AgentInstanceId, level);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QHavingClauseJoin(anOldData.Array);}
	                        var result = havingPerLevel[level.LevelNumber].Evaluate(evaluateParams);
	                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoxedBoolean());}
	                        if ((result == null) || (false.Equals(result))) {
	                            continue;
	                        }

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, anOldData.Array) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
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

	    private int HandleOutputLimitFirstViewNoHaving(
	        IList<UniformPair<EventBean[]>> viewEventsList,
	        bool generateSynthetic,
	        IList<EventBean>[] oldEventsPerLevel,
	        IList<object>[] oldEventsSortKeyPerLevel)
        {
	        var oldEventCount = 0;

	        // outer loop is the events
	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            // apply to aggregates
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            EventBean[] eventsPerStream;
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    eventsPerStream = new EventBean[] {aNewData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
	                                    GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
	                                    oldEventCount++;
	                                }
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    eventsPerStream = new EventBean[] {anOldData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;

	                        var outputStateGroup = _outputFirstHelpers[level.LevelNumber].GetOrAllocate(groupKey, _agentInstanceContext, _prototype.OptionalOutputFirstConditionFactory);
	                        var pass = outputStateGroup.UpdateOutputCondition(1, 0);
	                        if (pass) {
                                if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                                {
	                                if (_prototype.IsSelectRStream) {
	                                    GenerateOutputBatched(false, groupKey, level, eventsPerStream, false, generateSynthetic, oldEventsPerLevel, oldEventsSortKeyPerLevel);
	                                    oldEventCount++;
	                                }
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	        }
	        return oldEventCount;
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitDefaultView(
	        IList<UniformPair<EventBean[]>> viewEventsList,
	        bool generateSynthetic)
        {
	        IList<EventBean> newEvents = new List<EventBean>();
	        IList<object> newEventsSortKey = null;
	        if (_orderByProcessor != null) {
	            newEventsSortKey = new List<object>();
	        }

	        IList<EventBean> oldEvents = null;
	        IList<object> oldEventsSortKey = null;
	        if (_prototype.IsSelectRStream) {
	            oldEvents = new List<EventBean>();
	            if (_orderByProcessor != null) {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        foreach (var pair in viewEventsList) {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            ResetEventPerGroupBuf();
	            var newDataMultiKey = GenerateGroupKeysView(newData, _eventPerGroupBuf, true);
	            var oldDataMultiKey = GenerateGroupKeysView(oldData, _eventPerGroupBuf, false);

	            if (_prototype.IsSelectRStream) {
	                GenerateOutputBatchedCollectNonJoin(_eventPerGroupBuf, false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }

	            // update aggregates
	            var eventsPerStream = new EventBean[1];
	            if (newData != null) {
	                for (var i = 0; i < newData.Length; i++) {
	                    eventsPerStream[0] = newData[i];
	                    _aggregationService.ApplyEnter(eventsPerStream, newDataMultiKey[i], _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                for (var i = 0; i < oldData.Length; i++) {
	                    eventsPerStream[0] = oldData[i];
	                    _aggregationService.ApplyLeave(eventsPerStream, oldDataMultiKey[i], _agentInstanceContext);
	                }
	            }

	            GenerateOutputBatchedCollectNonJoin(_eventPerGroupBuf, true, generateSynthetic, newEvents, newEventsSortKey);
	        }

	        return ConvertToArrayMaySort(newEvents, newEventsSortKey, oldEvents, oldEventsSortKey);
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitDefaultJoin(
	        IList<UniformPair<ISet<MultiKey<EventBean>>>> viewEventsList,
	        bool generateSynthetic)
        {
	        IList<EventBean> newEvents = new List<EventBean>();
	        IList<object> newEventsSortKey = null;
	        if (_orderByProcessor != null) {
	            newEventsSortKey = new List<object>();
	        }

	        IList<EventBean> oldEvents = null;
	        IList<object> oldEventsSortKey = null;
	        if (_prototype.IsSelectRStream) {
	            oldEvents = new List<EventBean>();
	            if (_orderByProcessor != null) {
	                oldEventsSortKey = new List<object>();
	            }
	        }

	        foreach (var pair in viewEventsList) {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            ResetEventPerGroupJoinBuf();
	            var newDataMultiKey = GenerateGroupKeysJoin(newData, _eventPerGroupJoinBuf, true);
	            var oldDataMultiKey = GenerateGroupKeysJoin(oldData, _eventPerGroupJoinBuf, false);

	            if (_prototype.IsSelectRStream) {
	                GenerateOutputBatchedCollectJoin(_eventPerGroupJoinBuf, false, generateSynthetic, oldEvents, oldEventsSortKey);
	            }

	            // update aggregates
	            if (newData != null) {
	                var count = 0;
	                foreach (var newEvent in newData) {
	                    _aggregationService.ApplyEnter(newEvent.Array, newDataMultiKey[count++], _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                var count = 0;
	                foreach (var oldEvent in oldData) {
	                    _aggregationService.ApplyLeave(oldEvent.Array, oldDataMultiKey[count++], _agentInstanceContext);
	                }
	            }

	            GenerateOutputBatchedCollectJoin(_eventPerGroupJoinBuf, true, generateSynthetic, newEvents, newEventsSortKey);
	        }

	        return ConvertToArrayMaySort(newEvents, newEventsSortKey, oldEvents, oldEventsSortKey);
	    }

	    public bool HasAggregation
	    {
	        get { return true; }
	    }

	    public void Removed(object key) {
	        throw new UnsupportedOperationException();
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

	    private void GenerateOutputBatched(bool join, object mk, AggregationGroupByRollupLevel level, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, IList<EventBean>[] resultEvents, IList<object>[] optSortKeys) {
	        var resultList = resultEvents[level.LevelNumber];
	        var sortKeys = optSortKeys == null ? null : optSortKeys[level.LevelNumber];
	        GenerateOutputBatched(join, mk, level, eventsPerStream, isNewData, isSynthesize, resultList, sortKeys);
	    }

	    public void GenerateOutputBatched(bool join, object mk, AggregationGroupByRollupLevel level, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, IList<EventBean> resultEvents, IList<object> optSortKeys) {
	        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, level);

	        if (_prototype.PerLevelExpression.OptionalHavingNodes != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
	            var result = _prototype.PerLevelExpression.OptionalHavingNodes[level.LevelNumber].Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	            if ((result == null) || (false.Equals(result))) {
	                return;
	            }
	        }

	        resultEvents.Add(_prototype.PerLevelExpression.SelectExprProcessor[level.LevelNumber].Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));

	        if (_prototype.IsSorting) {
	            optSortKeys.Add(_orderByProcessor.GetSortKey(eventsPerStream, isNewData, _agentInstanceContext, _prototype.PerLevelExpression.OptionalOrderByElements[level.LevelNumber]));
	        }
	    }

	    public void GenerateOutputBatchedMapUnsorted(bool join, object mk, AggregationGroupByRollupLevel level, EventBean[] eventsPerStream, bool isNewData, bool isSynthesize, IDictionary<object, EventBean> resultEvents) {
	        _aggregationService.SetCurrentAccess(mk, _agentInstanceContext.AgentInstanceId, level);

	        if (_prototype.PerLevelExpression.OptionalHavingNodes != null)
	        {
	            if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().QHavingClauseNonJoin(eventsPerStream[0]); else InstrumentationHelper.Get().QHavingClauseJoin(eventsPerStream);}
	            var evaluateParams = new EvaluateParams(eventsPerStream, isNewData, _agentInstanceContext);
	            var result = _prototype.PerLevelExpression.OptionalHavingNodes[level.LevelNumber].Evaluate(evaluateParams);
                if (InstrumentationHelper.ENABLED) { if (!join) InstrumentationHelper.Get().AHavingClauseNonJoin(result.AsBoolean()); else InstrumentationHelper.Get().AHavingClauseJoin(result.AsBoolean()); }
	            if ((result == null) || (false.Equals(result))) {
	                return;
	            }
	        }

	        resultEvents.Put(mk, _prototype.PerLevelExpression.SelectExprProcessor[level.LevelNumber].Process(eventsPerStream, isNewData, isSynthesize, _agentInstanceContext));
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitLastView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic)
        {
	        var oldEventCount = 0;
	        if (_prototype.IsSelectRStream) {
	            _rstreamEventSortArrayBuf.Reset();
	        }

	        foreach (var aGroupRepsView in _groupRepsPerLevelBuf) {
	            aGroupRepsView.Clear();
	        }

	        // outer loop is the events
	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            // apply to aggregates
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            EventBean[] eventsPerStream;
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    eventsPerStream = new EventBean[] {aNewData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                            {
	                            if (_prototype.IsSelectRStream) {
	                                GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                                oldEventCount++;
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    eventsPerStream = new EventBean[] {anOldData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream) == null)
                            {
	                            if (_prototype.IsSelectRStream) {
	                                GenerateOutputBatched(true, groupKey, level, eventsPerStream, true, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                                oldEventCount++;
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	        }

	        return GenerateAndSort(_groupRepsPerLevelBuf, generateSynthetic, oldEventCount);
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitLastJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> viewEventsList, bool generateSynthetic)
        {
	        var oldEventCount = 0;
	        if (_prototype.IsSelectRStream) {
	            _rstreamEventSortArrayBuf.Reset();
	        }

	        foreach (var aGroupRepsView in _groupRepsPerLevelBuf) {
	            aGroupRepsView.Clear();
	        }

	        // outer loop is the events
	        foreach (var pair in viewEventsList)
	        {
	            var newData = pair.First;
	            var oldData = pair.Second;

	            // apply to aggregates
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, aNewData.Array) == null)
                            {
	                            if (_prototype.IsSelectRStream) {
	                                GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                                oldEventCount++;
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            if (_groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, anOldData.Array) == null)
                            {
	                            if (_prototype.IsSelectRStream) {
	                                GenerateOutputBatched(true, groupKey, level, anOldData.Array, true, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                                oldEventCount++;
	                            }
	                        }
	                    }
	                    _aggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	        }

	        return GenerateAndSort(_groupRepsPerLevelBuf, generateSynthetic, oldEventCount);
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitAllView(IList<UniformPair<EventBean[]>> viewEventsList, bool generateSynthetic) {

	        var oldEventCount = 0;
	        if (_prototype.IsSelectRStream) {
	            _rstreamEventSortArrayBuf.Reset();

	            foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                var groupGenerators = _groupRepsPerLevelBuf[level.LevelNumber];
	                foreach (var entry in groupGenerators) {
	                    GenerateOutputBatched(false, entry.Key, level, entry.Value, false, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
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
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    var eventsPerStream = new EventBean[] {aNewData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            var existing = _groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream);

	                        if (existing == null && _prototype.IsSelectRStream) {
	                            GenerateOutputBatched(false, groupKey, level, eventsPerStream, true, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                            oldEventCount++;
	                        }
	                    }
	                    _aggregationService.ApplyEnter(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    var eventsPerStream = new EventBean[] {anOldData};
	                    var groupKeyComplete = GenerateGroupKey(eventsPerStream, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            var existing = _groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, eventsPerStream);

	                        if (existing == null && _prototype.IsSelectRStream) {
	                            GenerateOutputBatched(false, groupKey, level, eventsPerStream, false, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                            oldEventCount++;
	                        }
	                    }
	                    _aggregationService.ApplyLeave(eventsPerStream, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	        }

	        return GenerateAndSort(_groupRepsPerLevelBuf, generateSynthetic, oldEventCount);
	    }

	    private UniformPair<EventBean[]> HandleOutputLimitAllJoin(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, bool generateSynthetic)
        {
            var oldEventCount = 0;
	        if (_prototype.IsSelectRStream) {
	            _rstreamEventSortArrayBuf.Reset();

	            foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                var groupGenerators = _groupRepsPerLevelBuf[level.LevelNumber];
	                foreach (var entry in groupGenerators) {
	                    GenerateOutputBatched(false, entry.Key, level, entry.Value, false, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
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
	            var groupKeysPerLevel = new object[_prototype.GroupByRollupDesc.Levels.Length];
	            if (newData != null) {
	                foreach (var aNewData in newData) {
	                    var groupKeyComplete = GenerateGroupKey(aNewData.Array, true);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            var existing = _groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, aNewData.Array);

	                        if (existing == null && _prototype.IsSelectRStream) {
	                            GenerateOutputBatched(false, groupKey, level, aNewData.Array, true, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                            oldEventCount++;
	                        }
	                    }
	                    _aggregationService.ApplyEnter(aNewData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	            if (oldData != null) {
	                foreach (var anOldData in oldData) {
	                    var groupKeyComplete = GenerateGroupKey(anOldData.Array, false);
	                    foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	                        var groupKey = level.ComputeSubkey(groupKeyComplete);
	                        groupKeysPerLevel[level.LevelNumber] = groupKey;
                            var existing = _groupRepsPerLevelBuf[level.LevelNumber].Push(groupKey, anOldData.Array);

	                        if (existing == null && _prototype.IsSelectRStream) {
	                            GenerateOutputBatched(false, groupKey, level, anOldData.Array, false, generateSynthetic, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	                            oldEventCount++;
	                        }
	                    }
	                    _aggregationService.ApplyLeave(anOldData.Array, groupKeysPerLevel, _agentInstanceContext);
	                }
	            }
	        }

	        return GenerateAndSort(_groupRepsPerLevelBuf, generateSynthetic, oldEventCount);
	    }

	    private void GenerateOutputBatchedCollectNonJoin(IDictionary<object, EventBean>[] eventPairs, bool isNewData, bool generateSynthetic, IList<EventBean> events, IList<object> sortKey)
        {
	        var levels = _prototype.GroupByRollupDesc.Levels;
	        var eventsPerStream = new EventBean[1];

	        foreach (var level in levels) {
	            var eventsForLevel = eventPairs[level.LevelNumber];
	            foreach (var pair in eventsForLevel) {
	                eventsPerStream[0] = pair.Value;
	                GenerateOutputBatched(false, pair.Key, level, eventsPerStream, isNewData, generateSynthetic, events, sortKey);
	            }
	        }
	    }

	    private void GenerateOutputBatchedCollectJoin(IDictionary<object, EventBean[]>[] eventPairs, bool isNewData, bool generateSynthetic, IList<EventBean> events, IList<object> sortKey)
        {
	        var levels = _prototype.GroupByRollupDesc.Levels;

	        foreach (var level in levels) {
	            var eventsForLevel = eventPairs[level.LevelNumber];
	            foreach (var pair in eventsForLevel) {
	                GenerateOutputBatched(false, pair.Key, level, pair.Value, isNewData, generateSynthetic, events, sortKey);
	            }
	        }
	    }

	    private void ResetEventPerGroupBuf() {
	        foreach (var anEventPerGroupBuf in _eventPerGroupBuf) {
	            anEventPerGroupBuf.Clear();
	        }
	    }

	    private void ResetEventPerGroupJoinBuf() {
	        foreach (var anEventPerGroupBuf in _eventPerGroupJoinBuf) {
	            anEventPerGroupBuf.Clear();
	        }
	    }

	    private EventsAndSortKeysPair GetOldEventsSortKeys(int oldEventCount, IList<EventBean>[] oldEventsPerLevel, IList<object>[] oldEventsSortKeyPerLevel) {
	        var oldEventsArr = new EventBean[oldEventCount];
	        object[] oldEventsSortKeys = null;
	        if (_orderByProcessor != null) {
	            oldEventsSortKeys = new object[oldEventCount];
	        }
	        var countEvents = 0;
	        var countSortKeys = 0;
	        foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	            var events = oldEventsPerLevel[level.LevelNumber];
	            foreach (var @event in events) {
	                oldEventsArr[countEvents++] = @event;
	            }
	            if (_orderByProcessor != null) {
	                var sortKeys = oldEventsSortKeyPerLevel[level.LevelNumber];
	                foreach (var sortKey in sortKeys) {
	                    oldEventsSortKeys[countSortKeys++] = sortKey;
	                }
	            }
	        }
	        return new EventsAndSortKeysPair(oldEventsArr, oldEventsSortKeys);
	    }

	    protected object[][] GenerateGroupKeysView(EventBean[] events, IDictionary<object, EventBean>[] eventPerKey, bool isNewData)
	    {
	        if (events == null) {
	            return null;
	        }

	        var result = new object[events.Length][];
	        var eventsPerStream = new EventBean[1];

	        for (var i = 0; i < events.Length; i++) {
	            eventsPerStream[0] = events[i];
	            var groupKeyComplete = GenerateGroupKey(eventsPerStream, isNewData);
	            var levels = _prototype.GroupByRollupDesc.Levels;
	            result[i] = new object[levels.Length];
	            for (var j = 0; j < levels.Length; j++) {
	                var subkey = levels[j].ComputeSubkey(groupKeyComplete);
	                result[i][j] = subkey;
	                eventPerKey[levels[j].LevelNumber].Put(subkey, events[i]);
	            }
	        }

	        return result;
	    }

	    private object[][] GenerateGroupKeysJoin(ISet<MultiKey<EventBean>> events, IDictionary<object, EventBean[]>[] eventPerKey, bool isNewData)
	    {
	        if (events == null || events.IsEmpty()) {
	            return null;
	        }

	        var result = new object[events.Count][];

	        var count = -1;
	        foreach (var eventrow in events) {
	            count++;
	            var groupKeyComplete = GenerateGroupKey(eventrow.Array, isNewData);
	            var levels = _prototype.GroupByRollupDesc.Levels;
	            result[count] = new object[levels.Length];
	            for (var j = 0; j < levels.Length; j++) {
	                var subkey = levels[j].ComputeSubkey(groupKeyComplete);
	                result[count][j] = subkey;
	                eventPerKey[levels[j].LevelNumber].Put(subkey, eventrow.Array);
	            }
	        }

	        return result;
	    }

	    private UniformPair<EventBean[]> GenerateAndSort(IDictionary<object, EventBean[]>[] outputLimitGroupRepsPerLevel, bool generateSynthetic, int oldEventCount) {
	        // generate old events: ordered by level by default
	        EventBean[] oldEventsArr = null;
	        object[] oldEventSortKeys = null;
	        if (_prototype.IsSelectRStream && oldEventCount > 0) {
	            var pair = GetOldEventsSortKeys(oldEventCount, _rstreamEventSortArrayBuf.EventsPerLevel, _rstreamEventSortArrayBuf.SortKeyPerLevel);
	            oldEventsArr = pair.Events;
	            oldEventSortKeys = pair.SortKeys;
	        }

	        IList<EventBean> newEvents = new List<EventBean>();
	        IList<object> newEventsSortKey = null;
	        if (_orderByProcessor != null) {
	            newEventsSortKey = new List<object>();
	        }

	        foreach (var level in _prototype.GroupByRollupDesc.Levels) {
	            var groupGenerators = outputLimitGroupRepsPerLevel[level.LevelNumber];
	            foreach (KeyValuePair<object, EventBean[]> entry in groupGenerators) {
	                GenerateOutputBatched(false, entry.Key, level, entry.Value, true, generateSynthetic, newEvents, newEventsSortKey);
	            }
	        }

	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        if (_orderByProcessor != null) {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream) {
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, oldEventSortKeys, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null)) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    public virtual void ApplyViewResult(EventBean[] newData, EventBean[] oldData) {
	        var eventsPerStream = new EventBean[1];
	        if (newData != null) {
	            foreach (var aNewData in newData) {
	                eventsPerStream[0] = aNewData;
	                var keys = GenerateGroupKeysNonJoin(eventsPerStream, true);
	                _aggregationService.ApplyEnter(eventsPerStream, keys, _agentInstanceContext);
	            }
	        }
	        if (oldData != null) {
	            foreach (var anOldData in oldData) {
	                eventsPerStream[0] = anOldData;
	                var keys = GenerateGroupKeysNonJoin(eventsPerStream, false);
	                _aggregationService.ApplyLeave(eventsPerStream, keys, _agentInstanceContext);
	            }
	        }
	    }

	    public void ApplyJoinResult(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {
	        if (newEvents != null) {
	            foreach (var mk in newEvents) {
	                var keys = GenerateGroupKeysNonJoin(mk.Array, true);
	                _aggregationService.ApplyEnter(mk.Array, keys, _agentInstanceContext);
	            }
	        }
	        if (oldEvents != null) {
	            foreach (var mk in oldEvents) {
	                var keys = GenerateGroupKeysNonJoin(mk.Array, false);
	                _aggregationService.ApplyLeave(mk.Array, keys, _agentInstanceContext);
	            }
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

	    public virtual void Stop() {
	        if (_outputLastHelper != null) {
	            _outputLastHelper.Destroy();
	        }
	        if (_outputFirstHelpers != null) {
	            foreach (var helper in _outputFirstHelpers) {
	                helper.Destroy();
	            }
	        }
	        if (_outputAllHelper != null) {
	            _outputAllHelper.Destroy();
	        }
	    }

	    private UniformPair<EventBean[]> ConvertToArrayMaySort(IList<EventBean> newEvents, IList<object> newEventsSortKey, IList<EventBean> oldEvents, IList<object> oldEventsSortKey) {
	        var newEventsArr = (newEvents.IsEmpty()) ? null : newEvents.ToArray();
	        EventBean[] oldEventsArr = null;
	        if (_prototype.IsSelectRStream) {
	            oldEventsArr = (oldEvents.IsEmpty()) ? null : oldEvents.ToArray();
	        }

	        if (_orderByProcessor != null) {
	            var sortKeysNew = (newEventsSortKey.IsEmpty()) ? null : newEventsSortKey.ToArray();
	            newEventsArr = _orderByProcessor.Sort(newEventsArr, sortKeysNew, _agentInstanceContext);
	            if (_prototype.IsSelectRStream) {
	                var sortKeysOld = (oldEventsSortKey.IsEmpty()) ? null : oldEventsSortKey.ToArray();
	                oldEventsArr = _orderByProcessor.Sort(oldEventsArr, sortKeysOld, _agentInstanceContext);
	            }
	        }

	        if ((newEventsArr == null) && (oldEventsArr == null)) {
	            return null;
	        }
	        return new UniformPair<EventBean[]>(newEventsArr, oldEventsArr);
	    }

	    private object[] GenerateGroupKeysNonJoin(EventBean[] eventsPerStream, bool isNewData) {
	        var groupKeyComplete = GenerateGroupKey(eventsPerStream, true);
	        var levels = _prototype.GroupByRollupDesc.Levels;
	        var result = new object[levels.Length];
	        for (var j = 0; j < levels.Length; j++) {
	            var subkey = levels[j].ComputeSubkey(groupKeyComplete);
	            result[j] = subkey;
	        }
	        return result;
	    }

	    internal class EventArrayAndSortKeyArray
        {
            internal EventArrayAndSortKeyArray(IList<EventBean>[] eventsPerLevel, IList<object>[] sortKeyPerLevel)
	        {
	            EventsPerLevel = eventsPerLevel;
	            SortKeyPerLevel = sortKeyPerLevel;
	        }

	        public readonly IList<EventBean>[] EventsPerLevel;
	        public readonly IList<object>[] SortKeyPerLevel;

	        public void Reset()
	        {
	            foreach (var anEventsPerLevel in EventsPerLevel)
	            {
	                anEventsPerLevel.Clear();
	            }
	            if (SortKeyPerLevel != null)
	            {
	                foreach (var anSortKeyPerLevel in SortKeyPerLevel)
	                {
	                    anSortKeyPerLevel.Clear();
	                }
	            }
	        }
        }

        internal class EventsAndSortKeysPair
        {
            public readonly EventBean[] Events;
            public readonly object[] SortKeys;

            internal EventsAndSortKeysPair(EventBean[] events, object[] sortKeys)
            {
	            Events = events;
	            SortKeys = sortKeys;
	        }
        }
	}
} // end of namespace

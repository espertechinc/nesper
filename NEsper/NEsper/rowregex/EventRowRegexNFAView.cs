///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.schedule;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// View for match recognize support.
    /// </summary>
    public class EventRowRegexNFAView 
        : ViewSupport 
        , EventRowRegexNFAViewService
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const bool IS_DEBUG = false;

        private static readonly IEnumerator<EventBean> NULL_ITERATOR =
            EnumerationHelper<EventBean>.CreateEmptyEnumerator();
    
        private readonly EventRowRegexNFAViewFactory _factory;
        private readonly MatchRecognizeSpec _matchRecognizeSpec;
        private readonly bool _isUnbound;
        private readonly bool _isIterateOnly;
        private readonly bool _isCollectMultimatches;
    
        private readonly EventType _rowEventType;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly AggregationServiceMatchRecognize _aggregationService;
    
        // for interval-handling
        private readonly ScheduleSlot _scheduleSlot;
        private readonly EPStatementHandleCallback _handle;
        private readonly SortedDictionary<long, object> _schedule;
        private readonly bool _isOrTerminated;
    
        private readonly ExprEvaluator[] _columnEvaluators;
        private readonly string[] _columnNames;
    
        private readonly RegexNFAState[] _startStates;
        private readonly RegexNFAState[] _allStates;
    
        private readonly string[] _multimatchVariablesArray;
        private readonly int[] _multimatchStreamNumToVariable;
        private readonly int[] _multimatchVariableToStreamNum;
        private readonly LinkedHashMap<string, Pair<int, bool>> _variableStreams;
        private readonly IDictionary<int, string> _streamsVariables;
        private readonly int _numEventsEventsPerStreamDefine;
        private readonly bool _isDefineAsksMultimatches;
        private readonly ObjectArrayBackedEventBean _defineMultimatchEventBean;
        private readonly RegexPartitionStateRepoGroupMeta _stateRepoGroupMeta;
        private readonly RowRegexExprNode _expandedPatternNode;
    
        private readonly RegexPartitionStateRandomAccessGetter _prevGetter;
        private readonly ObjectArrayBackedEventBean _compositeEventBean;
    
        // state
        private RegexPartitionStateRepo _regexPartitionStateRepo;
        private LinkedHashSet<EventBean> _windowMatchedEventset; // this is NOT per partition - some optimizations are done for batch-processing (minus is out-of-sequence in partition) 
        private int _eventSequenceNumber;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <param name="compositeEventType">final event type</param>
        /// <param name="rowEventType">event type for input rows</param>
        /// <param name="matchRecognizeSpec">specification</param>
        /// <param name="variableStreams">variables and their assigned stream number</param>
        /// <param name="streamsVariables">stream number and the assigned variable</param>
        /// <param name="variablesSingle">single variables</param>
        /// <param name="agentInstanceContext">The agent instance context.</param>
        /// <param name="callbacksPerIndex">for handling the 'prev' function</param>
        /// <param name="aggregationService">handles aggregations</param>
        /// <param name="isDefineAsksMultimatches">if set to <c>true</c> [is define asks multimatches].</param>
        /// <param name="defineMultimatchEventBean">The define multimatch event bean.</param>
        /// <param name="isExprRequiresMultimatchState">State of the is expr requires multimatch.</param>
        /// <param name="isUnbound">true if unbound stream</param>
        /// <param name="isIterateOnly">true for iterate-only</param>
        /// <param name="isCollectMultimatches">if asking for multimatches</param>
        public EventRowRegexNFAView(
            EventRowRegexNFAViewFactory factory,
            ObjectArrayEventType compositeEventType,
            EventType rowEventType,
            MatchRecognizeSpec matchRecognizeSpec,
            LinkedHashMap<string, Pair<int, bool>> variableStreams,
            IDictionary<int, string> streamsVariables,
            ISet<string> variablesSingle,
            AgentInstanceContext agentInstanceContext,
            IDictionary<int, IList<ExprPreviousMatchRecognizeNode>> callbacksPerIndex,
            AggregationServiceMatchRecognize aggregationService,
            bool isDefineAsksMultimatches,
            ObjectArrayBackedEventBean defineMultimatchEventBean,
            bool[] isExprRequiresMultimatchState,
            bool isUnbound,
            bool isIterateOnly,
            bool isCollectMultimatches,
            RowRegexExprNode expandedPatternNode)
        {
            _factory = factory;
            _matchRecognizeSpec = matchRecognizeSpec;
            _compositeEventBean = new ObjectArrayEventBean(new object[variableStreams.Count], compositeEventType);
            _rowEventType = rowEventType;
            _variableStreams = variableStreams;
            _expandedPatternNode = expandedPatternNode;
    
            // determine names of multimatching variables
            if (variablesSingle.Count == variableStreams.Count) {
                _multimatchVariablesArray = new string[0];
                _multimatchStreamNumToVariable = new int[0];
                _multimatchVariableToStreamNum = new int[0];
            }
            else {
                _multimatchVariablesArray = new string[variableStreams.Count - variablesSingle.Count];
                _multimatchVariableToStreamNum = new int[_multimatchVariablesArray.Length];
                _multimatchStreamNumToVariable = new int[variableStreams.Count];
                CompatExtensions.Fill(_multimatchStreamNumToVariable, -1);
                var count = 0;
                foreach (var entry in variableStreams) {
                    if (entry.Value.Second) {
                        var index = count;
                        _multimatchVariablesArray[index] = entry.Key;
                        _multimatchVariableToStreamNum[index] = entry.Value.First;
                        _multimatchStreamNumToVariable[entry.Value.First] = index;
                        count++;
                    }
                }
            }
    
            _streamsVariables = streamsVariables;
            _aggregationService = aggregationService;
            _isDefineAsksMultimatches = isDefineAsksMultimatches;
            _defineMultimatchEventBean = defineMultimatchEventBean;
            _numEventsEventsPerStreamDefine = isDefineAsksMultimatches ? variableStreams.Count + 1 : variableStreams.Count;
            _isUnbound = isUnbound;
            _isIterateOnly = isIterateOnly;
            _agentInstanceContext = agentInstanceContext;
            _isCollectMultimatches = isCollectMultimatches;
    
            if (matchRecognizeSpec.Interval != null)
            {
                _scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
                ScheduleHandleCallback callback = new ProxyScheduleHandleCallback
                {
                    ProcScheduledTrigger = (extensionServicesContext) => 
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegExScheduledEval();}
                        Triggered();
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExScheduledEval();}
                    },
                };
                _handle = new EPStatementHandleCallback(agentInstanceContext.EpStatementAgentInstanceHandle, callback);
                _schedule = new SortedDictionary<long, object>();
    
                agentInstanceContext.AddTerminationCallback(Stop);
                _isOrTerminated = matchRecognizeSpec.Interval.IsOrTerminated;
            }
            else
            {
                _scheduleSlot = null;
                _handle = null;
                _schedule = null;
                _isOrTerminated = false;
            }
    
            _windowMatchedEventset = new LinkedHashSet<EventBean>();
    
            // handle "previous" function nodes (performance-optimized for direct index access)
            if (!callbacksPerIndex.IsEmpty())
            {
                // Build an array of indexes
                var randomAccessIndexesRequested = new int[callbacksPerIndex.Count];
                var count = 0;
                foreach (var entry in callbacksPerIndex)
                {
                    randomAccessIndexesRequested[count] = entry.Key;
                    count++;
                }
                _prevGetter = new RegexPartitionStateRandomAccessGetter(randomAccessIndexesRequested, isUnbound);
            }
            else
            {
                _prevGetter = null;
            }
    
            IDictionary<string, ExprNode> variableDefinitions = new LinkedHashMap<string, ExprNode>();
            foreach (var defineItem in matchRecognizeSpec.Defines)
            {
                variableDefinitions.Put(defineItem.Identifier, defineItem.Expression);
            }
    
            // build states
            var strand = EventRowRegexHelper.RecursiveBuildStartStates(expandedPatternNode, variableDefinitions, variableStreams, isExprRequiresMultimatchState);
            _startStates = strand.StartStates.ToArray();
            _allStates = strand.AllStates.ToArray();
    
            if (Log.IsDebugEnabled || IS_DEBUG)
            {
                Log.Info("NFA tree:\n" + Print(_startStates));
            }
    
            // create evaluators
            _columnNames = new string[matchRecognizeSpec.Measures.Count];
            _columnEvaluators = new ExprEvaluator[matchRecognizeSpec.Measures.Count];
            var countX = 0;
            foreach (var measureItem in matchRecognizeSpec.Measures)
            {
                _columnNames[countX] = measureItem.Name;
                _columnEvaluators[countX] = measureItem.Expr.ExprEvaluator;
                countX++;
            }
    
            // create state repository
            if (_matchRecognizeSpec.PartitionByExpressions.IsEmpty())
            {
                _stateRepoGroupMeta = null;
                _regexPartitionStateRepo = new RegexPartitionStateRepoNoGroup(_prevGetter, matchRecognizeSpec.Interval != null);
            }
            else
            {
                _stateRepoGroupMeta = new RegexPartitionStateRepoGroupMeta(
                    matchRecognizeSpec.Interval != null,
                    ExprNodeUtility.ToArray(matchRecognizeSpec.PartitionByExpressions),
                    ExprNodeUtility.GetEvaluators(matchRecognizeSpec.PartitionByExpressions), agentInstanceContext);
                _regexPartitionStateRepo = new RegexPartitionStateRepoGroup(_prevGetter, _stateRepoGroupMeta);
            }
        }
    
        public void Stop()
        {
            if (_handle != null)
            {
                _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
            }
        }
    
        public void Init(EventBean[] newEvents)
        {
            UpdateInternal(newEvents, null, false);
        }
    
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            UpdateInternal(newData, oldData, true);
        }
    
        private void UpdateInternal(EventBean[] newData, EventBean[] oldData, bool postOutput)
        {
            if (_isIterateOnly)
            {
                if (oldData != null)
                {
                    _regexPartitionStateRepo.RemoveOld(oldData, false, new bool[oldData.Length]);
                }
                if (newData != null)
                {
                    foreach (var newEvent in newData)
                    {
                        var partitionState = _regexPartitionStateRepo.GetState(newEvent, true);
                        if ((partitionState != null) && (partitionState.RandomAccess != null))
                        {
                            partitionState.RandomAccess.NewEventPrepare(newEvent);
                        }
                    }
                }            
                return;
            }
    
            if (oldData != null)
            {
                var isOutOfSequenceRemove = false;
    
                EventBean first = null;
                if (!_windowMatchedEventset.IsEmpty())
                {
                    first = _windowMatchedEventset.First();
                }
    
                // remove old data, if found in set
                var found = new bool[oldData.Length];
                var count = 0;
    
                // detect out-of-sequence removes
                foreach (var oldEvent in oldData)
                {
                    var removed = _windowMatchedEventset.Remove(oldEvent);
                    if (removed)
                    {
                        if ((oldEvent != first) && (first != null))
                        {
                            isOutOfSequenceRemove = true;
                        }
                        found[count++] = true;
                        if (!_windowMatchedEventset.IsEmpty())
                        {
                            first = _windowMatchedEventset.First();
                        }
                    }
                }
    
                // remove old events from repository - and let the repository know there are no interesting events left
                _regexPartitionStateRepo.RemoveOld(oldData, _windowMatchedEventset.IsEmpty(), found);
    
                // reset, rebuilding state
                if (isOutOfSequenceRemove)
                {
                    _regexPartitionStateRepo = _regexPartitionStateRepo.CopyForIterate();
                    _windowMatchedEventset = new LinkedHashSet<EventBean>();
                    IEnumerator<EventBean> parentEvents = Parent.GetEnumerator();
                    var iteratorResult = ProcessIterator(_startStates, parentEvents, _regexPartitionStateRepo);
                    _eventSequenceNumber = iteratorResult.EventSequenceNum;
                }
            }
    
            if (newData == null)
            {
                return;
            }
            
            IList<RegexNFAStateEntry> endStates = new List<RegexNFAStateEntry>();
            IList<RegexNFAStateEntry> nextStates = new List<RegexNFAStateEntry>();
            IList<RegexNFAStateEntry> terminationStatesAll = null;
    
            foreach (var newEvent in newData)
            {
                _eventSequenceNumber++;
    
                // get state holder for this event
                var partitionState = _regexPartitionStateRepo.GetState(newEvent, true);
                var currentStates = partitionState.CurrentStates;
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegEx(newEvent, partitionState);}
    
                // add start states for each new event
                foreach (var startState in _startStates)
                {
                    long time = 0;
                    if (_matchRecognizeSpec.Interval != null)
                    {
                        time = _agentInstanceContext.StatementContext.SchedulingService.Time;
                    }
                    currentStates.Add(new RegexNFAStateEntry(_eventSequenceNumber, time, startState, new EventBean[_numEventsEventsPerStreamDefine], new int[_allStates.Length], null, partitionState.OptionalKeys));
                }
    
                if (partitionState.RandomAccess != null)
                {
                    partitionState.RandomAccess.NewEventPrepare(newEvent);
                }
    
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluating event " + newEvent.Underlying + "\n" +
                        "current : " + PrintStates(currentStates));
                }
    
                var terminationStates = Step(currentStates, newEvent, nextStates, endStates, !_isUnbound, _eventSequenceNumber, partitionState.OptionalKeys);
    
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluated event " + newEvent.Underlying + "\n" +
                        "next : " + PrintStates(nextStates) + "\n" +
                        "end : " + PrintStates(endStates));
                }
    
                // add termination states, for use with interval and "or terminated"
                if (terminationStates != null) {
                    if (terminationStatesAll == null) {
                        terminationStatesAll = terminationStates;
                    }
                    else {
                        terminationStatesAll.AddAll(terminationStates);
                    }
                }
    
                partitionState.CurrentStates = nextStates;
                nextStates = currentStates;
                nextStates.Clear();
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegEx(partitionState, endStates, terminationStates);}
            }
    
            if (endStates.IsEmpty() && (!_isOrTerminated || terminationStatesAll == null))
            {
                return;
            }
    
            // perform inter-ranking and elimination of duplicate matches
            if (!_matchRecognizeSpec.IsAllMatches)
            {
                endStates = RankEndStatesMultiPartition(endStates);
            }
    
            // handle interval for the set of matches
            if (_matchRecognizeSpec.Interval != null)
            {
                for (int ii = 0; ii < endStates.Count; ii++)
                {
                    RegexNFAStateEntry endState = endStates[ii];

                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegIntervalState(endState, _variableStreams, _multimatchStreamNumToVariable, _agentInstanceContext.StatementContext.SchedulingService.Time); }
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null)
                    {
                        Log.Warn("Null partition state encountered, skipping row");
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalState(false); }
                        continue;
                    }

                    // determine whether to schedule
                    bool scheduleDelivery;
                    if (!_isOrTerminated)
                    {
                        scheduleDelivery = true;
                    }
                    else
                    {
                        // determine whether there can be more matches
                        if (endState.State.NextStates.Count == 1 &&
                            endState.State.NextStates[0] is RegexNFAStateEnd)
                        {
                            scheduleDelivery = false;
                        }
                        else
                        {
                            scheduleDelivery = true;
                        }
                    }

                    // only schedule if not an end-state or not or-terminated
                    if (scheduleDelivery)
                    {
                        var matchBeginTime = endState.MatchBeginEventTime;
                        var current = _agentInstanceContext.StatementContext.SchedulingService.Time;
                        var deltaFromStart = current - matchBeginTime;
                        var deltaUntil =
                            _matchRecognizeSpec.Interval.GetScheduleForwardDelta(current, _agentInstanceContext) -
                            deltaFromStart;

                        if (_schedule.ContainsKey(matchBeginTime))
                        {
                            ScheduleCallback(deltaUntil, endState);
                            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalState(true); }
                            endStates.RemoveAt(ii--);
                        }
                        else
                        {
                            if (deltaFromStart < deltaUntil)
                            {
                                ScheduleCallback(deltaUntil, endState);
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalState(true); }
                                endStates.RemoveAt(ii--);
                            }
                            else
                            {
                                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalState(false); }
                            }
                        }
                    }
                    else
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegIntervalState(false); }
                    }
                }

                // handle termination states - those that terminated the pattern and remove the callback
                if (_isOrTerminated && terminationStatesAll != null) {
                    foreach (var terminationState in terminationStatesAll)
                    {
                        var partitionState = _regexPartitionStateRepo.GetState(terminationState.PartitionKey);
                        if (partitionState == null) {
                            Log.Warn("Null partition state encountered, skipping row");
                            continue;
                        }
    
                        RemoveScheduleAddEndState(terminationState, endStates);
                    }
    
                    // rank
                    if (!_matchRecognizeSpec.IsAllMatches) {
                        endStates = RankEndStatesMultiPartition(endStates);
                    }
                }
    
                if (endStates.IsEmpty())
                {
                    return;
                }
            }
            // handle skip for incremental mode
            else if (_matchRecognizeSpec.Skip.Skip == MatchRecognizeSkipEnum.PAST_LAST_ROW)
            {
                IEnumerator<RegexNFAStateEntry> endStateIter = endStates.GetEnumerator();
                while(endStateIter.MoveNext())
                {
                    RegexNFAStateEntry endState = endStateIter.Current;
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null)
                    {
                        Log.Warn("Null partition state encountered, skipping row");
                        continue;
                    }

                    var regexNfaStateEntries = partitionState.CurrentStates;
                    for (int ii = 0; ii < regexNfaStateEntries.Count; ii++)
                    {
                        var currentState = regexNfaStateEntries[ii];
                        if (currentState.MatchBeginEventSeqNo <= endState.MatchEndEventSeqNo)
                        {
                            regexNfaStateEntries.RemoveAt(ii--);
                        }
                    }
                }
            }
            else if (_matchRecognizeSpec.Skip.Skip == MatchRecognizeSkipEnum.TO_NEXT_ROW)
            {
                IEnumerator<RegexNFAStateEntry> endStateIter = endStates.GetEnumerator();
                while(endStateIter.MoveNext())
                {
                    RegexNFAStateEntry endState = endStateIter.Current;
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null)
                    {
                        Log.Warn("Null partition state encountered, skipping row");
                        continue;
                    }

                    var regexNfaStateEntries = partitionState.CurrentStates;
                    for (int ii = 0; ii < regexNfaStateEntries.Count; ii++)
                    {
                        var currentState = regexNfaStateEntries[ii];
                        if (currentState.MatchBeginEventSeqNo <= endState.MatchBeginEventSeqNo)
                        {
                            regexNfaStateEntries.RemoveAt(ii--);
                        }
                    }
                }
            }
    
            var outBeans = new EventBean[endStates.Count];
            var countX = 0;
            foreach (var endState in endStates)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegMeasure(endState, _variableStreams, _multimatchStreamNumToVariable);}
                outBeans[countX] = GenerateOutputRow(endState);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegMeasure(outBeans[countX]);}
                countX++;
    
                // check partition state - if empty delete (no states and no random access)
                if (endState.PartitionKey != null) {
                    var state = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (state.CurrentStates.IsEmpty() && state.RandomAccess == null) {
                        _regexPartitionStateRepo.RemoveState(endState.PartitionKey);
                    }
                }
            }
    
            if (postOutput) {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegOut(outBeans);}
                UpdateChildren(outBeans, null);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegOut();}
            }
        }
    
        private RegexNFAStateEntry RankEndStates(IList<RegexNFAStateEntry> endStates)
        {
            // sort by end-event descending (newest first)
            endStates.SortInPlace(EventRowRegexHelper.END_STATE_COMPARATOR);
    
            // find the earliest begin-event
            RegexNFAStateEntry found = null;
            int min = int.MaxValue;
            var multipleMinimums = false;
            foreach (var state in endStates)
            {
                if (state.MatchBeginEventSeqNo < min)
                {
                    found = state;
                    min = state.MatchBeginEventSeqNo;
                }
                else if (state.MatchBeginEventSeqNo == min)
                {
                    multipleMinimums = true;
                }
            }
    
            if (!multipleMinimums)
            {
                Collections.SingletonList(found);
            }
    
            // compare greedy counts
            int[] best = null;
            found = null;
            foreach (var state in endStates)
            {
                if (state.MatchBeginEventSeqNo != min)
                {
                    continue;
                }
                if (best == null)
                {
                    best = state.GreedyCountPerState;
                    found = state;
                }
                else
                {
                    int[] current = state.GreedyCountPerState;
                    if (Compare(current, best))
                    {
                        best = current;
                        found = state;
                    }
                }
            }
    
            return found;
        }
    
        private bool Compare(int[] current, int[] best)
        {
            foreach (var state in _allStates)
            {
                if (state.IsGreedy == null)
                {
                    continue;
                }
                if (state.IsGreedy.Value)
                {
                    if (current[state.NodeNumFlat] > best[state.NodeNumFlat])
                    {
                        return true;
                    }
                } else
                {
                    if (current[state.NodeNumFlat] < best[state.NodeNumFlat])
                    {
                        return true;
                    }
                }
            }
    
            return false;
        }

        private EventRowRegexIteratorResult ProcessIterator(
            RegexNFAState[] startStates,
            IEnumerator<EventBean> events,
            RegexPartitionStateRepo regexPartitionStateRepo)
        {
            IList<RegexNFAStateEntry> endStates = new List<RegexNFAStateEntry>();
            IList<RegexNFAStateEntry> nextStates = new List<RegexNFAStateEntry>();
            IList<RegexNFAStateEntry> currentStates;
            var eventSequenceNumber = 0;
    
            EventBean theEvent;
            while (events.MoveNext())
            {
                theEvent = events.Current;
                eventSequenceNumber++;
    
                var partitionState = regexPartitionStateRepo.GetState(theEvent, false);
                currentStates = partitionState.CurrentStates;
    
                // add start states for each new event
                foreach (var startState in startStates)
                {
                    long time = 0;
                    if (_matchRecognizeSpec.Interval != null)
                    {
                        time = _agentInstanceContext.StatementContext.SchedulingService.Time;
                    }
                    currentStates.Add(new RegexNFAStateEntry(eventSequenceNumber, time, startState, new EventBean[_numEventsEventsPerStreamDefine], new int[_allStates.Length], null, partitionState.OptionalKeys));
                }
    
                if (partitionState.RandomAccess != null)
                {
                    partitionState.RandomAccess.ExistingEventPrepare(theEvent);
                }
    
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluating event " + theEvent.Underlying + "\n" +
                        "current : " + PrintStates(currentStates));
                }
    
                Step(currentStates, theEvent, nextStates, endStates, false, eventSequenceNumber, partitionState.OptionalKeys);
    
                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluating event " + theEvent.Underlying + "\n" +
                        "next : " + PrintStates(nextStates) + "\n" +
                        "end : " + PrintStates(endStates));
                }
    
                partitionState.CurrentStates = nextStates;
                nextStates = currentStates;
                nextStates.Clear();
            }
    
            return new EventRowRegexIteratorResult(endStates, eventSequenceNumber);
        }

        public override EventType EventType
        {
            get { return _rowEventType; }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_isUnbound)
            {
                return NULL_ITERATOR;
            }
    
            IEnumerator<EventBean> it = Parent.GetEnumerator();
    
            var regexPartitionStateRepoNew = _regexPartitionStateRepo.CopyForIterate();
    
            var iteratorResult = ProcessIterator(_startStates, it, regexPartitionStateRepoNew);
            var endStates = iteratorResult.EndStates;
            if (endStates.IsEmpty())
            {
                return NULL_ITERATOR;
            }
            else
            {
                endStates = RankEndStatesMultiPartition(endStates);
            }
    
            var output = endStates.Select(GenerateOutputRow).ToList();
            return output.GetEnumerator();
        }
    
        public void Accept(EventRowRegexNFAViewServiceVisitor visitor)
        {
            _regexPartitionStateRepo.Accept(visitor);
        }
    
        private IList<RegexNFAStateEntry> RankEndStatesMultiPartition(IList<RegexNFAStateEntry> endStates)
        {
            if (endStates.IsEmpty())
            {
                return endStates;
            }
            if (endStates.Count == 1)
            {
                return endStates;
            }
    
            // unpartitioned case -
            if (_matchRecognizeSpec.PartitionByExpressions.IsEmpty())
            {
                return RankEndStatesWithinPartitionByStart(endStates);
            }
    
            // partitioned case - structure end states by partition
            IDictionary<object, object> perPartition = new LinkedHashMap<object, object>();
            foreach (var endState in endStates)
            {
                var value = perPartition.Get(endState.PartitionKey);
                if (value == null)
                {
                    perPartition.Put(endState.PartitionKey, endState);
                }
                else if (value is IList<RegexNFAStateEntry>)
                {
                    var entries = (IList<RegexNFAStateEntry>) value;
                    entries.Add(endState);
                }
                else
                {
                    IList<RegexNFAStateEntry> entries = new List<RegexNFAStateEntry>();
                    entries.Add((RegexNFAStateEntry) value);
                    entries.Add(endState);
                    perPartition.Put(endState.PartitionKey, entries);
                }
            }
    
            IList<RegexNFAStateEntry> finalEndStates = new List<RegexNFAStateEntry>();
            foreach (var entry in perPartition)
            {
                if (entry.Value is RegexNFAStateEntry)
                {
                    finalEndStates.Add((RegexNFAStateEntry) entry.Value);
                }
                else
                {
                    var entries = (IList<RegexNFAStateEntry>) entry.Value;
                    finalEndStates.AddAll(RankEndStatesWithinPartitionByStart(entries));
                }            
            }
            return finalEndStates;
        }
    
        private IList<RegexNFAStateEntry> RankEndStatesWithinPartitionByStart(IList<RegexNFAStateEntry> endStates) {
            if (endStates.IsEmpty())
            {
                return endStates;
            }
            if (endStates.Count == 1)
            {
                return endStates;
            }
    
            var endStatesPerBeginEvent = new SortedDictionary<int, object>();
            foreach (var entry in endStates)
            {
                int beginNum = entry.MatchBeginEventSeqNo;
                var value = endStatesPerBeginEvent.Get(beginNum);
                if (value == null)
                {
                    endStatesPerBeginEvent.Put(beginNum, entry);
                }
                else if (value is IList<RegexNFAStateEntry>)
                {
                    var entries = (IList<RegexNFAStateEntry>) value;
                    entries.Add(entry);
                }
                else
                {
                    IList<RegexNFAStateEntry> entries = new List<RegexNFAStateEntry>();
                    entries.Add((RegexNFAStateEntry) value);
                    entries.Add(entry);
                    endStatesPerBeginEvent.Put(beginNum, entries);
                }
            }
    
            if (endStatesPerBeginEvent.Count == 1)
            {
                var endStatesUnranked = (IList<RegexNFAStateEntry>) endStatesPerBeginEvent.Values.First();
                if (_matchRecognizeSpec.IsAllMatches)
                {
                    return endStatesUnranked;
                }
                var chosen = RankEndStates(endStatesUnranked);
                return Collections.SingletonList(chosen);
            }
    
            IList<RegexNFAStateEntry> endStatesRanked = new List<RegexNFAStateEntry>();
            ICollection<int> keyset = endStatesPerBeginEvent.Keys;
            var keys = keyset.ToArray();
            foreach (var key in keys)
            {
                var value = endStatesPerBeginEvent.Pluck(key);
                if (value == null)
                {
                    continue;
                }
    
                RegexNFAStateEntry entryTaken;
                if (value is IList<RegexNFAStateEntry>)
                {
                    var endStatesUnranked = (IList<RegexNFAStateEntry>) value;
                    if (endStatesUnranked.IsEmpty())
                    {
                        continue;
                    }
                    entryTaken = RankEndStates(endStatesUnranked);
    
                    if (_matchRecognizeSpec.IsAllMatches)
                    {
                        endStatesRanked.AddAll(endStatesUnranked);  // we take all matches and don't rank except to determine skip-past
                    }
                    else
                    {
                        endStatesRanked.Add(entryTaken);
                    }
                }
                else
                {
                    entryTaken = (RegexNFAStateEntry) value;
                    endStatesRanked.Add(entryTaken);
                }
                // could be null as removals take place
    
                if (entryTaken != null)
                {
                    if (_matchRecognizeSpec.Skip.Skip == MatchRecognizeSkipEnum.PAST_LAST_ROW)
                    {
                        var skipPastRow = entryTaken.MatchEndEventSeqNo;
                        RemoveSkippedEndStates(endStatesPerBeginEvent, skipPastRow);
                    }
                    else if (_matchRecognizeSpec.Skip.Skip == MatchRecognizeSkipEnum.TO_NEXT_ROW)
                    {
                        var skipPastRow = entryTaken.MatchBeginEventSeqNo;
                        RemoveSkippedEndStates(endStatesPerBeginEvent, skipPastRow);
                    }
                }
            }
    
            return endStatesRanked;
        }
    
        private void RemoveSkippedEndStates(IDictionary<int, object> endStatesPerEndEvent, int skipPastRow)
        {
            foreach (var entry in endStatesPerEndEvent.ToList())
            {
                var value = entry.Value;
                if (value is IList<RegexNFAStateEntry>)
                {
                    var endStatesUnranked = (IList<RegexNFAStateEntry>) value;

                    endStatesUnranked.RemoveWhere(
                        endState => endState.MatchBeginEventSeqNo <= skipPastRow);
                }
                else
                {
                    var endState = (RegexNFAStateEntry) value;
                    if (endState.MatchBeginEventSeqNo <= skipPastRow)
                    {
                        endStatesPerEndEvent.Put(entry.Key, null);
                    }
                }
            }
        }

        private IList<RegexNFAStateEntry> Step(
            IList<RegexNFAStateEntry> currentStates,
            EventBean theEvent,
            IList<RegexNFAStateEntry> nextStates,
            IList<RegexNFAStateEntry> endStates,
            bool isRetainEventSet,
            int currentEventSequenceNumber,
            object partitionKey)
        {
            IList<RegexNFAStateEntry> terminationStates = null;  // always null or a list of entries (no singleton list)
    
            foreach (var currentState in currentStates)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegExState(currentState, _variableStreams, _multimatchStreamNumToVariable);}
    
                var eventsPerStream = currentState.EventsPerStream;
                var currentStateStreamNum = currentState.State.StreamNum;
                eventsPerStream[currentStateStreamNum] = theEvent;
                if (_isDefineAsksMultimatches) {
                    eventsPerStream[_numEventsEventsPerStreamDefine-1] = GetMultimatchState(currentState);
                }
    
                if (currentState.State.Matches(eventsPerStream, _agentInstanceContext))
                {
                    if (isRetainEventSet)
                    {
                        _windowMatchedEventset.Add(theEvent);
                    }
                    var nextStatesFromHere = currentState.State.NextStates;
    
                    // save state for each next state
                    var copy = nextStatesFromHere.Count > 1;
                    foreach (var next in nextStatesFromHere)
                    {
                        var eventsForState = eventsPerStream;
                        var multimatches = currentState.OptionalMultiMatches;
                        int[] greedyCounts = currentState.GreedyCountPerState;
    
                        if (copy)
                        {
                            eventsForState = new EventBean[eventsForState.Length];
                            Array.Copy(eventsPerStream, 0, eventsForState, 0, eventsForState.Length);
    
                            var greedyCountsCopy = new int[greedyCounts.Length];
                            Array.Copy(greedyCounts, 0, greedyCountsCopy, 0, greedyCounts.Length);
                            greedyCounts = greedyCountsCopy;
    
                            if (_isCollectMultimatches) {
                                multimatches = DeepCopy(multimatches);
                            }
                        }
    
                        if ((_isCollectMultimatches) && (currentState.State.IsMultiple))
                        {
                            multimatches = AddTag(currentState.State.StreamNum, theEvent, multimatches);
                        }
    
                        if ((currentState.State.IsGreedy != null) && (currentState.State.IsGreedy.Value))
                        {
                            greedyCounts[currentState.State.NodeNumFlat]++;
                        }
    
                        var entry = new RegexNFAStateEntry(currentState.MatchBeginEventSeqNo, currentState.MatchBeginEventTime, currentState.State, eventsForState, greedyCounts, multimatches, partitionKey);
                        if (next is RegexNFAStateEnd)
                        {
                            entry.MatchEndEventSeqNo = currentEventSequenceNumber;
                            endStates.Add(entry);
                        }
                        else
                        {
                            entry.State = next;
                            nextStates.Add(entry);
                        }
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExState(nextStates, _variableStreams, _multimatchStreamNumToVariable);}
                }
                // when not-matches
                else {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExState(Collections.GetEmptyList<RegexNFAStateEntry>(), _variableStreams, _multimatchStreamNumToVariable);}
    
                    // determine interval and or-terminated
                    if (_isOrTerminated) {
                        eventsPerStream[currentStateStreamNum] = null;  // deassign
                        var nextStatesFromHere = currentState.State.NextStates;
    
                        // save state for each next state
                        RegexNFAState theEndState = null;
                        foreach (var next in nextStatesFromHere) {
                            if (next is RegexNFAStateEnd) {
                                theEndState = next;
                            }
                        }
                        if (theEndState != null) {
                            var entry = new RegexNFAStateEntry(currentState.MatchBeginEventSeqNo, currentState.MatchBeginEventTime, theEndState, eventsPerStream, currentState.GreedyCountPerState, currentState.OptionalMultiMatches, partitionKey);
                            if (terminationStates == null) {
                                terminationStates = new List<RegexNFAStateEntry>();
                            }
                            terminationStates.Add(entry);
                        }
                    }
                }
            }
    
            return terminationStates;   // only for immediate use, not for scheduled use as no copy of state
        }
    
        private ObjectArrayBackedEventBean GetMultimatchState(RegexNFAStateEntry currentState)
        {
            if (currentState.OptionalMultiMatches == null || !currentState.State.IsExprRequiresMultimatchState)
            {
                return null;
            }
            var props = _defineMultimatchEventBean.Properties;
            var states = currentState.OptionalMultiMatches;
            for (var i = 0; i < props.Length; i++) {
                var state = states[i];
                if (state == null) {
                    props[i] = null;
                }
                else {
                    props[i] = state.GetShrinkEventArray();
                }
            }
            return _defineMultimatchEventBean;
        }
    
        private MultimatchState[] DeepCopy(MultimatchState[] multimatchStates) {
            if (multimatchStates == null) {
                return null;
            }
    
            var copy = new MultimatchState[multimatchStates.Length];
            for (var i = 0; i < copy.Length; i++) {
                if (multimatchStates[i] != null) {
                    copy[i] = new MultimatchState(multimatchStates[i]);
                }
            }
    
            return copy;
        }
    
        private MultimatchState[] AddTag(int streamNum, EventBean theEvent, MultimatchState[] multimatches)
        {
            if (multimatches == null) {
                multimatches = new MultimatchState[_multimatchVariablesArray.Length];
            }
    
            var index = _multimatchStreamNumToVariable[streamNum];
            var state = multimatches[index];
            if (state == null) {
                multimatches[index] = new MultimatchState(theEvent);
                return multimatches;
            }
    
            multimatches[index].Add(theEvent);
            return multimatches;
        }
    
        private string PrintStates(IList<RegexNFAStateEntry> states)
        {
            var buf = new StringBuilder();
            var delimiter = "";
            foreach (var state in states)
            {
                buf.Append(delimiter);
                buf.Append(state.State.NodeNumNested);
    
                buf.Append("{");
                var eventsPerStream = state.EventsPerStream;
                if (eventsPerStream == null)
                {
                    buf.Append("null");
                }
                else
                {
                    var eventDelimiter = "";
                    foreach (var streamVariable in _streamsVariables)
                    {
                        buf.Append(eventDelimiter);
                        buf.Append(streamVariable.Value);
                        buf.Append('=');
                        var single = !_variableStreams.Get(streamVariable.Value).Second;
                        if (single)
                        {
                            if (eventsPerStream[streamVariable.Key] == null)
                            {
                                buf.Append("null");
                            }
                            else
                            {
                                buf.Append(eventsPerStream[streamVariable.Key].Underlying);
                            }
                        }
                        else
                        {
                            var streamNum = state.State.StreamNum;
                            var index = _multimatchStreamNumToVariable[streamNum];
                            if (state.OptionalMultiMatches == null) {
                                buf.Append("null-mm");
                            }
                            else if (state.OptionalMultiMatches[index] == null) {
                                buf.Append("no-entry");
                            }
                            else
                            {
                                buf.Append("{");
                                var arrayEventDelimiter = "";
                                var multiMatch = state.OptionalMultiMatches[index].Buffer;
                                var count = state.OptionalMultiMatches[index].Count;
                                for (var i = 0; i < count; i++)
                                {
                                    buf.Append(arrayEventDelimiter);
                                    buf.Append(multiMatch[i].Underlying);
                                    arrayEventDelimiter = ", ";
                                }
                                buf.Append("}");
                            }
                        }
                        eventDelimiter = ", ";
                    }
                }
                buf.Append("}");
    
                delimiter = ", ";
            }
            return buf.ToString();
        }
    
        private string Print(RegexNFAState[] states)
        {
            var writer = new StringWriter();
            var currentStack = new Stack<RegexNFAState>();
            Print(states, writer, 0, currentStack);
            return writer.ToString();
        }
    
        private void Print(IList<RegexNFAState> states, TextWriter writer, int indent, Stack<RegexNFAState> currentStack)
        {
            foreach (var state in states)
            {
                Indent(writer, indent);
                if (currentStack.Contains(state))
                {
                    writer.WriteLine("(self)");
                }
                else
                {
                    writer.WriteLine(PrintState(state));
    
                    currentStack.Push(state);
                    Print(state.NextStates, writer, indent + 4, currentStack);
                    currentStack.Pop();
                }
            }
        }
    
        private string PrintState(RegexNFAState state)
        {
            if (state is RegexNFAStateEnd)
            {
                return "#" + state.NodeNumNested;
            }
            else
            {
                return "#" + state.NodeNumNested + " " + state.VariableName + " s" + state.StreamNum + " defined as " + state;
            }
        }
    
        private void Indent(TextWriter writer, int indent)
        {
            for (var i = 0; i < indent; i++)
            {
                writer.Write(' ');
            }
        }
        
        private EventBean GenerateOutputRow(RegexNFAStateEntry entry)
        {
            var rowDataRaw = _compositeEventBean.Properties;
    
            // we first generate a raw row of <string, object> for each variable name.
            foreach (var variableDef in _variableStreams)
            {
                if (!variableDef.Value.Second) {
                    int index = variableDef.Value.First;
                    rowDataRaw[index] = entry.EventsPerStream[index];
                }
            }
            if (_aggregationService != null)
            {
                _aggregationService.ClearResults();
            }
            if (entry.OptionalMultiMatches != null)
            {
                var multimatchState = entry.OptionalMultiMatches;
                for (var i = 0; i < multimatchState.Length; i++)
                {
                    if (multimatchState[i] == null) {
                        rowDataRaw[_multimatchVariableToStreamNum[i]] = null;
                        continue;
                    }
                    EventBean[] multimatchEvents = multimatchState[i].GetShrinkEventArray();
                    rowDataRaw[_multimatchVariableToStreamNum[i]] = multimatchEvents;
    
                    if (_aggregationService != null) {
                        var eventsPerStream = entry.EventsPerStream;
                        var streamNum = _multimatchVariableToStreamNum[i];
    
                        foreach (var multimatchEvent in multimatchEvents) {
                            eventsPerStream[streamNum] = multimatchEvent;
                            _aggregationService.ApplyEnter(eventsPerStream, streamNum, _agentInstanceContext);
                        }
                    }
                }
            }
            else {
                foreach (var index in _multimatchVariableToStreamNum) {
                    rowDataRaw[index] = null;
                }
            }
    
            IDictionary<string, object> row = new Dictionary<string, object>();
            var columnNum = 0;
            var eventsPerStreamX = new EventBean[1];
            foreach (var expression in _columnEvaluators)
            {
                eventsPerStreamX[0] = _compositeEventBean;
                var result = expression.Evaluate(new EvaluateParams(eventsPerStreamX, true, _agentInstanceContext));
                row.Put(_columnNames[columnNum], result);
                columnNum++;
            }
    
            return _agentInstanceContext.StatementContext.EventAdapterService.AdapterForTypedMap(row, _rowEventType);
        }
    
        private void ScheduleCallback(long msecAfterCurrentTime, RegexNFAStateEntry endState)
        {
            var matchBeginTime = endState.MatchBeginEventTime;
            if (_schedule.IsEmpty())
            {
                _schedule.Put(matchBeginTime, endState);
                _agentInstanceContext.StatementContext.SchedulingService.Add(msecAfterCurrentTime, _handle, _scheduleSlot);
            }
            else
            {
                var value = _schedule.Get(matchBeginTime);
                if (value == null)
                {
                    long currentFirstKey = _schedule.Keys.First();
                    if (currentFirstKey > matchBeginTime)
                    {
                        _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                        _agentInstanceContext.StatementContext.SchedulingService.Add(msecAfterCurrentTime, _handle, _scheduleSlot);
                    }
    
                    _schedule.Put(matchBeginTime, endState);
                }
                else if (value is RegexNFAStateEntry)
                {
                    var valueEntry = (RegexNFAStateEntry) value;
                    IList<RegexNFAStateEntry> list = new List<RegexNFAStateEntry>();
                    list.Add(valueEntry);
                    list.Add(endState);
                    _schedule.Put(matchBeginTime, list);
                }
                else
                {
                    var list = (IList<RegexNFAStateEntry>) value;
                    list.Add(endState);
                }
            }
        }
    
        private void RemoveScheduleAddEndState(RegexNFAStateEntry terminationState, IList<RegexNFAStateEntry> foundEndStates) {
            var matchBeginTime = terminationState.MatchBeginEventTime;
            var value = _schedule.Get(matchBeginTime);
            if (value == null) {
                return;
            }
            if (value is RegexNFAStateEntry) {
                var single = (RegexNFAStateEntry) value;
                if (CompareTerminationStateToEndState(terminationState, single)) {
                    _schedule.Remove(matchBeginTime);
                    if (_schedule.IsEmpty()) {
                        // we do not reschedule and accept a wasted schedule check
                        _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                    }
                    foundEndStates.Add(single);
                }
            }
            else {
                var entries = (IList<RegexNFAStateEntry>) value;

                for (int ii = 0; ii < entries.Count; ii++)
                {
                    var endState = entries[ii];
                    if (CompareTerminationStateToEndState(terminationState, endState))
                    {
                        entries.RemoveAt(ii--);
                        foundEndStates.Add(endState);
                    }
                }

                if (entries.IsEmpty()) {
                    _schedule.Remove(matchBeginTime);
                    if (_schedule.IsEmpty()) {
                        // we do not reschedule and accept a wasted schedule check
                        _agentInstanceContext.StatementContext.SchedulingService.Remove(_handle, _scheduleSlot);
                    }
                }
            }
        }
    
        // End-state may have less events then the termination state
        private bool CompareTerminationStateToEndState(RegexNFAStateEntry terminationState, RegexNFAStateEntry endState)
        {
            if (terminationState.MatchBeginEventSeqNo != endState.MatchBeginEventSeqNo) {
                return false;
            }
            foreach (var entry in _variableStreams) {
                var stream = entry.Value.First;
                var multi = entry.Value.Second;
                if (multi) {
                    var termStreamEvents = GetMultimatchArray(terminationState, stream);
                    var endStreamEvents = GetMultimatchArray(endState, stream);
                    if (endStreamEvents != null) {
                        if (termStreamEvents == null) {
                            return false;
                        }
                        for (var i = 0; i < endStreamEvents.Length; i++) {
                            if (termStreamEvents.Length > i && endStreamEvents[i] != termStreamEvents[i]) {
                                return false;
                            }
                        }
                    }
                }
                else {
                    var termStreamEvent = terminationState.EventsPerStream[stream];
                    var endStreamEvent = endState.EventsPerStream[stream];
                    if (endStreamEvent != null && endStreamEvent != termStreamEvent) {
                        return false;
                    }
                }
            }
            return true;
        }
    
        private EventBean[] GetMultimatchArray(RegexNFAStateEntry state, int stream) {
            if (state.OptionalMultiMatches == null) {
                return null;
            }
            var index = _multimatchStreamNumToVariable[stream];
            var multiMatches = state.OptionalMultiMatches[index];
            if (multiMatches == null) {
                return null;
            }
            return multiMatches.GetShrinkEventArray();
        }
    
        private void Triggered()
        {
            var currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            long intervalMSec = _matchRecognizeSpec.Interval.GetScheduleBackwardDelta(currentTime, _agentInstanceContext);
            if (_schedule.IsEmpty()) {
                return;
            }
    
            IList<RegexNFAStateEntry> indicatables = new List<RegexNFAStateEntry>();
            while (true)
            {
                var firstKey = _schedule.Keys.First();
                var cutOffTime = currentTime - intervalMSec;
                if (firstKey > cutOffTime)
                {
                    break;
                }
    
                var value = _schedule.Pluck(firstKey);
                if (value is RegexNFAStateEntry)
                {
                    indicatables.Add((RegexNFAStateEntry) value);
                }
                else
                {
                    var list = (IList<RegexNFAStateEntry>) value;
                    indicatables.AddAll(list);
                }
    
                if (_schedule.IsEmpty())
                {
                    break;
                }
            }
    
            // schedule next
            if (!_schedule.IsEmpty())
            {
                var msecAfterCurrentTime = _schedule.Keys.First() + intervalMSec - _agentInstanceContext.StatementContext.SchedulingService.Time;
                _agentInstanceContext.StatementContext.SchedulingService.Add(msecAfterCurrentTime, _handle, _scheduleSlot);
            }
    
            if (!_matchRecognizeSpec.IsAllMatches)
            {
                indicatables = RankEndStatesMultiPartition(indicatables);
            }
    
            var outBeans = new EventBean[indicatables.Count];
            var count = 0;
            foreach (var endState in indicatables)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegMeasure(endState, _variableStreams, _multimatchStreamNumToVariable);}
                outBeans[count] = GenerateOutputRow(endState);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegMeasure(outBeans[count]);}
                count++;
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegOut(outBeans);}
            UpdateChildren(outBeans, null);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegOut();}
        }

        public RegexExprPreviousEvalStrategy PreviousEvaluationStrategy
        {
            get { return _prevGetter; }
        }

        public EventRowRegexNFAViewFactory Factory
        {
            get { return _factory; }
        }
    }
}

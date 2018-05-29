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
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.events.arr;
using com.espertech.esper.metrics.instrumentation;
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
        , EventRowRegexNFAViewScheduleCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const bool IS_DEBUG = false;

        private static readonly IEnumerator<EventBean> NULL_ITERATOR =
            EnumerationHelper.Empty<EventBean>();

        private readonly EventRowRegexNFAViewFactory _factory;
        private readonly MatchRecognizeSpec _matchRecognizeSpec;
        private readonly bool _isUnbound;
        private readonly bool _isIterateOnly;
        private readonly bool _isCollectMultimatches;
        private readonly bool _isTrackMaxStates;

        private readonly EventType _rowEventType;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly AggregationServiceMatchRecognize _aggregationService;

        // for interval-handling
        private readonly EventRowRegexNFAViewScheduler _scheduler;
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

        private readonly RegexPartitionStateRandomAccessGetter _prevGetter;
        private readonly ObjectArrayBackedEventBean _compositeEventBean;

        // state
        private RegexPartitionStateRepo _regexPartitionStateRepo;
        private readonly LinkedHashSet<EventBean> _windowMatchedEventset; // this is NOT per partition - some optimizations are done for batch-processing (minus is out-of-sequence in partition) 

        /// <summary>
        /// Gets the regex partition state repo.
        /// </summary>
        /// <value>
        /// The regex partition state repo.
        /// </value>
        protected internal RegexPartitionStateRepo RegexPartitionStateRepo
        {
            get { return _regexPartitionStateRepo; }
        }

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
        /// <param name="expandedPatternNode">the expanded pattern node</param>
        /// <param name="matchRecognizeConfig">the match recognition configuration</param>
        /// <param name="scheduler">the scheduler</param>
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
            RowRegexExprNode expandedPatternNode,
            ConfigurationEngineDefaults.MatchRecognizeConfig matchRecognizeConfig,
            EventRowRegexNFAViewScheduler scheduler)
        {
            _factory = factory;
            _matchRecognizeSpec = matchRecognizeSpec;
            _isTrackMaxStates = matchRecognizeConfig != null && matchRecognizeConfig.MaxStates != null;
            _compositeEventBean = new ObjectArrayEventBean(new object[variableStreams.Count], compositeEventType);
            _rowEventType = rowEventType;
            _variableStreams = variableStreams;
            _scheduler = scheduler;

            // determine names of multimatching variables
            if (variablesSingle.Count == variableStreams.Count)
            {
                _multimatchVariablesArray = new string[0];
                _multimatchStreamNumToVariable = new int[0];
                _multimatchVariableToStreamNum = new int[0];
            }
            else
            {
                _multimatchVariablesArray = new string[variableStreams.Count - variablesSingle.Count];
                _multimatchVariableToStreamNum = new int[_multimatchVariablesArray.Length];
                _multimatchStreamNumToVariable = new int[variableStreams.Count];
                CompatExtensions.Fill(_multimatchStreamNumToVariable, -1);
                var count = 0;
                foreach (var entry in variableStreams)
                {
                    if (entry.Value.Second)
                    {
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
                agentInstanceContext.AddTerminationCallback(Stop);
                _isOrTerminated = matchRecognizeSpec.Interval.IsOrTerminated;
            }
            else
            {
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
                Log.Info("NFA tree:\n" + EventRowRegexNFAViewUtil.Print(_startStates));
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
            var repoFactory = agentInstanceContext.StatementContext.RegexPartitionStateRepoFactory;
            var terminationStateCompare = new RegexPartitionTerminationStateComparator(_multimatchStreamNumToVariable, variableStreams);
            if (_matchRecognizeSpec.PartitionByExpressions.IsEmpty())
            {
                _regexPartitionStateRepo = repoFactory.MakeSingle(_prevGetter, agentInstanceContext, this, matchRecognizeSpec.Interval != null, terminationStateCompare);
            }
            else
            {
                var stateRepoGroupMeta = new RegexPartitionStateRepoGroupMeta(matchRecognizeSpec.Interval != null,
                    ExprNodeUtility.ToArray(matchRecognizeSpec.PartitionByExpressions),
                    ExprNodeUtility.GetEvaluators(matchRecognizeSpec.PartitionByExpressions), agentInstanceContext);
                _regexPartitionStateRepo = repoFactory.MakePartitioned(_prevGetter, stateRepoGroupMeta, agentInstanceContext, this, matchRecognizeSpec.Interval != null, terminationStateCompare);
            }
        }

        public void Stop()
        {
            if (_scheduler != null)
            {
                _scheduler.RemoveSchedule();
            }
            if (_isTrackMaxStates)
            {
                int size = _regexPartitionStateRepo.StateCount;
                MatchRecognizeStatePoolStmtSvc poolSvc = _agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc;
                poolSvc.EngineSvc.DecreaseCount(_agentInstanceContext, size);
                poolSvc.StmtHandler.DecreaseCount(size);
            }

            _regexPartitionStateRepo.Dispose();
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

                // reset, rebuilding state
                if (isOutOfSequenceRemove)
                {
                    if (_isTrackMaxStates)
                    {
                        int size = _regexPartitionStateRepo.StateCount;
                        MatchRecognizeStatePoolStmtSvc poolSvc = _agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc;
                        poolSvc.EngineSvc.DecreaseCount(_agentInstanceContext, size);
                        poolSvc.StmtHandler.DecreaseCount(size);
                    }

                    _regexPartitionStateRepo = _regexPartitionStateRepo.CopyForIterate(true);
                    var parentEvents = Parent.GetEnumerator();
                    EventRowRegexIteratorResult iteratorResult = ProcessIterator(true, parentEvents, _regexPartitionStateRepo);
                    _regexPartitionStateRepo.EventSequenceNum = iteratorResult.EventSequenceNum;
                }
                else
                {
                    // remove old events from repository - and let the repository know there are no interesting events left
                    int numRemoved = _regexPartitionStateRepo.RemoveOld(oldData, _windowMatchedEventset.IsEmpty(), found);

                    if (_isTrackMaxStates)
                    {
                        MatchRecognizeStatePoolStmtSvc poolSvc = _agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc;
                        poolSvc.EngineSvc.DecreaseCount(_agentInstanceContext, numRemoved);
                        poolSvc.StmtHandler.DecreaseCount(numRemoved);
                    }
                }
            }

            if (newData == null)
            {
                return;
            }

            IList<RegexNFAStateEntry> endStates = new List<RegexNFAStateEntry>();
            IList<RegexNFAStateEntry> terminationStatesAll = null;

            foreach (var newEvent in newData)
            {
                var nextStates = new List<RegexNFAStateEntry>();
                int eventSequenceNumber = _regexPartitionStateRepo.IncrementAndGetEventSequenceNum();

                // get state holder for this event
                var partitionState = _regexPartitionStateRepo.GetState(newEvent, true);
                var currentStatesIterator = partitionState.CurrentStatesEnumerator;
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegEx(newEvent, partitionState); }

                if (partitionState.RandomAccess != null)
                {
                    partitionState.RandomAccess.NewEventPrepare(newEvent);
                }

                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluating event " + newEvent.Underlying + "\n" +
                        "current : " + EventRowRegexNFAViewUtil.PrintStates(partitionState.CurrentStatesForPrint, _streamsVariables, _variableStreams, _multimatchStreamNumToVariable));
                }

                var terminationStates = Step(false, currentStatesIterator, newEvent, nextStates, endStates, !_isUnbound, eventSequenceNumber, partitionState.OptionalKeys);

                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluated event " + newEvent.Underlying + "\n" +
                        "next : " + EventRowRegexNFAViewUtil.PrintStates(nextStates, _streamsVariables, _variableStreams, _multimatchStreamNumToVariable) + "\n" +
                        "end : " + EventRowRegexNFAViewUtil.PrintStates(endStates, _streamsVariables, _variableStreams, _multimatchStreamNumToVariable));
                }

                // add termination states, for use with interval and "or terminated"
                if (terminationStates != null)
                {
                    if (terminationStatesAll == null)
                    {
                        terminationStatesAll = terminationStates;
                    }
                    else
                    {
                        terminationStatesAll.AddAll(terminationStates);
                    }
                }

                partitionState.CurrentStates = nextStates;
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegEx(partitionState, endStates, terminationStates); }
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

                        if (_regexPartitionStateRepo.ScheduleState.ContainsKey(matchBeginTime))
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
                if (_isOrTerminated && terminationStatesAll != null)
                {
                    foreach (var terminationState in terminationStatesAll)
                    {
                        var partitionState = _regexPartitionStateRepo.GetState(terminationState.PartitionKey);
                        if (partitionState == null)
                        {
                            Log.Warn("Null partition state encountered, skipping row");
                            continue;
                        }

                        RemoveScheduleAddEndState(terminationState, endStates);
                    }

                    // rank
                    if (!_matchRecognizeSpec.IsAllMatches)
                    {
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
                var endStateIter = endStates.GetEnumerator();
                while (endStateIter.MoveNext())
                {
                    var endState = endStateIter.Current;
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null)
                    {
                        Log.Warn("Null partition state encountered, skipping row");
                        continue;
                    }

                    partitionState.CurrentStates.RemoveWhere(
                        state => state.MatchBeginEventSeqNo <= endState.MatchEndEventSeqNo);

                }
            }
            else if (_matchRecognizeSpec.Skip.Skip == MatchRecognizeSkipEnum.TO_NEXT_ROW)
            {
                var endStateIter = endStates.GetEnumerator();
                while (endStateIter.MoveNext())
                {
                    var endState = endStateIter.Current;
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null)
                    {
                        Log.Warn("Null partition state encountered, skipping row");
                        continue;
                    }

                    partitionState.CurrentStates.RemoveWhere(
                        state => state.MatchBeginEventSeqNo <= endState.MatchBeginEventSeqNo);
                }
            }

            var outBeans = new EventBean[endStates.Count];
            var countX = 0;
            foreach (var endState in endStates)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegMeasure(endState, _variableStreams, _multimatchStreamNumToVariable); }
                outBeans[countX] = GenerateOutputRow(endState);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegMeasure(outBeans[countX]); }
                countX++;

                // check partition state - if empty delete (no states and no random access)
                if (endState.PartitionKey != null)
                {
                    var state = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (state.IsEmptyCurrentState && state.RandomAccess == null)
                    {
                        _regexPartitionStateRepo.RemoveState(endState.PartitionKey);
                    }
                }
            }

            if (postOutput)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegOut(outBeans); }
                UpdateChildren(outBeans, null);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegOut(); }
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
                }
                else
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
            bool isOutOfSeqDelete,
            IEnumerator<EventBean> events,
            RegexPartitionStateRepo regexPartitionStateRepo)
        {
            IList<RegexNFAStateEntry> endStates = new List<RegexNFAStateEntry>();
            IEnumerator<RegexNFAStateEntry> currentStates;
            var eventSequenceNumber = 0;

            while (events.MoveNext())
            {
                var nextStates = new List<RegexNFAStateEntry>();
                var theEvent = events.Current;
                eventSequenceNumber++;

                var partitionState = regexPartitionStateRepo.GetState(theEvent, false);
                currentStates = partitionState.CurrentStatesEnumerator;

                if (partitionState.RandomAccess != null)
                {
                    partitionState.RandomAccess.ExistingEventPrepare(theEvent);
                }

                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluating event " + theEvent.Underlying + "\n" +
                        "current : " + EventRowRegexNFAViewUtil.PrintStates(partitionState.CurrentStatesForPrint, _streamsVariables, _variableStreams, _multimatchStreamNumToVariable));
                }

                Step(!isOutOfSeqDelete, currentStates, theEvent, nextStates, endStates, false, eventSequenceNumber, partitionState.OptionalKeys);

                if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled) || (IS_DEBUG))
                {
                    Log.Info("Evaluating event " + theEvent.Underlying + "\n" +
                        "next : " + EventRowRegexNFAViewUtil.PrintStates(nextStates, _streamsVariables, _variableStreams, _multimatchStreamNumToVariable) + "\n" +
                        "end : " + EventRowRegexNFAViewUtil.PrintStates(endStates, _streamsVariables, _variableStreams, _multimatchStreamNumToVariable));
                }

                partitionState.CurrentStates = nextStates;
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

            var regexPartitionStateRepoNew = _regexPartitionStateRepo.CopyForIterate(false);

            var iteratorResult = ProcessIterator(false, it, regexPartitionStateRepoNew);
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
                    var entries = (IList<RegexNFAStateEntry>)value;
                    entries.Add(endState);
                }
                else
                {
                    IList<RegexNFAStateEntry> entries = new List<RegexNFAStateEntry>();
                    entries.Add((RegexNFAStateEntry)value);
                    entries.Add(endState);
                    perPartition.Put(endState.PartitionKey, entries);
                }
            }

            List<RegexNFAStateEntry> finalEndStates = new List<RegexNFAStateEntry>();
            foreach (var entry in perPartition)
            {
                if (entry.Value is RegexNFAStateEntry)
                {
                    finalEndStates.Add((RegexNFAStateEntry)entry.Value);
                }
                else
                {
                    var entries = (IList<RegexNFAStateEntry>)entry.Value;
                    finalEndStates.AddAll(RankEndStatesWithinPartitionByStart(entries));
                }
            }
            return finalEndStates;
        }

        private IList<RegexNFAStateEntry> RankEndStatesWithinPartitionByStart(IList<RegexNFAStateEntry> endStates)
        {
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
                    var entries = (IList<RegexNFAStateEntry>)value;
                    entries.Add(entry);
                }
                else
                {
                    List<RegexNFAStateEntry> entries = new List<RegexNFAStateEntry>();
                    entries.Add((RegexNFAStateEntry)value);
                    entries.Add(entry);
                    endStatesPerBeginEvent.Put(beginNum, entries);
                }
            }

            if (endStatesPerBeginEvent.Count == 1)
            {
                var endStatesUnranked = (List<RegexNFAStateEntry>)endStatesPerBeginEvent.Values.First();
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
                var value = endStatesPerBeginEvent.Delete(key);
                if (value == null)
                {
                    continue;
                }

                RegexNFAStateEntry entryTaken;
                if (value is IList<RegexNFAStateEntry>)
                {
                    var endStatesUnranked = (IList<RegexNFAStateEntry>)value;
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
                    entryTaken = (RegexNFAStateEntry)value;
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
                    var endStatesUnranked = (IList<RegexNFAStateEntry>)value;

                    endStatesUnranked.RemoveWhere(
                        endState => endState.MatchBeginEventSeqNo <= skipPastRow);
                }
                else
                {
                    var endState = (RegexNFAStateEntry)value;
                    if (endState.MatchBeginEventSeqNo <= skipPastRow)
                    {
                        endStatesPerEndEvent.Put(entry.Key, null);
                    }
                }
            }
        }

        private IList<RegexNFAStateEntry> Step(
            bool skipTrackMaxState,
            IEnumerator<RegexNFAStateEntry> currentStatesIterator,
            EventBean theEvent,
            IList<RegexNFAStateEntry> nextStates,
            IList<RegexNFAStateEntry> endStates,
            bool isRetainEventSet,
            int currentEventSequenceNumber,
            Object partitionKey)
        {
            IList<RegexNFAStateEntry> terminationStates = null;  // always null or a list of entries (no singleton list)

            // handle current state matching
            while (currentStatesIterator.MoveNext())
            {
                RegexNFAStateEntry currentState = currentStatesIterator.Current;

                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegExState(currentState, _variableStreams, _multimatchStreamNumToVariable); }

                if (_isTrackMaxStates && !skipTrackMaxState)
                {
                    MatchRecognizeStatePoolStmtSvc poolSvc = _agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc;
                    poolSvc.EngineSvc.DecreaseCount(_agentInstanceContext);
                    poolSvc.StmtHandler.DecreaseCount();
                }

                var eventsPerStream = currentState.EventsPerStream;
                var currentStateStreamNum = currentState.State.StreamNum;
                eventsPerStream[currentStateStreamNum] = theEvent;
                if (_isDefineAsksMultimatches)
                {
                    eventsPerStream[_numEventsEventsPerStreamDefine - 1] = GetMultimatchState(currentState);
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

                            if (_isCollectMultimatches)
                            {
                                multimatches = DeepCopy(multimatches);
                            }
                        }

                        if ((_isCollectMultimatches) && (currentState.State.IsMultiple))
                        {
                            multimatches = AddTag(currentState.State.StreamNum, theEvent, multimatches);
                            eventsForState[currentStateStreamNum] = null; // remove event from evaluation list
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
                            if (_isTrackMaxStates && !skipTrackMaxState)
                            {
                                var poolSvc = _agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc;
                                var allow = poolSvc.EngineSvc.TryIncreaseCount(_agentInstanceContext);
                                if (allow)
                                {
                                    poolSvc.StmtHandler.IncreaseCount();
                                    entry.State = next;
                                    nextStates.Add(entry);
                                }
                            }
                            else
                            {
                                entry.State = next;
                                nextStates.Add(entry);
                            }
                        }
                    }
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExState(nextStates, _variableStreams, _multimatchStreamNumToVariable); }
                }
                // when not-matches
                else
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExState(Collections.GetEmptyList<RegexNFAStateEntry>(), _variableStreams, _multimatchStreamNumToVariable); }

                    // determine interval and or-terminated
                    if (_isOrTerminated)
                    {
                        eventsPerStream[currentStateStreamNum] = null;  // deassign
                        var nextStatesFromHere = currentState.State.NextStates;

                        // save state for each next state
                        RegexNFAState theEndState = null;
                        foreach (var next in nextStatesFromHere)
                        {
                            if (next is RegexNFAStateEnd)
                            {
                                theEndState = next;
                            }
                        }
                        if (theEndState != null)
                        {
                            var entry = new RegexNFAStateEntry(currentState.MatchBeginEventSeqNo, currentState.MatchBeginEventTime, theEndState, eventsPerStream, currentState.GreedyCountPerState, currentState.OptionalMultiMatches, partitionKey);
                            if (terminationStates == null)
                            {
                                terminationStates = new List<RegexNFAStateEntry>();
                            }
                            terminationStates.Add(entry);
                        }
                    }
                }
            }

            // handle start states for the event
            foreach (RegexNFAState startState in _startStates)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegExStateStart(startState, _variableStreams, _multimatchStreamNumToVariable); }

                var eventsPerStream = new EventBean[_numEventsEventsPerStreamDefine];
                int currentStateStreamNum = startState.StreamNum;
                eventsPerStream[currentStateStreamNum] = theEvent;

                if (startState.Matches(eventsPerStream, _agentInstanceContext))
                {
                    if (isRetainEventSet)
                    {
                        _windowMatchedEventset.Add(theEvent);
                    }
                    var nextStatesFromHere = startState.NextStates;

                    // save state for each next state
                    var copy = nextStatesFromHere.Count > 1;
                    foreach (RegexNFAState next in nextStatesFromHere)
                    {
                        if (_isTrackMaxStates && !skipTrackMaxState)
                        {
                            var poolSvc = _agentInstanceContext.StatementContext.MatchRecognizeStatePoolStmtSvc;
                            var allow = poolSvc.EngineSvc.TryIncreaseCount(_agentInstanceContext);
                            if (!allow)
                            {
                                continue;
                            }
                            poolSvc.StmtHandler.IncreaseCount();
                        }

                        var eventsForState = eventsPerStream;
                        var multimatches = _isCollectMultimatches ? new MultimatchState[_multimatchVariablesArray.Length] : null;
                        var greedyCounts = new int[_allStates.Length];

                        if (copy)
                        {
                            eventsForState = new EventBean[eventsForState.Length];
                            Array.Copy(eventsPerStream, 0, eventsForState, 0, eventsForState.Length);

                            var greedyCountsCopy = new int[greedyCounts.Length];
                            Array.Copy(greedyCounts, 0, greedyCountsCopy, 0, greedyCounts.Length);
                            greedyCounts = greedyCountsCopy;
                        }

                        if ((_isCollectMultimatches) && (startState.IsMultiple))
                        {
                            multimatches = AddTag(startState.StreamNum, theEvent, multimatches);
                            eventsForState[currentStateStreamNum] = null; // remove event from evaluation list
                        }

                        if ((startState.IsGreedy != null) && (startState.IsGreedy.Value))
                        {
                            greedyCounts[startState.NodeNumFlat]++;
                        }

                        long time = 0;
                        if (_matchRecognizeSpec.Interval != null)
                        {
                            time = _agentInstanceContext.StatementContext.SchedulingService.Time;
                        }

                        var entry = new RegexNFAStateEntry(currentEventSequenceNumber, time, startState, eventsForState, greedyCounts, multimatches, partitionKey);
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
                }

                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegExStateStart(nextStates, _variableStreams, _multimatchStreamNumToVariable); }
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
            for (var i = 0; i < props.Length; i++)
            {
                var state = states[i];
                if (state == null)
                {
                    props[i] = null;
                }
                else
                {
                    props[i] = state.ShrinkEventArray;
                }
            }
            return _defineMultimatchEventBean;
        }

        private MultimatchState[] DeepCopy(MultimatchState[] multimatchStates)
        {
            if (multimatchStates == null)
            {
                return null;
            }

            var copy = new MultimatchState[multimatchStates.Length];
            for (var i = 0; i < copy.Length; i++)
            {
                if (multimatchStates[i] != null)
                {
                    copy[i] = new MultimatchState(multimatchStates[i]);
                }
            }

            return copy;
        }

        private MultimatchState[] AddTag(int streamNum, EventBean theEvent, MultimatchState[] multimatches)
        {
            if (multimatches == null)
            {
                multimatches = new MultimatchState[_multimatchVariablesArray.Length];
            }

            var index = _multimatchStreamNumToVariable[streamNum];
            var state = multimatches[index];
            if (state == null)
            {
                multimatches[index] = new MultimatchState(theEvent);
                return multimatches;
            }

            multimatches[index].Add(theEvent);
            return multimatches;
        }

        private EventBean GenerateOutputRow(RegexNFAStateEntry entry)
        {
            var rowDataRaw = _compositeEventBean.Properties;

            // we first generate a raw row of <string, object> for each variable name.
            foreach (var variableDef in _variableStreams)
            {
                if (!variableDef.Value.Second)
                {
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
                    if (multimatchState[i] == null)
                    {
                        rowDataRaw[_multimatchVariableToStreamNum[i]] = null;
                        continue;
                    }
                    EventBean[] multimatchEvents = multimatchState[i].ShrinkEventArray;
                    rowDataRaw[_multimatchVariableToStreamNum[i]] = multimatchEvents;

                    if (_aggregationService != null)
                    {
                        var eventsPerStream = entry.EventsPerStream;
                        var streamNum = _multimatchVariableToStreamNum[i];

                        foreach (var multimatchEvent in multimatchEvents)
                        {
                            eventsPerStream[streamNum] = multimatchEvent;
                            _aggregationService.ApplyEnter(eventsPerStream, streamNum, _agentInstanceContext);
                        }
                    }
                }
            }
            else
            {
                foreach (var index in _multimatchVariableToStreamNum)
                {
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

        private void ScheduleCallback(long timeDelta, RegexNFAStateEntry endState)
        {
            var matchBeginTime = endState.MatchBeginEventTime;
            if (_regexPartitionStateRepo.ScheduleState.IsEmpty())
            {
                _regexPartitionStateRepo.ScheduleState.PutOrAdd(matchBeginTime, endState);
                _scheduler.AddSchedule(timeDelta);
            }
            else
            {
                var newEntry = _regexPartitionStateRepo.ScheduleState.PutOrAdd(matchBeginTime, endState);
                if (newEntry)
                {
                    long currentFirstKey = _regexPartitionStateRepo.ScheduleState.FirstKey();
                    if (currentFirstKey > matchBeginTime)
                    {
                        _scheduler.ChangeSchedule(timeDelta);
                    }
                }
            }
        }

        private void RemoveScheduleAddEndState(RegexNFAStateEntry terminationState, IList<RegexNFAStateEntry> foundEndStates)
        {
            var matchBeginTime = terminationState.MatchBeginEventTime;
            var removedOne = _regexPartitionStateRepo.ScheduleState.FindRemoveAddToList(matchBeginTime, terminationState, foundEndStates);
            if (removedOne && _regexPartitionStateRepo.ScheduleState.IsEmpty())
            {
                _scheduler.RemoveSchedule();
            }
        }

        public void Triggered()
        {
            var currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            long intervalMSec = _matchRecognizeSpec.Interval.GetScheduleBackwardDelta(currentTime, _agentInstanceContext);
            if (_regexPartitionStateRepo.ScheduleState.IsEmpty())
            {
                return;
            }

            IList<RegexNFAStateEntry> indicatables = new List<RegexNFAStateEntry>();
            while (true)
            {
                var firstKey = _regexPartitionStateRepo.ScheduleState.FirstKey();
                var cutOffTime = currentTime - intervalMSec;
                if (firstKey > cutOffTime)
                {
                    break;
                }

                _regexPartitionStateRepo.ScheduleState.RemoveAddRemoved(firstKey, indicatables);
                if (_regexPartitionStateRepo.ScheduleState.IsEmpty())
                {
                    break;
                }
            }

            // schedule next
            if (!_regexPartitionStateRepo.ScheduleState.IsEmpty())
            {
                long msecAfterCurrentTime = _regexPartitionStateRepo.ScheduleState.FirstKey() + intervalMSec - _agentInstanceContext.StatementContext.SchedulingService.Time;
                _scheduler.AddSchedule(msecAfterCurrentTime);
            }

            if (!_matchRecognizeSpec.IsAllMatches)
            {
                indicatables = RankEndStatesMultiPartition(indicatables);
            }

            var outBeans = new EventBean[indicatables.Count];
            var count = 0;
            foreach (var endState in indicatables)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegMeasure(endState, _variableStreams, _multimatchStreamNumToVariable); }
                outBeans[count] = GenerateOutputRow(endState);
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegMeasure(outBeans[count]); }
                count++;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QRegOut(outBeans); }
            UpdateChildren(outBeans, null);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ARegOut(); }
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

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.@event.arr;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    /// <summary>
    /// View for match recognize support.
    /// </summary>
    public class RowRecogNFAView : ViewSupport,
        AgentInstanceStopCallback,
        RowRecogNFAViewService,
        RowRecogNFAViewScheduleCallback
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RowRecogNFAView));
        private const bool IsDebug = false;

        private readonly RowRecogNFAViewFactory _factory;
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly RowRecogNFAViewScheduler _scheduler; // for interval-handling

        private readonly RowRecogPreviousStrategyImpl _rowRecogPreviousStrategy;
        private readonly ObjectArrayBackedEventBean _compositeEventBean;

        // state
        private RowRecogPartitionStateRepo _regexPartitionStateRepo;

        private readonly ISet<EventBean>
            _windowMatchedEventset; // this is NOT per partition - some optimizations are done for batch-processing (minus is out-of-sequence in partition)

        private readonly ObjectArrayBackedEventBean _defineMultimatchEventBean;

        public RowRecogNFAView(
            RowRecogNFAViewFactory factory,
            AgentInstanceContext agentInstanceContext,
            RowRecogNFAViewScheduler scheduler)
        {
            _factory = factory;
            var desc = factory.Desc;
            _scheduler = scheduler;
            _agentInstanceContext = agentInstanceContext;

            var compositeEventType = desc.CompositeEventType;
            _compositeEventBean = new ObjectArrayEventBean(
                new object[compositeEventType.PropertyNames.Length],
                compositeEventType);

            var multimatchEventType = desc.MultimatchEventType;
            _defineMultimatchEventBean = multimatchEventType == null
                ? null
                : agentInstanceContext.EventBeanTypedEventFactory.AdapterForTypedObjectArray(
                    new object[multimatchEventType.PropertyNames.Length],
                    multimatchEventType);

            _windowMatchedEventset = new LinkedHashSet<EventBean>();

            // handle "previous" function nodes (performance-optimized for direct index access)
            if (desc.PreviousRandomAccessIndexes != null) {
                // Build an array of indexes
                _rowRecogPreviousStrategy = new RowRecogPreviousStrategyImpl(
                    desc.PreviousRandomAccessIndexes,
                    factory.Desc.IsUnbound);
            }
            else {
                _rowRecogPreviousStrategy = null;
            }

            // create state repository
            var repoFactory = agentInstanceContext.RowRecogStateRepoFactory;
            var terminationStateCompare =
                new RowRecogPartitionTerminationStateComparator(
                    desc.MultimatchStreamNumToVariable,
                    desc.VariableStreams);
            if (desc.PartitionEvalMayNull == null) {
                _regexPartitionStateRepo = repoFactory.MakeSingle(
                    _rowRecogPreviousStrategy,
                    agentInstanceContext,
                    this,
                    desc.HasInterval,
                    terminationStateCompare);
            }
            else {
                var stateRepoGroupMeta = new RowRecogPartitionStateRepoGroupMeta(
                    desc.HasInterval,
                    desc.PartitionEvalMayNull,
                    agentInstanceContext);
                _regexPartitionStateRepo = repoFactory.MakePartitioned(
                    _rowRecogPreviousStrategy,
                    stateRepoGroupMeta,
                    agentInstanceContext,
                    this,
                    desc.HasInterval,
                    terminationStateCompare);
            }
        }

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            UpdateInternal(newData, oldData, true);
        }

        public void Stop(AgentInstanceStopServices services)
        {
            if (_scheduler != null) {
                _scheduler.RemoveSchedule();
            }

            if (_factory.IsTrackMaxStates) {
                var size = _regexPartitionStateRepo.StateCount;
                var poolSvc = _agentInstanceContext.StatementContext.RowRecogStatePoolStmtSvc;
                poolSvc.RuntimeSvc.DecreaseCount(_agentInstanceContext, size);
                poolSvc.StmtHandler.DecreaseCount(size);
            }

            _regexPartitionStateRepo.Destroy();
        }

        private void UpdateInternal(
            EventBean[] newData,
            EventBean[] oldData,
            bool postOutput)
        {
            var desc = _factory.Desc;
            if (desc.IsIterateOnly) {
                if (oldData != null) {
                    _regexPartitionStateRepo.RemoveOld(oldData, false, new bool[oldData.Length]);
                }

                if (newData != null) {
                    foreach (var newEvent in newData) {
                        var partitionState = _regexPartitionStateRepo.GetState(newEvent, true);
                        if (partitionState?.RandomAccess != null) {
                            partitionState.RandomAccess.NewEventPrepare(newEvent);
                        }
                    }
                }

                return;
            }

            if (oldData != null) {
                var isOutOfSequenceRemove = false;

                EventBean first = null;
                if (!_windowMatchedEventset.IsEmpty()) {
                    first = _windowMatchedEventset.First();
                }

                // remove old data, if found in set
                var found = new bool[oldData.Length];
                var countX = 0;

                // detect out-of-sequence removes
                foreach (var oldEvent in oldData) {
                    var removed = _windowMatchedEventset.Remove(oldEvent);
                    if (removed) {
                        if ((oldEvent != first) && (first != null)) {
                            isOutOfSequenceRemove = true;
                        }

                        found[countX++] = true;
                        if (!_windowMatchedEventset.IsEmpty()) {
                            first = _windowMatchedEventset.First();
                        }
                    }
                }

                // reset, rebuilding state
                if (isOutOfSequenceRemove) {
                    if (_factory.IsTrackMaxStates) {
                        var size = _regexPartitionStateRepo.StateCount;
                        var poolSvc =
                            _agentInstanceContext.StatementContext.RowRecogStatePoolStmtSvc;
                        poolSvc.RuntimeSvc.DecreaseCount(_agentInstanceContext, size);
                        poolSvc.StmtHandler.DecreaseCount(size);
                    }

                    _regexPartitionStateRepo = _regexPartitionStateRepo.CopyForIterate(true);
                    var parentEvents = Parent.GetEnumerator();
                    var iteratorResult = ProcessIterator(
                        true,
                        parentEvents,
                        _regexPartitionStateRepo);
                    _regexPartitionStateRepo.EventSequenceNum = iteratorResult.EventSequenceNum;
                }
                else {
                    // remove old events from repository - and let the repository know there are no interesting events left
                    var numRemoved = _regexPartitionStateRepo.RemoveOld(
                        oldData,
                        _windowMatchedEventset.IsEmpty(),
                        found);

                    if (_factory.IsTrackMaxStates) {
                        var poolSvc =
                            _agentInstanceContext.StatementContext.RowRecogStatePoolStmtSvc;
                        poolSvc.RuntimeSvc.DecreaseCount(_agentInstanceContext, numRemoved);
                        poolSvc.StmtHandler.DecreaseCount(numRemoved);
                    }
                }
            }

            if (newData == null) {
                return;
            }

            IList<RowRecogNFAStateEntry> endStates = new List<RowRecogNFAStateEntry>();
            IList<RowRecogNFAStateEntry> terminationStatesAll = null;

            foreach (var newEvent in newData) {
                IList<RowRecogNFAStateEntry> nextStates = new List<RowRecogNFAStateEntry>(2);
                var eventSequenceNumber = _regexPartitionStateRepo.IncrementAndGetEventSequenceNum();

                // get state holder for this event
                var partitionState = _regexPartitionStateRepo.GetState(newEvent, true);
                var currentStatesIterator = partitionState.CurrentStatesIterator;
                _agentInstanceContext.InstrumentationProvider.QRegEx(newEvent, partitionState);

                partitionState.RandomAccess?.NewEventPrepare(newEvent);

                var terminationStates = Step(
                    false,
                    currentStatesIterator,
                    newEvent,
                    nextStates,
                    endStates,
                    !desc.IsUnbound,
                    eventSequenceNumber,
                    partitionState.OptionalKeys);

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
                _agentInstanceContext.InstrumentationProvider.ARegEx(partitionState, endStates, terminationStates);
            }

            if (endStates.IsEmpty() && (!desc.IsOrTerminated || terminationStatesAll == null)) {
                return;
            }

            // perform inter-ranking and elimination of duplicate matches
            if (!desc.IsAllMatches) {
                endStates = RankEndStatesMultiPartition(endStates);
                if (endStates.IsReadOnly) {
                    endStates = new List<RowRecogNFAStateEntry>(endStates);
                }
            }

            // handle interval for the set of matches
            if (desc.HasInterval) {
                for (var ii = 0; ii < endStates.Count; ii++) {
                    var endState = endStates[ii];
                    _agentInstanceContext.InstrumentationProvider.QRegIntervalState(
                        endState,
                        _factory.Desc.VariableStreams,
                        _factory.Desc.MultimatchStreamNumToVariable,
                        _agentInstanceContext.StatementContext.SchedulingService.Time);

                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null) {
                        Log.Warn("Null partition state encountered, skipping row");
                        _agentInstanceContext.InstrumentationProvider.ARegIntervalState(false);
                        continue;
                    }

                    // determine whether to schedule
                    bool scheduleDelivery;
                    if (!desc.IsOrTerminated) {
                        scheduleDelivery = true;
                    }
                    else {
                        // determine whether there can be more matches
                        if (endState.State.NextStates.Length == 1 &&
                            endState.State.NextStates[0] is RowRecogNFAStateEndEval) {
                            scheduleDelivery = false;
                        }
                        else {
                            scheduleDelivery = true;
                        }
                    }

                    // only schedule if not an end-state or not or-terminated
                    if (scheduleDelivery) {
                        var matchBeginTime = endState.MatchBeginEventTime;
                        var current = _agentInstanceContext.StatementContext.SchedulingService.Time;
                        var deltaFromStart = current - matchBeginTime;
                        var deltaUntil = ComputeScheduleForwardDelta(current, deltaFromStart);

                        if (_regexPartitionStateRepo.ScheduleState.ContainsKey(matchBeginTime)) {
                            ScheduleCallback(deltaUntil, endState);
                            _agentInstanceContext.InstrumentationProvider.ARegIntervalState(true);
                            endStates.RemoveAt(ii--);
                        }
                        else {
                            if (deltaFromStart < deltaUntil) {
                                ScheduleCallback(deltaUntil, endState);
                                _agentInstanceContext.InstrumentationProvider.ARegIntervalState(true);
                                endStates.RemoveAt(ii--);
                            }
                            else {
                                _agentInstanceContext.InstrumentationProvider.ARegIntervalState(false);
                            }
                        }
                    }
                    else {
                        _agentInstanceContext.InstrumentationProvider.ARegIntervalState(false);
                    }
                }

                // handle termination states - those that terminated the pattern and remove the callback
                if (desc.IsOrTerminated && terminationStatesAll != null) {
                    foreach (var terminationState in terminationStatesAll) {
                        var partitionState =
                            _regexPartitionStateRepo.GetState(terminationState.PartitionKey);
                        if (partitionState == null) {
                            Log.Warn("Null partition state encountered, skipping row");
                            continue;
                        }

                        RemoveScheduleAddEndState(terminationState, endStates);
                    }

                    // rank
                    if (!desc.IsAllMatches) {
                        endStates = RankEndStatesMultiPartition(endStates);
                        if (endStates.IsReadOnly) {
                            endStates = new List<RowRecogNFAStateEntry>(endStates);
                        }
                    }
                }

                if (endStates.IsEmpty()) {
                    return;
                }
            }
            else if (desc.Skip == MatchRecognizeSkipEnum.PAST_LAST_ROW) {
                // handle skip for incremental mode
                for (var ii = 0; ii < endStates.Count; ii++) {
                    var endState = endStates[ii];
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null) {
                        Log.Warn("Null partition state encountered, skipping row");
                        continue;
                    }

                    var currentStates = partitionState.CurrentStates;
                    for (int ss = 0; ss < currentStates.Count; ss++) {
                        var currentState = currentStates[ss];
                        if (currentState.MatchBeginEventSeqNo <= endState.MatchEndEventSeqNo) {
                            currentStates.RemoveAt(ss--);
                        }
                    }
                }
            }
            else if (desc.Skip == MatchRecognizeSkipEnum.TO_NEXT_ROW) {
                for (var endStatesIndex = 0; endStatesIndex < endStates.Count; endStatesIndex++) {
                    var endState = endStates[endStatesIndex];
                    var partitionState = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (partitionState == null) {
                        Log.Warn("Null partition state encountered, skipping row");
                        continue;
                    }

                    var currentStates = partitionState.CurrentStates;
                    for (var stateIndex = 0; stateIndex < currentStates.Count; stateIndex++) {
                        var currentState = currentStates[stateIndex];
                        if (currentState.MatchBeginEventSeqNo <= endState.MatchBeginEventSeqNo) {
                            currentStates.RemoveAt(stateIndex);
                            stateIndex--;
                        }
                    }
                }
            }

            var outBeans = new EventBean[endStates.Count];
            var count = 0;
            foreach (var endState in endStates) {
                _agentInstanceContext.InstrumentationProvider.QRegMeasure(
                    endState,
                    _factory.Desc.VariableStreams,
                    _factory.Desc.MultimatchStreamNumToVariable);

                outBeans[count] = GenerateOutputRow(endState);

                _agentInstanceContext.InstrumentationProvider.ARegMeasure(outBeans[count]);
                count++;

                // check partition state - if empty delete (no states and no random access)
                if (endState.PartitionKey != null) {
                    var state = _regexPartitionStateRepo.GetState(endState.PartitionKey);
                    if (state.IsEmptyCurrentState && state.RandomAccess == null) {
                        _regexPartitionStateRepo.RemoveState(endState.PartitionKey);
                    }
                }
            }

            if (postOutput) {
                _agentInstanceContext.InstrumentationProvider.QRegOut(outBeans);
                Child.Update(outBeans, null);
                _agentInstanceContext.InstrumentationProvider.ARegOut();
            }
        }

        private long ComputeScheduleForwardDelta(
            long current,
            long deltaFromStart)
        {
            _agentInstanceContext.InstrumentationProvider.QRegIntervalValue();
            var result = _factory.Desc.IntervalCompute.DeltaAdd(current, null, true, null);
            _agentInstanceContext.InstrumentationProvider.ARegIntervalValue(result);
            return result - deltaFromStart;
        }

        private RowRecogNFAStateEntry RankEndStates(IList<RowRecogNFAStateEntry> endStates)
        {
            // sort by end-event descending (newest first)
            Collections.SortInPlace(endStates, RowRecogHelper.END_STATE_COMPARATOR);

            // find the earliest begin-event
            RowRecogNFAStateEntry found = null;
            var min = Int32.MaxValue;
            var multipleMinimums = false;
            foreach (var state in endStates) {
                if (state.MatchBeginEventSeqNo < min) {
                    found = state;
                    min = state.MatchBeginEventSeqNo;
                }
                else if (state.MatchBeginEventSeqNo == min) {
                    multipleMinimums = true;
                }
            }

            if (!multipleMinimums) {
                Collections.SingletonList(found);
            }

            // compare greedy counts
            int[] best = null;
            found = null;
            foreach (var state in endStates) {
                if (state.MatchBeginEventSeqNo != min) {
                    continue;
                }

                if (best == null) {
                    best = state.GreedycountPerState;
                    found = state;
                }
                else {
                    var current = state.GreedycountPerState;
                    if (Compare(current, best)) {
                        best = current;
                        found = state;
                    }
                }
            }

            return found;
        }

        private bool Compare(
            int[] current,
            int[] best)
        {
            foreach (var state in _factory.AllStates) {
                if (state.IsGreedy == null) {
                    continue;
                }

                if (state.IsGreedy.GetValueOrDefault()) {
                    if (current[state.NodeNumFlat] > best[state.NodeNumFlat]) {
                        return true;
                    }
                }
                else {
                    if (current[state.NodeNumFlat] < best[state.NodeNumFlat]) {
                        return true;
                    }
                }
            }

            return false;
        }

        private RowRecogIteratorResult ProcessIterator(
            bool isOutOfSeqDelete,
            IEnumerator<EventBean> events,
            RowRecogPartitionStateRepo regexPartitionStateRepo)
        {
            IList<RowRecogNFAStateEntry> endStates = new List<RowRecogNFAStateEntry>();
            IEnumerator<RowRecogNFAStateEntry> currentStates;
            var eventSequenceNumber = 0;

            EventBean theEvent;
            for (; events.MoveNext();) {
                IList<RowRecogNFAStateEntry> nextStates = new List<RowRecogNFAStateEntry>(2);
                theEvent = events.Current;
                eventSequenceNumber++;

                var partitionState = regexPartitionStateRepo.GetState(theEvent, false);
                currentStates = partitionState.CurrentStatesIterator;

                partitionState.RandomAccess?.ExistingEventPrepare(theEvent);

                Step(
                    !isOutOfSeqDelete,
                    currentStates,
                    theEvent,
                    nextStates,
                    endStates,
                    false,
                    eventSequenceNumber,
                    partitionState.OptionalKeys);

                partitionState.CurrentStates = nextStates;
            }

            return new RowRecogIteratorResult(endStates, eventSequenceNumber);
        }

        public override EventType EventType => _factory.Desc.RowEventType;

        public override IEnumerator<EventBean> GetEnumerator()
        {
            if (_factory.Desc.IsUnbound) {
                return CollectionUtil.NULL_EVENT_ITERATOR;
            }

            var it = Parent.GetEnumerator();

            var regexPartitionStateRepoNew = _regexPartitionStateRepo.CopyForIterate(false);

            var iteratorResult = ProcessIterator(false, it, regexPartitionStateRepoNew);
            var endStates = iteratorResult.GetEndStates();
            if (endStates.IsEmpty()) {
                return CollectionUtil.NULL_EVENT_ITERATOR;
            }
            else {
                endStates = RankEndStatesMultiPartition(endStates);
            }

            IList<EventBean> output = new List<EventBean>();
            foreach (var endState in endStates) {
                output.Add(GenerateOutputRow(endState));
            }

            return output.GetEnumerator();
        }

        public void Accept(RowRecogNFAViewServiceVisitor visitor)
        {
            _regexPartitionStateRepo.Accept(visitor);
        }

        private IList<RowRecogNFAStateEntry> RankEndStatesMultiPartition(IList<RowRecogNFAStateEntry> endStates)
        {
            if (endStates.IsEmpty()) {
                return endStates;
            }

            if (endStates.Count == 1) {
                return endStates;
            }

            // unpartitioned case -
            if (_factory.Desc.PartitionEvalMayNull == null) {
                return RankEndStatesWithinPartitionByStart(endStates);
            }

            // partitioned case - structure end states by partition
            IDictionary<object, object> perPartition = new LinkedHashMap<object, object>();
            foreach (var endState in endStates) {
                var value = perPartition.Get(endState.PartitionKey);
                if (value == null) {
                    perPartition.Put(endState.PartitionKey, endState);
                }
                else if (value is IList<RowRecogNFAStateEntry>) {
                    var entries = (IList<RowRecogNFAStateEntry>) value;
                    entries.Add(endState);
                }
                else {
                    IList<RowRecogNFAStateEntry> entries = new List<RowRecogNFAStateEntry>();
                    entries.Add((RowRecogNFAStateEntry) value);
                    entries.Add(endState);
                    perPartition.Put(endState.PartitionKey, entries);
                }
            }

            IList<RowRecogNFAStateEntry> finalEndStates = new List<RowRecogNFAStateEntry>();
            foreach (var entry in perPartition) {
                if (entry.Value is RowRecogNFAStateEntry) {
                    finalEndStates.Add((RowRecogNFAStateEntry) entry.Value);
                }
                else {
                    var entries = (IList<RowRecogNFAStateEntry>) entry.Value;
                    finalEndStates.AddAll(RankEndStatesWithinPartitionByStart(entries));
                }
            }

            return finalEndStates;
        }

        private IList<RowRecogNFAStateEntry> RankEndStatesWithinPartitionByStart(IList<RowRecogNFAStateEntry> endStates)
        {
            if (endStates.IsEmpty()) {
                return endStates;
            }

            if (endStates.Count == 1) {
                return endStates;
            }

            var rowRecogDesc = _factory.Desc;
            var endStatesPerBeginEvent = new SortedDictionary<int, object>();
            foreach (var entry in endStates) {
                var beginNum = entry.MatchBeginEventSeqNo;
                var value = endStatesPerBeginEvent.Get(beginNum);
                if (value == null) {
                    endStatesPerBeginEvent.Put(beginNum, entry);
                }
                else if (value is IList<RowRecogNFAStateEntry>) {
                    var entries = (IList<RowRecogNFAStateEntry>) value;
                    entries.Add(entry);
                }
                else {
                    IList<RowRecogNFAStateEntry> entries = new List<RowRecogNFAStateEntry>();
                    entries.Add((RowRecogNFAStateEntry) value);
                    entries.Add(entry);
                    endStatesPerBeginEvent.Put(beginNum, entries);
                }
            }

            if (endStatesPerBeginEvent.Count == 1) {
                var endStatesUnranked =
                    (IList<RowRecogNFAStateEntry>) endStatesPerBeginEvent.Values.First();
                if (rowRecogDesc.IsAllMatches) {
                    return endStatesUnranked;
                }

                var chosen = RankEndStates(endStatesUnranked);
                return Collections.SingletonList(chosen);
            }

            IList<RowRecogNFAStateEntry> endStatesRanked = new List<RowRecogNFAStateEntry>();
            var keys = endStatesPerBeginEvent.Keys.ToArray();
            foreach (var key in keys) {
                var value = endStatesPerBeginEvent.Delete(key);
                if (value == null) {
                    continue;
                }

                RowRecogNFAStateEntry entryTaken;
                if (value is IList<RowRecogNFAStateEntry>) {
                    var endStatesUnranked = (IList<RowRecogNFAStateEntry>) value;
                    if (endStatesUnranked.IsEmpty()) {
                        continue;
                    }

                    entryTaken = RankEndStates(endStatesUnranked);

                    if (rowRecogDesc.IsAllMatches) {
                        endStatesRanked.AddAll(
                            endStatesUnranked); // we take all matches and don't rank except to determine skip-past
                    }
                    else {
                        endStatesRanked.Add(entryTaken);
                    }
                }
                else {
                    entryTaken = (RowRecogNFAStateEntry) value;
                    endStatesRanked.Add(entryTaken);
                }
                // could be null as removals take place

                if (entryTaken != null) {
                    if (rowRecogDesc.Skip == MatchRecognizeSkipEnum.PAST_LAST_ROW) {
                        var skipPastRow = entryTaken.MatchEndEventSeqNo;
                        RemoveSkippedEndStates(endStatesPerBeginEvent, skipPastRow);
                    }
                    else if (rowRecogDesc.Skip == MatchRecognizeSkipEnum.TO_NEXT_ROW) {
                        var skipPastRow = entryTaken.MatchBeginEventSeqNo;
                        RemoveSkippedEndStates(endStatesPerBeginEvent, skipPastRow);
                    }
                }
            }

            return endStatesRanked;
        }

        private void RemoveSkippedEndStates(
            SortedDictionary<int, object> endStatesPerEndEvent,
            int skipPastRow)
        {
            var endStatePostActions = new List<Action>();

            foreach (var entry in endStatesPerEndEvent) {
                var value = entry.Value;

                if (value is IList<RowRecogNFAStateEntry> endStatesUnranked) {
                    for (var ii = 0; ii < endStatesUnranked.Count; ii++) {
                        var endState = endStatesUnranked[ii];
                        if (endState.MatchBeginEventSeqNo <= skipPastRow) {
                            endStatesUnranked.RemoveAt(ii);
                            ii--;
                        }
                    }
                }
                else {
                    var endState = (RowRecogNFAStateEntry) value;
                    if (endState.MatchBeginEventSeqNo <= skipPastRow) {
                        endStatePostActions.Add(() => endStatesPerEndEvent.Put(entry.Key, null));
                    }
                }
            }

            endStatePostActions.ForEach(action => action.Invoke());
        }

        private IList<RowRecogNFAStateEntry> Step(
            bool skipTrackMaxState,
            IEnumerator<RowRecogNFAStateEntry> currentStatesEnumerator,
            EventBean theEvent,
            IList<RowRecogNFAStateEntry> nextStates,
            IList<RowRecogNFAStateEntry> endStates,
            bool isRetainEventSet,
            int currentEventSequenceNumber,
            object partitionKey)
        {
            var rowRecogDesc = _factory.Desc;
            IList<RowRecogNFAStateEntry> terminationStates = null; // always null or a list of entries (no singleton list)

            // handle current state matching
            while (currentStatesEnumerator.MoveNext()) {
                var currentState = currentStatesEnumerator.Current;
                _agentInstanceContext.InstrumentationProvider.QRegExState(
                    currentState,
                    _factory.Desc.VariableStreams,
                    _factory.Desc.MultimatchStreamNumToVariable);

                if (_factory.IsTrackMaxStates && !skipTrackMaxState) {
                    var poolSvc = _agentInstanceContext.StatementContext.RowRecogStatePoolStmtSvc;
                    poolSvc.RuntimeSvc.DecreaseCount(_agentInstanceContext);
                    poolSvc.StmtHandler.DecreaseCount();
                }

                var eventsPerStream = currentState.EventsPerStream;
                var currentStateStreamNum = currentState.State.StreamNum;
                eventsPerStream[currentStateStreamNum] = theEvent;
                if (rowRecogDesc.IsDefineAsksMultimatches) {
                    eventsPerStream[rowRecogDesc.NumEventsEventsPerStreamDefine - 1] = GetMultimatchState(currentState);
                }

                if (currentState.State.Matches(eventsPerStream, _agentInstanceContext)) {
                    if (isRetainEventSet) {
                        _windowMatchedEventset.Add(theEvent);
                    }

                    var nextStatesFromHere = currentState.State.NextStates;

                    // save state for each next state
                    var copy = nextStatesFromHere.Length > 1;
                    foreach (var next in nextStatesFromHere) {
                        var eventsForState = eventsPerStream;
                        var multimatches = currentState.OptionalMultiMatches;
                        var greedyCounts = currentState.GreedycountPerState;

                        if (copy) {
                            eventsForState = new EventBean[eventsForState.Length];
                            Array.Copy(eventsPerStream, 0, eventsForState, 0, eventsForState.Length);

                            var greedyCountsCopy = new int[greedyCounts.Length];
                            Array.Copy(greedyCounts, 0, greedyCountsCopy, 0, greedyCounts.Length);
                            greedyCounts = greedyCountsCopy;

                            if (rowRecogDesc.IsCollectMultimatches) {
                                multimatches = DeepCopy(multimatches);
                            }
                        }

                        if (rowRecogDesc.IsCollectMultimatches && (currentState.State.IsMultiple)) {
                            multimatches = AddTag(currentState.State.StreamNum, theEvent, multimatches);
                            eventsForState[currentStateStreamNum] = null; // remove event from evaluation list
                        }

                        if ((currentState.State.IsGreedy != null) && (currentState.State.IsGreedy.Value)) {
                            greedyCounts[currentState.State.NodeNumFlat]++;
                        }

                        var entry = new RowRecogNFAStateEntry(
                            currentState.MatchBeginEventSeqNo,
                            currentState.MatchBeginEventTime,
                            currentState.State,
                            eventsForState,
                            greedyCounts,
                            multimatches,
                            partitionKey);
                        if (next is RowRecogNFAStateEndEval) {
                            entry.MatchEndEventSeqNo = currentEventSequenceNumber;
                            endStates.Add(entry);
                        }
                        else {
                            if (_factory.IsTrackMaxStates && !skipTrackMaxState) {
                                var poolSvc =
                                    _agentInstanceContext.StatementContext.RowRecogStatePoolStmtSvc;
                                var allow = poolSvc.RuntimeSvc.TryIncreaseCount(_agentInstanceContext);
                                if (allow) {
                                    poolSvc.StmtHandler.IncreaseCount();
                                    entry.State = next;
                                    nextStates.Add(entry);
                                }
                            }
                            else {
                                entry.State = next;
                                nextStates.Add(entry);
                            }
                        }
                    }

                    _agentInstanceContext.InstrumentationProvider.ARegExState(
                        nextStates,
                        _factory.Desc.VariableStreams,
                        _factory.Desc.MultimatchStreamNumToVariable);
                }
                else {
                    // when not-matches
                    _agentInstanceContext.InstrumentationProvider.ARegExState(
                        Collections.GetEmptyList<RowRecogNFAStateEntry>(),
                        _factory.Desc.VariableStreams,
                        _factory.Desc.MultimatchStreamNumToVariable);

                    // determine interval and or-terminated
                    if (rowRecogDesc.IsOrTerminated) {
                        eventsPerStream[currentStateStreamNum] = null; // deassign
                        var nextStatesFromHere = currentState.State.NextStates;

                        // save state for each next state
                        RowRecogNFAState theEndState = null;
                        foreach (var next in nextStatesFromHere) {
                            if (next is RowRecogNFAStateEndEval) {
                                theEndState = next;
                            }
                        }

                        if (theEndState != null) {
                            var entry = new RowRecogNFAStateEntry(
                                currentState.MatchBeginEventSeqNo,
                                currentState.MatchBeginEventTime,
                                theEndState,
                                eventsPerStream,
                                currentState.GreedycountPerState,
                                currentState.OptionalMultiMatches,
                                partitionKey);
                            if (terminationStates == null) {
                                terminationStates = new List<RowRecogNFAStateEntry>();
                            }

                            terminationStates.Add(entry);
                        }
                    }
                }
            }

            // handle start states for the event
            foreach (var startState in _factory.StartStates) {
                _agentInstanceContext.InstrumentationProvider.QRegExStateStart(
                    startState,
                    _factory.Desc.VariableStreams,
                    _factory.Desc.MultimatchStreamNumToVariable);

                var eventsPerStream = new EventBean[rowRecogDesc.NumEventsEventsPerStreamDefine];
                var currentStateStreamNum = startState.StreamNum;
                eventsPerStream[currentStateStreamNum] = theEvent;

                if (startState.Matches(eventsPerStream, _agentInstanceContext)) {
                    if (isRetainEventSet) {
                        _windowMatchedEventset.Add(theEvent);
                    }

                    var nextStatesFromHere = startState.NextStates;

                    // save state for each next state
                    var copy = nextStatesFromHere.Length > 1;
                    foreach (var next in nextStatesFromHere) {
                        if (_factory.IsTrackMaxStates && !skipTrackMaxState) {
                            var poolSvc =
                                _agentInstanceContext.StatementContext.RowRecogStatePoolStmtSvc;
                            var allow = poolSvc.RuntimeSvc.TryIncreaseCount(_agentInstanceContext);
                            if (!allow) {
                                continue;
                            }

                            poolSvc.StmtHandler.IncreaseCount();
                        }

                        var eventsForState = eventsPerStream;
                        var multimatches = rowRecogDesc.IsCollectMultimatches
                            ? new RowRecogMultimatchState[rowRecogDesc.MultimatchVariablesArray.Length]
                            : null;
                        var greedyCounts = new int[_factory.AllStates.Length];

                        if (copy) {
                            eventsForState = new EventBean[eventsForState.Length];
                            Array.Copy(eventsPerStream, 0, eventsForState, 0, eventsForState.Length);

                            var greedyCountsCopy = new int[greedyCounts.Length];
                            Array.Copy(greedyCounts, 0, greedyCountsCopy, 0, greedyCounts.Length);
                            greedyCounts = greedyCountsCopy;
                        }

                        if (rowRecogDesc.IsCollectMultimatches && (startState.IsMultiple)) {
                            multimatches = AddTag(startState.StreamNum, theEvent, multimatches);
                            eventsForState[currentStateStreamNum] = null; // remove event from evaluation list
                        }

                        if ((startState.IsGreedy != null) && (startState.IsGreedy.Value)) {
                            greedyCounts[startState.NodeNumFlat]++;
                        }

                        long time = 0;
                        if (rowRecogDesc.HasInterval) {
                            time = _agentInstanceContext.StatementContext.SchedulingService.Time;
                        }

                        var entry = new RowRecogNFAStateEntry(
                            currentEventSequenceNumber,
                            time,
                            startState,
                            eventsForState,
                            greedyCounts,
                            multimatches,
                            partitionKey);
                        if (next is RowRecogNFAStateEndEval) {
                            entry.MatchEndEventSeqNo = currentEventSequenceNumber;
                            endStates.Add(entry);
                        }
                        else {
                            entry.State = next;
                            nextStates.Add(entry);
                        }
                    }
                }

                _agentInstanceContext.InstrumentationProvider.ARegExStateStart(
                    nextStates,
                    _factory.Desc.VariableStreams,
                    _factory.Desc.MultimatchStreamNumToVariable);
            }

            return terminationStates; // only for immediate use, not for scheduled use as no copy of state
        }

        private ObjectArrayBackedEventBean GetMultimatchState(RowRecogNFAStateEntry currentState)
        {
            if (currentState.OptionalMultiMatches == null || !currentState.State.IsExprRequiresMultimatchState) {
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

        private RowRecogMultimatchState[] DeepCopy(RowRecogMultimatchState[] multimatchStates)
        {
            if (multimatchStates == null) {
                return null;
            }

            var copy = new RowRecogMultimatchState[multimatchStates.Length];
            for (var i = 0; i < copy.Length; i++) {
                if (multimatchStates[i] != null) {
                    copy[i] = new RowRecogMultimatchState(multimatchStates[i]);
                }
            }

            return copy;
        }

        private RowRecogMultimatchState[] AddTag(
            int streamNum,
            EventBean theEvent,
            RowRecogMultimatchState[] multimatches)
        {
            if (multimatches == null) {
                multimatches = new RowRecogMultimatchState[_factory.Desc.MultimatchVariablesArray.Length];
            }

            var index = _factory.Desc.MultimatchStreamNumToVariable[streamNum];
            var state = multimatches[index];
            if (state == null) {
                multimatches[index] = new RowRecogMultimatchState(theEvent);
                return multimatches;
            }

            multimatches[index].Add(theEvent);
            return multimatches;
        }

        private EventBean GenerateOutputRow(RowRecogNFAStateEntry entry)
        {
            var aggregationServiceFactories = _factory.Desc.AggregationServiceFactories;
            if (aggregationServiceFactories != null) {
                // we must synchronize here when aggregations are used
                // since expression futures are set
                var aggregationServices = new AggregationService[aggregationServiceFactories.Length];
                for (var i = 0; i < aggregationServices.Length; i++) {
                    if (aggregationServiceFactories[i] != null) {
                        aggregationServices[i] = aggregationServiceFactories[i]
                            .MakeService(
                                _agentInstanceContext,
                                _agentInstanceContext.ImportServiceRuntime,
                                false,
                                null,
                                null);
                        _factory.Desc.AggregationResultFutureAssignables[i].Assign(aggregationServices[i]);
                    }
                }

                lock (_factory) {
                    return GenerateOutputRowUnderLockIfRequired(entry, aggregationServices);
                }
            }
            else {
                return GenerateOutputRowUnderLockIfRequired(entry, null);
            }
        }

        private EventBean GenerateOutputRowUnderLockIfRequired(
            RowRecogNFAStateEntry entry,
            AggregationService[] aggregationServices)
        {
            var rowDataRaw = _compositeEventBean.Properties;

            // we first generate a raw row of <String, Object> for each variable name.
            foreach (var variableDef in _factory.Desc.VariableStreams) {
                if (!variableDef.Value.Second) {
                    var index = variableDef.Value.First;
                    rowDataRaw[index] = entry.EventsPerStream[index];
                }
            }

            var multimatchVariableToStreamNum = _factory.Desc.MultimatchVariableToStreamNum;
            if (entry.OptionalMultiMatches != null) {
                var multimatchState = entry.OptionalMultiMatches;
                for (var i = 0; i < multimatchState.Length; i++) {
                    var streamNum = multimatchVariableToStreamNum[i];
                    if (multimatchState[i] == null) {
                        rowDataRaw[streamNum] = null;
                        continue;
                    }

                    var multimatchEvents = multimatchState[i].GetShrinkEventArray();
                    rowDataRaw[streamNum] = multimatchEvents;

                    if (aggregationServices != null && aggregationServices[streamNum] != null) {
                        var entryEventsPerStream = entry.EventsPerStream;

                        foreach (var multimatchEvent in multimatchEvents) {
                            entryEventsPerStream[streamNum] = multimatchEvent;
                            aggregationServices[streamNum]
                                .ApplyEnter(entryEventsPerStream, null, _agentInstanceContext);
                        }
                    }
                }
            }
            else {
                foreach (var index in multimatchVariableToStreamNum) {
                    rowDataRaw[index] = null;
                }
            }

            IDictionary<string, object> row = new Dictionary<string, object>();
            var columnNum = 0;
            var eventsPerStream = new EventBean[1];
            var columnNames = _factory.Desc.ColumnNames;
            foreach (var expression in _factory.Desc.ColumnEvaluators) {
                eventsPerStream[0] = _compositeEventBean;
                var result = expression.Evaluate(eventsPerStream, true, _agentInstanceContext);
                row.Put(columnNames[columnNum], result);
                columnNum++;
            }

            return _agentInstanceContext.StatementContext.EventBeanTypedEventFactory.AdapterForTypedMap(
                row,
                _factory.Desc.RowEventType);
        }

        private void ScheduleCallback(
            long timeDelta,
            RowRecogNFAStateEntry endState)
        {
            var matchBeginTime = endState.MatchBeginEventTime;
            if (_regexPartitionStateRepo.ScheduleState.IsEmpty) {
                _regexPartitionStateRepo.ScheduleState.PutOrAdd(matchBeginTime, endState);
                _scheduler.AddSchedule(timeDelta);
            }
            else {
                var newEntry = _regexPartitionStateRepo.ScheduleState.PutOrAdd(matchBeginTime, endState);
                if (newEntry) {
                    var currentFirstKey = _regexPartitionStateRepo.ScheduleState.FirstKey();
                    if (currentFirstKey > matchBeginTime) {
                        _scheduler.ChangeSchedule(timeDelta);
                    }
                }
            }
        }

        private void RemoveScheduleAddEndState(
            RowRecogNFAStateEntry terminationState,
            IList<RowRecogNFAStateEntry> foundEndStates)
        {
            var matchBeginTime = terminationState.MatchBeginEventTime;
            var removedOne = _regexPartitionStateRepo.ScheduleState.FindRemoveAddToList(
                matchBeginTime,
                terminationState,
                foundEndStates);
            if (removedOne && _regexPartitionStateRepo.ScheduleState.IsEmpty) {
                _scheduler.RemoveSchedule();
            }
        }

        public void Triggered()
        {
            var currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            var intervalMSec = ComputeScheduleBackwardDelta(currentTime);
            if (_regexPartitionStateRepo.ScheduleState.IsEmpty) {
                return;
            }

            IList<RowRecogNFAStateEntry> indicatables = new List<RowRecogNFAStateEntry>();
            while (true) {
                var firstKey = _regexPartitionStateRepo.ScheduleState.FirstKey();
                var cutOffTime = currentTime - intervalMSec;
                if (firstKey > cutOffTime) {
                    break;
                }

                _regexPartitionStateRepo.ScheduleState.RemoveAddRemoved(firstKey, indicatables);

                if (_regexPartitionStateRepo.ScheduleState.IsEmpty) {
                    break;
                }
            }

            // schedule next
            if (!_regexPartitionStateRepo.ScheduleState.IsEmpty) {
                var msecAfterCurrentTime = _regexPartitionStateRepo.ScheduleState.FirstKey() +
                                            intervalMSec -
                                            _agentInstanceContext.StatementContext.SchedulingService.Time;
                _scheduler.AddSchedule(msecAfterCurrentTime);
            }

            if (!_factory.Desc.IsAllMatches) {
                indicatables = RankEndStatesMultiPartition(indicatables);
            }

            var outBeans = new EventBean[indicatables.Count];
            var count = 0;
            foreach (var endState in indicatables) {
                _agentInstanceContext.InstrumentationProvider.QRegMeasure(
                    endState,
                    _factory.Desc.VariableStreams,
                    _factory.Desc.MultimatchStreamNumToVariable);
                outBeans[count] = GenerateOutputRow(endState);
                _agentInstanceContext.InstrumentationProvider.ARegMeasure(outBeans[count]);
                count++;
            }

            _agentInstanceContext.InstrumentationProvider.QRegOut(outBeans);
            Child.Update(outBeans, null);
            _agentInstanceContext.InstrumentationProvider.ARegOut();
        }

        private long ComputeScheduleBackwardDelta(long currentTime)
        {
            _agentInstanceContext.InstrumentationProvider.QRegIntervalValue();
            var result = _factory.Desc.IntervalCompute.DeltaSubtract(currentTime, null, true, null);
            _agentInstanceContext.InstrumentationProvider.ARegIntervalValue(result);
            return result;
        }

        public RowRecogPreviousStrategy PreviousEvaluationStrategy => _rowRecogPreviousStrategy;

        public RowRecogNFAViewFactory Factory => _factory;
    }
} // end of namespace
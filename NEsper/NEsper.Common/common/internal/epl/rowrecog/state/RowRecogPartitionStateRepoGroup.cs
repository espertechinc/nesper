///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    /// <summary>
    ///     Partition-by implementation for partition state.
    /// </summary>
    public class RowRecogPartitionStateRepoGroup : RowRecogPartitionStateRepo
    {
        /// <summary>
        ///     Empty state collection initial threshold.
        /// </summary>
        public const int INITIAL_COLLECTION_MIN = 100;

        private readonly RowRecogPartitionStateRepoGroupMeta _meta;
        private readonly RowRecogPreviousStrategyImpl _getter;
        private readonly IDictionary<object, RowRecogPartitionStateImpl> _states;
        private readonly RowRecogPartitionStateRepoScheduleStateImpl _optionalIntervalSchedules;

        private int _currentCollectionSize = INITIAL_COLLECTION_MIN;
        private int _eventSequenceNumber;

        public RowRecogPartitionStateRepoGroup(
            RowRecogPreviousStrategyImpl getter,
            RowRecogPartitionStateRepoGroupMeta meta,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare)
        {
            _getter = getter;
            _meta = meta;
            _states = new Dictionary<object, RowRecogPartitionStateImpl>();
            _optionalIntervalSchedules = keepScheduleState
                ? new RowRecogPartitionStateRepoScheduleStateImpl(terminationStateCompare)
                : null;
        }

        public bool IsPartitioned => true;

        public IDictionary<object, RowRecogPartitionStateImpl> States {
            get => _states;
        }

        public int IncrementAndGetEventSequenceNum()
        {
            ++_eventSequenceNumber;
            return _eventSequenceNumber;
        }

        public int EventSequenceNum {
            set => _eventSequenceNumber = value;
        }

        public RowRecogPartitionStateRepoScheduleState ScheduleState => _optionalIntervalSchedules;

        public void RemoveState(object partitionKey)
        {
            States.Remove(partitionKey);
        }

        public RowRecogPartitionStateRepo CopyForIterate(bool forOutOfOrderReprocessing)
        {
            var copy = new RowRecogPartitionStateRepoGroup(_getter, _meta, false, null);
            foreach (var entry in States) {
                copy.States.Put(entry.Key, new RowRecogPartitionStateImpl(entry.Value.RandomAccess, entry.Key));
            }

            return copy;
        }

        public int RemoveOld(
            EventBean[] oldData,
            bool isEmpty,
            bool[] found)
        {
            if (isEmpty) {
                int countRemovedInner;
                if (_getter == null) {
                    // no "prev" used, clear all state
                    countRemovedInner = StateCount;
                    States.Clear();
                }
                else {
                    countRemovedInner = 0;
                    foreach (var entry in States) {
                        countRemovedInner += entry.Value.NumStates;
                        entry.Value.CurrentStates = Collections.GetEmptyList<RowRecogNFAStateEntry>();
                    }
                }

                // clear "prev" state
                if (_getter != null) {
                    // we will need to remove event-by-event
                    for (var i = 0; i < oldData.Length; i++) {
                        var partitionState = GetStateImpl(oldData[i], true);

                        partitionState?.RemoveEventFromPrev(oldData);
                    }
                }

                return countRemovedInner;
            }

            // we will need to remove event-by-event
            var countRemoved = 0;
            for (var i = 0; i < oldData.Length; i++) {
                var partitionState = GetStateImpl(oldData[i], true);
                if (partitionState == null) {
                    continue;
                }

                if (found[i]) {
                    countRemoved += partitionState.RemoveEventFromState(oldData[i]);
                    var cleared = partitionState.NumStates == 0;
                    if (cleared) {
                        if (_getter == null) {
                            States.Remove(partitionState.OptionalKeys);
                        }
                    }

                    partitionState.RemoveEventFromPrev(oldData[i]);
                }
            }

            return countRemoved;
        }

        public RowRecogPartitionState GetState(object key)
        {
            return States.Get(key);
        }

        public void Accept(RowRecogNFAViewServiceVisitor visitor)
        {
            visitor.VisitPartitioned(
                States.TransformRight<object, RowRecogPartitionStateImpl, RowRecogPartitionState>());
        }

        public void Destroy()
        {
        }

        public int StateCount {
            get {
                var total = 0;
                foreach (var entry in States) {
                    total += entry.Value.NumStates;
                }

                return total;
            }
        }

        internal RowRecogPartitionStateImpl GetStateImpl(
            EventBean theEvent,
            bool isCollect)
        {
            _meta.AgentInstanceContext.InstrumentationProvider.QRegExPartition(theEvent);

            // collect unused states
            if (isCollect && States.Count >= _currentCollectionSize) {
                IList<object> removeList = new List<object>();
                foreach (var entry in States) {
                    if (entry.Value.IsEmptyCurrentState &&
                        (entry.Value.RandomAccess == null || entry.Value.RandomAccess.IsEmpty())) {
                        removeList.Add(entry.Key);
                    }
                }

                foreach (var removeKey in removeList) {
                    States.Remove(removeKey);
                }

                if (removeList.Count < _currentCollectionSize / 5) {
                    _currentCollectionSize *= 2;
                }
            }

            var key = GetKeys(theEvent, _meta);

            var state = States.Get(key);
            if (state != null) {
                _meta.AgentInstanceContext.InstrumentationProvider.ARegExPartition(true, key, state);
                return state;
            }

            state = new RowRecogPartitionStateImpl(_getter, new List<RowRecogNFAStateEntry>(), key);
            States.Put(key, state);

            _meta.AgentInstanceContext.InstrumentationProvider.ARegExPartition(false, key, state);
            return state;
        }

        public RowRecogPartitionState GetState(
            EventBean theEvent,
            bool isCollect)
        {
            return GetStateImpl(theEvent, isCollect);
        }

        public static object GetKeys(
            EventBean theEvent,
            RowRecogPartitionStateRepoGroupMeta meta)
        {
            var eventsPerStream = meta.EventsPerStream;
            eventsPerStream[0] = theEvent;
            return meta.PartitionExpression.Evaluate(eventsPerStream, true, meta.AgentInstanceContext);
        }
    }
} // end of namespace
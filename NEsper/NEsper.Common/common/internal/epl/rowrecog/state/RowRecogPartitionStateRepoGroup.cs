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

        private readonly RowRecogPreviousStrategyImpl getter;

        private readonly RowRecogPartitionStateRepoGroupMeta meta;
        private readonly RowRecogPartitionStateRepoScheduleStateImpl optionalIntervalSchedules;

        private int currentCollectionSize = INITIAL_COLLECTION_MIN;
        private int eventSequenceNumber;

        public RowRecogPartitionStateRepoGroup(
            RowRecogPreviousStrategyImpl getter,
            RowRecogPartitionStateRepoGroupMeta meta,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare)
        {
            this.getter = getter;
            this.meta = meta;
            States = new Dictionary<object, RowRecogPartitionStateImpl>();
            optionalIntervalSchedules = keepScheduleState
                ? new RowRecogPartitionStateRepoScheduleStateImpl(terminationStateCompare)
                : null;
        }

        public bool IsPartitioned => true;

        public IDictionary<object, RowRecogPartitionStateImpl> States { get; }

        public int IncrementAndGetEventSequenceNum()
        {
            ++eventSequenceNumber;
            return eventSequenceNumber;
        }

        public int EventSequenceNum {
            set => eventSequenceNumber = value;
        }

        public RowRecogPartitionStateRepoScheduleState ScheduleState => optionalIntervalSchedules;

        public void RemoveState(object partitionKey)
        {
            States.Remove(partitionKey);
        }

        public RowRecogPartitionStateRepo CopyForIterate(bool forOutOfOrderReprocessing)
        {
            var copy = new RowRecogPartitionStateRepoGroup(getter, meta, false, null);
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
                if (getter == null) {
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
                if (getter != null) {
                    // we will need to remove event-by-event
                    for (var i = 0; i < oldData.Length; i++) {
                        var partitionState = GetState(oldData[i], true);
                        if (partitionState == null) {
                            continue;
                        }

                        partitionState.RemoveEventFromPrev(oldData);
                    }
                }

                return countRemovedInner;
            }

            // we will need to remove event-by-event
            var countRemoved = 0;
            for (var i = 0; i < oldData.Length; i++) {
                var partitionState = GetState(oldData[i], true);
                if (partitionState == null) {
                    continue;
                }

                if (found[i]) {
                    countRemoved += partitionState.RemoveEventFromState(oldData[i]);
                    var cleared = partitionState.NumStates == 0;
                    if (cleared) {
                        if (getter == null) {
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

        public RowRecogPartitionState GetState(
            EventBean theEvent,
            bool isCollect)
        {
            meta.AgentInstanceContext.InstrumentationProvider.QRegExPartition(theEvent);

            // collect unused states
            if (isCollect && States.Count >= currentCollectionSize) {
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

                if (removeList.Count < currentCollectionSize / 5) {
                    currentCollectionSize *= 2;
                }
            }

            var key = GetKeys(theEvent, meta);

            var state = States.Get(key);
            if (state != null) {
                meta.AgentInstanceContext.InstrumentationProvider.ARegExPartition(true, key, state);
                return state;
            }

            state = new RowRecogPartitionStateImpl(getter, new List<RowRecogNFAStateEntry>(), key);
            States.Put(key, state);

            meta.AgentInstanceContext.InstrumentationProvider.ARegExPartition(false, key, state);
            return state;
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
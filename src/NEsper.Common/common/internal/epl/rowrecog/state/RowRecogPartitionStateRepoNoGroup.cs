///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
    ///     State for when no partitions (single partition) is required.
    /// </summary>
    public class RowRecogPartitionStateRepoNoGroup : RowRecogPartitionStateRepo
    {
        private readonly RowRecogPartitionStateRepoScheduleStateImpl optionalIntervalSchedules;
        private readonly RowRecogPartitionStateImpl singletonState;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="singletonState">state</param>
        public RowRecogPartitionStateRepoNoGroup(RowRecogPartitionStateImpl singletonState)
        {
            this.singletonState = singletonState;
            optionalIntervalSchedules = null;
        }

        public RowRecogPartitionStateRepoNoGroup(
            RowRecogPreviousStrategyImpl getter,
            bool keepScheduleState,
            RowRecogPartitionTerminationStateComparator terminationStateCompare)
        {
            singletonState = new RowRecogPartitionStateImpl(getter, new List<RowRecogNFAStateEntry>());
            optionalIntervalSchedules = keepScheduleState
                ? new RowRecogPartitionStateRepoScheduleStateImpl(terminationStateCompare)
                : null;
        }

        public bool IsPartitioned => false;

        public int IncrementAndGetEventSequenceNum()
        {
            ++EventSequenceNum;
            return EventSequenceNum;
        }

        public int EventSequenceNum { get; set; }

        public RowRecogPartitionStateRepoScheduleState ScheduleState => optionalIntervalSchedules;

        public void RemoveState(object partitionKey)
        {
            // not an operation
        }

        /// <summary>
        ///     Copy state for iteration.
        /// </summary>
        /// <returns>copy</returns>
        public RowRecogPartitionStateRepo CopyForIterate(bool forOutOfOrderReprocessing)
        {
            var state = new RowRecogPartitionStateImpl(singletonState.RandomAccess, null);
            return new RowRecogPartitionStateRepoNoGroup(state);
        }

        public int RemoveOld(
            EventBean[] oldEvents,
            bool isEmpty,
            bool[] found)
        {
            var countRemoved = 0;
            if (isEmpty) {
                countRemoved = singletonState.NumStates;
                singletonState.CurrentStates = Collections.GetEmptyList<RowRecogNFAStateEntry>();
            }
            else {
                foreach (var oldEvent in oldEvents) {
                    countRemoved += singletonState.RemoveEventFromState(oldEvent);
                }
            }

            singletonState.RemoveEventFromPrev(oldEvents);
            return countRemoved;
        }

        public RowRecogPartitionState GetState(
            EventBean theEvent,
            bool collect)
        {
            return singletonState;
        }

        public RowRecogPartitionState GetState(object key)
        {
            return singletonState;
        }

        public void Accept(RowRecogNFAViewServiceVisitor visitor)
        {
            visitor.VisitUnpartitioned(singletonState);
        }

        public int StateCount => singletonState.NumStates;

        public void Destroy()
        {
        }
    }
} // end of namespace
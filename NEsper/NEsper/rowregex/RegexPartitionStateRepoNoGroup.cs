///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.rowregex
{
    /// <summary>
    /// State for when no partitions (single partition) is required.
    /// </summary>
    public class RegexPartitionStateRepoNoGroup : RegexPartitionStateRepo
    {
        private readonly RegexPartitionStateImpl _singletonState;
        private readonly RegexPartitionStateRepoScheduleStateImpl _optionalIntervalSchedules;
        private int _eventSequenceNumber;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="singletonState">state</param>
        public RegexPartitionStateRepoNoGroup(RegexPartitionStateImpl singletonState)
        {
            _singletonState = singletonState;
            _optionalIntervalSchedules = null;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">The getter.</param>
        /// <param name="keepScheduleState">if set to <c>true</c> [keep schedule state].</param>
        /// <param name="terminationStateCompare">The termination state compare.</param>
        public RegexPartitionStateRepoNoGroup(RegexPartitionStateRandomAccessGetter getter, bool keepScheduleState, RegexPartitionTerminationStateComparator terminationStateCompare)
        {
            _singletonState = new RegexPartitionStateImpl(getter, new List<RegexNFAStateEntry>());
            _optionalIntervalSchedules = keepScheduleState ? new RegexPartitionStateRepoScheduleStateImpl(terminationStateCompare) : null;
        }

        public int IncrementAndGetEventSequenceNum()
        {
            ++_eventSequenceNumber;
            return _eventSequenceNumber;
        }

        public int EventSequenceNum
        {
            get { return _eventSequenceNumber; }
            set { _eventSequenceNumber = value; }
        }

        public RegexPartitionStateRepoScheduleState ScheduleState
        {
            get { return _optionalIntervalSchedules; }
        }


        public void RemoveState(Object partitionKey)
        {
            // not an operation
        }

        /// <summary>
        /// Copy state for iteration.
        /// </summary>
        /// <param name="forOutOfOrderReprocessing">For out of order reprocessing.</param>
        /// <returns></returns>
        public RegexPartitionStateRepo CopyForIterate(bool forOutOfOrderReprocessing)
        {
            var state = new RegexPartitionStateImpl(_singletonState.RandomAccess, null);
            return new RegexPartitionStateRepoNoGroup(state);
        }

        public int RemoveOld(EventBean[] oldEvents, bool isEmpty, bool[] found)
        {
            int countRemoved = 0;
            if (isEmpty)
            {
                countRemoved = _singletonState.NumStates;
                _singletonState.CurrentStates = Collections.GetEmptyList<RegexNFAStateEntry>();
            }
            else
            {
                foreach (EventBean oldEvent in oldEvents) {
                    countRemoved += _singletonState.RemoveEventFromState(oldEvent);
                }
            }
            _singletonState.RemoveEventFromPrev(oldEvents);
            return countRemoved;
        }

        public RegexPartitionState GetState(EventBean theEvent, bool collect)
        {
            return _singletonState;
        }

        public RegexPartitionState GetState(Object key)
        {
            return _singletonState;
        }

        public void Accept(EventRowRegexNFAViewServiceVisitor visitor)
        {
            visitor.VisitUnpartitioned(_singletonState);
        }

        public bool IsPartitioned
        {
            get { return false; }
        }

        public int StateCount
        {
            get { return _singletonState.NumStates; }
        }
    }
}

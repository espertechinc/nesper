///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.@internal.epl.rowrecog.core;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public class RowRecogPartitionStateRepoScheduleStateImpl : RowRecogPartitionStateRepoScheduleState
    {
        private readonly RowRecogPartitionTerminationStateComparator terminationStateCompare;
        private readonly IOrderedDictionary<long, object> schedule = new OrderedListDictionary<long, object>();

        public RowRecogPartitionStateRepoScheduleStateImpl(
            RowRecogPartitionTerminationStateComparator terminationStateCompare)
        {
            this.terminationStateCompare = terminationStateCompare;
        }

        public bool IsEmpty => schedule.IsEmpty();

        public bool PutOrAdd(
            long matchBeginTime,
            RowRecogNFAStateEntry state)
        {
            var value = schedule.Get(matchBeginTime);
            if (value == null) {
                schedule.Put(matchBeginTime, state);
                return true;
            }

            if (value is RowRecogNFAStateEntry valueEntry) {
                IList<RowRecogNFAStateEntry> list = new List<RowRecogNFAStateEntry>();
                list.Add(valueEntry);
                list.Add(state);
                schedule.Put(matchBeginTime, list);
            }
            else {
                var list = (IList<RowRecogNFAStateEntry>)value;
                list.Add(state);
            }

            return false;
        }

        public object Get(long matchBeginTime)
        {
            return schedule.Get(matchBeginTime);
        }

        public long FirstKey()
        {
            return schedule.Keys.First();
        }

        public void RemoveAddRemoved(
            long matchBeginTime,
            IList<RowRecogNFAStateEntry> foundStates)
        {
            var found = schedule.Delete(matchBeginTime);
            if (found == null) {
                return;
            }

            if (found is RowRecogNFAStateEntry rowRecogNFAStateEntry) {
                foundStates.Add(rowRecogNFAStateEntry);
            }
            else {
                foundStates.AddAll((IList<RowRecogNFAStateEntry>)found);
            }
        }

        public bool ContainsKey(long matchBeginTime)
        {
            return schedule.ContainsKey(matchBeginTime);
        }

        public bool FindRemoveAddToList(
            long matchBeginTime,
            RowRecogNFAStateEntry state,
            IList<RowRecogNFAStateEntry> foundStates)
        {
            var entry = schedule.Get(matchBeginTime);
            if (entry == null) {
                return false;
            }

            if (entry is RowRecogNFAStateEntry single) {
                if (terminationStateCompare.CompareTerminationStateToEndState(state, single)) {
                    schedule.Remove(matchBeginTime);
                    foundStates.Add(single);
                    return true;
                }

                return false;
            }

            var entries = (IList<RowRecogNFAStateEntry>)entry;
            var removed = false;

            for (var ii = 0; ii < entries.Count; ii++) {
                var endState = entries[ii];
                if (terminationStateCompare.CompareTerminationStateToEndState(state, endState)) {
                    entries.RemoveAt(ii);
                    foundStates.Add(endState);
                    removed = true;
                    ii--;
                }
            }

            if (entries.IsEmpty()) {
                schedule.Remove(matchBeginTime);
            }

            return removed;
        }
    }
} // end of namespace
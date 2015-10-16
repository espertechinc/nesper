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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.rowregex
{
	public class RegexPartitionStateRepoScheduleStateImpl : RegexPartitionStateRepoScheduleState
    {
	    private readonly RegexPartitionTerminationStateComparator terminationStateCompare;
	    private readonly SortedDictionary<long?, object> schedule = new SortedDictionary<long?, object>();

	    public RegexPartitionStateRepoScheduleStateImpl(RegexPartitionTerminationStateComparator terminationStateCompare) {
	        this.terminationStateCompare = terminationStateCompare;
	    }

	    public bool IsEmpty() {
	        return schedule.IsEmpty();
	    }

	    public bool PutOrAdd(long matchBeginTime, RegexNFAStateEntry state) {
	        object value = schedule.Get(matchBeginTime);
	        if (value == null) {
	            schedule.Put(matchBeginTime, state);
	            return true;
	        }

	        if (value is RegexNFAStateEntry)
	        {
	            RegexNFAStateEntry valueEntry = (RegexNFAStateEntry) value;
	            IList<RegexNFAStateEntry> list = new List<RegexNFAStateEntry>();
	            list.Add(valueEntry);
	            list.Add(state);
	            schedule.Put(matchBeginTime, list);
	        }
	        else
	        {
	            IList<RegexNFAStateEntry> list = (IList<RegexNFAStateEntry>) value;
	            list.Add(state);
	        }

	        return false;
	    }

	    public object Get(long matchBeginTime) {
	        return schedule.Get(matchBeginTime);
	    }

	    public long FirstKey()
	    {
	        return schedule.Keys.First().Value;
	    }

	    public void RemoveAddRemoved(long matchBeginTime, IList<RegexNFAStateEntry> foundStates) {
	        var found = schedule.Pluck(matchBeginTime);
	        if (found == null) {
	            return;
	        }
	        if (found is RegexNFAStateEntry) {
	            foundStates.Add((RegexNFAStateEntry) found);
	        }
	        else {
	            foundStates.AddAll((IList<RegexNFAStateEntry>) found);
	        }
	    }

	    public bool ContainsKey(long matchBeginTime) {
	        return schedule.ContainsKey(matchBeginTime);
	    }

	    public bool FindRemoveAddToList(long matchBeginTime, RegexNFAStateEntry state, IList<RegexNFAStateEntry> foundStates) {
	        object entry = schedule.Get(matchBeginTime);
	        if (entry == null) {
	            return false;
	        }
	        if (entry is RegexNFAStateEntry) {
	            RegexNFAStateEntry single = (RegexNFAStateEntry) entry;
	            if (terminationStateCompare.CompareTerminationStateToEndState(state, single)) {
	                schedule.Remove(matchBeginTime);
	                foundStates.Add(single);
	                return true;
	            }
	            return false;
	        }

	        var entries = (IList<RegexNFAStateEntry>) entry;
	        var removed = entries.RemoveWhere(
	            endState => terminationStateCompare.CompareTerminationStateToEndState(state, endState),
	            endState => foundStates.Add(endState)) > 0;

	        if (entries.IsEmpty()) {
	            schedule.Remove(matchBeginTime);
	        }
	        return removed;
	    }
	}
} // end of namespace

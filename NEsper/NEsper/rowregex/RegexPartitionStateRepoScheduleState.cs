///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.rowregex
{
	/// <summary>
	/// Service for holding schedule state.
	/// </summary>
	public interface RegexPartitionStateRepoScheduleState
	{
	    bool IsEmpty();

	    /// <summary>
	    /// Add entry returning true if the key did not exist.
	    /// </summary>
	    /// <param name="matchBeginTime">key</param>
	    /// <param name="state">entry</param>
	    /// <returns>indicator</returns>
	    bool PutOrAdd(long matchBeginTime, RegexNFAStateEntry state);

	    long FirstKey();
	    void RemoveAddRemoved(long matchBeginTime, IList<RegexNFAStateEntry> foundStates);
	    bool ContainsKey(long matchBeginTime);

	    /// <summary>
	    /// Find and remove operation, wherein removed items are added to the found list,
	    /// returning an indicator whether the item was found and removed
	    /// </summary>
	    /// <param name="matchBeginTime">key</param>
	    /// <param name="state">entry</param>
	    /// <param name="foundStates">list to be added to</param>
	    /// <returns>indicator whether any item was found and removed</returns>
	    bool FindRemoveAddToList(long matchBeginTime, RegexNFAStateEntry state, IList<RegexNFAStateEntry> foundStates);
	}
} // end of namespace

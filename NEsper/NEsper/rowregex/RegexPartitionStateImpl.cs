///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.rowregex
{
	/// <summary>
	/// All current state holding partial NFA matches.
	/// </summary>
	public class RegexPartitionStateImpl : RegexPartitionState
	{
	    private readonly RegexPartitionStateRandomAccess _randomAccess;
	    private IList<RegexNFAStateEntry> _currentStates = new List<RegexNFAStateEntry>();
	    private readonly object _optionalKeys;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="randomAccess">for handling "prev" functions, if any</param>
	    /// <param name="optionalKeys">keys for "partition", if any</param>
	    public RegexPartitionStateImpl(RegexPartitionStateRandomAccess randomAccess, object optionalKeys)
	    {
	        this._randomAccess = randomAccess;
	        this._optionalKeys = optionalKeys;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="getter">for "prev" access</param>
	    /// <param name="currentStates">existing state</param>
        public RegexPartitionStateImpl(RegexPartitionStateRandomAccessGetter getter, ICollection<RegexNFAStateEntry> currentStates)
	        : this(getter, currentStates, null)
        {
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="getter">for "prev" access</param>
	    /// <param name="currentStates">existing state</param>
	    /// <param name="optionalKeys">partition keys if any</param>
	    public RegexPartitionStateImpl(RegexPartitionStateRandomAccessGetter getter, ICollection<RegexNFAStateEntry> currentStates, object optionalKeys)
        {
	        if (getter != null)
	        {
	            _randomAccess = new RegexPartitionStateRandomAccessImpl(getter);
	        }

	        this._currentStates = currentStates.AsList();
            this._optionalKeys = optionalKeys;
	    }

	    /// <summary>
	    /// Returns the random access for "prev".
	    /// </summary>
	    /// <value>access</value>
	    public RegexPartitionStateRandomAccess RandomAccess
	    {
	        get { return _randomAccess; }
	    }

	    /// <summary>
	    /// Returns partial matches.
	    /// </summary>
	    /// <value>state</value>
	    public IEnumerator<RegexNFAStateEntry> CurrentStatesEnumerator
	    {
	        get { return _currentStates.GetEnumerator(); }
	    }

	    /// <summary>
	    /// Sets partial matches.
	    /// </summary>
	    /// <value>state to set</value>
        public virtual IList<RegexNFAStateEntry> CurrentStates
	    {
            get { return this._currentStates; }
	        set { this._currentStates = value.AsList(); }
	    }

	    /// <summary>
	    /// Returns partition keys, if any.
	    /// </summary>
	    /// <value>keys</value>
	    public virtual object OptionalKeys
	    {
	        get { return _optionalKeys; }
	    }

	    /// <summary>
	    /// Remove an event from random access for "prev".
	    /// </summary>
	    /// <param name="oldEvents">to remove</param>
	    public virtual void RemoveEventFromPrev(EventBean[] oldEvents)
	    {
	        if (_randomAccess != null)
	        {
	            _randomAccess.Remove(oldEvents);
	        }
	    }

	    /// <summary>
	    /// Remove an event from random access for "prev".
	    /// </summary>
	    /// <param name="oldEvent">to remove</param>
	    public virtual void RemoveEventFromPrev(EventBean oldEvent)
	    {
	        if (_randomAccess != null)
	        {
	            _randomAccess.Remove(oldEvent);
	        }
	    }

	    /// <summary>
	    /// Remove an event from state.
	    /// </summary>
	    /// <param name="oldEvent">to remove</param>
	    /// <returns>true for removed, false for not found</returns>
	    public virtual int RemoveEventFromState(EventBean oldEvent)
	    {
	        int currentSize = _currentStates.Count;
	        var keepList = RemoveEventFromState(oldEvent, _currentStates.GetEnumerator());
	        if (_randomAccess != null) {
	            _randomAccess.Remove(oldEvent);
	        }
	        _currentStates = keepList;
	        return currentSize - keepList.Count;
	    }

	    public virtual int NumStates
	    {
	        get { return _currentStates.Count; }
	    }

	    public void ClearCurrentStates()
        {
	        _currentStates.Clear();
	    }

	    public ICollection<RegexNFAStateEntry> CurrentStatesForPrint
	    {
	        get { return _currentStates; }
	    }

	    public bool IsEmptyCurrentState
	    {
	        get { return _currentStates.IsEmpty(); }
	    }

        public static IList<RegexNFAStateEntry> RemoveEventFromState(EventBean oldEvent, IEnumerator<RegexNFAStateEntry> states)
	    {
            IList<RegexNFAStateEntry> keepList = new List<RegexNFAStateEntry>();
	        for (;states.MoveNext();)
	        {
	            RegexNFAStateEntry entry = states.Current;
	            var keep = true;

	            EventBean[] state = entry.EventsPerStream;
	            foreach (EventBean aState in state)
	            {
	                if (aState != null && aState.Equals(oldEvent))
	                {
	                    keep = false;
	                    break;
	                }
	            }

	            if (keep)
	            {
	                MultimatchState[] multimatch = entry.OptionalMultiMatches;
	                if (multimatch != null) {
	                    foreach (MultimatchState aMultimatch in multimatch) {
	                        if ((aMultimatch != null) && (aMultimatch.ContainsEvent(oldEvent))) {
	                            keep = false;
	                            break;
	                        }
	                    }
	                }
	            }

	            if (keep)
	            {
	                keepList.Add(entry);
	            }
	        }
	        return keepList;
	    }
	}
} // end of namespace

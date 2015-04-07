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
    /// All current state holding partial NFA matches.
    /// </summary>
    public class RegexPartitionState
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="randomAccess">for handling "prev" functions, if any</param>
        /// <param name="optionalKeys">keys for "partition", if any</param>
        /// <param name="hasInterval">true if an interval is provided</param>
        public RegexPartitionState(RegexPartitionStateRandomAccessImpl randomAccess, Object optionalKeys, bool hasInterval)
        {
            CurrentStates = new List<RegexNFAStateEntry>();
            RandomAccess = randomAccess;
            OptionalKeys = optionalKeys;
    
            if (hasInterval)
            {
                CallbackItems = new List<RegexNFAStateEntry>();
            }
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">for "prev" access</param>
        /// <param name="currentStates">existing state</param>
        /// <param name="hasInterval">true for interval</param>
        public RegexPartitionState(RegexPartitionStateRandomAccessGetter getter,
                                   IList<RegexNFAStateEntry> currentStates,
                                   bool hasInterval)
            : this(getter, currentStates, null, hasInterval)
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="getter">for "prev" access</param>
        /// <param name="currentStates">existing state</param>
        /// <param name="optionalKeys">partition keys if any</param>
        /// <param name="hasInterval">true for interval</param>
        public RegexPartitionState(RegexPartitionStateRandomAccessGetter getter,
                                   IList<RegexNFAStateEntry> currentStates,
                                   Object optionalKeys,
                                   bool hasInterval)
        {
            if (getter != null)
            {
                RandomAccess = new RegexPartitionStateRandomAccessImpl(getter);
            }
            CurrentStates = currentStates;
            OptionalKeys = optionalKeys;
    
            if (hasInterval)
            {
                CallbackItems = new List<RegexNFAStateEntry>();
            }
        }

        /// <summary>
        /// Returns the random access for "prev".
        /// </summary>
        /// <value>access</value>
        public RegexPartitionStateRandomAccessImpl RandomAccess { get; private set; }

        /// <summary>
        /// Returns partial matches.
        /// </summary>
        /// <value>state</value>
        public IList<RegexNFAStateEntry> CurrentStates { get; set; }

        /// <summary>
        /// Returns partition keys, if any.
        /// </summary>
        /// <value>keys</value>
        public object OptionalKeys { get; private set; }

        /// <summary>
        /// Remove an event from random access for "prev".
        /// </summary>
        /// <param name="oldEvents">to remove</param>
        public void RemoveEventFromPrev(EventBean[] oldEvents)
        {
            if (RandomAccess != null)
            {
                RandomAccess.Remove(oldEvents);
            }
        }

        /// <summary>
        /// Remove an event from random access for "prev".
        /// </summary>
        /// <param name="oldEvent">to remove</param>
        public void RemoveEventFromPrev(EventBean oldEvent)
        {
            if (RandomAccess != null)
            {
                RandomAccess.Remove(oldEvent);
            }
        }

        /// <summary>
        /// Remove an event from state.
        /// </summary>
        /// <param name="oldEvent">to remove</param>
        /// <returns>true for removed, false for not found</returns>
        public bool RemoveEventFromState(EventBean oldEvent)
        {
            IList<RegexNFAStateEntry> keepList = new List<RegexNFAStateEntry>();
    
            foreach (RegexNFAStateEntry entry in CurrentStates)
            {
                bool keep = true;
    
                EventBean[] state = entry.EventsPerStream;
                foreach (EventBean aState in state)
                {
                    if (aState == oldEvent)
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
    
            if (RandomAccess != null)
            {
                RandomAccess.Remove(oldEvent);
            }
    
            CurrentStates = keepList;
            return keepList.IsEmpty();
        }

        /// <summary>
        /// Returns the interval states, if any.
        /// </summary>
        /// <value>interval states</value>
        public IList<RegexNFAStateEntry> CallbackItems { get; private set; }

        /// <summary>
        /// Returns indicator if callback is schedule.
        /// </summary>
        /// <value>scheduled indicator</value>
        public bool IsCallbackScheduled { get; set; }

        /// <summary>
        /// Add a callback item for intervals.
        /// </summary>
        /// <param name="endState">to add</param>
        public void AddCallbackItem(RegexNFAStateEntry endState)
        {
            CallbackItems.Add(endState);
        }

        public int NumStates
        {
            get { return CurrentStates.Count; }
        }

        public int? NumIntervalCallbackItems
        {
            get { return CallbackItems == null ? (int?) null : CallbackItems.Count; }
        }
    }
}

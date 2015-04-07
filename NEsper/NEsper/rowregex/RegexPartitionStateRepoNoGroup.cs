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

namespace com.espertech.esper.rowregex
{
    /// <summary>State for when no partitions (single partition) is required. </summary>
    public class RegexPartitionStateRepoNoGroup : RegexPartitionStateRepo
    {
        private readonly RegexPartitionState singletonState;
        private readonly bool hasInterval;
    
        /// <summary>Ctor. </summary>
        /// <param name="singletonState">state</param>
        /// <param name="hasInterval">true for interval</param>
        public RegexPartitionStateRepoNoGroup(RegexPartitionState singletonState, bool hasInterval)
        {
            this.singletonState = singletonState;
            this.hasInterval = hasInterval;
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="getter">"prev" getter</param>
        /// <param name="hasInterval">true for interval</param>
        public RegexPartitionStateRepoNoGroup(RegexPartitionStateRandomAccessGetter getter, bool hasInterval)
        {
            singletonState = new RegexPartitionState(getter, new List<RegexNFAStateEntry>(), hasInterval);
            this.hasInterval = hasInterval;
        }
    
        public void RemoveState(Object partitionKey) {
            // not an operation
        }
    
        /// <summary>Copy state for iteration. </summary>
        /// <returns>copy</returns>
        public RegexPartitionStateRepo CopyForIterate()
        {
            RegexPartitionState state = new RegexPartitionState(singletonState.RandomAccess, null, hasInterval);
            return new RegexPartitionStateRepoNoGroup(state, hasInterval);
        }
    
        public void RemoveOld(EventBean[] oldEvents, bool isEmpty, bool[] found)
        {
            if (isEmpty)
            {
                singletonState.CurrentStates.Clear();
            }
            else
            {
                foreach (EventBean oldEvent in oldEvents)
                {
                    singletonState.RemoveEventFromState(oldEvent);
                }
            }
            singletonState.RemoveEventFromPrev(oldEvents);
        }
    
        public RegexPartitionState GetState(EventBean theEvent, bool collect)
        {
            return singletonState;
        }
    
        public RegexPartitionState GetState(Object key)
        {
            return singletonState;
        }
    
        public void Accept(EventRowRegexNFAViewServiceVisitor visitor) {
            visitor.VisitUnpartitioned(singletonState);
        }

        public bool IsPartitioned
        {
            get { return false; }
        }
    }
}

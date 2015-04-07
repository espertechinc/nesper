///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.filter;



namespace com.espertech.esper.support.filter
{
    public class SupportEventEvaluator : EventEvaluator
    {
        private int countInvoked;
        private EventBean lastEvent;
        private ICollection<FilterHandle> lastMatches;
    
        public void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            countInvoked++;
            lastEvent = theEvent;
            lastMatches = matches;
        }
    
        public EventBean GetLastEvent()
        {
            return lastEvent;
        }
    
        public ICollection<FilterHandle> GetLastMatches()
        {
            return lastMatches;
        }
    
        public void SetCountInvoked(int countInvoked)
        {
            this.countInvoked = countInvoked;
        }
    
        public void SetLastEvent(EventBean lastEvent)
        {
            this.lastEvent = lastEvent;
        }
    
        public void SetLastMatches(List<FilterHandle> lastMatches)
        {
            this.lastMatches = lastMatches;
        }
    
        public int GetAndResetCountInvoked()
        {
            int count = countInvoked;
            countInvoked = 0;
            return count;
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.filter;

namespace com.espertech.esper.supportunit.filter
{
    public class SupportEventEvaluator : EventEvaluator
    {
        private int _countInvoked;
        private EventBean _lastEvent;
        private ICollection<FilterHandle> _lastMatches;
    
        public void MatchEvent(EventBean theEvent, ICollection<FilterHandle> matches)
        {
            _countInvoked++;
            _lastEvent = theEvent;
            _lastMatches = matches;
        }
    
        public EventBean GetLastEvent()
        {
            return _lastEvent;
        }
    
        public ICollection<FilterHandle> GetLastMatches()
        {
            return _lastMatches;
        }
    
        public void SetCountInvoked(int countInvoked)
        {
            this._countInvoked = countInvoked;
        }
    
        public void SetLastEvent(EventBean lastEvent)
        {
            this._lastEvent = lastEvent;
        }
    
        public void SetLastMatches(List<FilterHandle> lastMatches)
        {
            this._lastMatches = lastMatches;
        }
    
        public int GetAndResetCountInvoked()
        {
            int count = _countInvoked;
            _countInvoked = 0;
            return count;
        }
    }
}

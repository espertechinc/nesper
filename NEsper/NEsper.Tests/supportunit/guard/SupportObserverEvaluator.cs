///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.pattern;
using com.espertech.esper.pattern.observer;

namespace com.espertech.esper.supportunit.guard
{
    public class SupportObserverEvaluator : ObserverEventEvaluator
    {
        private List<MatchedEventMap> _matchEvents = new List<MatchedEventMap>();
        private int _evaluateFalseCounter;
        private readonly PatternAgentInstanceContext _context;

        public SupportObserverEvaluator(PatternAgentInstanceContext context)
        {
            _context = context;
        }

        public void ObserverEvaluateTrue(MatchedEventMap matchEvent, bool quitted)
        {
            _matchEvents.Add(matchEvent);
        }
    
        public void ObserverEvaluateFalse(bool restartable)
        {
            _evaluateFalseCounter++;
        }
    
        public List<MatchedEventMap> GetAndClearMatchEvents()
        {
            List<MatchedEventMap> original = _matchEvents;
            _matchEvents = new List<MatchedEventMap>();
            return original;
        }

        public List<MatchedEventMap> MatchEvents
        {
            get { return _matchEvents; }
        }

        public int GetAndResetEvaluateFalseCounter()
        {
            int value = _evaluateFalseCounter;
            _evaluateFalseCounter = 0;
            return value;
        }

        public PatternAgentInstanceContext Context
        {
            get { return _context; }
        }
    }
}

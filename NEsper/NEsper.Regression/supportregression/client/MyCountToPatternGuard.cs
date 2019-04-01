///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.guard;

namespace com.espertech.esper.supportregression.client
{
    [Serializable]
    public class MyCountToPatternGuard : GuardSupport
    {
        private readonly int _numCountTo;
        private readonly Quitable _quitable;
    
        private int _counter;
    
        public MyCountToPatternGuard(int numCountTo, Quitable quitable)
        {
            _numCountTo = numCountTo;
            _quitable = quitable;
        }
    
        public override void StartGuard()
        {
            _counter = 0;
        }
    
        public override void StopGuard()
        {
            // No action required when a sub-expression quits, or when the pattern is stopped
        }
    
        public override bool Inspect(MatchedEventMap matchEvent)
        {
            _counter++;
            if (_counter > _numCountTo)
            {
                _quitable.GuardQuit();
                return false;
            }
            return true;
        }

        public override void Accept(EventGuardVisitor visitor)
        {
            visitor.VisitGuard(8);
        }
    }
}

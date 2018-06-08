///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.pattern;
using com.espertech.esper.pattern.guard;

namespace com.espertech.esper.supportunit.guard
{
    public class SupportQuitable : Quitable
    {
        private readonly PatternAgentInstanceContext _patternContext;
    
        public int QuitCounter = 0;
    
        public SupportQuitable(PatternAgentInstanceContext patternContext) {
            _patternContext = patternContext;
        }
    
        public void GuardQuit()
        {
            QuitCounter++;
        }
    
        public int GetAndResetQuitCounter()
        {
            return QuitCounter;
        }

        public PatternAgentInstanceContext Context
        {
            get { return _patternContext; }
        }
    }
}

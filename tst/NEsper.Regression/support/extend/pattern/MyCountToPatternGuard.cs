///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.regressionlib.support.extend.pattern
{
    public class MyCountToPatternGuard : Guard
    {
        private readonly int numCountTo;
        private readonly Quitable quitable;

        private int counter;

        public MyCountToPatternGuard(
            int numCountTo,
            Quitable quitable)
        {
            this.numCountTo = numCountTo;
            this.quitable = quitable;
        }

        public void StartGuard()
        {
            counter = 0;
        }

        public void StopGuard()
        {
            // No action required when a sub-expression quits, or when the pattern is stopped
        }

        public bool Inspect(MatchedEventMap matchEvent)
        {
            counter++;
            if (counter > numCountTo) {
                quitable.GuardQuit();
                return false;
            }

            return true;
        }

        public void Accept(EventGuardVisitor visitor)
        {
            visitor.VisitGuard(8);
        }
    }
} // end of namespace
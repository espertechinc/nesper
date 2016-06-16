///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.view
{
    public sealed class OutputConditionTerm : OutputConditionBase, OutputCondition
    {
        public OutputConditionTerm(OutputCallback outputCallback)
            : base(outputCallback)
        {
        }

        public override void UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
        }

        public override void Stop()
        {
            // no action required
        }
    }
}
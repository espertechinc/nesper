///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    public class OutputConditionTerm : OutputConditionBase,
        OutputCondition
    {
        public OutputConditionTerm(OutputCallback outputCallback)
            : base(outputCallback)
        {
        }

        public override void UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
        }

        public override void StopOutputCondition()
        {
            // no action required
        }
    }
} // end of namespace
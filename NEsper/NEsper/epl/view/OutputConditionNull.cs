///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.view
{
    /// <summary>An empty output condition that is always satisfied. </summary>
    public class OutputConditionNull : OutputConditionBase, OutputCondition
    {
        private static readonly bool DO_OUTPUT = true;
        private static readonly bool FORCE_UPDATE = false;

        /// <summary>Ctor. </summary>
        /// <param name="outputCallback">is the callback to make once the condition is satisfied</param>
        public OutputConditionNull(OutputCallback outputCallback)
            : base(outputCallback)
        {
        }

        public override void UpdateOutputCondition(int newEventsCount, int oldEventsCount)
        {
            OutputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
        }

        public override void Terminated()
        {
            OutputCallback.Invoke(true, true);
        }

        public override void Stop()
        {
            // no action required
        }
    }
}

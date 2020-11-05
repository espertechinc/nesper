///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    /// An empty output condition that is always satisfied.
    /// </summary>
    public class OutputConditionNull : OutputConditionBase,
        OutputCondition
    {
        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = false;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="outputCallback">is the callback to make once the condition is satisfied</param>
        public OutputConditionNull(OutputCallback outputCallback)
            : base(outputCallback)
        {
        }

        public override void UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            outputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
        }

        public override void Terminated()
        {
            outputCallback.Invoke(true, true);
        }

        public override void StopOutputCondition()
        {
            // no action required
        }
    }
} // end of namespace
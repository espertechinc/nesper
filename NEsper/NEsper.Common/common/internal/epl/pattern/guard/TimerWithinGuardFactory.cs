///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.epl.pattern.guard
{
    /// <summary>
    ///     Factory for <seealso cref="TimerWithinGuard" /> instances.
    /// </summary>
    public class TimerWithinGuardFactory : GuardFactory
    {
        private PatternDeltaCompute deltaCompute;
        private int scheduleCallbackId = -1;

        public PatternDeltaCompute DeltaCompute {
            set => deltaCompute = value;
        }

        public int ScheduleCallbackId {
            set => scheduleCallbackId = value;
        }

        public Guard MakeGuard(
            PatternAgentInstanceContext context, MatchedEventMap beginState, Quitable quitable, object guardState)
        {
            return new TimerWithinGuard(ComputeTime(beginState, context), quitable);
        }

        public long ComputeTime(MatchedEventMap beginState, PatternAgentInstanceContext context)
        {
            return deltaCompute.ComputeDelta(beginState, context);
        }
    }
} // end of namespace
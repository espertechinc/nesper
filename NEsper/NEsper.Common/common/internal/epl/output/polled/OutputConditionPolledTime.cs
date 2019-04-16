///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public sealed class OutputConditionPolledTime : OutputConditionPolled
    {
        private readonly OutputConditionPolledTimeFactory factory;
        private readonly AgentInstanceContext context;
        private readonly OutputConditionPolledTimeState state;

        public OutputConditionPolledTime(
            OutputConditionPolledTimeFactory factory,
            AgentInstanceContext context,
            OutputConditionPolledTimeState state)
        {
            this.factory = factory;
            this.context = context;
            this.state = state;
        }

        public OutputConditionPolledState State {
            get => state;
        }

        public bool UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            // If we pull the interval from a variable, then we may need to reschedule
            long msecIntervalSize = factory.timePeriodCompute.DeltaAdd(context.TimeProvider.Time, null, true, context);

            long current = context.TimeProvider.Time;
            if (state.LastUpdate == null || current - state.LastUpdate >= msecIntervalSize) {
                state.LastUpdate = current;
                return true;
            }

            return false;
        }
    }
} // end of namespace
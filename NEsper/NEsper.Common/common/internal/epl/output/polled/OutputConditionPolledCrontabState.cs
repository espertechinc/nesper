///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public class OutputConditionPolledCrontabState : OutputConditionPolledState
    {
        public OutputConditionPolledCrontabState(
            ScheduleSpec scheduleSpec,
            long? currentReferencePoint,
            long nextScheduledTime)
        {
            ScheduleSpec = scheduleSpec;
            CurrentReferencePoint = currentReferencePoint;
            NextScheduledTime = nextScheduledTime;
        }

        public ScheduleSpec ScheduleSpec { get; }

        public long? CurrentReferencePoint { get; set; }

        public long NextScheduledTime { get; set; }
    }
} // end of namespace
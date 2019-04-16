///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    public class OutputConditionPolledCrontabState : OutputConditionPolledState
    {
        private readonly ScheduleSpec scheduleSpec;
        private long? currentReferencePoint;
        private long nextScheduledTime;

        public OutputConditionPolledCrontabState(
            ScheduleSpec scheduleSpec,
            long? currentReferencePoint,
            long nextScheduledTime)
        {
            this.scheduleSpec = scheduleSpec;
            this.currentReferencePoint = currentReferencePoint;
            this.nextScheduledTime = nextScheduledTime;
        }

        public ScheduleSpec ScheduleSpec {
            get => scheduleSpec;
        }

        public long? CurrentReferencePoint {
            get => currentReferencePoint;
        }

        public void SetCurrentReferencePoint(long? currentReferencePoint)
        {
            this.currentReferencePoint = currentReferencePoint;
        }

        public long NextScheduledTime {
            get => nextScheduledTime;
        }

        public void SetNextScheduledTime(long nextScheduledTime)
        {
            this.nextScheduledTime = nextScheduledTime;
        }
    }
} // end of namespace
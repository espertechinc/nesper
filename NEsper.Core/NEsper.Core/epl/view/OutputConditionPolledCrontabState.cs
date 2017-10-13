///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.view
{
	public class OutputConditionPolledCrontabState : OutputConditionPolledState
	{
	    public OutputConditionPolledCrontabState(ScheduleSpec scheduleSpec, long? currentReferencePoint, long nextScheduledTime) {
	        ScheduleSpec = scheduleSpec;
	        CurrentReferencePoint = currentReferencePoint;
	        NextScheduledTime = nextScheduledTime;
	    }

	    public ScheduleSpec ScheduleSpec { get; private set; }

	    public long? CurrentReferencePoint { get; set; }

	    public long NextScheduledTime { get; set; }
	}
} // end of namespace

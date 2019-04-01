///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
	/// <summary>
	/// Output condition handling crontab-at schedule output.
	/// </summary>
	public sealed class OutputConditionPolledCrontab : OutputConditionPolled {
	    private readonly AgentInstanceContext agentInstanceContext;
	    private readonly OutputConditionPolledCrontabState state;

	    public OutputConditionPolledCrontab(AgentInstanceContext agentInstanceContext, OutputConditionPolledCrontabState state) {
	        this.agentInstanceContext = agentInstanceContext;
	        this.state = state;
	    }

	    public OutputConditionPolledState State
	    {
	        get => state;
	    }

	    public bool UpdateOutputCondition(int newEventsCount, int oldEventsCount) {
	        if ((ExecutionPathDebugLog.isDebugEnabled) && (log.IsDebugEnabled)) {
	            log.Debug(".updateOutputCondition, " +
	                    "  newEventsCount==" + newEventsCount +
	                    "  oldEventsCount==" + oldEventsCount);
	        }

	        bool output = false;
	        long currentTime = agentInstanceContext.StatementContext.SchedulingService.Time;
	        ImportServiceRuntime importService = agentInstanceContext.ImportServiceRuntime;
	        if (state.CurrentReferencePoint == null) {
	            state.CurrentReferencePoint = currentTime;
	            state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(state.ScheduleSpec, currentTime, importService.TimeZone, importService.TimeAbacus);
	            output = true;
	        }

	        if (state.NextScheduledTime <= currentTime) {
	            state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(state.ScheduleSpec, currentTime, importService.TimeZone, importService.TimeAbacus);
	            output = true;
	        }

	        return output;
	    }

	    private static readonly ILog log = LogManager.GetLogger(typeof(OutputConditionPolledCrontab));
	}
} // end of namespace
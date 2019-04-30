///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition handling crontab-at schedule output.
    /// </summary>
    public sealed class OutputConditionPolledCrontab : OutputConditionPolled
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly OutputConditionPolledCrontabState _state;

        public OutputConditionPolledCrontab(
            AgentInstanceContext agentInstanceContext,
            OutputConditionPolledCrontabState state)
        {
            this._agentInstanceContext = agentInstanceContext;
            this._state = state;
        }

        public OutputConditionPolledState State
        {
            get => _state;
        }

        public bool UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (log.IsDebugEnabled))
            {
                log.Debug(
                    ".updateOutputCondition, " +
                    "  newEventsCount==" + newEventsCount +
                    "  oldEventsCount==" + oldEventsCount);
            }

            bool output = false;
            long currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            ImportServiceRuntime importService = _agentInstanceContext.ImportServiceRuntime;
            if (_state.CurrentReferencePoint == null)
            {
                _state.CurrentReferencePoint = currentTime;
                _state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(
                    _state.ScheduleSpec, currentTime, importService.TimeZone, importService.TimeAbacus);
                output = true;
            }

            if (_state.NextScheduledTime <= currentTime)
            {
                _state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(
                    _state.ScheduleSpec, currentTime, importService.TimeZone, importService.TimeAbacus);
                output = true;
            }

            return output;
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(OutputConditionPolledCrontab));
    }
} // end of namespace
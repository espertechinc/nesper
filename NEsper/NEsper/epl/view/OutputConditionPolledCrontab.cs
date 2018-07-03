///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.schedule;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>Output condition handling crontab-at schedule output.</summary>
    public sealed class OutputConditionPolledCrontab : OutputConditionPolled
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly OutputConditionPolledCrontabState _state;
    
        public OutputConditionPolledCrontab(AgentInstanceContext agentInstanceContext, OutputConditionPolledCrontabState state) {
            _agentInstanceContext = agentInstanceContext;
            _state = state;
        }

        public OutputConditionPolledState State => _state;

        public bool UpdateOutputCondition(int newEventsCount, int oldEventsCount) {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".updateOutputCondition, " +
                        "  newEventsCount==" + newEventsCount +
                        "  oldEventsCount==" + oldEventsCount);
            }
    
            var output = false;
            var currentTime = _agentInstanceContext.StatementContext.SchedulingService.Time;
            var engineImportService = _agentInstanceContext.StatementContext.EngineImportService;
            if (_state.CurrentReferencePoint == null) {
                _state.CurrentReferencePoint = currentTime;
                _state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(_state.ScheduleSpec, currentTime, engineImportService.TimeZone, engineImportService.TimeAbacus);
                output = true;
            }
    
            if (_state.NextScheduledTime <= currentTime) {
                _state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(_state.ScheduleSpec, currentTime, engineImportService.TimeZone, engineImportService.TimeAbacus);
                output = true;
            }
    
            return output;
        }
    }
} // end of namespace

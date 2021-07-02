///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
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
        private readonly ExprEvaluatorContext _exprEvaluatorContext;
        private readonly OutputConditionPolledCrontabState _state;

        public OutputConditionPolledCrontab(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledCrontabState state)
        {
            _exprEvaluatorContext = exprEvaluatorContext;
            _state = state;
        }

        public OutputConditionPolledState State {
            get => _state;
        }

        public bool UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            if ((ExecutionPathDebugLog.IsDebugEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(
                    ".updateOutputCondition, " +
                    "  newEventsCount==" +
                    newEventsCount +
                    "  oldEventsCount==" +
                    oldEventsCount);
            }

            var output = false;
            var currentTime = _exprEvaluatorContext.TimeProvider.Time;
            if (_state.CurrentReferencePoint == null) {
                _state.CurrentReferencePoint = currentTime;
                _state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(
                    _state.ScheduleSpec,
                    currentTime,
                    _exprEvaluatorContext.TimeZone,
                    _exprEvaluatorContext.TimeAbacus);
                output = true;
            }

            if (_state.NextScheduledTime <= currentTime) {
                _state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(
                    _state.ScheduleSpec,
                    currentTime,
                    _exprEvaluatorContext.TimeZone,
                    _exprEvaluatorContext.TimeAbacus);
                output = true;
            }

            return output;
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(OutputConditionPolledCrontab));
    }
} // end of namespace
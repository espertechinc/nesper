///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.polled
{
    /// <summary>
    /// Output condition handling crontab-at schedule output.
    /// </summary>
    public class OutputConditionPolledCrontab : OutputConditionPolled
    {
        private readonly ExprEvaluatorContext exprEvaluatorContext;
        private readonly OutputConditionPolledCrontabState state;

        public OutputConditionPolledCrontab(
            ExprEvaluatorContext exprEvaluatorContext,
            OutputConditionPolledCrontabState state)
        {
            this.exprEvaluatorContext = exprEvaluatorContext;
            this.state = state;
        }

        public OutputConditionPolledState State => state;

        public bool UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".updateOutputCondition, " +
                    "  newEventsCount==" +
                    newEventsCount +
                    "  oldEventsCount==" +
                    oldEventsCount);
            }

            var output = false;
            var currentTime = exprEvaluatorContext.TimeProvider.Time;
            if (state.CurrentReferencePoint == null) {
                state.CurrentReferencePoint = currentTime;
                state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(
                    state.ScheduleSpec,
                    currentTime,
                    exprEvaluatorContext.TimeZone,
                    exprEvaluatorContext.TimeAbacus);
                output = true;
            }

            if (state.NextScheduledTime <= currentTime) {
                state.NextScheduledTime = ScheduleComputeHelper.ComputeNextOccurance(
                    state.ScheduleSpec,
                    currentTime,
                    exprEvaluatorContext.TimeZone,
                    exprEvaluatorContext.TimeAbacus);
                output = true;
            }

            return output;
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace
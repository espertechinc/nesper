///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    /// Output condition that is satisfied at the end
    /// of every time interval of a given length.
    /// </summary>
    public sealed class OutputConditionTime : OutputConditionBase,
        OutputCondition
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "time";
        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = true;

        private readonly AgentInstanceContext context;
        private readonly OutputConditionTimeFactory parent;

        private readonly long scheduleSlot;
        private long? currentReferencePoint;
        private bool isCallbackScheduled;
        private EPStatementHandleCallbackSchedule handle;
        private long currentScheduledTime;

        public OutputConditionTime(
            OutputCallback outputCallback,
            AgentInstanceContext context,
            OutputConditionTimeFactory outputConditionTimeFactory,
            bool isStartConditionOnCreation)
            : base(outputCallback)
        {
            this.context = context;
            parent = outputConditionTimeFactory;

            scheduleSlot = context.StatementContext.ScheduleBucket.AllocateSlot();
            if (isStartConditionOnCreation) {
                UpdateOutputCondition(0, 0);
            }
        }

        public override void UpdateOutputCondition(
            int newEventsCount,
            int oldEventsCount)
        {
            if (currentReferencePoint == null) {
                currentReferencePoint = context.StatementContext.SchedulingService.Time;
            }

            // If we pull the interval from a variable, then we may need to reschedule
            if (parent.IsVariable) {
                var now = context.StatementContext.SchedulingService.Time;
                var delta = parent.TimePeriodCompute.DeltaAddWReference(
                    now,
                    currentReferencePoint.Value,
                    null,
                    true,
                    context);
                if (delta.Delta != currentScheduledTime) {
                    if (isCallbackScheduled) {
                        // reschedule
                        context.AuditProvider.ScheduleRemove(
                            context,
                            handle,
                            ScheduleObjectType.outputratelimiting,
                            NAME_AUDITPROVIDER_SCHEDULE);
                        context.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
                        ScheduleCallback();
                    }
                }
            }

            // Schedule the next callback if there is none currently scheduled
            if (!isCallbackScheduled) {
                ScheduleCallback();
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }

        private void ScheduleCallback()
        {
            isCallbackScheduled = true;
            var current = context.StatementContext.SchedulingService.Time;
            var delta = parent.TimePeriodCompute.DeltaAddWReference(
                current,
                currentReferencePoint.Value,
                null,
                true,
                context);
            var deltaTime = delta.Delta;
            currentReferencePoint = delta.LastReference;
            currentScheduledTime = deltaTime;

            if (ExecutionPathDebugLog.IsDebugEnabled && log.IsDebugEnabled) {
                log.Debug(
                    ".scheduleCallback Scheduled new callback for " +
                    " afterMsec=" +
                    deltaTime +
                    " now=" +
                    current +
                    " currentReferencePoint=" +
                    currentReferencePoint);
            }

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback() {
                ProcScheduledTrigger = () => {
                    context.InstrumentationProvider.QOutputRateConditionScheduledEval();
                    context.AuditProvider.ScheduleFire(
                        context,
                        ScheduleObjectType.outputratelimiting,
                        NAME_AUDITPROVIDER_SCHEDULE);
                    isCallbackScheduled = false;
                    outputCallback.Invoke(DO_OUTPUT, FORCE_UPDATE);
                    ScheduleCallback();
                    context.InstrumentationProvider.AOutputRateConditionScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(context.EpStatementAgentInstanceHandle, callback);
            context.AuditProvider.ScheduleAdd(
                deltaTime,
                context,
                handle,
                ScheduleObjectType.outputratelimiting,
                NAME_AUDITPROVIDER_SCHEDULE);
            context.StatementContext.SchedulingService.Add(deltaTime, handle, scheduleSlot);
        }

        public override void StopOutputCondition()
        {
            if (handle != null) {
                context.AuditProvider.ScheduleRemove(
                    context,
                    handle,
                    ScheduleObjectType.outputratelimiting,
                    NAME_AUDITPROVIDER_SCHEDULE);
                context.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
            }
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(OutputConditionTime));
    }
} // end of namespace
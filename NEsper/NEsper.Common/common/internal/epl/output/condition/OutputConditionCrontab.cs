///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.condition
{
    /// <summary>
    ///     Output condition handling crontab-at schedule output.
    /// </summary>
    public sealed class OutputConditionCrontab : OutputConditionBase,
        OutputCondition
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "crontab";

        private const bool DO_OUTPUT = true;
        private const bool FORCE_UPDATE = true;

        private static readonly ILog log = LogManager.GetLogger(typeof(OutputConditionCrontab));

        private readonly AgentInstanceContext context;

        private readonly long scheduleSlot;
        private readonly ScheduleSpec scheduleSpec;
        private long? currentReferencePoint;
        private bool isCallbackScheduled;

        public OutputConditionCrontab(
            OutputCallback outputCallback,
            AgentInstanceContext context,
            bool isStartConditionOnCreation,
            ScheduleSpec scheduleSpec)
            : base(outputCallback)
        {
            this.context = context;
            this.scheduleSpec = scheduleSpec;
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

            // Schedule the next callback if there is none currently scheduled
            if (!isCallbackScheduled) {
                ScheduleCallback();
            }
        }

        public override void Terminated()
        {
            outputCallback.Invoke(true, true);
        }

        public override void StopOutputCondition()
        {
            // no action required
        }

        public override string ToString()
        {
            return GetType().Name +
                   " spec=" +
                   scheduleSpec;
        }

        private void ScheduleCallback()
        {
            isCallbackScheduled = true;

            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
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
            var handle = new EPStatementHandleCallbackSchedule(context.EpStatementAgentInstanceHandle, callback);
            var schedulingService = context.StatementContext.SchedulingService;
            var classpathImportService = context.StatementContext.ImportServiceRuntime;
            long nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                scheduleSpec,
                schedulingService.Time,
                classpathImportService.TimeZone,
                classpathImportService.TimeAbacus);
            context.AuditProvider.ScheduleAdd(
                nextScheduledTime,
                context,
                handle,
                ScheduleObjectType.outputratelimiting,
                NAME_AUDITPROVIDER_SCHEDULE);
            schedulingService.Add(nextScheduledTime, handle, scheduleSlot);
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextControllerConditionCrontabImpl : ContextControllerConditionNonHA,
        ContextControllerConditionCrontab
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "context-condition crontab";
        private readonly ContextControllerConditionCallback callback;

        private readonly IntSeqKey conditionPath;
        private readonly ContextController controller;
        private readonly ContextConditionDescriptorCrontab crontab;
        private readonly long scheduleSlot;

        private EPStatementHandleCallbackSchedule scheduleHandle;

        public ContextControllerConditionCrontabImpl(
            IntSeqKey conditionPath,
            long scheduleSlot,
            ScheduleSpec scheduleSpec,
            ContextConditionDescriptorCrontab crontab,
            ContextControllerConditionCallback callback,
            ContextController controller)
        {
            this.conditionPath = conditionPath;
            this.scheduleSlot = scheduleSlot;
            Schedule = scheduleSpec;
            this.crontab = crontab;
            this.callback = callback;
            this.controller = controller;
        }

        public ContextConditionDescriptor Descriptor => crontab;

        public ScheduleSpec Schedule { get; }

        public bool Activate(
            EventBean optionalTriggeringEvent,
            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider)
        {
            ScheduleHandleCallback scheduleCallback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
                    agentInstanceContext.InstrumentationProvider.QContextScheduledEval(
                        agentInstanceContext.StatementContext.ContextRuntimeDescriptor);

                    scheduleHandle = null; // terminates automatically unless scheduled again
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext, ScheduleObjectType.context, NAME_AUDITPROVIDER_SCHEDULE);
                    callback.RangeNotification(conditionPath, this, null, null, null, null);

                    agentInstanceContext.InstrumentationProvider.AContextScheduledEval();
                }
            };
            var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
            scheduleHandle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle, scheduleCallback);
            long nextScheduledTime = ScheduleComputeHelper.ComputeDeltaNextOccurance(
                Schedule, agentInstanceContext.TimeProvider.Time,
                agentInstanceContext.ImportServiceRuntime.TimeZone,
                agentInstanceContext.ImportServiceRuntime.TimeAbacus);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                nextScheduledTime, agentInstanceContext, scheduleHandle, ScheduleObjectType.context,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.SchedulingService.Add(nextScheduledTime, scheduleHandle, scheduleSlot);
            return false;
        }

        public void Deactivate()
        {
            if (scheduleHandle != null) {
                var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext, scheduleHandle, ScheduleObjectType.context, NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
            }

            scheduleHandle = null;
        }

        public bool IsImmediate => crontab.IsImmediate;

        public bool IsRunning => scheduleHandle != null;

        public long? ExpectedEndTime => crontab.GetExpectedEndTime(controller.Realization, Schedule);
    }
} // end of namespace
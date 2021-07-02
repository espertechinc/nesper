///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.controller.initterm;
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
            ScheduleSpec[] schedulesSpecs,
            ContextConditionDescriptorCrontab crontab,
            ContextControllerConditionCallback callback,
            ContextController controller)
        {
            this.conditionPath = conditionPath;
            this.scheduleSlot = scheduleSlot;
            Schedules = schedulesSpecs;
            this.crontab = crontab;
            this.callback = callback;
            this.controller = controller;
        }

        public ContextConditionDescriptor Descriptor => crontab;

        public ScheduleSpec[] Schedules { get; }

        public bool Activate(
            EventBean optionalTriggeringEvent,
            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider, 
            IDictionary<string, object> optionalTriggeringPattern)
        {
            ScheduleHandleCallback scheduleCallback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    var inProcAgentInstanceContext = controller.Realization.AgentInstanceContextCreate;
                    inProcAgentInstanceContext.InstrumentationProvider.QContextScheduledEval(
                        inProcAgentInstanceContext.StatementContext.ContextRuntimeDescriptor);

                    scheduleHandle = null; // terminates automatically unless scheduled again
                    inProcAgentInstanceContext.AuditProvider.ScheduleFire(
                        inProcAgentInstanceContext,
                        ScheduleObjectType.context,
                        NAME_AUDITPROVIDER_SCHEDULE);
                    callback.RangeNotification(conditionPath, this, null, null, null, null, null);

                    inProcAgentInstanceContext.InstrumentationProvider.AContextScheduledEval();
                }
            };
            var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
            scheduleHandle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                scheduleCallback);
            long nextScheduledTime = ContextControllerInitTermUtil.ComputeScheduleMinimumDelta(
                Schedules, agentInstanceContext.TimeProvider.Time, agentInstanceContext.ImportServiceRuntime);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                nextScheduledTime,
                agentInstanceContext,
                scheduleHandle,
                ScheduleObjectType.context,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.SchedulingService.Add(nextScheduledTime, scheduleHandle, scheduleSlot);
            return false;
        }

        public void Deactivate()
        {
            if (scheduleHandle != null) {
                var agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.context,
                    NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
            }

            scheduleHandle = null;
        }

        public void Transfer(AgentInstanceTransferServices xfer)
        {
        }

        public bool IsImmediate => crontab.IsImmediate;

        public bool IsRunning => scheduleHandle != null;

        public long? ExpectedEndTime => crontab.GetExpectedEndTime(controller.Realization, Schedules);
    }
} // end of namespace
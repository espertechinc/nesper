///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.controller.condition
{
    public class ContextControllerConditionTimePeriod : ContextControllerConditionNonHA
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "context-condition time-period";

        private readonly long scheduleSlot;
        private readonly ContextConditionDescriptorTimePeriod timePeriod;
        private readonly IntSeqKey conditionPath;
        private readonly ContextControllerConditionCallback callback;
        private readonly ContextController controller;

        private EPStatementHandleCallbackSchedule scheduleHandle;

        public ContextControllerConditionTimePeriod(
            long scheduleSlot,
            ContextConditionDescriptorTimePeriod timePeriod,
            IntSeqKey conditionPath,
            ContextControllerConditionCallback callback,
            ContextController controller)
        {
            this.scheduleSlot = scheduleSlot;
            this.timePeriod = timePeriod;
            this.conditionPath = conditionPath;
            this.callback = callback;
            this.controller = controller;
        }

        public bool Activate(
            EventBean optionalTriggeringEvent,
            ContextControllerEndConditionMatchEventProvider endConditionMatchEventProvider)
        {
            ScheduleHandleCallback scheduleCallback = new ProxyScheduleHandleCallback() {
                ProcScheduledTrigger = () => {
                    AgentInstanceContext agentInstanceContextX = controller.Realization.AgentInstanceContextCreate;
                    agentInstanceContextX.InstrumentationProvider.QContextScheduledEval(
                        agentInstanceContextX.StatementContext.ContextRuntimeDescriptor);

                    scheduleHandle = null; // terminates automatically unless scheduled again
                    agentInstanceContextX.AuditProvider.ScheduleFire(
                        agentInstanceContextX,
                        ScheduleObjectType.context,
                        NAME_AUDITPROVIDER_SCHEDULE);
                    callback.RangeNotification(conditionPath, this, null, null, null, null);

                    agentInstanceContextX.InstrumentationProvider.AContextScheduledEval();
                },
            };
            AgentInstanceContext agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
            scheduleHandle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle,
                scheduleCallback);
            long timeDelta = timePeriod.TimePeriodCompute.DeltaUseRuntimeTime(
                null,
                agentInstanceContext,
                agentInstanceContext.TimeProvider);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                timeDelta,
                agentInstanceContext,
                scheduleHandle,
                ScheduleObjectType.context,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.SchedulingService.Add(timeDelta, scheduleHandle, scheduleSlot);
            return false;
        }

        public void Deactivate()
        {
            if (scheduleHandle != null) {
                AgentInstanceContext agentInstanceContext = controller.Realization.AgentInstanceContextCreate;
                agentInstanceContext.AuditProvider.ScheduleRemove(
                    agentInstanceContext,
                    scheduleHandle,
                    ScheduleObjectType.context,
                    NAME_AUDITPROVIDER_SCHEDULE);
                agentInstanceContext.SchedulingService.Remove(scheduleHandle, scheduleSlot);
            }

            scheduleHandle = null;
        }

        public bool IsImmediate {
            get => timePeriod.IsImmediate;
        }

        public bool IsRunning {
            get => scheduleHandle != null;
        }

        public long? ExpectedEndTime {
            get => timePeriod.GetExpectedEndTime(controller.Realization);
        }

        public ContextConditionDescriptor Descriptor {
            get => timePeriod;
        }
    }
} // end of namespace
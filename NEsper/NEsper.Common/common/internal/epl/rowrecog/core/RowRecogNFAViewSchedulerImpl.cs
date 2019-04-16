///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.epl.rowrecog.core
{
    public class RowRecogNFAViewSchedulerImpl : RowRecogNFAViewScheduler
    {
        public const string NAME_AUDITPROVIDER_SCHEDULE = "interval";

        private AgentInstanceContext agentInstanceContext;
        private EPStatementHandleCallbackSchedule handle;
        private long scheduleSlot;

        public void SetScheduleCallback(
            AgentInstanceContext agentInstanceContext,
            RowRecogNFAViewScheduleCallback scheduleCallback)
        {
            this.agentInstanceContext = agentInstanceContext;
            scheduleSlot = agentInstanceContext.StatementContext.ScheduleBucket.AllocateSlot();
            ScheduleHandleCallback callback = new ProxyScheduleHandleCallback {
                ProcScheduledTrigger = () => {
                    agentInstanceContext.AuditProvider.ScheduleFire(
                        agentInstanceContext, ScheduleObjectType.matchrecognize, NAME_AUDITPROVIDER_SCHEDULE);
                    agentInstanceContext.InstrumentationProvider.QRegExScheduledEval();
                    scheduleCallback.Triggered();
                    agentInstanceContext.InstrumentationProvider.ARegExScheduledEval();
                }
            };
            handle = new EPStatementHandleCallbackSchedule(
                agentInstanceContext.EpStatementAgentInstanceHandle, callback);
        }

        public void AddSchedule(long timeDelta)
        {
            agentInstanceContext.AuditProvider.ScheduleAdd(
                timeDelta, agentInstanceContext, handle, ScheduleObjectType.matchrecognize,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, handle, scheduleSlot);
        }

        public void ChangeSchedule(long timeDelta)
        {
            agentInstanceContext.AuditProvider.ScheduleRemove(
                agentInstanceContext, handle, ScheduleObjectType.matchrecognize, NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
            agentInstanceContext.AuditProvider.ScheduleAdd(
                timeDelta, agentInstanceContext, handle, ScheduleObjectType.matchrecognize,
                NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.StatementContext.SchedulingService.Add(timeDelta, handle, scheduleSlot);
        }

        public void RemoveSchedule()
        {
            agentInstanceContext.AuditProvider.ScheduleRemove(
                agentInstanceContext, handle, ScheduleObjectType.matchrecognize, NAME_AUDITPROVIDER_SCHEDULE);
            agentInstanceContext.StatementContext.SchedulingService.Remove(handle, scheduleSlot);
        }
    }
} // end of namespace
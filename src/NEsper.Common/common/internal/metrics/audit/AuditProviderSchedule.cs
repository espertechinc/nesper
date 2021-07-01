///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.metrics.audit
{
    public interface AuditProviderSchedule
    {
        void ScheduleAdd(
            long nextScheduledTime,
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name);

        void ScheduleRemove(
            AgentInstanceContext agentInstanceContext,
            ScheduleHandle scheduleHandle,
            ScheduleObjectType objectType,
            string name);

        void ScheduleFire(
            AgentInstanceContext agentInstanceContext,
            ScheduleObjectType objectType,
            string name);
    }
} // end of namespace
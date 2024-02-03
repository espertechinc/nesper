///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.context.util
{
    public class EPStatementHandleCallbackSchedule : ScheduleHandle
    {
        private EPStatementAgentInstanceHandle agentInstanceHandle;
        private ScheduleHandleCallback scheduleCallback;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceHandle">is a statement handle</param>
        /// <param name="callback">is a schedule callback</param>
        public EPStatementHandleCallbackSchedule(
            EPStatementAgentInstanceHandle agentInstanceHandle,
            ScheduleHandleCallback callback)
        {
            this.agentInstanceHandle = agentInstanceHandle;
            scheduleCallback = callback;
        }

        public int StatementId => agentInstanceHandle.StatementId;

        public int AgentInstanceId => agentInstanceHandle.AgentInstanceId;

        /// <summary>
        /// Returns the statement handle.
        /// </summary>
        /// <returns>handle containing a statement resource lock</returns>
        public EPStatementAgentInstanceHandle AgentInstanceHandle => agentInstanceHandle;

        public ScheduleHandleCallback ScheduleCallback => scheduleCallback;
    }
} // end of namespace
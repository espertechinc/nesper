///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
        private readonly EPStatementAgentInstanceHandle agentInstanceHandle;
        private readonly ScheduleHandleCallback scheduleCallback;

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

        public int StatementId {
            get => agentInstanceHandle.StatementId;
        }

        public int AgentInstanceId {
            get => agentInstanceHandle.AgentInstanceId;
        }

        /// <summary>
        /// Returns the statement handle.
        /// </summary>
        /// <returns>handle containing a statement resource lock</returns>
        public EPStatementAgentInstanceHandle AgentInstanceHandle {
            get => agentInstanceHandle;
        }

        public ScheduleHandleCallback ScheduleCallback {
            get => scheduleCallback;
        }
    }
} // end of namespace
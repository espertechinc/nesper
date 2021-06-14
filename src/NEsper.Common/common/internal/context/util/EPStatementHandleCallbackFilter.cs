///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.filtersvc;

namespace com.espertech.esper.common.@internal.context.util
{
    /// <summary>
    /// Statement resource handle and callback for use with filter services.
    /// <para />Links the statement handle identifying a statement and containing the statement resource lock,
    /// with the actual callback to invoke for a statement together.
    /// </summary>
    public class EPStatementHandleCallbackFilter : FilterHandle
    {
        private EPStatementAgentInstanceHandle agentInstanceHandle;

        private FilterHandleCallback filterCallback;
        // private ScheduleHandleCallback scheduleCallback;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceHandle">is a statement handle</param>
        /// <param name="callback">is a filter callback</param>
        public EPStatementHandleCallbackFilter(
            EPStatementAgentInstanceHandle agentInstanceHandle,
            FilterHandleCallback callback)
        {
            this.agentInstanceHandle = agentInstanceHandle;
            this.filterCallback = callback;
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

        /// <summary>
        /// Returns the statement filter callback, or null if this is a schedule callback handle.
        /// </summary>
        /// <returns>filter callback</returns>
        public FilterHandleCallback FilterCallback {
            get => filterCallback;
        }

        public void SetFilterCallback(FilterHandleCallback filterCallback)
        {
            this.filterCallback = filterCallback;
        }
    }
} // end of namespace
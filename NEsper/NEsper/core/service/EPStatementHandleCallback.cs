///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.filter;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Statement resource handle and callback for use with <seealso cref="com.espertech.esper.filter.FilterService" />
    /// and <seealso cref="com.espertech.esper.schedule.SchedulingService" />.
    /// <para/>
    /// Links the statement handle identifying a statement and containing the statement resource lock, with 
    /// the actual callback to invoke for a statement together.
    /// </summary>
    public class EPStatementHandleCallback : FilterHandle, ScheduleHandle
    {
        private EPStatementAgentInstanceHandle _agentInstanceHandle;
        private FilterHandleCallback _filterCallback;
        private ScheduleHandleCallback _scheduleCallback;
    
        /// <summary>Ctor. </summary>
        /// <param name="agentInstanceHandle">is a statement handle</param>
        /// <param name="callback">is a filter callback</param>
        public EPStatementHandleCallback(EPStatementAgentInstanceHandle agentInstanceHandle, FilterHandleCallback callback)
        {
            _agentInstanceHandle = agentInstanceHandle;
            _filterCallback = callback;
        }
    
        /// <summary>Ctor. </summary>
        /// <param name="agentInstanceHandle">is a statement handle</param>
        /// <param name="callback">is a schedule callback</param>
        public EPStatementHandleCallback(EPStatementAgentInstanceHandle agentInstanceHandle, ScheduleHandleCallback callback)
        {
            _agentInstanceHandle = agentInstanceHandle;
            _scheduleCallback = callback;
        }

        public int StatementId
        {
            get { return _agentInstanceHandle.StatementId; }
        }

        public int AgentInstanceId
        {
            get { return _agentInstanceHandle.AgentInstanceId; }
        }

        /// <summary>Returns the statement handle. </summary>
        /// <value>handle containing a statement resource lock</value>
        public EPStatementAgentInstanceHandle AgentInstanceHandle
        {
            get { return _agentInstanceHandle; }
        }

        /// <summary>Returns the statement filter callback, or null if this is a schedule callback handle. </summary>
        /// <value>filter callback</value>
        public FilterHandleCallback FilterCallback
        {
            get { return _filterCallback; }
            set { _filterCallback = value; }
        }

        /// <summary>Returns the statement schedule callback, or null if this is a filter callback handle. </summary>
        /// <value>schedule callback</value>
        public ScheduleHandleCallback ScheduleCallback
        {
            get { return _scheduleCallback; }
            set { _scheduleCallback = value; }
        }
    }
}

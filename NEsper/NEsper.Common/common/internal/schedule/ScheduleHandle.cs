///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.schedule
{
    /// <summary>
    /// Marker interface for use with <seealso cref="SchedulingService" />. Implementations
    /// serve as a schedule trigger values when the schedule is reached to trigger or return
    /// the handle.
    /// </summary>
    public interface ScheduleHandle
    {
        /// <summary>Returns the statement id. </summary>
        /// <value>statement id</value>
        int StatementId { get; }

        /// <summary>Returns the agent instance id. </summary>
        /// <value>agent instance id</value>
        int AgentInstanceId { get; }
    }
}
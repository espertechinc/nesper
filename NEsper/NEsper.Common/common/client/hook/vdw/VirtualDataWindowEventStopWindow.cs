///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>
    ///     This event is raised when a virtual data window is stopped.
    /// </summary>
    public class VirtualDataWindowEventStopWindow : VirtualDataWindowEvent
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="namedWindowName">named window name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        public VirtualDataWindowEventStopWindow(string namedWindowName, int agentInstanceId)
        {
            NamedWindowName = namedWindowName;
            AgentInstanceId = agentInstanceId;
        }

        /// <summary>
        ///     Returns the named window name.
        /// </summary>
        /// <returns>named window name</returns>
        public string NamedWindowName { get; }

        /// <summary>
        ///     Returns the agent instance id
        /// </summary>
        /// <returns>agent instance id</returns>
        public int AgentInstanceId { get; }
    }
} // end of namespace
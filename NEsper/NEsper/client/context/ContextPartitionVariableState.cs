///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.context
{
    /// <summary>
    /// The variable state for a context partitioned variable.
    /// </summary>
    public class ContextPartitionVariableState
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="agentInstanceId">agent instance id</param>
        /// <param name="identifier">context partition identification</param>
        /// <param name="state">variable state</param>
        public ContextPartitionVariableState(int agentInstanceId, ContextPartitionIdentifier identifier, object state)
        {
            AgentInstanceId = agentInstanceId;
            Identifier = identifier;
            State = state;
        }

        /// <summary>
        /// Returns the agent instance id
        /// </summary>
        /// <value>id</value>
        public int AgentInstanceId { get; private set; }

        /// <summary>
        /// Returns context partition identifier
        /// </summary>
        /// <value>context partition info</value>
        public ContextPartitionIdentifier Identifier { get; private set; }

        /// <summary>
        /// Returns the variable state
        /// </summary>
        /// <value>state</value>
        public object State { get; private set; }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.client.context
{
    /// <summary>Descriptor encapsulates information about a context partition. </summary>
    public class ContextPartitionDescriptor
    {
        /// <summary>Ctor. </summary>
        /// <param name="agentInstanceId">context partition id</param>
        /// <param name="identifier">identifier object specific to context declaration</param>
        /// <param name="state">current state</param>
        public ContextPartitionDescriptor(int agentInstanceId, ContextPartitionIdentifier identifier, ContextPartitionState state)
        {
            AgentInstanceId = agentInstanceId;
            Identifier = identifier;
            State = state;
        }

        /// <summary>Returns the context partition id. </summary>
        /// <value>id</value>
        public int AgentInstanceId { get; private set; }

        /// <summary>Returns an identifier object that identifies the context partition. </summary>
        /// <value>identifier</value>
        public ContextPartitionIdentifier Identifier { get; private set; }

        /// <summary>Returns context partition state. </summary>
        /// <value>state</value>
        public ContextPartitionState State { get; set; }
    }
}

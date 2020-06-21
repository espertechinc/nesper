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
    /// Base class for events indicating a named-window consumer management.
    /// </summary>
    public abstract class VirtualDataWindowEventConsumerBase : VirtualDataWindowEvent
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="namedWindowName">the named window name</param>
        /// <param name="consumerObject">an object that identifies the consumer, the same instance or the add and for the remove event</param>
        /// <param name="statementName">statement name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        protected VirtualDataWindowEventConsumerBase(
            string namedWindowName,
            object consumerObject,
            string statementName,
            int agentInstanceId)
        {
            NamedWindowName = namedWindowName;
            ConsumerObject = consumerObject;
            StatementName = statementName;
            AgentInstanceId = agentInstanceId;
        }

        /// <summary>
        /// Returns the named window name.
        /// </summary>
        /// <value>named window name</value>
        public string NamedWindowName { get; private set; }

        /// <summary>
        /// Returns an object that serves as a unique identifier for the consumer, with multiple consumer 
        /// per statements possible.
        /// <para/>
        /// Upon remove the removal event contains the same consumer object.
        /// </summary>
        /// <value>consumer object</value>
        public object ConsumerObject { get; private set; }

        /// <summary>
        /// Returns the statement name.
        /// </summary>
        /// <value>statement name</value>
        public string StatementName { get; private set; }

        /// <summary>
        /// Returns the agent instance id (context partition id).
        /// </summary>
        /// <value>agent instance id</value>
        public int AgentInstanceId { get; private set; }
    }
}
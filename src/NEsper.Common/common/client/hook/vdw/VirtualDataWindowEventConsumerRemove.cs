///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.hook.vdw
{
    /// <summary>Event indicating a named-window consuming statement is being removed. </summary>
    public class VirtualDataWindowEventConsumerRemove : VirtualDataWindowEventConsumerBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="namedWindowName">named window name</param>
        /// <param name="consumerObject">identifying object for consumer</param>
        /// <param name="statementName">statement name</param>
        /// <param name="agentInstanceId">agent instance id</param>
        public VirtualDataWindowEventConsumerRemove(
            string namedWindowName,
            object consumerObject,
            string statementName,
            int agentInstanceId)
            : base(namedWindowName, consumerObject, statementName, agentInstanceId)
        {
        }
    }
}
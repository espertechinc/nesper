///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.common.@internal.context.util
{
    [Serializable]
    public class AgentInstanceComparator : IComparer<AgentInstance>
    {
        public static AgentInstanceComparator INSTANCE = new AgentInstanceComparator();

        private readonly EPStatementAgentInstanceHandleComparator _innerComparator = new EPStatementAgentInstanceHandleComparator();

        public int Compare(
            AgentInstance ai1,
            AgentInstance ai2)
        {
            EPStatementAgentInstanceHandle o1 = ai1.AgentInstanceContext.EpStatementAgentInstanceHandle;
            EPStatementAgentInstanceHandle o2 = ai2.AgentInstanceContext.EpStatementAgentInstanceHandle;
            return _innerComparator.Compare(o1, o2);
        }
    }
}
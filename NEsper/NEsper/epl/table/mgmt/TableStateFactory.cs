///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.table.mgmt
{
    public interface TableStateFactory
    {
        TableStateInstance MakeTableState(AgentInstanceContext agentInstanceContext);
    }

    public class ProxyTableStateFactory : TableStateFactory
    {
        public Func<AgentInstanceContext, TableStateInstance> ProcMakeTableState { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyTableStateFactory"/> class.
        /// </summary>
        /// <param name="procMakeTableState">State of the proc make table.</param>
        public ProxyTableStateFactory(Func<AgentInstanceContext, TableStateInstance> procMakeTableState)
        {
            ProcMakeTableState = procMakeTableState;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyTableStateFactory"/> class.
        /// </summary>
        public ProxyTableStateFactory()
        {
        }

        public TableStateInstance MakeTableState(AgentInstanceContext agentInstanceContext)
        {
            return ProcMakeTableState(agentInstanceContext);
        }
    }
}

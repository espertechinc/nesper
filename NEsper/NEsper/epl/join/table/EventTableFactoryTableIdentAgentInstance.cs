///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.join.table
{
	public class EventTableFactoryTableIdentAgentInstance : EventTableFactoryTableIdent
	{
	    public EventTableFactoryTableIdentAgentInstance(AgentInstanceContext agentInstanceContext)
        {
	        AgentInstanceContext = agentInstanceContext;
	    }

	    public AgentInstanceContext AgentInstanceContext { get; private set; }

	    public StatementContext StatementContext
	    {
	        get { return AgentInstanceContext.StatementContext; }
	    }
	}
} // end of namespace

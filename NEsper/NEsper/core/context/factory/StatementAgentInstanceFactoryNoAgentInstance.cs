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
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;
using com.espertech.esper.view;

namespace com.espertech.esper.core.context.factory
{
	public class StatementAgentInstanceFactoryNoAgentInstance : StatementAgentInstanceFactory
    {
	    private readonly Viewable _sharedFinalView;

	    public StatementAgentInstanceFactoryNoAgentInstance(Viewable sharedFinalView)
        {
	        _sharedFinalView = sharedFinalView;
	    }

	    public StatementAgentInstanceFactoryResult NewContext(AgentInstanceContext agentInstanceContext, bool isRecoveringResilient)
	    {
	        return new StatementAgentInstanceFactoryCreateSchemaResult(_sharedFinalView, CollectionUtil.STOP_CALLBACK_NONE, agentInstanceContext);
	    }

	    public void AssignExpressions(StatementAgentInstanceFactoryResult result)
        {
	    }

	    public void UnassignExpressions()
        {
	    }
	}
} // end of namespace

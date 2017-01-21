///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;

namespace com.espertech.esper.epl.view
{
	public interface OutputConditionPolledFactory
	{
	    OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext);
	    OutputConditionPolled MakeFromState(AgentInstanceContext agentInstanceContext, OutputConditionPolledState state);
	}
} // end of namespace

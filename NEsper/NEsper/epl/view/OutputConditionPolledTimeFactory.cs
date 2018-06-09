///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.expression.time;

namespace com.espertech.esper.epl.view
{
	public sealed class OutputConditionPolledTimeFactory : OutputConditionPolledFactory
	{
	    private readonly ExprTimePeriod _timePeriod;

	    public OutputConditionPolledTimeFactory(ExprTimePeriod timePeriod, StatementContext statementContext) {
	        _timePeriod = timePeriod;
	        var numSeconds = timePeriod.EvaluateAsSeconds(null, true, new ExprEvaluatorContextStatement(statementContext, false));
	        if ((numSeconds < 0.001) && (!timePeriod.HasVariable)) {
	            throw new ArgumentException("Output condition by time requires a interval size of at least 1 msec or a variable");
	        }
	    }

	    public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext)
        {
	        return new OutputConditionPolledTime(this, agentInstanceContext, new OutputConditionPolledTimeState(null));
	    }

	    public OutputConditionPolled MakeFromState(AgentInstanceContext agentInstanceContext, OutputConditionPolledState state)
        {
	        var timeState = (OutputConditionPolledTimeState) state;
	        return new OutputConditionPolledTime(this, agentInstanceContext, timeState);
	    }

	    public ExprTimePeriod TimePeriod => _timePeriod;
	}
} // end of namespace

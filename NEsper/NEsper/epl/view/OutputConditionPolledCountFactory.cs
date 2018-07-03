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
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.view
{
	/// <summary>
	/// Output limit condition that is satisfied when either
	/// the total number of new events arrived or the total number
	/// of old events arrived is greater than a preset value.
	/// </summary>
	public sealed class OutputConditionPolledCountFactory : OutputConditionPolledFactory
	{
	    private readonly int _eventRate;
	    private readonly StatementContext _statementContext;
	    private readonly string _variableName;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="eventRate">is the number of old or new events thatmust arrive in order for the condition to be satisfied</param>
        /// <param name="statementContext">The statement context.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <exception cref="ArgumentException">Limiting output by event count requires an event count of at least 1 or a variable name</exception>
        public OutputConditionPolledCountFactory(int eventRate, StatementContext statementContext, string variableName)
	    {
	        if ((eventRate < 1) && (variableName == null)) {
	            throw new ArgumentException("Limiting output by event count requires an event count of at least 1 or a variable name");
	        }
	        _eventRate = eventRate;
	        _statementContext = statementContext;
	        _variableName = variableName;
	    }

	    public OutputConditionPolled MakeNew(AgentInstanceContext agentInstanceContext)
        {
	        OutputConditionPolledCountState state = new OutputConditionPolledCountState(_eventRate, _eventRate, _eventRate, true);
	        return new OutputConditionPolledCount(this, state, GetVariableReader(agentInstanceContext));
	    }

	    public OutputConditionPolled MakeFromState(AgentInstanceContext agentInstanceContext, OutputConditionPolledState state)
        {
	        return new OutputConditionPolledCount(this, (OutputConditionPolledCountState) state, GetVariableReader(agentInstanceContext));
	    }

	    private VariableReader GetVariableReader(AgentInstanceContext agentInstanceContext)
        {
	        if (_variableName == null)
	            return null;
	        return _statementContext.VariableService.GetReader(_variableName, agentInstanceContext.AgentInstanceId);
	    }
	}
} // end of namespace

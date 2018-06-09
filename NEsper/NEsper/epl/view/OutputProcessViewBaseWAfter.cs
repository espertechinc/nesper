///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.epl.view
{
	public abstract class OutputProcessViewBaseWAfter : OutputProcessViewBase
	{
	    private readonly OutputProcessViewAfterState _afterConditionState;

	    protected OutputProcessViewBaseWAfter(ResultSetProcessorHelperFactory resultSetProcessorHelperFactory, AgentInstanceContext agentInstanceContext, ResultSetProcessor resultSetProcessor, long? afterConditionTime, int? afterConditionNumberOfEvents, bool afterConditionSatisfied)
	        : base(resultSetProcessor)
        {
	        _afterConditionState = resultSetProcessorHelperFactory.MakeOutputConditionAfter(afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied, agentInstanceContext);
	    }

	    public override OutputProcessViewAfterState OptionalAfterConditionState => _afterConditionState;

        /// <summary>
        /// Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the view new events</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>
        /// indicator for output condition
        /// </returns>
        public bool CheckAfterCondition(EventBean[] newEvents, StatementContext statementContext)
	    {
	        return _afterConditionState.CheckUpdateAfterCondition(newEvents, statementContext);
	    }

        /// <summary>
        /// Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newEvents">is the join new events</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>
        /// indicator for output condition
        /// </returns>
        public bool CheckAfterCondition(ISet<MultiKey<EventBean>> newEvents, StatementContext statementContext)
	    {
	        return _afterConditionState.CheckUpdateAfterCondition(newEvents, statementContext);
	    }

        /// <summary>
        /// Returns true if the after-condition is satisfied.
        /// </summary>
        /// <param name="newOldEvents">is the new and old events pair</param>
        /// <param name="statementContext">The statement context.</param>
        /// <returns>
        /// indicator for output condition
        /// </returns>
        public bool CheckAfterCondition(UniformPair<EventBean[]> newOldEvents, StatementContext statementContext)
	    {
	        return _afterConditionState.CheckUpdateAfterCondition(newOldEvents, statementContext);
	    }

	    public override void Stop()
        {
	        _afterConditionState.Destroy();
	    }
	}
} // end of namespace

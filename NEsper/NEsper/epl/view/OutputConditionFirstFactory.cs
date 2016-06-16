///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.spec;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// An output condition that is satisfied at the first event of either a time-based or count-based batch.
    /// </summary>
    public class OutputConditionFirstFactory : OutputConditionFactory
    {
    	private readonly OutputConditionFactory _innerConditionFactory;

        public OutputConditionFirstFactory(OutputLimitSpec outputLimitSpec, StatementContext statementContext, bool isGrouped, bool isWithHavingClause, ResultSetProcessorHelperFactory resultSetProcessorHelperFactory)
    	{
    	    var innerSpec = new OutputLimitSpec(outputLimitSpec.Rate,
    	                                        outputLimitSpec.VariableName,
    	                                        outputLimitSpec.RateType, OutputLimitLimitType.DEFAULT,
    	                                        outputLimitSpec.WhenExpressionNode,
    	                                        outputLimitSpec.ThenExpressions,
    	                                        outputLimitSpec.CrontabAtSchedule,
    	                                        outputLimitSpec.TimePeriodExpr,
    	                                        outputLimitSpec.AfterTimePeriodExpr,
    	                                        outputLimitSpec.AfterNumberOfEvents,
    	                                        outputLimitSpec.IsAndAfterTerminate,
    	                                        outputLimitSpec.AndAfterTerminateExpr,
    	                                        outputLimitSpec.AndAfterTerminateThenExpressions);

    		_innerConditionFactory = OutputConditionFactoryFactory.CreateCondition(innerSpec, statementContext, isGrouped, isWithHavingClause, false, resultSetProcessorHelperFactory);
    	}
    
        public OutputCondition Make(AgentInstanceContext agentInstanceContext, OutputCallback outputCallback) {
            return new OutputConditionFirst(outputCallback, agentInstanceContext, _innerConditionFactory);
        }
    }
}

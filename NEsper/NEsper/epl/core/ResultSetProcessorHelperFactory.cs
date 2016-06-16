///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.service;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.variable;
using com.espertech.esper.epl.view;

namespace com.espertech.esper.epl.core
{
    public interface ResultSetProcessorHelperFactory
    {
	    OutputProcessViewConditionDeltaSet MakeOutputConditionChangeSet(bool isJoin, AgentInstanceContext agentInstanceContext);
	    OutputConditionFactory MakeOutputConditionTime(ExprTimePeriod timePeriodExpr, bool isStartConditionOnCreation);
	    OutputConditionFactory MakeOutputConditionExpression(ExprNode whenExpressionNode, IList<OnTriggerSetAssignment> thenExpressions, StatementContext statementContext, ExprNode andAfterTerminateExpr, IList<OnTriggerSetAssignment> andAfterTerminateThenExpressions, bool isStartConditionOnCreation) ;
	    OutputConditionFactory MakeOutputConditionCrontab(IList<ExprNode> crontabAtSchedule, StatementContext statementContext, bool isStartConditionOnCreation) ;
	    OutputConditionFactory MakeOutputConditionCount(int rate, VariableMetaData variableMetaData, StatementContext statementContext);
	    OutputProcessViewAfterState MakeOutputConditionAfter(long? afterConditionTime, int? afterConditionNumberOfEvents, bool afterConditionSatisfied, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorSimpleOutputLastHelper MakeRSSimpleOutputLast(ResultSetProcessorSimpleFactory prototype, ResultSetProcessorSimple simple, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorSimpleOutputAllHelper MakeRSSimpleOutputAll(ResultSetProcessorSimpleFactory prototype, ResultSetProcessorSimple resultSetProcessorSimple, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorAggregateAllOutputLastHelper MakeRSAggregateAllOutputLast(ResultSetProcessorAggregateAll processor, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorAggregateAllOutputAllHelper MakeRSAggregateAllOutputAll(ResultSetProcessorAggregateAll processor, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorRowForAllOutputLastHelper MakeRSRowForAllOutputLast(ResultSetProcessorRowForAll processor, ResultSetProcessorRowForAllFactory prototype, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorRowForAllOutputAllHelper MakeRSRowForAllOutputAll(ResultSetProcessorRowForAll processor, ResultSetProcessorRowForAllFactory prototype, AgentInstanceContext agentInstanceContext);
	    ResultSetProcessorGroupedOutputAllGroupReps MakeRSGroupedOutputAllNoOpt(AgentInstanceContext agentInstanceContext, ExprEvaluator[] groupKeyExpressions, int numStreams);
	    ResultSetProcessorRowPerGroupOutputAllHelper MakeRSRowPerGroupOutputAllOpt(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup, ResultSetProcessorRowPerGroupFactory prototype);
	    ResultSetProcessorRowPerGroupOutputLastHelper MakeRSRowPerGroupOutputLastOpt(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup, ResultSetProcessorRowPerGroupFactory prototype);
	    ResultSetProcessorRowPerGroupUnboundGroupRep MakeRSRowPerGroupUnboundGroupRep(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupFactory prototype);
	    ResultSetProcessorAggregateGroupedOutputAllHelper MakeRSAggregateGroupedOutputAll(AgentInstanceContext agentInstanceContext, ResultSetProcessorAggregateGrouped resultSetProcessorAggregateGrouped, ResultSetProcessorAggregateGroupedFactory prototype);
	    ResultSetProcessorAggregateGroupedOutputLastHelper MakeRSAggregateGroupedOutputLastOpt(AgentInstanceContext agentInstanceContext, ResultSetProcessorAggregateGrouped resultSetProcessorAggregateGrouped, ResultSetProcessorAggregateGroupedFactory prototype);
	    ResultSetProcessorGroupedOutputFirstHelper MakeRSGroupedOutputFirst(AgentInstanceContext agentInstanceContext, ExprEvaluator[] groupKeyNodes, OutputConditionPolledFactory optionalOutputFirstConditionFactory, AggregationGroupByRollupDesc optionalGroupByRollupDesc, int optionalRollupLevel);
	    ResultSetProcessorRowPerGroupRollupOutputLastHelper MakeRSRowPerGroupRollupLast(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup, ResultSetProcessorRowPerGroupRollupFactory prototype);
	    ResultSetProcessorRowPerGroupRollupOutputAllHelper MakeRSRowPerGroupRollupAll(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup, ResultSetProcessorRowPerGroupRollupFactory prototype);
	    ResultSetProcessorRowPerGroupRollupUnboundHelper MakeRSRowPerGroupRollupSnapshotUnbound(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupRollupFactory prototype);
	}
} // end of namespace

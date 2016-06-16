///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

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
	public class ResultSetProcessorHelperFactoryImpl : ResultSetProcessorHelperFactory
    {
	    public ResultSetProcessorSimpleOutputLastHelper MakeRSSimpleOutputLast(ResultSetProcessorSimpleFactory prototype, ResultSetProcessorSimple simple, AgentInstanceContext agentInstanceContext) {
	        return new ResultSetProcessorSimpleOutputLastHelperImpl(simple);
	    }

	    public ResultSetProcessorSimpleOutputAllHelper MakeRSSimpleOutputAll(ResultSetProcessorSimpleFactory prototype, ResultSetProcessorSimple resultSetProcessorSimple, AgentInstanceContext agentInstanceContext) {
	        return new ResultSetProcessorSimpleOutputAllHelperImpl(resultSetProcessorSimple);
	    }

	    public OutputProcessViewConditionDeltaSet MakeOutputConditionChangeSet(bool isJoin, AgentInstanceContext agentInstanceContext) {
	        return new OutputProcessViewConditionDeltaSetImpl(isJoin);
	    }

	    public OutputConditionFactory MakeOutputConditionTime(ExprTimePeriod timePeriodExpr, bool isStartConditionOnCreation) {
	        return new OutputConditionTimeFactory(timePeriodExpr, isStartConditionOnCreation);
	    }

	    public OutputConditionFactory MakeOutputConditionExpression(ExprNode whenExpressionNode, IList<OnTriggerSetAssignment> thenExpressions, StatementContext statementContext, ExprNode andAfterTerminateExpr, IList<OnTriggerSetAssignment> andAfterTerminateThenExpressions, bool isStartConditionOnCreation) {
	        return new OutputConditionExpressionFactory(whenExpressionNode, thenExpressions, statementContext, andAfterTerminateExpr, andAfterTerminateThenExpressions, isStartConditionOnCreation);
	    }

	    public OutputConditionFactory MakeOutputConditionCrontab(IList<ExprNode> crontabAtSchedule, StatementContext statementContext, bool isStartConditionOnCreation) {
	        return new OutputConditionCrontabFactory(crontabAtSchedule, statementContext, isStartConditionOnCreation);
	    }

	    public OutputConditionFactory MakeOutputConditionCount(int rate, VariableMetaData variableMetaData, StatementContext statementContext) {
	        return new OutputConditionCountFactory(rate, variableMetaData);
	    }

	    public OutputProcessViewAfterState MakeOutputConditionAfter(long? afterConditionTime, int? afterConditionNumberOfEvents, bool afterConditionSatisfied, AgentInstanceContext agentInstanceContext) {
	        if (afterConditionSatisfied) {
	            return OutputProcessViewAfterStateNone.INSTANCE;
	        }
	        return new OutputProcessViewAfterStateImpl(afterConditionTime, afterConditionNumberOfEvents);
	    }

	    public ResultSetProcessorAggregateAllOutputLastHelper MakeRSAggregateAllOutputLast(ResultSetProcessorAggregateAll processor, AgentInstanceContext agentInstanceContext) {
	        return new ResultSetProcessorAggregateAllOutputLastHelperImpl(processor);
	    }

	    public ResultSetProcessorAggregateAllOutputAllHelper MakeRSAggregateAllOutputAll(ResultSetProcessorAggregateAll processor, AgentInstanceContext agentInstanceContext) {
	        return new ResultSetProcessorAggregateAllOutputAllHelperImpl(processor);
	    }

	    public ResultSetProcessorRowForAllOutputLastHelper MakeRSRowForAllOutputLast(ResultSetProcessorRowForAll processor, ResultSetProcessorRowForAllFactory prototype, AgentInstanceContext agentInstanceContext) {
	        return new ResultSetProcessorRowForAllOutputLastHelperImpl(processor);
	    }

	    public ResultSetProcessorRowForAllOutputAllHelper MakeRSRowForAllOutputAll(ResultSetProcessorRowForAll processor, ResultSetProcessorRowForAllFactory prototype, AgentInstanceContext agentInstanceContext) {
	        return new ResultSetProcessorRowForAllOutputAllHelperImpl(processor);
	    }

	    public ResultSetProcessorGroupedOutputAllGroupReps MakeRSGroupedOutputAllNoOpt(AgentInstanceContext agentInstanceContext, ExprEvaluator[] groupKeyExpressions, int numStreams) {
	        return new ResultSetProcessorGroupedOutputAllGroupRepsImpl();
	    }

	    public ResultSetProcessorRowPerGroupOutputAllHelper MakeRSRowPerGroupOutputAllOpt(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup, ResultSetProcessorRowPerGroupFactory prototype) {
	        return new ResultSetProcessorRowPerGroupOutputAllHelperImpl(resultSetProcessorRowPerGroup);
	    }

	    public ResultSetProcessorRowPerGroupOutputLastHelper MakeRSRowPerGroupOutputLastOpt(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup, ResultSetProcessorRowPerGroupFactory prototype) {
	        return new ResultSetProcessorRowPerGroupOutputLastHelperImpl(resultSetProcessorRowPerGroup);
	    }

	    public ResultSetProcessorGroupedOutputFirstHelper MakeRSGroupedOutputFirst(AgentInstanceContext agentInstanceContext, ExprEvaluator[] groupKeyNodes, OutputConditionPolledFactory optionalOutputFirstConditionFactory, AggregationGroupByRollupDesc optionalGroupByRollupDesc, int optionalRollupLevel) {
	        return new ResultSetProcessorGroupedOutputFirstHelperImpl();
	    }

	    public ResultSetProcessorRowPerGroupUnboundGroupRep MakeRSRowPerGroupUnboundGroupRep(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupFactory prototype) {
	        return new ResultSetProcessorRowPerGroupUnboundGroupRepImpl();
	    }

	    public ResultSetProcessorAggregateGroupedOutputAllHelper MakeRSAggregateGroupedOutputAll(AgentInstanceContext agentInstanceContext, ResultSetProcessorAggregateGrouped processor, ResultSetProcessorAggregateGroupedFactory prototype) {
	        return new ResultSetProcessorAggregateGroupedOutputAllHelperImpl(processor);
	    }

	    public ResultSetProcessorAggregateGroupedOutputLastHelper MakeRSAggregateGroupedOutputLastOpt(AgentInstanceContext agentInstanceContext, ResultSetProcessorAggregateGrouped resultSetProcessorAggregateGrouped, ResultSetProcessorAggregateGroupedFactory prototype) {
	        return new ResultSetProcessorAggregateGroupedOutputLastHelperImpl(resultSetProcessorAggregateGrouped);
	    }

	    public ResultSetProcessorRowPerGroupRollupOutputLastHelper MakeRSRowPerGroupRollupLast(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup, ResultSetProcessorRowPerGroupRollupFactory prototype) {
	        return new ResultSetProcessorRowPerGroupRollupOutputLastHelperImpl(resultSetProcessorRowPerGroupRollup, prototype.GroupByRollupDesc.Levels.Length);
	    }

	    public ResultSetProcessorRowPerGroupRollupOutputAllHelper MakeRSRowPerGroupRollupAll(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup, ResultSetProcessorRowPerGroupRollupFactory prototype) {
	        return new ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl(resultSetProcessorRowPerGroupRollup, prototype.GroupByRollupDesc.Levels.Length);
	    }

	    public ResultSetProcessorRowPerGroupRollupUnboundHelper MakeRSRowPerGroupRollupSnapshotUnbound(AgentInstanceContext agentInstanceContext, ResultSetProcessorRowPerGroupRollupFactory prototype) {
	        int levelCount = prototype.GroupByRollupDesc.Levels.Length;
	        return new ResultSetProcessorRowPerGroupRollupUnboundHelperImpl(levelCount);
	    }
	}
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.eval;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.polled;
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.resultset.agggrouped;
using com.espertech.esper.common.@internal.epl.resultset.grouped;
using com.espertech.esper.common.@internal.epl.resultset.rowforall;
using com.espertech.esper.common.@internal.epl.resultset.rowperevent;
using com.espertech.esper.common.@internal.epl.resultset.rowpergroup;
using com.espertech.esper.common.@internal.epl.resultset.rowpergrouprollup;
using com.espertech.esper.common.@internal.epl.resultset.simple;
using com.espertech.esper.common.@internal.epl.variable.core;

namespace com.espertech.esper.common.@internal.epl.resultset.core
{
    public interface ResultSetProcessorHelperFactory
    {
        ResultSetProcessorRowPerGroupUnboundHelper MakeRSRowPerGroupUnboundGroupRep(
            Type[] groupKeyTypes,
            EventType eventType,
            AgentInstanceContext agentInstanceContext);

        ResultSetProcessorGroupedOutputFirstHelper MakeRSGroupedOutputFirst(
            AgentInstanceContext agentInstanceContext,
            Type[] groupKeyTypes,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory,
            AggregationGroupByRollupDesc optionalGroupByRollupDesc,
            int optionalRollupLevel);

        OutputProcessViewConditionDeltaSet MakeOutputConditionChangeSet(
            EventType[] eventTypes,
            AgentInstanceContext agentInstanceContext);

        OutputConditionFactory MakeOutputConditionTime(
            bool hasVariable,
            TimePeriodCompute timePeriodCompute,
            bool isStartConditionOnCreation,
            int scheduleCallbackId);

        ResultSetProcessorRowForAllOutputAllHelper MakeRSRowForAllOutputAll(
            ResultSetProcessorRowForAll processor,
            AgentInstanceContext agentInstanceContext);

        OutputConditionExpressionFactory MakeOutputConditionExpression();

        OutputConditionFactory MakeOutputConditionCrontab(
            ExprEvaluator[] crontabAtSchedule,
            bool isStartConditionOnCreation,
            int scheduleCallbackId);

        OutputConditionFactory MakeOutputConditionCount(
            int rate,
            Variable variableMetaData);

        OutputProcessViewAfterState MakeOutputConditionAfter(
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            AgentInstanceContext agentInstanceContext);

        ResultSetProcessorSimpleOutputLastHelper MakeRSSimpleOutputLast(
            ResultSetProcessorSimple simple,
            AgentInstanceContext agentInstanceContext,
            EventType[] eventTypes);

        ResultSetProcessorSimpleOutputAllHelper MakeRSSimpleOutputAll(
            ResultSetProcessorSimple simple,
            AgentInstanceContext agentInstanceContext,
            EventType[] eventTypes);

        ResultSetProcessorSimpleOutputFirstHelper MakeRSSimpleOutputFirst(AgentInstanceContext agentInstanceContext);

        ResultSetProcessorRowPerEventOutputLastHelper MakeRSRowPerEventOutputLast(
            ResultSetProcessorRowPerEvent processor,
            AgentInstanceContext agentInstanceContext);

        ResultSetProcessorRowPerEventOutputAllHelper MakeRSRowPerEventOutputAll(
            ResultSetProcessorRowPerEvent processor,
            AgentInstanceContext agentInstanceContext);

        ResultSetProcessorRowForAllOutputLastHelper MakeRSRowForAllOutputLast(
            ResultSetProcessorRowForAll processor,
            AgentInstanceContext agentInstanceContext);

        ResultSetProcessorGroupedOutputAllGroupReps MakeRSGroupedOutputAllNoOpt(
            AgentInstanceContext agentInstanceContext,
            Type[] groupKeyTypes,
            EventType[] eventTypes);

        ResultSetProcessorRowPerGroupOutputAllHelper MakeRSRowPerGroupOutputAllOpt(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup,
            Type[] groupKeyTypes,
            EventType[] eventTypes);

        ResultSetProcessorRowPerGroupOutputLastHelper MakeRSRowPerGroupOutputLastOpt(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup,
            Type[] groupKeyTypes,
            EventType[] eventTypes);

        ResultSetProcessorAggregateGroupedOutputAllHelper MakeRSAggregateGroupedOutputAll(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorAggregateGrouped resultSetProcessorAggregateGrouped,
            Type[] groupKeyTypes,
            EventType[] eventTypes);

        ResultSetProcessorAggregateGroupedOutputLastHelper MakeRSAggregateGroupedOutputLastOpt(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorAggregateGrouped resultSetProcessorAggregateGrouped,
            Type[] groupKeyTypes);

        ResultSetProcessorRowPerGroupRollupOutputLastHelper MakeRSRowPerGroupRollupLast(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            EventType[] eventTypes);

        ResultSetProcessorRowPerGroupRollupOutputAllHelper MakeRSRowPerGroupRollupAll(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            EventType[] eventTypes);

        ResultSetProcessorRowPerGroupRollupUnboundHelper MakeRSRowPerGroupRollupSnapshotUnbound(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            int numStreams,
            EventType[] eventTypes);
    }
} // end of namespace
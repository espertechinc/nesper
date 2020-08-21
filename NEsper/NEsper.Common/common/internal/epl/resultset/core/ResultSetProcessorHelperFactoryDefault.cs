///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
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
    public class ResultSetProcessorHelperFactoryDefault : ResultSetProcessorHelperFactory
    {
        public static readonly ResultSetProcessorHelperFactoryDefault INSTANCE =
            new ResultSetProcessorHelperFactoryDefault();

        private ResultSetProcessorHelperFactoryDefault()
        {
        }

        public ResultSetProcessorRowPerGroupUnboundHelper MakeRSRowPerGroupUnboundGroupRep(
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType eventType,
            AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorRowPerGroupUnboundHelperImpl();
        }

        public ResultSetProcessorGroupedOutputFirstHelper MakeRSGroupedOutputFirst(
            AgentInstanceContext agentInstanceContext,
            Type[] groupKeyTypes,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory,
            AggregationGroupByRollupDesc optionalGroupByRollupDesc,
            int optionalRollupLevel,
            DataInputOutputSerde serde)
        {
            return new ResultSetProcessorGroupedOutputFirstHelperImpl();
        }

        public OutputProcessViewConditionDeltaSet MakeOutputConditionChangeSet(
            EventType[] eventTypes,
            AgentInstanceContext agentInstanceContext)
        {
            return new OutputProcessViewConditionDeltaSetImpl(eventTypes.Length > 1);
        }

        public OutputConditionFactory MakeOutputConditionTime(
            bool hasVariable,
            TimePeriodCompute timePeriodCompute,
            bool isStartConditionOnCreation,
            int scheduleCallbackId)
        {
            return new OutputConditionTimeFactory(
                hasVariable,
                timePeriodCompute,
                isStartConditionOnCreation,
                scheduleCallbackId);
        }

        public ResultSetProcessorRowForAllOutputLastHelper MakeRSRowForAllOutputLast(
            ResultSetProcessorRowForAll processor,
            AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorRowForAllOutputLastHelperImpl(processor);
        }

        public ResultSetProcessorRowForAllOutputAllHelper MakeRSRowForAllOutputAll(
            ResultSetProcessorRowForAll processor,
            AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorRowForAllOutputAllHelperImpl(processor);
        }

        public ResultSetProcessorSimpleOutputLastHelper MakeRSSimpleOutputLast(
            ResultSetProcessorSimple simple,
            AgentInstanceContext agentInstanceContext,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorSimpleOutputLastHelperImpl(simple);
        }

        public ResultSetProcessorSimpleOutputAllHelper MakeRSSimpleOutputAll(
            ResultSetProcessorSimple simple,
            AgentInstanceContext agentInstanceContext,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorSimpleOutputAllHelperImpl(simple);
        }

        public ResultSetProcessorSimpleOutputFirstHelper MakeRSSimpleOutputFirst(
            AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorSimpleOutputFirstHelperImpl();
        }

        public OutputConditionExpressionFactory MakeOutputConditionExpression()
        {
            return new OutputConditionExpressionFactory();
        }

        public OutputConditionFactory MakeOutputConditionCrontab(
            ExprEvaluator[] crontabAtSchedule,
            bool isStartConditionOnCreation,
            int scheduleCallbackId)
        {
            return new OutputConditionCrontabFactory(crontabAtSchedule, isStartConditionOnCreation, scheduleCallbackId);
        }

        public OutputConditionFactory MakeOutputConditionCount(
            int rate,
            Variable variableMetaData)
        {
            return new OutputConditionCountFactory(rate, variableMetaData);
        }

        public OutputProcessViewAfterState MakeOutputConditionAfter(
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            AgentInstanceContext agentInstanceContext)
        {
            if (afterConditionSatisfied) {
                return OutputProcessViewAfterStateNone.INSTANCE;
            }

            return new OutputProcessViewAfterStateImpl(afterConditionTime, afterConditionNumberOfEvents);
        }

        ResultSetProcessorRowPerEventOutputLastHelper ResultSetProcessorHelperFactory.MakeRSRowPerEventOutputLast(
            ResultSetProcessorRowPerEvent processor,
            AgentInstanceContext agentInstanceContext)
        {
            return MakeRSRowPerEventOutputLast(processor, agentInstanceContext);
        }

        public ResultSetProcessorRowPerEventOutputAllHelper MakeRSRowPerEventOutputAll(
            ResultSetProcessorRowPerEvent processor,
            AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorRowPerEventOutputAllHelperImpl(processor);
        }

        public ResultSetProcessorGroupedOutputAllGroupReps MakeRSGroupedOutputAllNoOpt(
            AgentInstanceContext agentInstanceContext,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorGroupedOutputAllGroupRepsImpl();
        }

        public ResultSetProcessorRowPerGroupOutputAllHelper MakeRSRowPerGroupOutputAllOpt(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorRowPerGroupOutputAllHelperImpl(resultSetProcessorRowPerGroup);
        }

        public ResultSetProcessorRowPerGroupOutputLastHelper MakeRSRowPerGroupOutputLastOpt(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorRowPerGroupOutputLastHelperImpl(resultSetProcessorRowPerGroup);
        }

        public ResultSetProcessorAggregateGroupedOutputAllHelper MakeRSAggregateGroupedOutputAll(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorAggregateGrouped processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorAggregateGroupedOutputAllHelperImpl(processor);
        }

        public ResultSetProcessorAggregateGroupedOutputLastHelper MakeRSAggregateGroupedOutputLastOpt(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorAggregateGrouped processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde)
        {
            return new ResultSetProcessorAggregateGroupedOutputLastHelperImpl(processor);
        }

        public ResultSetProcessorRowPerGroupRollupOutputLastHelper MakeRSRowPerGroupRollupLast(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorRowPerGroupRollupOutputLastHelperImpl(
                resultSetProcessorRowPerGroupRollup,
                resultSetProcessorRowPerGroupRollup.GroupByRollupDesc.Levels.Length);
        }

        public ResultSetProcessorRowPerGroupRollupOutputAllHelper MakeRSRowPerGroupRollupAll(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            EventType[] eventTypes)
        {
            return new ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl(
                resultSetProcessorRowPerGroupRollup,
                resultSetProcessorRowPerGroupRollup.GroupByRollupDesc.Levels.Length);
        }

        public ResultSetProcessorRowPerGroupRollupUnboundHelper MakeRSRowPerGroupRollupSnapshotUnbound(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            int numStreams,
            EventType[] eventTypes)
        {
            var levelCount = resultSetProcessorRowPerGroupRollup.GroupByRollupDesc.Levels.Length;
            return new ResultSetProcessorRowPerGroupRollupUnboundHelperImpl(levelCount);
        }

        public ResultSetProcessorRowPerEventOutputLastHelperImpl MakeRSRowPerEventOutputLast(
            ResultSetProcessorRowPerEvent processor,
            AgentInstanceContext agentInstanceContext)
        {
            return new ResultSetProcessorRowPerEventOutputLastHelperImpl(processor);
        }
    }
} // end of namespace
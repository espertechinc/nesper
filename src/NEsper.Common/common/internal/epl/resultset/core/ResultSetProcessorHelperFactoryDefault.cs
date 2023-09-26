///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.serde;
using com.espertech.esper.common.client.util;
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
            StateMgmtSetting stateMgmtSettings,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            return new ResultSetProcessorRowPerGroupUnboundHelperImpl();
        }

        public ResultSetProcessorGroupedOutputFirstHelper MakeRSGroupedOutputFirst(
            ExprEvaluatorContext exprEvaluatorContext,
            Type[] groupKeyTypes,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory,
            AggregationGroupByRollupDesc optionalGroupByRollupDesc,
            int optionalRollupLevel,
            DataInputOutputSerde serde,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorGroupedOutputFirstHelperImpl();
        }

        public OutputProcessViewConditionDeltaSet MakeOutputConditionChangeSet(
            EventType[] eventTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSettings)
        {
            return new OutputProcessViewConditionDeltaSetImpl(eventTypes.Length > 1);
        }

        public OutputConditionFactory MakeOutputConditionTime(
            bool hasVariable,
            TimePeriodCompute timePeriodCompute,
            bool isStartConditionOnCreation,
            int scheduleCallbackId,
            StateMgmtSetting stateMgmtSetting)
        {
            return new OutputConditionTimeFactory(
                hasVariable,
                timePeriodCompute,
                isStartConditionOnCreation,
                scheduleCallbackId);
        }

        public ResultSetProcessorRowForAllOutputLastHelper MakeRSRowForAllOutputLast(
            ResultSetProcessorRowForAll processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSetting)
        {
            return new ResultSetProcessorRowForAllOutputLastHelperImpl(processor);
        }

        public ResultSetProcessorRowForAllOutputAllHelper MakeRSRowForAllOutputAll(
            ResultSetProcessorRowForAll processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorRowForAllOutputAllHelperImpl(processor);
        }

        public ResultSetProcessorSimpleOutputLastHelper MakeRSSimpleOutputLast(
            ResultSetProcessorSimple simple,
            ExprEvaluatorContext exprEvaluatorContext,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSetting)
        {
            return new ResultSetProcessorSimpleOutputLastHelperImpl(simple);
        }

        public ResultSetProcessorSimpleOutputAllHelper MakeRSSimpleOutputAll(
            ResultSetProcessorSimple simple,
            ExprEvaluatorContext exprEvaluatorContext,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorSimpleOutputAllHelperImpl(simple);
        }

        public ResultSetProcessorStraightOutputFirstHelper MakeRSStraightOutputFirst(
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSetting)
        {
            return new ResultSetProcessorStraightOutputFirstHelperImpl();
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
            Variable variableMetaData,
            StateMgmtSetting stateMgmtSetting)
        {
            return new OutputConditionCountFactory(rate, variableMetaData, stateMgmtSetting);
        }

        public OutputProcessViewAfterState MakeOutputConditionAfter(
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (afterConditionSatisfied) {
                return OutputProcessViewAfterStateNone.INSTANCE;
            }

            return new OutputProcessViewAfterStateImpl(afterConditionTime, afterConditionNumberOfEvents);
        }

        public ResultSetProcessorRowPerEventOutputLastHelper MakeRSRowPerEventOutputLast(
            ResultSetProcessorRowPerEvent processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSetting)
        {
            return new ResultSetProcessorRowPerEventOutputLastHelperImpl(processor);
        }

        public ResultSetProcessorRowPerEventOutputAllHelper MakeRSRowPerEventOutputAll(
            ResultSetProcessorRowPerEvent processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorRowPerEventOutputAllHelperImpl(processor);
        }

        public ResultSetProcessorGroupedOutputAllGroupReps MakeRSGroupedOutputAllNoOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorGroupedOutputAllGroupRepsImpl();
        }

        public ResultSetProcessorRowPerGroupOutputAllHelper MakeRSRowPerGroupOutputAllOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorRowPerGroupOutputAllHelperImpl(resultSetProcessorRowPerGroup);
        }

        public ResultSetProcessorRowPerGroupOutputLastHelper MakeRSRowPerGroupOutputLastOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroup resultSetProcessorRowPerGroup,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorRowPerGroupOutputLastHelperImpl(resultSetProcessorRowPerGroup);
        }

        public ResultSetProcessorAggregateGroupedOutputAllHelper MakeRSAggregateGroupedOutputAll(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorAggregateGrouped processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorAggregateGroupedOutputAllHelperImpl(processor);
        }

        public ResultSetProcessorAggregateGroupedOutputLastHelper MakeRSAggregateGroupedOutputLastOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorAggregateGrouped processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorAggregateGroupedOutputLastHelperImpl(processor);
        }

        public ResultSetProcessorRowPerGroupRollupOutputLastHelper MakeRSRowPerGroupRollupLast(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorRowPerGroupRollupOutputLastHelperImpl(
                resultSetProcessorRowPerGroupRollup,
                resultSetProcessorRowPerGroupRollup.GroupByRollupDesc.Levels.Length);
        }

        public ResultSetProcessorRowPerGroupRollupOutputAllHelper MakeRSRowPerGroupRollupAll(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            return new ResultSetProcessorRowPerGroupRollupOutputAllHelperImpl(
                resultSetProcessorRowPerGroupRollup,
                resultSetProcessorRowPerGroupRollup.GroupByRollupDesc.Levels.Length);
        }

        public ResultSetProcessorRowPerGroupRollupUnboundHelper MakeRSRowPerGroupRollupSnapshotUnbound(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroupRollup resultSetProcessorRowPerGroupRollup,
            Type[] groupKeyTypes,
            int numStreams,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings)
        {
            var levelCount = resultSetProcessorRowPerGroupRollup.GroupByRollupDesc.Levels.Length;
            return new ResultSetProcessorRowPerGroupRollupUnboundHelperImpl(levelCount);
        }
    }
} // end of namespace
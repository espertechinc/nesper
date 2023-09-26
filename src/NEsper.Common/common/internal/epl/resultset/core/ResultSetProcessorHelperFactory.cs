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
    public interface ResultSetProcessorHelperFactory
    {
        ResultSetProcessorRowPerGroupUnboundHelper MakeRSRowPerGroupUnboundGroupRep(
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType eventType,
            StateMgmtSetting stateMgmtSettings,
            ExprEvaluatorContext exprEvaluatorContext);

        ResultSetProcessorGroupedOutputFirstHelper MakeRSGroupedOutputFirst(
            ExprEvaluatorContext exprEvaluatorContext,
            Type[] groupKeyTypes,
            OutputConditionPolledFactory optionalOutputFirstConditionFactory,
            AggregationGroupByRollupDesc optionalGroupByRollupDesc,
            int optionalRollupLevel,
            DataInputOutputSerde serde,
            StateMgmtSetting stateMgmtSettings);

        OutputProcessViewConditionDeltaSet MakeOutputConditionChangeSet(
            EventType[] eventTypes,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSettings);

        OutputConditionFactory MakeOutputConditionTime(
            bool hasVariable,
            TimePeriodCompute timePeriodCompute,
            bool isStartConditionOnCreation,
            int scheduleCallbackId,
            StateMgmtSetting stateMgmtSetting);

        ResultSetProcessorRowForAllOutputAllHelper MakeRSRowForAllOutputAll(
            ResultSetProcessorRowForAll processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSettings);

        OutputConditionExpressionFactory MakeOutputConditionExpression();

        OutputConditionFactory MakeOutputConditionCrontab(
            ExprEvaluator[] crontabAtSchedule,
            bool isStartConditionOnCreation,
            int scheduleCallbackId);

        OutputConditionFactory MakeOutputConditionCount(
            int rate,
            Variable variableMetaData,
            StateMgmtSetting stateMgmtSetting);

        OutputProcessViewAfterState MakeOutputConditionAfter(
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            ExprEvaluatorContext exprEvaluatorContext);

        ResultSetProcessorSimpleOutputLastHelper MakeRSSimpleOutputLast(
            ResultSetProcessorSimple simple,
            ExprEvaluatorContext exprEvaluatorContext,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSetting);

        ResultSetProcessorSimpleOutputAllHelper MakeRSSimpleOutputAll(
            ResultSetProcessorSimple simple,
            ExprEvaluatorContext exprEvaluatorContext,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorStraightOutputFirstHelper MakeRSStraightOutputFirst(
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSetting);

        ResultSetProcessorRowPerEventOutputLastHelper MakeRSRowPerEventOutputLast(
            ResultSetProcessorRowPerEvent processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSetting);

        ResultSetProcessorRowPerEventOutputAllHelper MakeRSRowPerEventOutputAll(
            ResultSetProcessorRowPerEvent processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorRowForAllOutputLastHelper MakeRSRowForAllOutputLast(
            ResultSetProcessorRowForAll processor,
            ExprEvaluatorContext exprEvaluatorContext,
            StateMgmtSetting stateMgmtSetting);

        ResultSetProcessorGroupedOutputAllGroupReps MakeRSGroupedOutputAllNoOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorRowPerGroupOutputAllHelper MakeRSRowPerGroupOutputAllOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroup processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorRowPerGroupOutputLastHelper MakeRSRowPerGroupOutputLastOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroup processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorAggregateGroupedOutputAllHelper MakeRSAggregateGroupedOutputAll(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorAggregateGrouped processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorAggregateGroupedOutputLastHelper MakeRSAggregateGroupedOutputLastOpt(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorAggregateGrouped processor,
            Type[] groupKeyTypes,
            DataInputOutputSerde serde,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorRowPerGroupRollupOutputLastHelper MakeRSRowPerGroupRollupLast(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroupRollup processor,
            Type[] groupKeyTypes,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorRowPerGroupRollupOutputAllHelper MakeRSRowPerGroupRollupAll(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroupRollup processor,
            Type[] groupKeyTypes,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);

        ResultSetProcessorRowPerGroupRollupUnboundHelper MakeRSRowPerGroupRollupSnapshotUnbound(
            ExprEvaluatorContext exprEvaluatorContext,
            ResultSetProcessorRowPerGroupRollup processor,
            Type[] groupKeyTypes,
            int numStreams,
            EventType[] eventTypes,
            StateMgmtSetting stateMgmtSettings);
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.agg.access;
using com.espertech.esper.epl.agg.aggregator;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.baseagg;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.dot;
using com.espertech.esper.epl.expression.funcs;
using com.espertech.esper.epl.expression.ops;
using com.espertech.esper.epl.expression.prev;
using com.espertech.esper.epl.expression.prior;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.time;
using com.espertech.esper.epl.join.exec.@base;
using com.espertech.esper.epl.join.table;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.rettype;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.type;
using com.espertech.esper.view;

namespace com.espertech.esper.metrics.instrumentation
{
    public interface Instrumentation
    {
        void QStimulantEvent(EventBean eventBean, string engineURI);
    
        void AStimulantEvent();
    
        void QStimulantTime(long currentTime, string engineURI);
    
        void AStimulantTime();
    
        void QEvent(EventBean eventBean, string engineURI, bool providedBySendEvent);
    
        void AEvent();
    
        void QEventCP(EventBean theEvent, EPStatementAgentInstanceHandle handle, long engineTime);
    
        void AEventCP();
    
        void QTime(long engineTime, string engineURI);
    
        void ATime();
    
        void QTimeCP(EPStatementAgentInstanceHandle handle, long engineTime);
    
        void ATimeCP();
    
        void QNamedWindowDispatch(string engineURI);
    
        void ANamedWindowDispatch();
    
        void QNamedWindowCPSingle(string engineURI, IList<NamedWindowConsumerView> value, EventBean[] newData, EventBean[] oldData, EPStatementAgentInstanceHandle handle, long time);
    
        void ANamedWindowCPSingle();
    
        void QNamedWindowCPMulti(string engineURI, IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer, EPStatementAgentInstanceHandle handle, long time);
    
        void ANamedWindowCPMulti();
    
        void QRegEx(EventBean newEvent, RegexPartitionState partitionState);

        void ARegEx(RegexPartitionState partitionState, IList<RegexNFAStateEntry> endStates, IList<RegexNFAStateEntry> terminationStates);
    
        void QRegExState(RegexNFAStateEntry currentState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable);
    
        void ARegExState(IList<RegexNFAStateEntry> next, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable);
    
        void QRegExStateStart(RegexNFAState startState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable);

        void ARegExStateStart(IList<RegexNFAStateEntry> nextStates, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable);
    
        void QRegExPartition(ExprNode[] partitionExpressionNodes);
    
        void ARegExPartition(bool exists, RegexPartitionState state);
    
        void QRegIntervalValue(ExprNode exprNode);
    
        void ARegIntervalValue(long result);
    
        void QRegIntervalState(RegexNFAStateEntry endState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable, long engineTime);
    
        void ARegIntervalState(bool scheduled);
    
        void QRegOut(EventBean[] outBeans);
    
        void ARegOut();
    
        void QRegMeasure(RegexNFAStateEntry endState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable);
    
        void ARegMeasure(EventBean outBean);
    
        void QRegExScheduledEval();
    
        void ARegExScheduledEval();
    
        void QExprBool(ExprNode exprNode, EventBean[] eventsPerStream);
    
        void AExprBool(bool? result);
    
        void QExprValue(ExprNode exprNode, EventBean[] eventsPerStream);
    
        void AExprValue(Object result);
    
        void QExprEquals(ExprNode exprNode);
    
        void AExprEquals(bool? result);
    
        void QExprAnd(ExprNode exprNode);
    
        void AExprAnd(bool? result);
    
        void QExprLike(ExprNode exprNode);
    
        void AExprLike(bool? result);
    
        void QExprBitwise(ExprNode exprNode, BitWiseOpEnum bitWiseOpEnum);
    
        void AExprBitwise(Object result);
    
        void QExprMath(ExprMathNode exprMathNode, string op);
    
        void AExprMath(Object result);
    
        void QExprRegexp(ExprRegexpNode exprRegexpNode);
    
        void AExprRegexp(bool? result);
    
        void QExprIdent(string fullUnresolvedName);
    
        void AExprIdent(Object result);
    
        void QExprtypeof();
    
        void AExprtypeof(string typeName);
    
        void QExprOr(ExprOrNode exprOrNode);
    
        void AExprOr(bool? result);
    
        void QExprIn(ExprInNodeImpl exprInNode);
    
        void AExprIn(bool? result);
    
        void QExprCoalesce(ExprCoalesceNode exprCoalesceNode);
    
        void AExprCoalesce(Object value);
    
        void QExprConcat(ExprConcatNode exprConcatNode);
    
        void AExprConcat(string result);
    
        void QaExprConst(Object result);
    
        void QaExprTimestamp(ExprTimestampNode exprTimestampNode, long value);
    
        void QExprBetween(ExprBetweenNodeImpl exprBetweenNode);
    
        void AExprBetween(bool? result);
    
        void QExprCast(ExprCastNode exprCastNode);
    
        void AExprCast(Object result);
    
        void QExprCase(ExprCaseNode exprCaseNode);
    
        void AExprCase(Object result);
    
        void QExprArray(ExprArrayNode exprArrayNode);
    
        void AExprArray(Object result);
    
        void QExprEqualsAnyOrAll(ExprEqualsAllAnyNode exprEqualsAllAnyNode);
    
        void AExprEqualsAnyOrAll(bool? result);
    
        void QExprMinMaxRow(ExprMinMaxRowNode exprMinMaxRowNode);
    
        void AExprMinMaxRow(Object result);
    
        void QExprNew(ExprNewStructNode exprNewNode);
    
        void AExprNew(IDictionary<string, Object> props);
    
        void QExprNot(ExprNotNode exprNotNode);
    
        void AExprNot(bool? result);
    
        void QExprPropExists(ExprPropertyExistsNode exprPropertyExistsNode);
    
        void AExprPropExists(bool exists);
    
        void QExprRelOpAnyOrAll(ExprRelationalOpAllAnyNode exprRelationalOpAllAnyNode, string op);
    
        void AExprRelOpAnyOrAll(bool? result);
    
        void QExprRelOp(ExprRelationalOpNodeImpl exprRelationalOpNode, string op);
    
        void AExprRelOp(bool? result);
    
        void QExprStreamUnd(ExprStreamUnderlyingNodeImpl exprStreamUnderlyingNode);
    
        void AExprStreamUnd(Object result);
    
        void QExprStreamUndSelectClause(ExprStreamUnderlyingNode undNode);
    
        void AExprStreamUndSelectClause(EventBean @event);
    
        void QExprIs(ExprEqualsNodeImpl exprNode);
    
        void AExprIs(bool result);
    
        void QExprVariable(ExprVariableNode exprVariableNode);
    
        void AExprVariable(Object value);
    
        void QExprTimePeriod(ExprTimePeriodImpl exprTimePeriod);
    
        void AExprTimePeriod(Object result);
    
        void QExprInstanceof(ExprInstanceofNode exprInstanceofNode);
    
        void AExprInstanceof(bool? result);
    
        void QExprContextProp(ExprContextPropertyNode exprContextPropertyNode);
    
        void AExprContextProp(Object result);
    
        void QExprPlugInSingleRow(MethodInfo method);
    
        void AExprPlugInSingleRow(Object result);
    
        void QaExprAggValue(ExprAggregateNodeBase exprAggregateNodeBase, Object value);
    
        void QExprSubselect(ExprSubselectNode exprSubselectNode);
    
        void AExprSubselect(Object result);
    
        void QExprDot(ExprDotNode exprDotNode);
    
        void AExprDot(Object result);
    
        void QExprDotChain(EPType targetTypeInfo, Object target, ExprDotEval[] evalUnpacking);
    
        void AExprDotChain();
    
        void QExprDotChainElement(int num, ExprDotEval methodEval);
    
        void AExprDotChainElement(EPType typeInfo, Object result);
    
        void QaExprIStream(ExprIStreamNode exprIStreamNode, bool newData);
    
        void QExprDeclared(ExpressionDeclItem parent);
    
        void AExprDeclared(Object value);
    
        void QExprPrev(ExprPreviousNode exprPreviousNode, bool newData);
    
        void AExprPrev(Object result);
    
        void QExprPrior(ExprPriorNode exprPriorNode);
    
        void AExprPrior(Object result);
    
        void QExprStreamUndMethod(ExprDotNode exprDotEvalStreamMethod);
    
        void AExprStreamUndMethod(Object result);
    
        void QExprStreamEventMethod(ExprDotNode exprDotNode);
    
        void AExprStreamEventMethod(Object result);
    
        void QExprTableSubproperty(ExprNode exprNode, string tableName, string subpropName);
    
        void AExprTableSubproperty(Object result);
    
        void QExprTableTop(ExprNode exprNode, string tableName);
    
        void AExprTableTop(Object result);
    
        void QExprTableSubpropAccessor(ExprNode exprNode, string tableName, string subpropName, ExprAggregateNode aggregationExpression);
    
        void AExprTableSubpropAccessor(Object result);
    
        void QScheduleAdd(long currentTime, long afterMSec, ScheduleHandle handle, long slot);
    
        void AScheduleAdd();
    
        void QScheduleRemove(ScheduleHandle handle, long slot);
    
        void AScheduleRemove();
    
        void QScheduleEval(long currentTime);
    
        void AScheduleEval(ICollection<ScheduleHandle> handles);
    
        void QPatternAndEvaluateTrue(EvalAndNode evalAndNode, MatchedEventMap passUp);
    
        void APatternAndEvaluateTrue(bool quitted);
    
        void QPatternAndQuit(EvalAndNode evalAndNode);
    
        void APatternAndQuit();
    
        void QPatternAndEvaluateFalse(EvalAndNode evalAndNode);
    
        void APatternAndEvaluateFalse();
    
        void QPatternAndStart(EvalAndNode evalAndNode, MatchedEventMap beginState);
    
        void APatternAndStart();
    
        void QPatternFollowedByEvaluateTrue(EvalFollowedByNode evalFollowedByNode, MatchedEventMap matchEvent, int? index);
    
        void APatternFollowedByEvaluateTrue(bool quitted);
    
        void QPatternFollowedByQuit(EvalFollowedByNode evalFollowedByNode);
    
        void APatternFollowedByQuit();
    
        void QPatternFollowedByEvalFalse(EvalFollowedByNode evalFollowedByNode);
    
        void APatternFollowedByEvalFalse();
    
        void QPatternFollowedByStart(EvalFollowedByNode evalFollowedByNode, MatchedEventMap beginState);
    
        void APatternFollowedByStart();
    
        void QPatternOrEvaluateTrue(EvalOrNode evalOrNode, MatchedEventMap matchEvent);
    
        void APatternOrEvaluateTrue(bool quitted);
    
        void QPatternOrEvalFalse(EvalOrNode evalOrNode);
    
        void APatternOrEvalFalse();
    
        void QPatternOrQuit(EvalOrNode evalOrNode);
    
        void APatternOrQuit();
    
        void APatternOrStart();
    
        void QPatternOrStart(EvalOrNode evalOrNode, MatchedEventMap beginState);
    
        void QPatternFilterMatch(EvalFilterNode filterNode, EventBean theEvent);
    
        void APatternFilterMatch(bool quitted);
    
        void QPatternFilterStart(EvalFilterNode evalFilterNode, MatchedEventMap beginState);
    
        void APatternFilterStart();
    
        void QPatternFilterQuit(EvalFilterNode evalFilterNode, MatchedEventMap beginState);
    
        void APatternFilterQuit();
    
        void QPatternRootEvaluateTrue(MatchedEventMap matchEvent);
    
        void APatternRootEvaluateTrue(bool quitted);
    
        void QPatternRootStart(MatchedEventMap root);
    
        void APatternRootStart();
    
        void QPatternRootQuit();
    
        void APatternRootQuit();
    
        void QPatternRootEvalFalse();
    
        void APatternRootEvalFalse();
    
        void QPatternEveryEvaluateTrue(EvalEveryNode evalEveryNode, MatchedEventMap matchEvent);
    
        void APatternEveryEvaluateTrue();
    
        void QPatternEveryStart(EvalEveryNode evalEveryNode, MatchedEventMap beginState);
    
        void APatternEveryStart();
    
        void QPatternEveryEvalFalse(EvalEveryNode evalEveryNode);
    
        void APatternEveryEvalFalse();
    
        void QPatternEveryQuit(EvalEveryNode evalEveryNode);
    
        void APatternEveryQuit();
    
        void QPatternEveryDistinctEvaluateTrue(EvalEveryDistinctNode everyDistinctNode, MatchedEventMap matchEvent);
    
        void APatternEveryDistinctEvaluateTrue(ISet<Object> keysFromNodeNoExpire, IDictionary<Object, long> keysFromNodeExpire, Object matchEventKey, bool haveSeenThis);
    
        void QPatternEveryDistinctQuit(EvalEveryDistinctNode everyNode);
    
        void APatternEveryDistinctQuit();
    
        void QPatternEveryDistinctEvalFalse(EvalEveryDistinctNode everyNode);
    
        void APatternEveryDistinctEvalFalse();
    
        void QPatternEveryDistinctStart(EvalEveryDistinctNode everyNode, MatchedEventMap beginState);
    
        void APatternEveryDistinctStart();
    
        void QPatternGuardEvaluateTrue(EvalGuardNode evalGuardNode, MatchedEventMap matchEvent);
    
        void APatternGuardEvaluateTrue(bool quitted);
    
        void QPatternGuardStart(EvalGuardNode evalGuardNode, MatchedEventMap beginState);
    
        void APatternGuardStart();
    
        void QPatternGuardQuit(EvalGuardNode evalGuardNode);
    
        void APatternGuardQuit();
    
        void QPatternGuardGuardQuit(EvalGuardNode evalGuardNode);
    
        void APatternGuardGuardQuit();
    
        void QPatternGuardScheduledEval();
    
        void APatternGuardScheduledEval();
    
        void QPatternMatchUntilEvaluateTrue(EvalMatchUntilNode evalMatchUntilNode, MatchedEventMap matchEvent, bool matchFromUntil);
    
        void APatternMatchUntilEvaluateTrue(bool quitted);
    
        void QPatternMatchUntilStart(EvalMatchUntilNode evalMatchUntilNode, MatchedEventMap beginState);
    
        void APatternMatchUntilStart();
    
        void QPatternMatchUntilEvalFalse(EvalMatchUntilNode evalMatchUntilNode, bool matchFromUntil);
    
        void APatternMatchUntilEvalFalse();
    
        void QPatternMatchUntilQuit(EvalMatchUntilNode evalMatchUntilNode);
    
        void APatternMatchUntilQuit();
    
        void QPatternNotEvaluateTrue(EvalNotNode evalNotNode, MatchedEventMap matchEvent);
    
        void APatternNotEvaluateTrue(bool quitted);
    
        void APatternNotQuit();
    
        void QPatternNotQuit(EvalNotNode evalNotNode);
    
        void QPatternNotStart(EvalNotNode evalNotNode, MatchedEventMap beginState);
    
        void APatternNotStart();
    
        void QPatternNotEvalFalse(EvalNotNode evalNotNode);
    
        void APatternNotEvalFalse();
    
        void QPatternObserverEvaluateTrue(EvalObserverNode evalObserverNode, MatchedEventMap matchEvent);
    
        void APatternObserverEvaluateTrue();
    
        void QPatternObserverStart(EvalObserverNode evalObserverNode, MatchedEventMap beginState);
    
        void APatternObserverStart();
    
        void QPatternObserverQuit(EvalObserverNode evalObserverNode);
    
        void APatternObserverQuit();
    
        void QPatternObserverScheduledEval();
    
        void APatternObserverScheduledEval();
    
        void QContextPartitionAllocate(AgentInstanceContext agentInstanceContext);
    
        void AContextPartitionAllocate();
    
        void QContextPartitionDestroy(AgentInstanceContext agentInstanceContext);
    
        void AContextPartitionDestroy();
    
        void QContextScheduledEval(ContextDescriptor contextDescriptor);
    
        void AContextScheduledEval();
    
        void QOutputProcessNonBuffered(EventBean[] newData, EventBean[] oldData);
    
        void AOutputProcessNonBuffered();
    
        void QOutputProcessNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents);
    
        void AOutputProcessNonBufferedJoin();
    
        void QOutputProcessWCondition(EventBean[] newData, EventBean[] oldData);
    
        void AOutputProcessWCondition(bool buffered);
    
        void QOutputProcessWConditionJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents);
    
        void AOutputProcessWConditionJoin(bool buffered);
    
        void QOutputRateConditionUpdate(int newDataLength, int oldDataLength);
    
        void AOutputRateConditionUpdate();
    
        void QOutputRateConditionOutputNow();
    
        void AOutputRateConditionOutputNow(bool generate);
    
        void QOutputRateConditionScheduledEval();
    
        void AOutputRateConditionScheduledEval();
    
        void QResultSetProcessSimple();
    
        void AResultSetProcessSimple(EventBean[] selectNewEvents, EventBean[] selectOldEvents);
    
        void QResultSetProcessUngroupedFullyAgg();
    
        void AResultSetProcessUngroupedFullyAgg(EventBean[] selectNewEvents, EventBean[] selectOldEvents);
    
        void QResultSetProcessUngroupedNonfullyAgg();
    
        void AResultSetProcessUngroupedNonfullyAgg(EventBean[] selectNewEvents, EventBean[] selectOldEvents);
    
        void QResultSetProcessGroupedRowPerGroup();
    
        void AResultSetProcessGroupedRowPerGroup(EventBean[] selectNewEvents, EventBean[] selectOldEvents);
    
        void QResultSetProcessGroupedRowPerEvent();
    
        void AResultSetProcessGroupedRowPerEvent(EventBean[] selectNewEvents, EventBean[] selectOldEvents);
    
        void QResultSetProcessComputeGroupKeys(bool enter, ExprNode[] groupKeyNodeExpressions, EventBean[] eventsPerStream);
    
        void AResultSetProcessComputeGroupKeys(bool enter, Object groupKeysPerEvent);
    
        void QAggregationUngroupedApplyEnterLeave(bool enter, int numAggregators, int numAccessStates);
    
        void AAggregationUngroupedApplyEnterLeave(bool enter);
    
        void QAggregationGroupedApplyEnterLeave(bool enter, int numAggregators, int numAccessStates, Object groupKey);
    
        void AAggregationGroupedApplyEnterLeave(bool enter);
    
        void QAggregationGroupedRollupEvalParam(bool enter, int length);
    
        void AAggregationGroupedRollupEvalParam(Object result);
    
        void QAggNoAccessEnterLeave(bool enter, int index, AggregationMethod aggregationMethod, ExprNode aggExpr);
    
        void AAggNoAccessEnterLeave(bool enter, int index, AggregationMethod aggregationMethod);
    
        void QAggAccessEnterLeave(bool enter, int index, AggregationState state, ExprNode aggExpr);
    
        void AAggAccessEnterLeave(bool enter, int index, AggregationState state);
    
        void QSelectClause(EventBean[] eventsPerStream, bool newData, bool synthesize, ExprEvaluatorContext exprEvaluatorContext);
    
        void ASelectClause(bool newData, EventBean @event, Object[] subscriberParameters);
    
        void QViewProcessIRStream(View view, string viewName, EventBean[] newData, EventBean[] oldData);
    
        void AViewProcessIRStream();
    
        void QViewScheduledEval(View view, string viewName);
    
        void AViewScheduledEval();
    
        void QViewIndicate(View view, string viewName, EventBean[] newData, EventBean[] oldData);
    
        void AViewIndicate();
    
        void QSubselectAggregation(ExprNode optionalFilterExprNode);
    
        void ASubselectAggregation();
    
        void QFilterActivationSubselect(string eventTypeName, ExprSubselectNode subselectNode);
    
        void AFilterActivationSubselect();
    
        void QFilterActivationStream(string eventTypeName, int streamNumber);
    
        void AFilterActivationStream();
    
        void QFilterActivationNamedWindowInsert(string namedWindowName);
    
        void AFilterActivationNamedWindowInsert();
    
        void QFilterActivationOnTrigger(string eventTypeName);
    
        void AFilterActivationOnTrigger();
    
        void QRouteBetweenStmt(EventBean theEvent, EPStatementHandle epStatementHandle, bool addToFront);
    
        void ARouteBetweenStmt();
    
        void QIndexAddRemove(EventTable eventTable, EventBean[] newData, EventBean[] oldData);
    
        void AIndexAddRemove();
    
        void QIndexAdd(EventTable eventTable, EventBean[] addEvents);
    
        void AIndexAdd();
    
        void QIndexRemove(EventTable eventTable, EventBean[] removeEvents);
    
        void AIndexRemove();
    
        void QIndexSubordLookup(SubordTableLookupStrategy subordTableLookupStrategy, EventTable optionalEventIndex, int[] keyStreamNums);
    
        void AIndexSubordLookup(ICollection<EventBean> events, Object keys);
    
        void QIndexJoinLookup(JoinExecTableLookupStrategy strategy, EventTable index);
    
        void AIndexJoinLookup(ICollection<EventBean> result, object keys);
    
        void QFilter(EventBean theEvent);
    
        void AFilter(ICollection<FilterHandle> matches);
    
        void QFilterHandleSetIndexes(IList<FilterParamIndexBase> indizes);
    
        void AFilterHandleSetIndexes();
    
        void QaFilterHandleSetCallbacks(ICollection<FilterHandle> callbackSet);
    
        void QFilterReverseIndex(FilterParamIndexLookupableBase filterParamIndex, Object propertyValue);
    
        void AFilterReverseIndex(bool? match);
    
        void QFilterBoolean(FilterParamIndexBooleanExpr filterParamIndexBooleanExpr);
    
        void AFilterBoolean();
    
        void QFilterBooleanExpr(int num, KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals);
    
        void AFilterBooleanExpr(bool result);
    
        void QFilterAdd(FilterValueSet filterValueSet, FilterHandle filterCallback);
    
        void AFilterAdd();
    
        void QFilterRemove(FilterHandle filterCallback, EventTypeIndexBuilderValueIndexesPair pair);
    
        void AFilterRemove();
    
        void QWhereClauseFilter(ExprNode exprNode, EventBean[] newData, EventBean[] oldData);
    
        void AWhereClauseFilter(EventBean[] filteredNewData, EventBean[] filteredOldData);
    
        void QWhereClauseFilterEval(int num, EventBean @event, bool newData);
    
        void AWhereClauseFilterEval(bool? pass);
    
        void QWhereClauseIR(EventBean[] filteredNewData, EventBean[] filteredOldData);
    
        void AWhereClauseIR();
    
        void QHavingClauseNonJoin(EventBean theEvent);
    
        void AHavingClauseNonJoin(bool? pass);
    
        void QHavingClauseJoin(EventBean[] eventsPerStream);
    
        void AHavingClauseJoin(bool? pass);
    
        void QOrderBy(EventBean[] evalEventsPerStream, OrderByElement[] orderBy);
    
        void AOrderBy(Object values);
    
        void QJoinDispatch(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream);
    
        void AJoinDispatch();
    
        void QJoinExexStrategy();
    
        void AJoinExecStrategy(UniformPair<ISet<MultiKey<EventBean>>> joinSet);
    
        void QJoinExecFilter();
    
        void AJoinExecFilter(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents);
    
        void QJoinExecProcess(UniformPair<ISet<MultiKey<EventBean>>> joinSet);
    
        void AJoinExecProcess();
    
        void QJoinCompositionStreamToWin();
    
        void AJoinCompositionStreamToWin(ISet<MultiKey<EventBean>> newResults);
    
        void QJoinCompositionWinToWin();
    
        void AJoinCompositionWinToWin(ISet<MultiKey<EventBean>> newResults, ISet<MultiKey<EventBean>> oldResults);
    
        void QJoinCompositionHistorical();
    
        void AJoinCompositionHistorical(ISet<MultiKey<EventBean>> newResults, ISet<MultiKey<EventBean>> oldResults);
    
        void QJoinCompositionStepUpdIndex(int stream, EventBean[] added, EventBean[] removed);
    
        void AJoinCompositionStepUpdIndex();
    
        void QJoinCompositionQueryStrategy(bool insert, int streamNum, EventBean[] events);
    
        void AJoinCompositionQueryStrategy();
    
        void QInfraTriggeredLookup(SubordWMatchExprLookupStrategyType lookupStrategy);
    
        void AInfraTriggeredLookup(EventBean[] result);
    
        void QInfraOnAction(OnTriggerType triggerType, EventBean[] triggerEvents, EventBean[] matchingEvents);
    
        void AInfraOnAction();
    
        void QInfraUpdate(EventBean beforeUpdate, EventBean[] eventsPerStream, int length, bool copy);
    
        void AInfraUpdate(EventBean afterUpdate);
    
        void QInfraUpdateRHSExpr(int index, EventBeanUpdateItem updateItem);
    
        void AInfraUpdateRHSExpr(Object result);
    
        void QInfraMergeWhenThens(bool matched, EventBean triggerEvent, int numWhenThens);
    
        void AInfraMergeWhenThens(bool matched);
    
        void QInfraMergeWhenThenItem(bool matched, int count);
    
        void AInfraMergeWhenThenItem(bool matched, bool actionsApplied);
    
        void QInfraMergeWhenThenActions(int numActions);
    
        void AInfraMergeWhenThenActions();
    
        void QInfraMergeWhenThenActionItem(int count, string actionName);
    
        void AInfraMergeWhenThenActionItem(bool applies);
    
        void QEngineManagementStmtCompileStart(string engineURI, int statementId, string statementName, string epl, long engineTime);
    
        void AEngineManagementStmtCompileStart(bool success, string message);
    
        void QaEngineManagementStmtStarted(string engineURI, int statementId, string statementName, string epl, long engineTime);
    
        void QEngineManagementStmtStop(EPStatementState targetState, string engineURI, int statementId, string statementName, string epl, long engineTime);
    
        void AEngineManagementStmtStop();
    
        void QaStatementResultExecute(UniformPair<EventBean[]> events, int statementId, string statementName, int agentInstanceId, long threadId);
    
        void QSplitStream(bool all, EventBean theEvent, ExprEvaluator[] whereClauses);
    
        void ASplitStream(bool all, bool handled);
    
        void QSplitStreamWhere(int index);
    
        void ASplitStreamWhere(bool? pass);
    
        void QSplitStreamRoute(int index);
    
        void ASplitStreamRoute();
    
        void QUpdateIStream(InternalEventRouterEntry[] entries);
    
        void AUpdateIStream(EventBean finalEvent, bool haveCloned);
    
        void QUpdateIStreamApply(int index, InternalEventRouterEntry entry);
    
        void AUpdateIStreamApply(EventBean updated, bool applied);
    
        void QUpdateIStreamApplyWhere();
    
        void AUpdateIStreamApplyWhere(bool? result);
    
        void QUpdateIStreamApplyAssignments(InternalEventRouterEntry entry);
    
        void AUpdateIStreamApplyAssignments(Object[] values);
    
        void QUpdateIStreamApplyAssignmentItem(int index);
    
        void AUpdateIStreamApplyAssignmentItem(Object value);
    
        void QHistoricalScheduledEval();
    
        void AHistoricalScheduledEval();
    
        void QTableAddEvent(EventBean theEvent);
    
        void ATableAddEvent();
    
        void QTableDeleteEvent(EventBean theEvent);
    
        void ATableDeleteEvent();
    
        void QaTableUpdatedEvent(EventBean theEvent);
    
        void QaTableUpdatedEventWKeyBefore(EventBean theEvent);
    
        void QaTableUpdatedEventWKeyAfter(EventBean theEvent);
    }
    
} // end of namespace

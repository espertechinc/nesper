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
	public class InstrumentationDefault : Instrumentation
    {
	    public void QStimulantEvent(EventBean eventBean, string engineURI) {

	    }

	    public void AStimulantEvent() {

	    }

	    public void QStimulantTime(long currentTime, string engineURI) {

	    }

	    public void AStimulantTime() {

	    }

	    public void QEvent(EventBean eventBean, string engineURI, bool providedBySendEvent) {

	    }

	    public void AEvent() {

	    }

	    public void QEventCP(EventBean theEvent, EPStatementAgentInstanceHandle handle, long engineTime) {

	    }

	    public void AEventCP() {

	    }

	    public void QTime(long engineTime, string engineURI) {

	    }

	    public void ATime() {

	    }

	    public void QTimeCP(EPStatementAgentInstanceHandle handle, long engineTime) {

	    }

	    public void ATimeCP() {

	    }

	    public void QNamedWindowDispatch(string engineURI) {

	    }

	    public void ANamedWindowDispatch() {

	    }

	    public void QNamedWindowCPSingle(string engineURI, IList<NamedWindowConsumerView> value, EventBean[] newData, EventBean[] oldData, EPStatementAgentInstanceHandle handle, long time) {

	    }

	    public void ANamedWindowCPSingle() {

	    }

	    public void QNamedWindowCPMulti(string engineURI, IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer, EPStatementAgentInstanceHandle handle, long time) {

	    }

	    public void ANamedWindowCPMulti() {

	    }

	    public void QRegEx(EventBean newEvent, RegexPartitionState partitionState) {

	    }

	    public void ARegEx(RegexPartitionState partitionState, IList<RegexNFAStateEntry> endStates, IList<RegexNFAStateEntry> terminationStates) {

	    }

	    public void QRegExState(RegexNFAStateEntry currentState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {

	    }

	    public void ARegExState(IList<RegexNFAStateEntry> next, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {

	    }

	    public void QRegExStateStart(RegexNFAState startState, IDictionary<String, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {
	        
	    }

	    public void ARegExStateStart(IList<RegexNFAStateEntry> nextStates, IDictionary<String, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {

        }

	    public void QRegExPartition(ExprNode[] partitionExpressionNodes) {

	    }

	    public void ARegExPartition(bool exists, RegexPartitionState state) {

	    }

	    public void QRegIntervalValue(ExprNode exprNode) {

	    }

	    public void ARegIntervalValue(long result) {

	    }

	    public void QRegIntervalState(RegexNFAStateEntry endState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable, long engineTime) {

	    }

	    public void ARegIntervalState(bool scheduled) {

	    }

	    public void QRegOut(EventBean[] outBeans) {

	    }

	    public void ARegOut() {

	    }

	    public void QRegMeasure(RegexNFAStateEntry endState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {

	    }

	    public void ARegMeasure(EventBean outBean) {

	    }

	    public void QRegExScheduledEval() {

	    }

	    public void ARegExScheduledEval() {

	    }

	    public void QExprBool(ExprNode exprNode, EventBean[] eventsPerStream) {

	    }

	    public void AExprBool(bool? result) {

	    }

	    public void QExprValue(ExprNode exprNode, EventBean[] eventsPerStream) {

	    }

	    public void AExprValue(object result) {

	    }

	    public void QExprEquals(ExprNode exprNode) {

	    }

	    public void AExprEquals(bool? result) {

	    }

	    public void QExprAnd(ExprNode exprNode) {

	    }

	    public void AExprAnd(bool? result) {

	    }

	    public void QExprLike(ExprNode exprNode) {

	    }

	    public void AExprLike(bool? result) {

	    }

	    public void QExprBitwise(ExprNode exprNode, BitWiseOpEnum bitWiseOpEnum) {

	    }

	    public void AExprBitwise(object result) {

	    }

	    public void QExprMath(ExprMathNode exprMathNode, string op) {

	    }

	    public void AExprMath(object result) {

	    }

	    public void QExprRegexp(ExprRegexpNode exprRegexpNode) {

	    }

	    public void AExprRegexp(bool? result) {

	    }

	    public void QExprIdent(string fullUnresolvedName) {

	    }

	    public void AExprIdent(object result) {

	    }

	    public void QExprtypeof() {

	    }

	    public void AExprtypeof(string typeName) {

	    }

	    public void QExprOr(ExprOrNode exprOrNode) {

	    }

	    public void AExprOr(bool? result) {

	    }

	    public void QExprIn(ExprInNodeImpl exprInNode) {

	    }

	    public void AExprIn(bool? result) {

	    }

	    public void QExprCoalesce(ExprCoalesceNode exprCoalesceNode) {

	    }

	    public void AExprCoalesce(object value) {

	    }

	    public void QExprConcat(ExprConcatNode exprConcatNode) {

	    }

	    public void AExprConcat(string result) {

	    }

	    public void QaExprConst(object result) {

	    }

	    public void QaExprTimestamp(ExprTimestampNode exprTimestampNode, long value) {

	    }

	    public void QExprBetween(ExprBetweenNodeImpl exprBetweenNode) {

	    }

	    public void AExprBetween(bool? result) {

	    }

	    public void QExprCast(ExprCastNode exprCastNode) {

	    }

	    public void AExprCast(object result) {

	    }

	    public void QExprCase(ExprCaseNode exprCaseNode) {

	    }

	    public void AExprCase(object result) {

	    }

	    public void QExprArray(ExprArrayNode exprArrayNode) {

	    }

	    public void AExprArray(object result) {

	    }

	    public void QExprEqualsAnyOrAll(ExprEqualsAllAnyNode exprEqualsAllAnyNode) {

	    }

	    public void AExprEqualsAnyOrAll(bool? result) {

	    }

	    public void QExprMinMaxRow(ExprMinMaxRowNode exprMinMaxRowNode) {

	    }

	    public void AExprMinMaxRow(object result) {

	    }

        public void QExprNew(ExprNewStructNode exprNewNode) {

	    }

	    public void AExprNew(IDictionary<string, object> props) {

	    }

	    public void QExprNot(ExprNotNode exprNotNode) {

	    }

	    public void AExprNot(bool? result) {

	    }

	    public void QExprPropExists(ExprPropertyExistsNode exprPropertyExistsNode) {

	    }

	    public void AExprPropExists(bool exists) {

	    }

	    public void QExprRelOpAnyOrAll(ExprRelationalOpAllAnyNode exprRelationalOpAllAnyNode, string op) {

	    }

	    public void AExprRelOpAnyOrAll(bool? result) {

	    }

	    public void QExprRelOp(ExprRelationalOpNodeImpl exprRelationalOpNode, string op) {

	    }

	    public void AExprRelOp(bool? result) {

	    }

	    public void QExprStreamUnd(ExprStreamUnderlyingNodeImpl exprStreamUnderlyingNode) {

	    }

	    public void AExprStreamUnd(object result) {

	    }

	    public void QExprStreamUndSelectClause(ExprStreamUnderlyingNode undNode) {

	    }

	    public void AExprStreamUndSelectClause(EventBean @event) {

	    }

	    public void QExprIs(ExprEqualsNodeImpl exprNode) {

	    }

	    public void AExprIs(bool result) {

	    }

	    public void QExprVariable(ExprVariableNode exprVariableNode) {

	    }

	    public void AExprVariable(object value) {

	    }

	    public void QExprTimePeriod(ExprTimePeriodImpl exprTimePeriod) {

	    }

	    public void AExprTimePeriod(object result) {

	    }

	    public void QExprInstanceof(ExprInstanceofNode exprInstanceofNode) {

	    }

	    public void AExprInstanceof(bool? result) {

	    }

	    public void QExprContextProp(ExprContextPropertyNode exprContextPropertyNode) {

	    }

	    public void AExprContextProp(object result) {

	    }

	    public void QExprPlugInSingleRow(MethodInfo method) {

	    }

	    public void AExprPlugInSingleRow(object result) {

	    }

	    public void QaExprAggValue(ExprAggregateNodeBase exprAggregateNodeBase, object value) {

	    }

	    public void QExprSubselect(ExprSubselectNode exprSubselectNode) {

	    }

	    public void AExprSubselect(object result) {

	    }

	    public void QExprDot(ExprDotNode exprDotNode) {

	    }

	    public void AExprDot(object result) {

	    }

	    public void QExprDotChain(EPType targetTypeInfo, object target, ExprDotEval[] evalUnpacking) {

	    }

	    public void AExprDotChain() {

	    }

	    public void QExprDotChainElement(int num, ExprDotEval methodEval) {

	    }

	    public void AExprDotChainElement(EPType typeInfo, object result) {

	    }

	    public void QaExprIStream(ExprIStreamNode exprIStreamNode, bool newData) {

	    }

	    public void QExprDeclared(ExpressionDeclItem parent) {

	    }

	    public void AExprDeclared(object value) {

	    }

	    public void QExprPrev(ExprPreviousNode exprPreviousNode, bool newData) {

	    }

	    public void AExprPrev(object result) {

	    }

	    public void QExprPrior(ExprPriorNode exprPriorNode) {

	    }

	    public void AExprPrior(object result) {

	    }

	    public void QExprStreamUndMethod(ExprDotNode exprDotEvalStreamMethod) {

	    }

	    public void AExprStreamUndMethod(object result) {

	    }

	    public void QExprStreamEventMethod(ExprDotNode exprDotNode) {

	    }

	    public void AExprStreamEventMethod(object result) {

	    }

	    public void QScheduleAdd(long currentTime, long afterMSec, ScheduleHandle handle, long slot) {

	    }

	    public void AScheduleAdd() {

	    }

	    public void QScheduleRemove(ScheduleHandle handle, long slot) {

	    }

	    public void AScheduleRemove() {

	    }

	    public void QScheduleEval(long currentTime) {

	    }

	    public void AScheduleEval(ICollection<ScheduleHandle> handles) {

	    }

	    public void QPatternAndEvaluateTrue(EvalAndNode evalAndNode, MatchedEventMap passUp) {

	    }

	    public void APatternAndEvaluateTrue(bool quitted) {

	    }

	    public void QPatternAndQuit(EvalAndNode evalAndNode) {

	    }

	    public void APatternAndQuit() {

	    }

	    public void QPatternAndEvaluateFalse(EvalAndNode evalAndNode) {

	    }

	    public void APatternAndEvaluateFalse() {

	    }

	    public void QPatternAndStart(EvalAndNode evalAndNode, MatchedEventMap beginState) {

	    }

	    public void APatternAndStart() {

	    }

	    public void QPatternFollowedByEvaluateTrue(EvalFollowedByNode evalFollowedByNode, MatchedEventMap matchEvent, int? index) {

	    }

	    public void APatternFollowedByEvaluateTrue(bool quitted) {

	    }

	    public void QPatternFollowedByQuit(EvalFollowedByNode evalFollowedByNode) {

	    }

	    public void APatternFollowedByQuit() {

	    }

	    public void QPatternFollowedByEvalFalse(EvalFollowedByNode evalFollowedByNode) {

	    }

	    public void APatternFollowedByEvalFalse() {

	    }

	    public void QPatternFollowedByStart(EvalFollowedByNode evalFollowedByNode, MatchedEventMap beginState) {

	    }

	    public void APatternFollowedByStart() {

	    }

	    public void QPatternOrEvaluateTrue(EvalOrNode evalOrNode, MatchedEventMap matchEvent) {

	    }

	    public void APatternOrEvaluateTrue(bool quitted) {

	    }

	    public void QPatternOrEvalFalse(EvalOrNode evalOrNode) {

	    }

	    public void APatternOrEvalFalse() {

	    }

	    public void QPatternOrQuit(EvalOrNode evalOrNode) {

	    }

	    public void APatternOrQuit() {

	    }

	    public void APatternOrStart() {

	    }

	    public void QPatternOrStart(EvalOrNode evalOrNode, MatchedEventMap beginState) {

	    }

	    public void QPatternFilterMatch(EvalFilterNode filterNode, EventBean theEvent) {

	    }

	    public void APatternFilterMatch(bool quitted) {

	    }

	    public void QPatternFilterStart(EvalFilterNode evalFilterNode, MatchedEventMap beginState) {

	    }

	    public void APatternFilterStart() {

	    }

	    public void QPatternFilterQuit(EvalFilterNode evalFilterNode, MatchedEventMap beginState) {

	    }

	    public void APatternFilterQuit() {

	    }

	    public void QPatternRootEvaluateTrue(MatchedEventMap matchEvent) {

	    }

	    public void APatternRootEvaluateTrue(bool quitted) {

	    }

	    public void QPatternRootStart(MatchedEventMap root) {

	    }

	    public void APatternRootStart() {

	    }

	    public void QPatternRootQuit() {

	    }

	    public void APatternRootQuit() {

	    }

	    public void QPatternRootEvalFalse() {

	    }

	    public void APatternRootEvalFalse() {

	    }

	    public void QPatternEveryEvaluateTrue(EvalEveryNode evalEveryNode, MatchedEventMap matchEvent) {

	    }

	    public void APatternEveryEvaluateTrue() {

	    }

	    public void QPatternEveryStart(EvalEveryNode evalEveryNode, MatchedEventMap beginState) {

	    }

	    public void APatternEveryStart() {

	    }

	    public void QPatternEveryEvalFalse(EvalEveryNode evalEveryNode) {

	    }

	    public void APatternEveryEvalFalse() {

	    }

	    public void QPatternEveryQuit(EvalEveryNode evalEveryNode) {

	    }

	    public void APatternEveryQuit() {

	    }

	    public void QPatternEveryDistinctEvaluateTrue(EvalEveryDistinctNode everyDistinctNode, MatchedEventMap matchEvent) {

	    }

	    public void APatternEveryDistinctEvaluateTrue(ISet<object> keysFromNodeNoExpire, IDictionary<object, long> keysFromNodeExpire, object matchEventKey, bool haveSeenThis) {

	    }

	    public void QPatternEveryDistinctQuit(EvalEveryDistinctNode everyNode) {

	    }

	    public void APatternEveryDistinctQuit() {

	    }

	    public void QPatternEveryDistinctEvalFalse(EvalEveryDistinctNode everyNode) {

	    }

	    public void APatternEveryDistinctEvalFalse() {

	    }

	    public void QPatternEveryDistinctStart(EvalEveryDistinctNode everyNode, MatchedEventMap beginState) {

	    }

	    public void APatternEveryDistinctStart() {

	    }

	    public void QPatternGuardEvaluateTrue(EvalGuardNode evalGuardNode, MatchedEventMap matchEvent) {

	    }

	    public void APatternGuardEvaluateTrue(bool quitted) {

	    }

	    public void QPatternGuardStart(EvalGuardNode evalGuardNode, MatchedEventMap beginState) {

	    }

	    public void APatternGuardStart() {

	    }

	    public void QPatternGuardQuit(EvalGuardNode evalGuardNode) {

	    }

	    public void APatternGuardQuit() {

	    }

	    public void QPatternGuardGuardQuit(EvalGuardNode evalGuardNode) {

	    }

	    public void APatternGuardGuardQuit() {

	    }

	    public void QPatternGuardScheduledEval() {

	    }

	    public void APatternGuardScheduledEval() {

	    }

	    public void QPatternMatchUntilEvaluateTrue(EvalMatchUntilNode evalMatchUntilNode, MatchedEventMap matchEvent, bool matchFromUntil) {

	    }

	    public void APatternMatchUntilEvaluateTrue(bool quitted) {

	    }

	    public void QPatternMatchUntilStart(EvalMatchUntilNode evalMatchUntilNode, MatchedEventMap beginState) {

	    }

	    public void APatternMatchUntilStart() {

	    }

	    public void QPatternMatchUntilEvalFalse(EvalMatchUntilNode evalMatchUntilNode, bool matchFromUntil) {

	    }

	    public void APatternMatchUntilEvalFalse() {

	    }

	    public void QPatternMatchUntilQuit(EvalMatchUntilNode evalMatchUntilNode) {

	    }

	    public void APatternMatchUntilQuit() {

	    }

	    public void QPatternNotEvaluateTrue(EvalNotNode evalNotNode, MatchedEventMap matchEvent) {

	    }

	    public void APatternNotEvaluateTrue(bool quitted) {

	    }

	    public void APatternNotQuit() {

	    }

	    public void QPatternNotQuit(EvalNotNode evalNotNode) {

	    }

	    public void QPatternNotStart(EvalNotNode evalNotNode, MatchedEventMap beginState) {

	    }

	    public void APatternNotStart() {

	    }

	    public void QPatternNotEvalFalse(EvalNotNode evalNotNode) {

	    }

	    public void APatternNotEvalFalse() {

	    }

	    public void QPatternObserverEvaluateTrue(EvalObserverNode evalObserverNode, MatchedEventMap matchEvent) {

	    }

	    public void APatternObserverEvaluateTrue() {

	    }

	    public void QPatternObserverStart(EvalObserverNode evalObserverNode, MatchedEventMap beginState) {

	    }

	    public void APatternObserverStart() {

	    }

	    public void QPatternObserverQuit(EvalObserverNode evalObserverNode) {

	    }

	    public void APatternObserverQuit() {

	    }

	    public void QPatternObserverScheduledEval() {

	    }

	    public void APatternObserverScheduledEval() {

	    }

	    public void QContextPartitionAllocate(AgentInstanceContext agentInstanceContext) {

	    }

	    public void AContextPartitionAllocate() {

	    }

	    public void QContextPartitionDestroy(AgentInstanceContext agentInstanceContext) {

	    }

	    public void AContextPartitionDestroy() {

	    }

	    public void QContextScheduledEval(ContextDescriptor contextDescriptor) {

	    }

	    public void AContextScheduledEval() {

	    }

	    public void QOutputProcessNonBuffered(EventBean[] newData, EventBean[] oldData) {

	    }

	    public void AOutputProcessNonBuffered() {

	    }

	    public void QOutputProcessNonBufferedJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {

	    }

	    public void AOutputProcessNonBufferedJoin() {

	    }

	    public void QOutputProcessWCondition(EventBean[] newData, EventBean[] oldData) {

	    }

	    public void AOutputProcessWCondition(bool buffered) {

	    }

	    public void QOutputProcessWConditionJoin(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {

	    }

	    public void AOutputProcessWConditionJoin(bool buffered) {

	    }

	    public void QOutputRateConditionUpdate(int newDataLength, int oldDataLength) {

	    }

	    public void AOutputRateConditionUpdate() {

	    }

	    public void QOutputRateConditionOutputNow() {

	    }

	    public void AOutputRateConditionOutputNow(bool generate) {

	    }

	    public void QOutputRateConditionScheduledEval() {

	    }

	    public void AOutputRateConditionScheduledEval() {

	    }

	    public void QResultSetProcessSimple() {

	    }

	    public void AResultSetProcessSimple(EventBean[] selectNewEvents, EventBean[] selectOldEvents) {

	    }

	    public void QResultSetProcessUngroupedFullyAgg() {

	    }

	    public void AResultSetProcessUngroupedFullyAgg(EventBean[] selectNewEvents, EventBean[] selectOldEvents) {

	    }

	    public void QResultSetProcessUngroupedNonfullyAgg() {

	    }

	    public void AResultSetProcessUngroupedNonfullyAgg(EventBean[] selectNewEvents, EventBean[] selectOldEvents) {

	    }

	    public void QResultSetProcessGroupedRowPerGroup() {

	    }

	    public void AResultSetProcessGroupedRowPerGroup(EventBean[] selectNewEvents, EventBean[] selectOldEvents) {

	    }

	    public void QResultSetProcessGroupedRowPerEvent() {

	    }

	    public void AResultSetProcessGroupedRowPerEvent(EventBean[] selectNewEvents, EventBean[] selectOldEvents) {

	    }

	    public void QResultSetProcessComputeGroupKeys(bool enter, ExprNode[] groupKeyNodeExpressions, EventBean[] eventsPerStream) {

	    }

	    public void AResultSetProcessComputeGroupKeys(bool enter, object groupKeysPerEvent) {

	    }

	    public void QAggregationUngroupedApplyEnterLeave(bool enter, int numAggregators, int numAccessStates) {

	    }

	    public void AAggregationUngroupedApplyEnterLeave(bool enter) {

	    }

	    public void QAggregationGroupedApplyEnterLeave(bool enter, int numAggregators, int numAccessStates, object groupKey) {

	    }

	    public void AAggregationGroupedApplyEnterLeave(bool enter) {

	    }

	    public void QAggNoAccessEnterLeave(bool enter, int index, AggregationMethod aggregationMethod, ExprNode aggExpr) {

	    }

	    public void QAggAccessEnterLeave(bool enter, int index, AggregationState state, ExprNode aggExpr) {

	    }

	    public void AAggNoAccessEnterLeave(bool enter, int index, AggregationMethod aggregationMethod) {

	    }

	    public void AAggAccessEnterLeave(bool enter, int index, AggregationState state) {

	    }

	    public void QSelectClause(EventBean[] eventsPerStream, bool newData, bool synthesize, ExprEvaluatorContext exprEvaluatorContext) {

	    }

	    public void ASelectClause(bool newData, EventBean @event, object[] subscriberParameters) {

	    }

	    public void QViewProcessIRStream(View view, string viewName, EventBean[] newData, EventBean[] oldData) {

	    }

	    public void AViewProcessIRStream() {

	    }

	    public void QViewScheduledEval(View view, string viewName) {

	    }

	    public void AViewScheduledEval() {

	    }

	    public void QViewIndicate(View view, string viewName, EventBean[] newData, EventBean[] oldData) {

	    }

	    public void AViewIndicate() {

	    }

	    public void QSubselectAggregation(ExprNode optionalFilterExprNode) {

	    }

	    public void ASubselectAggregation() {

	    }

	    public void QFilterActivationSubselect(string eventTypeName, ExprSubselectNode subselectNode) {

	    }

	    public void AFilterActivationSubselect() {

	    }

	    public void QFilterActivationStream(string eventTypeName, int streamNumber) {

	    }

	    public void AFilterActivationStream() {

	    }

	    public void QFilterActivationNamedWindowInsert(string namedWindowName) {

	    }

	    public void AFilterActivationNamedWindowInsert() {

	    }

	    public void QFilterActivationOnTrigger(string eventTypeName) {

	    }

	    public void AFilterActivationOnTrigger() {

	    }

	    public void QRouteBetweenStmt(EventBean theEvent, EPStatementHandle epStatementHandle, bool addToFront) {

	    }

	    public void ARouteBetweenStmt() {

	    }

	    public void QIndexAddRemove(EventTable eventTable, EventBean[] newData, EventBean[] oldData) {

	    }

	    public void AIndexAddRemove() {

	    }

	    public void QIndexAdd(EventTable eventTable, EventBean[] addEvents) {

	    }

	    public void AIndexAdd() {

	    }

	    public void QIndexRemove(EventTable eventTable, EventBean[] removeEvents) {

	    }

	    public void AIndexRemove() {

	    }

	    public void QIndexSubordLookup(SubordTableLookupStrategy subordTableLookupStrategy, EventTable optionalEventIndex, int[] keyStreamNums) {

	    }

	    public void AIndexSubordLookup(ICollection<EventBean> events, object keys) {

	    }

	    public void QIndexJoinLookup(JoinExecTableLookupStrategy strategy, EventTable index) {

	    }

	    public void AIndexJoinLookup(ICollection<EventBean> result, object keys) {

	    }

	    public void QFilter(EventBean theEvent) {

	    }

	    public void AFilter(ICollection<FilterHandle> matches) {

	    }

	    public void QFilterHandleSetIndexes(IList<FilterParamIndexBase> indizes) {

	    }

	    public void AFilterHandleSetIndexes() {

	    }

	    public void QaFilterHandleSetCallbacks(ICollection<FilterHandle> callbackSet) {

	    }

	    public void QFilterReverseIndex(FilterParamIndexLookupableBase filterParamIndex, object propertyValue) {

	    }

	    public void AFilterReverseIndex(bool? match) {

	    }

	    public void QFilterBoolean(FilterParamIndexBooleanExpr filterParamIndexBooleanExpr) {

	    }

	    public void AFilterBoolean() {

	    }

	    public void QFilterBooleanExpr(int num, KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals) {

	    }

	    public void AFilterBooleanExpr(bool result) {

	    }

	    public void QFilterAdd(FilterValueSet filterValueSet, FilterHandle filterCallback) {

	    }

	    public void AFilterAdd() {

	    }

	    public void QFilterRemove(FilterHandle filterCallback, EventTypeIndexBuilderValueIndexesPair pair) {

	    }

	    public void AFilterRemove() {

	    }

	    public void QWhereClauseFilter(ExprNode exprNode, EventBean[] newData, EventBean[] oldData) {

	    }

	    public void AWhereClauseFilter(EventBean[] filteredNewData, EventBean[] filteredOldData) {

	    }

	    public void QWhereClauseFilterEval(int num, EventBean @event, bool newData) {

	    }

	    public void AWhereClauseFilterEval(bool? pass) {

	    }

	    public void QWhereClauseIR(EventBean[] filteredNewData, EventBean[] filteredOldData) {

	    }

	    public void AWhereClauseIR() {

	    }

	    public void QHavingClauseNonJoin(EventBean theEvent) {

	    }

	    public void AHavingClauseNonJoin(bool? pass) {

	    }

	    public void QHavingClauseJoin(EventBean[] eventsPerStream) {

	    }

	    public void AHavingClauseJoin(bool? pass) {

	    }

	    public void QOrderBy(EventBean[] evalEventsPerStream, OrderByElement[] orderBy) {

	    }

	    public void AOrderBy(object values) {

	    }

	    public void QJoinDispatch(EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream) {

	    }

	    public void AJoinDispatch() {

	    }

	    public void QJoinExexStrategy() {

	    }

	    public void AJoinExecStrategy(UniformPair<ISet<MultiKey<EventBean>>> joinSet) {

	    }

	    public void QJoinExecFilter() {

	    }

	    public void AJoinExecFilter(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents) {

	    }

	    public void QJoinExecProcess(UniformPair<ISet<MultiKey<EventBean>>> joinSet) {

	    }

	    public void AJoinExecProcess() {

	    }

	    public void QJoinCompositionStreamToWin() {

	    }

	    public void AJoinCompositionStreamToWin(ISet<MultiKey<EventBean>> newResults) {

	    }

	    public void QJoinCompositionWinToWin() {

	    }

	    public void AJoinCompositionWinToWin(ISet<MultiKey<EventBean>> newResults, ISet<MultiKey<EventBean>> oldResults) {

	    }

	    public void QJoinCompositionHistorical() {

	    }

	    public void AJoinCompositionHistorical(ISet<MultiKey<EventBean>> newResults, ISet<MultiKey<EventBean>> oldResults) {

	    }

	    public void QJoinCompositionStepUpdIndex(int stream, EventBean[] added, EventBean[] removed) {

	    }

	    public void AJoinCompositionStepUpdIndex() {

	    }

	    public void QJoinCompositionQueryStrategy(bool insert, int streamNum, EventBean[] events) {

	    }

	    public void AJoinCompositionQueryStrategy() {

	    }

	    public void QInfraTriggeredLookup(SubordWMatchExprLookupStrategyType lookupStrategy) {

	    }

	    public void AInfraTriggeredLookup(EventBean[] result) {

	    }

	    public void QInfraOnAction(OnTriggerType triggerType, EventBean[] triggerEvents, EventBean[] matchingEvents) {

	    }

	    public void AInfraOnAction() {

	    }

	    public void QInfraUpdate(EventBean beforeUpdate, EventBean[] eventsPerStream, int length, bool copy) {

	    }

	    public void AInfraUpdate(EventBean afterUpdate) {

	    }

	    public void QInfraUpdateRHSExpr(int index, EventBeanUpdateItem updateItem) {

	    }

	    public void AInfraUpdateRHSExpr(object result) {

	    }

	    public void QInfraMergeWhenThens(bool matched, EventBean triggerEvent, int numWhenThens) {

	    }

	    public void AInfraMergeWhenThens(bool matched) {

	    }

	    public void QInfraMergeWhenThenItem(bool matched, int count) {

	    }

	    public void AInfraMergeWhenThenItem(bool matched, bool actionsApplied) {

	    }

	    public void QInfraMergeWhenThenActions(int numActions) {

	    }

	    public void AInfraMergeWhenThenActions() {

	    }

	    public void QInfraMergeWhenThenActionItem(int count, string actionName) {

	    }

	    public void AInfraMergeWhenThenActionItem(bool applies) {

	    }

	    public void QEngineManagementStmtCompileStart(string engineURI, int statementId, string statementName, string epl, long engineTime) {

	    }

	    public void AEngineManagementStmtCompileStart(bool success, string message) {

	    }

	    public void QaEngineManagementStmtStarted(string engineURI, int statementId, string statementName, string epl, long engineTime) {

	    }

	    public void QEngineManagementStmtStop(EPStatementState targetState, string engineURI, int statementId, string statementName, string epl, long engineTime) {

	    }

	    public void AEngineManagementStmtStop() {

	    }

	    public void QaStatementResultExecute(UniformPair<EventBean[]> events, int statementId, string statementName, int agentInstanceId, long threadId) {

	    }

	    public void QSplitStream(bool all, EventBean theEvent, ExprEvaluator[] whereClauses) {

	    }

	    public void ASplitStream(bool all, bool handled) {

	    }

	    public void QSplitStreamWhere(int index) {

	    }

	    public void ASplitStreamWhere(bool? pass) {

	    }

	    public void QSplitStreamRoute(int index) {

	    }

	    public void ASplitStreamRoute() {

	    }

	    public void QUpdateIStream(InternalEventRouterEntry[] entries) {

	    }

	    public void AUpdateIStream(EventBean finalEvent, bool haveCloned) {

	    }

	    public void QUpdateIStreamApply(int index, InternalEventRouterEntry entry) {

	    }

	    public void AUpdateIStreamApply(EventBean updated, bool applied) {

	    }

	    public void QUpdateIStreamApplyWhere() {

	    }

	    public void AUpdateIStreamApplyWhere(bool? result) {

	    }

	    public void QUpdateIStreamApplyAssignments(InternalEventRouterEntry entry) {

	    }

	    public void AUpdateIStreamApplyAssignments(object[] values) {

	    }

	    public void QUpdateIStreamApplyAssignmentItem(int index) {

	    }

	    public void AUpdateIStreamApplyAssignmentItem(object value) {

	    }

	    public void QHistoricalScheduledEval() {

	    }

	    public void AHistoricalScheduledEval() {

	    }

	    public void QAggregationGroupedRollupEvalParam(bool enter, int length) {

	    }

	    public void AAggregationGroupedRollupEvalParam(object result) {

	    }

	    public void QExprTableSubproperty(ExprNode exprNode, string tableName, string subpropName) {

	    }

	    public void AExprTableSubproperty(object result) {

	    }

	    public void QExprTableTop(ExprNode exprNode, string tableName) {

	    }

	    public void AExprTableTop(object result) {

	    }

	    public void QExprTableSubpropAccessor(ExprNode exprNode, string tableName, string subpropName, ExprAggregateNode aggregationExpression) {

	    }

	    public void AExprTableSubpropAccessor(object result) {

	    }

	    public void QTableAddEvent(EventBean theEvent) {

	    }

	    public void ATableAddEvent() {

	    }

	    public void QTableDeleteEvent(EventBean theEvent) {

	    }

	    public void ATableDeleteEvent() {

	    }

	    public void QaTableUpdatedEvent(EventBean theEvent) {

	    }

	    public void QaTableUpdatedEventWKeyBefore(EventBean theEvent) {

	    }

	    public void QaTableUpdatedEventWKeyAfter(EventBean theEvent) {

	    }
	}
} // end of namespace

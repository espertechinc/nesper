///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.collection;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.@join.exec.@base;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.pattern.and;
using com.espertech.esper.common.@internal.epl.pattern.every;
using com.espertech.esper.common.@internal.epl.pattern.everydistinct;
using com.espertech.esper.common.@internal.epl.pattern.filter;
using com.espertech.esper.common.@internal.epl.pattern.followedby;
using com.espertech.esper.common.@internal.epl.pattern.guard;
using com.espertech.esper.common.@internal.epl.pattern.matchuntil;
using com.espertech.esper.common.@internal.epl.pattern.not;
using com.espertech.esper.common.@internal.epl.pattern.observer;
using com.espertech.esper.common.@internal.epl.pattern.or;
using com.espertech.esper.common.@internal.epl.rowrecog.nfa;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.rettype;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.filtersvcimpl;

namespace com.espertech.esper.runtime.@internal.metrics.instrumentation
{
    public class InstrumentationDefault : Instrumentation
    {
        public static readonly InstrumentationDefault INSTANCE = new InstrumentationDefault();

        public bool Activated()
        {
            return false;
        }

        public void QNamedWindowDispatch(string runtimeURI)
        {
        }

        public void ANamedWindowDispatch()
        {
        }

        public void QNamedWindowCPSingle(
            string runtimeURI,
            int numConsumers,
            EventBean[] newData,
            EventBean[] oldData,
            EPStatementAgentInstanceHandle handle,
            long time)
        {
        }

        public void ANamedWindowCPSingle()
        {
        }

        public void QNamedWindowCPMulti(
            string runtimeURI,
            IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer,
            EPStatementAgentInstanceHandle handle,
            long time)
        {
        }

        public void ANamedWindowCPMulti()
        {
        }

        public void QRegEx(
            EventBean newEvent,
            RowRecogPartitionState partitionState)
        {
        }

        public void ARegEx(
            RowRecogPartitionState partitionState,
            IList<RowRecogNFAStateEntry> endStates,
            IList<RowRecogNFAStateEntry> terminationStates)
        {
        }

        public void QRegExState(
            RowRecogNFAStateEntry currentState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable)
        {
        }

        public void ARegExState(
            IList<RowRecogNFAStateEntry> next,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable)
        {
        }

        public void QRegExStateStart(
            RowRecogNFAState startState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable)
        {
        }

        public void ARegExStateStart(
            IList<RowRecogNFAStateEntry> nextStates,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable)
        {
        }

        public void QRegExPartition(EventBean theEvent)
        {
        }

        public void ARegExPartition(
            bool exists,
            object partitionKey,
            RowRecogPartitionState state)
        {
        }

        public void QRegIntervalValue()
        {
        }

        public void ARegIntervalValue(long result)
        {
        }

        public void QRegIntervalState(
            RowRecogNFAStateEntry endState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable,
            long runtimeTime)
        {
        }

        public void ARegIntervalState(bool scheduled)
        {
        }

        public void QRegOut(EventBean[] outBeans)
        {
        }

        public void ARegOut()
        {
        }

        public void QRegMeasure(
            RowRecogNFAStateEntry endState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable)
        {
        }

        public void ARegMeasure(EventBean outBean)
        {
        }

        public void QRegExScheduledEval()
        {
        }

        public void ARegExScheduledEval()
        {
        }

        public void QRegFilter(
            string text,
            EventBean[] eventsPerStream)
        {
        }

        public void ARegFilter(bool? result)
        {
        }

        public void QFilterActivationStream(
            string eventTypeName,
            int streamNumber,
            AgentInstanceContext agentInstanceContext,
            bool subselect,
            int subselectNumber)
        {
        }

        public void AFilterActivationStream(
            AgentInstanceContext agentInstanceContext,
            bool subselect,
            int subselectNumber)
        {
        }

        public void QIndexSubordLookup(
            SubordTableLookupStrategy subordTableLookupStrategy,
            EventTable optionalEventIndex,
            int[] keyStreamNums)
        {
        }

        public void AIndexSubordLookup(
            ICollection<EventBean> events,
            object keys)
        {
        }

        public void QViewProcessIRStream(
            ViewFactory viewFactory,
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public void AViewProcessIRStream()
        {
        }

        public void QViewIndicate(
            ViewFactory viewFactory,
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public void AViewIndicate()
        {
        }

        public void QViewScheduledEval(ViewFactory viewFactory)
        {
        }

        public void AViewScheduledEval()
        {
        }

        public void QPatternFilterMatch(
            EvalFilterFactoryNode filterNode,
            EventBean theEvent)
        {
        }

        public void APatternFilterMatch(bool quitted)
        {
        }

        public void QPatternNotEvaluateTrue(
            EvalNotFactoryNode evalNotNode,
            MatchedEventMapMinimal matchEvent)
        {
        }

        public void APatternNotEvaluateTrue(bool quitted)
        {
        }

        public void QPatternObserverQuit(EvalObserverFactoryNode evalObserverNode)
        {
        }

        public void APatternObserverQuit()
        {
        }

        public void QPatternAndEvaluateFalse(EvalAndFactoryNode evalAndNode)
        {
        }

        public void APatternAndEvaluateFalse()
        {
        }

        public void QPatternRootEvalFalse()
        {
        }

        public void APatternRootEvalFalse()
        {
        }

        public void QPatternObserverScheduledEval()
        {
        }

        public void APatternObserverScheduledEval()
        {
        }

        public void QPatternObserverEvaluateTrue(
            EvalObserverFactoryNode evalObserverNode,
            MatchedEventMap matchEvent)
        {
        }

        public void APatternObserverEvaluateTrue()
        {
        }

        public void QPatternFollowedByEvaluateTrue(
            EvalFollowedByFactoryNode evalFollowedByNode,
            MatchedEventMap matchEvent,
            int? index)
        {
        }

        public void APatternFollowedByEvaluateTrue(bool quitted)
        {
        }

        public void QPatternGuardStart(
            EvalGuardFactoryNode evalGuardNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternGuardStart()
        {
        }

        public void QPatternAndStart(
            EvalAndFactoryNode evalAndNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternAndStart()
        {
        }

        public void QPatternFilterStart(
            EvalFilterFactoryNode evalFilterNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternFilterStart()
        {
        }

        public void QPatternNotStart(
            EvalNotFactoryNode evalNotNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternNotStart()
        {
        }

        public void QPatternAndEvaluateTrue(
            EvalAndFactoryNode evalAndNode,
            MatchedEventMap passUp)
        {
        }

        public void APatternAndEvaluateTrue(bool quitted)
        {
        }

        public void QPatternGuardScheduledEval()
        {
        }

        public void APatternGuardScheduledEval()
        {
        }

        public void QPatternGuardGuardQuit(EvalGuardFactoryNode evalGuardNode)
        {
        }

        public void APatternGuardGuardQuit()
        {
        }

        public void QPatternAndQuit(EvalAndFactoryNode evalAndNode)
        {
        }

        public void APatternAndQuit()
        {
        }

        public void QPatternFilterQuit(
            EvalFilterFactoryNode evalFilterNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternFilterQuit()
        {
        }

        public void QPatternNotQuit(EvalNotFactoryNode evalNotNode)
        {
        }

        public void APatternNotQuit()
        {
        }

        public void QPatternNotEvalFalse(EvalNotFactoryNode evalNotNode)
        {
        }

        public void APatternNotEvalFalse()
        {
        }

        public void QPatternRootEvaluateTrue(MatchedEventMap matchEvent)
        {
        }

        public void APatternRootEvaluateTrue(bool quitted)
        {
        }

        public void QPatternObserverStart(
            EvalObserverFactoryNode evalObserverNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternObserverStart()
        {
        }

        public void QPatternMatchUntilEvaluateTrue(
            EvalMatchUntilFactoryNode evalMatchUntilNode,
            MatchedEventMap matchEvent,
            bool matchFromUntil)
        {
        }

        public void APatternMatchUntilEvaluateTrue(bool quitted)
        {
        }

        public void QPatternMatchUntilStart(
            EvalMatchUntilFactoryNode evalMatchUntilNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternMatchUntilStart()
        {
        }

        public void QPatternMatchUntilQuit(EvalMatchUntilFactoryNode evalMatchUntilNode)
        {
        }

        public void APatternMatchUntilQuit()
        {
        }

        public void QPatternMatchUntilEvalFalse(
            EvalMatchUntilFactoryNode evalMatchUntilNode,
            bool matchFromUntil)
        {
        }

        public void APatternMatchUntilEvalFalse()
        {
        }

        public void QPatternGuardEvaluateTrue(
            EvalGuardFactoryNode evalGuardNode,
            MatchedEventMap matchEvent)
        {
        }

        public void APatternGuardEvaluateTrue(bool quitted)
        {
        }

        public void QPatternGuardQuit(EvalGuardFactoryNode evalGuardNode)
        {
        }

        public void APatternGuardQuit()
        {
        }

        public void QPatternEveryDistinctEvaluateTrue(
            EvalEveryDistinctFactoryNode everyDistinctNode,
            MatchedEventMap matchEvent)
        {
        }

        public void APatternEveryDistinctEvaluateTrue(
            ISet<object> keysFromNodeNoExpire,
            IDictionary<object, long> keysFromNodeExpire,
            object matchEventKey,
            bool haveSeenThis)
        {
        }

        public void QPatternEveryDistinctStart(
            EvalEveryDistinctFactoryNode everyNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternEveryDistinctStart()
        {
        }

        public void QPatternEveryDistinctQuit(EvalEveryDistinctFactoryNode everyNode)
        {
        }

        public void APatternEveryDistinctQuit()
        {
        }

        public void QPatternFollowedByEvalFalse(EvalFollowedByFactoryNode evalFollowedByNode)
        {
        }

        public void APatternFollowedByEvalFalse()
        {
        }

        public void QPatternEveryDistinctEvalFalse(EvalEveryDistinctFactoryNode everyNode)
        {
        }

        public void APatternEveryDistinctEvalFalse()
        {
        }

        public void QPatternEveryEvaluateTrue(
            EvalEveryFactoryNode evalEveryNode,
            MatchedEventMap matchEvent)
        {
        }

        public void APatternEveryEvaluateTrue()
        {
        }

        public void QPatternEveryStart(
            EvalEveryFactoryNode evalEveryNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternEveryStart()
        {
        }

        public void QPatternEveryQuit(EvalEveryFactoryNode evalEveryNode)
        {
        }

        public void APatternEveryQuit()
        {
        }

        public void QPatternEveryEvalFalse(EvalEveryFactoryNode evalEveryNode)
        {
        }

        public void APatternEveryEvalFalse()
        {
        }

        public void QPatternOrEvaluateTrue(
            EvalOrFactoryNode evalOrNode,
            MatchedEventMap matchEvent)
        {
        }

        public void APatternOrEvaluateTrue(bool quitted)
        {
        }

        public void QPatternOrStart(
            EvalOrFactoryNode evalOrNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternOrStart()
        {
        }

        public void QPatternOrQuit(EvalOrFactoryNode evalOrNode)
        {
        }

        public void APatternOrQuit()
        {
        }

        public void QPatternOrEvalFalse(EvalOrFactoryNode evalOrNode)
        {
        }

        public void APatternOrEvalFalse()
        {
        }

        public void QPatternFollowedByStart(
            EvalFollowedByFactoryNode evalFollowedByNode,
            MatchedEventMap beginState)
        {
        }

        public void APatternFollowedByStart()
        {
        }

        public void QPatternFollowedByQuit(EvalFollowedByFactoryNode evalFollowedByNode)
        {
        }

        public void APatternFollowedByQuit()
        {
        }

        public void QPatternGuardEvalFalse(EvalGuardFactoryNode evalGuardNode)
        {
        }

        public void APatternGuardEvalFalse()
        {
        }

        public void QContextScheduledEval(ContextRuntimeDescriptor contextDescriptor)
        {
        }

        public void AContextScheduledEval()
        {
        }

        public void QContextPartitionAllocate(AgentInstanceContext agentInstanceContext)
        {
        }

        public void AContextPartitionAllocate()
        {
        }

        public void QContextPartitionDestroy(AgentInstanceContext agentInstanceContext)
        {
        }

        public void AContextPartitionDestroy()
        {
        }

        public void QPatternRootStart(MatchedEventMap root)
        {
        }

        public void APatternRootStart()
        {
        }

        public void QPatternRootQuit()
        {
        }

        public void APatternRootQuit()
        {
        }

        public void QInfraOnAction(
            OnTriggerType triggerType,
            EventBean[] triggerEvents,
            EventBean[] matchingEvents)
        {
        }

        public void AInfraOnAction()
        {
        }

        public void QTableUpdatedEvent(EventBean theEvent)
        {
        }

        public void ATableUpdatedEvent()
        {
        }

        public void QInfraMergeWhenThens(
            bool matched,
            EventBean triggerEvent,
            int numWhenThens)
        {
        }

        public void AInfraMergeWhenThens(bool matched)
        {
        }

        public void QInfraMergeWhenThenItem(
            bool matched,
            int count)
        {
        }

        public void AInfraMergeWhenThenItem(
            bool matched,
            bool actionsApplied)
        {
        }

        public void QInfraMergeWhenThenActions(int numActions)
        {
        }

        public void AInfraMergeWhenThenActions()
        {
        }

        public void QInfraMergeWhenThenActionItem(
            int count,
            string actionName)
        {
        }

        public void AInfraMergeWhenThenActionItem(bool applies)
        {
        }

        public void QInfraTriggeredLookup(string lookupStrategy)
        {
        }

        public void AInfraTriggeredLookup(EventBean[] result)
        {
        }

        public void QIndexJoinLookup(
            JoinExecTableLookupStrategy strategy,
            EventTable index)
        {
        }

        public void AIndexJoinLookup(
            ICollection<EventBean> result,
            object keys)
        {
        }

        public void QJoinDispatch(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream)
        {
        }

        public void AJoinDispatch()
        {
        }

        public void QJoinExecStrategy()
        {
        }

        public void AJoinExecStrategy(UniformPair<ISet<MultiKey<EventBean>>> joinSet)
        {
        }

        public void QJoinCompositionStreamToWin()
        {
        }

        public void AJoinCompositionStreamToWin(ISet<MultiKey<EventBean>> newResults)
        {
        }

        public void QJoinCompositionStepUpdIndex(
            int stream,
            EventBean[] added,
            EventBean[] removed)
        {
        }

        public void AJoinCompositionStepUpdIndex()
        {
        }

        public void QIndexAddRemove(
            EventTable eventTable,
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public void AIndexAddRemove()
        {
        }

        public void QIndexAdd(
            EventTable eventTable,
            EventBean[] addEvents)
        {
        }

        public void AIndexAdd()
        {
        }

        public void QIndexRemove(
            EventTable eventTable,
            EventBean[] removeEvents)
        {
        }

        public void AIndexRemove()
        {
        }

        public void QJoinCompositionQueryStrategy(
            bool insert,
            int streamNum,
            EventBean[] events)
        {
        }

        public void AJoinCompositionQueryStrategy()
        {
        }

        public void QJoinExecProcess(UniformPair<ISet<MultiKey<EventBean>>> joinSet)
        {
        }

        public void AJoinExecProcess()
        {
        }

        public void QJoinCompositionWinToWin()
        {
        }

        public void AJoinCompositionWinToWin(
            ISet<MultiKey<EventBean>> newResults,
            ISet<MultiKey<EventBean>> oldResults)
        {
        }

        public void QOutputProcessWCondition(
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public void AOutputProcessWCondition(bool buffered)
        {
        }

        public void QOutputRateConditionUpdate(
            int newDataLength,
            int oldDataLength)
        {
        }

        public void AOutputRateConditionUpdate()
        {
        }

        public void QOutputRateConditionOutputNow()
        {
        }

        public void AOutputRateConditionOutputNow(bool generate)
        {
        }

        public void QOutputProcessWConditionJoin(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents)
        {
        }

        public void AOutputProcessWConditionJoin(bool buffered)
        {
        }

        public void QWhereClauseFilter(
            string text,
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public void AWhereClauseFilter(
            EventBean[] filteredNewData,
            EventBean[] filteredOldData)
        {
        }

        public void QWhereClauseFilterEval(
            int num,
            EventBean @event,
            bool newData)
        {
        }

        public void AWhereClauseFilterEval(bool? pass)
        {
        }

        public void QWhereClauseIR(
            EventBean[] filteredNewData,
            EventBean[] filteredOldData)
        {
        }

        public void AWhereClauseIR()
        {
        }

        public void QSplitStream(
            bool all,
            EventBean theEvent,
            int numWhereClauses)
        {
        }

        public void ASplitStream(
            bool all,
            bool handled)
        {
        }

        public void QSplitStreamWhere(int index)
        {
        }

        public void ASplitStreamWhere(bool? pass)
        {
        }

        public void QSplitStreamRoute(int index)
        {
        }

        public void ASplitStreamRoute()
        {
        }

        public void QSubselectAggregation()
        {
        }

        public void ASubselectAggregation()
        {
        }

        public void QTableAddEvent(EventBean theEvent)
        {
        }

        public void ATableAddEvent()
        {
        }

        public void QaTableUpdatedEventWKeyBefore(EventBean theEvent)
        {
        }

        public void QaTableUpdatedEventWKeyAfter(EventBean theEvent)
        {
        }

        public void QTableDeleteEvent(EventBean theEvent)
        {
        }

        public void ATableDeleteEvent()
        {
        }

        public void QAggregationGroupedApplyEnterLeave(
            bool enter,
            int numAggregators,
            int numAccessStates,
            object groupKey)
        {
        }

        public void AAggregationGroupedApplyEnterLeave(bool enter)
        {
        }

        public void QAggNoAccessEnterLeave(
            bool enter,
            int index,
            object currentValue,
            string aggExpression)
        {
        }

        public void AAggNoAccessEnterLeave(
            bool enter,
            int index,
            object newValue)
        {
        }

        public void QAggAccessEnterLeave(
            bool enter,
            int index,
            string aggExpr)
        {
        }

        public void AAggAccessEnterLeave(
            bool enter,
            int index)
        {
        }

        public void QUpdateIStream(InternalEventRouterEntry[] entries)
        {
        }

        public void AUpdateIStream(
            EventBean finalEvent,
            bool haveCloned)
        {
        }

        public void QUpdateIStreamApply(
            int index,
            InternalEventRouterEntry entry)
        {
        }

        public void AUpdateIStreamApply(
            EventBean updated,
            bool applied)
        {
        }

        public void QUpdateIStreamApplyWhere()
        {
        }

        public void AUpdateIStreamApplyWhere(bool? result)
        {
        }

        public void QUpdateIStreamApplyAssignments(InternalEventRouterEntry entry)
        {
        }

        public void AUpdateIStreamApplyAssignments(object[] values)
        {
        }

        public void QUpdateIStreamApplyAssignmentItem(int index)
        {
        }

        public void AUpdateIStreamApplyAssignmentItem(object value)
        {
        }

        public void QOutputRateConditionScheduledEval()
        {
        }

        public void AOutputRateConditionScheduledEval()
        {
        }

        public void QHistoricalScheduledEval()
        {
        }

        public void AHistoricalScheduledEval()
        {
        }

        public void QJoinExecFilter()
        {
        }

        public void AJoinExecFilter(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents)
        {
        }

        public void QJoinCompositionHistorical()
        {
        }

        public void AJoinCompositionHistorical(
            ISet<MultiKey<EventBean>> newResults,
            ISet<MultiKey<EventBean>> oldResults)
        {
        }

        public void QStimulantEvent(
            EventBean eventBean,
            string runtimeURI)
        {
        }

        public void AStimulantEvent()
        {
        }

        public void QStimulantTime(
            long currentTime,
            long target,
            long ultimateTarget,
            bool span,
            long? resolution,
            string runtimeURI)
        {
        }

        public void AStimulantTime()
        {
        }

        public void QEvent(
            EventBean eventBean,
            string runtimeURI,
            bool providedBySendEvent)
        {
        }

        public void AEvent()
        {
        }

        public void QEventCP(
            EventBean theEvent,
            EPStatementAgentInstanceHandle handle,
            long runtimeTime)
        {
        }

        public void AEventCP()
        {
        }

        public void QTime(
            long runtimeTime,
            string runtimeURI)
        {
        }

        public void ATime()
        {
        }

        public void QTimeCP(
            EPStatementAgentInstanceHandle handle,
            long runtimeTime)
        {
        }

        public void ATimeCP()
        {
        }

        public void QExprEquals(string text)
        {
        }

        public void AExprEquals(bool result)
        {
        }

        public void QOutputProcessNonBuffered(
            EventBean[] newData,
            EventBean[] oldData)
        {
        }

        public void AOutputProcessNonBuffered()
        {
        }

        public void QOutputProcessNonBufferedJoin(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents)
        {
        }

        public void AOutputProcessNonBufferedJoin()
        {
        }

        public void QSelectClause(
            EventBean[] eventsPerStream,
            bool newData,
            bool synthesize,
            ExprEvaluatorContext exprEvaluatorContext)
        {
        }

        public void ASelectClause(
            bool newData,
            EventBean @event,
            object[] subscriberParameters)
        {
        }

        public void QExprBitwise(
            string text,
            BitWiseOpEnum bitWiseOpEnum)
        {
        }

        public void AExprBitwise(object result)
        {
        }

        public void QExprIdent(string fullUnresolvedName)
        {
        }

        public void AExprIdent(object result)
        {
        }

        public void QExprMath(
            string text,
            string op)
        {
        }

        public void AExprMath(object result)
        {
        }

        public void QExprRegexp(string text)
        {
        }

        public void AExprRegexp(bool result)
        {
        }

        public void QExprTypeof(string text)
        {
        }

        public void AExprTypeof(string typeName)
        {
        }

        public void QExprOr(string text)
        {
        }

        public void AExprOr(bool result)
        {
        }

        public void QExprIn(string text)
        {
        }

        public void AExprIn(bool result)
        {
        }

        public void QExprConcat(string text)
        {
        }

        public void AExprConcat(string result)
        {
        }

        public void QExprCoalesce(string text)
        {
        }

        public void AExprCoalesce(object value)
        {
        }

        public void QExprBetween(string text)
        {
        }

        public void AExprBetween(bool result)
        {
        }

        public void QExprCast(string text)
        {
        }

        public void AExprCast(object result)
        {
        }

        public void QExprCase(string text)
        {
        }

        public void AExprCase(object result)
        {
        }

        public void QExprArray(string text)
        {
        }

        public void AExprArray(object result)
        {
        }

        public void QExprEqualsAnyOrAll(string text)
        {
        }

        public void AExprEqualsAnyOrAll(bool result)
        {
        }

        public void QExprMinMaxRow(string text)
        {
        }

        public void AExprMinMaxRow(object result)
        {
        }

        public void QExprNew(string text)
        {
        }

        public void AExprNew(IDictionary<string, object> props)
        {
        }

        public void QExprNot(string text)
        {
        }

        public void AExprNot(bool result)
        {
        }

        public void QExprIStream(string text)
        {
        }

        public void AExprIStream(bool newData)
        {
        }

        public void QExprConst()
        {
        }

        public void AExprConst(object value)
        {
        }

        public void QExprPropExists(string text)
        {
        }

        public void AExprPropExists(bool exists)
        {
        }

        public void QExprRelOpAnyOrAll(
            string text,
            string op)
        {
        }

        public void AExprRelOpAnyOrAll(bool result)
        {
        }

        public void QExprRelOp(
            string text,
            string op)
        {
        }

        public void AExprRelOp(bool result)
        {
        }

        public void QExprStreamUndSelectClause(string text)
        {
        }

        public void AExprStreamUndSelectClause(EventBean @event)
        {
        }

        public void QExprIs(string text)
        {
        }

        public void AExprIs(bool result)
        {
        }

        public void QExprVariable(string text)
        {
        }

        public void AExprVariable(object value)
        {
        }

        public void QExprInstanceof(string text)
        {
        }

        public void AExprInstanceof(bool result)
        {
        }

        public void QExprTimestamp(string text)
        {
        }

        public void AExprTimestamp(long value)
        {
        }

        public void QExprContextProp(string text)
        {
        }

        public void AExprContextProp(object result)
        {
        }

        public void QExprPlugInSingleRow(
            string text,
            string declaringClass,
            string methodName,
            string returnTypeName,
            string[] parameterTypes)
        {
        }

        public void AExprPlugInSingleRow(object result)
        {
        }

        public void QExprDotChain(
            EPType targetTypeInfo,
            object target,
            int numUnpacking)
        {
        }

        public void AExprDotChain()
        {
        }

        public void QExprDot(string text)
        {
        }

        public void AExprDot(object result)
        {
        }

        public void QExprStreamUndMethod(string text)
        {
        }

        public void AExprStreamUndMethod(object result)
        {
        }

        public void QExprDotChainElement(
            int num,
            string methodType,
            string methodName)
        {
        }

        public void AExprDotChainElement(
            EPType typeInfo,
            object result)
        {
        }

        public void QExprPrev(
            string text,
            bool newData)
        {
        }

        public void AExprPrev(object result)
        {
        }

        public void QExprPrior(string text)
        {
        }

        public void AExprPrior(object result)
        {
        }

        public void QScheduleAdd(
            long currentTime,
            long afterMSec,
            ScheduleHandle handle,
            long slot)
        {
        }

        public void AScheduleAdd()
        {
        }

        public void QScheduleRemove(
            ScheduleHandle handle,
            long slot)
        {
        }

        public void AScheduleRemove()
        {
        }

        public void QFilterRemove(
            FilterHandle filterCallback,
            EventType eventType,
            FilterValueSetParam[][] parameters)
        {
        }

        public void AFilterRemove()
        {
        }

        public void QFilterAdd(
            EventType eventType,
            FilterValueSetParam[][] parameters,
            FilterHandle filterCallback)
        {
        }

        public void AFilterAdd()
        {
        }

        public void QExprAnd(string text)
        {
        }

        public void AExprAnd(bool result)
        {
        }

        public void QExprLike(string text)
        {
        }

        public void AExprLike(bool result)
        {
        }

        public void QResultSetProcessUngroupedFullyAgg()
        {
        }

        public void AResultSetProcessUngroupedFullyAgg(UniformPair<EventBean[]> pair)
        {
        }

        public void QAggregationUngroupedApplyEnterLeave(
            bool enter,
            int numAggregators,
            int numAccessStates)
        {
        }

        public void AAggregationUngroupedApplyEnterLeave(bool enter)
        {
        }

        public void QExprAggValue(string text)
        {
        }

        public void AExprAggValue(object value)
        {
        }

        public void QResultSetProcessGroupedRowPerGroup()
        {
        }

        public void AResultSetProcessGroupedRowPerGroup(UniformPair<EventBean[]> pair)
        {
        }

        public void QResultSetProcessComputeGroupKeys(
            bool enter,
            string[] groupKeyNodeExpressions,
            EventBean[] eventsPerStream)
        {
        }

        public void AResultSetProcessComputeGroupKeys(
            bool enter,
            object groupKeysPerEvent)
        {
        }

        public void QResultSetProcessUngroupedNonfullyAgg()
        {
        }

        public void AResultSetProcessUngroupedNonfullyAgg(UniformPair<EventBean[]> pair)
        {
        }

        public void QResultSetProcessGroupedRowPerEvent()
        {
        }

        public void AResultSetProcessGroupedRowPerEvent(UniformPair<EventBean[]> pair)
        {
        }

        public void QResultSetProcessSimple()
        {
        }

        public void AResultSetProcessSimple(UniformPair<EventBean[]> pair)
        {
        }

        public void QFilter(EventBean theEvent)
        {
        }

        public void AFilter(ICollection<FilterHandle> matches)
        {
        }

        public void QFilterHandleSetIndexes(IList<FilterParamIndexBase> indizes)
        {
        }

        public void AFilterHandleSetIndexes()
        {
        }

        public void QFilterReverseIndex(
            FilterParamIndexBase filterParamIndex,
            object propertyValue)
        {
        }

        public void AFilterReverseIndex(bool? match)
        {
        }

        public void QFilterBoolean(FilterParamIndexBooleanExpr filterParamIndexBooleanExpr)
        {
        }

        public void AFilterBoolean()
        {
        }

        public void QFilterBooleanExpr(
            int num,
            KeyValuePair<ExprNodeAdapterBase, EventEvaluator> evals)
        {
        }

        public void AFilterBooleanExpr(bool result)
        {
        }

        public void QExprDeclared(
            string text,
            string name,
            string expressionText,
            string[] parameterNames)
        {
        }

        public void AExprDeclared(object value)
        {
        }

        public void QInfraUpdate(
            EventBean beforeUpdate,
            EventBean[] eventsPerStream,
            int length,
            bool copy)
        {
        }

        public void AInfraUpdate(EventBean afterUpdate)
        {
        }

        public void QInfraUpdateRHSExpr(int index)
        {
        }

        public void AInfraUpdateRHSExpr(object result)
        {
        }

        public void QRouteBetweenStmt(
            EventBean theEvent,
            EPStatementHandle epStatementHandle,
            bool addToFront)
        {
        }

        public void ARouteBetweenStmt()
        {
        }

        public void QScheduleEval(long currentTime)
        {
        }

        public void AScheduleEval(ICollection<ScheduleHandle> handles)
        {
        }

        public void QStatementResultExecute(
            UniformPair<EventBean[]> events,
            string deploymentId,
            int statementId,
            string statementName,
            long threadId)
        {
        }

        public void AStatementResultExecute()
        {
        }

        public void QOrderBy(
            EventBean[] events,
            string[] expressions,
            bool[] @descending)
        {
        }

        public void AOrderBy(object values)
        {
        }

        public void QHavingClause(EventBean[] eventsPerStream)
        {
        }

        public void AHavingClause(bool pass)
        {
        }

        public void QExprSubselect(string text)
        {
        }

        public void AExprSubselect(object result)
        {
        }

        public void QExprTableSubpropAccessor(
            string text,
            string tableName,
            string subpropName,
            string aggregationExpression)
        {
        }

        public void AExprTableSubpropAccessor(object result)
        {
        }

        public void QExprTableSubproperty(
            string text,
            string tableName,
            string subpropName)
        {
        }

        public void AExprTableSubproperty(object result)
        {
        }

        public void QExprTableTop(
            string text,
            string tableName)
        {
        }

        public void AExprTableTop(object result)
        {
        }

        public void QaEngineManagementStmtStarted(
            string runtimeURI,
            string deploymentId,
            int statementId,
            string statementName,
            string epl,
            long runtimeTime)
        {
        }

        public void QaEngineManagementStmtStop(
            string runtimeURI,
            string deploymentId,
            int statementId,
            string statementName,
            string epl,
            long runtimeTime)
        {
        }

        public void QExprStreamUnd(string text)
        {
        }

        public void AExprStreamUnd(object result)
        {
        }

        public void QaFilterHandleSetCallbacks(ISet<FilterHandle> callbackSet)
        {
        }
    }
} // end of namespace
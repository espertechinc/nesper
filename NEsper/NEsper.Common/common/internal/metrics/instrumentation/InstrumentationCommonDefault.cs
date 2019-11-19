///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.metrics.instrumentation
{
    public class InstrumentationCommonDefault : InstrumentationCommon
    {
        public static readonly InstrumentationCommonDefault INSTANCE = new InstrumentationCommonDefault();

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

        public void QContextPartitionDestroy(AgentInstanceContext agentInstanceContext)
        {
        }

        public void AContextPartitionDestroy()
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

        public void QHistoricalScheduledEval()
        {
        }

        public void AHistoricalScheduledEval()
        {
        }

        public void QPatternObserverScheduledEval(EvalObserverFactoryNode node)
        {
        }
    }
} // end of namespace
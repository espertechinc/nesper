///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.join.exec.@base;
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
    public interface InstrumentationCommon
    {
        bool Activated();

        void QNamedWindowDispatch(string runtimeURI);

        void ANamedWindowDispatch();

        void QNamedWindowCPSingle(
            string runtimeURI,
            int numConsumers,
            EventBean[] newData,
            EventBean[] oldData,
            EPStatementAgentInstanceHandle handle,
            long time);

        void ANamedWindowCPSingle();

        void QNamedWindowCPMulti(
            string runtimeURI,
            IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer,
            EPStatementAgentInstanceHandle handle,
            long time);

        void ANamedWindowCPMulti();

        void QRegEx(
            EventBean newEvent,
            RowRecogPartitionState partitionState);

        void ARegEx(
            RowRecogPartitionState partitionState,
            IList<RowRecogNFAStateEntry> endStates,
            IList<RowRecogNFAStateEntry> terminationStates);

        void QRegExState(
            RowRecogNFAStateEntry currentState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable);

        void ARegExState(
            IList<RowRecogNFAStateEntry> next,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable);

        void QRegExStateStart(
            RowRecogNFAState startState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable);

        void ARegExStateStart(
            IList<RowRecogNFAStateEntry> nextStates,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable);

        void QRegExPartition(EventBean theEvent);

        void ARegExPartition(
            bool exists,
            object partitionKey,
            RowRecogPartitionState state);

        void QRegIntervalValue();

        void ARegIntervalValue(long result);

        void QRegIntervalState(
            RowRecogNFAStateEntry endState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable,
            long runtimeTime);

        void ARegIntervalState(bool scheduled);

        void QRegOut(EventBean[] outBeans);

        void ARegOut();

        void QRegMeasure(
            RowRecogNFAStateEntry endState,
            IDictionary<string, Pair<int, bool>> variableStreams,
            int[] multimatchStreamNumToVariable);

        void ARegMeasure(EventBean outBean);

        void QRegExScheduledEval();

        void ARegExScheduledEval();

        void QRegFilter(
            string text,
            EventBean[] eventsPerStream);

        void ARegFilter(bool? result);

        void QFilterActivationStream(
            string eventTypeName,
            int streamNumber,
            AgentInstanceContext agentInstanceContext,
            bool subselect,
            int subselectNumber);

        void AFilterActivationStream(
            AgentInstanceContext agentInstanceContext,
            bool subselect,
            int subselectNumber);

        void QIndexSubordLookup(
            SubordTableLookupStrategy subordTableLookupStrategy,
            EventTable optionalEventIndex,
            int[] keyStreamNums);

        void AIndexSubordLookup(
            ICollection<EventBean> events,
            object keys);

        void QViewProcessIRStream(
            ViewFactory viewFactory,
            EventBean[] newData,
            EventBean[] oldData);

        void AViewProcessIRStream();

        void QViewIndicate(
            ViewFactory viewFactory,
            EventBean[] newData,
            EventBean[] oldData);

        void AViewIndicate();

        void QViewScheduledEval(ViewFactory viewFactory);

        void AViewScheduledEval();

        void QPatternFilterMatch(
            EvalFilterFactoryNode filterNode,
            EventBean theEvent);

        void APatternFilterMatch(bool quitted);

        void QPatternNotEvaluateTrue(
            EvalNotFactoryNode evalNotNode,
            MatchedEventMapMinimal matchEvent);

        void APatternNotEvaluateTrue(bool quitted);

        void QPatternObserverQuit(EvalObserverFactoryNode evalObserverNode);

        void APatternObserverQuit();

        void QPatternAndEvaluateFalse(EvalAndFactoryNode evalAndNode);

        void APatternAndEvaluateFalse();

        void QPatternRootEvalFalse();

        void APatternRootEvalFalse();

        void QPatternObserverScheduledEval();

        void APatternObserverScheduledEval();

        void QPatternObserverEvaluateTrue(
            EvalObserverFactoryNode evalObserverNode,
            MatchedEventMap matchEvent);

        void APatternObserverEvaluateTrue();

        void QPatternFollowedByEvaluateTrue(
            EvalFollowedByFactoryNode evalFollowedByNode,
            MatchedEventMap matchEvent,
            int? index);

        void APatternFollowedByEvaluateTrue(bool quitted);

        void QPatternGuardStart(
            EvalGuardFactoryNode evalGuardNode,
            MatchedEventMap beginState);

        void APatternGuardStart();

        void QPatternAndStart(
            EvalAndFactoryNode evalAndNode,
            MatchedEventMap beginState);

        void APatternAndStart();

        void QPatternFilterStart(
            EvalFilterFactoryNode evalFilterNode,
            MatchedEventMap beginState);

        void APatternFilterStart();

        void QPatternNotStart(
            EvalNotFactoryNode evalNotNode,
            MatchedEventMap beginState);

        void APatternNotStart();

        void QPatternAndEvaluateTrue(
            EvalAndFactoryNode evalAndNode,
            MatchedEventMap passUp);

        void APatternAndEvaluateTrue(bool quitted);

        void QPatternGuardScheduledEval();

        void APatternGuardScheduledEval();

        void QPatternGuardGuardQuit(EvalGuardFactoryNode evalGuardNode);

        void APatternGuardGuardQuit();

        void QPatternAndQuit(EvalAndFactoryNode evalAndNode);

        void APatternAndQuit();

        void QPatternFilterQuit(
            EvalFilterFactoryNode evalFilterNode,
            MatchedEventMap beginState);

        void APatternFilterQuit();

        void QPatternNotQuit(EvalNotFactoryNode evalNotNode);

        void APatternNotQuit();

        void QPatternNotEvalFalse(EvalNotFactoryNode evalNotNode);

        void APatternNotEvalFalse();

        void QPatternRootEvaluateTrue(MatchedEventMap matchEvent);

        void APatternRootEvaluateTrue(bool quitted);

        void QPatternObserverStart(
            EvalObserverFactoryNode evalObserverNode,
            MatchedEventMap beginState);

        void APatternObserverStart();

        void QPatternMatchUntilEvaluateTrue(
            EvalMatchUntilFactoryNode evalMatchUntilNode,
            MatchedEventMap matchEvent,
            bool matchFromUntil);

        void APatternMatchUntilEvaluateTrue(bool quitted);

        void QPatternMatchUntilStart(
            EvalMatchUntilFactoryNode evalMatchUntilNode,
            MatchedEventMap beginState);

        void APatternMatchUntilStart();

        void QPatternMatchUntilQuit(EvalMatchUntilFactoryNode evalMatchUntilNode);

        void APatternMatchUntilQuit();

        void QPatternMatchUntilEvalFalse(
            EvalMatchUntilFactoryNode evalMatchUntilNode,
            bool matchFromUntil);

        void APatternMatchUntilEvalFalse();

        void QPatternGuardEvaluateTrue(
            EvalGuardFactoryNode evalGuardNode,
            MatchedEventMap matchEvent);

        void APatternGuardEvaluateTrue(bool quitted);

        void QPatternGuardQuit(EvalGuardFactoryNode evalGuardNode);

        void APatternGuardQuit();

        void QPatternEveryDistinctEvaluateTrue(
            EvalEveryDistinctFactoryNode everyDistinctNode,
            MatchedEventMap matchEvent);

        void APatternEveryDistinctEvaluateTrue(
            ISet<object> keysFromNodeNoExpire,
            IDictionary<object, long> keysFromNodeExpire,
            object matchEventKey,
            bool haveSeenThis);

        void QPatternEveryDistinctStart(
            EvalEveryDistinctFactoryNode everyNode,
            MatchedEventMap beginState);

        void APatternEveryDistinctStart();

        void QPatternEveryDistinctQuit(EvalEveryDistinctFactoryNode everyNode);

        void APatternEveryDistinctQuit();

        void QPatternFollowedByEvalFalse(EvalFollowedByFactoryNode evalFollowedByNode);

        void APatternFollowedByEvalFalse();

        void QPatternEveryDistinctEvalFalse(EvalEveryDistinctFactoryNode everyNode);

        void APatternEveryDistinctEvalFalse();

        void QPatternEveryEvaluateTrue(
            EvalEveryFactoryNode evalEveryNode,
            MatchedEventMap matchEvent);

        void APatternEveryEvaluateTrue();

        void QPatternEveryStart(
            EvalEveryFactoryNode evalEveryNode,
            MatchedEventMap beginState);

        void APatternEveryStart();

        void QPatternEveryQuit(EvalEveryFactoryNode evalEveryNode);

        void APatternEveryQuit();

        void QPatternEveryEvalFalse(EvalEveryFactoryNode evalEveryNode);

        void APatternEveryEvalFalse();

        void QPatternOrEvaluateTrue(
            EvalOrFactoryNode evalOrNode,
            MatchedEventMap matchEvent);

        void APatternOrEvaluateTrue(bool quitted);

        void QPatternOrStart(
            EvalOrFactoryNode evalOrNode,
            MatchedEventMap beginState);

        void APatternOrStart();

        void QPatternOrQuit(EvalOrFactoryNode evalOrNode);

        void APatternOrQuit();

        void QPatternOrEvalFalse(EvalOrFactoryNode evalOrNode);

        void APatternOrEvalFalse();

        void QPatternFollowedByStart(
            EvalFollowedByFactoryNode evalFollowedByNode,
            MatchedEventMap beginState);

        void APatternFollowedByStart();

        void QPatternFollowedByQuit(EvalFollowedByFactoryNode evalFollowedByNode);

        void APatternFollowedByQuit();

        void QPatternGuardEvalFalse(EvalGuardFactoryNode evalGuardNode);

        void APatternGuardEvalFalse();

        void QContextScheduledEval(ContextRuntimeDescriptor contextDescriptor);

        void AContextScheduledEval();

        void QContextPartitionAllocate(AgentInstanceContext agentInstanceContext);

        void AContextPartitionAllocate();

        void QContextPartitionDestroy(AgentInstanceContext agentInstanceContext);

        void AContextPartitionDestroy();

        void QPatternRootStart(MatchedEventMap root);

        void APatternRootStart();

        void QPatternRootQuit();

        void APatternRootQuit();

        void QInfraOnAction(
            OnTriggerType triggerType,
            EventBean[] triggerEvents,
            EventBean[] matchingEvents);

        void AInfraOnAction();

        void QTableUpdatedEvent(EventBean theEvent);

        void ATableUpdatedEvent();

        void QInfraMergeWhenThens(
            bool matched,
            EventBean triggerEvent,
            int numWhenThens);

        void AInfraMergeWhenThens(bool matched);

        void QInfraMergeWhenThenItem(
            bool matched,
            int count);

        void AInfraMergeWhenThenItem(
            bool matched,
            bool actionsApplied);

        void QInfraMergeWhenThenActions(int numActions);

        void AInfraMergeWhenThenActions();

        void QInfraMergeWhenThenActionItem(
            int count,
            string actionName);

        void AInfraMergeWhenThenActionItem(bool applies);

        void QInfraTriggeredLookup(string lookupStrategy);

        void AInfraTriggeredLookup(EventBean[] result);

        void QIndexJoinLookup(
            JoinExecTableLookupStrategy strategy,
            EventTable index);

        void AIndexJoinLookup(
            ICollection<EventBean> result,
            object keys);

        void QJoinDispatch(
            EventBean[][] newDataPerStream,
            EventBean[][] oldDataPerStream);

        void AJoinDispatch();

        void QJoinExecStrategy();

        void AJoinExecStrategy(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> joinSet);

        void QJoinCompositionStreamToWin();

        void AJoinCompositionStreamToWin(ISet<MultiKeyArrayOfKeys<EventBean>> newResults);

        void QJoinCompositionStepUpdIndex(
            int stream,
            EventBean[] added,
            EventBean[] removed);

        void AJoinCompositionStepUpdIndex();

        void QIndexAddRemove(
            EventTable eventTable,
            EventBean[] newData,
            EventBean[] oldData);

        void AIndexAddRemove();

        void QIndexAdd(
            EventTable eventTable,
            EventBean[] addEvents);

        void AIndexAdd();

        void QIndexRemove(
            EventTable eventTable,
            EventBean[] removeEvents);

        void AIndexRemove();

        void QJoinCompositionQueryStrategy(
            bool insert,
            int streamNum,
            EventBean[] events);

        void AJoinCompositionQueryStrategy();

        void QJoinExecProcess(UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> joinSet);

        void AJoinExecProcess();

        void QJoinCompositionWinToWin();

        void AJoinCompositionWinToWin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newResults,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldResults);

        void QOutputProcessWCondition(
            EventBean[] newData,
            EventBean[] oldData);

        void AOutputProcessWCondition(bool buffered);

        void QOutputRateConditionUpdate(
            int newDataLength,
            int oldDataLength);

        void AOutputRateConditionUpdate();

        void QOutputRateConditionOutputNow();

        void AOutputRateConditionOutputNow(bool generate);

        void QOutputProcessWConditionJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents);

        void AOutputProcessWConditionJoin(bool buffered);

        void QWhereClauseFilter(
            string text,
            EventBean[] newData,
            EventBean[] oldData);

        void AWhereClauseFilter(
            EventBean[] filteredNewData,
            EventBean[] filteredOldData);

        void QWhereClauseFilterEval(
            int num,
            EventBean @event,
            bool newData);

        void AWhereClauseFilterEval(bool? pass);

        void QWhereClauseIR(
            EventBean[] filteredNewData,
            EventBean[] filteredOldData);

        void AWhereClauseIR();

        void QSplitStream(
            bool all,
            EventBean theEvent,
            int numWhereClauses);

        void ASplitStream(
            bool all,
            bool handled);

        void QSplitStreamWhere(int index);

        void ASplitStreamWhere(bool? pass);

        void QSplitStreamRoute(int index);

        void ASplitStreamRoute();

        void QSubselectAggregation();

        void ASubselectAggregation();

        void QTableAddEvent(EventBean theEvent);

        void ATableAddEvent();

        void QaTableUpdatedEventWKeyBefore(EventBean theEvent);

        void QaTableUpdatedEventWKeyAfter(EventBean theEvent);

        void QTableDeleteEvent(EventBean theEvent);

        void ATableDeleteEvent();

        void QAggregationGroupedApplyEnterLeave(
            bool enter,
            int numAggregators,
            int numAccessStates,
            object groupKey);

        void AAggregationGroupedApplyEnterLeave(bool enter);

        void QAggNoAccessEnterLeave(
            bool enter,
            int index,
            object currentValue,
            string aggExpression);

        void AAggNoAccessEnterLeave(
            bool enter,
            int index,
            object newValue);

        void QAggAccessEnterLeave(
            bool enter,
            int index,
            string aggExpr);

        void AAggAccessEnterLeave(
            bool enter,
            int index);

        void QUpdateIStream(InternalEventRouterEntry[] entries);

        void AUpdateIStream(
            EventBean finalEvent,
            bool haveCloned);

        void QUpdateIStreamApply(
            int index,
            InternalEventRouterEntry entry);

        void AUpdateIStreamApply(
            EventBean updated,
            bool applied);

        void QUpdateIStreamApplyWhere();

        void AUpdateIStreamApplyWhere(bool? result);

        void QUpdateIStreamApplyAssignments(InternalEventRouterEntry entry);

        void AUpdateIStreamApplyAssignments(object[] values);

        void QUpdateIStreamApplyAssignmentItem(int index);

        void AUpdateIStreamApplyAssignmentItem(object value);

        void QOutputRateConditionScheduledEval();

        void AOutputRateConditionScheduledEval();

        void QHistoricalScheduledEval();

        void AHistoricalScheduledEval();

        void QJoinExecFilter();

        void AJoinExecFilter(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents);

        void QJoinCompositionHistorical();

        void AJoinCompositionHistorical(
            ISet<MultiKeyArrayOfKeys<EventBean>> newResults,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldResults);


        void QOutputProcessNonBuffered(
            EventBean[] newData,
            EventBean[] oldData);

        void AOutputProcessNonBuffered();

        void QOutputProcessNonBufferedJoin(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents);

        void AOutputProcessNonBufferedJoin();
    }
} // end of namespace
using System;
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
    public class ProxyInstrumentationCommon : InstrumentationCommon
    {
        public Func<bool> ProcActivated { get; set; }

        public Action ProcANamedWindowDispatch { get; set; }
        public Action ProcANamedWindowCPSingle { get; set; }
        public Action ProcANamedWindowCPMulti { get; set; }
        public Action<RowRecogPartitionState, IList<RowRecogNFAStateEntry>, IList<RowRecogNFAStateEntry>> ProcARegEx { get; set; }
        public Action<IList<RowRecogNFAStateEntry>,IDictionary<string, Pair<int, bool>>,int[]> ProcARegExState { get; set; }
        public Action<IList<RowRecogNFAStateEntry>,IDictionary<string, Pair<int, bool>>,int[]> ProcARegExStateStart { get; set; }
        public Action<bool, object, RowRecogPartitionState> ProcARegExPartition { get; set; }
        public Action<long> ProcARegIntervalValue { get; set; }
        public Action<bool> ProcARegIntervalState { get; set; }
        public Action ProcARegOut { get; set; }
        public Action<EventBean> ProcARegMeasure { get; set; }
        public Action ProcARegExScheduledEval { get; set; }
        public Action<bool?> ProcARegFilter { get; set; }
        public Action<AgentInstanceContext, bool, int> ProcAFilterActivationStream { get; set; }
        public Action<ICollection<EventBean>, object> ProcAIndexSubordLookup { get; set; }
        public Action ProcAViewProcessIRStream { get; set; }
        public Action ProcAViewIndicate { get; set; }
        public Action ProcAViewScheduledEval { get; set; }
        public Action<bool> ProcAPatternFilterMatch { get; set; }
        public Action<bool> ProcAPatternNotEvaluateTrue { get; set; }
        public Action ProcAPatternObserverQuit { get; set; }
        public Action ProcAPatternAndEvaluateFalse { get; set; }
        public Action ProcAPatternRootEvalFalse { get; set; }
        public Action ProcAPatternObserverScheduledEval { get; set; }
        public Action ProcAPatternObserverEvaluateTrue { get; set; }
        public Action<bool> ProcAPatternFollowedByEvaluateTrue { get; set; }
        public Action ProcAPatternGuardStart { get; set; }
        public Action ProcAPatternAndStart { get; set; }
        public Action ProcAPatternFilterStart { get; set; }
        public Action ProcAPatternNotStart { get; set; }
        public Action<bool> ProcAPatternAndEvaluateTrue { get; set; }
        public Action ProcAPatternGuardScheduledEval { get; set; }
        public Action ProcAPatternGuardGuardQuit { get; set; }
        public Action ProcAPatternAndQuit { get; set; }
        public Action ProcAPatternFilterQuit { get; set; }
        public Action ProcAPatternNotQuit { get; set; }
        public Action ProcAPatternNotEvalFalse { get; set; }
        public Action<bool> ProcAPatternRootEvaluateTrue { get; set; }
        public Action ProcAPatternObserverStart { get; set; }
        public Action<bool> ProcAPatternMatchUntilEvaluateTrue { get; set; }
        public Action ProcAPatternMatchUntilStart { get; set; }
        public Action ProcAPatternMatchUntilQuit { get; set; }
        public Action ProcAPatternMatchUntilEvalFalse { get; set; }
        public Action<bool> ProcAPatternGuardEvaluateTrue { get; set; }
        public Action ProcAPatternGuardQuit { get; set; }
        public Action<ISet<object>,IDictionary<object, long>,object,bool> ProcAPatternEveryDistinctEvaluateTrue { get; set; }
        public Action ProcAPatternEveryDistinctStart { get; set; }
        public Action ProcAPatternEveryDistinctQuit { get; set; }
        public Action ProcAPatternFollowedByEvalFalse { get; set; }
        public Action ProcAPatternEveryDistinctEvalFalse { get; set; }
        public Action ProcAPatternEveryEvaluateTrue { get; set; }
        public Action ProcAPatternEveryStart { get; set; }
        public Action ProcAPatternEveryQuit { get; set; }
        public Action ProcAPatternEveryEvalFalse { get; set; }
        public Action<bool> ProcAPatternOrEvaluateTrue { get; set; }
        public Action ProcAPatternOrStart { get; set; }
        public Action ProcAPatternOrQuit { get; set; }
        public Action ProcAPatternOrEvalFalse { get; set; }
        public Action ProcAPatternFollowedByStart { get; set; }
        public Action ProcAPatternFollowedByQuit { get; set; }
        public Action ProcAPatternGuardEvalFalse { get; set; }
        public Action ProcAContextScheduledEval { get; set; }
        public Action ProcAContextPartitionAllocate { get; set; }
        public Action ProcAContextPartitionDestroy { get; set; }
        public Action ProcAPatternRootStart { get; set; }
        public Action ProcAPatternRootQuit { get; set; }
        public Action ProcAInfraOnAction { get; set; }
        public Action ProcATableUpdatedEvent { get; set; }
        public Action<bool> ProcAInfraMergeWhenThens { get; set; }
        public Action<bool, bool> ProcAInfraMergeWhenThenItem { get; set; }
        public Action ProcAInfraMergeWhenThenActions { get; set; }
        public Action<bool> ProcAInfraMergeWhenThenActionItem { get; set; }
        public Action<EventBean[]> ProcAInfraTriggeredLookup { get; set; }
        public Action<ICollection<EventBean>, object> ProcAIndexJoinLookup { get; set; }
        public Action ProcAJoinDispatch { get; set; }
        public Action<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> ProcAJoinExecStrategy { get; set; }
        public Action<ISet<MultiKeyArrayOfKeys<EventBean>>> ProcAJoinCompositionStreamToWin { get; set; }
        public Action ProcAJoinCompositionStepUpdIndex { get; set; }
        public Action ProcAIndexAddRemove { get; set; }
        public Action ProcAIndexAdd { get; set; }
        public Action ProcAIndexRemove { get; set; }
        public Action ProcAJoinCompositionQueryStrategy { get; set; }
        public Action ProcAJoinExecProcess { get; set; }
        public Action<ISet<MultiKeyArrayOfKeys<EventBean>>, ISet<MultiKeyArrayOfKeys<EventBean>>> ProcAJoinCompositionWinToWin { get; set; }
        public Action<bool> ProcAOutputProcessWCondition { get; set; }
        public Action ProcAOutputRateConditionUpdate { get; set; }
        public Action<bool> ProcAOutputRateConditionOutputNow { get; set; }
        public Action<bool> ProcAOutputProcessWConditionJoin { get; set; }
        public Action<EventBean[], EventBean[]> ProcAWhereClauseFilter { get; set; }
        public Action<bool?> ProcAWhereClauseFilterEval { get; set; }
        public Action ProcAWhereClauseIR { get; set; }
        public Action<bool, bool> ProcASplitStream { get; set; }
        public Action<bool?> ProcASplitStreamWhere { get; set; }
        public Action ProcASplitStreamRoute { get; set; }
        public Action ProcASubselectAggregation { get; set; }
        public Action ProcATableAddEvent { get; set; }
        public Action ProcATableDeleteEvent { get; set; }
        public Action<bool> ProcAAggregationGroupedApplyEnterLeave { get; set; }
        public Action<bool, int, object> ProcAAggNoAccessEnterLeave { get; set; }
        public Action<bool, int> ProcAAggAccessEnterLeave { get; set; }
        public Action<EventBean, bool> ProcAUpdateIStream { get; set; }
        public Action<EventBean, bool> ProcAUpdateIStreamApply { get; set; }
        public Action<bool?> ProcAUpdateIStreamApplyWhere { get; set; }
        public Action<object[]> ProcAUpdateIStreamApplyAssignments { get; set; }
        public Action<object> ProcAUpdateIStreamApplyAssignmentItem { get; set; }
        public Action ProcAOutputRateConditionScheduledEval { get; set; }
        public Action ProcAHistoricalScheduledEval { get; set; }
        public Action<ISet<MultiKeyArrayOfKeys<EventBean>>, ISet<MultiKeyArrayOfKeys<EventBean>>> ProcAJoinExecFilter { get; set; }
        public Action<ISet<MultiKeyArrayOfKeys<EventBean>>, ISet<MultiKeyArrayOfKeys<EventBean>>> ProcAJoinCompositionHistorical { get; set; }

        public Action<string> ProcQNamedWindowDispatch { get; set; }
        public Action<string,int,EventBean[],EventBean[],EPStatementAgentInstanceHandle,long> ProcQNamedWindowCPSingle { get; set; }
        public Action<string,IDictionary<NamedWindowConsumerView,NamedWindowDeltaData>,EPStatementAgentInstanceHandle,long> ProcQNamedWindowCPMulti { get; set; }
        public Action<EventBean, RowRecogPartitionState> ProcQRegEx { get; set; }
        public Action<RowRecogNFAStateEntry,IDictionary<string, Pair<int, bool>>,int[]> ProcQRegExState { get; set; }
        public Action<RowRecogNFAState,IDictionary<string, Pair<int, bool>>,int[]> ProcQRegExStateStart { get; set; }
        public Action<EventBean> ProcQRegExPartition { get; set; }
        public Action ProcQRegIntervalValue { get ; set ; }
        public Action<RowRecogNFAStateEntry,IDictionary<string,Pair<int, bool>>,int[],long> ProcQRegIntervalState { get; set; }
        public Action<EventBean[]> ProcQRegOut { get; set; }
        public Action<RowRecogNFAStateEntry,IDictionary<string, Pair<int, bool>>,int[]> ProcQRegMeasure { get; set; }
        public Action ProcQRegExScheduledEval { get ; set ; }
        public Action<string, EventBean[]> ProcQRegFilter { get; set; }
        public Action<string,int,AgentInstanceContext,bool,int> ProcQFilterActivationStream { get; set; }
        public Action<SubordTableLookupStrategy,EventTable,int[]> ProcQIndexSubordLookup { get; set; }
        public Action<ViewFactory, EventBean[], EventBean[]> ProcQViewProcessIRStream { get; set; }
        public Action<ViewFactory, EventBean[], EventBean[]> ProcQViewIndicate { get; set; }
        public Action<ViewFactory> ProcQViewScheduledEval { get; set; }
        public Action<EvalFilterFactoryNode, EventBean> ProcQPatternFilterMatch { get; set; }
        public Action<EvalNotFactoryNode, MatchedEventMapMinimal> ProcQPatternNotEvaluateTrue { get; set; }
        public Action<EvalObserverFactoryNode> ProcQPatternObserverQuit { get; set; }
        public Action<EvalAndFactoryNode> ProcQPatternAndEvaluateFalse { get; set; }
        public Action ProcQPatternRootEvalFalse { get ; set ; }
        public Action ProcQPatternObserverScheduledEval { get ; set ; }
        public Action<EvalObserverFactoryNode, MatchedEventMap> ProcQPatternObserverEvaluateTrue { get; set; }
        public Action<EvalFollowedByFactoryNode,MatchedEventMap,int?> ProcQPatternFollowedByEvaluateTrue { get; set; }
        public Action<EvalGuardFactoryNode, MatchedEventMap> ProcQPatternGuardStart { get; set; }
        public Action<EvalAndFactoryNode, MatchedEventMap> ProcQPatternAndStart { get; set; }
        public Action<EvalFilterFactoryNode, MatchedEventMap> ProcQPatternFilterStart { get; set; }
        public Action<EvalNotFactoryNode, MatchedEventMap> ProcQPatternNotStart { get; set; }
        public Action<EvalAndFactoryNode, MatchedEventMap> ProcQPatternAndEvaluateTrue { get; set; }
        public Action ProcQPatternGuardScheduledEval { get ; set ; }
        public Action<EvalGuardFactoryNode> ProcQPatternGuardGuardQuit { get; set; }
        public Action<EvalAndFactoryNode> ProcQPatternAndQuit { get; set; }
        public Action<EvalFilterFactoryNode, MatchedEventMap> ProcQPatternFilterQuit { get; set; }
        public Action<EvalNotFactoryNode> ProcQPatternNotQuit { get; set; }
        public Action<EvalNotFactoryNode> ProcQPatternNotEvalFalse { get; set; }
        public Action<MatchedEventMap> ProcQPatternRootEvaluateTrue { get; set; }
        public Action<EvalObserverFactoryNode, MatchedEventMap> ProcQPatternObserverStart { get; set; }
        public Action<EvalMatchUntilFactoryNode,MatchedEventMap,bool> ProcQPatternMatchUntilEvaluateTrue { get; set; }
        public Action<EvalMatchUntilFactoryNode, MatchedEventMap> ProcQPatternMatchUntilStart { get; set; }
        public Action<EvalMatchUntilFactoryNode> ProcQPatternMatchUntilQuit { get; set; }
        public Action<EvalMatchUntilFactoryNode, bool> ProcQPatternMatchUntilEvalFalse { get; set; }
        public Action<EvalGuardFactoryNode, MatchedEventMap> ProcQPatternGuardEvaluateTrue { get; set; }
        public Action<EvalGuardFactoryNode> ProcQPatternGuardQuit { get; set; }
        public Action<EvalEveryDistinctFactoryNode, MatchedEventMap> ProcQPatternEveryDistinctEvaluateTrue { get; set; }
        public Action<EvalEveryDistinctFactoryNode, MatchedEventMap> ProcQPatternEveryDistinctStart { get; set; }
        public Action<EvalEveryDistinctFactoryNode> ProcQPatternEveryDistinctQuit { get; set; }
        public Action<EvalFollowedByFactoryNode> ProcQPatternFollowedByEvalFalse { get; set; }
        public Action<EvalEveryDistinctFactoryNode> ProcQPatternEveryDistinctEvalFalse { get; set; }
        public Action<EvalEveryFactoryNode, MatchedEventMap> ProcQPatternEveryEvaluateTrue { get; set; }
        public Action<EvalEveryFactoryNode, MatchedEventMap> ProcQPatternEveryStart { get; set; }
        public Action<EvalEveryFactoryNode> ProcQPatternEveryQuit { get; set; }
        public Action<EvalEveryFactoryNode> ProcQPatternEveryEvalFalse { get; set; }
        public Action<EvalOrFactoryNode, MatchedEventMap> ProcQPatternOrEvaluateTrue { get; set; }
        public Action<EvalOrFactoryNode, MatchedEventMap> ProcQPatternOrStart { get; set; }
        public Action<EvalOrFactoryNode> ProcQPatternOrQuit { get; set; }
        public Action<EvalOrFactoryNode> ProcQPatternOrEvalFalse { get; set; }
        public Action<EvalFollowedByFactoryNode, MatchedEventMap> ProcQPatternFollowedByStart { get; set; }
        public Action<EvalFollowedByFactoryNode> ProcQPatternFollowedByQuit { get; set; }
        public Action<EvalGuardFactoryNode> ProcQPatternGuardEvalFalse { get; set; }
        public Action<ContextRuntimeDescriptor> ProcQContextScheduledEval { get; set; }
        public Action<AgentInstanceContext> ProcQContextPartitionAllocate { get; set; }
        public Action<AgentInstanceContext> ProcQContextPartitionDestroy { get; set; }
        public Action<MatchedEventMap> ProcQPatternRootStart { get; set; }
        public Action ProcQPatternRootQuit { get ; set ; }
        public Action<OnTriggerType,EventBean[],EventBean[]> ProcQInfraOnAction { get; set; }
        public Action<EventBean> ProcQTableUpdatedEvent { get; set; }
        public Action<bool,EventBean,int> ProcQInfraMergeWhenThens { get; set; }
        public Action<bool, int> ProcQInfraMergeWhenThenItem { get; set; }
        public Action<int> ProcQInfraMergeWhenThenActions { get; set; }
        public Action<int, string> ProcQInfraMergeWhenThenActionItem { get; set; }
        public Action<string> ProcQInfraTriggeredLookup { get; set; }
        public Action<JoinExecTableLookupStrategy, EventTable> ProcQIndexJoinLookup { get; set; }
        public Action<EventBean[][], EventBean[][]> ProcQJoinDispatch { get; set; }
        public Action ProcQJoinExecStrategy { get ; set ; }
        public Action ProcQJoinCompositionStreamToWin { get ; set ; }
        public Action<int,EventBean[],EventBean[]> ProcQJoinCompositionStepUpdIndex { get; set; }
        public Action<EventTable,EventBean[],EventBean[]> ProcQIndexAddRemove { get; set; }
        public Action<EventTable, EventBean[]> ProcQIndexAdd { get; set; }
        public Action<EventTable, EventBean[]> ProcQIndexRemove { get; set; }
        public Action<bool,int,EventBean[]> ProcQJoinCompositionQueryStrategy { get; set; }
        public Action<UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>>> ProcQJoinExecProcess { get; set; }
        public Action ProcQJoinCompositionWinToWin { get ; set ; }
        public Action<EventBean[], EventBean[]> ProcQOutputProcessWCondition { get; set; }
        public Action<int, int> ProcQOutputRateConditionUpdate { get; set; }
        public Action ProcQOutputRateConditionOutputNow { get ; set ; }
        public Action<ISet<MultiKeyArrayOfKeys<EventBean>>, ISet<MultiKeyArrayOfKeys<EventBean>>> ProcQOutputProcessWConditionJoin { get; set; }
        public Action<string,EventBean[],EventBean[]> ProcQWhereClauseFilter { get; set; }
        public Action<int,EventBean,bool> ProcQWhereClauseFilterEval { get; set; }
        public Action<EventBean[], EventBean[]> ProcQWhereClauseIR { get; set; }
        public Action<bool,EventBean,int> ProcQSplitStream { get; set; }
        public Action<int> ProcQSplitStreamWhere { get; set; }
        public Action<int> ProcQSplitStreamRoute { get; set; }
        public Action ProcQSubselectAggregation { get ; set ; }
        public Action<EventBean> ProcQTableAddEvent { get; set; }
        public Action<EventBean> ProcQaTableUpdatedEventWKeyBefore { get; set; }
        public Action<EventBean> ProcQaTableUpdatedEventWKeyAfter { get; set; }
        public Action<EventBean> ProcQTableDeleteEvent { get; set; }
        public Action<bool,int,int,object> ProcQAggregationGroupedApplyEnterLeave { get; set; }
        public Action<bool,int,object,string> ProcQAggNoAccessEnterLeave { get; set; }
        public Action<bool,int,string> ProcQAggAccessEnterLeave { get; set; }
        public Action<InternalEventRouterEntry[]> ProcQUpdateIStream { get; set; }
        public Action<int, InternalEventRouterEntry> ProcQUpdateIStreamApply { get; set; }
        public Action ProcQUpdateIStreamApplyWhere { get ; set ; }
        public Action<InternalEventRouterEntry> ProcQUpdateIStreamApplyAssignments { get; set; }
        public Action<int> ProcQUpdateIStreamApplyAssignmentItem { get; set; }
        public Action ProcQOutputRateConditionScheduledEval { get ; set ; }
        public Action ProcQHistoricalScheduledEval { get ; set ; }
        public Action ProcQJoinExecFilter { get ; set ; }
        public Action ProcQJoinCompositionHistorical { get ; set ; }

        public bool Activated()
        {
            return ProcActivated.Invoke();
        }

         public void QNamedWindowDispatch (string runtimeURI) {
                ProcQNamedWindowDispatch (runtimeURI);
        }

        public void ANamedWindowDispatch () {
                ProcANamedWindowDispatch ();
        }

        public void QNamedWindowCPSingle (string runtimeURI, int numConsumers, EventBean[] newData, EventBean[] oldData, EPStatementAgentInstanceHandle handle, long time)
        {
                ProcQNamedWindowCPSingle (runtimeURI, numConsumers, newData, oldData, handle, time);
        }

        public void ANamedWindowCPSingle () {
                ProcANamedWindowCPSingle ();
        }

        public void QNamedWindowCPMulti (string runtimeURI, IDictionary<NamedWindowConsumerView, NamedWindowDeltaData> deltaPerConsumer, EPStatementAgentInstanceHandle handle, long time) {
                ProcQNamedWindowCPMulti (runtimeURI, deltaPerConsumer, handle, time);
        }

        public void ANamedWindowCPMulti () {
                ProcANamedWindowCPMulti ();
        }

        public void QRegEx (EventBean newEvent, RowRecogPartitionState partitionState)

        {
                ProcQRegEx (newEvent, partitionState);
        }

        public void ARegEx (RowRecogPartitionState partitionState, IList<RowRecogNFAStateEntry> endStates, IList<RowRecogNFAStateEntry> terminationStates) {
                ProcARegEx (partitionState, endStates, terminationStates);
        }

        public void QRegExState (RowRecogNFAStateEntry currentState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {
                ProcQRegExState (currentState, variableStreams, multimatchStreamNumToVariable);
        }

        public void ARegExState (IList<RowRecogNFAStateEntry> next, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {
                ProcARegExState (next, variableStreams, multimatchStreamNumToVariable);
        }

        public void QRegExStateStart (RowRecogNFAState startState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {
                ProcQRegExStateStart (startState, variableStreams, multimatchStreamNumToVariable);
        }

        public void ARegExStateStart (IList<RowRecogNFAStateEntry> nextStates, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable)
        {
                ProcARegExStateStart (nextStates, variableStreams, multimatchStreamNumToVariable);
        }

        public void QRegExPartition (EventBean theEvent) {
                ProcQRegExPartition (theEvent);
        }

        public void ARegExPartition (bool exists, object partitionKey, RowRecogPartitionState state) {
                ProcARegExPartition (exists, partitionKey, state);
        }

        public void QRegIntervalValue () {
                ProcQRegIntervalValue ();
        }

        public void ARegIntervalValue (long result) {
                ProcARegIntervalValue (result);
        }

        public void QRegIntervalState (RowRecogNFAStateEntry endState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable, long runtimeTime) {
                ProcQRegIntervalState (endState, variableStreams, multimatchStreamNumToVariable, runtimeTime);
        }

        public void ARegIntervalState (bool scheduled) {
                ProcARegIntervalState (scheduled);
        }

        public void QRegOut (EventBean[] outBeans) {
                ProcQRegOut (outBeans);
        }

        public void ARegOut () {
                ProcARegOut ();
        }

        public void QRegMeasure (RowRecogNFAStateEntry endState, IDictionary<string, Pair<int, bool>> variableStreams, int[] multimatchStreamNumToVariable) {
                ProcQRegMeasure (endState, variableStreams, multimatchStreamNumToVariable);
        }

        public void ARegMeasure (EventBean outBean) {
                ProcARegMeasure (outBean);
        }

        public void QRegExScheduledEval () {
                ProcQRegExScheduledEval ();
        }

        public void ARegExScheduledEval () {
                ProcARegExScheduledEval ();
        }

        public void QRegFilter (string text, EventBean[] eventsPerStream) {
                ProcQRegFilter (text, eventsPerStream);
        }

        public void ARegFilter (bool? result) {
                ProcARegFilter (result);
        }

        public void QFilterActivationStream (string eventTypeName, int streamNumber, AgentInstanceContext agentInstanceContext, bool subselect, int subselectNumber) {
                ProcQFilterActivationStream (eventTypeName, streamNumber, agentInstanceContext, subselect, subselectNumber);
        }

        public void AFilterActivationStream (AgentInstanceContext agentInstanceContext, bool subselect, int subselectNumber) {
                ProcAFilterActivationStream (agentInstanceContext, subselect, subselectNumber);
        }

        public void QIndexSubordLookup (SubordTableLookupStrategy subordTableLookupStrategy, EventTable optionalEventIndex, int[] keyStreamNums) {
                ProcQIndexSubordLookup (subordTableLookupStrategy, optionalEventIndex, keyStreamNums);
        }

        public void AIndexSubordLookup (ICollection<EventBean> events, object keys)

        {
                ProcAIndexSubordLookup (events, keys);
        }

        public void QViewProcessIRStream (ViewFactory viewFactory, EventBean[] newData, EventBean[] oldData) {
                ProcQViewProcessIRStream (viewFactory, newData, oldData);
        }

        public void AViewProcessIRStream () {
                ProcAViewProcessIRStream ();
        }

        public void QViewIndicate (ViewFactory viewFactory, EventBean[] newData, EventBean[] oldData) {
                ProcQViewIndicate (viewFactory, newData, oldData);
        }

        public void AViewIndicate () {
                ProcAViewIndicate ();
        }

        public void QViewScheduledEval (ViewFactory viewFactory) {
                ProcQViewScheduledEval (viewFactory);
        }

        public void AViewScheduledEval () {
                ProcAViewScheduledEval ();
        }

        public void QPatternFilterMatch (EvalFilterFactoryNode filterNode, EventBean theEvent) {
                ProcQPatternFilterMatch (filterNode, theEvent);
        }

        public void APatternFilterMatch (bool quitted) {
                ProcAPatternFilterMatch (quitted);
        }

        public void QPatternNotEvaluateTrue (EvalNotFactoryNode evalNotNode, MatchedEventMapMinimal matchEvent) {
                ProcQPatternNotEvaluateTrue (evalNotNode, matchEvent);
        }

        public void APatternNotEvaluateTrue (bool quitted) {
                ProcAPatternNotEvaluateTrue (quitted);
        }

        public void QPatternObserverQuit (EvalObserverFactoryNode evalObserverNode)

        {
                ProcQPatternObserverQuit (evalObserverNode);
        }

        public void APatternObserverQuit () {
                ProcAPatternObserverQuit ();
        }

        public void QPatternAndEvaluateFalse (EvalAndFactoryNode evalAndNode) {
                ProcQPatternAndEvaluateFalse (evalAndNode);
        }

        public void APatternAndEvaluateFalse () {
                ProcAPatternAndEvaluateFalse ();
        }

        public void QPatternRootEvalFalse () {
                ProcQPatternRootEvalFalse ();
        }

        public void APatternRootEvalFalse () {
                ProcAPatternRootEvalFalse ();
        }

        public void QPatternObserverScheduledEval () {
                ProcQPatternObserverScheduledEval ();
        }

        public void APatternObserverScheduledEval () {
                ProcAPatternObserverScheduledEval ();
        }

        public void QPatternObserverEvaluateTrue (EvalObserverFactoryNode evalObserverNode, MatchedEventMap matchEvent) {
                ProcQPatternObserverEvaluateTrue (evalObserverNode, matchEvent);
        }

        public void APatternObserverEvaluateTrue () {
                ProcAPatternObserverEvaluateTrue ();
        }

        public void QPatternFollowedByEvaluateTrue (EvalFollowedByFactoryNode evalFollowedByNode, MatchedEventMap matchEvent, int? index) {
                ProcQPatternFollowedByEvaluateTrue (evalFollowedByNode, matchEvent, index);
        }

        public void APatternFollowedByEvaluateTrue (bool quitted) {
                ProcAPatternFollowedByEvaluateTrue (quitted);
        }

        public void QPatternGuardStart (EvalGuardFactoryNode evalGuardNode, MatchedEventMap beginState) {
                ProcQPatternGuardStart (evalGuardNode, beginState);
        }

        public void APatternGuardStart () {
                ProcAPatternGuardStart ();
        }

        public void QPatternAndStart (EvalAndFactoryNode evalAndNode, MatchedEventMap beginState) {
                ProcQPatternAndStart (evalAndNode, beginState);
        }

        public void APatternAndStart () {
                ProcAPatternAndStart ();
        }

        public void QPatternFilterStart (EvalFilterFactoryNode evalFilterNode, MatchedEventMap beginState) {
                ProcQPatternFilterStart (evalFilterNode, beginState);
        }

        public void APatternFilterStart () {
                ProcAPatternFilterStart ();
        }

        public void QPatternNotStart (EvalNotFactoryNode evalNotNode, MatchedEventMap beginState) {
                ProcQPatternNotStart (evalNotNode, beginState);
        }

        public void APatternNotStart () {
                ProcAPatternNotStart ();
        }

        public void QPatternAndEvaluateTrue (EvalAndFactoryNode evalAndNode, MatchedEventMap passUp) {
                ProcQPatternAndEvaluateTrue (evalAndNode, passUp);
        }

        public void APatternAndEvaluateTrue (bool quitted) {
                ProcAPatternAndEvaluateTrue (quitted);
        }

        public void QPatternGuardScheduledEval () {
                ProcQPatternGuardScheduledEval ();
        }

        public void APatternGuardScheduledEval () {
                ProcAPatternGuardScheduledEval ();
        }

        public void QPatternGuardGuardQuit (EvalGuardFactoryNode evalGuardNode) {
                ProcQPatternGuardGuardQuit (evalGuardNode);
        }

        public void APatternGuardGuardQuit () {
                ProcAPatternGuardGuardQuit ();
        }

        public void QPatternAndQuit (EvalAndFactoryNode evalAndNode) {
                ProcQPatternAndQuit (evalAndNode);
        }

        public void APatternAndQuit () {
                ProcAPatternAndQuit ();
        }

        public void QPatternFilterQuit (EvalFilterFactoryNode evalFilterNode, MatchedEventMap beginState) {
                ProcQPatternFilterQuit (evalFilterNode, beginState);
        }

        public void APatternFilterQuit () {
                ProcAPatternFilterQuit ();
        }

        public void QPatternNotQuit (EvalNotFactoryNode evalNotNode) {
                ProcQPatternNotQuit (evalNotNode);
        }

        public void APatternNotQuit () {
                ProcAPatternNotQuit ();
        }

        public void QPatternNotEvalFalse (EvalNotFactoryNode evalNotNode) {
                ProcQPatternNotEvalFalse (evalNotNode);
        }

        public void APatternNotEvalFalse () {
                ProcAPatternNotEvalFalse ();
        }

        public void QPatternRootEvaluateTrue (MatchedEventMap matchEvent) {
                ProcQPatternRootEvaluateTrue (matchEvent);
        }

        public void APatternRootEvaluateTrue (bool quitted) {
                ProcAPatternRootEvaluateTrue (quitted);
        }

        public void QPatternObserverStart (EvalObserverFactoryNode evalObserverNode, MatchedEventMap beginState) {
                ProcQPatternObserverStart (evalObserverNode, beginState);
        }

        public void APatternObserverStart () {
                ProcAPatternObserverStart ();
        }

        public void QPatternMatchUntilEvaluateTrue (EvalMatchUntilFactoryNode evalMatchUntilNode, MatchedEventMap matchEvent, bool matchFromUntil) {
                ProcQPatternMatchUntilEvaluateTrue (evalMatchUntilNode, matchEvent, matchFromUntil);
        }

        public void APatternMatchUntilEvaluateTrue (bool quitted) {
                ProcAPatternMatchUntilEvaluateTrue (quitted);
        }

        public void QPatternMatchUntilStart (EvalMatchUntilFactoryNode evalMatchUntilNode, MatchedEventMap beginState) {
                ProcQPatternMatchUntilStart (evalMatchUntilNode, beginState);
        }

        public void APatternMatchUntilStart () {
                ProcAPatternMatchUntilStart ();
        }

        public void QPatternMatchUntilQuit (EvalMatchUntilFactoryNode evalMatchUntilNode)

        {
                ProcQPatternMatchUntilQuit (evalMatchUntilNode);
        }

        public void APatternMatchUntilQuit () {
                ProcAPatternMatchUntilQuit ();
        }

        public void QPatternMatchUntilEvalFalse (EvalMatchUntilFactoryNode evalMatchUntilNode, bool matchFromUntil) {
                ProcQPatternMatchUntilEvalFalse (evalMatchUntilNode, matchFromUntil);
        }

        public void APatternMatchUntilEvalFalse () {
                ProcAPatternMatchUntilEvalFalse ();
        }

        public void QPatternGuardEvaluateTrue (EvalGuardFactoryNode evalGuardNode, MatchedEventMap matchEvent) {
                ProcQPatternGuardEvaluateTrue (evalGuardNode, matchEvent);
        }

        public void APatternGuardEvaluateTrue (bool quitted) {
                ProcAPatternGuardEvaluateTrue (quitted);
        }

        public void QPatternGuardQuit (EvalGuardFactoryNode evalGuardNode) {
                ProcQPatternGuardQuit (evalGuardNode);
        }

        public void APatternGuardQuit () {
                ProcAPatternGuardQuit ();
        }

        public void QPatternEveryDistinctEvaluateTrue (EvalEveryDistinctFactoryNode everyDistinctNode, MatchedEventMap matchEvent) {
                ProcQPatternEveryDistinctEvaluateTrue (everyDistinctNode, matchEvent);
        }

        public void APatternEveryDistinctEvaluateTrue (ISet<object> keysFromNodeNoExpire, IDictionary<object,long> keysFromNodeExpire, object matchEventKey, bool haveSeenThis) {
                ProcAPatternEveryDistinctEvaluateTrue (keysFromNodeNoExpire, keysFromNodeExpire, matchEventKey, haveSeenThis);
        }

        public void QPatternEveryDistinctStart (EvalEveryDistinctFactoryNode everyNode, MatchedEventMap beginState) {
                ProcQPatternEveryDistinctStart (everyNode, beginState);
        }

        public void APatternEveryDistinctStart () {
                ProcAPatternEveryDistinctStart ();
        }

        public void QPatternEveryDistinctQuit (EvalEveryDistinctFactoryNode everyNode) {
                ProcQPatternEveryDistinctQuit (everyNode);
        }

        public void APatternEveryDistinctQuit () {
                ProcAPatternEveryDistinctQuit ();
        }

        public void QPatternFollowedByEvalFalse (EvalFollowedByFactoryNode evalFollowedByNode) {
                ProcQPatternFollowedByEvalFalse (evalFollowedByNode);
        }

        public void APatternFollowedByEvalFalse () {
                ProcAPatternFollowedByEvalFalse ();
        }

        public void QPatternEveryDistinctEvalFalse (EvalEveryDistinctFactoryNode everyNode) {
                ProcQPatternEveryDistinctEvalFalse (everyNode);
        }

        public void APatternEveryDistinctEvalFalse () {
                ProcAPatternEveryDistinctEvalFalse ();
        }

        public void QPatternEveryEvaluateTrue (EvalEveryFactoryNode evalEveryNode, MatchedEventMap matchEvent) {
                ProcQPatternEveryEvaluateTrue (evalEveryNode, matchEvent);
        }

        public void APatternEveryEvaluateTrue () {
                ProcAPatternEveryEvaluateTrue ();
        }

        public void QPatternEveryStart (EvalEveryFactoryNode evalEveryNode, MatchedEventMap beginState) {
                ProcQPatternEveryStart (evalEveryNode, beginState);
        }

        public void APatternEveryStart () {
                ProcAPatternEveryStart ();
        }

        public void QPatternEveryQuit (EvalEveryFactoryNode evalEveryNode) {
                ProcQPatternEveryQuit (evalEveryNode);
        }

        public void APatternEveryQuit () {
                ProcAPatternEveryQuit ();
        }

        public void QPatternEveryEvalFalse (EvalEveryFactoryNode evalEveryNode) {
                ProcQPatternEveryEvalFalse (evalEveryNode);
        }

        public void APatternEveryEvalFalse () {
                ProcAPatternEveryEvalFalse ();
        }

        public void QPatternOrEvaluateTrue (EvalOrFactoryNode evalOrNode, MatchedEventMap matchEvent) {
                ProcQPatternOrEvaluateTrue (evalOrNode, matchEvent);
        }

        public void APatternOrEvaluateTrue (bool quitted) {
                ProcAPatternOrEvaluateTrue (quitted);
        }

        public void QPatternOrStart (EvalOrFactoryNode evalOrNode, MatchedEventMap beginState) {
                ProcQPatternOrStart (evalOrNode, beginState);
        }

        public void APatternOrStart () {
                ProcAPatternOrStart ();
        }

        public void QPatternOrQuit (EvalOrFactoryNode evalOrNode) {
                ProcQPatternOrQuit (evalOrNode);
        }

        public void APatternOrQuit () {
                ProcAPatternOrQuit ();
        }

        public void QPatternOrEvalFalse (EvalOrFactoryNode evalOrNode) {
                ProcQPatternOrEvalFalse (evalOrNode);
        }

        public void APatternOrEvalFalse () {
                ProcAPatternOrEvalFalse ();
        }

        public void QPatternFollowedByStart (EvalFollowedByFactoryNode evalFollowedByNode, MatchedEventMap beginState) {
                ProcQPatternFollowedByStart (evalFollowedByNode, beginState);
        }

        public void APatternFollowedByStart () {
                ProcAPatternFollowedByStart ();
        }

        public void QPatternFollowedByQuit (EvalFollowedByFactoryNode evalFollowedByNode)

        {
                ProcQPatternFollowedByQuit (evalFollowedByNode);
        }

        public void APatternFollowedByQuit () {
                ProcAPatternFollowedByQuit ();
        }

        public void QPatternGuardEvalFalse (EvalGuardFactoryNode evalGuardNode) {
                ProcQPatternGuardEvalFalse (evalGuardNode);
        }

        public void APatternGuardEvalFalse () {
                ProcAPatternGuardEvalFalse ();
        }

        public void QContextScheduledEval (ContextRuntimeDescriptor contextDescriptor) {
                ProcQContextScheduledEval (contextDescriptor);
        }

        public void AContextScheduledEval () {
                ProcAContextScheduledEval ();
        }

        public void QContextPartitionAllocate (AgentInstanceContext agentInstanceContext) {
                ProcQContextPartitionAllocate (agentInstanceContext);
        }

        public void AContextPartitionAllocate () {
                ProcAContextPartitionAllocate ();
        }

        public void QContextPartitionDestroy (AgentInstanceContext agentInstanceContext)

        {
                ProcQContextPartitionDestroy (agentInstanceContext);
        }

        public void AContextPartitionDestroy () {
                ProcAContextPartitionDestroy ();
        }

        public void QPatternRootStart (MatchedEventMap root) {
                ProcQPatternRootStart (root);
        }

        public void APatternRootStart () {
                ProcAPatternRootStart ();
        }

        public void QPatternRootQuit () {
                ProcQPatternRootQuit ();
        }

        public void APatternRootQuit () {
                ProcAPatternRootQuit ();
        }

        public void QInfraOnAction (OnTriggerType triggerType, EventBean[] triggerEvents, EventBean[] matchingEvents) {
                ProcQInfraOnAction (triggerType, triggerEvents, matchingEvents);
        }

        public void AInfraOnAction () {
                ProcAInfraOnAction ();
        }

        public void QTableUpdatedEvent (EventBean theEvent) {
                ProcQTableUpdatedEvent (theEvent);
        }

        public void ATableUpdatedEvent () {
                ProcATableUpdatedEvent ();
        }

        public void QInfraMergeWhenThens (bool matched, EventBean triggerEvent, int numWhenThens) {
                ProcQInfraMergeWhenThens (matched, triggerEvent, numWhenThens);
        }

        public void AInfraMergeWhenThens (bool matched) {
                ProcAInfraMergeWhenThens (matched);
        }

        public void QInfraMergeWhenThenItem (bool matched, int count) {
                ProcQInfraMergeWhenThenItem (matched, count);
        }

        public void AInfraMergeWhenThenItem (bool matched, bool actionsApplied) {
                ProcAInfraMergeWhenThenItem (matched, actionsApplied);
        }

        public void QInfraMergeWhenThenActions (int numActions) {
                ProcQInfraMergeWhenThenActions (numActions);
        }

        public void AInfraMergeWhenThenActions () {
                ProcAInfraMergeWhenThenActions ();
        }

        public void QInfraMergeWhenThenActionItem (int count, string actionName) {
                ProcQInfraMergeWhenThenActionItem (count, actionName);
        }

        public void AInfraMergeWhenThenActionItem (bool applies) {
                ProcAInfraMergeWhenThenActionItem (applies);
        }

        public void QInfraTriggeredLookup (string lookupStrategy) {
                ProcQInfraTriggeredLookup (lookupStrategy);
        }

        public void AInfraTriggeredLookup (EventBean[] result) {
                ProcAInfraTriggeredLookup (result);
        }

        public void QIndexJoinLookup (JoinExecTableLookupStrategy strategy, EventTable index) {
                ProcQIndexJoinLookup (strategy, index);
        }

        public void AIndexJoinLookup (ICollection<EventBean> result, object keys) {
                ProcAIndexJoinLookup (result, keys);
        }

        public void QJoinDispatch (EventBean[][] newDataPerStream, EventBean[][] oldDataPerStream) {
                ProcQJoinDispatch (newDataPerStream, oldDataPerStream);
        }

        public void AJoinDispatch () {
                ProcAJoinDispatch ();
        }

        public void QJoinExecStrategy () {
                ProcQJoinExecStrategy ();
        }

        public void AJoinExecStrategy (UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> joinSet) {
                ProcAJoinExecStrategy (joinSet);
        }

        public void QJoinCompositionStreamToWin () {
                ProcQJoinCompositionStreamToWin ();
        }

        public void AJoinCompositionStreamToWin (ISet<MultiKeyArrayOfKeys<EventBean>> newResults) {
                ProcAJoinCompositionStreamToWin (newResults);
        }

        public void QJoinCompositionStepUpdIndex (int stream, EventBean[] added, EventBean[] removed) {
                ProcQJoinCompositionStepUpdIndex (stream, added, removed);
        }

        public void AJoinCompositionStepUpdIndex () {
                ProcAJoinCompositionStepUpdIndex ();
        }

        public void QIndexAddRemove (EventTable eventTable, EventBean[] newData, EventBean[] oldData) {
                ProcQIndexAddRemove (eventTable, newData, oldData);
        }

        public void AIndexAddRemove () {
                ProcAIndexAddRemove ();
        }

        public void QIndexAdd (EventTable eventTable, EventBean[] addEvents) {
                ProcQIndexAdd (eventTable, addEvents);
        }

        public void AIndexAdd () {
                ProcAIndexAdd ();
        }

        public void QIndexRemove (EventTable eventTable, EventBean[] removeEvents) {
                ProcQIndexRemove (eventTable, removeEvents);
        }

        public void AIndexRemove () {
                ProcAIndexRemove ();
        }

        public void QJoinCompositionQueryStrategy (bool insert, int streamNum, EventBean[] events) {
                ProcQJoinCompositionQueryStrategy (insert, streamNum, events);
        }

        public void AJoinCompositionQueryStrategy () {
                ProcAJoinCompositionQueryStrategy ();
        }

        public void QJoinExecProcess (UniformPair<ISet<MultiKeyArrayOfKeys<EventBean>>> joinSet) {
                ProcQJoinExecProcess (joinSet);
        }

        public void AJoinExecProcess () {
                ProcAJoinExecProcess ();
        }

        public void QJoinCompositionWinToWin () {
                ProcQJoinCompositionWinToWin ();
        }

        public void AJoinCompositionWinToWin (ISet<MultiKeyArrayOfKeys<EventBean>> newResults, ISet<MultiKeyArrayOfKeys<EventBean>> oldResults) {
                ProcAJoinCompositionWinToWin (newResults, oldResults);
        }

        public void QOutputProcessWCondition (EventBean[] newData, EventBean[] oldData) {
                ProcQOutputProcessWCondition (newData, oldData);
        }

        public void AOutputProcessWCondition (bool buffered) {
                ProcAOutputProcessWCondition (buffered);
        }

        public void QOutputRateConditionUpdate (int newDataLength, int oldDataLength) {
                ProcQOutputRateConditionUpdate (newDataLength, oldDataLength);
        }

        public void AOutputRateConditionUpdate () {
                ProcAOutputRateConditionUpdate ();
        }

        public void QOutputRateConditionOutputNow () {
                ProcQOutputRateConditionOutputNow ();
        }

        public void AOutputRateConditionOutputNow (bool generate) {
                ProcAOutputRateConditionOutputNow (generate);
        }

        public void QOutputProcessWConditionJoin (ISet<MultiKeyArrayOfKeys<EventBean>> newEvents, ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents) {
                ProcQOutputProcessWConditionJoin (newEvents, oldEvents);
        }

        public void AOutputProcessWConditionJoin (bool buffered) {
                ProcAOutputProcessWConditionJoin (buffered);
        }

        public void QWhereClauseFilter (string text, EventBean[] newData, EventBean[] oldData) {
                ProcQWhereClauseFilter (text, newData, oldData);
        }

        public void AWhereClauseFilter (EventBean[] filteredNewData, EventBean[] filteredOldData) {
                ProcAWhereClauseFilter (filteredNewData, filteredOldData);
        }

        public void QWhereClauseFilterEval (int num, EventBean @event, bool newData) {
                ProcQWhereClauseFilterEval (num, @event, newData);
        }

        public void AWhereClauseFilterEval (bool? pass) {
                ProcAWhereClauseFilterEval (pass);
        }

        public void QWhereClauseIR (EventBean[] filteredNewData, EventBean[] filteredOldData) {
                ProcQWhereClauseIR (filteredNewData, filteredOldData);
        }

        public void AWhereClauseIR () {
                ProcAWhereClauseIR ();
        }

        public void QSplitStream (bool all, EventBean theEvent, int numWhereClauses) {
                ProcQSplitStream (all, theEvent, numWhereClauses);
        }

        public void ASplitStream (bool all, bool handled) {
                ProcASplitStream (all, handled);
        }

        public void QSplitStreamWhere (int index) {
                ProcQSplitStreamWhere (index);
        }

        public void ASplitStreamWhere (bool? pass) {
                ProcASplitStreamWhere (pass);
        }

        public void QSplitStreamRoute (int index) {
                ProcQSplitStreamRoute (index);
        }

        public void ASplitStreamRoute () {
                ProcASplitStreamRoute ();
        }

        public void QSubselectAggregation () {
                ProcQSubselectAggregation ();
        }

        public void ASubselectAggregation () {
                ProcASubselectAggregation ();
        }

        public void QTableAddEvent (EventBean theEvent) {
                ProcQTableAddEvent (theEvent);
        }

        public void ATableAddEvent () {
                ProcATableAddEvent ();
        }

        public void QaTableUpdatedEventWKeyBefore (EventBean theEvent) {
                ProcQaTableUpdatedEventWKeyBefore (theEvent);
        }

        public void QaTableUpdatedEventWKeyAfter (EventBean theEvent) {
                ProcQaTableUpdatedEventWKeyAfter (theEvent);
        }

        public void QTableDeleteEvent (EventBean theEvent) {
                ProcQTableDeleteEvent (theEvent);
        }

        public void ATableDeleteEvent () {
                ProcATableDeleteEvent ();
        }

        public void QAggregationGroupedApplyEnterLeave (bool enter, int numAggregators, int numAccessStates, object groupKey) {
                ProcQAggregationGroupedApplyEnterLeave (enter, numAggregators, numAccessStates, groupKey);
        }

        public void AAggregationGroupedApplyEnterLeave (bool enter) {
                ProcAAggregationGroupedApplyEnterLeave (enter);
        }

        public void QAggNoAccessEnterLeave (bool enter, int index, object currentValue, string aggExpression) {
                ProcQAggNoAccessEnterLeave (enter, index, currentValue, aggExpression);
        }

        public void AAggNoAccessEnterLeave (bool enter, int index, object newValue) {
                ProcAAggNoAccessEnterLeave (enter, index, newValue);
        }

        public void QAggAccessEnterLeave (bool enter, int index, string aggExpr) {
                ProcQAggAccessEnterLeave (enter, index, aggExpr);
        }

        public void AAggAccessEnterLeave (bool enter, int index) {
                ProcAAggAccessEnterLeave (enter, index);
        }

        public void QUpdateIStream (InternalEventRouterEntry[] entries) {
                ProcQUpdateIStream (entries);
        }

        public void AUpdateIStream (EventBean finalEvent, bool haveCloned) {
                ProcAUpdateIStream (finalEvent, haveCloned);
        }

        public void QUpdateIStreamApply (int index, InternalEventRouterEntry entry) {
                ProcQUpdateIStreamApply (index, entry);
        }

        public void AUpdateIStreamApply (EventBean updated, bool applied) {
                ProcAUpdateIStreamApply (updated, applied);
        }

        public void QUpdateIStreamApplyWhere () {
                ProcQUpdateIStreamApplyWhere ();
        }

        public void AUpdateIStreamApplyWhere (bool? result) {
                ProcAUpdateIStreamApplyWhere (result);
        }

        public void QUpdateIStreamApplyAssignments (InternalEventRouterEntry entry) {
                ProcQUpdateIStreamApplyAssignments (entry);
        }

        public void AUpdateIStreamApplyAssignments (object[] values) {
                ProcAUpdateIStreamApplyAssignments (values);
        }

        public void QUpdateIStreamApplyAssignmentItem (int index) {
                ProcQUpdateIStreamApplyAssignmentItem (index);
        }

        public void AUpdateIStreamApplyAssignmentItem (object value) {
                ProcAUpdateIStreamApplyAssignmentItem (value);
        }

        public void QOutputRateConditionScheduledEval () {
                ProcQOutputRateConditionScheduledEval ();
        }

        public void AOutputRateConditionScheduledEval () {
                ProcAOutputRateConditionScheduledEval ();
        }

        public void QHistoricalScheduledEval () {
                ProcQHistoricalScheduledEval ();
        }

        public void AHistoricalScheduledEval () {
                ProcAHistoricalScheduledEval ();
        }

        public void QJoinExecFilter () {
                ProcQJoinExecFilter ();
        }

        public void AJoinExecFilter (ISet<MultiKeyArrayOfKeys<EventBean>> newEvents, ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents) {
                ProcAJoinExecFilter (newEvents, oldEvents);
        }

        public void QJoinCompositionHistorical () {
                ProcQJoinCompositionHistorical ();
        }

        public void AJoinCompositionHistorical (ISet<MultiKeyArrayOfKeys<EventBean>> newResults, ISet<MultiKeyArrayOfKeys<EventBean>> oldResults) {
                ProcAJoinCompositionHistorical (newResults, oldResults);
        }
    }
}
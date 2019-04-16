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
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     A view that prepares output events, batching incoming
    ///     events and invoking the result set processor as necessary.
    ///     <para />
    ///     Handles output rate limiting or stabilizing.
    /// </summary>
    public class OutputProcessViewConditionDefault : OutputProcessViewBaseWAfter
    {
        private readonly OutputCondition outputCondition;
        private readonly OutputProcessViewConditionFactory parent;

        public OutputProcessViewConditionDefault(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext,
            bool isJoin,
            EventType[] eventTypes)
            : base(
                agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents,
                afterConditionSatisfied)
        {
            this.parent = parent;

            var outputCallback = GetCallbackToLocal(parent.StreamCount);
            outputCondition =
                parent.OutputConditionFactory.InstantiateOutputCondition(agentInstanceContext, outputCallback);
            OptionalDeltaSet =
                agentInstanceContext.ResultSetProcessorHelperFactory.MakeOutputConditionChangeSet(
                    eventTypes, agentInstanceContext);
        }

        public override int NumChangesetRows => OptionalDeltaSet.NumChangesetRows;

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet { get; }

        public override OutputCondition OptionalOutputCondition => outputCondition;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        /// <summary>
        ///     The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QOutputProcessWCondition(newData, oldData);

            // add the incoming events to the event batches
            if (parent.IsAfter) {
                var afterSatisfied = CheckAfterCondition(newData, agentInstanceContext.StatementContext);
                if (!afterSatisfied) {
                    if (!parent.IsUnaggregatedUngrouped) {
                        OptionalDeltaSet.AddView(new UniformPair<EventBean[]>(newData, oldData));
                    }

                    instrumentationCommon.AOutputProcessWCondition(true);
                    return;
                }

                OptionalDeltaSet.AddView(new UniformPair<EventBean[]>(newData, oldData));
            }
            else {
                OptionalDeltaSet.AddView(new UniformPair<EventBean[]>(newData, oldData));
            }

            var newDataLength = 0;
            var oldDataLength = 0;
            if (newData != null) {
                newDataLength = newData.Length;
            }

            if (oldData != null) {
                oldDataLength = oldData.Length;
            }

            instrumentationCommon.QOutputRateConditionUpdate(newDataLength, oldDataLength);
            outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
            instrumentationCommon.AOutputRateConditionUpdate();

            instrumentationCommon.AOutputProcessWCondition(false);
        }

        /// <summary>
        ///     This process (update) method is for participation in a join.
        /// </summary>
        /// <param name="newEvents">new events</param>
        /// <param name="oldEvents">old events</param>
        public override void Process(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QOutputProcessWConditionJoin(newEvents, oldEvents);

            // add the incoming events to the event batches
            if (parent.IsAfter) {
                var afterSatisfied = CheckAfterCondition(newEvents, agentInstanceContext.StatementContext);
                if (!afterSatisfied) {
                    if (!parent.IsUnaggregatedUngrouped) {
                        AddToChangeset(newEvents, oldEvents, OptionalDeltaSet);
                    }

                    instrumentationCommon.AOutputProcessWConditionJoin(true);
                    return;
                }

                AddToChangeset(newEvents, oldEvents, OptionalDeltaSet);
            }
            else {
                AddToChangeset(newEvents, oldEvents, OptionalDeltaSet);
            }

            var newEventsSize = 0;
            if (newEvents != null) {
                newEventsSize = newEvents.Count;
            }

            var oldEventsSize = 0;
            if (oldEvents != null) {
                oldEventsSize = oldEvents.Count;
            }

            instrumentationCommon.QOutputRateConditionUpdate(newEventsSize, oldEventsSize);
            outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
            instrumentationCommon.AOutputRateConditionUpdate();

            instrumentationCommon.AOutputProcessWConditionJoin(false);
        }

        /// <summary>
        ///     Called once the output condition has been met.
        ///     Invokes the result set processor.
        ///     Used for non-join event data.
        /// </summary>
        /// <param name="doOutput">
        ///     true if the batched events should actually be output as well as processed, false if they should
        ///     just be processed
        /// </param>
        /// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingView(
            bool doOutput,
            bool forceUpdate)
        {
            agentInstanceContext.InstrumentationProvider.QOutputRateConditionOutputNow();

            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            // Process the events and get the result
            var newOldEvents = resultSetProcessor.ProcessOutputLimitedView(
                OptionalDeltaSet.ViewEventsSet, isGenerateSynthetic);

            if (parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, parent.EventBeanReader);
            }

            if (!isGenerateSynthetic && !isGenerateNatural) {
                ResetEventBatches();
                agentInstanceContext.InstrumentationProvider.AOutputRateConditionOutputNow(false);
                return;
            }

            if (doOutput) {
                Output(forceUpdate, newOldEvents);
            }

            ResetEventBatches();

            agentInstanceContext.InstrumentationProvider.AOutputRateConditionOutputNow(true);
        }

        protected virtual void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> results)
        {
            // Child view can be null in replay from named window
            if (child != null) {
                OutputStrategyUtil.Output(forceUpdate, results, child);
            }
        }

        public override void Stop(AgentInstanceStopServices services)
        {
            base.Stop(services);
            OptionalDeltaSet.Destroy();
            outputCondition.StopOutputCondition();
        }

        private void ResetEventBatches()
        {
            OptionalDeltaSet.Clear();
        }

        /// <summary>
        ///     Called once the output condition has been met.
        ///     Invokes the result set processor.
        ///     Used for join event data.
        /// </summary>
        /// <param name="doOutput">
        ///     true if the batched events should actually be output as well as processed, false if they should
        ///     just be processed
        /// </param>
        /// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingJoin(
            bool doOutput,
            bool forceUpdate)
        {
            agentInstanceContext.InstrumentationProvider.QOutputRateConditionOutputNow();

            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            // Process the events and get the result
            var newOldEvents = resultSetProcessor.ProcessOutputLimitedJoin(
                OptionalDeltaSet.JoinEventsSet, isGenerateSynthetic);

            if (parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, parent.EventBeanReader);
            }

            if (!isGenerateSynthetic && !isGenerateNatural) {
                if (AuditPath.isAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(agentInstanceContext.StatementContext, newOldEvents);
                }

                ResetEventBatches();
                agentInstanceContext.InstrumentationProvider.AOutputRateConditionOutputNow(false);
                return;
            }

            if (doOutput) {
                Output(forceUpdate, newOldEvents);
            }

            ResetEventBatches();

            agentInstanceContext.InstrumentationProvider.AOutputRateConditionOutputNow(true);
        }

        private OutputCallback GetCallbackToLocal(int streamCount)
        {
            // single stream means no join
            // multiple streams means a join
            if (streamCount == 1) {
                return (
                    doOutput,
                    forceUpdate) => ContinueOutputProcessingView(doOutput, forceUpdate);
            }

            return (
                doOutput,
                forceUpdate) => ContinueOutputProcessingJoin(doOutput, forceUpdate);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetIterator(
                joinExecutionStrategy, resultSetProcessor, parentView, parent.IsDistinct);
        }

        public override void Terminated()
        {
            if (parent.IsTerminable) {
                outputCondition.Terminated();
            }
        }

        private static void AddToChangeset(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            OutputProcessViewConditionDeltaSet joinEventsSet)
        {
            // add the incoming events to the event batches
            ISet<MultiKey<EventBean>> copyNew;
            if (newEvents != null) {
                copyNew = new LinkedHashSet<MultiKey<EventBean>>(newEvents);
            }
            else {
                copyNew = new LinkedHashSet<MultiKey<EventBean>>();
            }

            ISet<MultiKey<EventBean>> copyOld;
            if (oldEvents != null) {
                copyOld = new LinkedHashSet<MultiKey<EventBean>>(oldEvents);
            }
            else {
                copyOld = new LinkedHashSet<MultiKey<EventBean>>();
            }

            joinEventsSet.AddJoin(new UniformPair<ISet<MultiKey<EventBean>>>(copyNew, copyOld));
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.resultset.simple;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    /// Handles output rate limiting for FIRST, only applicable with a having-clause and no group-by clause.
    /// <para />Without having-clause the order of processing won't matter therefore its handled by the
    /// <seealso cref="OutputProcessViewConditionDefault" />. With group-by the <seealso cref="ResultSetProcessor" /> handles the per-group first criteria.
    /// </summary>
    public class OutputProcessViewConditionFirst : OutputProcessViewBaseWAfter
    {
        private readonly OutputProcessViewConditionFactory parent;
        private readonly OutputCondition outputCondition;

        // Posted events in ordered form (for applying to aggregates) and summarized per type
        // Using ArrayList as random access is a requirement.
        private IList<UniformPair<EventBean[]>> viewEventsList = new List<UniformPair<EventBean[]>>();

        private IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet =
            new List<UniformPair<ISet<MultiKey<EventBean>>>>();

        private ResultSetProcessorSimpleOutputFirstHelper witnessedFirstHelper;

        private static readonly ILog Log = LogManager.GetLogger(typeof(OutputProcessViewConditionFirst));

        public OutputProcessViewConditionFirst(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext)
            : base(agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied)
        {
            this.parent = parent;

            OutputCallback outputCallback = GetCallbackToLocal(parent.StreamCount);
            this.outputCondition =
                parent.OutputConditionFactory.InstantiateOutputCondition(agentInstanceContext, outputCallback);
            witnessedFirstHelper =
                agentInstanceContext.ResultSetProcessorHelperFactory.MakeRSSimpleOutputFirst(agentInstanceContext);
        }

        public override int NumChangesetRows => Math.Max(viewEventsList.Count, joinEventsSet.Count);

        public override OutputCondition OptionalOutputCondition => outputCondition;

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        /// <summary>
        /// The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(
                    ".update Received update, " +
                    "  newData.length==" + ((newData == null) ? 0 : newData.Length) +
                    "  oldData.length==" + ((oldData == null) ? 0 : oldData.Length));
            }

            if (!base.CheckAfterCondition(newData, agentInstanceContext.StatementContext)) {
                return;
            }

            if (!witnessedFirstHelper.WitnessedFirst) {
                StatementResultService statementResultService = agentInstanceContext.StatementResultService;
                bool isGenerateSynthetic = statementResultService.IsMakeSynthetic;

                // Process the events and get the result
                viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));
                UniformPair<EventBean[]> newOldEvents =
                    resultSetProcessor.ProcessOutputLimitedView(viewEventsList, isGenerateSynthetic);
                viewEventsList.Clear();

                if (!HasRelevantResults(newOldEvents)) {
                    return;
                }

                witnessedFirstHelper.WitnessedFirst = true;

                if (parent.IsDistinct) {
                    newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, parent.EventBeanReader);
                    newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                        newOldEvents.Second, parent.EventBeanReader);
                }

                bool isGenerateNatural = statementResultService.IsMakeNatural;
                if ((!isGenerateSynthetic) && (!isGenerateNatural)) {
                    if (AuditPath.isAuditEnabled) {
                        OutputStrategyUtil.IndicateEarlyReturn(agentInstanceContext.StatementContext, newOldEvents);
                    }

                    return;
                }

                Output(true, newOldEvents);
            }
            else {
                viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));
                resultSetProcessor.ProcessOutputLimitedView(viewEventsList, false);
                viewEventsList.Clear();
            }

            int newDataLength = 0;
            int oldDataLength = 0;
            if (newData != null) {
                newDataLength = newData.Length;
            }

            if (oldData != null) {
                oldDataLength = oldData.Length;
            }

            outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
        }

        /// <summary>
        /// This process (update) method is for participation in a join.
        /// </summary>
        /// <param name="newEvents">new events</param>
        /// <param name="oldEvents">old events</param>
        public override void Process(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(
                    ".process Received update, " +
                    "  newData.length==" + ((newEvents == null) ? 0 : newEvents.Count) +
                    "  oldData.length==" + ((oldEvents == null) ? 0 : oldEvents.Count));
            }

            if (!base.CheckAfterCondition(newEvents, agentInstanceContext.StatementContext)) {
                return;
            }

            // add the incoming events to the event batches
            if (!witnessedFirstHelper.WitnessedFirst) {
                StatementResultService statementResultService = agentInstanceContext.StatementResultService;

                AddToChangeSet(joinEventsSet, newEvents, oldEvents);
                bool isGenerateSynthetic = statementResultService.IsMakeSynthetic;
                UniformPair<EventBean[]> newOldEvents = resultSetProcessor
                    .ProcessOutputLimitedJoin(joinEventsSet, isGenerateSynthetic);
                joinEventsSet.Clear();

                if (!HasRelevantResults(newOldEvents)) {
                    return;
                }

                witnessedFirstHelper.WitnessedFirst = true;

                if (parent.IsDistinct) {
                    newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, parent.EventBeanReader);
                    newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                        newOldEvents.Second, parent.EventBeanReader);
                }

                bool isGenerateNatural = statementResultService.IsMakeNatural;
                if ((!isGenerateSynthetic) && (!isGenerateNatural)) {
                    if (AuditPath.isAuditEnabled) {
                        OutputStrategyUtil.IndicateEarlyReturn(agentInstanceContext.StatementContext, newOldEvents);
                    }

                    return;
                }

                Output(true, newOldEvents);
            }
            else {
                AddToChangeSet(joinEventsSet, newEvents, oldEvents);

                // Process the events and get the result
                resultSetProcessor.ProcessOutputLimitedJoin(joinEventsSet, false);
                joinEventsSet.Clear();
            }

            int newEventsSize = 0;
            if (newEvents != null) {
                newEventsSize = newEvents.Count;
            }

            int oldEventsSize = 0;
            if (oldEvents != null) {
                oldEventsSize = oldEvents.Count;
            }

            outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
        }

        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for non-join event data.
        /// </summary>
        /// <param name="doOutput">true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingView(
            bool doOutput,
            bool forceUpdate)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".continueOutputProcessingView");
            }

            witnessedFirstHelper.WitnessedFirst = false;
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

        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for join event data.
        /// </summary>
        /// <param name="doOutput">true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingJoin(
            bool doOutput,
            bool forceUpdate)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".continueOutputProcessingJoin");
            }

            witnessedFirstHelper.WitnessedFirst = false;
        }

        private OutputCallback GetCallbackToLocal(int streamCount)
        {
            // single stream means no join
            // multiple streams means a join
            if (streamCount == 1) {
                return ContinueOutputProcessingView;
            }
            else {
                return ContinueOutputProcessingJoin;
            }
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

        public override void Stop(AgentInstanceStopServices services)
        {
            base.Stop(services);
            outputCondition.StopOutputCondition();
            witnessedFirstHelper.Destroy();
        }

        private bool HasRelevantResults(UniformPair<EventBean[]> newOldEvents)
        {
            if (newOldEvents == null) {
                return false;
            }

            if (parent.SelectClauseStreamSelectorEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
                if (newOldEvents.First == null) {
                    return false; // nothing to indicate
                }
            }
            else if (parent.SelectClauseStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH) {
                if (newOldEvents.First == null && newOldEvents.Second == null) {
                    return false; // nothing to indicate
                }
            }
            else {
                if (newOldEvents.Second == null) {
                    return false; // nothing to indicate
                }
            }

            return true;
        }

        private static void AddToChangeSet(
            IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet,
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents)
        {
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

            joinEventsSet.Add(new UniformPair<ISet<MultiKey<EventBean>>>(copyNew, copyOld));
        }
    }
} // end of namespace
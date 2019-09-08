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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Handles output rate limiting for FIRST, only applicable with a having-clause and no group-by clause.
    ///     <para />
    ///     Without having-clause the order of processing won't matter therefore its handled by the
    ///     <seealso cref="OutputProcessViewConditionDefault" />. With group-by the <seealso cref="ResultSetProcessor" />
    ///     handles the per-group first criteria.
    /// </summary>
    public class OutputProcessViewConditionFirst : OutputProcessViewBaseWAfter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OutputProcessViewConditionFirst));
        private readonly OutputCondition _outputCondition;
        private readonly OutputProcessViewConditionFactory _parent;

        private readonly IList<UniformPair<ISet<MultiKey<EventBean>>>> _joinEventsSet =
            new List<UniformPair<ISet<MultiKey<EventBean>>>>();

        // Posted events in ordered form (for applying to aggregates) and summarized per type
        // Using ArrayList as random access is a requirement.
        private readonly IList<UniformPair<EventBean[]>> _viewEventsList = new List<UniformPair<EventBean[]>>();

        private readonly ResultSetProcessorSimpleOutputFirstHelper _witnessedFirstHelper;

        public OutputProcessViewConditionFirst(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext)
            : base(
                agentInstanceContext,
                resultSetProcessor,
                afterConditionTime,
                afterConditionNumberOfEvents,
                afterConditionSatisfied)
        {
            _parent = parent;

            var outputCallback = GetCallbackToLocal(parent.StreamCount);
            _outputCondition =
                parent.OutputConditionFactory.InstantiateOutputCondition(agentInstanceContext, outputCallback);
            _witnessedFirstHelper =
                agentInstanceContext.ResultSetProcessorHelperFactory.MakeRSSimpleOutputFirst(agentInstanceContext);
        }

        public override int NumChangesetRows => Math.Max(_viewEventsList.Count, _joinEventsSet.Count);

        public override OutputCondition OptionalOutputCondition => _outputCondition;

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

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
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".update Received update, " +
                    "  newData.length==" +
                    (newData == null ? 0 : newData.Length) +
                    "  oldData.length==" +
                    (oldData == null ? 0 : oldData.Length));
            }

            if (!CheckAfterCondition(newData, _agentInstanceContext.StatementContext)) {
                return;
            }

            if (!_witnessedFirstHelper.WitnessedFirst) {
                var statementResultService = _agentInstanceContext.StatementResultService;
                var isGenerateSynthetic = statementResultService.IsMakeSynthetic;

                // Process the events and get the result
                _viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));
                var newOldEvents =
                    _resultSetProcessor.ProcessOutputLimitedView(_viewEventsList, isGenerateSynthetic);
                _viewEventsList.Clear();

                if (!HasRelevantResults(newOldEvents)) {
                    return;
                }

                _witnessedFirstHelper.WitnessedFirst = true;

                if (_parent.IsDistinct) {
                    newOldEvents.First = EventBeanUtility.GetDistinctByProp(
                        newOldEvents.First,
                        _parent.EventBeanReader);
                    newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                        newOldEvents.Second,
                        _parent.EventBeanReader);
                }

                var isGenerateNatural = statementResultService.IsMakeNatural;
                if (!isGenerateSynthetic && !isGenerateNatural) {
                    if (AuditPath.isAuditEnabled) {
                        OutputStrategyUtil.IndicateEarlyReturn(_agentInstanceContext.StatementContext, newOldEvents);
                    }

                    return;
                }

                Output(true, newOldEvents);
            }
            else {
                _viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));
                _resultSetProcessor.ProcessOutputLimitedView(_viewEventsList, false);
                _viewEventsList.Clear();
            }

            var newDataLength = 0;
            var oldDataLength = 0;
            if (newData != null) {
                newDataLength = newData.Length;
            }

            if (oldData != null) {
                oldDataLength = oldData.Length;
            }

            _outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
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
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".process Received update, " +
                    "  newData.length==" +
                    (newEvents == null ? 0 : newEvents.Count) +
                    "  oldData.length==" +
                    (oldEvents == null ? 0 : oldEvents.Count));
            }

            if (!CheckAfterCondition(newEvents, _agentInstanceContext.StatementContext)) {
                return;
            }

            // add the incoming events to the event batches
            if (!_witnessedFirstHelper.WitnessedFirst) {
                var statementResultService = _agentInstanceContext.StatementResultService;

                AddToChangeSet(_joinEventsSet, newEvents, oldEvents);
                var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
                var newOldEvents = _resultSetProcessor
                    .ProcessOutputLimitedJoin(_joinEventsSet, isGenerateSynthetic);
                _joinEventsSet.Clear();

                if (!HasRelevantResults(newOldEvents)) {
                    return;
                }

                _witnessedFirstHelper.WitnessedFirst = true;

                if (_parent.IsDistinct) {
                    newOldEvents.First = EventBeanUtility.GetDistinctByProp(
                        newOldEvents.First,
                        _parent.EventBeanReader);
                    newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                        newOldEvents.Second,
                        _parent.EventBeanReader);
                }

                var isGenerateNatural = statementResultService.IsMakeNatural;
                if (!isGenerateSynthetic && !isGenerateNatural) {
                    if (AuditPath.isAuditEnabled) {
                        OutputStrategyUtil.IndicateEarlyReturn(_agentInstanceContext.StatementContext, newOldEvents);
                    }

                    return;
                }

                Output(true, newOldEvents);
            }
            else {
                AddToChangeSet(_joinEventsSet, newEvents, oldEvents);

                // Process the events and get the result
                _resultSetProcessor.ProcessOutputLimitedJoin(_joinEventsSet, false);
                _joinEventsSet.Clear();
            }

            var newEventsSize = 0;
            if (newEvents != null) {
                newEventsSize = newEvents.Count;
            }

            var oldEventsSize = 0;
            if (oldEvents != null) {
                oldEventsSize = oldEvents.Count;
            }

            _outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
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
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(".continueOutputProcessingView");
            }

            _witnessedFirstHelper.WitnessedFirst = false;
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
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(".continueOutputProcessingJoin");
            }

            _witnessedFirstHelper.WitnessedFirst = false;
        }

        private OutputCallback GetCallbackToLocal(int streamCount)
        {
            // single stream means no join
            // multiple streams means a join
            if (streamCount == 1) {
                return ContinueOutputProcessingView;
            }

            return ContinueOutputProcessingJoin;
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetEnumerator(
                joinExecutionStrategy,
                _resultSetProcessor,
                parentView,
                _parent.IsDistinct);
        }

        public override void Terminated()
        {
            if (_parent.IsTerminable) {
                _outputCondition.Terminated();
            }
        }

        public override void Stop(AgentInstanceStopServices services)
        {
            base.Stop(services);
            _outputCondition.StopOutputCondition();
            _witnessedFirstHelper.Destroy();
        }

        private bool HasRelevantResults(UniformPair<EventBean[]> newOldEvents)
        {
            if (newOldEvents == null) {
                return false;
            }

            if (_parent.SelectClauseStreamSelectorEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
                if (newOldEvents.First == null) {
                    return false; // nothing to indicate
                }
            }
            else if (_parent.SelectClauseStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH) {
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
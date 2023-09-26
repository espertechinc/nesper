///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.output.condition;
using com.espertech.esper.common.@internal.epl.output.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Handles output rate limiting for LAST and without order-by.
    /// </summary>
    public class OutputProcessViewConditionLastAllUnord : OutputProcessViewBaseWAfter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly OutputProcessViewConditionFactory _parent;

        public OutputProcessViewConditionLastAllUnord(
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
            OptionalOutputCondition =
                parent.OutputConditionFactory.InstantiateOutputCondition(agentInstanceContext, outputCallback);
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition { get; }

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".update Received update, " +
                    "  newData.length==" +
                    (newData?.Length ?? 0) +
                    "  oldData.length==" +
                    (oldData?.Length ?? 0));
            }

            var isGenerateSynthetic = _agentInstanceContext.StatementResultService.IsMakeSynthetic;
            _resultSetProcessor.ProcessOutputLimitedLastAllNonBufferedView(newData, oldData, isGenerateSynthetic);

            if (!CheckAfterCondition(newData, _agentInstanceContext.StatementContext)) {
                return;
            }

            var newDataLength = 0;
            var oldDataLength = 0;
            if (newData != null) {
                newDataLength = newData.Length;
            }

            if (oldData != null) {
                oldDataLength = oldData.Length;
            }

            OptionalOutputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
        }

        /// <summary>
        ///     This process (update) method is for participation in a join.
        /// </summary>
        /// <param name="newEvents">new events</param>
        /// <param name="oldEvents">old events</param>
        /// <param name="exprEvaluatorContext">the evaluator context</param>
        public override void Process(
            ISet<MultiKeyArrayOfKeys<EventBean>> newEvents,
            ISet<MultiKeyArrayOfKeys<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (ExecutionPathDebugLog.IsDebugEnabled && Log.IsDebugEnabled) {
                Log.Debug(
                    ".process Received update, " +
                    "  newData.length==" +
                    (newEvents?.Count ?? 0) +
                    "  oldData.length==" +
                    (oldEvents?.Count ?? 0));
            }

            var isGenerateSynthetic = _agentInstanceContext.StatementResultService.IsMakeSynthetic;
            _resultSetProcessor.ProcessOutputLimitedLastAllNonBufferedJoin(newEvents, oldEvents, isGenerateSynthetic);

            if (!CheckAfterCondition(newEvents, _agentInstanceContext.StatementContext)) {
                return;
            }

            var newEventsSize = 0;
            if (newEvents != null) {
                newEventsSize = newEvents.Count;
            }

            var oldEventsSize = 0;
            if (oldEvents != null) {
                oldEventsSize = oldEvents.Count;
            }

            OptionalOutputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
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

            var isGenerateSynthetic = _agentInstanceContext.StatementResultService.IsMakeSynthetic;
            var newOldEvents = _resultSetProcessor.ContinueOutputLimitedLastAllNonBufferedView(isGenerateSynthetic);

            ContinueOutputProcessingViewAndJoin(doOutput, forceUpdate, newOldEvents);
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

            var isGenerateSynthetic = _agentInstanceContext.StatementResultService.IsMakeSynthetic;
            var newOldEvents = _resultSetProcessor.ContinueOutputLimitedLastAllNonBufferedJoin(isGenerateSynthetic);

            ContinueOutputProcessingViewAndJoin(doOutput, forceUpdate, newOldEvents);
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
                _parent.IsDistinct,
                _parent.DistinctKeyGetter);
        }

        public override void Terminated()
        {
            if (_parent.IsTerminable) {
                OptionalOutputCondition.Terminated();
            }
        }

        public override void Stop(AgentInstanceStopServices services)
        {
            base.Stop(services);
            OptionalOutputCondition.StopOutputCondition();
        }

        private void ContinueOutputProcessingViewAndJoin(
            bool doOutput,
            bool forceUpdate,
            UniformPair<EventBean[]> newOldEvents)
        {
            if (_parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.DistinctKeyGetter);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                    newOldEvents.Second,
                    _parent.DistinctKeyGetter);
            }

            if (doOutput) {
                Output(forceUpdate, newOldEvents);
            }
        }
    }
} // end of namespace
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
using com.espertech.esper.common.@internal.epl.output.view;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.core
{
    /// <summary>
    ///     A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionSnapshot : OutputProcessViewBaseWAfter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly OutputProcessViewConditionFactory parent;

        public OutputProcessViewConditionSnapshot(
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext)
            : base(
                agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents,
                afterConditionSatisfied)
        {
            this.parent = parent;

            var outputCallback = GetCallbackToLocal(parent.StreamCount);
            OptionalOutputCondition =
                parent.OutputConditionFactory.InstantiateOutputCondition(agentInstanceContext, outputCallback);
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition { get; }

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        public override void Stop(AgentInstanceStopServices services)
        {
            base.Stop(services);
            OptionalOutputCondition.StopOutputCondition();
        }

        /// <summary>
        ///     The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            resultSetProcessor.ApplyViewResult(newData, oldData);

            if (!CheckAfterCondition(newData, agentInstanceContext.StatementContext)) {
                return;
            }

            // add the incoming events to the event batches
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
        public override void Process(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            resultSetProcessor.ApplyJoinResult(newEvents, oldEvents);

            if (!CheckAfterCondition(newEvents, agentInstanceContext.StatementContext)) {
                return;
            }

            var newEventsSize = 0;
            if (newEvents != null) {
                // add the incoming events to the event batches
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
            EventBean[] newEvents = null;
            EventBean[] oldEvents = null;

            var it = GetEnumerator();
            if (it.MoveNext()) {
                var snapshot = new List<EventBean>();
                while (it.MoveNext()) {
                    EventBean @event = it.Current;
                    snapshot.Add(@event);
                }

                newEvents = snapshot.ToArray();
                oldEvents = null;
            }

            var newOldEvents = new UniformPair<EventBean[]>(newEvents, oldEvents);

            if (doOutput) {
                Output(forceUpdate, newOldEvents);
            }
        }

        public virtual void Output(
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
            if (ExecutionPathDebugLog.IsEnabled && Log.IsDebugEnabled) {
                Log.Debug(".continueOutputProcessingJoin");
            }

            ContinueOutputProcessingView(doOutput, forceUpdate);
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
            return OutputStrategyUtil.GetIterator(
                joinExecutionStrategy, resultSetProcessor, parentView, parent.IsDistinct);
        }

        public override void Terminated()
        {
            if (parent.IsTerminable) {
                OptionalOutputCondition.Terminated();
            }
        }
    }
} // end of namespace
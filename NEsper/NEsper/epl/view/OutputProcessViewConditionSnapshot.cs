///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// A view that handles the "output snapshot" keyword in output rate stabilizing.
    /// </summary>
    public class OutputProcessViewConditionSnapshot : OutputProcessViewBaseWAfter
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly OutputProcessViewConditionFactory _parent;
        private readonly OutputCondition _outputCondition;

        public OutputProcessViewConditionSnapshot(
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext)
            : base(resultSetProcessorHelperFactory, agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied)
        {
            _parent = parent;
    
            var outputCallback = GetCallbackToLocal(parent.StreamCount);
            _outputCondition = parent.OutputConditionFactory.Make(agentInstanceContext, outputCallback);
        }
    
        public override void Stop()
        {
            base.Stop();
            _outputCondition.Stop();
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => _outputCondition;

        public override OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        /// <summary>
        /// The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">- new events</param>
        /// <param name="oldData">- old events</param>
        public override void Update(EventBean[] newData, EventBean[] oldData) {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".update Received update, " +
                        "  newData.Length==" + ((newData == null) ? 0 : newData.Length) +
                        "  oldData.Length==" + ((oldData == null) ? 0 : oldData.Length));
            }
    
            ResultSetProcessor.ApplyViewResult(newData, oldData);
    
            if (!base.CheckAfterCondition(newData, _parent.StatementContext)) {
                return;
            }
    
            // add the incoming events to the event batches
            int newDataLength = 0;
            int oldDataLength = 0;
            if (newData != null) {
                newDataLength = newData.Length;
            }
            if (oldData != null) {
                oldDataLength = oldData.Length;
            }
    
            _outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
        }

        /// <summary>
        /// This process (update) method is for participation in a join.
        /// </summary>
        /// <param name="newEvents">- new events</param>
        /// <param name="oldEvents">- old events</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        public override void Process(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents, 
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".Process Received update, " +
                        "  newData.Length==" + ((newEvents == null) ? 0 : newEvents.Count) +
                        "  oldData.Length==" + ((oldEvents == null) ? 0 : oldEvents.Count));
            }
    
            ResultSetProcessor.ApplyJoinResult(newEvents, oldEvents);
    
            if (!base.CheckAfterCondition(newEvents, _parent.StatementContext)) {
                return;
            }
    
            int newEventsSize = 0;
            if (newEvents != null) {
                // add the incoming events to the event batches
                newEventsSize = newEvents.Count;
            }
    
            int oldEventsSize = 0;
            if (oldEvents != null) {
                oldEventsSize = oldEvents.Count;
            }
    
            _outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
        }

        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for non-join event data.
        /// </summary>
        /// <param name="doOutput">- true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">- true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingView(bool doOutput, bool forceUpdate)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingView");
            }

            EventBean[] newEvents = null;
            EventBean[] oldEvents = null;

            IEnumerator<EventBean> en = GetEnumerator();

            var snapshot = new List<EventBean>();
            while (en.MoveNext())
            {
                EventBean @event = en.Current;
                snapshot.Add(@event);
            }
            newEvents = snapshot.ToArray();
            oldEvents = null;

            var newOldEvents = new UniformPair<EventBean[]>(newEvents, oldEvents);

            if (doOutput)
            {
                Output(forceUpdate, newOldEvents);
            }
        }

        public virtual void Output(bool forceUpdate, UniformPair<EventBean[]> results) {
            // Child view can be null in replay from named window
            if (ChildView != null) {
                OutputStrategyUtil.Output(forceUpdate, results, ChildView);
            }
        }
    
        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for join event data.
        /// </summary>
        /// <param name="doOutput">- true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">- true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingJoin(bool doOutput, bool forceUpdate) {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".continueOutputProcessingJoin");
            }
            ContinueOutputProcessingView(doOutput, forceUpdate);
        }

        private OutputCallback GetCallbackToLocal(int streamCount)
        {
            // single stream means no join
            // multiple streams means a join
            if (streamCount == 1)
            {
                return ContinueOutputProcessingView;
            }
            else
            {
                return ContinueOutputProcessingJoin;
            }
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetEnumerator(
                JoinExecutionStrategy, ResultSetProcessor, ParentView, _parent.IsDistinct);
        }

        public override void Terminated()
        {
            if (_parent.IsTerminable)
            {
                _outputCondition.Terminated();
            }
        }
    }
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// A view that prepares output events, batching incoming
    /// events and invoking the result set processor as necessary.
    /// <para>
    /// Handles output rate limiting or stabilizing.
    /// </para>
    /// </summary> 
    public class OutputProcessViewConditionDefault : OutputProcessViewBaseWAfter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly OutputProcessViewConditionFactory _parent ;
        private readonly OutputCondition _outputCondition ;

        // Posted events in ordered form (for applying to aggregates) and summarized per type
        // Using ArrayList as random access is a requirement.
        private readonly List<UniformPair<EventBean[]>> _viewEventsList = 
            new List<UniformPair<EventBean[]>>();

        private readonly List<UniformPair<ISet<MultiKey<EventBean>>>> _joinEventsSet =
            new List<UniformPair<ISet<MultiKey<EventBean>>>>();

        public OutputProcessViewConditionDefault(ResultSetProcessor resultSetProcessor, long? afterConditionTime, int? afterConditionNumberOfEvents, bool afterConditionSatisfied, OutputProcessViewConditionFactory parent, AgentInstanceContext agentInstanceContext)
            : base(resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied)
        {
            _parent = parent;
            OutputCallback outputCallback = GetCallbackToLocal(parent.StreamCount);
            _outputCondition = parent.OutputConditionFactory.Make(agentInstanceContext, outputCallback);
        }

        /// <summary>
        /// The Update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">The new data.</param>
        /// <param name="oldData">The old data.</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputProcessWCondition(newData, oldData); }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".Update Received Update, " +
                    "  newData.Length==" + ((newData == null) ? 0 : newData.Length) +
                    "  oldData.Length==" + ((oldData == null) ? 0 : oldData.Length));
            }

            if (!base.CheckAfterCondition(newData, _parent.StatementContext))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessWCondition(false); }
                return;
            }

            int newDataLength = 0;
            int oldDataLength = 0;
            if (newData != null)
            {
                newDataLength = newData.Length;
            }
            if (oldData != null)
            {
                oldDataLength = oldData.Length;
            }

            // add the incoming events to the event batches
            _viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputRateConditionUpdate(newDataLength, oldDataLength); }
            _outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputRateConditionUpdate(); }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessWCondition(true); }
        }

        /// <summary>
        /// This process (Update) method is for participation in a join.
        /// </summary>
        /// <param name="newEvents">The new events.</param>
        /// <param name="oldEvents">The old events.</param>
        /// <param name="exprEvaluatorContext">The expr evaluator context.</param>
        public override void Process(
            ISet<MultiKey<EventBean>> newEvents,
            ISet<MultiKey<EventBean>> oldEvents,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputProcessWConditionJoin(newEvents, oldEvents); }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".process Received Update, " +
                    "  newData.Length==" + ((newEvents == null) ? 0 : newEvents.Count) +
                    "  oldData.Length==" + ((oldEvents == null) ? 0 : oldEvents.Count));
            }

            if (!base.CheckAfterCondition(newEvents, _parent.StatementContext))
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessWConditionJoin(false); }
                return;
            }

            int newEventsSize = 0;
            if (newEvents != null)
            {
                newEventsSize = newEvents.Count;
            }

            int oldEventsSize = 0;
            if (oldEvents != null)
            {
                oldEventsSize = oldEvents.Count;
            }

            // add the incoming events to the event batches
            ISet<MultiKey<EventBean>> copyNew;
            if (newEvents != null)
            {
                copyNew = new LinkedHashSet<MultiKey<EventBean>>(newEvents);
            }
            else
            {
                copyNew = new LinkedHashSet<MultiKey<EventBean>>();
            }

            ISet<MultiKey<EventBean>> copyOld = oldEvents != null 
                ? new LinkedHashSet<MultiKey<EventBean>>(oldEvents) 
                : new LinkedHashSet<MultiKey<EventBean>>();

            _joinEventsSet.Add(new UniformPair<ISet<MultiKey<EventBean>>>(copyNew, copyOld));

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputRateConditionUpdate(newEventsSize, oldEventsSize); }
            _outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputRateConditionUpdate(); }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputProcessWConditionJoin(true); }
        }

        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for non-join event data.
        /// </summary>
        /// <param name="doOutput">true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingView(bool doOutput, bool forceUpdate)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputRateConditionOutputNow(); }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingView");
            }

            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;

            // Process the events and get the result
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessOutputLimitedView(
                _viewEventsList, isGenerateSynthetic, _parent.OutputLimitLimitType);

            if (_parent.IsDistinct && newOldEvents != null)
            {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, _parent.EventBeanReader);
            }

            if ((!isGenerateSynthetic) && (!isGenerateNatural))
            {
                if (AuditPath.IsAuditEnabled)
                {
                    OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                }
                ResetEventBatches();
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputRateConditionOutputNow(false); }
                return;
            }

            if (doOutput)
            {
                Output(forceUpdate, newOldEvents);
            }
            ResetEventBatches();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputRateConditionOutputNow(true); }
        }

        protected virtual void Output(bool forceUpdate, UniformPair<EventBean[]> results)
        {
            // Child view can be null in replay from named window
            if (ChildView != null)
            {
                OutputStrategyUtil.Output(forceUpdate, results, ChildView);
            }
        }

        private void ResetEventBatches()
        {
            _viewEventsList.Clear();
            _joinEventsSet.Clear();
        }

        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for join event data.
        /// </summary>
        /// <param name="doOutput">true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>

        protected void ContinueOutputProcessingJoin(bool doOutput, bool forceUpdate)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QOutputRateConditionOutputNow(); }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingJoin");
            }

            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;

            // Process the events and get the result
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessOutputLimitedJoin(
                _joinEventsSet, isGenerateSynthetic, _parent.OutputLimitLimitType);

            if (_parent.IsDistinct && newOldEvents != null)
            {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, _parent.EventBeanReader);
            }

            if ((!isGenerateSynthetic) && (!isGenerateNatural))
            {
                if (AuditPath.IsAuditEnabled)
                {
                    OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                }
                ResetEventBatches();
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputRateConditionOutputNow(false); }
                return;
            }

            if (doOutput)
            {
                Output(forceUpdate, newOldEvents);
            }
            ResetEventBatches();

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AOutputRateConditionOutputNow(true); }
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
}
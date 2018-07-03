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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
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
        private static readonly ILog Log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly OutputProcessViewConditionFactory _parent;
        private readonly OutputCondition _outputCondition;

        // Posted events in ordered form (for applying to aggregates) and summarized per type
        // Using List as random access is a requirement.
        private readonly OutputProcessViewConditionDeltaSet _deltaSet;

        public OutputProcessViewConditionDefault(
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewConditionFactory parent,
            AgentInstanceContext agentInstanceContext,
            bool isJoin)
            : base(
                resultSetProcessorHelperFactory, agentInstanceContext, resultSetProcessor, afterConditionTime,
                afterConditionNumberOfEvents, afterConditionSatisfied)
        {
            _parent = parent;

            OutputCallback outputCallback = GetCallbackToLocal(parent.StreamCount);
            _outputCondition = parent.OutputConditionFactory.Make(agentInstanceContext, outputCallback);
            _deltaSet = resultSetProcessorHelperFactory.MakeOutputConditionChangeSet(isJoin, agentInstanceContext);
        }

        private static void AddToChangeset(
            IEnumerable<MultiKey<EventBean>> newEvents,
            IEnumerable<MultiKey<EventBean>> oldEvents,
            OutputProcessViewConditionDeltaSet joinEventsSet)
        {
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

            ISet<MultiKey<EventBean>> copyOld;
            if (oldEvents != null)
            {
                copyOld = new LinkedHashSet<MultiKey<EventBean>>(oldEvents);
            }
            else
            {
                copyOld = new LinkedHashSet<MultiKey<EventBean>>();
            }

            joinEventsSet.AddJoin(new UniformPair<ISet<MultiKey<EventBean>>>(copyNew, copyOld));
        }

        public override int NumChangesetRows => _deltaSet.NumChangesetRows;

        public override OutputProcessViewConditionDeltaSet OptionalDeltaSet => _deltaSet;

        public override OutputCondition OptionalOutputCondition => _outputCondition;

        public override OutputProcessViewAfterState OptionalAfterConditionState => null;

        /// <summary>
        /// The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">- new events</param>
        /// <param name="oldData">- old events</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QOutputProcessWCondition(newData, oldData);
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".update Received update, " +
                    "  newData.Length==" + ((newData == null) ? 0 : newData.Length) +
                    "  oldData.Length==" + ((oldData == null) ? 0 : oldData.Length));
            }

            // add the incoming events to the event batches
            if (_parent.HasAfter)
            {
                bool afterSatisfied = base.CheckAfterCondition(newData, _parent.StatementContext);
                if (!afterSatisfied)
                {
                    if (!_parent.IsUnaggregatedUngrouped)
                    {
                        _deltaSet.AddView(new UniformPair<EventBean[]>(newData, oldData));
                    }
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AOutputProcessWCondition(false);
                    }
                    return;
                }
                else
                {
                    _deltaSet.AddView(new UniformPair<EventBean[]>(newData, oldData));
                }
            }
            else
            {
                _deltaSet.AddView(new UniformPair<EventBean[]>(newData, oldData));
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

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QOutputRateConditionUpdate(newDataLength, oldDataLength);
            }
            _outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AOutputRateConditionUpdate();
            }

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AOutputProcessWCondition(true);
            }
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
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QOutputProcessWConditionJoin(newEvents, oldEvents);
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(
                    ".process Received update, " +
                    "  newData.Length==" + ((newEvents == null) ? 0 : newEvents.Count) +
                    "  oldData.Length==" + ((oldEvents == null) ? 0 : oldEvents.Count));
            }

            // add the incoming events to the event batches
            if (_parent.HasAfter)
            {
                bool afterSatisfied = base.CheckAfterCondition(newEvents, _parent.StatementContext);
                if (!afterSatisfied)
                {
                    if (!_parent.IsUnaggregatedUngrouped)
                    {
                        AddToChangeset(newEvents, oldEvents, _deltaSet);
                    }
                    if (InstrumentationHelper.ENABLED)
                    {
                        InstrumentationHelper.Get().AOutputProcessWConditionJoin(false);
                    }
                    return;
                }
                else
                {
                    AddToChangeset(newEvents, oldEvents, _deltaSet);
                }
            }
            else
            {
                AddToChangeset(newEvents, oldEvents, _deltaSet);
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

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QOutputRateConditionUpdate(newEventsSize, oldEventsSize);
            }
            _outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AOutputRateConditionUpdate();
            }

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AOutputProcessWConditionJoin(true);
            }
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
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QOutputRateConditionOutputNow();
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingView");
            }

            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;

            // Process the events and get the result
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessOutputLimitedView(
                _deltaSet.ViewEventsSet, isGenerateSynthetic, _parent.OutputLimitLimitType);

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
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AOutputRateConditionOutputNow(false);
                }
                return;
            }

            if (doOutput)
            {
                Output(forceUpdate, newOldEvents);
            }
            ResetEventBatches();

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AOutputRateConditionOutputNow(true);
            }
        }

        protected virtual void Output(bool forceUpdate, UniformPair<EventBean[]> results)
        {
            // Child view can be null in replay from named window
            if (ChildView != null)
            {
                OutputStrategyUtil.Output(forceUpdate, results, ChildView);
            }
        }

        public override void Stop()
        {
            base.Stop();
            _deltaSet.Destroy();
            _outputCondition.Stop();
        }

        private void ResetEventBatches()
        {
            _deltaSet.Clear();
        }

        /// <summary>
        /// Called once the output condition has been met.
        /// Invokes the result set processor.
        /// Used for join event data.
        /// </summary>
        /// <param name="doOutput">- true if the batched events should actually be output as well as processed, false if they should just be processed</param>
        /// <param name="forceUpdate">- true if output should be made even when no updating events have arrived</param>
        protected void ContinueOutputProcessingJoin(bool doOutput, bool forceUpdate)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QOutputRateConditionOutputNow();
            }

            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingJoin");
            }

            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;

            // Process the events and get the result
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessOutputLimitedJoin(
                _deltaSet.JoinEventsSet, isGenerateSynthetic, _parent.OutputLimitLimitType);

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
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().AOutputRateConditionOutputNow(false);
                }
                return;
            }

            if (doOutput)
            {
                Output(forceUpdate, newOldEvents);
            }
            ResetEventBatches();

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().AOutputRateConditionOutputNow(true);
            }
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

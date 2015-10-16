///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.spec;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Handles output rate limiting for FIRST, only applicable with a having-clause and no group-by clause.
    /// <para/> 
    /// Without having-clause the order of processing won't matter therefore its handled by the <seealso cref="OutputProcessViewConditionDefault" />. 
    /// With group-by the <seealso cref="ResultSetProcessor" /> handles the per-group first criteria.
    /// </summary>
    public class OutputProcessViewConditionFirst : OutputProcessViewBaseWAfter
    {
        private readonly OutputProcessViewConditionFactory _parent;
        private readonly OutputCondition _outputCondition;
    
        // Posted events in ordered form (for applying to aggregates) and summarized per type
        // Using ArrayList as random access is a requirement.
        private readonly List<UniformPair<EventBean[]>> _viewEventsList = new List<UniformPair<EventBean[]>>();
    	private readonly List<UniformPair<ISet<MultiKey<EventBean>>>> _joinEventsSet = new List<UniformPair<ISet<MultiKey<EventBean>>>>();
        private bool _witnessedFirst;
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public OutputProcessViewConditionFirst(ResultSetProcessor resultSetProcessor, long? afterConditionTime, int? afterConditionNumberOfEvents, bool afterConditionSatisfied, OutputProcessViewConditionFactory parent, AgentInstanceContext agentInstanceContext)
                    : base(resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied)
        {
            _parent = parent;
    
            OutputCallback outputCallback = GetCallbackToLocal(parent.StreamCount);
            _outputCondition = parent.OutputConditionFactory.Make(agentInstanceContext, outputCallback);
        }

        public override int NumChangesetRows
        {
            get { return Math.Max(_viewEventsList.Count, _joinEventsSet.Count); }
        }

        /// <summary>The Update method is called if the view does not participate in a join. </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".Update Received Update, " +
                        "  newData.Length==" + ((newData == null) ? 0 : newData.Length) +
                        "  oldData.Length==" + ((oldData == null) ? 0 : oldData.Length));
            }
    
            if (!CheckAfterCondition(newData, _parent.StatementContext))
            {
                return;
            }
    
            if (!_witnessedFirst) {
                var isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
    
                // Process the events and get the result
                _viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));
                UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessOutputLimitedView(_viewEventsList, isGenerateSynthetic, OutputLimitLimitType.FIRST);
                _viewEventsList.Clear();

                if (!HasRelevantResults(newOldEvents)) {
                    return;
                }
    
                _witnessedFirst = true;

                if (_parent.IsDistinct)
                {
                    newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.EventBeanReader);
                    newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, _parent.EventBeanReader);
                }
    
                var isGenerateNatural = _parent.StatementResultService.IsMakeNatural;
                if ((!isGenerateSynthetic) && (!isGenerateNatural))
                {
                    if (AuditPath.IsAuditEnabled) {
                        OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                    }
                    return;
                }
    
                Output(true, newOldEvents);
            }
            else {
                _viewEventsList.Add(new UniformPair<EventBean[]>(newData, oldData));
                ResultSetProcessor.ProcessOutputLimitedView(_viewEventsList, false, OutputLimitLimitType.FIRST);
                _viewEventsList.Clear();
            }
    
            int newDataLength = 0;
            int oldDataLength = 0;
            if(newData != null)
            {
            	newDataLength = newData.Length;
            }
            if(oldData != null)
            {
            	oldDataLength = oldData.Length;
            }
    
            _outputCondition.UpdateOutputCondition(newDataLength, oldDataLength);
        }

        /// <summary>
        /// This process (Update) method is for participation in a join.
        /// </summary>
        /// <param name="newEvents">new events</param>
        /// <param name="oldEvents">old events</param>
        /// <param name="exprEvaluatorContext">expression evaluation context</param>
        public override void Process(ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents, ExprEvaluatorContext exprEvaluatorContext)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".process Received Update, " +
                        "  newData.Length==" + ((newEvents == null) ? 0 : newEvents.Count) +
                        "  oldData.Length==" + ((oldEvents == null) ? 0 : oldEvents.Count));
            }
    
            if (!CheckAfterCondition(newEvents, _parent.StatementContext))
            {
                return;
            }
    
            // add the incoming events to the event batches
            if (!_witnessedFirst) {
                AddToChangeSet(_joinEventsSet, newEvents, oldEvents);
                var isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
    
                var newOldEvents = ResultSetProcessor.ProcessOutputLimitedJoin(_joinEventsSet, isGenerateSynthetic, OutputLimitLimitType.FIRST);
                _joinEventsSet.Clear();

                if (!HasRelevantResults(newOldEvents)) {
                    return;
                }
    
                _witnessedFirst = true;

                if (_parent.IsDistinct)
                {
                    newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.EventBeanReader);
                    newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, _parent.EventBeanReader);
                }
    
                bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;
                if ((!isGenerateSynthetic) && (!isGenerateNatural))
                {
                    if (AuditPath.IsAuditEnabled) {
                        OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                    }
                    return;
                }
    
                Output(true, newOldEvents);
            }
            else {
                AddToChangeSet(_joinEventsSet, newEvents, oldEvents);
    
                // Process the events and get the result
                ResultSetProcessor.ProcessOutputLimitedJoin(_joinEventsSet, false, OutputLimitLimitType.FIRST);
                _joinEventsSet.Clear();
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
    
            _outputCondition.UpdateOutputCondition(newEventsSize, oldEventsSize);
        }

        protected virtual void ContinueOutputProcessingView(bool doOutput, bool forceUpdate)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingView");
            }
            _witnessedFirst = false;
        }

        private void Output(bool forceUpdate, UniformPair<EventBean[]> results)
        {
            // Child view can be null in replay from named window
            if (ChildView != null)
            {
                OutputStrategyUtil.Output(forceUpdate, results, ChildView);
            }
        }

        /// <summary>Called once the output condition has been met. Invokes the result set processor. Used for non-join event data. </summary>
    	/// <param name="doOutput">true if the batched events should actually be output as well as processed, false if they should just be processed</param>
    	/// <param name="forceUpdate">true if output should be made even when no updating events have arrived</param>
    	protected void ContinueOutputProcessingJoin(bool doOutput, bool forceUpdate)
    	{
    		if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled))
            {
                Log.Debug(".continueOutputProcessingJoin");
            }
            _witnessedFirst = false;
    	}
    
        private OutputCallback GetCallbackToLocal(int streamCount)
        {
            // single stream means no join
            // multiple streams means a join
            if(streamCount == 1)
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
            return OutputStrategyUtil.GetEnumerator(JoinExecutionStrategy, ResultSetProcessor, Parent, _parent.IsDistinct);
        }
    
        public override void Terminated()
        {
            if (_parent.IsTerminable) {
                _outputCondition.Terminated();
            }
        }

        private bool HasRelevantResults(UniformPair<EventBean[]> newOldEvents)
        {
            if (newOldEvents == null)
            {
                return false;
            }
            if (_parent.SelectClauseStreamSelectorEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY)
            {
                if (newOldEvents.First == null)
                {
                    return false; // nothing to indicate
                }
            }
            else if (_parent.SelectClauseStreamSelectorEnum == SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH)
            {
                if (newOldEvents.First == null && newOldEvents.Second == null)
                {
                    return false; // nothing to indicate
                }
            }
            else
            {
                if (newOldEvents.Second == null)
                {
                    return false; // nothing to indicate
                }
            }
            return true;
        }

        private static void AddToChangeSet(IList<UniformPair<ISet<MultiKey<EventBean>>>> joinEventsSet, ISet<MultiKey<EventBean>> newEvents, ISet<MultiKey<EventBean>> oldEvents)
        {
            ISet<MultiKey<EventBean>> copyNew = newEvents != null ? new LinkedHashSet<MultiKey<EventBean>>(newEvents) : new LinkedHashSet<MultiKey<EventBean>>();
            ISet<MultiKey<EventBean>> copyOld = oldEvents != null ? new LinkedHashSet<MultiKey<EventBean>>(oldEvents) : new LinkedHashSet<MultiKey<EventBean>>();
            joinEventsSet.Add(new UniformPair<ISet<MultiKey<EventBean>>>(copyNew, copyOld));
        }
    }
}

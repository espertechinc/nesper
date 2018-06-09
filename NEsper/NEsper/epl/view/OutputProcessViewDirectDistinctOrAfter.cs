///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.logging;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.view
{
    /// <summary>
    /// Output process view that does not enforce any output policies and may simply
    /// hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfter : OutputProcessViewBaseWAfter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly OutputProcessViewDirectDistinctOrAfterFactory _parent;

        public OutputProcessViewDirectDistinctOrAfter(
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewDirectDistinctOrAfterFactory parent)
            : base(resultSetProcessorHelperFactory, agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents, afterConditionSatisfied)
        {
            _parent = parent;
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => null;

        public override OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        /// <summary>
        /// The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">- new events</param>
        /// <param name="oldData">- old events</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".update Received update, " +
                        "  newData.Length==" + ((newData == null) ? 0 : newData.Length) +
                        "  oldData.Length==" + ((oldData == null) ? 0 : oldData.Length));
            }
    
            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;
    
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessViewResult(newData, oldData, isGenerateSynthetic);
    
            if (!base.CheckAfterCondition(newOldEvents, _parent.StatementContext)) {
                return;
            }
    
            if (_parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, _parent.EventBeanReader);
            }
    
            if ((!isGenerateSynthetic) && (!isGenerateNatural)) {
                if (AuditPath.IsAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                }
                return;
            }
    
            bool forceOutput = false;
            if ((newData == null) && (oldData == null) &&
                ((newOldEvents == null) || (newOldEvents.First == null && newOldEvents.Second == null)))
            {
                forceOutput = true;
            }

            // Child view can be null in replay from named window
            if (ChildView != null) {
                PostProcess(forceOutput, newOldEvents, ChildView);
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
            if ((ExecutionPathDebugLog.IsEnabled) && (Log.IsDebugEnabled)) {
                Log.Debug(".process Received update, " +
                        "  newData.Length==" + ((newEvents == null) ? 0 : newEvents.Count) +
                        "  oldData.Length==" + ((oldEvents == null) ? 0 : oldEvents.Count));
            }
    
            bool isGenerateSynthetic = _parent.StatementResultService.IsMakeSynthetic;
            bool isGenerateNatural = _parent.StatementResultService.IsMakeNatural;
    
            UniformPair<EventBean[]> newOldEvents = ResultSetProcessor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);
    
            if (!CheckAfterCondition(newOldEvents, _parent.StatementContext)) {
                return;
            }
    
            if (_parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, _parent.EventBeanReader);
            }
    
            if ((!isGenerateSynthetic) && (!isGenerateNatural)) {
                if (AuditPath.IsAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(_parent.StatementContext, newOldEvents);
                }
                return;
            }
    
            if (newOldEvents == null) {
                return;
            }
    
            // Child view can be null in replay from named window
            if (ChildView != null) {
                PostProcess(false, newOldEvents, ChildView);
            }
        }
    
        protected virtual void PostProcess(bool force, UniformPair<EventBean[]> newOldEvents, UpdateDispatchView childView)
        {
            OutputStrategyUtil.Output(force, newOldEvents, childView);
        }
    
        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetEnumerator(JoinExecutionStrategy, ResultSetProcessor, ParentView, _parent.IsDistinct);
        }
    
        public override void Terminated()
        {
            // Not applicable
        }
    }
} // end of namespace

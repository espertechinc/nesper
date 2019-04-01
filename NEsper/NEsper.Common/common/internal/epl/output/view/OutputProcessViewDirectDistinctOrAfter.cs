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
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    ///     Output process view that does not enforce any output policies and may simply
    ///     hand over events to child views, but works with distinct and after-output policies
    /// </summary>
    public class OutputProcessViewDirectDistinctOrAfter : OutputProcessViewBaseWAfter
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(OutputProcessViewDirectDistinctOrAfter));

        private readonly OutputProcessViewDirectDistinctOrAfterFactory parent;

        public OutputProcessViewDirectDistinctOrAfter(
            AgentInstanceContext agentInstanceContext, ResultSetProcessor resultSetProcessor, long? afterConditionTime,
            int? afterConditionNumberOfEvents, bool afterConditionSatisfied,
            OutputProcessViewDirectDistinctOrAfterFactory parent) : base(
            agentInstanceContext, resultSetProcessor, afterConditionTime, afterConditionNumberOfEvents,
            afterConditionSatisfied)
        {
            this.parent = parent;
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => null;

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        /// <summary>
        ///     The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(EventBean[] newData, EventBean[] oldData)
        {
            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            var newOldEvents = resultSetProcessor.ProcessViewResult(newData, oldData, isGenerateSynthetic);

            if (!CheckAfterCondition(newOldEvents, agentInstanceContext.StatementContext)) {
                return;
            }

            if (parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, parent.EventBeanReader);
            }

            if (!isGenerateSynthetic && !isGenerateNatural) {
                if (AuditPath.isAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(agentInstanceContext.StatementContext, newOldEvents);
                }

                return;
            }

            var forceOutput = false;
            if (newData == null && oldData == null &&
                (newOldEvents == null || newOldEvents.First == null && newOldEvents.Second == null)) {
                forceOutput = true;
            }

            // Child view can be null in replay from named window
            if (child != null) {
                PostProcess(forceOutput, newOldEvents, child);
            }
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
            if (ExecutionPathDebugLog.IsEnabled && log.IsDebugEnabled) {
                log.Debug(
                    ".process Received update, " +
                    "  newData.length==" + (newEvents == null ? 0 : newEvents.Count) +
                    "  oldData.length==" + (oldEvents == null ? 0 : oldEvents.Count));
            }

            var statementResultService = agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            var newOldEvents = resultSetProcessor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);

            if (!CheckAfterCondition(newOldEvents, agentInstanceContext.StatementContext)) {
                return;
            }

            if (parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, parent.EventBeanReader);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(newOldEvents.Second, parent.EventBeanReader);
            }

            if (!isGenerateSynthetic && !isGenerateNatural) {
                if (AuditPath.isAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(agentInstanceContext.StatementContext, newOldEvents);
                }

                return;
            }

            if (newOldEvents == null) {
                return;
            }

            // Child view can be null in replay from named window
            if (child != null) {
                PostProcess(false, newOldEvents, child);
            }
        }

        protected virtual void PostProcess(
            bool force, UniformPair<EventBean[]> newOldEvents, UpdateDispatchView childView)
        {
            OutputStrategyUtil.Output(force, newOldEvents, childView);
        }

        public override IEnumerator<EventBean> GetEnumerator()
        {
            return OutputStrategyUtil.GetIterator(
                joinExecutionStrategy, resultSetProcessor, parentView, parent.IsDistinct);
        }

        public override void Terminated()
        {
            // Not applicable
        }
    }
} // end of namespace
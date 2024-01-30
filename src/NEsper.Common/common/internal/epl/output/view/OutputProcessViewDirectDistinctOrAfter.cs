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
        private static readonly ILog Log = LogManager.GetLogger(typeof(OutputProcessViewDirectDistinctOrAfter));

        private readonly OutputProcessViewDirectDistinctOrAfterFactory _parent;

        public OutputProcessViewDirectDistinctOrAfter(
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor,
            long? afterConditionTime,
            int? afterConditionNumberOfEvents,
            bool afterConditionSatisfied,
            OutputProcessViewDirectDistinctOrAfterFactory parent)
            : base(
                agentInstanceContext,
                resultSetProcessor,
                afterConditionTime,
                afterConditionNumberOfEvents,
                afterConditionSatisfied)
        {
            _parent = parent;
        }

        public override int NumChangesetRows => 0;

        public override OutputCondition OptionalOutputCondition => null;

        public OutputProcessViewConditionDeltaSet OptionalDeltaSet => null;

        /// <summary>
        ///     The update method is called if the view does not participate in a join.
        /// </summary>
        /// <param name="newData">new events</param>
        /// <param name="oldData">old events</param>
        public override void Update(
            EventBean[] newData,
            EventBean[] oldData)
        {
            var statementResultService = _agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            var newOldEvents = _resultSetProcessor.ProcessViewResult(newData, oldData, isGenerateSynthetic);

            if (!CheckAfterCondition(newOldEvents, _agentInstanceContext.StatementContext)) {
                return;
            }

            if (_parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.DistinctKeyGetter);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                    newOldEvents.Second,
                    _parent.DistinctKeyGetter);
            }

            if (!isGenerateSynthetic && !isGenerateNatural) {
                if (AuditPath.isAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(_agentInstanceContext.StatementContext, newOldEvents);
                }

                return;
            }

            var forceOutput = false;
            if (newData == null &&
                oldData == null &&
                (newOldEvents == null || (newOldEvents.First == null && newOldEvents.Second == null))) {
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

            var statementResultService = _agentInstanceContext.StatementResultService;
            var isGenerateSynthetic = statementResultService.IsMakeSynthetic;
            var isGenerateNatural = statementResultService.IsMakeNatural;

            var newOldEvents = _resultSetProcessor.ProcessJoinResult(newEvents, oldEvents, isGenerateSynthetic);

            if (!CheckAfterCondition(newOldEvents, _agentInstanceContext.StatementContext)) {
                return;
            }

            if (_parent.IsDistinct && newOldEvents != null) {
                newOldEvents.First = EventBeanUtility.GetDistinctByProp(newOldEvents.First, _parent.DistinctKeyGetter);
                newOldEvents.Second = EventBeanUtility.GetDistinctByProp(
                    newOldEvents.Second,
                    _parent.DistinctKeyGetter);
            }

            if (!isGenerateSynthetic && !isGenerateNatural) {
                if (AuditPath.isAuditEnabled) {
                    OutputStrategyUtil.IndicateEarlyReturn(_agentInstanceContext.StatementContext, newOldEvents);
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
            bool force,
            UniformPair<EventBean[]> newOldEvents,
            UpdateDispatchView childView)
        {
            OutputStrategyUtil.Output(force, newOldEvents, childView);
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
            // Not applicable
        }
    }
} // end of namespace
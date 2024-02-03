///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.statement.dispatch;


namespace com.espertech.esper.common.@internal.epl.output.view
{
    /// <summary>
    /// An output strategy that handles routing (insert-into) and stream selection.
    /// </summary>
    public class OutputStrategyPostProcess
    {
        private readonly OutputStrategyPostProcessFactory parent;
        private readonly AgentInstanceContext agentInstanceContext;
        private readonly TableInstance tableInstance;
        private readonly bool audit;

        public OutputStrategyPostProcess(
            OutputStrategyPostProcessFactory parent,
            AgentInstanceContext agentInstanceContext,
            TableInstance tableInstance)
        {
            this.parent = parent;
            this.agentInstanceContext = agentInstanceContext;
            this.tableInstance = tableInstance;
            audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.Annotations) != null;
        }

        public void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> result,
            UpdateDispatchView finalView)
        {
            var newEvents = result?.First;
            var oldEvents = result?.Second;

            // route first
            if (parent.IsRoute) {
                if (newEvents != null && parent.InsertIntoStreamSelector.IsSelectsIStream()) {
                    Route(newEvents, agentInstanceContext);
                }

                if (oldEvents != null && parent.InsertIntoStreamSelector.IsSelectsRStream()) {
                    Route(oldEvents, agentInstanceContext);
                }
            }

            // discard one side of results
            if (parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY) {
                newEvents = oldEvents;
                oldEvents = null;
            }
            else if (parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
                oldEvents = null; // since the insert-into may require rstream
            }

            // dispatch
            if (newEvents != null || oldEvents != null) {
                finalView.NewResult(new UniformPair<EventBean[]>(newEvents, oldEvents));
            }
            else if (forceUpdate) {
                finalView.NewResult(new UniformPair<EventBean[]>(null, null));
            }
        }

        private void Route(
            EventBean[] events,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            foreach (var routed in events) {
                if (routed is NaturalEventBean) {
                    var natural = (NaturalEventBean)routed;
                    Route(natural.OptionalSynthetic, exprEvaluatorContext);
                }
                else {
                    Route(routed, exprEvaluatorContext);
                }
            }
        }

        private void Route(
            EventBean routed,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (audit) {
                agentInstanceContext.AuditProvider.Insert(routed, agentInstanceContext);
            }

            if (tableInstance != null) {
                TableEvalLockUtil.ObtainLockUnless(tableInstance.TableLevelRWLock.WriteLock, exprEvaluatorContext);
                tableInstance.AddEventUnadorned(routed);
            }
            else {
                // Evaluate event precedence
                var precedence = ExprNodeUtilityEvaluate.EvaluateIntOptional(
                    parent.EventPrecedence,
                    routed,
                    0,
                    agentInstanceContext);
                agentInstanceContext.InternalEventRouter.Route(
                    routed,
                    agentInstanceContext,
                    parent.IsAddToFront,
                    precedence);
            }
        }
    }
} // end of namespace
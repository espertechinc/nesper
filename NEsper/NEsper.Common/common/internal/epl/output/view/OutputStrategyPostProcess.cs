///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
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
    ///     An output strategy that handles routing (insert-into) and stream selection.
    /// </summary>
    public class OutputStrategyPostProcess
    {
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly bool _audit;
        private readonly OutputStrategyPostProcessFactory _parent;
        private readonly TableInstance _tableInstance;

        public OutputStrategyPostProcess(
            OutputStrategyPostProcessFactory parent,
            AgentInstanceContext agentInstanceContext,
            TableInstance tableInstance)
        {
            _parent = parent;
            _agentInstanceContext = agentInstanceContext;
            _tableInstance = tableInstance;
            _audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.Annotations) != null;
        }

        public void Output(
            bool forceUpdate,
            UniformPair<EventBean[]> result,
            UpdateDispatchView finalView)
        {
            var newEvents = result != null ? result.First : null;
            var oldEvents = result != null ? result.Second : null;

            // route first
            if (_parent.IsRoute) {
                if (newEvents != null && _parent.InsertIntoStreamSelector.IsSelectsIStream) {
                    Route(newEvents, _agentInstanceContext);
                }

                if (oldEvents != null && _parent.InsertIntoStreamSelector.IsSelectsRStream) {
                    Route(oldEvents, _agentInstanceContext);
                }
            }

            // discard one side of results
            if (_parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.RSTREAM_ONLY) {
                newEvents = oldEvents;
                oldEvents = null;
            }
            else if (_parent.SelectStreamDirEnum == SelectClauseStreamSelectorEnum.ISTREAM_ONLY) {
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
                    var natural = (NaturalEventBean) routed;
                    if (_audit) {
                        _agentInstanceContext.AuditProvider.Insert(natural.OptionalSynthetic, _agentInstanceContext);
                    }

                    if (_tableInstance != null) {
                        TableEvalLockUtil.ObtainLockUnless(
                            _tableInstance.TableLevelRWLock.WriteLock,
                            exprEvaluatorContext);
                        _tableInstance.AddEventUnadorned(natural.OptionalSynthetic);
                    }
                    else {
                        _agentInstanceContext.InternalEventRouter.Route(
                            natural.OptionalSynthetic,
                            _agentInstanceContext,
                            _parent.IsAddToFront);
                    }
                }
                else {
                    if (_audit) {
                        _agentInstanceContext.AuditProvider.Insert(routed, _agentInstanceContext);
                    }

                    if (_tableInstance != null) {
                        TableEvalLockUtil.ObtainLockUnless(
                            _tableInstance.TableLevelRWLock.WriteLock,
                            exprEvaluatorContext);
                        _tableInstance.AddEventUnadorned(routed);
                    }
                    else {
                        _agentInstanceContext.InternalEventRouter.Route(
                            routed,
                            _agentInstanceContext,
                            _parent.IsAddToFront);
                    }
                }
            }
        }
    }
} // end of namespace
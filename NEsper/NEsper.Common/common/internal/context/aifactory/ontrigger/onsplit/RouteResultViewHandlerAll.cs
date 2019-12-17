///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    /// <summary>
    ///     Handler for split-stream evaluating the all where-clauses and their matching select-clauses.
    /// </summary>
    public class RouteResultViewHandlerAll : RouteResultViewHandlerBase
    {
        public RouteResultViewHandlerAll(
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableInstance[] tableStateInstances,
            OnSplitItemEval[] items,
            ResultSetProcessor[] processors,
            AgentInstanceContext agentInstanceContext)
            : base(
                epStatementHandle,
                internalEventRouter,
                tableStateInstances,
                items,
                processors,
                agentInstanceContext)
        {
        }

        public override bool Handle(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QSplitStream(true, theEvent, items.Length);

            var isHandled = false;
            for (var i = 0; i < items.Length; i++) {
                var currentItem = items[i];
                eventsPerStream[0] = theEvent;

                // handle no-contained-event evaluation
                if (currentItem.PropertyEvaluator == null) {
                    isHandled |= ProcessAllCurrentEvent(i, exprEvaluatorContext);
                }
                else {
                    // handle contained-event evaluation
                    var containeds = currentItem.PropertyEvaluator.GetProperty(
                        eventsPerStream[0],
                        exprEvaluatorContext);
                    if (containeds == null || containeds.Length == 0) {
                        continue;
                    }

                    foreach (var contained in containeds) {
                        eventsPerStream[0] = contained;
                        isHandled |= ProcessAllCurrentEvent(i, exprEvaluatorContext);
                    }
                }
            }

            instrumentationCommon.ASplitStream(true, isHandled);
            return isHandled;
        }

        private bool ProcessAllCurrentEvent(
            int index,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var pass = CheckWhereClauseCurrentEvent(index, exprEvaluatorContext);
            if (!pass) {
                return false;
            }

            return MayRouteCurrentEvent(index, exprEvaluatorContext);
        }
    }
} // end of namespace
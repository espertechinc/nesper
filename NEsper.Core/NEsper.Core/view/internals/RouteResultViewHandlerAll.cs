///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Handler for split-stream evaluating the all where-clauses and their matching select-clauses.
    /// </summary>
    public class RouteResultViewHandlerAll : RouteResultViewHandlerBase
    {
        public RouteResultViewHandlerAll(
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableStateInstance[] tableStateInstances,
            EPStatementStartMethodOnTriggerItem[] items,
            ResultSetProcessor[] processors,
            ExprEvaluator[] whereClauses,
            AgentInstanceContext agentInstanceContext)
            : base(
                epStatementHandle, internalEventRouter, tableStateInstances, items, processors, whereClauses,
                agentInstanceContext)
        {
        }

        public override bool Handle(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QSplitStream(true, theEvent, WhereClauses);
            }

            bool isHandled = false;
            for (int i = 0; i < WhereClauses.Length; i++)
            {
                EPStatementStartMethodOnTriggerItem currentItem = Items[i];
                EventsPerStream[0] = theEvent;

                // handle no-contained-event evaluation
                if (currentItem.PropertyEvaluator == null)
                {
                    isHandled |= ProcessAllCurrentEvent(i, exprEvaluatorContext);
                }
                else
                {
                    // handle contained-event evaluation
                    EventBean[] containeds = currentItem.PropertyEvaluator.GetProperty(
                        EventsPerStream[0], exprEvaluatorContext);
                    if (containeds == null || containeds.Length == 0)
                    {
                        continue;
                    }

                    foreach (EventBean contained in containeds)
                    {
                        EventsPerStream[0] = contained;
                        isHandled |= ProcessAllCurrentEvent(i, exprEvaluatorContext);
                    }
                }
            }

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().ASplitStream(true, isHandled);
            }
            return isHandled;
        }

        private bool ProcessAllCurrentEvent(int index, ExprEvaluatorContext exprEvaluatorContext)
        {
            bool pass = CheckWhereClauseCurrentEvent(index, exprEvaluatorContext);
            if (!pass)
            {
                return false;
            }
            return MayRouteCurrentEvent(index, exprEvaluatorContext);
        }
    }
} // end of namespace

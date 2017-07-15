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
    /// Handler for split-stream evaluating the first where-clause matching select-clause.
    /// </summary>
    public class RouteResultViewHandlerFirst : RouteResultViewHandlerBase
    {
        public RouteResultViewHandlerFirst(
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
                InstrumentationHelper.Get().QSplitStream(false, theEvent, WhereClauses);
            }

            int index = -1;

            for (int i = 0; i < WhereClauses.Length; i++)
            {
                EPStatementStartMethodOnTriggerItem item = Items[i];
                EventsPerStream[0] = theEvent;

                // handle no contained-event evaluation
                if (item.PropertyEvaluator == null)
                {
                    bool pass = CheckWhereClauseCurrentEvent(i, exprEvaluatorContext);
                    if (pass)
                    {
                        index = i;
                        break;
                    }
                }
                else
                {
                    // need to get contained events first
                    EventBean[] containeds = Items[i].PropertyEvaluator.GetProperty(
                        EventsPerStream[0], exprEvaluatorContext);
                    if (containeds == null || containeds.Length == 0)
                    {
                        continue;
                    }

                    foreach (EventBean contained in containeds)
                    {
                        EventsPerStream[0] = contained;
                        bool pass = CheckWhereClauseCurrentEvent(i, exprEvaluatorContext);
                        if (pass)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1)
                    {
                        break;
                    }
                }
            }

            if (index != -1)
            {
                MayRouteCurrentEvent(index, exprEvaluatorContext);
            }

            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().ASplitStream(false, index != -1);
            }
            return index != -1;
        }
    }
} // end of namespace

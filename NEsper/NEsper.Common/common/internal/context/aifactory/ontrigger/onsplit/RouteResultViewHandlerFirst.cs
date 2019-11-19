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
using com.espertech.esper.common.@internal.metrics.instrumentation;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    /// <summary>
    /// Handler for split-stream evaluating the first where-clause matching select-clause.
    /// </summary>
    public class RouteResultViewHandlerFirst : RouteResultViewHandlerBase
    {
        public RouteResultViewHandlerFirst(
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableInstance[] tableInstances,
            OnSplitItemEval[] items,
            ResultSetProcessor[] processors,
            AgentInstanceContext agentInstanceContext)
            : base(epStatementHandle, internalEventRouter, tableInstances, items, processors, agentInstanceContext)
        {
        }

        public override bool Handle(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            InstrumentationCommon instrumentationCommon = agentInstanceContext.InstrumentationProvider;
            instrumentationCommon.QSplitStream(false, theEvent, items.Length);

            int index = -1;

            for (int i = 0; i < items.Length; i++) {
                OnSplitItemEval item = items[i];
                eventsPerStream[0] = theEvent;

                // handle no contained-event evaluation
                if (item.PropertyEvaluator == null) {
                    bool pass = CheckWhereClauseCurrentEvent(i, exprEvaluatorContext);
                    if (pass) {
                        index = i;
                        break;
                    }
                }
                else {
                    // need to get contained events first
                    EventBean[] containeds =
                        items[i].PropertyEvaluator.GetProperty(eventsPerStream[0], exprEvaluatorContext);
                    if (containeds == null || containeds.Length == 0) {
                        continue;
                    }

                    foreach (EventBean contained in containeds) {
                        eventsPerStream[0] = contained;
                        bool pass = CheckWhereClauseCurrentEvent(i, exprEvaluatorContext);
                        if (pass) {
                            index = i;
                            break;
                        }
                    }

                    if (index != -1) {
                        break;
                    }
                }
            }

            if (index != -1) {
                MayRouteCurrentEvent(index, exprEvaluatorContext);
            }

            bool handled = index != -1;
            instrumentationCommon.ASplitStream(false, handled);
            return handled;
        }
    }
} // end of namespace
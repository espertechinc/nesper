///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;

namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    public abstract class RouteResultViewHandlerBase : RouteResultViewHandler
    {
        internal readonly AgentInstanceContext agentInstanceContext;
        internal readonly bool audit;
        internal readonly EPStatementHandle epStatementHandle;
        internal readonly EventBean[] eventsPerStream = new EventBean[1];
        internal readonly InternalEventRouter internalEventRouter;
        internal readonly OnSplitItemEval[] items;
        internal readonly ResultSetProcessor[] processors;
        private readonly TableInstance[] tableStateInstances;

        public RouteResultViewHandlerBase(
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableInstance[] tableStateInstances,
            OnSplitItemEval[] items,
            ResultSetProcessor[] processors,
            AgentInstanceContext agentInstanceContext)
        {
            this.internalEventRouter = internalEventRouter;
            this.tableStateInstances = tableStateInstances;
            this.items = items;
            this.epStatementHandle = epStatementHandle;
            this.processors = processors;
            this.agentInstanceContext = agentInstanceContext;
            audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.Annotations) != null;
        }

        public abstract bool Handle(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext);

        internal bool CheckWhereClauseCurrentEvent(
            int index,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var pass = true;

            var itemWhereClause = items[index].WhereClause;
            if (itemWhereClause != null) {
                agentInstanceContext.InstrumentationProvider.QSplitStreamWhere(index);
                var passEvent = itemWhereClause.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if (passEvent == null || false.Equals(passEvent)) {
                    pass = false;
                }

                agentInstanceContext.InstrumentationProvider.ASplitStreamWhere(pass);
            }

            return pass;
        }

        internal bool MayRouteCurrentEvent(
            int index,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            agentInstanceContext.InstrumentationProvider.QSplitStreamRoute(index);
            var result = processors[index].ProcessViewResult(eventsPerStream, null, false);
            var routed = false;
            if (result != null && result.First != null && result.First.Length > 0) {
                Route(result.First[0], index, exprEvaluatorContext);
                routed = true;
            }

            agentInstanceContext.InstrumentationProvider.ASplitStreamRoute();
            return routed;
        }

        internal void Route(
            EventBean routed,
            int index,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            if (audit) {
                exprEvaluatorContext.AuditProvider.Insert(routed, exprEvaluatorContext);
            }

            var tableStateInstance = tableStateInstances[index];
            if (tableStateInstance != null) {
                tableStateInstance.AddEventUnadorned(routed);
            }
            else {
                var isNamedWindowInsert = items[index].IsNamedWindowInsert;
                agentInstanceContext.InternalEventRouter.Route(routed, agentInstanceContext, isNamedWindowInsert);
            }
        }
    }
} // end of namespace
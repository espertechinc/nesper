///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.table.core;


namespace com.espertech.esper.common.@internal.context.aifactory.ontrigger.onsplit
{
    public abstract class RouteResultViewHandlerBase : RouteResultViewHandler
    {
        protected readonly InternalEventRouter internalEventRouter;
        private readonly TableInstance[] tableStateInstances;
        protected readonly OnSplitItemEval[] items;
        protected readonly EPStatementHandle epStatementHandle;
        protected readonly ResultSetProcessor[] processors;
        protected readonly EventBean[] eventsPerStream = new EventBean[1];
        protected readonly AgentInstanceContext agentInstanceContext;
        protected readonly bool audit;

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

        internal bool CheckWhereClauseCurrentEvent(
            int index,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var pass = true;

            var itemWhereClause = items[index].WhereClause;
            if (itemWhereClause != null) {
                agentInstanceContext.InstrumentationProvider.QSplitStreamWhere(index);
                var passEvent = (bool)itemWhereClause.Evaluate(eventsPerStream, true, exprEvaluatorContext);
                if (passEvent == null || !passEvent) {
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
            var eval = items[index];
            var result = processors[index].ProcessViewResult(eventsPerStream, null, false);
            var routed = false;
            if (result != null && result.First != null && result.First.Length > 0) {
                var routedEvent = result.First[0];

                // Evaluate event precedence
                var precedence = ExprNodeUtilityEvaluate.EvaluateIntOptional(
                    eval.EventPrecedence,
                    routedEvent,
                    0,
                    exprEvaluatorContext);

                Route(routedEvent, index, exprEvaluatorContext, precedence);
                routed = true;
            }

            agentInstanceContext.InstrumentationProvider.ASplitStreamRoute();
            return routed;
        }

        private void Route(
            EventBean routed,
            int index,
            ExprEvaluatorContext exprEvaluatorContext,
            int priority)
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
                agentInstanceContext.InternalEventRouter.Route(
                    routed,
                    agentInstanceContext,
                    isNamedWindowInsert,
                    priority);
            }
        }

        public abstract bool Handle(
            EventBean theEvent,
            ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace
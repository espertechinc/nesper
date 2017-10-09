///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.collection;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.core.start;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.internals
{
    public abstract class RouteResultViewHandlerBase : RouteResultViewHandler
    {
        protected readonly InternalEventRouter InternalEventRouter;
        protected readonly EPStatementStartMethodOnTriggerItem[] Items;
        protected readonly EPStatementHandle EPStatementHandle;
        protected readonly ResultSetProcessor[] Processors;
        protected readonly ExprEvaluator[] WhereClauses;
        protected readonly EventBean[] EventsPerStream = new EventBean[1];
        protected readonly AgentInstanceContext AgentInstanceContext;
        protected readonly bool Audit;
        private readonly TableStateInstance[] _tableStateInstances;

        protected RouteResultViewHandlerBase(
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableStateInstance[] tableStateInstances,
            EPStatementStartMethodOnTriggerItem[] items,
            ResultSetProcessor[] processors,
            ExprEvaluator[] whereClauses,
            AgentInstanceContext agentInstanceContext)
        {
            InternalEventRouter = internalEventRouter;
            _tableStateInstances = tableStateInstances;
            Items = items;
            EPStatementHandle = epStatementHandle;
            Processors = processors;
            WhereClauses = whereClauses;
            AgentInstanceContext = agentInstanceContext;
            Audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.StatementContext.Annotations) != null;
        }

        protected bool CheckWhereClauseCurrentEvent(int index, ExprEvaluatorContext exprEvaluatorContext)
        {
            bool pass = true;

            if (WhereClauses[index] != null)
            {
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().QSplitStreamWhere(index);
                }
                
                var passEvent = WhereClauses[index].Evaluate(new EvaluateParams(EventsPerStream, true, exprEvaluatorContext));
                if ((passEvent == null) || (false.Equals(passEvent)))
                {
                    pass = false;
                }
                if (InstrumentationHelper.ENABLED)
                {
                    InstrumentationHelper.Get().ASplitStreamWhere(pass);
                }
            }

            return pass;
        }

        protected bool MayRouteCurrentEvent(int index, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QSplitStreamRoute(index);
            }
            UniformPair<EventBean[]> result = Processors[index].ProcessViewResult(EventsPerStream, null, false);
            bool routed = false;
            if ((result != null) && (result.First != null) && (result.First.Length > 0))
            {
                Route(result.First[0], index, exprEvaluatorContext);
                routed = true;
            }
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().ASplitStreamRoute();
            }
            return routed;
        }

        private void Route(EventBean routed, int index, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (Audit)
            {
                AuditPath.AuditInsertInto(AgentInstanceContext.EngineURI, AgentInstanceContext.StatementName, routed);
            }
            TableStateInstance tableStateInstance = _tableStateInstances[index];
            if (tableStateInstance != null)
            {
                tableStateInstance.AddEventUnadorned(routed);
            }
            else
            {
                bool isNamedWindowInsert = Items[index].IsNamedWindowInsert;
                InternalEventRouter.Route(
                    routed, EPStatementHandle, AgentInstanceContext.StatementContext.InternalEventEngineRouteDest,
                    exprEvaluatorContext, isNamedWindowInsert);
            }
        }

        public abstract bool Handle(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext);
    }
} // end of namespace

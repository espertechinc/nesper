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
using com.espertech.esper.compat;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Handler for split-stream evaluating the all where-clauses and their matching select-clauses.
    /// </summary>
    public class RouteResultViewHandlerAll : RouteResultViewHandler
    {
        private readonly InternalEventRouter _internalEventRouter;
        private readonly TableStateInstance[] _tableStateInstances;
        private readonly bool[] _isNamedWindowInsert;
        private readonly EPStatementHandle _epStatementHandle;
        private readonly ResultSetProcessor[] _processors;
        private readonly ExprEvaluator[] _whereClauses;
        private readonly EventBean[] _eventsPerStream = new EventBean[1];
        private readonly AgentInstanceContext _agentInstanceContext;
        private readonly bool _audit;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="epStatementHandle">handle</param>
        /// <param name="internalEventRouter">routes generated events</param>
        /// <param name="tableStateInstances">The table state instances.</param>
        /// <param name="isNamedWindowInsert">The is named window insert.</param>
        /// <param name="processors">select clauses</param>
        /// <param name="whereClauses">where clauses</param>
        /// <param name="agentInstanceContext">agent instance context</param>
        public RouteResultViewHandlerAll(
            EPStatementHandle epStatementHandle,
            InternalEventRouter internalEventRouter,
            TableStateInstance[] tableStateInstances,
            bool[] isNamedWindowInsert,
            ResultSetProcessor[] processors,
            ExprEvaluator[] whereClauses,
            AgentInstanceContext agentInstanceContext)
        {
            _internalEventRouter = internalEventRouter;
            _tableStateInstances = tableStateInstances;
            _isNamedWindowInsert = isNamedWindowInsert;
            _epStatementHandle = epStatementHandle;
            _processors = processors;
            _whereClauses = whereClauses;
            _agentInstanceContext = agentInstanceContext;
            _audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.StatementContext.Annotations) != null;
        }
    
        public bool Handle(EventBean theEvent, ExprEvaluatorContext exprEvaluatorContext)
        {
            var isHandled = new Mutable<bool>(false);

            using (Instrument.With(
                i => i.QSplitStream(true, theEvent, _whereClauses),
                i => i.ASplitStream(true, isHandled.Value)))
            {
                _eventsPerStream[0] = theEvent;

                for (int ii = 0; ii < _whereClauses.Length; ii++)
                {
                    var pass = new Mutable<bool>(true);
                    if (_whereClauses[ii] != null)
                    {
                        Instrument.With(
                            i => i.QSplitStreamWhere(ii),
                            i => i.ASplitStreamWhere(pass.Value),
                            () =>
                            {
                                var passEvent = (bool?) _whereClauses[ii].Evaluate(new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext));
                                if ((passEvent == null) || (!passEvent.Value))
                                {
                                    pass.Value = false;
                                }
                            });
                    }

                    if (pass.Value)
                    {
                        Instrument.With(
                            i => i.QSplitStreamRoute(ii),
                            i => i.ASplitStreamRoute(),
                            () =>
                            {
                                UniformPair<EventBean[]> result = _processors[ii].ProcessViewResult(
                                    _eventsPerStream, null, false);
                                if ((result != null) && (result.First != null) && (result.First.Length > 0))
                                {
                                    isHandled.Value = true;
                                    EventBean eventRouted = result.First[0];
                                    if (_audit)
                                    {
                                        AuditPath.AuditInsertInto(
                                            _agentInstanceContext.EngineURI, _agentInstanceContext.StatementName,
                                            eventRouted);
                                    }
                                    if (_tableStateInstances[ii] != null)
                                    {
                                        _tableStateInstances[ii].AddEventUnadorned(eventRouted);
                                    }
                                    else
                                    {
                                        _internalEventRouter.Route(
                                            eventRouted, _epStatementHandle, 
                                            _agentInstanceContext.StatementContext.InternalEventEngineRouteDest,
                                            exprEvaluatorContext, _isNamedWindowInsert[ii]);
                                    }
                                }
                            });
                    }
                }

                return isHandled.Value;
            }
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.view.internals
{
    /// <summary>
    /// Handler for split-stream evaluating the first where-clause matching select-clause.
    /// </summary>
    public class RouteResultViewHandlerFirst : RouteResultViewHandler
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
        /// <param name="tableStateInstances"></param>
        /// <param name="isNamedWindowInsert">The is named window insert.</param>
        /// <param name="processors">select clauses</param>
        /// <param name="whereClauses">where clauses</param>
        /// <param name="agentInstanceContext">agent instance context</param>
        public RouteResultViewHandlerFirst(EPStatementHandle epStatementHandle, InternalEventRouter internalEventRouter, TableStateInstance[] tableStateInstances, bool[] isNamedWindowInsert, ResultSetProcessor[] processors, ExprEvaluator[] whereClauses, AgentInstanceContext agentInstanceContext)
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
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QSplitStream(false, theEvent, _whereClauses);}
    
            int index = -1;
            _eventsPerStream[0] = theEvent;
    
            for (int i = 0; i < _whereClauses.Length; i++)
            {
                if (_whereClauses[i] == null)
                {
                    index = i;
                    break;
                }
    
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QSplitStreamWhere(i);}
                var pass = (bool?) _whereClauses[i].Evaluate(new EvaluateParams(_eventsPerStream, true, exprEvaluatorContext));
                if ((pass != null) && (pass.Value))
                {
                    index = i;
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASplitStreamWhere(pass.Value);}
                    break;
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASplitStreamWhere(pass.Value); }
            }
    
            if (index != -1)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QSplitStreamRoute(index);}
                UniformPair<EventBean[]> result = _processors[index].ProcessViewResult(_eventsPerStream, null, false);
                if ((result != null) && (result.First != null) && (result.First.Length > 0))
                {
                    if (_audit) {
                        AuditPath.AuditInsertInto(_agentInstanceContext.EngineURI, _agentInstanceContext.StatementName, result.First[0]);
                    }
                    if (_tableStateInstances[index] != null)
                    {
                        _tableStateInstances[index].AddEventUnadorned(result.First[0]);
                    }
                    else
                    {
                        _internalEventRouter.Route(result.First[0], _epStatementHandle, _agentInstanceContext.StatementContext.InternalEventEngineRouteDest, _agentInstanceContext, _isNamedWindowInsert[index]);
                    }
                }
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASplitStreamRoute();}
            }
    
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().ASplitStream(false, index != -1);}
            return index != -1;
        }
    }
}

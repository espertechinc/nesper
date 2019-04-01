///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.named
{
    public class NamedWindowOnMergeActionIns : NamedWindowOnMergeAction
    {
        private readonly SelectExprProcessor _insertHelper;
        private readonly InternalEventRouter _internalEventRouter;
        private readonly String _insertIntoTableName;
        private readonly TableService _tableService;
        private readonly EPStatementHandle _statementHandle;
        private readonly InternalEventRouteDest _internalEventRouteDest;
        private readonly bool _audit;

        public NamedWindowOnMergeActionIns(
            ExprEvaluator optionalFilter,
            SelectExprProcessor insertHelper,
            InternalEventRouter internalEventRouter,
            String insertIntoTableName, 
            TableService tableService,
            EPStatementHandle statementHandle,
            InternalEventRouteDest internalEventRouteDest,
            bool audit)
            : base(optionalFilter)
        {
            _insertHelper = insertHelper;
            _internalEventRouter = internalEventRouter;
            _insertIntoTableName = insertIntoTableName;
            _tableService = tableService;
            _statementHandle = statementHandle;
            _internalEventRouteDest = internalEventRouteDest;
            _audit = audit;
        }
    
        public override void Apply(EventBean matchingEvent, EventBean[] eventsPerStream, OneEventCollection newData, OneEventCollection oldData, ExprEvaluatorContext exprEvaluatorContext)
        {
            EventBean theEvent = _insertHelper.Process(eventsPerStream, true, true, exprEvaluatorContext);

            if (_insertIntoTableName != null)
            {
                TableStateInstance tableStateInstance = _tableService.GetState(_insertIntoTableName, exprEvaluatorContext.AgentInstanceId);
                if (_audit)
                {
                    AuditPath.AuditInsertInto(tableStateInstance.AgentInstanceContext.EngineURI, _statementHandle.StatementName, theEvent);
                }
                tableStateInstance.AddEventUnadorned(theEvent);
                return;
            }

            if (_internalEventRouter == null) {
                newData.Add(theEvent);
                return;
            }
    
            if (_audit) {
                AuditPath.AuditInsertInto(_internalEventRouteDest.EngineURI, _statementHandle.StatementName, theEvent);
            }
            _internalEventRouter.Route(theEvent, _statementHandle, _internalEventRouteDest, exprEvaluatorContext, false);
        }
    
        public override String GetName()
        {
            return _internalEventRouter != null ? "insert-into" : "select";
        }
    }
}

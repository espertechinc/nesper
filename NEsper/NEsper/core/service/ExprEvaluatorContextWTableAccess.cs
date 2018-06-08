///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    public class ExprEvaluatorContextWTableAccess : ExprEvaluatorContext
    {
        private readonly ExprEvaluatorContext _context;
        private readonly TableService _tableService;

        public ExprEvaluatorContextWTableAccess(ExprEvaluatorContext context, TableService tableService)
        {
            _context = context;
            _tableService = tableService;
        }

        public IContainer Container => _context.Container;

        public string StatementName
        {
            get { return _context.StatementName; }
        }

        public string EngineURI
        {
            get { return _context.EngineURI; }
        }

        public int StatementId
        {
            get { return _context.StatementId; }
        }

        public StatementType? StatementType
        {
            get { return _context.StatementType; }
        }

        public TimeProvider TimeProvider
        {
            get { return _context.TimeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return _context.ExpressionResultCacheService; }
        }

        public int AgentInstanceId
        {
            get { return _context.AgentInstanceId; }
        }

        public EventBean ContextProperties
        {
            get { return _context.ContextProperties; }
        }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext
        {
            get { return _context.AllocateAgentInstanceScriptContext; }
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return _context.AgentInstanceLock; }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { return _tableService.TableExprEvaluatorContext; }
        }

        public object StatementUserObject
        {
            get { return _context.StatementUserObject; }
        }
    }
}

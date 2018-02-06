///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Represents a statement-level-only context for expression evaluation, not allowing for agents instances and result cache.
    /// </summary>
    public class ExprEvaluatorContextStatement : ExprEvaluatorContext
    {
        private readonly StatementContext _statementContext;
        private readonly bool _allowTableAccess;
        private EventBean _contextProperties;

        public ExprEvaluatorContextStatement(StatementContext statementContext, bool allowTableAccess)
        {
            _statementContext = statementContext;
            _allowTableAccess = allowTableAccess;
        }

        public IContainer Container => _statementContext.Container;

        /// <summary>Returns the time provider. </summary>
        /// <value>time provider</value>
        public TimeProvider TimeProvider
        {
            get { return _statementContext.TimeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return _statementContext.ExpressionResultCacheServiceSharable; }
        }

        public int AgentInstanceId
        {
            get { return -1; }
        }

        public EventBean ContextProperties
        {
            get { return _contextProperties; }
            set { _contextProperties = value; }
        }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext
        {
            get { return _statementContext.AllocateAgentInstanceScriptContext; }
        }

        public String StatementName
        {
            get { return _statementContext.StatementName; }
        }

        public String EngineURI
        {
            get { return _statementContext.EngineURI; }
        }

        public int StatementId
        {
            get { return _statementContext.StatementId; }
        }

        public StatementType? StatementType
        {
            get { return _statementContext.StatementType; }
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return _statementContext.DefaultAgentInstanceLock; }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get
            {
                if (!_allowTableAccess)
                {
                    throw new EPException("Access to tables is not allowed");
                }
                return _statementContext.TableExprEvaluatorContext;
            }
        }

        public object StatementUserObject
        {
            get { return _statementContext.StatementUserObject; }
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents a minimal enginel-level context for expression evaluation, not allowing for agents instances and result cache.
    /// </summary>
    public class ExprEvaluatorContextTimeOnly : ExprEvaluatorContext
    {
        private readonly TimeProvider _timeProvider;
        private readonly ExpressionResultCacheService _expressionResultCacheService;
    
        public ExprEvaluatorContextTimeOnly(TimeProvider timeProvider) {
            _timeProvider = timeProvider;
            _expressionResultCacheService = new ExpressionResultCacheServiceThreadlocal();
        }
    
        /// <summary>Returns the time provider. </summary>
        /// <value>time provider</value>
        public TimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public ExpressionResultCacheService ExpressionResultCacheService
        {
            get { return _expressionResultCacheService; }
        }

        public int AgentInstanceId
        {
            get { return -1; }
        }

        public EventBean ContextProperties
        {
            get { return null; }
        }

        public AgentInstanceScriptContext AgentInstanceScriptContext
        {
            get { return null; }
        }

        public String StatementName
        {
            get { return null; }
        }

        public String EngineURI
        {
            get { return null; }
        }

        public String StatementId
        {
            get { return null; }
        }

        public IReaderWriterLock AgentInstanceLock
        {
            get { return null; }
        }

        public StatementType? StatementType
        {
            get { return null; }
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext
        {
            get { throw new EPException("Access to tables is not allowed"); }
        }

        public object StatementUserObject
        {
            get { return null; }
        }
    }
}

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
using com.espertech.esper.core.service;
using com.espertech.esper.epl.script;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.schedule;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents a minimal engine-level context for expression evaluation, not allowing for agents instances and result cache.
    /// </summary>
    public class ExprEvaluatorContextTimeOnly : ExprEvaluatorContext
    {
        private readonly TimeProvider _timeProvider;
        private readonly ExpressionResultCacheService _expressionResultCacheService;
        private readonly IContainer _container;

        public ExprEvaluatorContextTimeOnly(IContainer container, TimeProvider timeProvider)
        {
            _container = container;
            _timeProvider = timeProvider;
            _expressionResultCacheService = new ExpressionResultCacheService(1, _container.Resolve<IThreadLocalManager>());
        }

        public IContainer Container => _container;

        /// <summary>Returns the time provider. </summary>
        /// <value>time provider</value>
        public TimeProvider TimeProvider => _timeProvider;

        public ExpressionResultCacheService ExpressionResultCacheService => _expressionResultCacheService;

        public int AgentInstanceId => -1;

        public EventBean ContextProperties => null;

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext => null;

        public String StatementName => null;

        public String EngineURI => null;

        public int StatementId => -1;

        public IReaderWriterLock AgentInstanceLock => null;

        public StatementType? StatementType => null;

        public TableExprEvaluatorContext TableExprEvaluatorContext => throw new EPException("Access to tables is not allowed");

        public object StatementUserObject => null;
    }
}

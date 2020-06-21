///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.context.util
{
    public class ExprEvaluatorContextWTableAccess : ExprEvaluatorContext
    {
        private readonly ExprEvaluatorContext context;
        private readonly TableExprEvaluatorContext tableExprEvaluatorContext;

        public ExprEvaluatorContextWTableAccess(
            ExprEvaluatorContext context,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            this.context = context;
            this.tableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public string StatementName => context.StatementName;

        public string RuntimeURI => context.RuntimeURI;

        public int StatementId => context.StatementId;

        public string DeploymentId => context.DeploymentId;

        public TimeProvider TimeProvider => context.TimeProvider;

        public ExpressionResultCacheService ExpressionResultCacheService => context.ExpressionResultCacheService;

        public int AgentInstanceId => context.AgentInstanceId;

        public EventBean ContextProperties => context.ContextProperties;

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext => context.AllocateAgentInstanceScriptContext;

        public IReaderWriterLock AgentInstanceLock => context.AgentInstanceLock;

        public TableExprEvaluatorContext TableExprEvaluatorContext => tableExprEvaluatorContext;

        public object UserObjectCompileTime => context.UserObjectCompileTime;

        public EventBeanService EventBeanService => context.EventBeanService;

        public AuditProvider AuditProvider => context.AuditProvider;

        public InstrumentationCommon InstrumentationProvider => context.InstrumentationProvider;

        public ExceptionHandlingService ExceptionHandlingService => context.ExceptionHandlingService;
    }
} // end of namespace
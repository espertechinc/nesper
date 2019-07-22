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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

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

        public string StatementName {
            get => context.StatementName;
        }

        public string RuntimeURI {
            get => context.RuntimeURI;
        }

        public int StatementId {
            get => context.StatementId;
        }

        public string DeploymentId {
            get => context.DeploymentId;
        }

        public TimeProvider TimeProvider {
            get => context.TimeProvider;
        }

        public ExpressionResultCacheService ExpressionResultCacheService {
            get => context.ExpressionResultCacheService;
        }

        public int AgentInstanceId {
            get => context.AgentInstanceId;
        }

        public EventBean ContextProperties {
            get => context.ContextProperties;
        }

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext {
            get => context.AllocateAgentInstanceScriptContext;
        }

        public StatementAgentInstanceLock AgentInstanceLock {
            get => context.AgentInstanceLock;
        }

        public TableExprEvaluatorContext TableExprEvaluatorContext {
            get => tableExprEvaluatorContext;
        }

        public object UserObjectCompileTime {
            get => context.UserObjectCompileTime;
        }

        public EventBeanService EventBeanService {
            get => context.EventBeanService;
        }

        public AuditProvider AuditProvider {
            get => context.AuditProvider;
        }

        public InstrumentationCommon InstrumentationProvider {
            get => context.InstrumentationProvider;
        }
    }
} // end of namespace
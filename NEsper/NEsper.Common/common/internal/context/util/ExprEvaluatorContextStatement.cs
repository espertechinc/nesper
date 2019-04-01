///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

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

namespace com.espertech.esper.common.@internal.context.util
{
    public class ExprEvaluatorContextStatement : ExprEvaluatorContext
    {
        private readonly bool allowTableAccess;
        protected internal readonly StatementContext statementContext;

        public ExprEvaluatorContextStatement(
            StatementContext statementContext,
            bool allowTableAccess)
        {
            this.statementContext = statementContext;
            this.allowTableAccess = allowTableAccess;
        }

        /// <summary>
        ///     Returns the time provider.
        /// </summary>
        /// <returns>time provider</returns>
        public TimeProvider TimeProvider => statementContext.TimeProvider;

        public ExpressionResultCacheService ExpressionResultCacheService =>
            statementContext.ExpressionResultCacheServiceSharable;

        public int AgentInstanceId => -1;

        public EventBean ContextProperties { get; set; }

        public string StatementName => statementContext.StatementName;

        public int StatementId => statementContext.StatementId;

        public string DeploymentId => statementContext.DeploymentId;

        public object UserObjectCompileTime => statementContext.UserObjectCompileTime;

        public string RuntimeURI => statementContext.RuntimeURI;

        public EventBeanService EventBeanService => statementContext.EventBeanService;

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext =>
            statementContext.AllocateAgentInstanceScriptContext;

        public AuditProvider AuditProvider => AuditProviderDefault.INSTANCE;

        public InstrumentationCommon InstrumentationProvider => InstrumentationCommonDefault.INSTANCE;

        public StatementAgentInstanceLock AgentInstanceLock =>
            throw new UnsupportedOperationException("Agent-instance lock not available");

        public TableExprEvaluatorContext TableExprEvaluatorContext {
            get {
                if (!allowTableAccess) {
                    throw new EPException("Access to tables is not allowed");
                }

                return statementContext.TableExprEvaluatorContext;
            }
        }
    }
} // end of namespace
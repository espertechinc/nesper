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
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    public class ProxyExprEvaluatorContext : ExprEvaluatorContext
    {
        public Func<string> ProcStatementName { get; set; }
        public Func<object> ProcUserObjectCompileTime { get; set; }
        public Func<string> ProcRuntimeURI { get; set; }
        public Func<int> ProcStatementId { get; set; }
        public Func<EventBean> ProcContextProperties { get; set; }
        public Func<int> ProcAgentInstanceId { get; set; }
        public Func<EventBeanService> ProcEventBeanService { get; set; }
        public Func<TimeProvider> ProcTimeProvider { get; set; }
        public Func<IReaderWriterLock> ProcAgentInstanceLock { get; set; }
        public Func<ExpressionResultCacheService> ProcExpressionResultCacheService { get; set; }
        public Func<TableExprEvaluatorContext> ProcTableExprEvaluatorContext { get; set; }
        public Func<AgentInstanceScriptContext> ProcAllocateAgentInstanceScriptContext { get; set; }
        public Func<string> ProcDeploymentId { get; set; }
        public Func<AuditProvider> ProcAuditProvider { get; set; }
        public Func<InstrumentationCommon> ProcInstrumentationProvider { get; set; }
        public Func<ExceptionHandlingService> ProcExceptionHandlingService { get; set; }

        public string StatementName => ProcStatementName?.Invoke();
        public object UserObjectCompileTime => ProcUserObjectCompileTime?.Invoke();
        public string RuntimeURI => ProcRuntimeURI?.Invoke();
        public int StatementId => ProcStatementId.Invoke();
        public EventBean ContextProperties => ProcContextProperties?.Invoke();
        public int AgentInstanceId => ProcAgentInstanceId.Invoke();
        public EventBeanService EventBeanService => ProcEventBeanService?.Invoke();
        public TimeProvider TimeProvider => ProcTimeProvider?.Invoke();
        public IReaderWriterLock AgentInstanceLock => ProcAgentInstanceLock?.Invoke();
        public ExpressionResultCacheService ExpressionResultCacheService => ProcExpressionResultCacheService?.Invoke();
        public TableExprEvaluatorContext TableExprEvaluatorContext => ProcTableExprEvaluatorContext?.Invoke();

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext =>
            ProcAllocateAgentInstanceScriptContext?.Invoke();

        public string DeploymentId => ProcDeploymentId?.Invoke();
        public AuditProvider AuditProvider => ProcAuditProvider?.Invoke();
        public InstrumentationCommon InstrumentationProvider => ProcInstrumentationProvider?.Invoke();

        public ExceptionHandlingService ExceptionHandlingService => ProcExceptionHandlingService.Invoke();

        public object FilterReboolConstant {
            get;
            set;
        }
    }
}
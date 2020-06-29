///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPEventServiceExprEvaluatorContext : ExprEvaluatorContext
    {
        public EPEventServiceExprEvaluatorContext(
            string runtimeURI,
            EventBeanService eventBeanService,
            ExceptionHandlingService exceptionHandlingService,
            SchedulingService schedulingService)
        {
            RuntimeUri = runtimeURI;
            EventBeanService = eventBeanService;
            ExceptionHandlingService = exceptionHandlingService;
            SchedulingService = schedulingService;
        }

        public IReaderWriterLock AgentInstanceLock => null;

        public string RuntimeUri { get; }

        public SchedulingService SchedulingService { get; }

        public TimeProvider TimeProvider => SchedulingService;

        public int AgentInstanceId => -1;

        public EventBean ContextProperties => null;

        public string StatementName => "(statement name not available)";

        public string RuntimeURI => RuntimeUri;

        public int StatementId => -1;

        public string DeploymentId => "(deployment id not available)";

        public object UserObjectCompileTime => null;

        public EventBeanService EventBeanService { get; }

        public ExpressionResultCacheService ExpressionResultCacheService => null;

        public TableExprEvaluatorContext TableExprEvaluatorContext =>
            throw new UnsupportedOperationException("Table-access evaluation is not supported in this expression");

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext => null;

        public AuditProvider AuditProvider => AuditProviderDefault.INSTANCE;

        public InstrumentationCommon InstrumentationProvider => InstrumentationCommonDefault.INSTANCE;

        public ExceptionHandlingService ExceptionHandlingService { get; }

        public object FilterReboolConstant { get; set; }
    }
} // end of namespace
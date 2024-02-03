///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
    internal class FAFQueryMethodSelectNoFromExprEvaluatorContext : ExprEvaluatorContext
    {
        private readonly StatementContextRuntimeServices _services;
        private readonly FAFQueryMethodSelect _select;
        private readonly StatementAgentInstanceLock _lock;
        private readonly TableExprEvaluatorContext tableExprEvaluatorContext;
        private EventBean contextProperties;

        public FAFQueryMethodSelectNoFromExprEvaluatorContext(
            StatementContextRuntimeServices services,
            FAFQueryMethodSelect select)
        {
            _services = services;
            _select = select;
            _lock = new StatementAgentInstanceLockRW(false);

            tableExprEvaluatorContext = select.HasTableAccess
                ? new TableExprEvaluatorContext(services.Container.ThreadLocalManager())
                : null;
        }

        public TimeProvider TimeProvider => _services.SchedulingService;

        public int AgentInstanceId => -1;

        public string StatementName => "(statement name not available)";

        public string RuntimeURI => _services.RuntimeURI;

        public int StatementId => -1;

        public string DeploymentId => "(deployment id not available)";

        public object UserObjectCompileTime => null;

        public EventBeanService EventBeanService => _services.EventBeanService;

        public IReaderWriterLock AgentInstanceLock => _lock;

        public ExpressionResultCacheService ExpressionResultCacheService => null;

        public TableExprEvaluatorContext TableExprEvaluatorContext => tableExprEvaluatorContext;

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext => null;

        public AuditProvider AuditProvider => AuditProviderDefault.INSTANCE;

        public InstrumentationCommon InstrumentationProvider => InstrumentationCommonDefault.INSTANCE;

        public ExceptionHandlingService ExceptionHandlingService => _services.ExceptionHandlingService;

        public object FilterReboolConstant {
            set => throw new UnsupportedOperationException("Operation not implemented");
            get => null;
        }

        public string ContextName => _select.ContextName;

        public string EPLWhenAvailable => _select.ContextName;

        public TimeZoneInfo TimeZone => _services.ImportServiceRuntime.TimeZone;

        public TimeAbacus TimeAbacus => _services.ImportServiceRuntime.TimeAbacus;

        public VariableManagementService VariableManagementService => _services.VariableManagementService;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => _services.EventBeanService;

        public string ModuleName => null;

        public bool IsWritesToTables => false;

        public Attribute[] Annotations => _select.Annotations;

        public EventBean ContextProperties {
            set => contextProperties = value;
            get => contextProperties;
        }

        public TypeResolver TypeResolver => throw new NotImplementedException();
    }
} // end of namespace
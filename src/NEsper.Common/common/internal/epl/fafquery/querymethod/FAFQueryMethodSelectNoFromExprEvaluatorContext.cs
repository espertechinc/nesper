///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.epl.fafquery.querymethod
{
	public class FAFQueryMethodSelectNoFromExprEvaluatorContext : ExprEvaluatorContext
	{
		private readonly StatementContextRuntimeServices _services;
		private readonly FAFQueryMethodSelect _select;
		private readonly IReaderWriterLock _lock;
		private readonly TableExprEvaluatorContext _tableExprEvaluatorContext;
		private EventBean _contextProperties;

		public FAFQueryMethodSelectNoFromExprEvaluatorContext(
			StatementContextRuntimeServices services,
			FAFQueryMethodSelect select)
		{
			var threadLocalManager = services.Container.ThreadLocalManager();
			_lock = services.Container.RWLockManager().CreateLock(GetType());
			_services = services;
			_select = select;
			_tableExprEvaluatorContext = select.HasTableAccess ? new TableExprEvaluatorContext(threadLocalManager) : null;
		}

		public TimeProvider TimeProvider => _services.SchedulingService;

		public int AgentInstanceId => -1;

		public EventBean ContextProperties {
			get => _contextProperties;
			set => _contextProperties = value;
		}

		public string StatementName => "(statement name not available)";

		public string RuntimeURI => _services.RuntimeURI;

		public int StatementId => -1;

		public string DeploymentId => "(deployment id not available)";

		public object UserObjectCompileTime => null;

		public EventBeanService EventBeanService => _services.EventBeanService;

		public IReaderWriterLock AgentInstanceLock => _lock;

		public ExpressionResultCacheService ExpressionResultCacheService => null;

		public TableExprEvaluatorContext TableExprEvaluatorContext => _tableExprEvaluatorContext;

		public AgentInstanceScriptContext AllocateAgentInstanceScriptContext => null;

		public AuditProvider AuditProvider => AuditProviderDefault.INSTANCE;

		public InstrumentationCommon InstrumentationProvider => InstrumentationCommonDefault.INSTANCE;

		public ExceptionHandlingService ExceptionHandlingService => _services.ExceptionHandlingService;

		public object FilterReboolConstant {
			get => null;
			set => throw new UnsupportedOperationException("Operation not implemented");
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
	}
} // end of namespace

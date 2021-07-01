///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.airegistry;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.dataflow.filtersvcadapter;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.pattern.pool;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.subselect;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.compat.threading.threadlocal;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementContext : ExprEvaluatorContext
        , SubSelectStrategyFactoryContext
        , EventTableFactoryFactoryContext
    {
        private IList<StatementFinalizeCallback> finalizeCallbacks;
        private AgentInstanceScriptContext defaultAgentInstanceScriptContext;

        public StatementContext(
            IContainer container,
            ContextRuntimeDescriptor contextRuntimeDescriptor,
            string deploymentId,
            int statementId,
            string statementName,
            string moduleName,
            StatementInformationalsRuntime statementInformationals,
            object userObjectRuntime,
            StatementContextRuntimeServices statementContextRuntimeServices,
            EPStatementHandle epStatementHandle,
            IDictionary<int, FilterSpecActivatable> filterSpecActivatables,
            PatternSubexpressionPoolStmtSvc patternSubexpressionPoolSvc,
            RowRecogStatePoolStmtSvc rowRecogStatePoolStmtSvc,
            ScheduleBucket scheduleBucket,
            StatementAIResourceRegistry statementAIResourceRegistry,
            StatementCPCacheService statementCPCacheService,
            StatementAIFactoryProvider statementAIFactoryProvider,
            StatementResultService statementResultService,
            UpdateDispatchView updateDispatchView,
            FilterService filterService,
            SchedulingService schedulingService,
            InternalEventRouteDest internalEventRouteDest)
        {
            Container = container;
            ContextRuntimeDescriptor = contextRuntimeDescriptor;
            DeploymentId = deploymentId;
            StatementId = statementId;
            StatementName = statementName;
            ModuleName = moduleName;
            StatementInformationals = statementInformationals;
            UserObjectRuntime = userObjectRuntime;
            StatementContextRuntimeServices = statementContextRuntimeServices;
            EpStatementHandle = epStatementHandle;
            FilterSpecActivatables = filterSpecActivatables;
            PatternSubexpressionPoolSvc = patternSubexpressionPoolSvc;
            RowRecogStatePoolStmtSvc = rowRecogStatePoolStmtSvc;
            ScheduleBucket = scheduleBucket;
            StatementAIResourceRegistry = statementAIResourceRegistry;
            StatementCPCacheService = statementCPCacheService;
            StatementAIFactoryProvider = statementAIFactoryProvider;
            StatementResultService = statementResultService;
            UpdateDispatchView = updateDispatchView;
            StatementContextFilterEvalEnv = new StatementContextFilterEvalEnv(
                statementContextRuntimeServices.ImportServiceRuntime,
                statementInformationals.Annotations,
                statementContextRuntimeServices.VariableManagementService,
                statementContextRuntimeServices.TableExprEvaluatorContext);
            this.FilterService = filterService;
            this.SchedulingService = schedulingService;
            this.InternalEventRouteDest = internalEventRouteDest;
        }

        public IContainer Container { get; set; }

        public IThreadLocalManager ThreadLocalManager => Container.ThreadLocalManager();

        public ILockManager LockManager => Container.LockManager();

        public IReaderWriterLockManager RWLockManager => Container.RWLockManager();

        public Attribute[] Annotations => StatementInformationals.Annotations;

        public string ContextName => StatementInformationals.OptionalContextName;

        public ContextRuntimeDescriptor ContextRuntimeDescriptor { get; }

        public ContextServiceFactory ContextServiceFactory => StatementContextRuntimeServices.ContextServiceFactory;

        public RuntimeSettingsService RuntimeSettingsService => StatementContextRuntimeServices.RuntimeSettingsService;

        public string DeploymentId { get; }

        public EPStatementHandle EpStatementHandle { get; }

        public RuntimeExtensionServices RuntimeExtensionServices => StatementContextRuntimeServices.RuntimeExtensionServices;

        public EventBeanTypedEventFactory EventBeanTypedEventFactory => StatementContextRuntimeServices.EventBeanTypedEventFactory;

        public EventBeanService EventBeanService => StatementContextRuntimeServices.EventBeanService;

        public string RuntimeURI => StatementContextRuntimeServices.RuntimeURI;

        public ExpressionResultCacheService ExpressionResultCacheServiceSharable => StatementContextRuntimeServices.ExpressionResultCacheService;

        public ImportServiceRuntime ImportServiceRuntime => StatementContextRuntimeServices.ImportServiceRuntime;

        public EventTableIndexService EventTableIndexService => StatementContextRuntimeServices.EventTableIndexService;

        public EventTypeRepositoryImpl EventTypeRepositoryPreconfigured => StatementContextRuntimeServices.EventTypeRepositoryPreconfigured;

        public FilterService FilterService { get; set; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory => StatementContextRuntimeServices.FilterBooleanExpressionFactory;

        public FilterSharedLookupableRepository FilterSharedLookupableRepository => StatementContextRuntimeServices.FilterSharedLookupableRepository;

        public FilterSharedBoolExprRepository FilterSharedBoolExprRepository => StatementContextRuntimeServices.FilterSharedBoolExprRepository;

        public IDictionary<int, FilterSpecActivatable> FilterSpecActivatables { get; }

        public InternalEventRouter InternalEventRouter => StatementContextRuntimeServices.InternalEventRouter;

        public InternalEventRouteDest InternalEventRouteDest { get; set; }

        public NamedWindowConsumerManagementService NamedWindowConsumerManagementService => 
            StatementContextRuntimeServices.NamedWindowConsumerManagementService;

        public NamedWindowManagementService NamedWindowManagementService => StatementContextRuntimeServices.NamedWindowManagementService;

        public int Priority => EpStatementHandle.Priority;

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory => StatementContextRuntimeServices.ResultSetProcessorHelperFactory;

        public int StatementId { get; }

        public StatementCPCacheService StatementCPCacheService { get; }

        public StatementContextRuntimeServices StatementContextRuntimeServices { get; }

        public SchedulingService SchedulingService { get; set; }

        public string StatementName { get; }

        public StatementAIResourceRegistry StatementAIResourceRegistry { get; }

        public StatementAIFactoryProvider StatementAIFactoryProvider { get; }

        public ScheduleBucket ScheduleBucket { get; }

        public bool IsStatelessSelect => StatementInformationals.IsStateless;

        public StatementAgentInstanceLockFactory StatementAgentInstanceLockFactory =>
            StatementContextRuntimeServices.StatementAgentInstanceLockFactory;

        public StatementResultService StatementResultService { get; }

        public TableManagementService TableManagementService => StatementContextRuntimeServices.TableManagementService;

        public TimeProvider TimeProvider => SchedulingService;

        public object UserObjectCompileTime => StatementInformationals.UserObjectCompileTime;

        public UpdateDispatchView UpdateDispatchView { get; }

        public ViewServicePreviousFactory ViewServicePreviousFactory => StatementContextRuntimeServices.ViewServicePreviousFactory;

        public ViewFactoryService ViewFactoryService => StatementContextRuntimeServices.ViewFactoryService;

        public StatementResourceService StatementResourceService => StatementCPCacheService.StatementResourceService;

        public PathRegistry<string, ContextMetaData> PathContextRegistry => StatementContextRuntimeServices.PathContextRegistry;

        public PatternSubexpressionPoolStmtSvc PatternSubexpressionPoolSvc { get; }

        public StatementInformationalsRuntime StatementInformationals { get; }
        
        public virtual object FilterReboolConstant
        {
            get => null;
            set { }
        }

        public void AddFinalizeCallback(StatementFinalizeCallback callback)
        {
            if (finalizeCallbacks == null) {
                finalizeCallbacks = Collections.SingletonList(callback);
                return;
            }

            if (finalizeCallbacks.Count == 1) {
                IList<StatementFinalizeCallback> list = new List<StatementFinalizeCallback>(2);
                list.AddAll(finalizeCallbacks);
                finalizeCallbacks = list;
            }

            finalizeCallbacks.Add(callback);
        }

        public IEnumerator<StatementFinalizeCallback> FinalizeCallbacks =>
            finalizeCallbacks?.GetEnumerator() ?? EnumerationHelper.Empty<StatementFinalizeCallback>();

        public ExceptionHandlingService ExceptionHandlingService =>
            StatementContextRuntimeServices.ExceptionHandlingService;

        public AgentInstanceContext MakeAgentInstanceContextUnpartitioned()
        {
            var @lock = StatementAIFactoryProvider.Factory.ObtainAgentInstanceLock(this, -1);
            var epStatementAgentInstanceHandle = new EPStatementAgentInstanceHandle(EpStatementHandle, -1, @lock);
            var auditProvider = StatementInformationals.AuditProvider;
            var instrumentationProvider = StatementInformationals.InstrumentationProvider;
            return new AgentInstanceContext(
                this,
                epStatementAgentInstanceHandle,
                null,
                null,
                auditProvider,
                instrumentationProvider);
        }

        public ContextManagementService ContextManagementService => StatementContextRuntimeServices.ContextManagementService;

        public VariableManagementService VariableManagementService => StatementContextRuntimeServices.VariableManagementService;

        public StatementContextFilterEvalEnv StatementContextFilterEvalEnv { get; }

        public StatementDestroyCallback DestroyCallback { get; set; }

        public TableExprEvaluatorContext TableExprEvaluatorContext => StatementContextRuntimeServices.TableExprEvaluatorContext;

        public EventBean ContextProperties =>
            throw new IllegalStateException("Context properties not available at statement-level");

        public int AgentInstanceId =>
            throw new IllegalStateException("Agent instance id not available at statement-level");

        public IReaderWriterLock AgentInstanceLock =>
            throw new IllegalStateException("Agent instance lock not available at statement-level");

        public ExpressionResultCacheService ExpressionResultCacheService => ExpressionResultCacheServiceSharable;

        public AgentInstanceScriptContext AllocateAgentInstanceScriptContext {
            get {
                if (defaultAgentInstanceScriptContext == null) {
                    defaultAgentInstanceScriptContext = AgentInstanceScriptContext.From(this);
                }

                return defaultAgentInstanceScriptContext;
            }
        }

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory => StatementContextRuntimeServices.EventTypeResolvingBeanFactory;

        public PathRegistry<string, EventType> EventTypePathRegistry => StatementContextRuntimeServices.EventTypePathRegistry;

        public EventTypeAvroHandler EventTypeAvroHandler => StatementContextRuntimeServices.EventTypeAvroHandler;

        public object Runtime => StatementContextRuntimeServices.Runtime;

        public RowRecogStatePoolStmtSvc RowRecogStatePoolStmtSvc { get; }

        public RowRecogStateRepoFactory RowRecogStateRepoFactory => StatementContextRuntimeServices.RowRecogStateRepoFactory;

        public HistoricalDataCacheFactory HistoricalDataCacheFactory => StatementContextRuntimeServices.HistoricalDataCacheFactory;

        public DatabaseConfigServiceRuntime DatabaseConfigService => StatementContextRuntimeServices.DatabaseConfigService;

        public EPRuntimeEventProcessWrapped EPRuntimeEventProcessWrapped => StatementContextRuntimeServices.EPRuntimeEventProcessWrapped;

        public EventServiceSendEventCommon EPRuntimeSendEvent => StatementContextRuntimeServices.EPRuntimeSendEvent;

        public EPRenderEventService EPRuntimeRenderEvent => StatementContextRuntimeServices.EPRuntimeRenderEvent;

        public DataFlowFilterServiceAdapter DataFlowFilterServiceAdapter => StatementContextRuntimeServices.DataFlowFilterServiceAdapter;

        public MetricReportingService MetricReportingService => StatementContextRuntimeServices.MetricReportingService;

        public AuditProvider AuditProvider => AuditProviderDefault.INSTANCE;

        public InstrumentationCommon InstrumentationProvider => InstrumentationCommonDefault.INSTANCE;

        public INamingContext RuntimeEnvContext {
            get => StatementContextRuntimeServices.RuntimeEnvContext;
        }

        public object UserObjectRuntime { get; }

        public StatementType StatementType => StatementInformationals.StatementType;

        public string ModuleName { get; }

        public EventTableFactoryFactoryContext EventTableFactoryContext => this;
    }
} // end of namespace
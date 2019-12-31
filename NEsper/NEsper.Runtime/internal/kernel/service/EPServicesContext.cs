///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.filtersvcadapter;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.pool;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.epl.script.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.render;
using com.espertech.esper.common.@internal.@event.xml;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.statement.multimatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;
using com.espertech.esper.runtime.@internal.timer;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    /// <summary>
    ///     Convenience class to hold implementations for all services.
    /// </summary>
    public sealed class EPServicesContext
    {
        private readonly DataFlowFilterServiceAdapter dataFlowFilterServiceAdapter;
        private readonly EPRuntimeSPI epRuntime;

        private readonly ExpressionResultCacheService expressionResultCacheService;
        private readonly HistoricalDataCacheFactory historicalDataCacheFactory;

        private readonly RowRecogStateRepoFactory rowRecogStateRepoFactory;

        private StatementContextRuntimeServices statementContextRuntimeServices;

        public EPServicesContext(
            IContainer container,
            AggregationServiceFactoryService aggregationServiceFactoryService,
            BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate,
            BeanEventTypeStemService beanEventTypeStemService,
            ClassForNameProvider classForNameProvider,
            Configuration configSnapshot,
            ContextManagementService contextManagementService,
            PathRegistry<string, ContextMetaData> contextPathRegistry,
            ContextServiceFactory contextServiceFactory,
            EPDataFlowServiceImpl dataflowService,
            DataFlowFilterServiceAdapter dataFlowFilterServiceAdapter,
            DataInputOutputSerdeProvider dataInputOutputSerdeProvider,
            DatabaseConfigServiceRuntime databaseConfigServiceRuntime,
            DeploymentLifecycleService deploymentLifecycleService,
            DispatchService dispatchService,
            RuntimeEnvContext runtimeEnvContext,
            RuntimeSettingsService runtimeSettingsService,
            string runtimeURI,
            ImportServiceRuntime importServiceRuntime,
            EPStatementFactory epStatementFactory,
            PathRegistry<string, ExpressionDeclItem> exprDeclaredPathRegistry,
            IReaderWriterLock eventProcessingRWLock,
            EPServicesHA epServicesHA,
            EPRuntimeSPI epRuntime,
            EventBeanService eventBeanService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EPRenderEventServiceImpl eventRenderer,
            EventTableIndexService eventTableIndexService,
            EventTypeAvroHandler eventTypeAvroHandler,
            EventTypeFactory eventTypeFactory,
            PathRegistry<string, EventType> eventTypePathRegistry,
            EventTypeRepositoryImpl eventTypeRepositoryBus,
            EventTypeResolvingBeanFactory eventTypeResolvingBeanFactory,
            ExceptionHandlingService exceptionHandlingService,
            ExpressionResultCacheService expressionResultCacheService,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            FilterServiceSPI filterService,
            FilterSharedBoolExprRepository filterSharedBoolExprRepository,
            FilterSharedLookupableRepository filterSharedLookupableRepository,
            HistoricalDataCacheFactory historicalDataCacheFactory,
            InternalEventRouterImpl internalEventRouter,
            MetricReportingService metricReportingService,
            MultiMatchHandlerFactory multiMatchHandlerFactory,
            NamedWindowConsumerManagementService namedWindowConsumerManagementService,
            NamedWindowDispatchService namedWindowDispatchService,
            NamedWindowFactoryService namedWindowFactoryService,
            NamedWindowManagementService namedWindowManagementService,
            PathRegistry<string, NamedWindowMetaData> namedWindowPathRegistry,
            PatternFactoryService patternFactoryService,
            PatternSubexpressionPoolRuntimeSvc patternSubexpressionPoolEngineSvc,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            RowRecogStateRepoFactory rowRecogStateRepoFactory,
            RowRecogStatePoolRuntimeSvc rowRecogStatePoolEngineSvc,
            SchedulingServiceSPI schedulingService,
            PathRegistry<NameAndParamNum, ExpressionScriptProvided> scriptPathRegistry,
            StatementLifecycleService statementLifecycleService,
            StatementAgentInstanceLockFactory statementAgentInstanceLockFactory,
            StatementResourceHolderBuilder statementResourceHolderBuilder,
            TableExprEvaluatorContext tableExprEvaluatorContext,
            TableManagementService tableManagementService,
            PathRegistry<string, TableMetaData> tablePathRegistry,
            ThreadingService threadingService,
            TimeAbacus timeAbacus,
            TimeSourceService timeSourceService,
            TimerService timerService,
            VariableManagementService variableManagementService,
            PathRegistry<string, VariableMetaData> variablePathRegistry,
            ViewableActivatorFactory viewableActivatorFactory,
            ViewFactoryService viewFactoryService,
            ViewServicePreviousFactory viewServicePreviousFactory,
            XMLFragmentEventTypeFactory xmlFragmentEventTypeFactory)
        {
            Container = container;
            AggregationServiceFactoryService = aggregationServiceFactoryService;
            BeanEventTypeFactoryPrivate = beanEventTypeFactoryPrivate;
            BeanEventTypeStemService = beanEventTypeStemService;
            ClassForNameProvider = classForNameProvider;
            ConfigSnapshot = configSnapshot;
            ContextManagementService = contextManagementService;
            ContextPathRegistry = contextPathRegistry;
            ContextServiceFactory = contextServiceFactory;
            DataflowService = dataflowService;
            this.dataFlowFilterServiceAdapter = dataFlowFilterServiceAdapter;
            DataInputOutputSerdeProvider = dataInputOutputSerdeProvider;
            DatabaseConfigServiceRuntime = databaseConfigServiceRuntime;
            DeploymentLifecycleService = deploymentLifecycleService;
            DispatchService = dispatchService;
            RuntimeEnvContext = runtimeEnvContext;
            RuntimeSettingsService = runtimeSettingsService;
            RuntimeURI = runtimeURI;
            ImportServiceRuntime = importServiceRuntime;
            EpStatementFactory = epStatementFactory;
            ExprDeclaredPathRegistry = exprDeclaredPathRegistry;
            EventProcessingRWLock = eventProcessingRWLock;
            EpServicesHA = epServicesHA;
            this.epRuntime = epRuntime;
            EventBeanService = eventBeanService;
            EventBeanTypedEventFactory = eventBeanTypedEventFactory;
            EventRenderer = eventRenderer;
            EventTableIndexService = eventTableIndexService;
            EventTypeAvroHandler = eventTypeAvroHandler;
            EventTypeFactory = eventTypeFactory;
            EventTypePathRegistry = eventTypePathRegistry;
            EventTypeRepositoryBus = eventTypeRepositoryBus;
            EventTypeResolvingBeanFactory = eventTypeResolvingBeanFactory;
            ExceptionHandlingService = exceptionHandlingService;
            this.expressionResultCacheService = expressionResultCacheService;
            FilterBooleanExpressionFactory = filterBooleanExpressionFactory;
            FilterService = filterService;
            FilterSharedBoolExprRepository = filterSharedBoolExprRepository;
            FilterSharedLookupableRepository = filterSharedLookupableRepository;
            this.historicalDataCacheFactory = historicalDataCacheFactory;
            InternalEventRouter = internalEventRouter;
            MetricReportingService = metricReportingService;
            MultiMatchHandlerFactory = multiMatchHandlerFactory;
            NamedWindowConsumerManagementService = namedWindowConsumerManagementService;
            NamedWindowDispatchService = namedWindowDispatchService;
            NamedWindowFactoryService = namedWindowFactoryService;
            NamedWindowManagementService = namedWindowManagementService;
            NamedWindowPathRegistry = namedWindowPathRegistry;
            PatternFactoryService = patternFactoryService;
            PatternSubexpressionPoolRuntimeSvc = patternSubexpressionPoolEngineSvc;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
            this.rowRecogStateRepoFactory = rowRecogStateRepoFactory;
            RowRecogStatePoolEngineSvc = rowRecogStatePoolEngineSvc;
            SchedulingService = schedulingService;
            ScriptPathRegistry = scriptPathRegistry;
            StatementLifecycleService = statementLifecycleService;
            StatementAgentInstanceLockFactory = statementAgentInstanceLockFactory;
            StatementResourceHolderBuilder = statementResourceHolderBuilder;
            TableExprEvaluatorContext = tableExprEvaluatorContext;
            TableManagementService = tableManagementService;
            TablePathRegistry = tablePathRegistry;
            ThreadingService = threadingService;
            TimeAbacus = timeAbacus;
            TimeSourceService = timeSourceService;
            TimerService = timerService;
            VariableManagementService = variableManagementService;
            VariablePathRegistry = variablePathRegistry;
            ViewableActivatorFactory = viewableActivatorFactory;
            ViewFactoryService = viewFactoryService;
            ViewServicePreviousFactory = viewServicePreviousFactory;
            XmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
        }

        public IContainer Container { get; }

        public RuntimeExtensionServices RuntimeExtensionServices => EpServicesHA.RuntimeExtensionServices;

        public DeploymentRecoveryService DeploymentRecoveryService => EpServicesHA.DeploymentRecoveryService;

        public ListenerRecoveryService ListenerRecoveryService => EpServicesHA.ListenerRecoveryService;

        public InternalEventRouteDest InternalEventRouteDest { set; get; }

        public AggregationServiceFactoryService AggregationServiceFactoryService { get; }

        public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate { get; }

        public BeanEventTypeStemService BeanEventTypeStemService { get; }

        public ClassForNameProvider ClassForNameProvider { get; }

        public Configuration ConfigSnapshot { get; }

        public ContextManagementService ContextManagementService { get; }

        public ContextServiceFactory ContextServiceFactory { get; }

        public DatabaseConfigServiceRuntime DatabaseConfigServiceRuntime { get; }

        public EPDataFlowServiceImpl DataflowService { get; }

        public DataInputOutputSerdeProvider DataInputOutputSerdeProvider { get; }

        public DeploymentLifecycleService DeploymentLifecycleService { get; }

        public DispatchService DispatchService { get; }

        public RuntimeEnvContext RuntimeEnvContext { get; }

        public ImportServiceRuntime ImportServiceRuntime { get; }

        public RuntimeSettingsService RuntimeSettingsService { get; }

        public string RuntimeURI { get; }

        public EPStatementFactory EpStatementFactory { get; }

        public IReaderWriterLock EventProcessingRWLock { get; }

        public EPServicesHA EpServicesHA { get; }

        public EPRuntime EpRuntime => epRuntime;

        public EventBeanService EventBeanService { get; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public EPRenderEventServiceImpl EventRenderer { get; }

        public EventTableIndexService EventTableIndexService { get; }

        public EventTypeAvroHandler EventTypeAvroHandler { get; }

        public EventTypeFactory EventTypeFactory { get; }

        public EventTypeRepositoryImpl EventTypeRepositoryBus { get; }

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory { get; }

        public ExceptionHandlingService ExceptionHandlingService { get; }

        public PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry { get; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; }

        public FilterServiceSPI FilterService { get; }

        public FilterSharedBoolExprRepository FilterSharedBoolExprRepository { get; }

        public FilterSharedLookupableRepository FilterSharedLookupableRepository { get; }

        public InternalEventRouterImpl InternalEventRouter { get; }

        public RowRecogStatePoolRuntimeSvc RowRecogStatePoolEngineSvc { get; }

        public MetricReportingService MetricReportingService { get; }

        public MultiMatchHandlerFactory MultiMatchHandlerFactory { get; }

        public NamedWindowConsumerManagementService NamedWindowConsumerManagementService { get; }

        public NamedWindowDispatchService NamedWindowDispatchService { get; }

        public NamedWindowFactoryService NamedWindowFactoryService { get; }

        public NamedWindowManagementService NamedWindowManagementService { get; }

        public PathRegistry<string, ContextMetaData> ContextPathRegistry { get; }

        public PathRegistry<string, EventType> EventTypePathRegistry { get; }

        public PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry { get; }

        public PatternFactoryService PatternFactoryService { get; }

        public PatternSubexpressionPoolRuntimeSvc PatternSubexpressionPoolRuntimeSvc { get; }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; }

        public SchedulingServiceSPI SchedulingService { get; }

        public PathRegistry<NameAndParamNum, ExpressionScriptProvided> ScriptPathRegistry { get; }

        public StatementLifecycleService StatementLifecycleService { get; }

        public StatementAgentInstanceLockFactory StatementAgentInstanceLockFactory { get; }

        public StatementResourceHolderBuilder StatementResourceHolderBuilder { get; }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        public TableManagementService TableManagementService { get; }

        public PathRegistry<string, TableMetaData> TablePathRegistry { get; }

        public ThreadingService ThreadingService { get; }

        public TimeAbacus TimeAbacus { get; }

        public TimerService TimerService { get; }

        public TimeSourceService TimeSourceService { get; }

        public VariableManagementService VariableManagementService { get; }

        public PathRegistry<string, VariableMetaData> VariablePathRegistry { get; }

        public ViewableActivatorFactory ViewableActivatorFactory { get; }

        public ViewFactoryService ViewFactoryService { get; }

        public ViewServicePreviousFactory ViewServicePreviousFactory { get; }

        public XMLFragmentEventTypeFactory XmlFragmentEventTypeFactory { get; }

        public void Destroy()
        {
            if (EpServicesHA != null) {
                EpServicesHA.Destroy();
            }
        }

        public void Initialize()
        {
        }

        public StatementContextRuntimeServices StatementContextRuntimeServices {
            get {
                if (statementContextRuntimeServices == null) {
                    statementContextRuntimeServices = new StatementContextRuntimeServices(
                        Container,
                        ContextManagementService,
                        ContextServiceFactory,
                        DatabaseConfigServiceRuntime,
                        dataFlowFilterServiceAdapter,
                        DataflowService,
                        RuntimeURI,
                        RuntimeEnvContext,
                        ImportServiceRuntime,
                        RuntimeSettingsService,
                        EpServicesHA.RuntimeExtensionServices,
                        epRuntime,
                        EventRenderer,
                        epRuntime.EventServiceSPI,
                        (EPEventServiceSPI) epRuntime.EventService,
                        EventBeanService,
                        EventBeanTypedEventFactory,
                        EventTableIndexService,
                        EventTypeAvroHandler,
                        EventTypePathRegistry,
                        EventTypeRepositoryBus,
                        EventTypeResolvingBeanFactory,
                        ExceptionHandlingService,
                        expressionResultCacheService,
                        FilterService,
                        FilterBooleanExpressionFactory,
                        FilterSharedBoolExprRepository,
                        FilterSharedLookupableRepository,
                        historicalDataCacheFactory,
                        InternalEventRouter,
                        InternalEventRouteDest,
                        MetricReportingService,
                        NamedWindowConsumerManagementService,
                        NamedWindowManagementService,
                        ContextPathRegistry,
                        NamedWindowPathRegistry,
                        rowRecogStateRepoFactory,
                        ResultSetProcessorHelperFactory,
                        SchedulingService,
                        StatementAgentInstanceLockFactory,
                        StatementResourceHolderBuilder,
                        TableExprEvaluatorContext,
                        TableManagementService,
                        VariableManagementService,
                        ViewFactoryService,
                        ViewServicePreviousFactory);
                }

                return statementContextRuntimeServices;
            }
        }
    }
}
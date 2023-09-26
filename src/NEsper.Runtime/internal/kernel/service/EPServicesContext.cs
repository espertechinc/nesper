///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.classprovided.core;
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
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde.runtime.@event;
using com.espertech.esper.common.@internal.serde.runtime.eventtype;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.dispatch;
using com.espertech.esper.common.@internal.statement.multimatch;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.stage;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;
using com.espertech.esper.runtime.@internal.timer;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPServicesContext : EPServicesEvaluation, EPServicesPath
	{
		private readonly IContainer _container;
	    private readonly AggregationServiceFactoryService _aggregationServiceFactoryService;
	    private readonly BeanEventTypeFactoryPrivate _beanEventTypeFactoryPrivate;
	    private readonly BeanEventTypeStemService _beanEventTypeStemService;
	    private readonly TypeResolver _typeResolver;
	    private readonly ParentTypeResolver _typeResolverParent;
	    private readonly PathRegistry<string, ClassProvided> _classProvidedPathRegistry;
	    private readonly Configuration _configSnapshot;
	    private readonly ContextManagementService _contextManagementService;
	    private readonly PathRegistry<string, ContextMetaData> _contextPathRegistry;
	    private readonly ContextServiceFactory _contextServiceFactory;
	    private readonly EPDataFlowServiceImpl _dataflowService;
	    private readonly DataFlowFilterServiceAdapter _dataFlowFilterServiceAdapter;
	    private readonly DatabaseConfigServiceRuntime _databaseConfigServiceRuntime;
	    private readonly DeploymentLifecycleService _deploymentLifecycleService;
	    private readonly DispatchService _dispatchService;
	    private readonly RuntimeEnvContext _runtimeEnvContext;
	    private readonly RuntimeSettingsService _runtimeSettingsService;
	    private readonly string _runtimeUri;
	    private readonly ImportServiceRuntime _importServiceRuntime;
	    private readonly EPStatementFactory _epStatementFactory;
	    private readonly PathRegistry<string, ExpressionDeclItem> _exprDeclaredPathRegistry;
	    private readonly IReaderWriterLock _eventProcessingRWLock;
	    private readonly EPServicesHA _epServicesHA;
	    private readonly EPRuntimeSPI _epRuntime;
	    private readonly EventBeanService _eventBeanService;
	    private readonly EventBeanTypedEventFactory _eventBeanTypedEventFactory;
	    private readonly EPRenderEventServiceImpl _eventRenderer;
	    private readonly EventSerdeFactory _eventSerdeFactory;
	    private readonly EventTableIndexService _eventTableIndexService;
	    private readonly EventTypeAvroHandler _eventTypeAvroHandler;
	    private readonly EventTypeFactory _eventTypeFactory;
	    private readonly EventTypeIdResolver _eventTypeIdResolver;
	    private readonly PathRegistry<string, EventType> _eventTypePathRegistry;
	    private readonly EventTypeRepositoryImpl _eventTypeRepositoryBus;
	    private readonly EventTypeResolvingBeanFactory _eventTypeResolvingBeanFactory;
	    private readonly EventTypeSerdeRepository _eventTypeSerdeRepository;
	    private readonly EventTypeXMLXSDHandler _eventTypeXMLXSDHandler;
	    private readonly ExceptionHandlingService _exceptionHandlingService;
	    private readonly ExpressionResultCacheService _expressionResultCacheService;
	    private readonly FilterBooleanExpressionFactory _filterBooleanExpressionFactory;
	    private readonly FilterServiceSPI _filterService;
	    private readonly FilterSharedBoolExprRepository _filterSharedBoolExprRepository;
	    private readonly FilterSharedLookupableRepository _filterSharedLookupableRepository;
	    private readonly HistoricalDataCacheFactory _historicalDataCacheFactory;
	    private readonly InternalEventRouterImpl _internalEventRouter;
	    private readonly MetricReportingService _metricReportingService;
	    private readonly MultiMatchHandlerFactory _multiMatchHandlerFactory;
	    private readonly NamedWindowConsumerManagementService _namedWindowConsumerManagementService;
	    private readonly NamedWindowDispatchService _namedWindowDispatchService;
	    private readonly NamedWindowFactoryService _namedWindowFactoryService;
	    private readonly NamedWindowManagementService _namedWindowManagementService;
	    private readonly PathRegistry<string, NamedWindowMetaData> _namedWindowPathRegistry;
	    private readonly PatternFactoryService _patternFactoryService;
	    private readonly PatternSubexpressionPoolRuntimeSvc _patternSubexpressionPoolEngineSvc;
	    private readonly ResultSetProcessorHelperFactory _resultSetProcessorHelperFactory;
	    private readonly RowRecogStateRepoFactory _rowRecogStateRepoFactory;
	    private readonly RowRecogStatePoolRuntimeSvc _rowRecogStatePoolEngineSvc;
	    private readonly ScriptCompiler _scriptCompiler;
	    private readonly SchedulingServiceSPI _schedulingService;
	    private readonly PathRegistry<NameAndParamNum, ExpressionScriptProvided> _scriptPathRegistry;
	    private readonly StageRecoveryService _stageRecoveryService;
	    private readonly StatementLifecycleService _statementLifecycleService;
	    private readonly StatementAgentInstanceLockFactory _statementAgentInstanceLockFactory;
	    private readonly StatementResourceHolderBuilder _statementResourceHolderBuilder;
	    private readonly TableExprEvaluatorContext _tableExprEvaluatorContext;
	    private readonly TableManagementService _tableManagementService;
	    private readonly PathRegistry<string, TableMetaData> _tablePathRegistry;
	    private readonly ThreadingService _threadingService;
	    private readonly TimeAbacus _timeAbacus;
	    private readonly TimeSourceService _timeSourceService;
	    private readonly TimerService _timerService;
	    private readonly VariableManagementService _variableManagementService;
	    private readonly PathRegistry<string, VariableMetaData> _variablePathRegistry;
	    private readonly ViewableActivatorFactory _viewableActivatorFactory;
	    private readonly ViewFactoryService _viewFactoryService;
	    private readonly ViewServicePreviousFactory _viewServicePreviousFactory;
	    private readonly XMLFragmentEventTypeFactory _xmlFragmentEventTypeFactory;

	    private StatementContextRuntimeServices _statementContextRuntimeServices;
	    private InternalEventRouteDest _internalEventRouteDest;
	    private StageRuntimeServices _stageRuntimeServices;

	    public EPServicesContext(
		    IContainer container,
		    AggregationServiceFactoryService aggregationServiceFactoryService,
		    BeanEventTypeFactoryPrivate beanEventTypeFactoryPrivate,
		    BeanEventTypeStemService beanEventTypeStemService,
		    TypeResolver typeResolver,
		    ParentTypeResolver typeResolverParent,
		    PathRegistry<string, ClassProvided> classProvidedPathRegistry,
		    Configuration configSnapshot,
		    ContextManagementService contextManagementService,
		    PathRegistry<string, ContextMetaData> contextPathRegistry,
		    ContextServiceFactory contextServiceFactory,
		    EPDataFlowServiceImpl dataflowService,
		    DataFlowFilterServiceAdapter dataFlowFilterServiceAdapter,
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
		    EventSerdeFactory eventSerdeFactory,
		    EventTableIndexService eventTableIndexService,
		    EventTypeAvroHandler eventTypeAvroHandler,
		    EventTypeFactory eventTypeFactory,
		    EventTypeIdResolver eventTypeIdResolver,
		    PathRegistry<string, EventType> eventTypePathRegistry,
		    EventTypeRepositoryImpl eventTypeRepositoryBus,
		    EventTypeResolvingBeanFactory eventTypeResolvingBeanFactory,
		    EventTypeSerdeRepository eventTypeSerdeRepository,
		    EventTypeXMLXSDHandler eventTypeXMLXSDHandler,
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
		    ScriptCompiler scriptCompiler,
		    StageRecoveryService stageRecoveryService,
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
		    _container = container;
	        _aggregationServiceFactoryService = aggregationServiceFactoryService;
	        _beanEventTypeFactoryPrivate = beanEventTypeFactoryPrivate;
	        _beanEventTypeStemService = beanEventTypeStemService;
	        _typeResolver = typeResolver;
	        _typeResolverParent = typeResolverParent;
	        _classProvidedPathRegistry = classProvidedPathRegistry;
	        _configSnapshot = configSnapshot;
	        _contextManagementService = contextManagementService;
	        _contextPathRegistry = contextPathRegistry;
	        _contextServiceFactory = contextServiceFactory;
	        _dataflowService = dataflowService;
	        _dataFlowFilterServiceAdapter = dataFlowFilterServiceAdapter;
	        _databaseConfigServiceRuntime = databaseConfigServiceRuntime;
	        _deploymentLifecycleService = deploymentLifecycleService;
	        _dispatchService = dispatchService;
	        _runtimeEnvContext = runtimeEnvContext;
	        _runtimeSettingsService = runtimeSettingsService;
	        _runtimeUri = runtimeURI;
	        _importServiceRuntime = importServiceRuntime;
	        _epStatementFactory = epStatementFactory;
	        _exprDeclaredPathRegistry = exprDeclaredPathRegistry;
	        _eventProcessingRWLock = eventProcessingRWLock;
	        _epServicesHA = epServicesHA;
	        _epRuntime = epRuntime;
	        _eventBeanService = eventBeanService;
	        _eventBeanTypedEventFactory = eventBeanTypedEventFactory;
	        _eventRenderer = eventRenderer;
	        _eventSerdeFactory = eventSerdeFactory;
	        _eventTableIndexService = eventTableIndexService;
	        _eventTypeAvroHandler = eventTypeAvroHandler;
	        _eventTypeFactory = eventTypeFactory;
	        _eventTypeIdResolver = eventTypeIdResolver;
	        _eventTypePathRegistry = eventTypePathRegistry;
	        _eventTypeRepositoryBus = eventTypeRepositoryBus;
	        _eventTypeResolvingBeanFactory = eventTypeResolvingBeanFactory;
	        _eventTypeSerdeRepository = eventTypeSerdeRepository;
	        _eventTypeXMLXSDHandler = eventTypeXMLXSDHandler;
	        _exceptionHandlingService = exceptionHandlingService;
	        _expressionResultCacheService = expressionResultCacheService;
	        _filterBooleanExpressionFactory = filterBooleanExpressionFactory;
	        _filterService = filterService;
	        _filterSharedBoolExprRepository = filterSharedBoolExprRepository;
	        _filterSharedLookupableRepository = filterSharedLookupableRepository;
	        _historicalDataCacheFactory = historicalDataCacheFactory;
	        _internalEventRouter = internalEventRouter;
	        _metricReportingService = metricReportingService;
	        _multiMatchHandlerFactory = multiMatchHandlerFactory;
	        _namedWindowConsumerManagementService = namedWindowConsumerManagementService;
	        _namedWindowDispatchService = namedWindowDispatchService;
	        _namedWindowFactoryService = namedWindowFactoryService;
	        _namedWindowManagementService = namedWindowManagementService;
	        _namedWindowPathRegistry = namedWindowPathRegistry;
	        _patternFactoryService = patternFactoryService;
	        _patternSubexpressionPoolEngineSvc = patternSubexpressionPoolEngineSvc;
	        _resultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
	        _rowRecogStateRepoFactory = rowRecogStateRepoFactory;
	        _rowRecogStatePoolEngineSvc = rowRecogStatePoolEngineSvc;
	        _schedulingService = schedulingService;
	        _scriptPathRegistry = scriptPathRegistry;
	        _stageRecoveryService = stageRecoveryService;
	        _statementLifecycleService = statementLifecycleService;
	        _statementAgentInstanceLockFactory = statementAgentInstanceLockFactory;
	        _statementResourceHolderBuilder = statementResourceHolderBuilder;
	        _tableExprEvaluatorContext = tableExprEvaluatorContext;
	        _tableManagementService = tableManagementService;
	        _tablePathRegistry = tablePathRegistry;
	        _threadingService = threadingService;
	        _timeAbacus = timeAbacus;
	        _timeSourceService = timeSourceService;
	        _timerService = timerService;
	        _variableManagementService = variableManagementService;
	        _variablePathRegistry = variablePathRegistry;
	        _viewableActivatorFactory = viewableActivatorFactory;
	        _viewFactoryService = viewFactoryService;
	        _viewServicePreviousFactory = viewServicePreviousFactory;
	        _xmlFragmentEventTypeFactory = xmlFragmentEventTypeFactory;
	        _scriptCompiler = scriptCompiler;
	    }

	    public void Destroy()
	    {
		    _epServicesHA?.Destroy();
	    }

	    public void Initialize() {
	    }

	    public IContainer Container => _container;

	    public RuntimeExtensionServices RuntimeExtensionServices => _epServicesHA.RuntimeExtensionServices;

	    public DeploymentRecoveryService DeploymentRecoveryService => _epServicesHA.DeploymentRecoveryService;

	    public ListenerRecoveryService ListenerRecoveryService => _epServicesHA.ListenerRecoveryService;

	    public StatementContextRuntimeServices StatementContextRuntimeServices {
		    get {
			    if (_statementContextRuntimeServices == null) {
				    _statementContextRuntimeServices = new StatementContextRuntimeServices(
					    _container,
					    _configSnapshot,
					    _contextManagementService,
					    _contextPathRegistry,
					    _contextServiceFactory,
					    _databaseConfigServiceRuntime,
					    _dataFlowFilterServiceAdapter,
					    _dataflowService,
					    _runtimeUri,
					    _runtimeEnvContext,
					    _importServiceRuntime,
					    _runtimeSettingsService,
					    _epServicesHA.RuntimeExtensionServices,
					    _epRuntime,
					    _eventRenderer,
					    _epRuntime.EventServiceSPI,
					    (EPEventServiceSPI) _epRuntime.EventService,
					    _eventBeanService,
					    _eventBeanTypedEventFactory,
					    _eventTableIndexService,
					    _eventTypeAvroHandler,
					    _eventTypePathRegistry,
					    _eventTypeRepositoryBus,
					    _eventTypeResolvingBeanFactory,
					    _eventProcessingRWLock,
					    _exceptionHandlingService,
					    _expressionResultCacheService,
					    _filterBooleanExpressionFactory,
					    _filterSharedBoolExprRepository,
					    _filterSharedLookupableRepository,
					    _historicalDataCacheFactory,
					    _internalEventRouter,
					    _metricReportingService,
					    _namedWindowConsumerManagementService,
					    _namedWindowManagementService,
					    _contextPathRegistry,
					    _namedWindowPathRegistry,
					    _rowRecogStateRepoFactory,
					    _resultSetProcessorHelperFactory,
					    _schedulingService,
					    _statementAgentInstanceLockFactory,
					    _statementResourceHolderBuilder,
					    _tableExprEvaluatorContext,
					    _tableManagementService,
					    _variableManagementService,
					    _viewFactoryService,
					    _viewServicePreviousFactory);
			    }

			    return _statementContextRuntimeServices;
		    }
	    }

	    public StageRuntimeServices StageRuntimeServices {
		    get {
			    if (_stageRuntimeServices == null) {
				    _stageRuntimeServices = new StageRuntimeServices(
					    _container,
					    _importServiceRuntime,
					    _configSnapshot,
					    _dispatchService,
					    _eventBeanService,
					    _eventBeanTypedEventFactory,
					    _eventTypeRepositoryBus,
					    _eventTypeResolvingBeanFactory,
					    _exceptionHandlingService,
					    _namedWindowDispatchService,
					    _runtimeUri,
					    _runtimeSettingsService,
					    _statementLifecycleService,
					    _tableExprEvaluatorContext,
					    _threadingService,
					    _variableManagementService);
			    }

			    return _stageRuntimeServices;
		    }
	    }

	    public AggregationServiceFactoryService AggregationServiceFactoryService => _aggregationServiceFactoryService;

	    public BeanEventTypeFactoryPrivate BeanEventTypeFactoryPrivate => _beanEventTypeFactoryPrivate;

	    public BeanEventTypeStemService BeanEventTypeStemService => _beanEventTypeStemService;

	    public TypeResolver TypeResolver => _typeResolver;

	    public ParentTypeResolver TypeResolverParent => _typeResolverParent;

	    public PathRegistry<string, ClassProvided> ClassProvidedPathRegistry => _classProvidedPathRegistry;

	    public Configuration ConfigSnapshot => _configSnapshot;

	    public ContextManagementService ContextManagementService => _contextManagementService;

	    public ContextServiceFactory ContextServiceFactory => _contextServiceFactory;

	    public DatabaseConfigServiceRuntime DatabaseConfigServiceRuntime => _databaseConfigServiceRuntime;

	    public EPDataFlowServiceImpl DataflowService => _dataflowService;

	    public DeploymentLifecycleService DeploymentLifecycleService => _deploymentLifecycleService;

	    public DispatchService DispatchService => _dispatchService;

	    public RuntimeEnvContext RuntimeEnvContext => _runtimeEnvContext;

	    public ImportServiceRuntime ImportServiceRuntime => _importServiceRuntime;

	    public RuntimeSettingsService RuntimeSettingsService => _runtimeSettingsService;

	    public string RuntimeURI => _runtimeUri;

	    public EPStatementFactory EpStatementFactory => _epStatementFactory;

	    public IReaderWriterLock EventProcessingRWLock => _eventProcessingRWLock;

	    public EPServicesHA EpServicesHA => _epServicesHA;

	    public EPRuntime EpRuntime => _epRuntime;

	    public EventBeanService EventBeanService => _eventBeanService;

	    public EventBeanTypedEventFactory EventBeanTypedEventFactory => _eventBeanTypedEventFactory;

	    public EPRenderEventServiceImpl EventRenderer => _eventRenderer;

	    public EventSerdeFactory EventSerdeFactory => _eventSerdeFactory;

	    public EventTableIndexService EventTableIndexService => _eventTableIndexService;

	    public EventTypeAvroHandler EventTypeAvroHandler => _eventTypeAvroHandler;

	    public EventTypeFactory EventTypeFactory => _eventTypeFactory;

	    public EventTypeIdResolver EventTypeIdResolver => _eventTypeIdResolver;

	    public EventTypeRepositoryImpl EventTypeRepositoryBus => _eventTypeRepositoryBus;

	    public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory => _eventTypeResolvingBeanFactory;

	    public EventTypeSerdeRepository EventTypeSerdeRepository => _eventTypeSerdeRepository;

	    public EventTypeXMLXSDHandler EventTypeXMLXSDHandler => _eventTypeXMLXSDHandler;

	    public ExceptionHandlingService ExceptionHandlingService => _exceptionHandlingService;

	    public PathRegistry<string, ExpressionDeclItem> ExprDeclaredPathRegistry => _exprDeclaredPathRegistry;

	    public FilterBooleanExpressionFactory FilterBooleanExpressionFactory => _filterBooleanExpressionFactory;

	    public FilterServiceSPI FilterServiceSPI => _filterService;

	    public FilterService FilterService => _filterService;

	    public FilterSharedBoolExprRepository FilterSharedBoolExprRepository => _filterSharedBoolExprRepository;

	    public FilterSharedLookupableRepository FilterSharedLookupableRepository => _filterSharedLookupableRepository;

	    public InternalEventRouterImpl InternalEventRouter => _internalEventRouter;

	    public InternalEventRouteDest InternalEventRouteDest {
		    get => _internalEventRouteDest;
		    set => _internalEventRouteDest = value;
	    }

	    public RowRecogStatePoolRuntimeSvc RowRecogStatePoolEngineSvc => _rowRecogStatePoolEngineSvc;

	    public MetricReportingService MetricReportingService => _metricReportingService;

	    public MultiMatchHandlerFactory MultiMatchHandlerFactory => _multiMatchHandlerFactory;

	    public NamedWindowConsumerManagementService NamedWindowConsumerManagementService => _namedWindowConsumerManagementService;

	    public NamedWindowDispatchService NamedWindowDispatchService => _namedWindowDispatchService;

	    public NamedWindowFactoryService NamedWindowFactoryService => _namedWindowFactoryService;

	    public NamedWindowManagementService NamedWindowManagementService => _namedWindowManagementService;

	    public PathRegistry<string, ContextMetaData> ContextPathRegistry => _contextPathRegistry;

	    public PathRegistry<string, EventType> EventTypePathRegistry => _eventTypePathRegistry;

	    public PathRegistry<string, NamedWindowMetaData> NamedWindowPathRegistry => _namedWindowPathRegistry;

	    public PatternFactoryService PatternFactoryService => _patternFactoryService;

	    public PatternSubexpressionPoolRuntimeSvc PatternSubexpressionPoolRuntimeSvc => _patternSubexpressionPoolEngineSvc;

	    public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory => _resultSetProcessorHelperFactory;

	    public SchedulingService SchedulingService => _schedulingService;

	    public SchedulingServiceSPI SchedulingServiceSPI => _schedulingService;
	    
	    public PathRegistry<NameAndParamNum, ExpressionScriptProvided> ScriptPathRegistry => _scriptPathRegistry;

	    public ScriptCompiler ScriptCompiler => _scriptCompiler;

	    public StageRecoveryService StageRecoveryService => _stageRecoveryService;

	    public StatementLifecycleService StatementLifecycleService => _statementLifecycleService;

	    public StatementAgentInstanceLockFactory StatementAgentInstanceLockFactory => _statementAgentInstanceLockFactory;

	    public StatementResourceHolderBuilder StatementResourceHolderBuilder => _statementResourceHolderBuilder;

	    public TableExprEvaluatorContext TableExprEvaluatorContext => _tableExprEvaluatorContext;

	    public TableManagementService TableManagementService => _tableManagementService;

	    public PathRegistry<string, TableMetaData> TablePathRegistry => _tablePathRegistry;

	    public ThreadingService ThreadingService => _threadingService;

	    public TimeAbacus TimeAbacus => _timeAbacus;

	    public TimerService TimerService => _timerService;

	    public TimeSourceService TimeSourceService => _timeSourceService;

	    public VariableManagementService VariableManagementService => _variableManagementService;

	    public PathRegistry<string, VariableMetaData> VariablePathRegistry => _variablePathRegistry;

	    public ViewableActivatorFactory ViewableActivatorFactory => _viewableActivatorFactory;

	    public ViewFactoryService ViewFactoryService => _viewFactoryService;

	    public ViewServicePreviousFactory ViewServicePreviousFactory => _viewServicePreviousFactory;

	    public XMLFragmentEventTypeFactory XmlFragmentEventTypeFactory => _xmlFragmentEventTypeFactory;
	}
} // end of namespace

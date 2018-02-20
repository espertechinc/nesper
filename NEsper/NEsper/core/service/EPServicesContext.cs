///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.activator;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.core.deploy;
using com.espertech.esper.core.service.multimatch;
using com.espertech.esper.core.thread;
using com.espertech.esper.dataflow.core;
using com.espertech.esper.dispatch;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.db;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.pattern.pool;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.script;
using com.espertech.esper.timer;
using com.espertech.esper.view;
using com.espertech.esper.view.stream;

namespace com.espertech.esper.core.service
{
    /// <summary>
    /// Convenience class to hold implementations for all services.
    /// </summary>
    public sealed class EPServicesContext
    {
        // Supplied after construction to avoid circular dependency

        /// <summary>
        /// Constructor - sets up new set of services.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="engineURI">is the engine URI</param>
        /// <param name="schedulingService">service to get time and schedule callbacks</param>
        /// <param name="eventAdapterService">service to resolve event types</param>
        /// <param name="engineImportService">is engine imported static func packages and aggregation functions</param>
        /// <param name="engineSettingsService">provides engine settings</param>
        /// <param name="databaseConfigService">service to resolve a database name to database connection factory and configs</param>
        /// <param name="plugInViews">resolves view namespace and name to view factory class</param>
        /// <param name="statementLockFactory">creates statement-level locks</param>
        /// <param name="eventProcessingRWLock">is the engine lock for statement management</param>
        /// <param name="extensionServicesContext">marker interface allows adding additional services</param>
        /// <param name="engineEnvContext">is engine environment/directory information for use with adapters and external env</param>
        /// <param name="statementContextFactory">is the factory to use to create statement context objects</param>
        /// <param name="plugInPatternObjects">resolves plug-in pattern objects</param>
        /// <param name="timerService">is the timer service</param>
        /// <param name="filterService">the filter service</param>
        /// <param name="streamFactoryService">is hooking up filters to streams</param>
        /// <param name="namedWindowMgmtService">The named window MGMT service.</param>
        /// <param name="namedWindowDispatchService">The named window dispatch service.</param>
        /// <param name="variableService">provides access to variable values</param>
        /// <param name="tableService">The table service.</param>
        /// <param name="timeSourceService">time source provider class</param>
        /// <param name="valueAddEventService">handles Update events</param>
        /// <param name="metricsReportingService">for metric reporting</param>
        /// <param name="statementEventTypeRef">statement to event type reference holding</param>
        /// <param name="statementVariableRef">statement to variabke reference holding</param>
        /// <param name="configSnapshot">configuration snapshot</param>
        /// <param name="threadingServiceImpl">engine-level threading services</param>
        /// <param name="internalEventRouter">routing of events</param>
        /// <param name="statementIsolationService">maintains isolation information per statement</param>
        /// <param name="schedulingMgmtService">schedule management for statements</param>
        /// <param name="deploymentStateService">The deployment state service.</param>
        /// <param name="exceptionHandlingService">The exception handling service.</param>
        /// <param name="patternNodeFactory">The pattern node factory.</param>
        /// <param name="eventTypeIdGenerator">The event type id generator.</param>
        /// <param name="statementMetadataFactory">The statement metadata factory.</param>
        /// <param name="contextManagementService">The context management service.</param>
        /// <param name="patternSubexpressionPoolSvc">The pattern subexpression pool SVC.</param>
        /// <param name="matchRecognizeStatePoolEngineSvc">The match recognize state pool engine SVC.</param>
        /// <param name="dataFlowService">The data flow service.</param>
        /// <param name="exprDeclaredService">The expr declared service.</param>
        /// <param name="contextControllerFactoryFactorySvc">The context controller factory factory SVC.</param>
        /// <param name="contextManagerFactoryService">The context manager factory service.</param>
        /// <param name="epStatementFactory">The ep statement factory.</param>
        /// <param name="regexHandlerFactory">The regex handler factory.</param>
        /// <param name="viewableActivatorFactory">The viewable activator factory.</param>
        /// <param name="filterNonPropertyRegisteryService">The filter non property registery service.</param>
        /// <param name="resultSetProcessorHelperFactory">The result set processor helper factory.</param>
        /// <param name="viewServicePreviousFactory">The view service previous factory.</param>
        /// <param name="eventTableIndexService">The event table index service.</param>
        /// <param name="epRuntimeIsolatedFactory">The ep runtime isolated factory.</param>
        /// <param name="filterBooleanExpressionFactory">The filter boolean expression factory.</param>
        /// <param name="dataCacheFactory">The data cache factory.</param>
        /// <param name="multiMatchHandlerFactory">The multi match handler factory.</param>
        /// <param name="namedWindowConsumerMgmtService">The named window consumer MGMT service.</param>
        /// <param name="aggregationFactoryFactory"></param>
        /// <param name="scriptingService">The scripting service.</param>
        public EPServicesContext(
            IContainer container,
            string engineURI,
            SchedulingServiceSPI schedulingService,
            EventAdapterService eventAdapterService,
            EngineImportService engineImportService,
            EngineSettingsService engineSettingsService,
            DatabaseConfigService databaseConfigService,
            PluggableObjectCollection plugInViews,
            StatementLockFactory statementLockFactory,
            IReaderWriterLock eventProcessingRWLock,
            EngineLevelExtensionServicesContext extensionServicesContext,
            Directory engineEnvContext,
            StatementContextFactory statementContextFactory,
            PluggableObjectCollection plugInPatternObjects,
            TimerService timerService,
            FilterServiceSPI filterService,
            StreamFactoryService streamFactoryService,
            NamedWindowMgmtService namedWindowMgmtService,
            NamedWindowDispatchService namedWindowDispatchService,
            VariableService variableService,
            TableService tableService,
            TimeSourceService timeSourceService,
            ValueAddEventService valueAddEventService,
            MetricReportingServiceSPI metricsReportingService,
            StatementEventTypeRef statementEventTypeRef,
            StatementVariableRef statementVariableRef,
            ConfigurationInformation configSnapshot,
            ThreadingService threadingServiceImpl,
            InternalEventRouterImpl internalEventRouter,
            StatementIsolationService statementIsolationService,
            SchedulingMgmtService schedulingMgmtService,
            DeploymentStateService deploymentStateService,
            ExceptionHandlingService exceptionHandlingService,
            PatternNodeFactory patternNodeFactory,
            EventTypeIdGenerator eventTypeIdGenerator,
            StatementMetadataFactory statementMetadataFactory,
            ContextManagementService contextManagementService,
            PatternSubexpressionPoolEngineSvc patternSubexpressionPoolSvc,
            MatchRecognizeStatePoolEngineSvc matchRecognizeStatePoolEngineSvc,
            DataFlowService dataFlowService,
            ExprDeclaredService exprDeclaredService,
            ContextControllerFactoryFactorySvc contextControllerFactoryFactorySvc,
            ContextManagerFactoryService contextManagerFactoryService,
            EPStatementFactory epStatementFactory,
            RegexHandlerFactory regexHandlerFactory,
            ViewableActivatorFactory viewableActivatorFactory,
            FilterNonPropertyRegisteryService filterNonPropertyRegisteryService,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            ViewServicePreviousFactory viewServicePreviousFactory,
            EventTableIndexService eventTableIndexService,
            EPRuntimeIsolatedFactory epRuntimeIsolatedFactory,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            DataCacheFactory dataCacheFactory,
            MultiMatchHandlerFactory multiMatchHandlerFactory,
            NamedWindowConsumerMgmtService namedWindowConsumerMgmtService,
            AggregationFactoryFactory aggregationFactoryFactory,
            ScriptingService scriptingService)
        {
            Container = container;
            EngineURI = engineURI;
            SchedulingService = schedulingService;
            EventAdapterService = eventAdapterService;
            EngineImportService = engineImportService;
            EngineSettingsService = engineSettingsService;
            DatabaseRefService = databaseConfigService;
            FilterService = filterService;
            TimerService = timerService;
            DispatchService = DispatchServiceProvider.NewService(container.Resolve<IThreadLocalManager>());
            ViewService = ViewServiceProvider.NewService();
            StreamService = streamFactoryService;
            PlugInViews = plugInViews;
            StatementLockFactory = statementLockFactory;
            EventProcessingRWLock = eventProcessingRWLock;
            EngineLevelExtensionServicesContext = extensionServicesContext;
            EngineEnvContext = engineEnvContext;
            StatementContextFactory = statementContextFactory;
            PlugInPatternObjects = plugInPatternObjects;
            NamedWindowMgmtService = namedWindowMgmtService;
            NamedWindowDispatchService = namedWindowDispatchService;
            VariableService = variableService;
            TableService = tableService;
            TimeSource = timeSourceService;
            ValueAddEventService = valueAddEventService;
            MetricsReportingService = metricsReportingService;
            StatementEventTypeRefService = statementEventTypeRef;
            ConfigSnapshot = configSnapshot;
            ThreadingService = threadingServiceImpl;
            InternalEventRouter = internalEventRouter;
            StatementIsolationService = statementIsolationService;
            SchedulingMgmtService = schedulingMgmtService;
            StatementVariableRefService = statementVariableRef;
            DeploymentStateService = deploymentStateService;
            ExceptionHandlingService = exceptionHandlingService;
            PatternNodeFactory = patternNodeFactory;
            EventTypeIdGenerator = eventTypeIdGenerator;
            StatementMetadataFactory = statementMetadataFactory;
            ContextManagementService = contextManagementService;
            PatternSubexpressionPoolSvc = patternSubexpressionPoolSvc;
            MatchRecognizeStatePoolEngineSvc = matchRecognizeStatePoolEngineSvc;
            DataFlowService = dataFlowService;
            ExprDeclaredService = exprDeclaredService;
            ExpressionResultCacheSharable = new ExpressionResultCacheService(
                configSnapshot.EngineDefaults.Execution.DeclaredExprValueCacheSize, 
                container.Resolve<IThreadLocalManager>());
            ContextControllerFactoryFactorySvc = contextControllerFactoryFactorySvc;
            ContextManagerFactoryService = contextManagerFactoryService;
            EpStatementFactory = epStatementFactory;
            RegexHandlerFactory = regexHandlerFactory;
            ViewableActivatorFactory = viewableActivatorFactory;
            FilterNonPropertyRegisteryService = filterNonPropertyRegisteryService;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
            ViewServicePreviousFactory = viewServicePreviousFactory;
            EventTableIndexService = eventTableIndexService;
            EpRuntimeIsolatedFactory = epRuntimeIsolatedFactory;
            FilterBooleanExpressionFactory = filterBooleanExpressionFactory;
            DataCacheFactory = dataCacheFactory;
            MultiMatchHandlerFactory = multiMatchHandlerFactory;
            NamedWindowConsumerMgmtService = namedWindowConsumerMgmtService;
            AggregationFactoryFactory = aggregationFactoryFactory;
            ScriptingService = scriptingService;
        }

        /// <summary>
        /// Returns the service container.
        /// </summary>
        public IContainer Container { get; set; }

        public PatternNodeFactory PatternNodeFactory { get; private set; }

        /// <summary>Returns the event routing destination. </summary>
        /// <value>event routing destination</value>
        public InternalEventRouteDest InternalEventEngineRouteDest { get; set; }

        /// <summary>Returns router for internal event processing. </summary>
        /// <value>router for internal event processing</value>
        public InternalEventRouterImpl InternalEventRouter { get; private set; }

        /// <summary>Returns filter evaluation service implementation. </summary>
        /// <value>filter evaluation service</value>
        public FilterServiceSPI FilterService { get; private set; }

        /// <summary>Returns time provider service implementation. </summary>
        /// <value>time provider service</value>
        public TimerService TimerService { get; private set; }

        /// <summary>Returns scheduling service implementation. </summary>
        /// <value>scheduling service</value>
        public SchedulingServiceSPI SchedulingService { get; private set; }

        /// <summary>Returns dispatch service responsible for dispatching events to listeners. </summary>
        /// <value>dispatch service.</value>
        public DispatchService DispatchService { get; private set; }

        /// <summary>Returns services for view creation, sharing and removal. </summary>
        /// <value>view service</value>
        public ViewService ViewService { get; private set; }

        /// <summary>Returns stream service. </summary>
        /// <value>stream service</value>
        public StreamFactoryService StreamService { get; private set; }

        /// <summary>Returns event type resolution service. </summary>
        /// <value>service resolving event type</value>
        public EventAdapterService EventAdapterService { get; private set; }

        /// <summary>Returns the import and class name resolution service. </summary>
        /// <value>import service</value>
        public EngineImportService EngineImportService { get; private set; }

        /// <summary>Returns the database settings service. </summary>
        /// <value>database info service</value>
        public DatabaseConfigService DatabaseRefService { get; private set; }

        /// <summary>Information to resolve plug-in view namespace and name. </summary>
        /// <value>plug-in view information</value>
        public PluggableObjectCollection PlugInViews { get; private set; }

        /// <summary>Information to resolve plug-in pattern object namespace and name. </summary>
        /// <value>plug-in pattern object information</value>
        public PluggableObjectCollection PlugInPatternObjects { get; private set; }

        /// <summary>Factory for statement-level locks. </summary>
        /// <value>factory</value>
        public StatementLockFactory StatementLockFactory { get; private set; }

        /// <summary>Returns the event processing lock for coordinating statement administration with event processing. </summary>
        /// <value>lock</value>
        public IReaderWriterLock EventProcessingRWLock { get; private set; }

        /// <summary>Returns statement lifecycle svc </summary>
        /// <value>service for statement start and stop</value>
        public StatementLifecycleSvc StatementLifecycleSvc { get; internal set; }

        /// <summary>Returns extension service for adding custom the services. </summary>
        /// <value>extension service context</value>
        public EngineLevelExtensionServicesContext EngineLevelExtensionServicesContext { get; private set; }

        /// <summary>Returns the engine environment context for getting access to engine-external resources, such as adapters </summary>
        /// <value>engine environment context</value>
        public Directory EngineEnvContext { get; private set; }

        /// <summary>Returns engine-level threading settings. </summary>
        /// <value>threading service</value>
        public ThreadingService ThreadingService { get; private set; }

        /// <summary>Returns the factory to use for creating a statement context.</summary>
        /// <value>statement context factory</value>
        public StatementContextFactory StatementContextFactory { get; private set; }

        /// <summary>Returns the engine URI. </summary>
        /// <value>engine URI</value>
        public string EngineURI { get; private set; }

        /// <summary>Returns engine settings. </summary>
        /// <value>settings</value>
        public EngineSettingsService EngineSettingsService { get; private set; }

        /// <summary>Returns the named window management service. </summary>
        /// <value>service for managing named windows</value>
        public NamedWindowMgmtService NamedWindowMgmtService { get; private set; }

        /// <summary>Gets the named window dispatch service.</summary>
        /// <value>The named window dispatch service.</value>
        public NamedWindowDispatchService NamedWindowDispatchService { get; private set; }

        /// <summary>Returns the variable service. </summary>
        /// <value>variable service</value>
        public VariableService VariableService { get; private set; }

        /// <summary>Returns the time source provider class. </summary>
        /// <value>time source</value>
        public TimeSourceService TimeSource { get; private set; }

        /// <summary>Returns the service for handling updates to events. </summary>
        /// <value>revision service</value>
        public ValueAddEventService ValueAddEventService { get; private set; }

        /// <summary>Returns metrics reporting. </summary>
        /// <value>metrics reporting</value>
        public MetricReportingServiceSPI MetricsReportingService { get; private set; }

        /// <summary>Returns service for statement to event type mapping. </summary>
        /// <value>statement-type mapping</value>
        public StatementEventTypeRef StatementEventTypeRefService { get; private set; }

        /// <summary>Returns the configuration. </summary>
        /// <value>configuration</value>
        public ConfigurationInformation ConfigSnapshot { get; private set; }

        /// <summary>Service for keeping track of variable-statement use. </summary>
        /// <value>svc</value>
        public StatementVariableRef StatementVariableRefService { get; private set; }

        /// <summary>Returns the schedule management service. </summary>
        /// <value>schedule management service</value>
        public SchedulingMgmtService SchedulingMgmtService { get; private set; }

        /// <summary>Returns the service for maintaining statement isolation information. </summary>
        /// <value>isolation service</value>
        public StatementIsolationService StatementIsolationService { get; set; }

        public DeploymentStateService DeploymentStateService { get; private set; }

        public ExceptionHandlingService ExceptionHandlingService { get; private set; }

        public EventTypeIdGenerator EventTypeIdGenerator { get; private set; }

        public StatementMetadataFactory StatementMetadataFactory { get; private set; }

        public ContextManagementService ContextManagementService { get; private set; }

        public PatternSubexpressionPoolEngineSvc PatternSubexpressionPoolSvc { get; private set; }

        public MatchRecognizeStatePoolEngineSvc MatchRecognizeStatePoolEngineSvc { get; private set; }

        public DataFlowService DataFlowService { get; private set; }

        public ExprDeclaredService ExprDeclaredService { get; private set; }

        public ExpressionResultCacheService ExpressionResultCacheSharable { get; private set; }

        public ScriptingService ScriptingService { get; private set; }

        public TableService TableService { get; private set; }

        public ContextControllerFactoryFactorySvc ContextControllerFactoryFactorySvc { get; private set; }

        public EPStatementFactory EpStatementFactory { get; private set; }

        public ContextManagerFactoryService ContextManagerFactoryService { get; private set; }

        public RegexHandlerFactory RegexHandlerFactory { get; private set; }

        public ViewableActivatorFactory ViewableActivatorFactory { get; private set; }

        public FilterNonPropertyRegisteryService FilterNonPropertyRegisteryService { get; private set; }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; private set; }

        public ViewServicePreviousFactory ViewServicePreviousFactory { get; private set; }

        public EventTableIndexService EventTableIndexService { get; private set; }

        public EPRuntimeIsolatedFactory EpRuntimeIsolatedFactory { get; private set; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; private set; }

        public DataCacheFactory DataCacheFactory { get; private set; }

        public MultiMatchHandlerFactory MultiMatchHandlerFactory { get; private set; }

        public NamedWindowConsumerMgmtService NamedWindowConsumerMgmtService { get; private set; }

        public AggregationFactoryFactory AggregationFactoryFactory { get; private set; }

        public IThreadLocalManager ThreadLocalManager {
            get => Container.Resolve<IThreadLocalManager>();
        }

        public ILockManager LockManager {
            get => Container.Resolve<ILockManager>();
        }

        public IReaderWriterLockManager RWLockManager {
            get => Container.Resolve<IReaderWriterLockManager>();
        }

        public IResourceManager ResourceManager {
            get => Container.Resolve<IResourceManager>();
        }

        /// <summary>Sets the service dealing with starting and stopping statements. </summary>
        /// <param name="statementLifecycleSvc">statement lifycycle svc</param>
        public void SetStatementLifecycleSvc(StatementLifecycleSvc statementLifecycleSvc)
        {
            StatementLifecycleSvc = statementLifecycleSvc;
        }

        /// <summary>Dispose services. </summary>
        public void Dispose()
        {
            if (ScriptingService != null)
            {
                ScriptingService.Dispose();
            }
            if (ExprDeclaredService != null)
            {
                ExprDeclaredService.Dispose();
            }
            if (DataFlowService != null)
            {
                DataFlowService.Dispose();
            }
            if (VariableService != null)
            {
                VariableService.Dispose();
            }
            if (MetricsReportingService != null)
            {
                MetricsReportingService.Dispose();
            }
            if (ThreadingService != null)
            {
                ThreadingService.Dispose();
            }
            if (StatementLifecycleSvc != null)
            {
                StatementLifecycleSvc.Dispose();
            }
            if (FilterService != null)
            {
                FilterService.Dispose();
            }
            if (SchedulingService != null)
            {
                SchedulingService.Dispose();
            }
            if (SchedulingMgmtService != null)
            {
                SchedulingMgmtService.Dispose();
            }
            if (StreamService != null)
            {
                StreamService.Destroy();
            }
            if (NamedWindowMgmtService != null)
            {
                NamedWindowMgmtService.Dispose();
            }
            if (NamedWindowDispatchService != null)
            {
                NamedWindowDispatchService.Dispose();
            }
            if (EngineLevelExtensionServicesContext != null)
            {
                EngineLevelExtensionServicesContext.Dispose();
            }
            if (StatementIsolationService != null)
            {
                StatementIsolationService.Dispose();
            }
            if (DeploymentStateService != null)
            {
                DeploymentStateService.Dispose();
            }
        }

        /// <summary>Dispose services. </summary>
        public void Initialize()
        {
            ScriptingService = null;
            StatementLifecycleSvc = null;
            EngineURI = null;
            SchedulingService = null;
            EventAdapterService = null;
            EngineImportService = null;
            EngineSettingsService = null;
            DatabaseRefService = null;
            FilterService = null;
            TimerService = null;
            DispatchService = null;
            ViewService = null;
            StreamService = null;
            PlugInViews = null;
            StatementLockFactory = null;
            EngineLevelExtensionServicesContext = null;
            EngineEnvContext = null;
            StatementContextFactory = null;
            PlugInPatternObjects = null;
            NamedWindowMgmtService = null;
            ValueAddEventService = null;
            MetricsReportingService = null;
            StatementEventTypeRefService = null;
            ThreadingService = null;
            ExpressionResultCacheSharable = null;
            Container = null;
        }
    }
}
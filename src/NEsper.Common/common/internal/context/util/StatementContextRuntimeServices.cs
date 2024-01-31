///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.hook.expr;
using com.espertech.esper.common.client.render;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.dataflow.core;
using com.espertech.esper.common.@internal.epl.dataflow.filtersvcadapter;
using com.espertech.esper.common.@internal.epl.enummethod.cache;
using com.espertech.esper.common.@internal.epl.historical.database.connection;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.@event.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;
using com.espertech.esper.compat.directory;
using com.espertech.esper.compat.threading.locks;
using com.espertech.esper.container;

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementContextRuntimeServices
    {
        public StatementContextRuntimeServices(
            IContainer container,
            Configuration configSnapshot,
            ContextManagementService contextManagementService,
            PathRegistry<string, ContextMetaData> contextPathRegistry,
            ContextServiceFactory contextServiceFactory,
            DatabaseConfigServiceRuntime databaseConfigService,
            DataFlowFilterServiceAdapter dataFlowFilterServiceAdapter,
            EPDataFlowServiceImpl dataflowService,
            string runtimeURI,
            INamingContext runtimeEnvContext,
            ImportServiceRuntime importServiceRuntime,
            RuntimeSettingsService runtimeSettingsService,
            RuntimeExtensionServices runtimeExtensionServices,
            object epRuntime,
            EPRenderEventService epRuntimeRenderEvent,
            EventServiceSendEventCommon eventServiceSendEventInternal,
            EPRuntimeEventProcessWrapped epRuntimeEventProcessWrapped,
            EventBeanService eventBeanService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            EventTableIndexService eventTableIndexService,
            EventTypeAvroHandler eventTypeAvroHandler,
            PathRegistry<string, EventType> eventTypePathRegistry,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            EventTypeResolvingBeanFactory eventTypeResolvingBeanFactory,
            IReaderWriterLock eventProcessingRWLock,
            ExceptionHandlingService exceptionHandlingService,
            ExpressionResultCacheService expressionResultCacheService,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            FilterSharedBoolExprRepository filterSharedBoolExprRepository,
            FilterSharedLookupableRepository filterSharedLookupableRepository,
            HistoricalDataCacheFactory historicalDataCacheFactory,
            InternalEventRouter internalEventRouter,
            MetricReportingService metricReportingService,
            NamedWindowConsumerManagementService namedWindowConsumerManagementService,
            NamedWindowManagementService namedWindowManagementService,
            PathRegistry<string, ContextMetaData> pathContextRegistry,
            PathRegistry<string, NamedWindowMetaData> pathNamedWindowRegistry,
            RowRecogStateRepoFactory rowRecogStateRepoFactory,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory,
            SchedulingService schedulingService,
            StatementAgentInstanceLockFactory statementAgentInstanceLockFactory,
            StatementResourceHolderBuilder statementResourceHolderBuilder,
            TableExprEvaluatorContext tableExprEvaluatorContext,
            TableManagementService tableManagementService,
            VariableManagementService variableManagementService,
            ViewFactoryService viewFactoryService,
            ViewServicePreviousFactory viewServicePreviousFactory)
        {
            Container = container;
            ConfigSnapshot = configSnapshot;
            ContextManagementService = contextManagementService;
            ContextPathRegistry = contextPathRegistry;
            ContextServiceFactory = contextServiceFactory;
            DatabaseConfigService = databaseConfigService;
            DataFlowFilterServiceAdapter = dataFlowFilterServiceAdapter;
            DataflowService = dataflowService;
            RuntimeURI = runtimeURI;
            RuntimeEnvContext = runtimeEnvContext;
            ImportServiceRuntime = importServiceRuntime;
            RuntimeSettingsService = runtimeSettingsService;
            RuntimeExtensionServices = runtimeExtensionServices;
            Runtime = epRuntime;
            EPRuntimeRenderEvent = epRuntimeRenderEvent;
            EventServiceSendEventInternal = eventServiceSendEventInternal;
            EPRuntimeEventProcessWrapped = epRuntimeEventProcessWrapped;
            EventBeanService = eventBeanService;
            EventBeanTypedEventFactory = eventBeanTypedEventFactory;
            EventTableIndexService = eventTableIndexService;
            EventTypeAvroHandler = eventTypeAvroHandler;
            EventTypePathRegistry = eventTypePathRegistry;
            EventTypeRepositoryPreconfigured = eventTypeRepositoryPreconfigured;
            EventTypeResolvingBeanFactory = eventTypeResolvingBeanFactory;
            EventProcessingRWLock = eventProcessingRWLock;
            ExceptionHandlingService = exceptionHandlingService;
            ExpressionResultCacheService = expressionResultCacheService;
            FilterBooleanExpressionFactory = filterBooleanExpressionFactory;
            FilterSharedBoolExprRepository = filterSharedBoolExprRepository;
            FilterSharedLookupableRepository = filterSharedLookupableRepository;
            HistoricalDataCacheFactory = historicalDataCacheFactory;
            InternalEventRouter = internalEventRouter;
            MetricReportingService = metricReportingService;
            NamedWindowConsumerManagementService = namedWindowConsumerManagementService;
            NamedWindowManagementService = namedWindowManagementService;
            PathContextRegistry = pathContextRegistry;
            PathNamedWindowRegistry = pathNamedWindowRegistry;
            RowRecogStateRepoFactory = rowRecogStateRepoFactory;
            ResultSetProcessorHelperFactory = resultSetProcessorHelperFactory;
            SchedulingService = schedulingService;
            StatementAgentInstanceLockFactory = statementAgentInstanceLockFactory;
            StatementResourceHolderBuilder = statementResourceHolderBuilder;
            TableExprEvaluatorContext = tableExprEvaluatorContext;
            TableManagementService = tableManagementService;
            VariableManagementService = variableManagementService;
            ViewFactoryService = viewFactoryService;
            ViewServicePreviousFactory = viewServicePreviousFactory;
        }

        public StatementContextRuntimeServices(IContainer container)
        {
            Container = container;
            ConfigSnapshot = null;
            ContextManagementService = null;
            ContextPathRegistry = null;
            ContextServiceFactory = null;
            DatabaseConfigService = null;
            DataFlowFilterServiceAdapter = null;
            DataflowService = null;
            RuntimeURI = null;
            RuntimeEnvContext = null;
            ImportServiceRuntime = null;
            RuntimeSettingsService = null;
            RuntimeExtensionServices = null;
            Runtime = null;
            EPRuntimeRenderEvent = null;
            EventServiceSendEventInternal = null;
            EPRuntimeEventProcessWrapped = null;
            EventBeanService = null;
            EventBeanTypedEventFactory = null;
            EventTableIndexService = null;
            EventTypeAvroHandler = null;
            EventTypePathRegistry = null;
            EventTypeRepositoryPreconfigured = null;
            EventTypeResolvingBeanFactory = null;
            EventProcessingRWLock = null;
            ExceptionHandlingService = null;
            ExpressionResultCacheService = null;
            FilterBooleanExpressionFactory = null;
            FilterSharedBoolExprRepository = null;
            FilterSharedLookupableRepository = null;
            HistoricalDataCacheFactory = null;
            InternalEventRouter = null;
            MetricReportingService = null;
            NamedWindowConsumerManagementService = null;
            NamedWindowManagementService = null;
            PathContextRegistry = null;
            PathNamedWindowRegistry = null;
            RowRecogStateRepoFactory = null;
            ResultSetProcessorHelperFactory = null;
            SchedulingService = null;
            StatementAgentInstanceLockFactory = null;
            StatementResourceHolderBuilder = null;
            TableExprEvaluatorContext = null;
            TableManagementService = null;
            VariableManagementService = null;
            ViewFactoryService = null;
            ViewServicePreviousFactory = null;
        }

        public IContainer Container { get; }
        public Configuration ConfigSnapshot { get; }

        public ContextManagementService ContextManagementService { get; }

        public ContextServiceFactory ContextServiceFactory { get; }

        public DatabaseConfigServiceRuntime DatabaseConfigService { get; }

        public string RuntimeURI { get; }

        public RuntimeExtensionServices RuntimeExtensionServices { get; }

        public ImportServiceRuntime ImportServiceRuntime { get; }

        public RuntimeSettingsService RuntimeSettingsService { get; }

        public EventServiceSendEventCommon EventServiceSendEventInternal { get; }

        public EPRuntimeEventProcessWrapped EPRuntimeEventProcessWrapped { get; }

        public object Runtime { get; }

        public EventTableIndexService EventTableIndexService { get; }

        public PathRegistry<string, EventType> EventTypePathRegistry { get; }

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory { get; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public EventBeanService EventBeanService { get; }

        public EventTypeAvroHandler EventTypeAvroHandler { get; }

        public ExceptionHandlingService ExceptionHandlingService { get; }

        public ExpressionResultCacheService ExpressionResultCacheService { get; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; }

        public FilterSharedBoolExprRepository FilterSharedBoolExprRepository { get; }

        public FilterSharedLookupableRepository FilterSharedLookupableRepository { get; }

        public HistoricalDataCacheFactory HistoricalDataCacheFactory { get; }

        public InternalEventRouter InternalEventRouter { get; }

        public NamedWindowConsumerManagementService NamedWindowConsumerManagementService { get; }

        public NamedWindowManagementService NamedWindowManagementService { get; }

        public PathRegistry<string, ContextMetaData> PathContextRegistry { get; }

        public PathRegistry<string, NamedWindowMetaData> PathNamedWindowRegistry { get; }

        public RowRecogStateRepoFactory RowRecogStateRepoFactory { get; }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; }

        public StatementAgentInstanceLockFactory StatementAgentInstanceLockFactory { get; }

        public StatementResourceHolderBuilder StatementResourceHolderBuilder { get; }

        public ViewServicePreviousFactory ViewServicePreviousFactory { get; }

        public ViewFactoryService ViewFactoryService { get; }

        public EventTypeRepositoryImpl EventTypeRepositoryPreconfigured { get; }

        public VariableManagementService VariableManagementService { get; }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; }

        public TableManagementService TableManagementService { get; }

        public EPDataFlowServiceImpl DataflowService { get; }

        public EventServiceSendEventCommon EPRuntimeSendEvent => EventServiceSendEventInternal;

        public EPRenderEventService EPRuntimeRenderEvent { get; }

        public DataFlowFilterServiceAdapter DataFlowFilterServiceAdapter { get; }

        public MetricReportingService MetricReportingService { get; }

        public INamingContext RuntimeEnvContext { get; }

        public SchedulingService SchedulingService { get; }

        public PathRegistry<string, ContextMetaData> ContextPathRegistry { get; }

        public IReaderWriterLock EventProcessingRWLock { get; }
    }
} // end of namespace
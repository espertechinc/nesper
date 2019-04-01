///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Runtime.Remoting.Contexts;
using com.espertech.esper.common.client;
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
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.statement.resource;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.common.@internal.view.previous;

namespace com.espertech.esper.common.@internal.context.util
{
    public class StatementContextRuntimeServices
    {
        public StatementContextRuntimeServices(
            ContextManagementService contextManagementService, ContextServiceFactory contextServiceFactory,
            DatabaseConfigServiceRuntime databaseConfigService,
            DataFlowFilterServiceAdapter dataFlowFilterServiceAdapter, EPDataFlowServiceImpl dataflowService,
            string runtimeURI, Context runtimeEnvContext, ImportServiceRuntime importServiceRuntime,
            RuntimeSettingsService runtimeSettingsService, RuntimeExtensionServices runtimeExtensionServices,
            object epRuntime, EPRenderEventService epRuntimeRenderEvent,
            EventServiceSendEventCommon eventServiceSendEventInternal,
            EPRuntimeEventProcessWrapped epRuntimeEventProcessWrapped, EventBeanService eventBeanService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory, EventTableIndexService eventTableIndexService,
            EventTypeAvroHandler eventTypeAvroHandler, PathRegistry<string, EventType> eventTypePathRegistry,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            EventTypeResolvingBeanFactory eventTypeResolvingBeanFactory,
            ExceptionHandlingService exceptionHandlingService,
            ExpressionResultCacheService expressionResultCacheService, FilterService filterService,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            FilterSharedBoolExprRepository filterSharedBoolExprRepository,
            FilterSharedLookupableRepository filterSharedLookupableRepository,
            HistoricalDataCacheFactory historicalDataCacheFactory, InternalEventRouter internalEventRouter,
            InternalEventRouteDest internalEventRouteDest, MetricReportingService metricReportingService,
            NamedWindowConsumerManagementService namedWindowConsumerManagementService,
            NamedWindowManagementService namedWindowManagementService,
            PathRegistry<string, ContextMetaData> pathContextRegistry,
            PathRegistry<string, NamedWindowMetaData> pathNamedWindowRegistry,
            RowRecogStateRepoFactory rowRecogStateRepoFactory,
            ResultSetProcessorHelperFactory resultSetProcessorHelperFactory, SchedulingService schedulingService,
            StatementAgentInstanceLockFactory statementAgentInstanceLockFactory,
            StatementResourceHolderBuilder statementResourceHolderBuilder,
            TableExprEvaluatorContext tableExprEvaluatorContext, TableManagementService tableManagementService,
            VariableManagementService variableManagementService, ViewFactoryService viewFactoryService,
            ViewServicePreviousFactory viewServicePreviousFactory)
        {
            ContextManagementService = contextManagementService;
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
            ExceptionHandlingService = exceptionHandlingService;
            ExpressionResultCacheService = expressionResultCacheService;
            FilterService = filterService;
            FilterBooleanExpressionFactory = filterBooleanExpressionFactory;
            FilterSharedBoolExprRepository = filterSharedBoolExprRepository;
            FilterSharedLookupableRepository = filterSharedLookupableRepository;
            HistoricalDataCacheFactory = historicalDataCacheFactory;
            InternalEventRouter = internalEventRouter;
            InternalEventRouteDest = internalEventRouteDest;
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

        public EventTypeResolvingBeanFactory EventTypeResolvingBeanFactory { get; }

        public EventBeanTypedEventFactory EventBeanTypedEventFactory { get; }

        public EventBeanService EventBeanService { get; }

        public EventTypeAvroHandler EventTypeAvroHandler { get; }

        public ExceptionHandlingService ExceptionHandlingService { get; }

        public ExpressionResultCacheService ExpressionResultCacheService { get; }

        public FilterService FilterService { get; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; }

        public FilterSharedBoolExprRepository FilterSharedBoolExprRepository { get; }

        public FilterSharedLookupableRepository FilterSharedLookupableRepository { get; }

        public HistoricalDataCacheFactory HistoricalDataCacheFactory { get; }

        public InternalEventRouter InternalEventRouter { get; }

        public InternalEventRouteDest InternalEventRouteDest { get; }

        public NamedWindowConsumerManagementService NamedWindowConsumerManagementService { get; }

        public NamedWindowManagementService NamedWindowManagementService { get; }

        public RowRecogStateRepoFactory RowRecogStateRepoFactory { get; }

        public ResultSetProcessorHelperFactory ResultSetProcessorHelperFactory { get; }

        public SchedulingService SchedulingService { get; }

        public StatementAgentInstanceLockFactory StatementAgentInstanceLockFactory { get; }

        public StatementResourceHolderBuilder StatementResourceHolderBuilder { get; }

        public TimeProvider TimeProvider => SchedulingService;

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

        public Context RuntimeEnvContext { get; }

        public PathRegistry<string, EventType> EventTypePathRegistry { get; }

        public PathRegistry<string, ContextMetaData> PathContextRegistry { get; }

        public PathRegistry<string, NamedWindowMetaData> PathNamedWindowRegistry { get; }
    }
} // end of namespace
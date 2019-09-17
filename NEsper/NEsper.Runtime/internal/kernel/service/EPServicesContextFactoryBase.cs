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
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.hook.condition;
using com.espertech.esper.common.client.hook.exception;
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.@internal.deploymentlifesvc;
using com.espertech.esper.runtime.@internal.filtersvcimpl;
using com.espertech.esper.runtime.@internal.kernel.statement;
using com.espertech.esper.runtime.@internal.kernel.thread;
using com.espertech.esper.runtime.@internal.metrics.stmtmetrics;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;
using com.espertech.esper.runtime.@internal.timer;

using static com.espertech.esper.common.@internal.context.util.StatementCPCacheService;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public abstract class EPServicesContextFactoryBase : EPServicesContextFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(EPServicesContextFactoryDefault));

        public abstract EPEventServiceImpl CreateEPRuntime(
            EPServicesContext services,
            AtomicBoolean serviceStatusProvider);

        public EPServicesContext CreateServicesContext(
            EPRuntimeSPI epRuntime,
            Configuration configs)
        {
            var container = epRuntime.Container;
            var runtimeEnvContext = new RuntimeEnvContext();
            var eventProcessingRWLock = new ManagedReadWriteLock("EventProcLock", false);
            var deploymentLifecycleService = new DeploymentLifecycleServiceImpl();

            var runtimeSettingsService = MakeRuntimeSettingsService(configs);

            var timeAbacus = TimeAbacusFactory.Make(configs.Common.TimeSource.TimeUnit);
            var timeZone = configs.Runtime.Expression.TimeZone ?? TimeZoneInfo.Utc;
            var importServiceRuntime = new ImportServiceRuntime(
                container,
                configs.Common.TransientConfiguration, timeAbacus,
                configs.Common.EventTypeAutoNameNamespaces, timeZone,
                configs.Common.MethodInvocationReferences,
                configs.Common.Imports,
                configs.Common.AnnotationImports);

            var epServicesHA = InitHA(epRuntime.URI, configs, runtimeEnvContext, eventProcessingRWLock, runtimeSettingsService);

            var eventTypeAvroHandler = MakeEventTypeAvroHandler(
                importServiceRuntime, configs.Common.EventMeta.AvroSettings, epServicesHA.RuntimeExtensionServices);
            var resolvedBeanEventTypes = BeanEventTypeRepoUtil.ResolveBeanEventTypes(configs.Common.EventTypeNames, importServiceRuntime);
            var eventBeanTypedEventFactory = MakeEventBeanTypedEventFactory(eventTypeAvroHandler);
            var beanEventTypeStemService =
                BeanEventTypeRepoUtil.MakeBeanEventTypeStemService(configs, resolvedBeanEventTypes, eventBeanTypedEventFactory);
            var eventTypeRepositoryPreconfigured = new EventTypeRepositoryImpl(false);
            var eventTypeFactory = MakeEventTypeFactory(
                epServicesHA.RuntimeExtensionServices, eventTypeRepositoryPreconfigured, deploymentLifecycleService);
            var beanEventTypeFactoryPrivate = new BeanEventTypeFactoryPrivate(eventBeanTypedEventFactory, eventTypeFactory, beanEventTypeStemService);
            EventTypeRepositoryBeanTypeUtil.BuildBeanTypes(
                beanEventTypeStemService, eventTypeRepositoryPreconfigured, resolvedBeanEventTypes, beanEventTypeFactoryPrivate,
                configs.Common.EventTypesBean);
            EventTypeRepositoryMapTypeUtil.BuildMapTypes(
                eventTypeRepositoryPreconfigured, configs.Common.MapTypeConfigurations, configs.Common.EventTypesMapEvents,
                configs.Common.EventTypesNestableMapEvents, beanEventTypeFactoryPrivate, importServiceRuntime);
            EventTypeRepositoryOATypeUtil.BuildOATypes(
                eventTypeRepositoryPreconfigured, configs.Common.ObjectArrayTypeConfigurations, configs.Common.EventTypesNestableObjectArrayEvents,
                beanEventTypeFactoryPrivate, importServiceRuntime);
            var xmlFragmentEventTypeFactory = new XMLFragmentEventTypeFactory(beanEventTypeFactoryPrivate, null, eventTypeRepositoryPreconfigured);
            EventTypeRepositoryXMLTypeUtil.BuildXMLTypes(
                eventTypeRepositoryPreconfigured,
                configs.Common.EventTypesXMLDOM,
                beanEventTypeFactoryPrivate,
                xmlFragmentEventTypeFactory,
                container.ResourceManager());
                //importServiceRuntime
            EventTypeRepositoryAvroTypeUtil.BuildAvroTypes(
                eventTypeRepositoryPreconfigured, configs.Common.EventTypesAvro, eventTypeAvroHandler,
                beanEventTypeFactoryPrivate.EventBeanTypedEventFactory);
            EventTypeRepositoryVariantStreamUtil.BuildVariantStreams(
                eventTypeRepositoryPreconfigured, configs.Common.VariantStreams, eventTypeFactory);

            var eventTypeResolvingBeanFactory = MakeEventTypeResolvingBeanFactory(eventTypeRepositoryPreconfigured, eventTypeAvroHandler);

            var viewableActivatorFactory = InitViewableActivatorFactory();

            var statementLifecycleService = new StatementLifecycleServiceImpl();

            EventTypeIdResolver idResolver = new ProxyEventTypeIdResolver {
                ProcGetTypeById = (
                    eventTypeIdPublic,
                    eventTypeIdProtected) => {
                    if (eventTypeIdProtected == -1) {
                        return eventTypeRepositoryPreconfigured.GetTypeById(eventTypeIdPublic);
                    }

                    var deployerResult = deploymentLifecycleService.GetDeploymentByCRC(eventTypeIdPublic);
                    return deployerResult.DeploymentTypes.Get(eventTypeIdProtected);
                }
            };
            var filterSharedBoolExprRepository = MakeFilterSharedBoolExprRepository();
            var filterSharedLookupableRepository = MakeFilterSharedLookupableRepository();
            var filterServiceSPI = MakeFilterService(
                epServicesHA.RuntimeExtensionServices, eventTypeRepositoryPreconfigured, statementLifecycleService, runtimeSettingsService,
                idResolver, filterSharedLookupableRepository);
            var filterBooleanExpressionFactory = MakeFilterBooleanExpressionFactory(statementLifecycleService);

            var statementResourceHolderBuilder = MakeStatementResourceHolderBuilder();

            var aggregationServiceFactoryService = MakeAggregationServiceFactoryService(epServicesHA.RuntimeExtensionServices);

            var viewFactoryService = MakeViewFactoryService();
            var patternFactoryService = MakePatternFactoryService();

            var exceptionHandlingService = InitExceptionHandling(
                epRuntime.URI, configs.Runtime.ExceptionHandling, configs.Runtime.ConditionHandling, ClassForNameProviderDefault.INSTANCE);

            var timeSourceService = MakeTimeSource(configs);
            var schedulingService = MakeSchedulingService(
                epServicesHA, timeSourceService, epServicesHA.RuntimeExtensionServices, runtimeSettingsService, statementLifecycleService);

            var internalEventRouter = new InternalEventRouterImpl(eventBeanTypedEventFactory);

            var multiMatchHandlerFactory = MakeMultiMatchHandlerFactory(configs);

            var dispatchService = new DispatchService();
            var contextServiceFactory = MakeContextServiceFactory(epServicesHA.RuntimeExtensionServices);
            ContextManagementService contextManagementService = new ContextManagementServiceImpl();

            var viewServicePreviousFactory = MakeViewServicePreviousFactory(epServicesHA.RuntimeExtensionServices);

            var dataInputOutputSerdeProvider = MakeSerdeProvider(epServicesHA.RuntimeExtensionServices);

            var epStatementFactory = MakeEPStatementFactory();

            var msecTimerResolution = configs.Runtime.Threading.InternalTimerMsecResolution;
            if (msecTimerResolution <= 0) {
                throw new ConfigurationException("Timer resolution configuration not set to a valid value, expecting a non-zero value");
            }

            TimerService timerService = new TimerServiceImpl(epRuntime.URI, msecTimerResolution);
            StatementAgentInstanceLockFactory statementAgentInstanceLockFactory = new StatementAgentInstanceLockFactoryImpl(
                configs.Runtime.Execution.IsFairlock, configs.Runtime.Execution.IsDisableLocking);

            var eventTableIndexService = MakeEventTableIndexService(epServicesHA.RuntimeExtensionServices);
            var expressionResultCacheSharable = new ExpressionResultCacheService(
                configs.Runtime.Execution.DeclaredExprValueCacheSize,
                epRuntime.Container.ThreadLocalManager());

            var resultSetProcessorHelperFactory = MakeResultSetProcessorHelperFactory(epServicesHA.RuntimeExtensionServices);

            var variableRepositoryPreconfigured = new VariableRepositoryPreconfigured();
            VariableUtil.ConfigureVariables(
                variableRepositoryPreconfigured, configs.Common.Variables, importServiceRuntime, eventBeanTypedEventFactory,
                eventTypeRepositoryPreconfigured, beanEventTypeFactoryPrivate);
            var variableManagementService = MakeVariableManagementService(
                configs, schedulingService, eventBeanTypedEventFactory, runtimeSettingsService, epServicesHA);
            foreach (var publicVariable in variableRepositoryPreconfigured.Metadata) {
                variableManagementService.AddVariable(null, publicVariable.Value, null);
                variableManagementService.AllocateVariableState(
                    null, publicVariable.Key, DEFAULT_AGENT_INSTANCE_ID, false, null, eventBeanTypedEventFactory);
            }

            var variablePathRegistry = new PathRegistry<string, VariableMetaData>(PathRegistryObjectType.VARIABLE);

            var tableExprEvaluatorContext = new TableExprEvaluatorContext(
                epRuntime.Container.ThreadLocalManager());
            var tableManagementService = MakeTableManagementService(epServicesHA.RuntimeExtensionServices, tableExprEvaluatorContext);
            var tablePathRegistry = new PathRegistry<string, TableMetaData>(PathRegistryObjectType.TABLE);

            var metricsReporting = new MetricReportingServiceImpl(configs.Runtime.MetricsReporting, epRuntime.URI);

            var namedWindowFactoryService = MakeNamedWindowFactoryService();
            var namedWindowDispatchService = MakeNamedWindowDispatchService(
                schedulingService, configs, eventProcessingRWLock, exceptionHandlingService, variableManagementService, tableManagementService,
                metricsReporting);
            NamedWindowManagementService namedWindowManagementService = new NamedWindowManagementServiceImpl();
            var namedWindowConsumerManagementService = MakeNamedWindowConsumerManagementService(namedWindowManagementService);

            var pathNamedWindowRegistry = new PathRegistry<string, NamedWindowMetaData>(PathRegistryObjectType.NAMEDWINDOW);
            var eventTypePathRegistry = new PathRegistry<string, EventType>(PathRegistryObjectType.EVENTTYPE);
            var pathContextRegistry = new PathRegistry<string, ContextMetaData>(PathRegistryObjectType.CONTEXT);
            EventBeanService eventBeanService = new EventBeanServiceImpl(
                eventTypeRepositoryPreconfigured, eventTypePathRegistry, eventBeanTypedEventFactory);

            PatternSubexpressionPoolRuntimeSvc patternSubexpressionPoolSvc;
            if (configs.Runtime.Patterns.MaxSubexpressions != null) {
                patternSubexpressionPoolSvc = new PatternSubexpressionPoolRuntimeSvcImpl(
                    configs.Runtime.Patterns.MaxSubexpressions.Value,
                    configs.Runtime.Patterns.IsMaxSubexpressionPreventStart);
            }
            else {
                patternSubexpressionPoolSvc = PatternSubexpressionPoolRuntimeSvcNoOp.INSTANCE;
            }

            var exprDeclaredPathRegistry = new PathRegistry<string, ExpressionDeclItem>(PathRegistryObjectType.EXPRDECL);
            var scriptPathRegistry = new PathRegistry<NameAndParamNum, ExpressionScriptProvided>(PathRegistryObjectType.SCRIPT);

            RowRecogStatePoolRuntimeSvc rowRecogStatePoolEngineSvc = null;
            if (configs.Runtime.MatchRecognize.MaxStates != null) {
                rowRecogStatePoolEngineSvc = new RowRecogStatePoolRuntimeSvc(
                    configs.Runtime.MatchRecognize.MaxStates.Value,
                    configs.Runtime.MatchRecognize.IsMaxStatesPreventStart);
            }

            var rowRecogStateRepoFactory = MakeRowRecogStateRepoFactory();

            DatabaseConfigServiceRuntime databaseConfigServiceRuntime =
                new DatabaseConfigServiceImpl(configs.Common.DatabaseReferences, importServiceRuntime);
            var historicalDataCacheFactory = MakeHistoricalDataCacheFactory(epServicesHA.RuntimeExtensionServices);

            var dataflowService = new EPDataFlowServiceImpl();
            var dataFlowFilterServiceAdapter = MakeDataFlowFilterServiceAdapter();

            var threadingService = MakeThreadingService(configs);
            var eventRenderer = new EPRenderEventServiceImpl();

            return new EPServicesContext(
                container,
                aggregationServiceFactoryService,
                beanEventTypeFactoryPrivate,
                beanEventTypeStemService,
                ClassForNameProviderDefault.INSTANCE,
                configs,
                contextManagementService,
                pathContextRegistry,
                contextServiceFactory,
                dataflowService,
                dataFlowFilterServiceAdapter,
                dataInputOutputSerdeProvider,
                databaseConfigServiceRuntime,
                deploymentLifecycleService,
                dispatchService,
                runtimeEnvContext,
                runtimeSettingsService,
                epRuntime.URI,
                importServiceRuntime,
                epStatementFactory,
                exprDeclaredPathRegistry,
                eventProcessingRWLock,
                epServicesHA,
                epRuntime,
                eventBeanService,
                eventBeanTypedEventFactory,
                eventRenderer,
                eventTableIndexService,
                eventTypeAvroHandler,
                eventTypeFactory,
                eventTypePathRegistry,
                eventTypeRepositoryPreconfigured,
                eventTypeResolvingBeanFactory,
                exceptionHandlingService,
                expressionResultCacheSharable,
                filterBooleanExpressionFactory,
                filterServiceSPI,
                filterSharedBoolExprRepository,
                filterSharedLookupableRepository,
                historicalDataCacheFactory,
                internalEventRouter,
                metricsReporting,
                multiMatchHandlerFactory,
                namedWindowConsumerManagementService,
                namedWindowDispatchService,
                namedWindowFactoryService,
                namedWindowManagementService,
                pathNamedWindowRegistry,
                patternFactoryService,
                patternSubexpressionPoolSvc,
                resultSetProcessorHelperFactory,
                rowRecogStateRepoFactory,
                rowRecogStatePoolEngineSvc,
                schedulingService,
                scriptPathRegistry,
                statementLifecycleService,
                statementAgentInstanceLockFactory,
                statementResourceHolderBuilder,
                tableExprEvaluatorContext,
                tableManagementService,
                tablePathRegistry,
                threadingService,
                timeAbacus,
                timeSourceService,
                timerService,
                variableManagementService,
                variablePathRegistry,
                viewableActivatorFactory,
                viewFactoryService,
                viewServicePreviousFactory,
                xmlFragmentEventTypeFactory
            );
        }

        protected abstract EPServicesHA InitHA(
            string runtimeURI,
            Configuration configurationSnapshot,
            RuntimeEnvContext runtimeEnvContext,
            ManagedReadWriteLock eventProcessingRWLock,
            RuntimeSettingsService runtimeSettingsService);

        protected abstract ViewableActivatorFactory InitViewableActivatorFactory();

        protected abstract FilterServiceSPI MakeFilterService(
            RuntimeExtensionServices runtimeExt,
            EventTypeRepository eventTypeRepository,
            StatementLifecycleServiceImpl statementLifecycleService,
            RuntimeSettingsService runtimeSettingsService,
            EventTypeIdResolver eventTypeIdResolver,
            FilterSharedLookupableRepository filterSharedLookupableRepository);

        protected abstract StatementResourceHolderBuilder MakeStatementResourceHolderBuilder();

        protected abstract RuntimeSettingsService MakeRuntimeSettingsService(Configuration configurationSnapshot);

        protected abstract FilterSharedLookupableRepository MakeFilterSharedLookupableRepository();

        protected abstract FilterSharedBoolExprRepository MakeFilterSharedBoolExprRepository();

        protected abstract FilterBooleanExpressionFactory MakeFilterBooleanExpressionFactory(StatementLifecycleServiceImpl statementLifecycleService);

        protected abstract AggregationServiceFactoryService MakeAggregationServiceFactoryService(RuntimeExtensionServices runtimeExt);

        protected abstract ViewFactoryService MakeViewFactoryService();

        protected abstract EventTypeFactory MakeEventTypeFactory(
            RuntimeExtensionServices runtimeExt,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            DeploymentLifecycleServiceImpl deploymentLifecycleService);

        protected abstract EventTypeResolvingBeanFactory MakeEventTypeResolvingBeanFactory(
            EventTypeRepository eventTypeRepository,
            EventTypeAvroHandler eventTypeAvroHandler);

        protected abstract PatternFactoryService MakePatternFactoryService();

        protected abstract SchedulingServiceSPI MakeSchedulingService(
            EPServicesHA epServicesHA,
            TimeSourceService timeSourceService,
            RuntimeExtensionServices runtimeExt,
            RuntimeSettingsService runtimeSettingsService,
            StatementContextResolver statementContextResolver);

        protected abstract MultiMatchHandlerFactory MakeMultiMatchHandlerFactory(Configuration configurationInformation);

        protected abstract ContextServiceFactory MakeContextServiceFactory(RuntimeExtensionServices runtimeExtensionServices);

        protected abstract ViewServicePreviousFactory MakeViewServicePreviousFactory(RuntimeExtensionServices ext);

        protected abstract EPStatementFactory MakeEPStatementFactory();

        protected abstract EventBeanTypedEventFactory MakeEventBeanTypedEventFactory(EventTypeAvroHandler eventTypeAvroHandler);

        protected abstract EventTableIndexService MakeEventTableIndexService(RuntimeExtensionServices ext);

        protected abstract DataInputOutputSerdeProvider MakeSerdeProvider(RuntimeExtensionServices ext);

        protected abstract ResultSetProcessorHelperFactory MakeResultSetProcessorHelperFactory(RuntimeExtensionServices ext);

        protected abstract NamedWindowDispatchService MakeNamedWindowDispatchService(
            SchedulingServiceSPI schedulingService,
            Configuration configurationSnapshot,
            ManagedReadWriteLock eventProcessingRWLock,
            ExceptionHandlingService exceptionHandlingService,
            VariableManagementService variableManagementService,
            TableManagementService tableManagementService,
            MetricReportingService metricReportingService);

        protected abstract NamedWindowConsumerManagementService MakeNamedWindowConsumerManagementService(
            NamedWindowManagementService namedWindowManagementService);

        protected abstract NamedWindowFactoryService MakeNamedWindowFactoryService();

        protected abstract RowRecogStateRepoFactory MakeRowRecogStateRepoFactory();

        protected abstract VariableManagementService MakeVariableManagementService(
            Configuration configs,
            SchedulingServiceSPI schedulingService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            RuntimeSettingsService runtimeSettingsService,
            EPServicesHA epServicesHA);

        protected abstract TableManagementService MakeTableManagementService(
            RuntimeExtensionServices runtimeExt,
            TableExprEvaluatorContext tableExprEvaluatorContext);

        protected abstract EventTypeAvroHandler MakeEventTypeAvroHandler(
            ImportServiceRuntime importServiceRuntime,
            ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings,
            RuntimeExtensionServices runtimeExt);

        protected abstract HistoricalDataCacheFactory MakeHistoricalDataCacheFactory(RuntimeExtensionServices runtimeExtensionServices);

        protected abstract DataFlowFilterServiceAdapter MakeDataFlowFilterServiceAdapter();

        protected abstract ThreadingService MakeThreadingService(Configuration configs);

        protected static ExceptionHandlingService InitExceptionHandling(
            string runtimeURI,
            ConfigurationRuntimeExceptionHandling exceptionHandling,
            ConfigurationRuntimeConditionHandling conditionHandling,
            ClassForNameProvider classForNameProvider)
        {
            IList<ExceptionHandler> exceptionHandlers;
            if (exceptionHandling.HandlerFactories == null || exceptionHandling.HandlerFactories.IsEmpty()) {
                exceptionHandlers = new EmptyList<ExceptionHandler>();
            }
            else {
                exceptionHandlers = new List<ExceptionHandler>();
                var context = new ExceptionHandlerFactoryContext(runtimeURI);
                foreach (var className in exceptionHandling.HandlerFactories) {
                    try {
                        var factory = TypeHelper.Instantiate<ExceptionHandlerFactory>(
                            className, classForNameProvider);
                        var handler = factory.GetHandler(context);
                        if (handler == null) {
                            Log.Warn("Exception handler factory '" + className + "' returned a null handler, skipping factory");
                            continue;
                        }

                        exceptionHandlers.Add(handler);
                    }
                    catch (Exception ex) {
                        throw new ConfigurationException(
                            "Exception initializing exception handler from exception handler factory '" + className + "': " + ex.Message, ex);
                    }
                }
            }

            IList<ConditionHandler> conditionHandlers;
            if (conditionHandling.HandlerFactories == null || conditionHandling.HandlerFactories.IsEmpty()) {
                conditionHandlers = new EmptyList<ConditionHandler>();
            }
            else {
                conditionHandlers = new List<ConditionHandler>();
                var context = new ConditionHandlerFactoryContext(runtimeURI);
                foreach (var className in conditionHandling.HandlerFactories) {
                    try {
                        var factory = TypeHelper.Instantiate<ConditionHandlerFactory>(
                            className, classForNameProvider);
                        var handler = factory.GetHandler(context);
                        if (handler == null) {
                            Log.Warn("Condition handler factory '" + className + "' returned a null handler, skipping factory");
                            continue;
                        }

                        conditionHandlers.Add(handler);
                    }
                    catch (Exception ex) {
                        throw new ConfigurationException(
                            "Exception initializing exception handler from exception handler factory '" + className + "': " + ex.Message, ex);
                    }
                }
            }

            return new ExceptionHandlingService(runtimeURI, exceptionHandlers, conditionHandlers);
        }

        private static TimeSourceService MakeTimeSource(Configuration configSnapshot)
        {
            if (configSnapshot.Runtime.TimeSource.TimeSourceType == TimeSourceType.NANO) {
                // this is a static variable to keep overhead down for getting a current time
                TimeSourceServiceImpl.IsSystemCurrentTime = false;
            }

            return new TimeSourceServiceImpl();
        }
    }
} // end of namespace
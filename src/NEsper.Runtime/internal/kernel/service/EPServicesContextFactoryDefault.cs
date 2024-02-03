///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.common;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.mgr;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.agg.core;
using com.espertech.esper.common.@internal.epl.dataflow.filtersvcadapter;
using com.espertech.esper.common.@internal.epl.historical.datacache;
using com.espertech.esper.common.@internal.epl.index.@base;
using com.espertech.esper.common.@internal.epl.namedwindow.consume;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.epl.pattern.pool;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.rowrecog.state;
using com.espertech.esper.common.@internal.epl.table.core;
using com.espertech.esper.common.@internal.epl.variable.core;
using com.espertech.esper.common.@internal.@event.avro;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.common.@internal.@event.eventtyperepo;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.serde.runtime.@event;
using com.espertech.esper.common.@internal.serde.runtime.eventtype;
using com.espertech.esper.common.@internal.settings;
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
using com.espertech.esper.runtime.@internal.namedwindow;
using com.espertech.esper.runtime.@internal.schedulesvcimpl;
using com.espertech.esper.runtime.@internal.statementlifesvc;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPServicesContextFactoryDefault : EPServicesContextFactoryBase
    {
        private IContainer _container;

        public EPServicesContextFactoryDefault(IContainer container)
        {
            this._container = container;
        }

        protected override RuntimeSettingsService MakeRuntimeSettingsService(Configuration configurationSnapshot)
        {
            return new RuntimeSettingsService(configurationSnapshot.Common, configurationSnapshot.Runtime);
        }

        protected override EPServicesHA InitHA(
            string runtimeURI,
            Configuration configurationSnapshot,
            RuntimeEnvContext runtimeEnvContext,
            IReaderWriterLock eventProcessingRWLock,
            RuntimeSettingsService runtimeSettingsService,
            EPRuntimeOptions options,
            ParentTypeResolver typeResolverParent)
        {
            return new EPServicesHA(
                RuntimeExtensionServicesNoHA.INSTANCE,
                DeploymentRecoveryServiceImpl.INSTANCE,
                ListenerRecoveryServiceImpl.INSTANCE,
                new StatementIdRecoveryServiceImpl(),
                null,
                null);
        }

        protected override ViewableActivatorFactory InitViewableActivatorFactory()
        {
            return ViewableActivatorFactoryImpl.INSTANCE;
        }

        protected override FilterServiceSPI MakeFilterService(
            RuntimeExtensionServices runtimeExt,
            EventTypeRepository eventTypeRepository,
            StatementLifecycleServiceImpl statementLifecycleService,
            RuntimeSettingsService runtimeSettingsService,
            EventTypeIdResolver eventTypeIdResolver,
            FilterSharedLookupableRepository filterSharedLookupableRepository)
        {
            return new FilterServiceLockCoarse(_container.RWLockManager(), -1);
        }

        public override EPEventServiceImpl CreateEPRuntime(
            EPServicesContext services,
            AtomicBoolean serviceStatusProvider)
        {
            return new EPEventServiceImpl(services);
        }

        protected override StatementResourceHolderBuilder MakeStatementResourceHolderBuilder()
        {
            return StatementResourceHolderBuilderImpl.INSTANCE;
        }

        protected override FilterSharedLookupableRepository MakeFilterSharedLookupableRepository()
        {
            return FilterSharedLookupableRepositoryImpl.INSTANCE;
        }

        protected override AggregationServiceFactoryService MakeAggregationServiceFactoryService(RuntimeExtensionServices runtimeExt)
        {
            return AggregationServiceFactoryServiceImpl.INSTANCE;
        }

        protected override ViewFactoryService MakeViewFactoryService()
        {
            return ViewFactoryServiceImpl.INSTANCE;
        }

        protected override PatternFactoryService MakePatternFactoryService()
        {
            return PatternFactoryServiceImpl.INSTANCE;
        }

        protected override EventTypeFactory MakeEventTypeFactory(
            RuntimeExtensionServices runtimeExt,
            EventTypeRepositoryImpl eventTypeRepositoryPreconfigured,
            DeploymentLifecycleServiceImpl deploymentLifecycleService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory)
        {
            return EventTypeFactoryImpl.GetInstance(_container);
        }

        protected override EventTypeResolvingBeanFactory MakeEventTypeResolvingBeanFactory(
            EventTypeRepository eventTypeRepository,
            EventTypeAvroHandler eventTypeAvroHandler)
        {
            return new EventTypeResolvingBeanFactoryImpl(eventTypeRepository, eventTypeAvroHandler);
        }

        protected override SchedulingServiceSPI MakeSchedulingService(
            EPServicesHA epServicesHA,
            TimeSourceService timeSourceService,
            RuntimeExtensionServices runtimeExt,
            RuntimeSettingsService runtimeSettingsService,
            StatementContextResolver statementContextResolver,
            string zoneId)
        {
            return new SchedulingServiceImpl(-1, timeSourceService);
        }

        protected override FilterBooleanExpressionFactory MakeFilterBooleanExpressionFactory(StatementLifecycleServiceImpl statementLifecycleService)
        {
            return FilterBooleanExpressionFactoryImpl.INSTANCE;
        }

        protected override MultiMatchHandlerFactory MakeMultiMatchHandlerFactory(Configuration configurationInformation)
        {
            return new MultiMatchHandlerFactoryImpl(configurationInformation.Runtime.Expression.IsSelfSubselectPreeval);
        }

        protected override ContextServiceFactory MakeContextServiceFactory(RuntimeExtensionServices runtimeExtensionServices)
        {
            return ContextServiceFactoryDefault.INSTANCE;
        }

        protected override ViewServicePreviousFactory MakeViewServicePreviousFactory(RuntimeExtensionServices ext)
        {
            return ViewServicePreviousFactoryImpl.INSTANCE;
        }

        protected override EPStatementFactory MakeEPStatementFactory()
        {
            return EPStatementFactoryDefault.INSTANCE;
        }

        protected override EventBeanTypedEventFactory MakeEventBeanTypedEventFactory(EventTypeAvroHandler eventTypeAvroHandler)
        {
            return new EventBeanTypedEventFactoryRuntime(eventTypeAvroHandler);
        }

        protected override EventTypeSerdeRepository MakeEventTypeSerdeRepository(
            EventTypeRepository preconfigureds,
            PathRegistry<String, EventType> eventTypePathRegistry)
        {
            return EventTypeSerdeRepositoryDefault.INSTANCE;
        }

        protected override EventTableIndexService MakeEventTableIndexService(RuntimeExtensionServices runtimeExtensionServices)
        {
            return EventTableIndexServiceImpl.INSTANCE;
        }

        protected override ResultSetProcessorHelperFactory MakeResultSetProcessorHelperFactory(RuntimeExtensionServices ext)
        {
            return ResultSetProcessorHelperFactoryDefault.INSTANCE;
        }

        protected override NamedWindowDispatchService MakeNamedWindowDispatchService(
            SchedulingServiceSPI schedulingService,
            Configuration configurationSnapshot,
            IReaderWriterLock eventProcessingRWLock,
            ExceptionHandlingService exceptionHandlingService,
            VariableManagementService variableManagementService,
            TableManagementService tableManagementService,
            MetricReportingService metricReportingService)
        {
            return new NamedWindowDispatchServiceImpl(
                schedulingService,
                variableManagementService,
                tableManagementService,
                configurationSnapshot.Runtime.Execution.IsPrioritized,
                eventProcessingRWLock,
                exceptionHandlingService,
                metricReportingService);
        }

        protected override NamedWindowConsumerManagementService MakeNamedWindowConsumerManagementService(
            NamedWindowManagementService namedWindowManagementService)
        {
            return NamedWindowConsumerManagementServiceImpl.INSTANCE;
        }

        protected override NamedWindowFactoryService MakeNamedWindowFactoryService()
        {
            return NamedWindowFactoryServiceImpl.INSTANCE;
        }

        protected override FilterSharedBoolExprRepository MakeFilterSharedBoolExprRepository()
        {
            return FilterSharedBoolExprRepositoryImpl.INSTANCE;
        }

        protected override VariableManagementService MakeVariableManagementService(
            Configuration configs,
            SchedulingServiceSPI schedulingService,
            EventBeanTypedEventFactory eventBeanTypedEventFactory,
            RuntimeSettingsService runtimeSettingsService,
            EPServicesHA epServicesHA)
        {
            return new VariableManagementServiceImpl(
                _container.RWLockManager(),
                configs.Runtime.Variables.MsecVersionRelease,
                schedulingService,
                eventBeanTypedEventFactory,
                null);
        }

        protected override TableManagementService MakeTableManagementService(
            RuntimeExtensionServices runtimeExt,
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            return new TableManagementServiceImpl(tableExprEvaluatorContext);
        }

        protected override RowRecogStateRepoFactory MakeRowRecogStateRepoFactory()
        {
            return RowRecogStateRepoFactoryDefault.INSTANCE;
        }

        protected override EventTypeAvroHandler MakeEventTypeAvroHandler(
            ImportServiceRuntime importServiceRuntime,
            ConfigurationCommonEventTypeMeta.AvroSettingsConfig avroSettings,
            RuntimeExtensionServices runtimeExt)
        {
            return EventTypeAvroHandlerFactory.Resolve(
                importServiceRuntime, avroSettings,
                EventTypeAvroHandlerConstants.RUNTIME_NONHA_HANDLER_IMPL);
        }

        protected override HistoricalDataCacheFactory MakeHistoricalDataCacheFactory(RuntimeExtensionServices runtimeExtensionServices)
        {
            return new HistoricalDataCacheFactory();
        }

        protected override DataFlowFilterServiceAdapter MakeDataFlowFilterServiceAdapter()
        {
            return DataFlowFilterServiceAdapterNonHA.INSTANCE;
        }

        protected override ThreadingService MakeThreadingService(Configuration configs)
        {
            return new ThreadingServiceImpl(configs.Runtime.Threading);
        }
        
        protected override EventSerdeFactory MakeEventSerdeFactory(RuntimeExtensionServices ext)
        {
            return EventSerdeFactoryDefault.INSTANCE;
        }

        protected override StageRecoveryService MakeStageRecoveryService(EPServicesHA epServicesHA)
        {
            return StageRecoveryServiceImpl.INSTANCE;
        }

        protected override PatternSubexpressionPoolRuntimeSvc MakePatternSubexpressionPoolSvc(
            long maxSubexpressions,
            bool maxSubexpressionPreventStart,
            RuntimeExtensionServices runtimeExtensionServices)
        {
            return new PatternSubexpressionPoolRuntimeSvcImpl(maxSubexpressions, maxSubexpressionPreventStart);
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.context.mgr;
using com.espertech.esper.epl.agg.factory;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.filter;
using com.espertech.esper.pattern;
using com.espertech.esper.rowregex;
using com.espertech.esper.schedule;
using com.espertech.esper.timer;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    public sealed class StatementContextEngineServices
    {
        public StatementContextEngineServices(
            IContainer container,
            String engineURI,
            EventAdapterService eventAdapterService,
            NamedWindowMgmtService namedWindowMgmtService,
            VariableService variableService,
            TableService tableService,
            EngineSettingsService engineSettingsService,
            ValueAddEventService valueAddEventService,
            ConfigurationInformation configSnapshot,
            MetricReportingServiceSPI metricReportingService,
            ViewService viewService,
            ExceptionHandlingService exceptionHandlingService,
            ExpressionResultCacheService expressionResultCacheService,
            StatementEventTypeRef statementEventTypeRef,
            TableExprEvaluatorContext tableExprEvaluatorContext,
            EngineLevelExtensionServicesContext engineLevelExtensionServicesContext,
            RegexHandlerFactory regexHandlerFactory,
            StatementLockFactory statementLockFactory,
            ContextManagementService contextManagementService,
            ViewServicePreviousFactory viewServicePreviousFactory,
            EventTableIndexService eventTableIndexService,
            PatternNodeFactory patternNodeFactory,
            FilterBooleanExpressionFactory filterBooleanExpressionFactory,
            TimeSourceService timeSourceService,
            EngineImportService engineImportService,
            AggregationFactoryFactory aggregationFactoryFactory,
            SchedulingService schedulingService,
            ExprDeclaredService exprDeclaredService)
        {
            Container = container;
            EngineURI = engineURI;
            EventAdapterService = eventAdapterService;
            NamedWindowMgmtService = namedWindowMgmtService;
            VariableService = variableService;
            TableService = tableService;
            EngineSettingsService = engineSettingsService;
            ValueAddEventService = valueAddEventService;
            ConfigSnapshot = configSnapshot;
            MetricReportingService = metricReportingService;
            ViewService = viewService;
            ExceptionHandlingService = exceptionHandlingService;
            ExpressionResultCacheService = expressionResultCacheService;
            StatementEventTypeRef = statementEventTypeRef;
            TableExprEvaluatorContext = tableExprEvaluatorContext;
            EngineLevelExtensionServicesContext = engineLevelExtensionServicesContext;
            RegexHandlerFactory = regexHandlerFactory;
            StatementLockFactory = statementLockFactory;
            ContextManagementService = contextManagementService;
            ViewServicePreviousFactory = viewServicePreviousFactory;
            EventTableIndexService = eventTableIndexService;
            PatternNodeFactory = patternNodeFactory;
            FilterBooleanExpressionFactory = filterBooleanExpressionFactory;
            TimeSourceService = timeSourceService;
            EngineImportService = engineImportService;
            AggregationFactoryFactory = aggregationFactoryFactory;
            SchedulingService = schedulingService;
            ExprDeclaredService = exprDeclaredService;
        }

        public string EngineURI { get; private set; }

        public EventAdapterService EventAdapterService { get; private set; }

        public NamedWindowMgmtService NamedWindowMgmtService { get; private set; }

        public VariableService VariableService { get; private set; }

        public IList<Uri> PlugInTypeResolutionURIs {
            get { return EngineSettingsService.PlugInEventTypeResolutionURIs; }
        }

        public ValueAddEventService ValueAddEventService { get; private set; }

        public ConfigurationInformation ConfigSnapshot { get; private set; }

        public MetricReportingServiceSPI MetricReportingService { get; private set; }

        public ViewService ViewService { get; private set; }

        public ExceptionHandlingService ExceptionHandlingService { get; private set; }

        public ExpressionResultCacheService ExpressionResultCacheService { get; private set; }

        public StatementEventTypeRef StatementEventTypeRef { get; private set; }

        public TableService TableService { get; private set; }

        public EngineSettingsService EngineSettingsService { get; private set; }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; private set; }

        public EngineLevelExtensionServicesContext EngineLevelExtensionServicesContext { get; private set; }

        public RegexHandlerFactory RegexHandlerFactory { get; private set; }

        public StatementLockFactory StatementLockFactory { get; private set; }

        public ContextManagementService ContextManagementService { get; private set; }

        public ViewServicePreviousFactory ViewServicePreviousFactory { get; private set; }

        public EventTableIndexService EventTableIndexService { get; private set; }

        public PatternNodeFactory PatternNodeFactory { get; private set; }

        public FilterBooleanExpressionFactory FilterBooleanExpressionFactory { get; private set; }

        public TimeSourceService TimeSourceService { get; private set; }

        public EngineImportService EngineImportService { get; private set; }

        public AggregationFactoryFactory AggregationFactoryFactory { get; private set; }

        public SchedulingService SchedulingService { get; private set; }

        public ExprDeclaredService ExprDeclaredService { get; private set; }

        public IContainer Container { get; set; }

        public IThreadLocalManager ThreadLocalManager =>
            Container.Resolve<IThreadLocalManager>();

        public ILockManager LockManager =>
            Container.Resolve<ILockManager>();

        public IReaderWriterLockManager RWLockManager =>
            Container.Resolve<IReaderWriterLockManager>();

        public IResourceManager ResourceManager =>
            Container.Resolve<IResourceManager>();
    }
}
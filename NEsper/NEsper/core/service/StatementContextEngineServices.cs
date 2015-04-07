///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.epl.variable;
using com.espertech.esper.events;
using com.espertech.esper.events.vaevent;
using com.espertech.esper.view;

namespace com.espertech.esper.core.service
{
    public sealed class StatementContextEngineServices
    {
        private readonly EngineSettingsService _engineSettingsService;

        public StatementContextEngineServices(
            String engineURI,
            EventAdapterService eventAdapterService,
            NamedWindowService namedWindowService,
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
            TableExprEvaluatorContext tableExprEvaluatorContext)
        {
            EngineURI = engineURI;
            EventAdapterService = eventAdapterService;
            NamedWindowService = namedWindowService;
            VariableService = variableService;
            TableService = tableService;
            _engineSettingsService = engineSettingsService;
            ValueAddEventService = valueAddEventService;
            ConfigSnapshot = configSnapshot;
            MetricReportingService = metricReportingService;
            ViewService = viewService;
            ExceptionHandlingService = exceptionHandlingService;
            ExpressionResultCacheService = expressionResultCacheService;
            StatementEventTypeRef = statementEventTypeRef;
            TableExprEvaluatorContext = tableExprEvaluatorContext;
        }

        public string EngineURI { get; private set; }

        public EventAdapterService EventAdapterService { get; private set; }

        public NamedWindowService NamedWindowService { get; private set; }

        public VariableService VariableService { get; private set; }

        public IList<Uri> PlugInTypeResolutionURIs
        {
            get { return _engineSettingsService.PlugInEventTypeResolutionURIs; }
        }

        public ValueAddEventService ValueAddEventService { get; private set; }

        public ConfigurationInformation ConfigSnapshot { get; private set; }

        public MetricReportingServiceSPI MetricReportingService { get; private set; }

        public ViewService ViewService { get; private set; }

        public ExceptionHandlingService ExceptionHandlingService { get; private set; }

        public ExpressionResultCacheService ExpressionResultCacheService { get; private set; }

        public StatementEventTypeRef StatementEventTypeRef { get; private set; }

        public TableService TableService { get; private set; }

        public TableExprEvaluatorContext TableExprEvaluatorContext { get; private set; }
    }
}
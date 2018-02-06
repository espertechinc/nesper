///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;

namespace com.espertech.esper.epl.named
{
    /// <summary>
    /// View for the on-delete statement that handles removing events from a named window.
    /// </summary>
    public class NamedWindowOnMergeViewFactory : NamedWindowOnExprBaseViewFactory
    {
        private readonly StatementMetricHandle _createNamedWindowMetricHandle;
        private readonly MetricReportingService _metricReportingService;
        private readonly NamedWindowOnMergeHelper _namedWindowOnMergeHelper;
        private readonly StatementResultService _statementResultService;

        public NamedWindowOnMergeViewFactory(EventType namedWindowEventType,
                                             NamedWindowOnMergeHelper namedWindowOnMergeHelper,
                                             StatementResultService statementResultService,
                                             StatementMetricHandle createNamedWindowMetricHandle,
                                             MetricReportingService metricReportingService)
            : base(namedWindowEventType)
        {
            _namedWindowOnMergeHelper = namedWindowOnMergeHelper;
            _statementResultService = statementResultService;
            _createNamedWindowMetricHandle = createNamedWindowMetricHandle;
            _metricReportingService = metricReportingService;
        }

        public NamedWindowOnMergeHelper NamedWindowOnMergeHelper => _namedWindowOnMergeHelper;

        public StatementResultService StatementResultService => _statementResultService;

        public StatementMetricHandle CreateNamedWindowMetricHandle => _createNamedWindowMetricHandle;

        public MetricReportingService MetricReportingService => _metricReportingService;

        public override NamedWindowOnExprBaseView Make(SubordWMatchExprLookupStrategy lookupStrategy, NamedWindowRootViewInstance namedWindowRootViewInstance, AgentInstanceContext agentInstanceContext, ResultSetProcessor resultSetProcessor)
        {
            return new NamedWindowOnMergeView(lookupStrategy, namedWindowRootViewInstance, agentInstanceContext, this);
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.metric;
using com.espertech.esper.epl.table.merge;
using com.espertech.esper.epl.table.mgmt;

namespace com.espertech.esper.epl.table.onaction
{
    public class TableOnMergeViewFactory : TableOnViewFactory
    {
        public TableOnMergeViewFactory(
            TableMetadata tableMetadata,
            TableOnMergeHelper onMergeHelper,
            StatementResultService statementResultService,
            StatementMetricHandle metricsHandle,
            MetricReportingServiceSPI metricReportingService)
        {
            TableMetadata = tableMetadata;
            OnMergeHelper = onMergeHelper;
            StatementResultService = statementResultService;
            MetricsHandle = metricsHandle;
            MetricReportingService = metricReportingService;
        }

        public TableMetadata TableMetadata { get; private set; }

        public TableOnMergeHelper OnMergeHelper { get; private set; }

        public StatementResultService StatementResultService { get; private set; }

        public StatementMetricHandle MetricsHandle { get; private set; }

        public MetricReportingServiceSPI MetricReportingService { get; private set; }

        public TableOnViewBase Make(
            SubordWMatchExprLookupStrategy lookupStrategy,
            TableStateInstance tableState,
            AgentInstanceContext agentInstanceContext,
            ResultSetProcessor resultSetProcessor)
        {
            return new TableOnMergeView(lookupStrategy, tableState, agentInstanceContext, TableMetadata, this);
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.schedule;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     Execution context for metrics reporting executions.
    /// </summary>
    public class MetricExecutionContext
    {
        public MetricExecutionContext(
            FilterService filterService,
            SchedulingService schedulingService,
            EventServiceSendEventCommon epRuntimeSendEvent,
            StatementMetricRepository statementMetricRepository)
        {
            FilterService = filterService;
            SchedulingService = schedulingService;
            EPRuntimeSendEvent = epRuntimeSendEvent;
            StatementMetricRepository = statementMetricRepository;
        }

        public FilterService FilterService { get; }

        public SchedulingService SchedulingService { get; }

        public EventServiceSendEventCommon EPRuntimeSendEvent { get; }

        /// <summary>
        ///     Returns statement metric holder
        /// </summary>
        /// <returns>holder for metrics</returns>
        public StatementMetricRepository StatementMetricRepository { get; }
    }
} // end of namespace
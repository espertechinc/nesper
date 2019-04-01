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
    ///     Metrics reporting service for instrumentation data publishing, if enabled.
    /// </summary>
    public interface MetricReportingService
    {
        void SetContext(
            FilterService filterService, SchedulingService schedulingService,
            EventServiceSendEventCommon eventServiceSendEventInternal);

        /// <summary>
        ///     Indicates current runtime time.
        /// </summary>
        /// <param name="currentTime">runtime time</param>
        void ProcessTimeEvent(long currentTime);

        /// <summary>
        ///     Destroy the service.
        /// </summary>
        void Destroy();

        /// <summary>
        ///     Account for statement CPU and wall time.
        /// </summary>
        /// <param name="metricsHandle">statement handle</param>
        /// <param name="deltaCPU">cpu time nsec</param>
        /// <param name="deltaWall">wall time nsec</param>
        /// <param name="numInput">number of input rows</param>
        void AccountTime(StatementMetricHandle metricsHandle, long deltaCPU, long deltaWall, int numInput);

        /// <summary>
        ///     Account for statement output row counting.
        /// </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="numIStream">number of insert stream rows</param>
        /// <param name="numRStream">number of remove stream rows</param>
        /// <param name="epStatement">statement</param>
        /// <param name="runtime">runtime</param>
        void AccountOutput(
            StatementMetricHandle handle, int numIStream, int numRStream, object epStatement, object runtime);

        /// <summary>
        ///     Returns for a new statement a handle for later accounting.
        /// </summary>
        /// <param name="statementId">statement id</param>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="statementName">statement name</param>
        /// <returns>handle</returns>
        StatementMetricHandle GetStatementHandle(int statementId, string deploymentId, string statementName);

        /// <summary>
        ///     Change the reporting interval for the given statement group name.
        /// </summary>
        /// <param name="stmtGroupName">group name</param>
        /// <param name="newInterval">new interval, or zero or negative value to disable reporting</param>
        void SetMetricsReportingInterval(string stmtGroupName, long newInterval);

        /// <summary>
        ///     Disable metrics reporting for statement.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="statementName">statement name</param>
        void SetMetricsReportingStmtDisabled(string deploymentId, string statementName);

        /// <summary>
        ///     Enable metrics reporting for statement.
        /// </summary>
        /// <param name="deploymentId">deployment id</param>
        /// <param name="statementName">statement name</param>
        void SetMetricsReportingStmtEnabled(string deploymentId, string statementName);

        /// <summary>
        ///     Enables metrics reporting globally.
        /// </summary>
        void SetMetricsReportingEnabled();

        /// <summary>
        ///     Disables metrics reporting globally.
        /// </summary>
        void SetMetricsReportingDisabled();

        bool IsMetricsReportingEnabled { get; }
    }
} // end of namespace
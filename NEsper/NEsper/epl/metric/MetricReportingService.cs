///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.metric
{
    /// <summary>
    /// Metrics reporting service for instrumentation data publishing, if enabled.
    /// </summary>
    public interface MetricReportingService : IDisposable
    {
        /// <summary>
        /// Gets the performance collector.
        /// </summary>
        /// <value>The performance collector.</value>
        PerformanceCollector PerformanceCollector { get; }

        /// <summary>Sets runtime and services. </summary>
        /// <param name="runtime">runtime</param>
        /// <param name="servicesContext">services</param>
        void SetContext(EPRuntime runtime, EPServicesContext servicesContext);

        /// <summary>Indicates current engine time. </summary>
        /// <param name="currentTime">engine time</param>
        void ProcessTimeEvent(long currentTime);

        /// <summary>
        /// Account for statement execution time.
        /// </summary>
        /// <param name="metricsHandle">statement handle</param>
        /// <param name="cpuTime">The cpu time.</param>
        /// <param name="wallTime">The wall time.</param>
        /// <param name="numInput">The num input.</param>
        void AccountTime(StatementMetricHandle metricsHandle, long cpuTime, long wallTime, int numInput);

        /// <summary>Account for statement output row counting. </summary>
        /// <param name="handle">statement handle</param>
        /// <param name="numIStream">number of insert stream rows</param>
        /// <param name="numRStream">number of remove stream rows</param>
        void AccountOutput(StatementMetricHandle handle, int numIStream, int numRStream);

        /// <summary>Returns for a new statement a handle for later accounting. </summary>
        /// <param name="statementId">statement id</param>
        /// <param name="statementName">statement name</param>
        /// <returns>handle</returns>
        StatementMetricHandle GetStatementHandle(int statementId, string statementName);

        /// <summary>Change the reporting interval for the given statement group name. </summary>
        /// <param name="stmtGroupName">group name</param>
        /// <param name="newInterval">new interval, or zero or negative value to disable reporting</param>
        void SetMetricsReportingInterval(String stmtGroupName, long newInterval);

        /// <summary>Disable metrics reporting for statement. </summary>
        /// <param name="statementName">statement name</param>
        void SetMetricsReportingStmtDisabled(String statementName);

        /// <summary>Enable metrics reporting for statement. </summary>
        /// <param name="statementName">statement name</param>
        void SetMetricsReportingStmtEnabled(String statementName);

        /// <summary>Enables metrics reporting globally. </summary>
        void SetMetricsReportingEnabled();

        /// <summary>Disables metrics reporting globally. </summary>
        void SetMetricsReportingDisabled();
    }
}

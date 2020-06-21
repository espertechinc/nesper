///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.client.metric
{
    /// <summary>
    /// Service for metrics reporting.
    /// </summary>
    public interface EPMetricsService
    {
        /// <summary>
        /// Sets a new interval for metrics reporting for a pre-configured statement group, or changes
        /// the default statement reporting interval if supplying a null value for the statement group name.
        /// </summary>
        /// <param name="stmtGroupName">name of statement group, provide a null value for the default statement interval (default group)</param>
        /// <param name="newIntervalMSec">millisecond interval, use zero or negative value to disable</param>
        /// <throws>ConfigurationException if the statement group cannot be found</throws>
        void SetMetricsReportingInterval(
            string stmtGroupName,
            long newIntervalMSec);

        /// <summary>
        /// Enable metrics reporting for the given statement.
        /// <para />This operation can only be performed at runtime and is not available at runtime initialization time.
        /// <para />Statement metric reporting follows the configured default or statement group interval.
        /// <para />Only if metrics reporting (on the runtime level) has been enabled at initialization time
        /// can statement-level metrics reporting be enabled through this method.
        /// </summary>
        /// <param name="deploymentId">for which to enable metrics reporting</param>
        /// <param name="statementName">for which to enable metrics reporting</param>
        /// <throws>ConfigurationException if the statement cannot be found</throws>
        void SetMetricsReportingStmtEnabled(
            string deploymentId,
            string statementName);

        /// <summary>
        /// Disable metrics reporting for a given statement.
        /// </summary>
        /// <param name="deploymentId">for which to enable metrics reporting</param>
        /// <param name="statementName">for which to disable metrics reporting</param>
        /// <throws>ConfigurationException if the statement cannot be found</throws>
        void SetMetricsReportingStmtDisabled(
            string deploymentId,
            string statementName);

        /// <summary>
        /// Enable runtime-level metrics reporting.
        /// <para />Use this operation to control, at runtime, metrics reporting globally.
        /// <para />Only if metrics reporting (on the runtime level) has been enabled at initialization time
        /// can metrics reporting be re-enabled at runtime through this method.
        /// </summary>
        /// <throws>ConfigurationException if use at runtime and metrics reporting had not been enabled at initialization time</throws>
        void SetMetricsReportingEnabled();

        /// <summary>
        /// Disable runtime-level metrics reporting.
        /// <para />Use this operation to control, at runtime, metrics reporting globally. Setting metrics reporting
        /// to disabled removes all performance cost for metrics reporting.
        /// </summary>
        /// <throws>ConfigurationException if use at runtime and metrics reporting had not been enabled at initialization time</throws>
        void SetMetricsReportingDisabled();
    }
} // end of namespace
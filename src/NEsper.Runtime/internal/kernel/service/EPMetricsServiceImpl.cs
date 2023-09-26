///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.compat.function;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
    public class EPMetricsServiceImpl : EPMetricsService
    {
        private readonly EPServicesContext services;

        public EPMetricsServiceImpl(EPServicesContext services)
        {
            this.services = services;
        }

        public void SetMetricsReportingInterval(string stmtGroupName, long newIntervalMSec)
        {
            try
            {
                services.MetricReportingService.SetMetricsReportingInterval(stmtGroupName, newIntervalMSec);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error updating interval for metric reporting: " + e.Message, e);
            }
        }

        public void SetMetricsReportingStmtEnabled(string deploymentId, string statementName)
        {
            try
            {
                services.MetricReportingService.SetMetricsReportingStmtEnabled(deploymentId, statementName);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting for statement: " + e.Message, e);
            }
        }

        public void SetMetricsReportingStmtDisabled(string deploymentId, string statementName)
        {
            try
            {
                services.MetricReportingService.SetMetricsReportingStmtDisabled(deploymentId, statementName);
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting for statement: " + e.Message, e);
            }
        }

        public void SetMetricsReportingEnabled()
        {
            try
            {
                services.MetricReportingService.SetMetricsReportingEnabled();
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting: " + e.Message, e);
            }
        }

        public void SetMetricsReportingDisabled()
        {
            try
            {
                services.MetricReportingService.SetMetricsReportingDisabled();
            }
            catch (Exception e)
            {
                throw new ConfigurationException("Error enabling metric reporting: " + e.Message, e);
            }
        }

        public void EnumerateStatementGroups(Consumer<EPMetricsStatementGroup> consumer)
        {
            services.MetricReportingService.EnumerateMetrics(consumer);
        }

        public RuntimeMetric GetRuntimeMetric()
        {
            long inputCount = services.FilterService.NumEventsEvaluated;
            long schedDepth = services.SchedulingService.ScheduleHandleCount;
            return new RuntimeMetric(services.RuntimeURI, services.SchedulingService.Time, inputCount, 0, schedDepth);
        }
    }
} // end of namespace
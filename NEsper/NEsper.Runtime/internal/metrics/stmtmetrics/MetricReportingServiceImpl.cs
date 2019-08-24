///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.common.client.configuration.runtime;
using com.espertech.esper.common.client.metric;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.filtersvc;
using com.espertech.esper.common.@internal.metrics.stmtmetrics;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace com.espertech.esper.runtime.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     Metrics reporting.
    ///     <para />
    ///     Reports for all statements even if not in a statement group, i.e. statement in default group.
    /// </summary>
    public class MetricReportingServiceImpl : MetricReportingServiceSPI,
        MetricEventRouter,
        DeploymentStateListener
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MetricReportingServiceImpl));
        private readonly MetricsExecutor metricsExecutor;
        private readonly string runtimeURI;
        private readonly MetricScheduleService schedule;

        private readonly ConfigurationRuntimeMetricsReporting specification;

        private readonly IDictionary<DeploymentIdNamePair, StatementMetricHandle> statementMetricHandles;
        private readonly StatementMetricRepository stmtMetricRepository;

        private volatile MetricExecutionContext executionContext;

        private bool isScheduled;

        private MetricExecEngine metricExecEngine;
        private MetricExecStatement metricExecStmtGroupDefault;
        private readonly IDictionary<string, MetricExecStatement> statementGroupExecutions;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="specification">configuration</param>
        /// <param name="runtimeURI">runtime URI</param>
        public MetricReportingServiceImpl(
            ConfigurationRuntimeMetricsReporting specification,
            string runtimeURI)
        {
            this.specification = specification;
            this.runtimeURI = runtimeURI;
            if (!specification.IsEnableMetricsReporting) {
                schedule = null;
                stmtMetricRepository = null;
                statementMetricHandles = null;
                metricsExecutor = null;
                return;
            }

            if (specification.IsEnableMetricsReporting) {
                //MetricUtil.Initialize();
            }

            schedule = new MetricScheduleService();

            stmtMetricRepository = new StatementMetricRepository(runtimeURI, specification);
            statementGroupExecutions = new LinkedHashMap<string, MetricExecStatement>();
            statementMetricHandles = new Dictionary<DeploymentIdNamePair, StatementMetricHandle>();
            StatementOutputHooks = new CopyOnWriteArraySet<MetricsStatementResultListener>();

            if (specification.IsThreading) {
                metricsExecutor = new MetricsExecutorThreaded(runtimeURI);
            }
            else {
                metricsExecutor = new MetricsExecutorUnthreaded();
            }
        }

        public ICollection<MetricsStatementResultListener> StatementOutputHooks { get; }

        public void OnDeployment(DeploymentStateEventDeployed @event)
        {
        }

        public void OnUndeployment(DeploymentStateEventUndeployed @event)
        {
            if (!specification.IsEnableMetricsReporting) {
                return;
            }

            foreach (EPStatement stmt in @event.Statements) {
                var pair = new DeploymentIdNamePair(stmt.DeploymentId, stmt.Name);
                stmtMetricRepository.RemoveStatement(pair);
                statementMetricHandles.Remove(pair);
            }
        }

        public void Route(MetricEvent metricEvent)
        {
            executionContext.EpRuntimeSendEvent.SendEventBean(metricEvent, metricEvent.GetType().FullName);
        }

        public bool IsMetricsReportingEnabled => specification.IsEnableMetricsReporting;

        public void AddStatementResultListener(MetricsStatementResultListener listener)
        {
            StatementOutputHooks.Add(listener);
        }

        public void RemoveStatementResultListener(MetricsStatementResultListener listener)
        {
            StatementOutputHooks.Remove(listener);
        }

        public void SetContext(
            FilterService filterService,
            SchedulingService schedulingService,
            EventServiceSendEventCommon eventServiceSendEventInternal)
        {
            var metricsExecutionContext = new MetricExecutionContext(
                filterService, schedulingService, eventServiceSendEventInternal, stmtMetricRepository);

            // create all runtime and statement executions
            metricExecEngine = new MetricExecEngine(this, runtimeURI, schedule, specification.RuntimeInterval);
            metricExecStmtGroupDefault = new MetricExecStatement(this, schedule, specification.StatementInterval, 0);

            var countGroups = 1;
            foreach (var entry in specification.StatementGroups) {
                var config = entry.Value;
                var metricsExecution = new MetricExecStatement(this, schedule, config.Interval, countGroups);
                statementGroupExecutions.Put(entry.Key, metricsExecution);
                countGroups++;
            }

            // last assign this volatile variable so the time event processing may schedule callbacks
            executionContext = metricsExecutionContext;
        }

        public void ProcessTimeEvent(long timeEventTime)
        {
            if (!specification.IsEnableMetricsReporting) {
                return;
            }

            schedule.CurrentTime = timeEventTime;
            if (!isScheduled) {
                if (executionContext != null) {
                    ScheduleExecutions();
                    isScheduled = true;
                }
                else {
                    return; // not initialized yet, race condition and must wait till initialized
                }
            }

            // fast evaluation against nearest scheduled time
            var nearestTime = schedule.NearestTime;
            if (nearestTime == null || nearestTime > timeEventTime) {
                return;
            }

            // get executions
            IList<MetricExec> executions = new List<MetricExec>(2);
            schedule.Evaluate(executions);
            if (executions.IsEmpty()) {
                return;
            }

            // execute
            if (executionContext == null) {
                log.Debug(".processTimeEvent No execution context");
                return;
            }

            foreach (var execution in executions) {
                metricsExecutor.Execute(execution, executionContext);
            }
        }

        public void AccountTime(
            StatementMetricHandle metricsHandle,
            PerformanceMetrics performanceMetrics,
            int numInputEvents)
        {
            stmtMetricRepository.AccountTimes(metricsHandle, performanceMetrics, numInputEvents);
        }

        public void AccountOutput(
            StatementMetricHandle handle,
            int numIStream,
            int numRStream,
            object epStatement,
            object runtime)
        {
            stmtMetricRepository.AccountOutput(handle, numIStream, numRStream);
            if (!StatementOutputHooks.IsEmpty()) {
                var statement = (EPStatement) epStatement;
                var service = (EPRuntime) runtime;
                foreach (var listener in StatementOutputHooks) {
                    listener.Update(numIStream, numRStream, statement, service);
                }
            }
        }

        public StatementMetricHandle GetStatementHandle(
            int statementId,
            string deploymentId,
            string statementName)
        {
            if (!specification.IsEnableMetricsReporting) {
                return new StatementMetricHandle(false);
            }

            var statement = new DeploymentIdNamePair(deploymentId, statementName);
            var handle = stmtMetricRepository.AddStatement(statement);
            statementMetricHandles.Put(statement, handle);
            return handle;
        }

        public void SetMetricsReportingInterval(
            string stmtGroupName,
            long newInterval)
        {
            if (stmtGroupName == null) {
                metricExecStmtGroupDefault.Interval = newInterval;
                return;
            }

            var exec = statementGroupExecutions.Get(stmtGroupName);
            if (exec == null) {
                throw new ArgumentException("Statement group by name '" + stmtGroupName + "' could not be found");
            }

            exec.Interval = newInterval;
        }

        public void SetMetricsReportingStmtDisabled(
            string deploymentId,
            string statementName)
        {
            var handle = statementMetricHandles.Get(new DeploymentIdNamePair(deploymentId, statementName));
            if (handle == null) {
                throw new ConfigurationException("Statement by name '" + statementName + "' not found in metrics collection");
            }

            handle.IsEnabled = false;
        }

        public void SetMetricsReportingStmtEnabled(
            string deploymentId,
            string statementName)
        {
            var handle = statementMetricHandles.Get(new DeploymentIdNamePair(deploymentId, statementName));
            if (handle == null) {
                throw new ConfigurationException("Statement by name '" + statementName + "' not found in metrics collection");
            }

            handle.IsEnabled = true;
        }

        public void SetMetricsReportingEnabled()
        {
            if (!specification.IsEnableMetricsReporting) {
                throw new ConfigurationException("Metrics reporting must be enabled through initialization-time configuration");
            }

            ScheduleExecutions();
        }

        public void SetMetricsReportingDisabled()
        {
            schedule.Clear();
        }

        public void Dispose()
        {
            schedule.Clear();
            metricsExecutor.Dispose();
        }

        private bool IsConsiderSchedule(long value)
        {
            if (value > 0 && value < long.MaxValue) {
                return true;
            }

            return false;
        }

        private void ScheduleExecutions()
        {
            if (!specification.IsEnableMetricsReporting) {
                return;
            }

            if (IsConsiderSchedule(metricExecEngine.Interval)) {
                schedule.Add(metricExecEngine.Interval, metricExecEngine);
            }

            // schedule each statement group, count the "default" group as the first group
            if (IsConsiderSchedule(metricExecStmtGroupDefault.Interval)) {
                schedule.Add(metricExecStmtGroupDefault.Interval, metricExecStmtGroupDefault);
            }

            foreach (MetricExecStatement metricsExecution in statementGroupExecutions.Values) {
                if (IsConsiderSchedule(metricsExecution.Interval)) {
                    schedule.Add(metricsExecution.Interval, metricsExecution);
                }
            }
        }
    }
} // end of namespace
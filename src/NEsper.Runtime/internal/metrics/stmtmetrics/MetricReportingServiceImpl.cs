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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.diagnostics;
using com.espertech.esper.compat.function;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading.locks;
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
        private static readonly ILog Log = LogManager.GetLogger(typeof(MetricReportingServiceImpl));
        
        private readonly MetricsExecutor _metricsExecutor;
        private readonly string _runtimeUri;
        private readonly MetricScheduleService _schedule;

        private readonly ConfigurationRuntimeMetricsReporting _specification;

        private readonly IDictionary<DeploymentIdNamePair, StatementMetricHandle> _statementMetricHandles;
        private readonly StatementMetricRepository _stmtMetricRepository;

        private volatile MetricExecutionContext _executionContext;

        private bool _isScheduled;

        private MetricExecEngine _metricExecEngine;
        private MetricExecStatement _metricExecStmtGroupDefault;
        private readonly IDictionary<string, MetricExecStatement> _statementGroupExecutions;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="specification">configuration</param>
        /// <param name="runtimeUri">runtime URI</param>
        /// <param name="rwLockManager">the read-write lock manager</param>
        public MetricReportingServiceImpl(
            ConfigurationRuntimeMetricsReporting specification,
            string runtimeUri,
            IReaderWriterLockManager rwLockManager)
        {
            this._specification = specification;
            this._runtimeUri = runtimeUri;
            if (!specification.IsEnableMetricsReporting) {
                _schedule = null;
                _stmtMetricRepository = null;
                _statementMetricHandles = null;
                _metricsExecutor = null;
                return;
            }

            if (specification.IsEnableMetricsReporting) {
                //MetricUtil.Initialize();
            }

            _schedule = new MetricScheduleService();

            _stmtMetricRepository = new StatementMetricRepository(runtimeUri, specification, rwLockManager);
            _statementGroupExecutions = new LinkedHashMap<string, MetricExecStatement>();
            _statementMetricHandles = new Dictionary<DeploymentIdNamePair, StatementMetricHandle>();
            StatementOutputHooks = new CopyOnWriteArraySet<MetricsStatementResultListener>();

            if (specification.IsThreading) {
                _metricsExecutor = new MetricsExecutorThreaded(runtimeUri);
            }
            else {
                _metricsExecutor = new MetricsExecutorUnthreaded();
            }
        }

        public ICollection<MetricsStatementResultListener> StatementOutputHooks { get; }

        public void OnDeployment(DeploymentStateEventDeployed @event)
        {
        }

        public void OnUndeployment(DeploymentStateEventUndeployed @event)
        {
            if (!_specification.IsEnableMetricsReporting) {
                return;
            }

            foreach (EPStatement stmt in @event.Statements) {
                var pair = new DeploymentIdNamePair(stmt.DeploymentId, stmt.Name);
                _stmtMetricRepository.RemoveStatement(pair);
                _statementMetricHandles.Remove(pair);
            }
        }

        public void Route(MetricEvent metricEvent)
        {
            _executionContext.EPRuntimeSendEvent.SendEventBean(metricEvent, metricEvent.GetType().FullName);
        }

        public bool IsMetricsReportingEnabled => _specification.IsEnableMetricsReporting;

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
                filterService, schedulingService, eventServiceSendEventInternal, _stmtMetricRepository);

            // create all runtime and statement executions
            _metricExecEngine = new MetricExecEngine(this, _runtimeUri, _schedule, _specification.RuntimeInterval);
            _metricExecStmtGroupDefault = new MetricExecStatement(this, _schedule, _specification.StatementInterval, 0);

            var countGroups = 1;
            foreach (var entry in _specification.StatementGroups) {
                var config = entry.Value;
                var metricsExecution = new MetricExecStatement(this, _schedule, config.Interval, countGroups);
                _statementGroupExecutions.Put(entry.Key, metricsExecution);
                countGroups++;
            }

            // last assign this volatile variable so the time event processing may schedule callbacks
            _executionContext = metricsExecutionContext;
        }

        public void ProcessTimeEvent(long timeEventTime)
        {
            if (!_specification.IsEnableMetricsReporting) {
                return;
            }

            _schedule.CurrentTime = timeEventTime;
            if (!_isScheduled) {
                if (_executionContext != null) {
                    ScheduleExecutions();
                    _isScheduled = true;
                }
                else {
                    return; // not initialized yet, race condition and must wait till initialized
                }
            }

            // fast evaluation against nearest scheduled time
            var nearestTime = _schedule.NearestTime;
            if (nearestTime == null || nearestTime > timeEventTime) {
                return;
            }

            // get executions
            IList<MetricExec> executions = new List<MetricExec>(2);
            _schedule.Evaluate(executions);
            if (executions.IsEmpty()) {
                return;
            }

            // execute
            if (_executionContext == null) {
                Log.Debug(".processTimeEvent No execution context");
                return;
            }

            foreach (var execution in executions) {
                _metricsExecutor.Execute(execution, _executionContext);
            }
        }

        public void AccountTime(
            StatementMetricHandle metricsHandle,
            PerformanceMetrics performanceMetrics,
            int numInputEvents)
        {
            _stmtMetricRepository.AccountTimes(metricsHandle, performanceMetrics, numInputEvents);
        }

        public void AccountOutput(
            StatementMetricHandle handle,
            int numIStream,
            int numRStream,
            object epStatement,
            object runtime)
        {
            _stmtMetricRepository.AccountOutput(handle, numIStream, numRStream);
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
            if (!_specification.IsEnableMetricsReporting) {
                return new StatementMetricHandle(false);
            }

            var statement = new DeploymentIdNamePair(deploymentId, statementName);
            var handle = _stmtMetricRepository.AddStatement(statement);
            _statementMetricHandles.Put(statement, handle);
            return handle;
        }

        public void SetMetricsReportingInterval(
            string stmtGroupName,
            long newInterval)
        {
            if (stmtGroupName == null) {
                _metricExecStmtGroupDefault.Interval = newInterval;
                return;
            }

            var exec = _statementGroupExecutions.Get(stmtGroupName);
            if (exec == null) {
                throw new ArgumentException("Statement group by name '" + stmtGroupName + "' could not be found");
            }

            exec.Interval = newInterval;
        }

        public void SetMetricsReportingStmtDisabled(
            string deploymentId,
            string statementName)
        {
            var handle = _statementMetricHandles.Get(new DeploymentIdNamePair(deploymentId, statementName));
            if (handle == null) {
                throw new ConfigurationException("Statement by name '" + statementName + "' not found in metrics collection");
            }

            handle.IsEnabled = false;
        }

        public void SetMetricsReportingStmtEnabled(
            string deploymentId,
            string statementName)
        {
            var handle = _statementMetricHandles.Get(new DeploymentIdNamePair(deploymentId, statementName));
            if (handle == null) {
                throw new ConfigurationException("Statement by name '" + statementName + "' not found in metrics collection");
            }

            handle.IsEnabled = true;
        }

        public void SetMetricsReportingEnabled()
        {
            if (!_specification.IsEnableMetricsReporting) {
                throw new ConfigurationException("Metrics reporting must be enabled through initialization-time configuration");
            }

            ScheduleExecutions();
        }

        public void SetMetricsReportingDisabled()
        {
            _schedule.Clear();
        }

        public void EnumerateMetrics(Consumer<EPMetricsStatementGroup> consumer)
        {
            if (_stmtMetricRepository == null) {
                throw new IllegalStateException("Metric reporting is not enabled");
            }

            _stmtMetricRepository.EnumerateMetrics(consumer);
        }

        public void Dispose()
        {
            _schedule?.Clear();
            _metricsExecutor?.Dispose();
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
            if (!_specification.IsEnableMetricsReporting) {
                return;
            }

            if (IsConsiderSchedule(_metricExecEngine.Interval)) {
                _schedule.Add(_metricExecEngine.Interval, _metricExecEngine);
            }

            // schedule each statement group, count the "default" group as the first group
            if (IsConsiderSchedule(_metricExecStmtGroupDefault.Interval)) {
                _schedule.Add(_metricExecStmtGroupDefault.Interval, _metricExecStmtGroupDefault);
            }

            foreach (MetricExecStatement metricsExecution in _statementGroupExecutions.Values) {
                if (IsConsiderSchedule(metricsExecution.Interval)) {
                    _schedule.Add(metricsExecution.Interval, metricsExecution);
                }
            }
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.metric;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.service;

namespace com.espertech.esper.epl.metric
{
    /// <summary>
    /// Metrics reporting.
    /// <para/>
    /// Reports for all statements even if not in a statement group, i.e. statement in default group.
    /// </summary>
    public class MetricReportingServiceImpl
        : MetricReportingServiceSPI
        , MetricEventRouter
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ConfigurationMetricsReporting _specification;
        private readonly String _engineUri;

        private volatile MetricExecutionContext _executionContext;

        private bool _isScheduled;
        private readonly MetricScheduleService _schedule;
        private readonly StatementMetricRepository _stmtMetricRepository;

        private MetricExecEngine _metricExecEngine;
        private MetricExecStatement _metricExecStmtGroupDefault;
        private readonly IDictionary<String, MetricExecStatement> _statementGroupExecutions;

        private readonly IDictionary<String, StatementMetricHandle> _statementMetricHandles;
        private readonly MetricsExecutor _metricsExecutor;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="specification">configuration</param>
        /// <param name="engineUri">engine URI</param>
        /// <param name="rwLockManager">The rw lock manager.</param>
        public MetricReportingServiceImpl(
            ConfigurationMetricsReporting specification, String engineUri,
            IReaderWriterLockManager rwLockManager)
        {
            _specification = specification;
            _engineUri = engineUri;
            _schedule = new MetricScheduleService();

            _stmtMetricRepository = new StatementMetricRepository(engineUri, specification, rwLockManager);
            _statementGroupExecutions = new LinkedHashMap<String, MetricExecStatement>();
            _statementMetricHandles = new Dictionary<String, StatementMetricHandle>();
            StatementOutputHooks = new CopyOnWriteArraySet<StatementResultListener>();

            if (specification.IsThreading)
            {
                _metricsExecutor = new MetricsExecutorThreaded(engineUri);
            }
            else
            {
                _metricsExecutor = new MetricsExecutorUnthreaded();
            }
        }

        /// <summary>
        /// Gets the performance collector.
        /// </summary>
        /// <value>The performance collector.</value>
        public PerformanceCollector PerformanceCollector
        {
            get { return AccountTime; }
        }

        public void AddStatementResultListener(StatementResultListener listener)
        {
            StatementOutputHooks.Add(listener);
        }

        public void RemoveStatementResultListener(StatementResultListener listener)
        {
            StatementOutputHooks.Remove(listener);
        }

        public ICollection<StatementResultListener> StatementOutputHooks { get; private set; }

        public void SetContext(EPRuntime runtime, EPServicesContext servicesContext)
        {
            var metricsExecutionContext = new MetricExecutionContext(servicesContext, runtime, _stmtMetricRepository);

            // create all engine and statement executions
            _metricExecEngine = new MetricExecEngine(this, _engineUri, _schedule, _specification.EngineInterval);
            _metricExecStmtGroupDefault = new MetricExecStatement(this, _schedule, _specification.StatementInterval, 0);

            int countGroups = 1;
            foreach (var entry in _specification.StatementGroups)
            {
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
            if (!MetricReportingPath.IsMetricsEnabled)
            {
                return;
            }

            _schedule.CurrentTime = timeEventTime;
            if (!_isScheduled)
            {
                if (_executionContext != null)
                {
                    ScheduleExecutions();
                    _isScheduled = true;
                }
                else
                {
                    return; // not initialized yet, race condition and must wait till initialized
                }
            }

            // fast evaluation against nearest scheduled time
            var nearestTime = _schedule.NearestTime;
            if ((nearestTime == null) || (nearestTime > timeEventTime))
            {
                return;
            }

            // get executions
            var executions = new List<MetricExec>(2);
            _schedule.Evaluate(executions);
            if (executions.IsEmpty())
            {
                return;
            }

            // execute
            if (_executionContext == null)
            {
                Log.Debug(".processTimeEvent No execution context");
                return;
            }

            foreach (MetricExec execution in executions)
            {
                _metricsExecutor.Execute(execution, _executionContext);
            }
        }

        public void Dispose()
        {
            _schedule.Clear();
            _metricsExecutor.Dispose();
        }

        public void Route(MetricEvent metricEvent)
        {
            _executionContext.Runtime.SendEvent(metricEvent);
        }

        public void AccountTime(StatementMetricHandle metricsHandle, long deltaCPU, long deltaWall, int numInputEvents)
        {
            _stmtMetricRepository.AccountTimes(metricsHandle, deltaCPU, deltaWall, numInputEvents);
        }

        public void AccountOutput(StatementMetricHandle handle, int numIStream, int numRStream)
        {
            _stmtMetricRepository.AccountOutput(handle, numIStream, numRStream);
        }

        public StatementMetricHandle GetStatementHandle(int statementId, string statementName)
        {
            if (!MetricReportingPath.IsMetricsEnabled)
            {
                return null;
            }

            StatementMetricHandle handle = _stmtMetricRepository.AddStatement(statementName);
            _statementMetricHandles.Put(statementName, handle);
            return handle;
        }

        public void Observe(StatementLifecycleEvent theEvent)
        {
            if (!MetricReportingPath.IsMetricsEnabled)
            {
                return;
            }

            if (theEvent.EventType == StatementLifecycleEvent.LifecycleEventType.STATECHANGE)
            {
                if (theEvent.Statement.IsDisposed)
                {
                    _stmtMetricRepository.RemoveStatement(theEvent.Statement.Name);
                    _statementMetricHandles.Remove(theEvent.Statement.Name);
                }
            }
        }

        public void SetMetricsReportingInterval(String stmtGroupName, long newInterval)
        {
            if (stmtGroupName == null)
            {
                _metricExecStmtGroupDefault.Interval = newInterval;
                return;
            }

            MetricExecStatement exec = _statementGroupExecutions.Get(stmtGroupName);
            if (exec == null)
            {
                throw new ArgumentException("Statement group by name '" + stmtGroupName + "' could not be found");
            }
            exec.Interval = newInterval;
        }

        private bool IsConsiderSchedule(long value)
        {
            if ((value > 0) && (value < long.MaxValue))
            {
                return true;
            }
            return false;
        }

        public void SetMetricsReportingStmtDisabled(String statementName)
        {
            StatementMetricHandle handle = _statementMetricHandles.Get(statementName);
            if (handle == null)
            {
                throw new ConfigurationException("Statement by name '" + statementName + "' not found in metrics collection");
            }
            handle.IsEnabled = false;
        }

        public void SetMetricsReportingStmtEnabled(String statementName)
        {
            StatementMetricHandle handle = _statementMetricHandles.Get(statementName);
            if (handle == null)
            {
                throw new ConfigurationException("Statement by name '" + statementName + "' not found in metrics collection");
            }
            handle.IsEnabled = true;
        }

        public void SetMetricsReportingEnabled()
        {
            if (!_specification.IsEnableMetricsReporting)
            {
                throw new ConfigurationException("Metrics reporting must be enabled through initialization-time configuration");
            }
            ScheduleExecutions();
            MetricReportingPath.IsMetricsEnabled = true;
        }

        public void SetMetricsReportingDisabled()
        {
            _schedule.Clear();
            MetricReportingPath.IsMetricsEnabled = false;
        }

        private void ScheduleExecutions()
        {
            if (!_specification.IsEnableMetricsReporting)
            {
                return;
            }

            if (IsConsiderSchedule(_metricExecEngine.Interval))
            {
                _schedule.Add(_metricExecEngine.Interval, _metricExecEngine);
            }

            // schedule each statement group, count the "default" group as the first group
            if (IsConsiderSchedule(_metricExecStmtGroupDefault.Interval))
            {
                _schedule.Add(_metricExecStmtGroupDefault.Interval, _metricExecStmtGroupDefault);
            }

            foreach (MetricExecStatement metricsExecution in _statementGroupExecutions.Values)
            {
                if (IsConsiderSchedule(metricsExecution.Interval))
                {
                    _schedule.Add(metricsExecution.Interval, metricsExecution);
                }
            }
        }
    }
}

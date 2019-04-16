///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     Metrics execution producing statement metric events.
    /// </summary>
    public class MetricExecStatement : MetricExec
    {
        private readonly MetricEventRouter metricEventRouter;
        private readonly MetricScheduleService metricScheduleService;
        private readonly int statementGroup;
        private long _interval;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="metricEventRouter">for routing metric events</param>
        /// <param name="metricScheduleService">for scheduling a new execution</param>
        /// <param name="interval">for rescheduling the execution</param>
        /// <param name="statementGroup">group number of statement group</param>
        public MetricExecStatement(
            MetricEventRouter metricEventRouter,
            MetricScheduleService metricScheduleService,
            long interval,
            int statementGroup)
        {
            this.metricEventRouter = metricEventRouter;
            this.metricScheduleService = metricScheduleService;
            Interval = interval;
            this.statementGroup = statementGroup;
        }

        /// <summary>
        ///     Returns reporting interval.
        /// </summary>
        /// <returns>reporting interval</returns>
        public long Interval {
            get => _interval;
            set {
                // Set a new interval, cancels the existing schedule, re-establishes the new
                // schedule if the interval is a positive number.
                _interval = value;
                metricScheduleService.Remove(this);
                if (_interval > 0) {
                    metricScheduleService.Add(Interval, this);
                }
            }
        }

        public void Execute(MetricExecutionContext context)
        {
            var timestamp = metricScheduleService.CurrentTime;
            var metrics = context.StatementMetricRepository.ReportGroup(statementGroup);
            if (metrics != null) {
                for (var i = 0; i < metrics.Length; i++) {
                    var metric = metrics[i];
                    if (metric != null) {
                        metric.Timestamp = timestamp;
                        metricEventRouter.Route(metrics[i]);
                    }
                }
            }

            if (_interval != -1) {
                metricScheduleService.Add(_interval, this);
            }
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.metric;

namespace com.espertech.esper.common.@internal.metrics.stmtmetrics
{
    /// <summary>
    ///     Metrics execution producing runtime metric events.
    /// </summary>
    public class MetricExecEngine : MetricExec
    {
        private readonly MetricEventRouter metricEventRouter;
        private readonly MetricScheduleService metricScheduleService;
        private readonly string runtimeURI;
        private RuntimeMetric lastMetric;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="metricEventRouter">for routing metric events</param>
        /// <param name="runtimeURI">runtime URI</param>
        /// <param name="metricScheduleService">for scheduling a new execution</param>
        /// <param name="interval">for rescheduling the execution</param>
        public MetricExecEngine(
            MetricEventRouter metricEventRouter,
            string runtimeURI,
            MetricScheduleService metricScheduleService,
            long interval)
        {
            this.metricEventRouter = metricEventRouter;
            this.runtimeURI = runtimeURI;
            this.metricScheduleService = metricScheduleService;
            Interval = interval;
        }

        /// <summary>
        ///     Returns reporting interval.
        /// </summary>
        /// <returns>reporting interval</returns>
        public long Interval { get; }

        public void Execute(MetricExecutionContext context)
        {
            long inputCount = context.FilterService.NumEventsEvaluated;
            long schedDepth = context.SchedulingService.ScheduleHandleCount;
            var deltaInputCount = lastMetric == null ? inputCount : inputCount - lastMetric.InputCount;
            var metric = new RuntimeMetric(
                runtimeURI, metricScheduleService.CurrentTime, inputCount, deltaInputCount, schedDepth);
            lastMetric = metric;
            metricEventRouter.Route(metric);
            metricScheduleService.Add(Interval, this);
        }
    }
} // end of namespace
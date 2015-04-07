///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.client.metric;

namespace com.espertech.esper.epl.metric
{
    /// <summary>Metrics execution producing engine metric events. </summary>
    public class MetricExecEngine : MetricExec
    {
        private readonly String _engineURI;
        private readonly MetricEventRouter _metricEventRouter;
        private readonly MetricScheduleService _metricScheduleService;
        private EngineMetric _lastMetric;

        /// <summary>Ctor. </summary>
        /// <param name="metricEventRouter">for routing metric events</param>
        /// <param name="engineURI">engine uri</param>
        /// <param name="metricScheduleService">for scheduling a new execution</param>
        /// <param name="interval">for rescheduling the execution</param>
        public MetricExecEngine(MetricEventRouter metricEventRouter,
                                String engineURI,
                                MetricScheduleService metricScheduleService,
                                long interval)
        {
            _metricEventRouter = metricEventRouter;
            _engineURI = engineURI;
            _metricScheduleService = metricScheduleService;
            Interval = interval;
        }

        /// <summary>Returns reporting interval. </summary>
        /// <value>reporting interval</value>
        public long Interval { get; private set; }

        #region MetricExec Members

        public void Execute(MetricExecutionContext context)
        {
            long inputCount = context.Services.FilterService.NumEventsEvaluated;
            long schedDepth = context.Services.SchedulingService.ScheduleHandleCount;
            long deltaInputCount = _lastMetric == null ? inputCount : inputCount - _lastMetric.InputCount;
            var metric = new EngineMetric(_engineURI, _metricScheduleService.CurrentTime, inputCount, deltaInputCount,
                                          schedDepth);
            _lastMetric = metric;
            _metricEventRouter.Route(metric);
            _metricScheduleService.Add(Interval, this);
        }

        #endregion
    }
}
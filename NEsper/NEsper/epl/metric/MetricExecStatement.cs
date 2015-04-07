///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using com.espertech.esper.client.metric;

namespace com.espertech.esper.epl.metric
{
    /// <summary>Metrics execution producing statement metric events. </summary>
    public class MetricExecStatement : MetricExec
    {
        private readonly MetricEventRouter metricEventRouter;
        private readonly MetricScheduleService metricScheduleService;
        private readonly int statementGroup;
    
        private long interval;
    
        /// <summary>Ctor. </summary>
        /// <param name="metricEventRouter">for routing metric events</param>
        /// <param name="metricScheduleService">for scheduling a new execution</param>
        /// <param name="interval">for rescheduling the execution</param>
        /// <param name="statementGroup">group number of statement group</param>
        public MetricExecStatement(MetricEventRouter metricEventRouter, MetricScheduleService metricScheduleService, long interval, int statementGroup)
        {
            this.metricEventRouter = metricEventRouter;
            this.metricScheduleService = metricScheduleService;
            this.interval = interval;
            this.statementGroup = statementGroup;
        }
    
        public void Execute(MetricExecutionContext context)
        {
            long timestamp = metricScheduleService.CurrentTime;
            StatementMetric[] metrics = context.StatementMetricRepository.ReportGroup(statementGroup);
            if (metrics != null)
            {
                for (int i = 0; i < metrics.Length; i++)
                {
                    StatementMetric metric = metrics[i];
                    if (metric != null)
                    {
                        metric.Timestamp = timestamp;
                        metricEventRouter.Route(metrics[i]);
                    }
                }
            }
            
            if (interval != -1)
            {
                metricScheduleService.Add(interval, this);
            }
        }

        /// <summary>
        /// Set a new interval, cancels the existing schedule, re-establishes the new schedule if the interval is a positive number.
        /// </summary>
        public long Interval
        {
            get { return interval; }
            set
            {
                interval = value;
                metricScheduleService.Remove(this);
                if (interval > 0)
                {
                    metricScheduleService.Add(interval, this);
                }
            }
        }
    }
}

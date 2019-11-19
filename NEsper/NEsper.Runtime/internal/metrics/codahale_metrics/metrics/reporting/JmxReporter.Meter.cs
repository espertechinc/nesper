///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    public partial class JmxReporter
    {
        public class Meter : AbstractBean,
            MeterMBean
        {
            private readonly Metered metric;

            public Meter(
                Metered metric,
                string objectName)
                : base(objectName)
            {
                this.metric = metric;
            }

            public long Count => metric.Count;

            public string EventType => metric.EventType;

            public TimeUnit RateUnit => metric.RateUnit;

            public double MeanRate => metric.MeanRate;

            public double OneMinuteRate => metric.OneMinuteRate;

            public double FiveMinuteRate => metric.FiveMinuteRate;

            public double FifteenMinuteRate => metric.FifteenMinuteRate;
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    public partial class JmxReporter
    {
        public interface MeterMBean : MetricMBean
        {
            long Count { get; }

            string EventType { get; }

            TimeUnit RateUnit { get; }

            double MeanRate { get; }

            double OneMinuteRate { get; }

            double FiveMinuteRate { get; }

            double FifteenMinuteRate { get; }
        }
    }
}
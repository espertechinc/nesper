///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    public partial class JmxReporter
    {
        public class Histogram : HistogramMBean
        {
            private readonly core.Histogram metric;
            private readonly string objectName;

            public Histogram(
                core.Histogram metric,
                string objectName)
            {
                this.metric = metric;
                this.objectName = objectName;
            }

            public string ObjectName()
            {
                return objectName;
            }

            public double P50 => metric.Snapshot.Median;

            public long Count => metric.Count;

            public double Min => metric.Min;

            public double Max => metric.Max;

            public double Mean => metric.Mean;

            public double StdDev => metric.StdDev;

            public double P75 => metric.Snapshot.P75;

            public double P95 => metric.Snapshot.P95;

            public double P98 => metric.Snapshot.P98;

            public double P99 => metric.Snapshot.P99;

            public double P999 => metric.Snapshot.P999;

            public double[] Values => metric.Snapshot.Values;
        }
    }
}
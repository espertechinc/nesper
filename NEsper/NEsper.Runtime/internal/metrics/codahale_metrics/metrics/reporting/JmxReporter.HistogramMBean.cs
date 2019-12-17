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
        public interface HistogramMBean : MetricMBean
        {
            long Count { get; }

            double Min { get; }

            double Max { get; }

            double Mean { get; }

            double StdDev { get; }

            double P50 { get; }

            double P75 { get; }

            double P95 { get; }

            double P98 { get; }

            double P99 { get; }

            double P999 { get; }

            double[] Values { get; }
        }
    }
}
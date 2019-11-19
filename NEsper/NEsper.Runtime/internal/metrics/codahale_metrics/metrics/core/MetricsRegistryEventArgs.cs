///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    public class MetricsRegistryEventArgs : EventArgs
    {
        public MetricName Name { get; set; }
        public Metric Metric { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsRegistryEventArgs"/> class.
        /// </summary>
        public MetricsRegistryEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsRegistryEventArgs"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MetricsRegistryEventArgs(MetricName name)
        {
            Name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricsRegistryEventArgs"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="metric">The metric.</param>
        public MetricsRegistryEventArgs(
            MetricName name,
            Metric metric)
        {
            Name = name;
            Metric = metric;
        }
    }
}

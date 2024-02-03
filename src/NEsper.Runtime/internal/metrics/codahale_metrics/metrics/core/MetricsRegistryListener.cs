///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// Listeners for events from the registry.  Listeners must be thread-safe.
    /// </summary>
    public interface MetricsRegistryListener
    {
        /// <summary>
        /// Called when a metric has been added to the <seealso cref="MetricsRegistry" />.
        /// </summary>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        /// <param name="metric">the <seealso cref="Metric" /></param>
        void OnMetricAdded(MetricName name, Metric metric);

        /// <summary>
        /// Called when a metric has been removed from the <seealso cref="MetricsRegistry" />.
        /// </summary>
        /// <param name="name">the name of the <seealso cref="Metric" /></param>
        void OnMetricRemoved(MetricName name);
    }
} // end of namespace
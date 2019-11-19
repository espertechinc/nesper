///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// A <seealso cref="MetricPredicate" /> is used to determine whether a metric should be included when sorting
    /// and filtering metrics. This is especially useful for limited metric reporting.
    /// </summary>
    public interface MetricPredicate
    {
        /// <summary>
        /// Returns {@code true} if the metric matches the predicate.
        /// </summary>
        /// <param name="name">the name of the metric</param>
        /// <param name="metric">the metric itself</param>
        /// <returns>{@code true} if the predicate applies, {@code false} otherwise</returns>
        bool Matches(MetricName name, Metric metric);
    }
} // end of namespace
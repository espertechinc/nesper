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
    ///     A tag interface to indicate that a class is a metric.
    /// </summary>
    public interface Metric
    {
        /// <summary>Allow the given <seealso cref="MetricProcessor{T}" /> to process this as a metric.</summary>
        /// <typeparam name="T">the type of the context object</typeparam>
        /// <param name="processor">a <seealso cref="MetricProcessor{T}" /></param>
        /// <param name="name">the name of the current metric</param>
        /// <param name="context">a given context which should be passed on to processor</param>
        /// <throws>Exception if something goes wrong</throws>
        void ProcessWith<T>(
            MetricProcessor<T> processor,
            MetricName name,
            T context);
    }
} // end of namespace
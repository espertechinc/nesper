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
    /// A processor of metric instances.
    /// </summary>
    public interface MetricProcessor<T>
    {
        /// <summary>
        /// Process the given <seealso cref="Metered" /> instance.
        /// </summary>
        /// <param name="name">the name of the meter</param>
        /// <param name="meter">the meter</param>
        /// <param name="context">the context of the meter</param>
        /// <throws>Exception if something goes wrong</throws>
        void ProcessMeter(MetricName name, Metered meter, T context);

        /// <summary>
        /// Process the given counter.
        /// </summary>
        /// <param name="name">the name of the counter</param>
        /// <param name="counter">the counter</param>
        /// <param name="context">the context of the meter</param>
        /// <throws>Exception if something goes wrong</throws>
        void ProcessCounter(MetricName name, Counter counter, T context);

        /// <summary>
        /// Process the given histogram.
        /// </summary>
        /// <param name="name">the name of the histogram</param>
        /// <param name="histogram">the histogram</param>
        /// <param name="context">the context of the meter</param>
        /// <throws>Exception if something goes wrong</throws>
        void ProcessHistogram(MetricName name, Histogram histogram, T context);

        /// <summary>
        /// Process the given timer.
        /// </summary>
        /// <param name="name">the name of the timer</param>
        /// <param name="timer">the timer</param>
        /// <param name="context">the context of the meter</param>
        /// <throws>Exception if something goes wrong</throws>
        void ProcessTimer(MetricName name, Timer timer, T context);

        /// <summary>
        /// Process the given gauge.
        /// </summary>
        /// <param name="name">the name of the gauge</param>
        /// <param name="gauge">the gauge</param>
        /// <param name="context">the context of the meter</param>
        /// <throws>Exception if something goes wrong</throws>
        void ProcessGauge<TV>(MetricName name, Gauge<TV> gauge, T context);
    }
} // end of namespace
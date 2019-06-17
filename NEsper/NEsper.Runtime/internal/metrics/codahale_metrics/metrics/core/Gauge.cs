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
    ///     A gauge metric is an instantaneous reading of a particular value. To instrument a queue's depth,
    ///     final Queue&lt;String&gt; queue = new ConcurrentLinkedQueue&lt;String&gt;();
    ///     final Gauge&lt;Integer&gt; queueDepth = new Gauge&lt;Integer&gt;() {
    ///     public Integer value() {
    ///     return queue.size();
    ///     }
    ///     };
    /// </summary>
    public abstract class Gauge<T> : Metric
    {
        /// <summary>
        ///     Returns the metric's current value.
        /// </summary>
        /// <value>the metric's current value</value>
        public abstract T Value { get; }

        public void ProcessWith<U>(
            MetricProcessor<U> processor,
            MetricName name,
            U context)
        {
            processor.ProcessGauge(name, this, context);
        }
    }
} // end of namespace
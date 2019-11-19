///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// An incrementing and decrementing counter metric.
    /// </summary>
    public class Counter : Metric
    {
        private readonly AtomicLong count;

        internal Counter()
        {
            this.count = new AtomicLong(0);
        }

        /// <summary>
        /// Increment the counter by one.
        /// </summary>
        public void Inc()
        {
            Inc(1);
        }

        /// <summary>
        /// Increment the counter by {@code n}.
        /// </summary>
        /// <param name="n">the amount by which the counter will be increased</param>
        public void Inc(long n)
        {
            count.IncrementAndGet(n);
        }

        /// <summary>
        /// Decrement the counter by one.
        /// </summary>
        public void Dec()
        {
            Dec(1);
        }

        /// <summary>
        /// Decrement the counter by {@code n}.
        /// </summary>
        /// <param name="n">the amount by which the counter will be increased</param>
        public void Dec(long n)
        {
            count.IncrementAndGet(0 - n);
        }

        /// <summary>
        /// Returns the counter's current value.
        /// </summary>
        /// <returns>the counter's current value</returns>
        public long Count()
        {
            return count.Get();
        }

        /// <summary>
        /// Resets the counter to 0.
        /// </summary>
        public void Clear()
        {
            count.Set(0);
        }

        public void ProcessWith<T>(
            MetricProcessor<T> processor,
            MetricName name,
            T context)
        {
            processor.ProcessCounter(name, this, context);
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using Antlr4.Runtime.Sharpen;

using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    /// A metric which calculates the distribution of a value.
    /// <para>
    /// See <a href="http://www.johndcook.com/standard_deviation.html">Accurately computing running variance</a>
    /// </para>
    /// </summary>
    public partial class Histogram : Metric,
        Sampling,
        Summarizable
    {
        private static readonly int DEFAULT_SAMPLE_SIZE = 1028;
        private static readonly double DEFAULT_ALPHA = 0.015;

        private readonly Sample sample;
        private readonly AtomicLong min = new AtomicLong();
        private readonly AtomicLong max = new AtomicLong();
        private readonly AtomicLong sum = new AtomicLong();

        // These are for the Welford algorithm for calculating running variance
        // without floating-point doom.
        private readonly AtomicReference<double[]> variance = new AtomicReference<double[]>(new double[] { -1, 0 }); // M, S

        private readonly AtomicLong count = new AtomicLong();

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        internal Histogram(SampleType type)
            : this(type.NewSample())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Histogram"/> class.
        /// </summary>
        /// <param name="sample">The sample.</param>
        internal Histogram(Sample sample)
        {
            this.sample = sample;
            Clear();
        }

        /// <summary>
        /// Clears all recorded values.
        /// </summary>
        public void Clear()
        {
            sample.Clear();
            count.Set(0);
            max.Set(Int64.MinValue);
            min.Set(Int64.MaxValue);
            sum.Set(0);
            variance.Set(new double[] { -1, 0 });
        }

        /// <summary>
        /// Adds a recorded value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Update(long value)
        {
            count.IncrementAndGet();
            sample.Update(value);
            SetMax(value);
            SetMin(value);
            sum.IncrementAndGet(value);
            UpdateVariance(value);
        }

        public Snapshot Snapshot => sample.Snapshot;

        public long Count
        {
            get { return count.Get(); }
        }

        public double Sum
        {
            get { return (double) sum.Get(); }
        }

        public double Max
        {
            get {
                if (Count > 0)
                {
                    return max.Get();
                }

                return 0.0;
            }
        }

        public double Min
        {
            get {
                if (Count > 0)
                {
                    return min.Get();
                }

                return 0.0;
            }
        }

        public double Mean
        {
            get {
                if (Count > 0)
                {
                    return sum.Get() / (double) Count;
                }

                return 0.0;
            }
        }

        public double StdDev
        {
            get {
                if (Count > 0)
                {
                    return Math.Sqrt(Variance);
                }

                return 0.0;
            }
        }

        private double Variance
        {
            get {
                if (Count <= 1)
                {
                    return 0.0;
                }

                return variance.Get()[1] / (Count - 1);
            }
        }

        private void SetMax(long potentialMax)
        {
            var done = false;
            while (!done)
            {
                long currentMax = max.Get();
                done = currentMax >= potentialMax || max.CompareAndSet(currentMax, potentialMax);
            }
        }

        private void SetMin(long potentialMin)
        {
            var done = false;
            while (!done)
            {
                long currentMin = min.Get();
                done = currentMin <= potentialMin || min.CompareAndSet(currentMin, potentialMin);
            }
        }

        private void UpdateVariance(long value)
        {
            while (true)
            {
                double[] oldValues = variance.Get();
                var newValues = new double[2];
                if (oldValues[0] == -1)
                {
                    newValues[0] = value;
                    newValues[1] = 0;
                }
                else
                {
                    var oldM = oldValues[0];
                    var oldS = oldValues[1];

                    var newM = oldM + (value - oldM) / Count;
                    var newS = oldS + (value - oldM) * (value - newM);

                    newValues[0] = newM;
                    newValues[1] = newS;
                }

                if (variance.CompareAndSet(oldValues, newValues))
                {
                    return;
                }
            }
        }

        public void ProcessWith<T>(
            MetricProcessor<T> processor,
            MetricName name,
            T context)
        {
            processor.ProcessHistogram(name, this, context);
        }
    }
} // end of namespace
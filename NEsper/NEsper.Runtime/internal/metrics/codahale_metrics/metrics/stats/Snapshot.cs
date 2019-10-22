///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats
{
    /// <summary>
    /// A statistical snapshot of a <seealso cref="Snapshot" />.
    /// </summary>
    public class Snapshot
    {
        private const double MEDIAN_Q = 0.5;
        private const double P75_Q = 0.75;
        private const double P95_Q = 0.95;
        private const double P98_Q = 0.98;
        private const double P99_Q = 0.99;
        private const double P999_Q = 0.999;

        private readonly double[] values;

        /// <summary>
        /// Create a new <seealso cref="Snapshot" /> with the given values.
        /// </summary>
        /// <param name="values">an unordered set of values in the sample</param>
        public Snapshot(ICollection<long> values)
        {
            long[] copy = values.ToArray();
            this.values = new double[copy.Length];
            for (int i = 0; i < copy.Length; i++)
            {
                this.values[i] = (long) copy[i];
            }
            Array.Sort(this.values);
        }

        /// <summary>
        /// Create a new <seealso cref="Snapshot" /> with the given values.
        /// </summary>
        /// <param name="values">an unordered set of values in the sample</param>
        public Snapshot(double[] values)
        {
            this.values = new double[values.Length];
            Array.Copy(values, 0, this.values, 0, values.Length);
            Array.Sort(this.values);
        }

        /// <summary>
        /// Returns the value at the given quantile.
        /// </summary>
        /// <param name="quantile">a given quantile, in {@code [0..1]}</param>
        /// <returns>the value in the distribution at {@code quantile}</returns>
        public double GetValue(double quantile)
        {
            if (quantile < 0.0 || quantile > 1.0)
            {
                throw new ArgumentException(quantile + " is not in [0..1]");
            }

            if (values.Length == 0)
            {
                return 0.0;
            }

            double pos = quantile * (values.Length + 1);

            if (pos < 1)
            {
                return values[0];
            }

            if (pos >= values.Length)
            {
                return values[values.Length - 1];
            }

            double lower = values[(int) pos - 1];
            double upper = values[(int) pos];
            return lower + (pos - Math.Floor(pos)) * (upper - lower);
        }

        /// <summary>
        /// Returns the number of values in the snapshot.
        /// </summary>
        /// <returns>the number of values in the snapshot</returns>
        public int Size()
        {
            return values.Length;
        }

        /// <summary>
        /// Returns the median value in the distribution.
        /// </summary>
        /// <returns>the median value in the distribution</returns>
        public double Median
        {
            get => GetValue(MEDIAN_Q);
        }

        /// <summary>
        /// Returns the value at the 75th percentile in the distribution.
        /// </summary>
        /// <returns>the value at the 75th percentile in the distribution</returns>
        public double P75
        {
            get => GetValue(P75_Q);
        }

        /// <summary>
        /// Returns the value at the 95th percentile in the distribution.
        /// </summary>
        /// <returns>the value at the 95th percentile in the distribution</returns>
        public double P95
        {
            get => GetValue(P95_Q);
        }

        /// <summary>
        /// Returns the value at the 98th percentile in the distribution.
        /// </summary>
        /// <returns>the value at the 98th percentile in the distribution</returns>
        public double P98
        {
            get => GetValue(P98_Q);
        }

        /// <summary>
        /// Returns the value at the 99th percentile in the distribution.
        /// </summary>
        /// <returns>the value at the 99th percentile in the distribution</returns>
        public double P99
        {
            get => GetValue(P99_Q);
        }

        /// <summary>
        /// Returns the value at the 99.9th percentile in the distribution.
        /// </summary>
        /// <returns>the value at the 99.9th percentile in the distribution</returns>
        public double P999
        {
            get => GetValue(P999_Q);
        }

        /// <summary>
        /// Returns the entire set of values in the snapshot.
        /// </summary>
        /// <returns>the entire set of values in the snapshot</returns>
        public double[] Values
        {
            get => Arrays.CopyOf(values, values.Length);
        }

        /// <summary>
        /// Writes the values of the sample to the given file.
        /// </summary>
        /// <param name="output">the file to which the values will be written</param>
        /// <throws>IOException if there is an error writing the values</throws>
        public void Dump(FileInfo output)
        {
            using (TextWriter writer = output.CreateText()) {
                foreach (double value in values) {
                    writer.WriteLine(value);
                }
            }
        }
    }
} // end of namespace
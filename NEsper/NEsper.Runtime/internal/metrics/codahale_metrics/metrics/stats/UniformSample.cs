///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats
{
    /// <summary>
    ///     A random sample of a stream of {@code long}s. Uses Vitter's Algorithm R to produce a
    ///     statistically representative sample.
    ///     <para>
    ///         @see <a href="http://www.cs.umd.edu/~samir/498/vitter.pdf">Random Sampling with a Reservoir</a>
    ///     </para>
    /// </summary>
    public class UniformSample : Sample
    {
        private const int BITS_PER_LONG = 63;
        private readonly AtomicLong count = new AtomicLong();
        private readonly AtomicLong[] values;

        /// <summary>
        ///     Creates a new <seealso cref="UniformSample" />.
        /// </summary>
        /// <param name="reservoirSize">the number of samples to keep in the sampling reservoir</param>
        public UniformSample(int reservoirSize)
        {
            values = new AtomicLong[reservoirSize];
            Clear();
        }

        public void Clear()
        {
            for (var i = 0; i < values.Length; i++) {
                values[i].Set(0);
            }

            count.Set(0);
        }

        public int Count {
            get {
                var c = count.Get();
                if (c > values.Length) {
                    return values.Length;
                }

                return (int) c;
            }
        }

        public void Update(long value)
        {
            var c = count.IncrementAndGet();
            if (c <= values.Length) {
                values[c - 1].Set(value);
            }
            else {
                var r = NextLong(c);
                if (r < values.Length) {
                    values[r].Set(value);
                }
            }
        }

        public Snapshot Snapshot {
            get {
                var s = Count;
                IList<long> copy = new List<long>(s);
                for (var i = 0; i < s; i++) {
                    copy.Add(values[i].Get());
                }

                return new Snapshot(copy);
            }
        }

        /// <summary>
        ///     Get a pseudo-random long uniformly between 0 and n-1.
        /// </summary>
        /// <param name="n">the bound</param>
        /// <returns>a value select randomly from the range {@code [0..n)}.</returns>
        private static long NextLong(long n)
        {
            long bits, val;
            do {
                bits = ThreadLocalRandom.Current.NextLong(Int64.MaxValue) & ~(1L << BITS_PER_LONG);
                val = bits % n;
            } while (bits - val + (n - 1) < 0L);

            return val;
        }
    }
} // end of namespace
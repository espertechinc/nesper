///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.compat.threading.threadlocal;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats
{
    /// <summary>
    ///     Copied directly from the JSR-166 project.
    /// </summary>
    internal class ThreadLocalRandom
    {
        private const long MULTIPLIER = 0x5DEECE66DL;

        private const long ADDEND = 0xBL;
        private const long MASK = (1L << 48) - 1;

        /// <summary>
        ///     The actual ThreadLocal
        /// </summary>
        private static readonly IThreadLocal<ThreadLocalRandom> LOCAL_RANDOM_THREAD_LOCAL =
            new FastThreadLocal<ThreadLocalRandom>(() => new ThreadLocalRandom());

        /// <summary>
        ///     Initialization flag to permit calls to setSeed to succeed only while executing the Random
        ///     constructor.  We can't allow others since it would cause setting seed in one part of a
        ///     program to unintentionally impact other usages by the thread.
        /// </summary>
        private readonly bool initialized;

        /// <summary>
        ///     The random seed. We can't use super.seed.
        /// </summary>
        private long rnd;

        private readonly Random random;

        /// <summary>
        ///     Constructor called only by localRandom.initialValue.
        /// </summary>
        private ThreadLocalRandom()
        {
            initialized = true;
            random = new Random();
        }

        /// <summary>
        ///     Returns the current thread's {@code ThreadLocalRandom}.
        /// </summary>
        /// <value>the current thread's {@code ThreadLocalRandom}</value>
        public static ThreadLocalRandom Current {
            get { return LOCAL_RANDOM_THREAD_LOCAL.GetOrCreate(); }
        }

        public Random Random => random;

        /// <summary>
        ///     Throws {@code UnsupportedOperationException}.  Setting seeds in this generator is not
        ///     supported.
        /// </summary>
        /// <throws>UnsupportedOperationException always</throws>
        public void SetSeed(long seed)
        {
            if (initialized) {
                throw new UnsupportedOperationException();
            }

            rnd = (seed ^ MULTIPLIER) & MASK;
        }

        public int Next(int bits)
        {
            rnd = (rnd * MULTIPLIER + ADDEND) & MASK;
            return (int) (rnd >> (48 - bits));
        }

        /// <summary>
        ///     Returns a pseudorandom, uniformly distributed value between the given least value (inclusive)
        ///     and bound (exclusive).
        /// </summary>
        /// <param name="least">the least value returned</param>
        /// <param name="bound">the upper bound (exclusive)</param>
        /// <returns>the next value</returns>
        /// <throws>ArgumentException if least greater than or equal to bound</throws>
        public int NextInt(
            int least,
            int bound)
        {
            if (least >= bound) {
                throw new ArgumentException();
            }

            return random.Next(bound - least) + least;
        }

        /// <summary>
        ///     Returns a pseudorandom, uniformly distributed value between 0 (inclusive) and the specified
        ///     value (exclusive).
        /// </summary>
        /// <param name="n">the bound on the random number to be returned.  Must be positive.</param>
        /// <returns>the next value</returns>
        /// <throws>ArgumentException if n is not positive</throws>
        public long NextLong(long n)
        {
            if (n <= 0) {
                throw new ArgumentException("n must be positive");
            }

            // Divide n by two until small enough for nextInt. On each
            // iteration (at most 31 of them but usually much less),
            // randomly choose both whether to include high bit in result
            // (offset) and whether to continue with the lower vs upper
            // half (which makes a difference only if odd).
            long offset = 0;
            while (n >= int.MaxValue) {
                var bits = Next(2);
                var half = n >> 1;
                var nextn = (bits & 2) == 0 ? half : n - half;
                if ((bits & 1) == 0) {
                    offset += n - nextn;
                }

                n = nextn;
            }

            return offset + random.Next((int) n);
        }

        /// <summary>
        ///     Returns a pseudorandom, uniformly distributed value between the given least value (inclusive)
        ///     and bound (exclusive).
        /// </summary>
        /// <param name="least">the least value returned</param>
        /// <param name="bound">the upper bound (exclusive)</param>
        /// <returns>the next value</returns>
        /// <throws>ArgumentException if least greater than or equal to bound</throws>
        public long NextLong(
            long least,
            long bound)
        {
            if (least >= bound) {
                throw new ArgumentException();
            }

            return NextLong(bound - least) + least;
        }

        /// <summary>
        ///     Returns a pseudorandom, uniformly distributed {@code double} value between 0 (inclusive) and
        ///     1.0 (inclusive).
        /// </summary>
        /// <returns>the next value</returns>
        public double NextDouble()
        {
            return random.NextDouble();
        }

        /// <summary>
        ///     Returns a pseudorandom, uniformly distributed {@code double} value between 0 (inclusive) and
        ///     the specified value (exclusive).
        /// </summary>
        /// <param name="n">the bound on the random number to be returned.  Must be positive.</param>
        /// <returns>the next value</returns>
        /// <throws>ArgumentException if n is not positive</throws>
        public double NextDouble(double n)
        {
            if (n <= 0) {
                throw new ArgumentException("n must be positive");
            }

            return random.NextDouble() * n;
        }

        /// <summary>
        ///     Returns a pseudorandom, uniformly distributed value between the given least value (inclusive)
        ///     and bound (exclusive).
        /// </summary>
        /// <param name="least">the least value returned</param>
        /// <param name="bound">the upper bound (exclusive)</param>
        /// <returns>the next value</returns>
        /// <throws>ArgumentException if least greater than or equal to bound</throws>
        public double NextDouble(
            double least,
            double bound)
        {
            if (least >= bound) {
                throw new ArgumentException();
            }

            return random.NextDouble() * (bound - least) + least;
        }
    }
} // end of namespace
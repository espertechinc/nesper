///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats
{
    /// <summary>
    /// An exponentially-weighted moving average.
    /// </summary>
    /// <unknown>@see &lt;a href="http://www.teamquest.com/pdfs/whitepaper/ldavg1.pdf"&gt;UNIX Load Average Part 1: How</unknown>
    /// <unknown>@see &lt;a href="http://www.teamquest.com/pdfs/whitepaper/ldavg2.pdf"&gt;UNIX Load Average Part 2: Not</unknown>
    public class EWMA
    {
        private const int INTERVAL = 5;
        private const double SECONDS_PER_MINUTE = 60.0;
        private const int ONE_MINUTE = 1;
        private const int FIVE_MINUTES = 5;
        private const int FIFTEEN_MINUTES = 15;

        private static readonly double M1_ALPHA = 1.0d - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / ONE_MINUTE);
        private static readonly double M5_ALPHA = 1.0d - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / FIVE_MINUTES);
        private static readonly double M15_ALPHA = 1.0d - Math.Exp(-INTERVAL / SECONDS_PER_MINUTE / FIFTEEN_MINUTES);

        private volatile bool initialized = false;
        private double rate = 0.0;

        private readonly AtomicLong uncounted = new AtomicLong();
        private readonly double alpha, interval;

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX one minute load average and which expects
        /// to be ticked every 5 seconds.
        /// </summary>
        /// <returns>a one-minute EWMA</returns>
        public static EWMA OneMinuteEWMA()
        {
            return new EWMA(M1_ALPHA, INTERVAL, TimeUnit.SECONDS);
        }

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX five minute load average and which expects
        /// to be ticked every 5 seconds.
        /// </summary>
        /// <returns>a five-minute EWMA</returns>
        public static EWMA FiveMinuteEWMA()
        {
            return new EWMA(M5_ALPHA, INTERVAL, TimeUnit.SECONDS);
        }

        /// <summary>
        /// Creates a new EWMA which is equivalent to the UNIX fifteen minute load average and which
        /// expects to be ticked every 5 seconds.
        /// </summary>
        /// <returns>a fifteen-minute EWMA</returns>
        public static EWMA FifteenMinuteEWMA()
        {
            return new EWMA(M15_ALPHA, INTERVAL, TimeUnit.SECONDS);
        }

        /// <summary>
        /// Create a new EWMA with a specific smoothing constant.
        /// </summary>
        /// <param name="alpha">the smoothing constant</param>
        /// <param name="interval">the expected tick interval</param>
        /// <param name="intervalUnit">the time unit of the tick interval</param>
        public EWMA(double alpha, long interval, TimeUnit intervalUnit)
        {
            this.interval = intervalUnit.ToNanos(interval);
            this.alpha = alpha;
        }

        /// <summary>
        /// Update the moving average with a new value.
        /// </summary>
        /// <param name="n">the new value</param>
        public void Update(long n)
        {
            uncounted.IncrementAndGet(n);
        }

        /// <summary>
        /// Mark the passage of time and decay the current rate accordingly.
        /// </summary>
        public void Tick()
        {
            long count = uncounted.GetAndSet(0);
            double instantRate = count / interval;
            if (initialized)
            {
                rate += alpha * (instantRate - rate);
            }
            else
            {
                rate = instantRate;
                initialized = true;
            }
        }

        /// <summary>
        /// Returns the rate in the given units of time.
        /// </summary>
        /// <param name="rateUnit">the unit of time</param>
        /// <returns>the rate</returns>
        public double Rate(TimeUnit rateUnit)
        {
            return rate * (double) rateUnit.ToNanos(1);
        }
    }
} // end of namespace
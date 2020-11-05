///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     A meter metric which measures mean throughput and one-, five-, and fifteen-minute
    ///     exponentially-weighted moving average throughputs.
    ///     <para>
    ///         See <a href="http://en.wikipedia.org/wiki/Moving_average#Exponential_moving_average">EMA</a>
    ///     </para>
    /// </summary>
    public class Meter : Metered,
        Stoppable
    {
        private const long INTERVAL = 5; // seconds
        private readonly Clock clock;

        private readonly AtomicLong count = new AtomicLong();
        private readonly IScheduledFuture _iFuture;
        private readonly EWMA m15Rate = EWMA.FifteenMinuteEWMA();

        private readonly EWMA m1Rate = EWMA.OneMinuteEWMA();
        private readonly EWMA m5Rate = EWMA.FiveMinuteEWMA();
        private readonly long startTime;

        /// <summary>
        ///     Creates a new <seealso cref="Meter" />.
        /// </summary>
        /// <param name="scheduledExecutorService">background thread for updating the rates</param>
        /// <param name="eventType">
        ///     the plural name of the event the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="rateUnit">the rate unit of the new meter</param>
        /// <param name="clock">the clock to use for the meter ticks</param>
        internal Meter(
            IScheduledExecutorService scheduledExecutorService,
            string eventType,
            TimeUnit rateUnit,
            Clock clock)
        {
            RateUnit = rateUnit;
            EventType = eventType;
            _iFuture = scheduledExecutorService.ScheduleAtFixedRate(
                Tick, 
                TimeUnitHelper.ToTimeSpan(INTERVAL, TimeUnit.SECONDS),
                TimeUnitHelper.ToTimeSpan(INTERVAL, TimeUnit.SECONDS));
            this.clock = clock;
            startTime = this.clock.Tick;
        }

        public TimeUnit RateUnit { get; }

        public string EventType { get; }

        public long Count => count.Get();

        public double FifteenMinuteRate => m15Rate.Rate(RateUnit);

        public double FiveMinuteRate => m5Rate.Rate(RateUnit);

        public double MeanRate {
            get {
                if (Count == 0) {
                    return 0.0;
                }

                var elapsed = clock.Tick - startTime;
                return ConvertNsRate(Count / (double) elapsed);
            }
        }

        public double OneMinuteRate => m1Rate.Rate(RateUnit);

        public void ProcessWith<T>(
            MetricProcessor<T> processor,
            MetricName name,
            T context)
        {
            processor.ProcessMeter(name, this, context);
        }

        public void Stop()
        {
            _iFuture.Cancel(false);
        }

        /// <summary>
        ///     Updates the moving averages.
        /// </summary>
        private void Tick()
        {
            m1Rate.Tick();
            m5Rate.Tick();
            m15Rate.Tick();
        }

        /// <summary>
        ///     Mark the occurrence of an event.
        /// </summary>
        public void Mark()
        {
            Mark(1);
        }

        /// <summary>
        ///     Mark the occurrence of a given number of events.
        /// </summary>
        /// <param name="n">the number of events</param>
        public void Mark(long n)
        {
            count.IncrementAndGet(n);
            m1Rate.Update(n);
            m5Rate.Update(n);
            m15Rate.Update(n);
        }

        private double ConvertNsRate(double ratePerNs)
        {
            return ratePerNs * (double) RateUnit.ToNanos(1);
        }
    }
} // end of namespace
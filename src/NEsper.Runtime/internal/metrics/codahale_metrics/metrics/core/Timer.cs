///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.compat;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     A timer metric which aggregates timing durations and provides duration statistics, plus
    ///     throughput statistics via <seealso cref="Meter" />.
    /// </summary>
    public class Timer : Metered,
        Stoppable,
        Sampling,
        Summarizable
    {
        private readonly Clock clock;
        private readonly TimeUnit durationUnit;
        private readonly Histogram histogram = new Histogram(Histogram.BiasedSampleType.INSTANCE);
        private readonly Meter meter;

        /// <summary>
        ///     Creates a new <seealso cref="Timer" />.
        /// </summary>
        /// <param name="tickThread">background thread for updating the rates</param>
        /// <param name="durationUnit">the scale unit for this timer's duration metrics</param>
        /// <param name="rateUnit">the scale unit for this timer's rate metrics</param>
        public Timer(
            IScheduledExecutorService tickThread,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
            : this(tickThread, durationUnit, rateUnit, Clock.DefaultClock)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" />.
        /// </summary>
        /// <param name="tickThread">background thread for updating the rates</param>
        /// <param name="durationUnit">the scale unit for this timer's duration metrics</param>
        /// <param name="rateUnit">the scale unit for this timer's rate metrics</param>
        /// <param name="clock">the clock used to calculate duration</param>
        public Timer(
            IScheduledExecutorService tickThread,
            TimeUnit durationUnit,
            TimeUnit rateUnit,
            Clock clock)
        {
            this.durationUnit = durationUnit;
            RateUnit = rateUnit;
            meter = new Meter(tickThread, "calls", rateUnit, clock);
            this.clock = clock;
            Clear();
        }

        public TimeUnit RateUnit { get; }

        public long Count => histogram.Count;

        public double FifteenMinuteRate => meter.FifteenMinuteRate;

        public double FiveMinuteRate => meter.FiveMinuteRate;

        public double MeanRate => meter.MeanRate;

        public double OneMinuteRate => meter.OneMinuteRate;

        public string EventType => meter.EventType;

        public void ProcessWith<T>(
            MetricProcessor<T> processor,
            MetricName name,
            T context)
        {
            processor.ProcessTimer(name, this, context);
        }

        public Snapshot Snapshot {
            get {
                var values = histogram.Snapshot.Values;
                var converted = new double[values.Length];
                for (var i = 0; i < values.Length; i++) {
                    converted[i] = ConvertFromNS(values[i]);
                }

                return new Snapshot(converted);
            }
        }

        public void Stop()
        {
            meter.Stop();
        }

        /// <summary>
        ///     Returns the longest recorded duration.
        /// </summary>
        /// <returns>the longest recorded duration</returns>
        public double Max => ConvertFromNS(histogram.Max);

        /// <summary>
        ///     Returns the shortest recorded duration.
        /// </summary>
        /// <returns>the shortest recorded duration</returns>
        public double Min => ConvertFromNS(histogram.Min);

        /// <summary>
        ///     Returns the arithmetic mean of all recorded durations.
        /// </summary>
        /// <returns>the arithmetic mean of all recorded durations</returns>
        public double Mean => ConvertFromNS(histogram.Mean);

        /// <summary>
        ///     Returns the standard deviation of all recorded durations.
        /// </summary>
        /// <returns>the standard deviation of all recorded durations</returns>
        public double StdDev => ConvertFromNS(histogram.StdDev);

        /// <summary>
        ///     Returns the sum of all recorded durations.
        /// </summary>
        /// <returns>the sum of all recorded durations</returns>
        public double Sum => ConvertFromNS(histogram.Sum);

        /// <summary>
        ///     Returns the timer's duration scale unit.
        /// </summary>
        /// <returns>the timer's duration scale unit</returns>
        public TimeUnit DurationUnit()
        {
            return durationUnit;
        }

        /// <summary>
        ///     Clears all recorded durations.
        /// </summary>
        public void Clear()
        {
            histogram.Clear();
        }

        /// <summary>
        ///     Adds a recorded duration.
        /// </summary>
        /// <param name="duration">the length of the duration</param>
        /// <param name="unit">the scale unit of {@code duration}</param>
        public void Update(
            long duration,
            TimeUnit unit)
        {
            Update(unit.ToNanos(duration));
        }

        /// <summary>
        ///     Times and records the duration of event.
        /// </summary>
        /// <param name="event">
        ///     a <seealso cref="Callable" /> whose Call method implements a process whose duration should be timed
        /// </param>
        /// <typeparam name="T">the type of the value returned by {@code event}</typeparam>
        /// <returns>the value returned by {@code event}</returns>
        /// <throws>Exception if {@code event} throws an <seealso cref="Exception" /></throws>
        public T Time<T>(Callable<T> @event)
        {
            var startTime = clock.Tick;
            try {
                return @event.Invoke();
            }
            finally {
                Update(clock.Tick - startTime);
            }
        }

        /// <summary>
        ///     Returns a timing <seealso cref="TimerContext" />, which measures an elapsed time in nanoseconds.
        /// </summary>
        /// <returns>a new <seealso cref="TimerContext" /></returns>
        public TimerContext Time()
        {
            return new TimerContext(this, clock);
        }

        private void Update(long duration)
        {
            if (duration >= 0) {
                histogram.Update(duration);
                meter.Mark();
            }
        }

        private double ConvertFromNS(double ns)
        {
            return ns / TimeUnitHelper.Convert(1, TimeUnit.NANOSECONDS, durationUnit);
        }
    }
} // end of namespace
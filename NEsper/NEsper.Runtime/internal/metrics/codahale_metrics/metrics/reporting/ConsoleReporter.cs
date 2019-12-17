///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.IO;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    /// <summary>
    ///     A simple reporters which prints out application metrics to a <seealso cref="TextWriter" /> periodically.
    /// </summary>
    public class ConsoleReporter : AbstractPollingReporter,
        MetricProcessor<TextWriter>
    {
        private const int CONSOLE_WIDTH = 80;

        private readonly Clock _clock;
        private readonly CultureInfo _locale;
        private readonly TextWriter _writer;
        private readonly MetricPredicate _predicate;
        private readonly TimeZoneInfo _timeZone;

        /// <summary>
        ///     Creates a new <seealso cref="ConsoleReporter" /> for the default metrics registry, with unrestricted
        ///     output.
        /// </summary>
        /// <param name="writer">the <seealso cref="TextWriter" /> to which output will be written</param>
        public ConsoleReporter(TextWriter writer)
            : this(Metrics.DefaultRegistry(), writer, AnyMetricPredicate.INSTANCE)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="ConsoleReporter" /> for a given metrics registry.
        /// </summary>
        /// <param name="metricsRegistry">the metrics registry</param>
        /// <param name="writer">the <seealso cref="TextWriter" /> to which output will be written</param>
        /// <param name="predicate">
        ///     the <seealso cref="MetricPredicate" /> used to determine whether a metric will be output
        /// </param>
        public ConsoleReporter(
            MetricsRegistry metricsRegistry,
            TextWriter writer,
            MetricPredicate predicate)
            : this(metricsRegistry, writer, predicate, Clock.DefaultClock, TimeZoneInfo.Utc)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="ConsoleReporter" /> for a given metrics registry.
        /// </summary>
        /// <param name="metricsRegistry">the metrics registry</param>
        /// <param name="writer">the <seealso cref="TextWriter" /> to which output will be written</param>
        /// <param name="predicate">
        ///     the <seealso cref="MetricPredicate" /> used to determine whether a metric will be output
        /// </param>
        /// <param name="clock">the <seealso cref="Clock" /> used to print time</param>
        /// <param name="timeZone">the <seealso cref="TimeZone" /> used to print time</param>
        public ConsoleReporter(
            MetricsRegistry metricsRegistry,
            TextWriter writer,
            MetricPredicate predicate,
            Clock clock,
            TimeZoneInfo timeZone)
            : this(metricsRegistry, writer, predicate, clock, timeZone, CultureInfo.CurrentCulture)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="ConsoleReporter" /> for a given metrics registry.
        /// </summary>
        /// <param name="metricsRegistry">the metrics registry</param>
        /// <param name="writer">the <seealso cref="TextWriter" /> to which output will be written</param>
        /// <param name="predicate">
        ///     the <seealso cref="MetricPredicate" /> used to determine whether a metric will be output
        /// </param>
        /// <param name="clock">the <seealso cref="Clock" /> used to print time</param>
        /// <param name="timeZone">the <seealso cref="TimeZone" /> used to print time</param>
        /// <param name="locale">the <seealso cref="CultureInfo" /> used to print values</param>
        public ConsoleReporter(
            MetricsRegistry metricsRegistry,
            TextWriter writer,
            MetricPredicate predicate,
            Clock clock,
            TimeZoneInfo timeZone,
            CultureInfo locale)
            : base(metricsRegistry, "console-reporter")
        {
            _writer = writer;
            _predicate = predicate;
            _clock = clock;
            _timeZone = timeZone;
            _locale = locale;
        }

        public void ProcessCounter(
            MetricName name,
            Counter counter,
            TextWriter stream)
        {
            stream.Write(string.Format(_locale, "    count = {0}\n", counter.Count()));
        }

        public void ProcessMeter(
            MetricName name,
            Metered meter,
            TextWriter stream)
        {
            var unit = Abbrev(meter.RateUnit);
            stream.Write(
                string.Format(_locale, "             count = {0}\n", meter.Count));
            stream.Write(
                string.Format(
                    _locale, "         mean rate = {0:##.##} {1}/{2}\n",
                    meter.MeanRate,
                    meter.EventType,
                    unit));
            stream.Write(
                string.Format(
                    _locale, "     1-minute rate = {0:##.##} {1}/{2}\n",
                    meter.OneMinuteRate,
                    meter.EventType,
                    unit));
            stream.Write(
                string.Format(
                    _locale, "     5-minute rate = {0:##.##} {1}/{2}\n",
                    meter.FiveMinuteRate,
                    meter.EventType,
                    unit));
            stream.Write(
                string.Format(
                    _locale,
                    "    15-minute rate = {0:##.##} {1}/{2}\n",
                    meter.FifteenMinuteRate,
                    meter.EventType,
                    unit));
        }

        public void ProcessHistogram(
            MetricName name,
            Histogram histogram,
            TextWriter stream)
        {
            var snapshot = histogram.Snapshot;
            stream.Write(string.Format(_locale, "               min = {0:##.##}\n", histogram.Min));
            stream.Write(string.Format(_locale, "               max = {0:##.##}\n", histogram.Max));
            stream.Write(string.Format(_locale, "              mean = {0:##.##}\n", histogram.Mean));
            stream.Write(string.Format(_locale, "            stddev = {0:##.##}\n", histogram.StdDev));
            stream.Write(string.Format(_locale, "            median = {0:##.##}\n", snapshot.Median));
            stream.Write(string.Format(_locale, "              75%% <= {0:##.##}\n", snapshot.P75));
            stream.Write(string.Format(_locale, "              95%% <= {0:##.##}\n", snapshot.P95));
            stream.Write(string.Format(_locale, "              98%% <= {0:##.##}\n", snapshot.P98));
            stream.Write(string.Format(_locale, "              99%% <= {0:##.##}\n", snapshot.P99));
            stream.Write(string.Format(_locale, "            99.9%% <= {0:##.##}\n", snapshot.P999));
        }

        public void ProcessTimer(
            MetricName name,
            Timer timer,
            TextWriter stream)
        {
            ProcessMeter(name, timer, stream);
            var durationUnit = Abbrev(timer.DurationUnit());
            var snapshot = timer.Snapshot;
            stream.Write(string.Format(_locale, "               min = {0:##.##}{1}\n", timer.Min, durationUnit));
            stream.Write(string.Format(_locale, "               max = {0:##.##}{1}\n", timer.Max, durationUnit));
            stream.Write(string.Format(_locale, "              mean = {0:##.##}{1}\n", timer.Mean, durationUnit));
            stream.Write(string.Format(_locale, "            stddev = {0:##.##}{1}\n", timer.StdDev, durationUnit));
            stream.Write(string.Format(_locale, "            median = {0:##.##}{1}\n", snapshot.Median, durationUnit));
            stream.Write(string.Format(_locale, "              75%% <= {0:##.##}{1}\n", snapshot.P75, durationUnit));
            stream.Write(string.Format(_locale, "              95%% <= {0:##.##}{1}\n", snapshot.P95, durationUnit));
            stream.Write(string.Format(_locale, "              98%% <= {0:##.##}{1}\n", snapshot.P98, durationUnit));
            stream.Write(string.Format(_locale, "              99%% <= {0:##.##}{1}\n", snapshot.P99, durationUnit));
            stream.Write(string.Format(_locale, "            99.9%% <= {0:##.##}{1}\n", snapshot.P999, durationUnit));
        }

        /// <summary>
        ///     Enables the console reporter for the default metrics registry, and causes it to print to
        ///     STDOUT with the specified period.
        /// </summary>
        /// <param name="period">the period between successive outputs</param>
        /// <param name="unit">the time unit of {@code period}</param>
        public static void Enable(
            long period,
            TimeUnit unit)
        {
            Enable(Metrics.DefaultRegistry(), period, unit);
        }

        /// <summary>
        ///     Enables the console reporter for the given metrics registry, and causes it to print to STDOUT
        ///     with the specified period and unrestricted output.
        /// </summary>
        /// <param name="metricsRegistry">the metrics registry</param>
        /// <param name="period">the period between successive outputs</param>
        /// <param name="unit">the time unit of {@code period}</param>
        public static void Enable(
            MetricsRegistry metricsRegistry,
            long period,
            TimeUnit unit)
        {
            var reporter = new ConsoleReporter(
                metricsRegistry,
                Console.Out,
                AnyMetricPredicate.INSTANCE);
            reporter.Start(period, unit);
        }

        public override void Run()
        {
            try {
                var format = new SimpleDateFormat("s", _locale);
                var dateTime = DateTimeEx.GetInstance(_timeZone, _clock.Time());
                var dateTimeString = format.Format(dateTime);
                _writer.Write(dateTimeString);
                _writer.Write(' ');
                for (var i = 0; i < CONSOLE_WIDTH - dateTimeString.Length - 1; i++) {
                    _writer.Write('=');
                }

                _writer.WriteLine();
                foreach (var entry in MetricsRegistry.GroupedMetrics(_predicate)) {
                    _writer.Write(entry.Key);
                    _writer.WriteLine(':');
                    foreach (var subEntry in entry.Value) {
                        _writer.Write("  ");
                        _writer.Write(subEntry.Key.Name);
                        _writer.WriteLine(':');
                        subEntry.Value.ProcessWith(this, subEntry.Key, _writer);
                        _writer.WriteLine();
                    }

                    _writer.WriteLine();
                }

                _writer.WriteLine();
                _writer.Flush();
            }
            catch (Exception e) {
                _writer.WriteLine(e.StackTrace);
            }
        }

        public void ProcessGauge<T>(
            MetricName name,
            Gauge<T> gauge,
            TextWriter stream)
        {
            stream.Write(string.Format(_locale, "    value = {0}\n", gauge.Value));
        }

        private string Abbrev(TimeUnit unit)
        {
            switch (unit) {
                case TimeUnit.NANOSECONDS:
                    return "ns";

                case TimeUnit.MICROSECONDS:
                    return "us";

                case TimeUnit.MILLISECONDS:
                    return "ms";

                case TimeUnit.SECONDS:
                    return "s";

                case TimeUnit.MINUTES:
                    return "m";

                case TimeUnit.HOURS:
                    return "h";

                case TimeUnit.DAYS:
                    return "d";

                default:
                    throw new ArgumentException("Unrecognized TimeUnit: " + unit);
            }
        }
    }
} // end of namespace
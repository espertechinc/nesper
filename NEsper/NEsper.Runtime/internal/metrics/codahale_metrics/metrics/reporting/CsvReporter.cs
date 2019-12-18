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
using System.Text;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.stats;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    /// <summary>
    /// A reporter which periodically appends data from each metric to a metric-specific CSV file in
    /// an output directory.
    /// </summary>
    public class CsvReporter : AbstractPollingReporter,
            MetricProcessor<CsvReporter.Context>
    {
        /// <summary>
        /// Enables the CSV reporter for the default metrics registry, and causes it to write to files in
        /// {@code outputDir} with the specified period.
        /// </summary>
        /// <param name="outputDir">the directory in which {@code .csv} files will be created</param>
        /// <param name="period">the period between successive outputs</param>
        /// <param name="unit">the time unit of {@code period}</param>
        public static void Enable(DirectoryInfo outputDir, long period, TimeUnit unit)
        {
            Enable(Metrics.DefaultRegistry(), outputDir, period, unit);
        }

        /// <summary>
        /// Enables the CSV reporter for the given metrics registry, and causes it to write to files in
        /// {@code outputDir} with the specified period.
        /// </summary>
        /// <param name="metricsRegistry">the metrics registry</param>
        /// <param name="outputDir">the directory in which {@code .csv} files will be created</param>
        /// <param name="period">the period between successive outputs</param>
        /// <param name="unit">the time unit of {@code period}</param>
        public static void Enable(MetricsRegistry metricsRegistry, DirectoryInfo outputDir, long period, TimeUnit unit)
        {
            CsvReporter reporter = new CsvReporter(metricsRegistry, outputDir);
            reporter.Start(period, unit);
        }

        private readonly MetricPredicate predicate;
        private readonly DirectoryInfo outputDir;
        private readonly IDictionary<MetricName, TextWriter> streamMap;
        private readonly Clock clock;
        private long startTime;

        /// <summary>
        /// Creates a new <seealso cref="CsvReporter" /> which will write all metrics from the given
        /// <seealso cref="MetricsRegistry" /> to CSV files in the given output directory.
        /// </summary>
        /// <param name="outputDir">the directory to which files will be written</param>
        /// <param name="metricsRegistry">the <seealso cref="MetricsRegistry" /> containing the metrics this reporter will report
        /// </param>
        public CsvReporter(MetricsRegistry metricsRegistry, DirectoryInfo outputDir) : 
            this(metricsRegistry, AnyMetricPredicate.INSTANCE, outputDir)
        {
        }

        /// <summary>
        /// Creates a new <seealso cref="CsvReporter" /> which will write metrics from the given
        /// <seealso cref="MetricsRegistry" /> which match the given <seealso cref="MetricPredicate" /> to CSV files in the
        /// given output directory.
        /// </summary>
        /// <param name="metricsRegistry">the <seealso cref="MetricsRegistry" /> containing the metrics this reporter will report
        /// </param>
        /// <param name="predicate">the <seealso cref="MetricPredicate" /> which metrics are required to match before being written to files
        /// </param>
        /// <param name="outputDir">the directory to which files will be written</param>
        public CsvReporter(MetricsRegistry metricsRegistry,
                           MetricPredicate predicate,
                           DirectoryInfo outputDir) :
            this(metricsRegistry, predicate, outputDir, Clock.DefaultClock)
        {
        }

        /// <summary>
        /// Creates a new <seealso cref="CsvReporter" /> which will write metrics from the given
        /// <seealso cref="MetricsRegistry" /> which match the given <seealso cref="MetricPredicate" /> to CSV files in the
        /// given output directory.
        /// </summary>
        /// <param name="metricsRegistry">the <seealso cref="MetricsRegistry" /> containing the metrics this reporter will report
        /// </param>
        /// <param name="predicate">the <seealso cref="MetricPredicate" /> which metrics are required to match before being written to files
        /// </param>
        /// <param name="outputDir">the directory to which files will be written</param>
        /// <param name="clock">the clock used to measure time</param>
        public CsvReporter(MetricsRegistry metricsRegistry,
                           MetricPredicate predicate,
                           DirectoryInfo outputDir,
                           Clock clock) : base(metricsRegistry, "csv-reporter")
        {
            if (!outputDir.Exists)
            {
                throw new ArgumentException(outputDir + " does not exist (or is not a directory)");
            }
            this.outputDir = outputDir;
            this.predicate = predicate;
            this.streamMap = new Dictionary<MetricName, TextWriter>();
            this.startTime = 0L;
            this.clock = clock;
        }

        /// <summary>
        /// Returns an opened <seealso cref="TextWriter" /> for the given <seealso cref="MetricName" /> which outputs data
        /// to a metric-specific {@code .csv} file in the output directory.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <returns>an opened <seealso cref="TextWriter" /> specific to {@code metricName}</returns>
        /// <throws>IOException if there is an error opening the stream</throws>
        protected TextWriter CreateStreamForMetric(MetricName metricName)
        {
            var newFile = new FileInfo(Path.Combine(outputDir.ToString(), metricName.Name + ".csv"));
            return newFile.CreateText();
        }

        public override void Run()
        {
            long time = TimeUnit.MILLISECONDS.ToSeconds(clock.Time() - startTime);
            var metrics = MetricsRegistry.AllMetrics;
            try
            {
                foreach (var entry in metrics)
                {
                    MetricName metricName = entry.Key;
                    Metric metric = entry.Value;
                    if (predicate.Matches(metricName, metric)) {
                        metric.ProcessWith(this, entry.Key, header => {
                            var textWriter = GetTextWriter(metricName, header);
                            textWriter.Write(time);
                            textWriter.Write(',');
                            return textWriter;
                        });
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
            }
        }

        public void ProcessMeter(
            MetricName name,
            Metered meter,
            Context context)
        {
            TextWriter stream = context.Invoke(
                "# time,count,1 min rate,mean rate,5 min rate,15 min rate");
            stream.WriteLine(
                new StringBuilder()
                    .Append(meter.Count).Append(',')
                    .Append(meter.OneMinuteRate).Append(',')
                    .Append(meter.MeanRate).Append(',')
                    .Append(meter.FiveMinuteRate).Append(',')
                    .Append(meter.FifteenMinuteRate)
                    .ToString()
            );
            stream.Flush();
        }

        public void ProcessCounter(MetricName name, Counter counter, Context context)
        {
            TextWriter stream = context.Invoke("# time,count");
            stream.WriteLine(counter.Count());
            stream.Flush();
        }

        public void ProcessHistogram(
            MetricName name,
            Histogram histogram,
            Context context)
        {
            TextWriter stream = context.Invoke("# time,min,max,mean,median,stddev,95%,99%,99.9%");
            Snapshot snapshot = histogram.Snapshot;
            stream.WriteLine(
                new StringBuilder()
                    .Append(histogram.Min).Append(',')
                    .Append(histogram.Max).Append(',')
                    .Append(histogram.Mean).Append(',')
                    .Append(snapshot.Median).Append(',')
                    .Append(histogram.StdDev).Append(',')
                    .Append(snapshot.P95).Append(',')
                    .Append(snapshot.P99).Append(',')
                    .Append(snapshot.P999)
                    .ToString()
                );
            stream.WriteLine();
            stream.Flush();
        }

        public void ProcessTimer(
            MetricName name,
            Timer timer,
            Context context)
        {
            TextWriter stream = context.Invoke("# time,min,max,mean,median,stddev,95%,99%,99.9%");
            Snapshot snapshot = timer.Snapshot;
            stream.WriteLine(
                new StringBuilder()
                    .Append(timer.Min).Append(',')
                    .Append(timer.Max).Append(',')
                    .Append(timer.Mean).Append(',')
                    .Append(snapshot.Median).Append(',')
                    .Append(timer.StdDev).Append(',')
                    .Append(snapshot.P95).Append(',')
                    .Append(snapshot.P99).Append(',')
                    .Append(snapshot.P999).ToString());
            stream.Flush();
        }

        public void ProcessGauge<TV>(
            MetricName name,
            Gauge<TV> gauge,
            Context context)
        {
            TextWriter stream = context.Invoke("# time,value");
            stream.WriteLine(gauge.Value);
            stream.Flush();
        }

        public override void Start(long period, TimeUnit unit)
        {
            this.startTime = clock.Time();
            base.Start(period, unit);
        }

        public override void Shutdown()
        {
            try
            {
                base.Shutdown();
            }
            finally
            {
                foreach (TextWriter @out in streamMap.Values)
                {
                    @out.Close();
                }
            }
        }

        private TextWriter GetTextWriter(MetricName metricName, string header)
        {
            TextWriter stream;
            lock (streamMap) {
                stream = streamMap.Get(metricName);
                if (stream == null)
                {
                    stream = CreateStreamForMetric(metricName);
                    streamMap.Put(metricName, stream);
                    stream.WriteLine(header);
                }
            }
            return stream;
        }

        /// <summary>
        /// The context used to output metrics.
        /// <para>
        /// Returns an open <seealso cref="TextWriter" /> for the metric with {@code header} already written
        /// to it.
        /// </para>
        /// </summary>
        /// <param name="header">the CSV header</param>
        /// <returns>an open <seealso cref="TextWriter" /></returns>
        public delegate TextWriter Context(string header);
    }
} // end of namespace
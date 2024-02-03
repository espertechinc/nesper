///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core
{
    /// <summary>
    ///     A registry of metric instances.
    /// </summary>
    public class MetricsRegistry
    {
        private readonly Clock clock;
        private readonly ConcurrentDictionary<MetricName, Metric> metrics;
        private readonly ThreadPools threadPools;

        /// <summary>
        ///     Creates a new <seealso cref="MetricsRegistry" />.
        /// </summary>
        public MetricsRegistry()
            : this(Clock.DefaultClock)
        {
        }

        /// <summary>
        ///     Creates a new <seealso cref="MetricsRegistry" /> with the given <seealso cref="Clock" /> instance.
        /// </summary>
        /// <param name="clock">a <seealso cref="Clock"/> instance</param>
        public MetricsRegistry(Clock clock)
        {
            this.clock = clock;
            metrics = NewMetricsMap();
            threadPools = new ThreadPools();
        }

        /// <summary>
        ///     Given a new <seealso cref="Gauge{T}" />, registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="metric">the metric</param>
        /// <typeparam name="T">the type of the value returned by the metric</typeparam>
        /// <returns>{@code metric}</returns>
        public Gauge<T> NewGauge<T>(
            Type klass,
            string name,
            Gauge<T> metric)
        {
            return NewGauge(klass, name, null, metric);
        }

        /// <summary>
        ///     Given a new <seealso cref="Gauge{T}" />, registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="metric">the metric</param>
        /// <typeparam name="T">the type of the value returned by the metric</typeparam>
        /// <returns>{@code metric}</returns>
        public Gauge<T> NewGauge<T>(
            Type klass,
            string name,
            string scope,
            Gauge<T> metric)
        {
            return NewGauge(CreateName(klass, name, scope), metric);
        }

        /// <summary>
        ///     Given a new <seealso cref="Gauge{T}" />, registers it under the given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="metric">the metric</param>
        /// <typeparam name="T">the type of the value returned by the metric</typeparam>
        /// <returns>{@code metric}</returns>
        public Gauge<T> NewGauge<T>(
            MetricName metricName,
            Gauge<T> metric)
        {
            return GetOrAdd(metricName, metric);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Counter" /> and registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <returns>a new <seealso cref="Counter"/></returns>
        public Counter NewCounter(
            Type klass,
            string name)
        {
            return NewCounter(klass, name, null);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Counter" /> and registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <returns>a new <seealso cref="Counter"/></returns>
        public Counter NewCounter(
            Type klass,
            string name,
            string scope)
        {
            return NewCounter(CreateName(klass, name, scope));
        }

        /// <summary>
        ///     Creates a new <seealso cref="Counter" /> and registers it under the given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <returns>a new <seealso cref="Counter"/></returns>
        public Counter NewCounter(MetricName metricName)
        {
            return GetOrAdd(metricName, new Counter());
        }

        /// <summary>
        ///     Creates a new <seealso cref="Histogram" /> and registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="biased">whether or not the histogram should be biased</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public Histogram NewHistogram(
            Type klass,
            string name,
            bool biased)
        {
            return NewHistogram(klass, name, null, biased);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Histogram" /> and registers it under the given class, name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="biased">whether or not the histogram should be biased</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public Histogram NewHistogram(
            Type klass,
            string name,
            string scope,
            bool biased)
        {
            return NewHistogram(CreateName(klass, name, scope), biased);
        }

        /// <summary>
        ///     Creates a new non-biased <seealso cref="Histogram" /> and registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public Histogram NewHistogram(
            Type klass,
            string name)
        {
            return NewHistogram(klass, name, false);
        }

        /// <summary>
        ///     Creates a new non-biased <seealso cref="Histogram" /> and registers it under the given class, name, and
        ///     scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public Histogram NewHistogram(
            Type klass,
            string name,
            string scope)
        {
            return NewHistogram(klass, name, scope, false);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Histogram" /> and registers it under the given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="biased">whether or not the histogram should be biased</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public Histogram NewHistogram(
            MetricName metricName,
            bool biased)
        {
            return GetOrAdd(
                metricName,
                new Histogram(biased
                    ? (Histogram.SampleType) Histogram.BiasedSampleType.INSTANCE
                    : (Histogram.SampleType) Histogram.UniformSampleType.INSTANCE));
        }

        /// <summary>
        ///     Creates a new <seealso cref="Meter" /> and registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="eventType">
        ///     the plural name of the type of events the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="unit">the rate unit of the new meter</param>
        /// <returns>a new <seealso cref="Meter"/></returns>
        public Meter NewMeter(
            Type klass,
            string name,
            string eventType,
            TimeUnit unit)
        {
            return NewMeter(klass, name, null, eventType, unit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Meter" /> and registers it under the given class, name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="eventType">
        ///     the plural name of the type of events the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="unit">the rate unit of the new meter</param>
        /// <returns>a new <seealso cref="Meter"/></returns>
        public Meter NewMeter(
            Type klass,
            string name,
            string scope,
            string eventType,
            TimeUnit unit)
        {
            return NewMeter(CreateName(klass, name, scope), eventType, unit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Meter" /> and registers it under the given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="eventType">
        ///     the plural name of the type of events the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="unit">the rate unit of the new meter</param>
        /// <returns>a new <seealso cref="Meter"/></returns>
        public Meter NewMeter(
            MetricName metricName,
            string eventType,
            TimeUnit unit)
        {
            Metric existingMetric = metrics.Get(metricName);
            if (existingMetric != null)
            {
                return (Meter) existingMetric;
            }

            return GetOrAdd(metricName, new Meter(NewMeterTickThreadPool(), eventType, unit, clock));
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class and name, measuring
        ///     elapsed time in milliseconds and invocations per second.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public Timer NewTimer(
            Type klass,
            string name)
        {
            return NewTimer(klass, name, null, TimeUnit.MILLISECONDS, TimeUnit.SECONDS);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="durationUnit">the duration scale unit of the new timer</param>
        /// <param name="rateUnit">the rate scale unit of the new timer</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public Timer NewTimer(
            Type klass,
            string name,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
        {
            return NewTimer(klass, name, null, durationUnit, rateUnit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class, name, and scope,
        ///     measuring elapsed time in milliseconds and invocations per second.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public Timer NewTimer(
            Type klass,
            string name,
            string scope)
        {
            return NewTimer(klass, name, scope, TimeUnit.MILLISECONDS, TimeUnit.SECONDS);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class, name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="durationUnit">the duration scale unit of the new timer</param>
        /// <param name="rateUnit">the rate scale unit of the new timer</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public Timer NewTimer(
            Type klass,
            string name,
            string scope,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
        {
            return NewTimer(CreateName(klass, name, scope), durationUnit, rateUnit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="durationUnit">the duration scale unit of the new timer</param>
        /// <param name="rateUnit">the rate scale unit of the new timer</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public Timer NewTimer(
            MetricName metricName,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
        {
            Metric existingMetric = metrics.Get(metricName);
            if (existingMetric != null)
            {
                return (Timer) existingMetric;
            }

            return GetOrAdd(
                metricName,
                new Timer(NewMeterTickThreadPool(), durationUnit, rateUnit, clock));
        }

        /// <summary>
        ///     Returns an unmodifiable map of all metrics and their names.
        /// </summary>
        /// <value>an unmodifiable map of all metrics and their names</value>
        public IDictionary<MetricName, Metric> AllMetrics => new ReadOnlyDictionary<MetricName, Metric>(metrics);

        /// <summary>
        ///     Returns a grouped and sorted map of all registered metrics.
        /// </summary>
        /// <returns>all registered metrics, grouped by name and sorted</returns>
        public IDictionary<string, IDictionary<MetricName, Metric>> GroupedMetrics()
        {
            return GroupedMetrics(AnyMetricPredicate.INSTANCE);
        }

        /// <summary>
        ///     Returns a grouped and sorted map of all registered metrics which match then given {@link
        ///     MetricPredicate}.
        /// </summary>
        /// <param name="predicate">a predicate which metrics have to match to be in the results</param>
        /// <returns>all registered metrics which match {@code predicate}, sorted by name</returns>
        public IDictionary<string, IDictionary<MetricName, Metric>> GroupedMetrics(MetricPredicate predicate)
        {
            IDictionary<string, IDictionary<MetricName, Metric>> groups =
                new SortedDictionary<string, IDictionary<MetricName, Metric>>();
            foreach (KeyValuePair<MetricName, Metric> entry in metrics)
            {
                var qualifiedTypeName = entry.Key.Group + "." + entry.Key
                                            .Type;
                if (predicate.Matches(entry.Key, entry.Value))
                {
                    string scopedName;
                    if (entry.Key.HasScope)
                    {
                        scopedName = qualifiedTypeName + "." + entry.Key.Scope;
                    }
                    else
                    {
                        scopedName = qualifiedTypeName;
                    }

                    IDictionary<MetricName, Metric> group = groups.Get(scopedName);
                    if (group == null)
                    {
                        group = new SortedDictionary<MetricName, Metric>();
                        groups.Put(scopedName, group);
                    }

                    group.Put(entry.Key, entry.Value);
                }
            }

            return new ReadOnlyDictionary<string, IDictionary<MetricName, Metric>>(groups);
        }

        /// <summary>
        ///     Shut down this registry's thread pools.
        /// </summary>
        public void Shutdown()
        {
            threadPools.Shutdown();
        }

        /// <summary>
        ///     Creates a new scheduled thread pool of a given size with the given name, or returns an
        ///     existing thread pool if one was already created with the same name.
        /// </summary>
        /// <param name="poolSize">the number of threads to create</param>
        /// <param name="name">the name of the pool</param>
        /// <returns>a new <seealso cref="IScheduledExecutorService"/></returns>
        public IScheduledExecutorService NewScheduledThreadPool(
            int poolSize,
            string name)
        {
            return threadPools.NewScheduledThreadPool(poolSize, name);
        }

        /// <summary>
        ///     Removes the metric for the given class with the given name.
        /// </summary>
        /// <param name="klass">the klass the metric is associated with</param>
        /// <param name="name">the name of the metric</param>
        public void RemoveMetric(
            Type klass,
            string name)
        {
            RemoveMetric(klass, name, null);
        }

        /// <summary>
        ///     Removes the metric for the given class with the given name and scope.
        /// </summary>
        /// <param name="klass">the klass the metric is associated with</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        public void RemoveMetric(
            Type klass,
            string name,
            string scope)
        {
            RemoveMetric(CreateName(klass, name, scope));
        }

        /// <summary>
        ///     Removes the metric with the given name.
        /// </summary>
        /// <param name="name">the name of the metric</param>
        public void RemoveMetric(MetricName name)
        {
            if (metrics.TryRemove(name, out Metric metric))
            {
                if (metric is Stoppable)
                {
                    ((Stoppable) metric).Stop();
                }

                NotifyMetricRemoved(name);
            }
        }

        /// <summary>
        /// Called when a metric has been added to the <seealso cref="MetricsRegistry" />.
        /// </summary>
        public event EventHandler<MetricsRegistryEventArgs> MetricAdded;

        /// <summary>
        /// Called when a metric has been removed from the <seealso cref="MetricsRegistry" />.
        /// </summary>
        public event EventHandler<MetricsRegistryEventArgs> MetricRemoved;

        /// <summary>
        ///     Override to customize how <seealso cref="MetricName" />s are created.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the metric's scope</param>
        /// <returns>the metric's full name</returns>
        protected MetricName CreateName(
            Type klass,
            string name,
            string scope)
        {
            return new MetricName(klass, name, scope);
        }

        /// <summary>
        ///     Returns a new <seealso cref="ConcurrentDictionary{TK,TV}" /> implementation. Subclass this to do weird
        ///     things with
        ///     your own <seealso cref="MetricsRegistry" /> implementation.
        /// </summary>
        /// <returns>a new <seealso cref="ConcurrentDictionary{TK,TV}" /></returns>
        protected ConcurrentDictionary<MetricName, Metric> NewMetricsMap()
        {
            return new ConcurrentDictionary<MetricName, Metric>();
        }

        /// <summary>
        ///     Gets any existing metric with the given name or, if none exists, adds the given metric.
        /// </summary>
        /// <param name="name">the metric's name</param>
        /// <param name="metric">the new metric</param>
        /// <typeparam name="T">the type of the metric</typeparam>
        /// <returns>either the existing metric or {@code metric}</returns>
        protected T GetOrAdd<T>(
            MetricName name,
            T metric) where T : Metric
        {
            Metric existingMetric = metrics.Get(name);
            if (existingMetric == null)
            {
                Metric justAddedMetric = metrics.PutIfAbsent(name, metric);
                if (justAddedMetric == null)
                {
                    NotifyMetricAdded(name, metric);
                    return metric;
                }

                if (metric is Stoppable)
                {
                    ((Stoppable) metric).Stop();
                }

                return (T) justAddedMetric;
            }

            return (T) existingMetric;
        }

        private IScheduledExecutorService NewMeterTickThreadPool()
        {
            return threadPools.NewScheduledThreadPool(2, "meter-tick");
        }

        private void NotifyMetricRemoved(MetricName name)
        {
            MetricRemoved?.Invoke(this, new MetricsRegistryEventArgs(name));
        }

        private void NotifyMetricAdded(
            MetricName name,
            Metric metric)
        {
            MetricAdded?.Invoke(this, new MetricsRegistryEventArgs(name, metric));
        }
    }
} // end of namespace
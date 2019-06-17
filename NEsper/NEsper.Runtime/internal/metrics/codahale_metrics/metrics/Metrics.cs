///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.compat;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting;
using Timer = com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core.Timer;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics
{
    /// <summary>
    ///     A set of factory methods for creating centrally registered metric instances.
    /// </summary>
    public class Metrics
    {
        private static readonly MetricsRegistry DEFAULT_REGISTRY = new MetricsRegistry();

        private Metrics()
        { /* unused */
        }

        /// <summary>
        ///     Given a new <seealso cref="Gauge{T}" />, registers it under the given class and
        ///     name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="metric">the metric</param>
        /// <typeparam name="T">the type of the value returned by the metric</typeparam>
        /// <returns>{@code metric}</returns>
        public static Gauge<T> NewGauge<T>(
            Type klass,
            string name,
            Gauge<T> metric)
        {
            return DEFAULT_REGISTRY.NewGauge(klass, name, metric);
        }

        /// <summary>
        ///     Given a new <seealso cref="Gauge{T}" />, registers it under the given class and
        ///     name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="metric">the metric</param>
        /// <typeparam name="T">the type of the value returned by the metric</typeparam>
        /// <returns>{@code metric}</returns>
        public static Gauge<T> NewGauge<T>(
            Type klass,
            string name,
            string scope,
            Gauge<T> metric)
        {
            return DEFAULT_REGISTRY.NewGauge(klass, name, scope, metric);
        }

        /// <summary>
        ///     Given a new <seealso cref="Gauge{T}" />, registers it under the given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="metric">the metric</param>
        /// <typeparam name="T">the type of the value returned by the metric</typeparam>
        /// <returns>{@code metric}</returns>
        public static Gauge<T> NewGauge<T>(
            MetricName metricName,
            Gauge<T> metric)
        {
            return DEFAULT_REGISTRY.NewGauge(metricName, metric);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Counter" /> and registers it under the given class
        ///     and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <returns>a new <seealso cref="Counter"/></returns>
        public static Counter NewCounter(
            Type klass,
            string name)
        {
            return DEFAULT_REGISTRY.NewCounter(klass, name);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Counter" /> and registers it under the given class
        ///     and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <returns>a new <seealso cref="Counter"/></returns>
        public static Counter NewCounter(
            Type klass,
            string name,
            string scope)
        {
            return DEFAULT_REGISTRY.NewCounter(klass, name, scope);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Counter" /> and registers it under the given metric
        ///     name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <returns>a new <seealso cref="Counter"/></returns>
        public static Counter NewCounter(MetricName metricName)
        {
            return DEFAULT_REGISTRY.NewCounter(metricName);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Histogram" /> and registers it under the given
        ///     class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="biased">whether or not the histogram should be biased</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public static Histogram NewHistogram(
            Type klass,
            string name,
            bool biased)
        {
            return DEFAULT_REGISTRY.NewHistogram(klass, name, biased);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Histogram" /> and registers it under the given
        ///     class, name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="biased">whether or not the histogram should be biased</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public static Histogram NewHistogram(
            Type klass,
            string name,
            string scope,
            bool biased)
        {
            return DEFAULT_REGISTRY.NewHistogram(klass, name, scope, biased);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Histogram" /> and registers it under the given
        ///     metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="biased">whether or not the histogram should be biased</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public static Histogram NewHistogram(
            MetricName metricName,
            bool biased)
        {
            return DEFAULT_REGISTRY.NewHistogram(metricName, biased);
        }

        /// <summary>
        ///     Creates a new non-biased <seealso cref="Histogram" /> and registers it under the
        ///     given class and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public static Histogram NewHistogram(
            Type klass,
            string name)
        {
            return DEFAULT_REGISTRY.NewHistogram(klass, name);
        }

        /// <summary>
        ///     Creates a new non-biased <seealso cref="Histogram" /> and registers it under the
        ///     given class, name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public static Histogram NewHistogram(
            Type klass,
            string name,
            string scope)
        {
            return DEFAULT_REGISTRY.NewHistogram(klass, name, scope);
        }

        /// <summary>
        ///     Creates a new non-biased <seealso cref="Histogram" /> and registers it under the
        ///     given metric name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <returns>a new <seealso cref="Histogram"/></returns>
        public static Histogram NewHistogram(MetricName metricName)
        {
            return NewHistogram(metricName, false);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Meter" /> and registers it under the given class
        ///     and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="eventType">
        ///     the plural name of the type of events the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="unit">the rate unit of the new meter</param>
        /// <returns>a new <seealso cref="Meter"/></returns>
        public static Meter NewMeter(
            Type klass,
            string name,
            string eventType,
            TimeUnit unit)
        {
            return DEFAULT_REGISTRY.NewMeter(klass, name, eventType, unit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Meter" /> and registers it under the given class,
        ///     name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="eventType">
        ///     the plural name of the type of events the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="unit">the rate unit of the new meter</param>
        /// <returns>a new <seealso cref="Meter"/></returns>
        public static Meter NewMeter(
            Type klass,
            string name,
            string scope,
            string eventType,
            TimeUnit unit)
        {
            return DEFAULT_REGISTRY.NewMeter(klass, name, scope, eventType, unit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Meter" /> and registers it under the given metric
        ///     name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="eventType">
        ///     the plural name of the type of events the meter is measuring (e.g., {@code"requests"})
        /// </param>
        /// <param name="unit">the rate unit of the new meter</param>
        /// <returns>a new <seealso cref="Meter"/></returns>
        public static Meter NewMeter(
            MetricName metricName,
            string eventType,
            TimeUnit unit)
        {
            return DEFAULT_REGISTRY.NewMeter(metricName, eventType, unit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class
        ///     and name.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="durationUnit">the duration scale unit of the new timer</param>
        /// <param name="rateUnit">the rate scale unit of the new timer</param>
        /// <returns>a new <seealso cref="core.Timer"/></returns>
        public static Timer NewTimer(
            Type klass,
            string name,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
        {
            return DEFAULT_REGISTRY.NewTimer(klass, name, durationUnit, rateUnit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class
        ///     and name, measuring elapsed time in milliseconds and invocations per second.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public static Timer NewTimer(
            Type klass,
            string name)
        {
            return DEFAULT_REGISTRY.NewTimer(klass, name);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class,
        ///     name, and scope.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <param name="durationUnit">the duration scale unit of the new timer</param>
        /// <param name="rateUnit">the rate scale unit of the new timer</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public static Timer NewTimer(
            Type klass,
            string name,
            string scope,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
        {
            return DEFAULT_REGISTRY.NewTimer(klass, name, scope, durationUnit, rateUnit);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given class,
        ///     name, and scope, measuring elapsed time in milliseconds and invocations per second.
        /// </summary>
        /// <param name="klass">the class which owns the metric</param>
        /// <param name="name">the name of the metric</param>
        /// <param name="scope">the scope of the metric</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public static Timer NewTimer(
            Type klass,
            string name,
            string scope)
        {
            return DEFAULT_REGISTRY.NewTimer(klass, name, scope);
        }

        /// <summary>
        ///     Creates a new <seealso cref="Timer" /> and registers it under the given metric
        ///     name.
        /// </summary>
        /// <param name="metricName">the name of the metric</param>
        /// <param name="durationUnit">the duration scale unit of the new timer</param>
        /// <param name="rateUnit">the rate scale unit of the new timer</param>
        /// <returns>a new <seealso cref="Timer"/></returns>
        public static Timer NewTimer(
            MetricName metricName,
            TimeUnit durationUnit,
            TimeUnit rateUnit)
        {
            return DEFAULT_REGISTRY.NewTimer(metricName, durationUnit, rateUnit);
        }

        /// <summary>
        ///     Returns the (static) default registry.
        /// </summary>
        /// <returns>the metrics registry</returns>
        public static MetricsRegistry DefaultRegistry()
        {
            return DEFAULT_REGISTRY;
        }

        /// <summary>
        ///     Shuts down all thread pools for the default registry.
        /// </summary>
        public static void Shutdown()
        {
            DEFAULT_REGISTRY.Shutdown();
        }
    }
} // end of namespace
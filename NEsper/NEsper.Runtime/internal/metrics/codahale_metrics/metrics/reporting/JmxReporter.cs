///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;

namespace com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting
{
    using HistogramMetric = Histogram;

    /// <summary>
    ///     A reporter which exposes application metric as JMX MBeans.
    /// </summary>
    public partial class JmxReporter : AbstractReporter,
        MetricsRegistryListener,
        MetricProcessor<JmxReporter.MetaContext>
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static JmxReporter Instance;

        private readonly IDictionary<MetricName, string> registeredBeans;

        /// <summary>
        ///     Creates a new <seealso cref="JmxReporter" /> for the given registry.
        /// </summary>
        /// <param name="registry">a <seealso cref="MetricsRegistry" /></param>
        public JmxReporter(MetricsRegistry registry)
            : base(registry)
        {
            registeredBeans = new ConcurrentDictionary<MetricName, string>();
        }

        public void ProcessMeter(
            MetricName name,
            Metered meter,
            MetaContext context)
        {
            RegisterBean(
                context.MetricName, new Meter(meter, context.ObjectName),
                context.ObjectName);
        }

        public void ProcessHistogram(
            MetricName name,
            HistogramMetric histogram,
            MetaContext context)
        {
            RegisterBean(
                context.MetricName, new Histogram(histogram, context.ObjectName),
                context.ObjectName);
        }

        public void ProcessTimer(
            MetricName name,
            Timer timer,
            MetaContext context)
        {
            RegisterBean(
                context.MetricName, new TimerImpl(timer, context.ObjectName),
                context.ObjectName);
        }

        public void OnMetricAdded(
            MetricName name,
            Metric metric)
        {
            if (metric != null) {
                try {
                    metric.ProcessWith(this, name, new MetaContext(name, name.MBeanName));
                }
                catch (Exception e) {
                    Log.Warn("Error processing '" + name + "': " + e.Message, e);
                }
            }
        }

        public void OnMetricRemoved(MetricName name)
        {
            var objectName = registeredBeans.Delete(name);
            if (objectName != null) {
                UnregisterBean(objectName);
            }
        }

        /// <summary>
        ///     Starts the default instance of <seealso cref="JmxReporter" />.
        /// </summary>
        /// <param name="registry">the <seealso cref="MetricsRegistry" /> to report from</param>
        public static void StartDefault(MetricsRegistry registry)
        {
            Instance = new JmxReporter(registry);
            Instance.Start();
        }

        /// <summary>
        ///     Returns the default instance of <seealso cref="JmxReporter" /> if it has been started.
        /// </summary>
        /// <returns>The default instance or null if the default is not used</returns>
        public static JmxReporter GetDefault()
        {
            return Instance;
        }

        /// <summary>
        ///     Stops the default instance of <seealso cref="JmxReporter" />.
        /// </summary>
        public static void ShutdownDefault()
        {
            Instance?.Shutdown();
        }

        public void ProcessCounter(
            MetricName name,
            Counter counter,
            MetaContext context)
        {
            RegisterBean(
                context.MetricName, new Counter(counter, context.ObjectName),
                context.ObjectName);
        }

        public void ProcessGauge(
            MetricName name,
            Gauge gauge,
            MetaContext context)
        {
            RegisterBean(
                context.MetricName, new Gauge(gauge, context.ObjectName),
                context.ObjectName);
        }

        public override void Shutdown()
        {
            MetricsRegistry.RemoveListener(this);
            foreach (var name in registeredBeans.Values) {
                UnregisterBean(name);
            }

            registeredBeans.Clear();
        }

        /// <summary>
        ///     Starts the reporter.
        /// </summary>
        public void Start()
        {
            MetricsRegistry.AddListener(this);
        }

        private void RegisterBean(
            MetricName name,
            MetricMBean bean,
            string objectName)
        {
            registeredBeans.Put(name, objectName);
        }

        private void UnregisterBean(string name)
        {
        }
    }
} // end of namespace
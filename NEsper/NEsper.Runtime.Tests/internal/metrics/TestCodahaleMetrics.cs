///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core;
using com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.reporting;
using NUnit.Framework;

using Timer = com.espertech.esper.runtime.@internal.metrics.codahale_metrics.metrics.core.Timer;

namespace com.espertech.esper.runtime.@internal.metrics
{
    [TestFixture]
    public class TestCodahaleMetrics : AbstractRuntimeTest
    {
        [Test]
        public void TestMetrics()
        {
            var engineURI = "default";
            IList<MetricName> metricNames = new List<MetricName>();

            // Exposes a single "value" attribute
            var count = new AtomicLong();
            var gaugeName = MetricNameFactory.Name(engineURI, "type-testgauge", this.GetType());
            Metrics.NewGauge(gaugeName, new ProxyGauge<long>(() => count.Get()));

            metricNames.Add(gaugeName);

            // Exposes a counter, which is more efficient then the gauge when the size() call is expensive
            var counterName = MetricNameFactory.Name(engineURI, "type-testcounter", this.GetType());
            var counter = Metrics.NewCounter(counterName);
            metricNames.Add(gaugeName);

            // exposes a 1-second, 10-second etc. exponential weighted average
            var meterName = MetricNameFactory.Name(engineURI, "type-testmeter", this.GetType());
            var meter = Metrics.NewMeter(meterName, "request", TimeUnit.SECONDS);
            metricNames.Add(meterName);

            // exposes a histogramm of avg, min, max, 50th%, 95%, 99%
            var histName = MetricNameFactory.Name(engineURI, "type-testhist", this.GetType());
            var hist = Metrics.NewHistogram(histName, true);
            metricNames.Add(histName);

            // exposes a timer with a rates avg, one minute, 5 minute, 15 minute
            var timerName = MetricNameFactory.Name(engineURI, "type-testtimer", this.GetType());
            var timer = Metrics.NewTimer(timerName, TimeUnit.MILLISECONDS, TimeUnit.MILLISECONDS);
            metricNames.Add(timerName);

            // assert names found
            foreach (var name in metricNames)
            {
                AssertFound(name.MBeanName);
            }

            // Increase here for a longer run
            long TESTINTERVAL = 300;

            var random = new Random();
            var start = PerformanceObserver.MilliTime;
            var histogrammChoices = new long[] { 100, 1000, 5000, 8000, 10000 };
            while (PerformanceObserver.MilliTime - start < TESTINTERVAL)
            {
                var timerContext = timer.Time();
                meter.Mark();
                count.IncrementAndGet();
                counter.Inc();
                hist.Update(histogrammChoices[(int) (random.NextDouble() * histogrammChoices.Length)]);
                Thread.Sleep(100);
                timerContext.Stop();
            }

            foreach (var name in metricNames)
            {
                Metrics.DefaultRegistry().RemoveMetric(name);
                AssertNotFound(name.MBeanName);
            }
        }

        private void AssertFound(string name)
        {
#if METRICS_BORKED
            var instance = ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
            Assert.IsNotNull(instance);
#endif
        }

        private void AssertNotFound(string name)
        {
#if METRICS_BORKED
            try
            {
                ManagementFactory.PlatformMBeanServer.GetObjectInstance(new ObjectName(name));
                Assert.Fail();
            }
            catch (javax.management.InstanceNotFoundException ex)
            {
                // expected
            }
#endif
        }
    }
} // end of namespace

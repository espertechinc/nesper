///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.concurrency;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.StockTicker.monitor;
using NEsper.Examples.Support;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.StockTicker
{
    [TestFixture]
    public class TestStockTickerMultithreaded : StockTickerRegressionConstants,
        IDisposable
    {
        [SetUp]
        public void SetUp()
        {
            _listener = new StockTickerResultListener();

            var container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.Common.AddEventType("PriceLimit", typeof(PriceLimit).FullName);
            configuration.Common.AddEventType("StockTick", typeof(StockTick).FullName);

            _runtime = EPRuntimeProvider.GetRuntime("TestStockTickerMultithreaded", configuration);
            _runtime.Initialize();
            new StockTickerMonitor(_runtime, _listener);
        }

        private StockTickerResultListener _listener;
        private EPRuntime _runtime;

        public void PerformTest(int numberOfThreads,
            int numberOfTicksToSend,
            int ratioPriceOutOfLimit,
            int numberOfSecondsWaitForCompletion)
        {
            var totalNumTicks = numberOfTicksToSend + 2 * TestStockTickerGenerator.NUM_STOCK_NAMES;

            Log.Info(
                ".performTest Generating data, numberOfTicksToSend=" +
                numberOfTicksToSend +
                "  ratioPriceOutOfLimit=" +
                ratioPriceOutOfLimit);

            var generator = new StockTickerEventGenerator();
            var stream = generator.MakeEventStream(
                numberOfTicksToSend,
                ratioPriceOutOfLimit,
                TestStockTickerGenerator.NUM_STOCK_NAMES,
                PRICE_LIMIT_PCT_LOWER_LIMIT,
                PRICE_LIMIT_PCT_UPPER_LIMIT,
                PRICE_LOWER_LIMIT,
                PRICE_UPPER_LIMIT,
                true);

            var eventService = _runtime.EventService;
            
            Log.Info(".performTest Send limit and initial tick events - singlethreaded");
            for (var i = 0; i < TestStockTickerGenerator.NUM_STOCK_NAMES * 2; i++) {
                var @event = stream.First();
                stream.RemoveAt(0);
                eventService.SendEventBean(@event, @event.GetType().Name);
            }

            Log.Info(".performTest Loading thread pool work queue, numberOfRunnables=" + stream.Count);

            var executorService = new DedicatedExecutorService(string.Empty, numberOfThreads, new LinkedBlockingQueue<Runnable>());
            foreach (var @event in stream) {
                var innerEvent = @event;
                executorService.Submit(() => eventService.SendEventBean(
                    innerEvent, innerEvent.GetType().Name));
            }

            Log.Info(".performTest Listening for completion");
            EPRuntimeUtil.AwaitCompletion(_runtime, totalNumTicks, numberOfSecondsWaitForCompletion, 1, 10);

            executorService.Shutdown();

            // Check results : make sure the given ratio of out-of-limit stock prices was reported
            var expectedNumEmitted = numberOfTicksToSend / ratioPriceOutOfLimit + 1;
            ClassicAssert.IsTrue(_listener.Count == expectedNumEmitted);

            Log.Info(".performTest Done test");
        }

        public void Dispose()
        {
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Test]
        public void TestMultithreaded()
        {
            //performTest(3, 1000000, 100000, 60);  // on fast systems
            PerformTest(3, 50000, 10000, 15); // for unit tests on slow machines
        }
    }
}
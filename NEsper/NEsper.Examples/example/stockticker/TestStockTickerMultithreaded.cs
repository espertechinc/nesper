///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Linq;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.core.support;
using com.espertech.esper.events;
using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.StockTicker.monitor;

using NUnit.Framework;

namespace NEsper.Examples.StockTicker
{
    [TestFixture]
    public class TestStockTickerMultithreaded : StockTickerRegressionConstants, IDisposable
    {
        #region Setup/Teardown

        [SetUp]
        public void SetUp()
        {
            _listener = new StockTickerResultListener();

            var container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.AddEventType("PriceLimit", typeof (PriceLimit).FullName);
            configuration.AddEventType("StockTick", typeof (StockTick).FullName);

            _epService = EPServiceProviderManager.GetProvider(
                container, "TestStockTickerMultithreaded", configuration);
            _epService.Initialize();
            new StockTickerMonitor(_epService, _listener);
        }

        #endregion

        private StockTickerResultListener _listener;
        private EPServiceProvider _epService;

        public void PerformTest(int numberOfThreads,
                                int numberOfTicksToSend,
                                int ratioPriceOutOfLimit,
                                int numberOfSecondsWaitForCompletion)
        {
            int totalNumTicks = numberOfTicksToSend + 2*TestStockTickerGenerator.NUM_STOCK_NAMES;

            Log.Info(".performTest Generating data, numberOfTicksToSend=" + numberOfTicksToSend +
                     "  ratioPriceOutOfLimit=" + ratioPriceOutOfLimit);

            var generator = new StockTickerEventGenerator();
            var stream = generator.MakeEventStream(numberOfTicksToSend, ratioPriceOutOfLimit,
                                                                  TestStockTickerGenerator.NUM_STOCK_NAMES,
                                                                  PRICE_LIMIT_PCT_LOWER_LIMIT,
                                                                  PRICE_LIMIT_PCT_UPPER_LIMIT,
                                                                  PRICE_LOWER_LIMIT, PRICE_UPPER_LIMIT, true);

            Log.Info(".performTest Send limit and initial tick events - singlethreaded");
            for (int i = 0; i < TestStockTickerGenerator.NUM_STOCK_NAMES*2; i++) {
                Object @event = stream.First();
                stream.RemoveAt(0);
                _epService.EPRuntime.SendEvent(@event);
            }

            Log.Info(".performTest Loading thread pool work queue, numberOfRunnables=" + stream.Count);

            var executorService = new DedicatedExecutorService(string.Empty, numberOfThreads, new LinkedBlockingQueue<Runnable>());
            foreach (Object @event in stream) {
                object innerEvent = @event;
                executorService.Submit(() => _epService.EPRuntime.SendEvent(innerEvent));
            }

            Log.Info(".performTest Listening for completion");
            EPRuntimeUtil.AwaitCompletion(_epService.EPRuntime, totalNumTicks, numberOfSecondsWaitForCompletion, 1, 10);

            executorService.Shutdown();

            // Check results : make sure the given ratio of out-of-limit stock prices was reported
            int expectedNumEmitted = (numberOfTicksToSend/ratioPriceOutOfLimit) + 1;
            Assert.IsTrue(_listener.Count == expectedNumEmitted);

            Log.Info(".performTest Done test");
        }

        [Test]
        public void TestMultithreaded()
        {
            //performTest(3, 1000000, 100000, 60);  // on fast systems
            PerformTest(3, 50000, 10000, 15); // for unit tests on slow machines
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}

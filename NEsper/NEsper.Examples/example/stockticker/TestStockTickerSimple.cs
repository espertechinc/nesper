///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;

using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.StockTicker.monitor;

using NUnit.Framework;

namespace NEsper.Examples.StockTicker
{
    [TestFixture]
    public class TestStockTickerSimple : IDisposable
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
                container, "TestStockTickerSimple", configuration);

            // To reduce logging noise and get max performance
            _epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
        }

        #endregion

        private StockTickerResultListener _listener;
        private EPServiceProvider _epService;

        public void PerformEventFlowTest()
        {
            const string STOCK_NAME = "IBM.N";
            const double STOCK_PRICE = 50;
            const double LIMIT_PERCENT = 10;
            const double LIMIT_PERCENT_LARGE = 20;
            const string USER_ID_ONE = "junit";
            const string USER_ID_TWO = "jack";
            const string USER_ID_THREE = "anna";

            const double STOCK_PRICE_WITHIN_LIMIT_LOW = 46.0;
            const double STOCK_PRICE_OUTSIDE_LIMIT_LOW = 44.9;
            const double STOCK_PRICE_WITHIN_LIMIT_HIGH = 51.0;
            const double STOCK_PRICE_OUTSIDE_LIMIT_HIGH = 55.01;

            Log.Debug(".testEvents");
            _listener.ClearMatched();

            // Set a limit
            SendEvent(new PriceLimit(USER_ID_ONE, STOCK_NAME, LIMIT_PERCENT));
            Assert.IsTrue(_listener.Count == 0);

            // First stock ticker sets the initial price
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE));

            // Go within the limit, expect no response
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_LOW));
            Assert.IsTrue(_listener.Count == 0);

            // Go outside the limit, expect an event
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_LOW));
            Sleep(500);
            Assert.IsTrue(_listener.Count == 1);
            _listener.ClearMatched();

            // Go within the limit, expect no response
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_HIGH));
            Assert.IsTrue(_listener.Count == 0);

            // Go outside the limit, expect an event
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_HIGH));
            Sleep(500);
            Assert.IsTrue(_listener.Count == 1);
            var alert = (LimitAlert) _listener.MatchEvents[0];
            _listener.ClearMatched();
            Assert.IsTrue(alert.InitialPrice == STOCK_PRICE);
            Assert.IsTrue(alert.PriceLimit.UserId.Equals(USER_ID_ONE));
            Assert.IsTrue(alert.PriceLimit.StockSymbol.Equals(STOCK_NAME));
            Assert.IsTrue(alert.PriceLimit.LimitPct == LIMIT_PERCENT);
            Assert.IsTrue(alert.Tick.StockSymbol.Equals(STOCK_NAME));
            Assert.IsTrue(alert.Tick.Price == STOCK_PRICE_OUTSIDE_LIMIT_HIGH);

            // Set a new limit for the same stock
            // With the new limit none of these should fire
            SendEvent(new PriceLimit(USER_ID_ONE, STOCK_NAME, LIMIT_PERCENT_LARGE));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_LOW));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_LOW));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_HIGH));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_HIGH));
            Sleep(500);
            Assert.IsTrue(_listener.Count == 0);

            // Set a smaller limit for another couple of users
            SendEvent(new PriceLimit(USER_ID_TWO, STOCK_NAME, LIMIT_PERCENT));
            SendEvent(new PriceLimit(USER_ID_THREE, STOCK_NAME, LIMIT_PERCENT_LARGE));

            // Set limit back to original limit, send same prices, expect exactly 2 event
            SendEvent(new PriceLimit(USER_ID_ONE, STOCK_NAME, LIMIT_PERCENT));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_LOW));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_LOW));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_HIGH));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_HIGH));
            Sleep(500);

            Log.Info(".performEventFlowTest listSize=" + _listener.Count);
            Assert.IsTrue(_listener.Count == 4);
        }

        public void PerformBoundaryTest()
        {
            const string STOCK_NAME = "BOUNDARY_TEST";

            _listener.ClearMatched();
            SendEvent(new PriceLimit("junit", STOCK_NAME, 25.0));
            SendEvent(new StockTick(STOCK_NAME, 46.0));
            SendEvent(new StockTick(STOCK_NAME, 46.0 - 11.5));
            SendEvent(new StockTick(STOCK_NAME, 46.0 + 11.5));
            Sleep(500);
            Assert.IsTrue(_listener.Count == 0);

            SendEvent(new StockTick(STOCK_NAME, 46.0 - 11.5001));
            SendEvent(new StockTick(STOCK_NAME, 46.0 + 11.5001));
            Sleep(500);
            Assert.IsTrue(_listener.Count == 2);
        }

        private void Sleep(int msec)
        {
            try {
                Thread.Sleep(msec);
            }
            catch (ThreadInterruptedException e) {
                Log.Fatal(string.Empty, e);
            }
        }

        private void SendEvent(Object @event)
        {
            _epService.EPRuntime.SendEvent(@event);
        }

        [Test]
        public void TestStockTicker()
        {
            Log.Info(".testStockTicker");

            new StockTickerMonitor(_epService, _listener);

            PerformEventFlowTest();
            PerformBoundaryTest();
        }

        #region Implementation of IDisposable

        public void Dispose()
        {
        }

        #endregion

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}

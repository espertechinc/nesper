///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.StockTicker.monitor;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.StockTicker
{
    [TestFixture]
    public class TestStockTickerSimple : IDisposable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private StockTickerResultListener _listener;
        private EPRuntime _runtime;
        private EventSender _priceLimitSender;
        private EventSender _stockTickSender;

        [SetUp]
        public void SetUp()
        {
            _listener = new StockTickerResultListener();

            var container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.Common.AddEventType("PriceLimit", typeof(PriceLimit));
            configuration.Common.AddEventType("StockTick", typeof(StockTick));

            _runtime = EPRuntimeProvider.GetRuntime("TestStockTickerSimple", configuration);

            // To reduce logging noise and get max performance
            _runtime.EventService.ClockExternal();

            _priceLimitSender = _runtime.EventService.GetEventSender("PriceLimit");
            _stockTickSender = _runtime.EventService.GetEventSender("StockTick");
        }

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
            ClassicAssert.IsTrue(_listener.Count == 0);

            // First stock ticker sets the initial price
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE));

            // Go within the limit, expect no response
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_LOW));
            ClassicAssert.IsTrue(_listener.Count == 0);

            // Go outside the limit, expect an event
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_LOW));
            Sleep(500);
            ClassicAssert.IsTrue(_listener.Count == 1);
            _listener.ClearMatched();

            // Go within the limit, expect no response
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_HIGH));
            ClassicAssert.IsTrue(_listener.Count == 0);

            // Go outside the limit, expect an event
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_HIGH));
            Sleep(500);
            ClassicAssert.IsTrue(_listener.Count == 1);
            var alert = (LimitAlert) _listener.MatchEvents[0];
            _listener.ClearMatched();
            ClassicAssert.IsTrue(alert.InitialPrice == STOCK_PRICE);
            ClassicAssert.IsTrue(alert.PriceLimit.UserId.Equals(USER_ID_ONE));
            ClassicAssert.IsTrue(alert.PriceLimit.StockSymbol.Equals(STOCK_NAME));
            ClassicAssert.IsTrue(alert.PriceLimit.LimitPct == LIMIT_PERCENT);
            ClassicAssert.IsTrue(alert.Tick.StockSymbol.Equals(STOCK_NAME));
            ClassicAssert.IsTrue(alert.Tick.Price == STOCK_PRICE_OUTSIDE_LIMIT_HIGH);

            // Set a new limit for the same stock
            // With the new limit none of these should fire
            SendEvent(new PriceLimit(USER_ID_ONE, STOCK_NAME, LIMIT_PERCENT_LARGE));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_LOW));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_LOW));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_WITHIN_LIMIT_HIGH));
            SendEvent(new StockTick(STOCK_NAME, STOCK_PRICE_OUTSIDE_LIMIT_HIGH));
            Sleep(500);
            ClassicAssert.IsTrue(_listener.Count == 0);

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
            ClassicAssert.IsTrue(_listener.Count == 4);
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
            ClassicAssert.IsTrue(_listener.Count == 0);

            SendEvent(new StockTick(STOCK_NAME, 46.0 - 11.5001));
            SendEvent(new StockTick(STOCK_NAME, 46.0 + 11.5001));
            Sleep(500);
            ClassicAssert.IsTrue(_listener.Count == 2);
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

        private void SendEvent(StockTick @event)
        {
            _stockTickSender.SendEvent(@event);
        }

        private void SendEvent(PriceLimit @event)
        {
            _priceLimitSender.SendEvent(@event);
        }

        public void Dispose()
        {
        }

        [Test]
        public void TestStockTicker()
        {
            Log.Info(".testStockTicker");

            new StockTickerMonitor(_runtime, _listener);

            PerformEventFlowTest();
            PerformBoundaryTest();
        }
    }
}
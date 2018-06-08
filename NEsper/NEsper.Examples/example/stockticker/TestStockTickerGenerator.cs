///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using NEsper.Examples.StockTicker.eventbean;

using NUnit.Framework;

namespace NEsper.Examples.StockTicker
{
    [TestFixture]
    public class TestStockTickerGenerator : StockTickerRegressionConstants, IDisposable
    {
        public const int NUM_STOCK_NAMES = 1000;

        public void Dispose()
        {
        }

        [Test]
        public void TestFlow()
        {
            var generator = new StockTickerEventGenerator();

            PriceLimit[] limitBeans = generator.MakeLimits("nunit",
                                                           NUM_STOCK_NAMES, PRICE_LIMIT_PCT_LOWER_LIMIT,
                                                           PRICE_LIMIT_PCT_UPPER_LIMIT);

            Assert.IsTrue(limitBeans.Length == NUM_STOCK_NAMES);
            Assert.IsTrue(limitBeans[0].UserId.Equals("nunit"));
            for (int i = 0; i < limitBeans.Length; i++) {
                Assert.IsTrue(limitBeans[i].LimitPct >= PRICE_LIMIT_PCT_LOWER_LIMIT);
                Assert.IsTrue(limitBeans[i].LimitPct <= PRICE_LIMIT_PCT_UPPER_LIMIT);
            }

            StockTick[] initialPrices = generator.MakeInitialPriceStockTicks(limitBeans,
                                                                             PRICE_LOWER_LIMIT, PRICE_LOWER_LIMIT);

            Assert.IsTrue(initialPrices.Length == NUM_STOCK_NAMES);
            for (int i = 0; i < initialPrices.Length; i++) {
                Assert.IsTrue(initialPrices[i].Price >= PRICE_LOWER_LIMIT);
                Assert.IsTrue(initialPrices[i].Price <= PRICE_UPPER_LIMIT);
            }

            for (int i = 0; i < 100000; i++) {
                StockTick tick = generator.MakeStockTick(limitBeans[0], initialPrices[0]);

                double initialPrice = initialPrices[0].Price;
                double range = initialPrice*limitBeans[0].LimitPct/100;

                Assert.IsTrue(tick.Price > (initialPrice - range) - 1);
                Assert.IsTrue(tick.Price < (initialPrice + range) + 1);
            }
        }

        [Test]
        public void TestMakeStream()
        {
            var generator = new StockTickerEventGenerator();

            const int NUM_EVENTS = 1000;

            IList<object> stream = generator.MakeEventStream(NUM_EVENTS, 1000, NUM_STOCK_NAMES, 25, 30, 46, 54, true);

            Assert.IsTrue(stream.Count == (NUM_STOCK_NAMES*2 + NUM_EVENTS));
        }
    }
}

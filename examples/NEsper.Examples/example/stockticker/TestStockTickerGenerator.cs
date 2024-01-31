///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace NEsper.Examples.StockTicker
{
    [TestFixture]
    public class TestStockTickerGenerator : StockTickerRegressionConstants,
        IDisposable
    {
        public const int NUM_STOCK_NAMES = 1000;

        public void Dispose()
        {
        }

        [Test]
        public void TestFlow()
        {
            var generator = new StockTickerEventGenerator();

            var limitBeans = generator.MakeLimits(
                "nunit",
                NUM_STOCK_NAMES,
                PRICE_LIMIT_PCT_LOWER_LIMIT,
                PRICE_LIMIT_PCT_UPPER_LIMIT);

            ClassicAssert.IsTrue(limitBeans.Length == NUM_STOCK_NAMES);
            ClassicAssert.IsTrue(limitBeans[0].UserId.Equals("nunit"));
            for (var i = 0; i < limitBeans.Length; i++) {
                ClassicAssert.IsTrue(limitBeans[i].LimitPct >= PRICE_LIMIT_PCT_LOWER_LIMIT);
                ClassicAssert.IsTrue(limitBeans[i].LimitPct <= PRICE_LIMIT_PCT_UPPER_LIMIT);
            }

            var initialPrices = generator.MakeInitialPriceStockTicks(
                limitBeans,
                PRICE_LOWER_LIMIT,
                PRICE_LOWER_LIMIT);

            ClassicAssert.IsTrue(initialPrices.Length == NUM_STOCK_NAMES);
            for (var i = 0; i < initialPrices.Length; i++) {
                ClassicAssert.IsTrue(initialPrices[i].Price >= PRICE_LOWER_LIMIT);
                ClassicAssert.IsTrue(initialPrices[i].Price <= PRICE_UPPER_LIMIT);
            }

            for (var i = 0; i < 100000; i++) {
                var tick = generator.MakeStockTick(limitBeans[0], initialPrices[0]);

                var initialPrice = initialPrices[0].Price;
                var range = initialPrice * limitBeans[0].LimitPct / 100;

                ClassicAssert.IsTrue(tick.Price > initialPrice - range - 1);
                ClassicAssert.IsTrue(tick.Price < initialPrice + range + 1);
            }
        }

        [Test]
        public void TestMakeStream()
        {
            var generator = new StockTickerEventGenerator();

            const int NUM_EVENTS = 1000;

            var stream = generator.MakeEventStream(NUM_EVENTS, 1000, NUM_STOCK_NAMES, 25, 30, 46, 54, true);

            ClassicAssert.IsTrue(stream.Count == NUM_STOCK_NAMES * 2 + NUM_EVENTS);
        }
    }
}
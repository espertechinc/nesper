///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.StockTicker
{
    public class StockTickerEventGenerator
    {
        private readonly Random _random = new Random(Environment.TickCount);

        public IList<object> MakeEventStream(int numberOfTicks,
            int ratioOutOfLimit,
            int numberOfStocks,
            double priceLimitPctLowerLimit,
            double priceLimitPctUpperLimit,
            double priceLowerLimit,
            double priceUpperLimit,
            bool isLastTickOutOfLimit)
        {
            var stream = new List<object>();

            var limitBeans = MakeLimits(
                "example_user",
                numberOfStocks,
                priceLimitPctLowerLimit,
                priceLimitPctUpperLimit);

            for (var i = 0; i < limitBeans.Length; i++) {
                stream.Add(limitBeans[i]);
            }

            // The first stock ticker sets up an initial price
            var initialPrices = MakeInitialPriceStockTicks(limitBeans, priceLowerLimit, priceUpperLimit);

            for (var i = 0; i < initialPrices.Length; i++) {
                stream.Add(initialPrices[i]);
            }

            for (var i = 0; i < numberOfTicks; i++) {
                var index = i % limitBeans.Length;
                var tick = MakeStockTick(limitBeans[index], initialPrices[index]);

                // Generate an out-of-limit price
                if (i % ratioOutOfLimit == 0) {
                    tick = new StockTick(tick.StockSymbol, -1);
                }

                // Last tick is out-of-limit as well
                if (i == numberOfTicks - 1 && isLastTickOutOfLimit) {
                    tick = new StockTick(tick.StockSymbol, 9999);
                }

                stream.Add(tick);
            }

            return stream;
        }

        public StockTick MakeStockTick(PriceLimit limitBean,
            StockTick initialPrice)
        {
            var stockSymbol = limitBean.StockSymbol;
            var range = initialPrice.Price * limitBean.LimitPct / 100;
            var price = initialPrice.Price - range + range * 2 * _random.NextDouble();

            var priceReducedPrecision = To1tenthPrecision(price);

            if (priceReducedPrecision < initialPrice.Price - range) {
                priceReducedPrecision = initialPrice.Price;
            }

            if (priceReducedPrecision > initialPrice.Price + range) {
                priceReducedPrecision = initialPrice.Price;
            }

            return new StockTick(stockSymbol, priceReducedPrecision);
        }

        public PriceLimit[] MakeLimits(string userName,
            int numBeans,
            double limit_pct_lower_boundary,
            double limit_pct_upper_boundary)
        {
            var limitBeans = new PriceLimit[numBeans];

            for (var i = 0; i < numBeans; i++) {
                var stockSymbol = "SYM_" + i;

                var diff = limit_pct_upper_boundary - limit_pct_lower_boundary;
                var limitPct = limit_pct_lower_boundary + _random.NextDouble() * diff;

                limitBeans[i] = new PriceLimit(userName, stockSymbol, To1tenthPrecision(limitPct));
            }

            return limitBeans;
        }

        public StockTick[] MakeInitialPriceStockTicks(PriceLimit[] limitBeans,
            double price_lower_boundary,
            double price_upper_boundary)
        {
            var stockTickBeans = new StockTick[limitBeans.Length];

            for (var i = 0; i < stockTickBeans.Length; i++) {
                var stockSymbol = limitBeans[i].StockSymbol;

                // Determine a random price
                var diff = price_upper_boundary - price_lower_boundary;
                var price = price_lower_boundary + _random.NextDouble() * diff;

                stockTickBeans[i] = new StockTick(stockSymbol, To1tenthPrecision(price));
            }

            return stockTickBeans;
        }

        private static double To1tenthPrecision(double aDouble)
        {
            var intValue = (int) (aDouble * 10);
            return intValue / 10.0;
        }
    }
}
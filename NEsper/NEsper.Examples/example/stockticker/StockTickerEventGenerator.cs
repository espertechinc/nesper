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

namespace NEsper.Examples.StockTicker
{
    public class StockTickerEventGenerator
    {
        private readonly Random _random = new Random(Environment.TickCount);

        public IList<Object> MakeEventStream(int numberOfTicks,
                                             int ratioOutOfLimit,
                                             int numberOfStocks,
                                             double priceLimitPctLowerLimit,
                                             double priceLimitPctUpperLimit,
                                             double priceLowerLimit,
                                             double priceUpperLimit,
                                             bool isLastTickOutOfLimit)
        {
            var stream = new List<Object>();

            PriceLimit[] limitBeans = MakeLimits("example_user", numberOfStocks, priceLimitPctLowerLimit,
                                                 priceLimitPctUpperLimit);

            for (int i = 0; i < limitBeans.Length; i++) {
                stream.Add(limitBeans[i]);
            }

            // The first stock ticker sets up an initial price
            StockTick[] initialPrices = MakeInitialPriceStockTicks(limitBeans, priceLowerLimit, priceUpperLimit);

            for (int i = 0; i < initialPrices.Length; i++) {
                stream.Add(initialPrices[i]);
            }

            for (int i = 0; i < numberOfTicks; i++) {
                int index = i%limitBeans.Length;
                StockTick tick = MakeStockTick(limitBeans[index], initialPrices[index]);

                // Generate an out-of-limit price
                if ((i%ratioOutOfLimit) == 0) {
                    tick = new StockTick(tick.StockSymbol, -1);
                }

                // Last tick is out-of-limit as well
                if ((i == (numberOfTicks - 1)) && (isLastTickOutOfLimit)) {
                    tick = new StockTick(tick.StockSymbol, 9999);
                }

                stream.Add(tick);
            }

            return stream;
        }

        public StockTick MakeStockTick(PriceLimit limitBean, StockTick initialPrice)
        {
            String stockSymbol = limitBean.StockSymbol;
            double range = initialPrice.Price*limitBean.LimitPct/100;
            double price = (initialPrice.Price - range + (range*2*_random.NextDouble()));

            double priceReducedPrecision = To1tenthPrecision(price);

            if (priceReducedPrecision < (initialPrice.Price - range)) {
                priceReducedPrecision = initialPrice.Price;
            }

            if (priceReducedPrecision > (initialPrice.Price + range)) {
                priceReducedPrecision = initialPrice.Price;
            }

            return new StockTick(stockSymbol, priceReducedPrecision);
        }

        public PriceLimit[] MakeLimits(String userName,
                                       int numBeans,
                                       double limit_pct_lower_boundary,
                                       double limit_pct_upper_boundary)
        {
            var limitBeans = new PriceLimit[numBeans];

            for (int i = 0; i < numBeans; i++) {
                String stockSymbol = "SYM_" + i;

                double diff = limit_pct_upper_boundary - limit_pct_lower_boundary;
                double limitPct = limit_pct_lower_boundary + (_random.NextDouble()*diff);

                limitBeans[i] = new PriceLimit(userName, stockSymbol, To1tenthPrecision(limitPct));
            }

            return limitBeans;
        }

        public StockTick[] MakeInitialPriceStockTicks(PriceLimit[] limitBeans,
                                                      double price_lower_boundary,
                                                      double price_upper_boundary)
        {
            var stockTickBeans = new StockTick[limitBeans.Length];

            for (int i = 0; i < stockTickBeans.Length; i++) {
                String stockSymbol = limitBeans[i].StockSymbol;

                // Determine a random price
                double diff = price_upper_boundary - price_lower_boundary;
                double price = price_lower_boundary + _random.NextDouble()*diff;

                stockTickBeans[i] = new StockTick(stockSymbol, To1tenthPrecision(price));
            }

            return stockTickBeans;
        }

        private static double To1tenthPrecision(double aDouble)
        {
            var intValue = (int) (aDouble*10);
            return intValue/10.0;
        }
    }
}

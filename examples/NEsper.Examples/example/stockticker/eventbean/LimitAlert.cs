///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace NEsper.Examples.StockTicker.eventbean
{
    public class LimitAlert
    {
        public LimitAlert(StockTick tick,
            PriceLimit limit,
            double initialPrice)
        {
            Tick = tick;
            PriceLimit = limit;
            InitialPrice = initialPrice;
        }

        /// <summary>
        ///     Gets or sets the tick.
        /// </summary>
        /// <value>The tick.</value>
        public StockTick Tick { get; }

        /// <summary>
        ///     Gets or sets the price limit.
        /// </summary>
        /// <value>The price limit.</value>
        public PriceLimit PriceLimit { get; }

        /// <summary>
        ///     Gets or sets the initial price.
        /// </summary>
        /// <value>The initial price.</value>
        public double InitialPrice { get; }

        public override string ToString()
        {
            return string.Format("Tick: {0}, PriceLimit: {1}, InitialPrice: {2}", Tick, PriceLimit, InitialPrice);
        }
    }
}
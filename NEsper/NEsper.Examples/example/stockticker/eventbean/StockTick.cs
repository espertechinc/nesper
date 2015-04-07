///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace NEsper.Examples.StockTicker.eventbean
{
    public class StockTick
    {
        public StockTick(String stockSymbol, double price)
        {
            StockSymbol = stockSymbol;
            Price = price;
        }

        /// <summary>
        /// Gets or sets the stock symbol.
        /// </summary>
        /// <value>The stock symbol.</value>
        public string StockSymbol { get; private set; }

        /// <summary>
        /// Gets or sets the price.
        /// </summary>
        /// <value>The price.</value>
        public double Price { get; private set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("StockSymbol: {0}, Price: {1}", StockSymbol, Price);
        }
    }
}

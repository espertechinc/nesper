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
    public class PriceLimit
    {
        public PriceLimit(String userId, String stockSymbol, double limitPct)
        {
            UserId = userId;
            StockSymbol = stockSymbol;
            LimitPct = limitPct;
        }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        /// <value>The user id.</value>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets or sets the stock symbol.
        /// </summary>
        /// <value>The stock symbol.</value>
        public string StockSymbol { get; private set; }

        /// <summary>
        /// Gets or sets the limit PCT.
        /// </summary>
        /// <value>The limit PCT.</value>
        public double LimitPct { get; private set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return string.Format("UserId: {0}, StockSymbol: {1}, LimitPct: {2}", UserId, StockSymbol, LimitPct);
        }
    }
}

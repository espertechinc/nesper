///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;

using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.StockTicker.monitor
{
    public class StockTickerAlertListener
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly StockTick _initialPriceTick;
        private readonly PriceLimit _limit;
        private readonly StockTickerResultListener _stockTickerResultListener;

        public StockTickerAlertListener(
            EPRuntime runtime,
            PriceLimit limit,
            StockTick initialPriceTick,
            StockTickerResultListener stockTickerResultListener)
        {
            _limit = limit;
            _initialPriceTick = initialPriceTick;
            _stockTickerResultListener = stockTickerResultListener;
        }

        public void Update(
            object sender,
            UpdateEventArgs e)
        {
            var newEvents = e.NewEvents;
            var @event = newEvents[0].Get("tick");
            var tick = (StockTick) @event;

            Log.Debug(
                ".update Alert for stock=" +
                tick.StockSymbol +
                "  Price=" +
                tick.Price +
                "  initialPriceTick=" +
                _initialPriceTick.Price +
                "  limt=" +
                _limit.LimitPct);

            var alert = new LimitAlert(tick, _limit, _initialPriceTick.Price);
            _stockTickerResultListener.Emitted(alert);
        }
    }
}
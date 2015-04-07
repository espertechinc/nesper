///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.StockTicker.monitor
{
    public class StockTickerMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPServiceProvider _epService;
        private readonly StockTickerResultListener _stockTickerResultListener;
        private EPStatement _highPriceListener = null;

        private EPStatement _initialPriceListener = null;
        private readonly PriceLimit _limit = null;
        private EPStatement _lowPriceListener = null;
        private readonly EPStatement _newLimitListener = null;

        public StockTickerMonitor(EPServiceProvider epService, StockTickerResultListener stockTickerResultListener)
        {
            _epService = epService;
            _stockTickerResultListener = stockTickerResultListener;

            // Listen to all limits to be set
            String expressionText = "every pricelimit=PriceLimit()";
            EPStatement factory = epService.EPAdministrator.CreatePattern(expressionText);

            factory.Events += HandleEvents;
        }

        public StockTickerMonitor(EPServiceProvider epService, PriceLimit limit,
                                  StockTickerResultListener stockTickerResultListener)
        {
            _epService = epService;
            _limit = limit;
            _stockTickerResultListener = stockTickerResultListener;

            String expressionText = "every pricelimit=PriceLimit" +
                                    "(UserId='" + limit.UserId + "'," +
                                    "StockSymbol='" + limit.StockSymbol + "')";
            _newLimitListener = epService.EPAdministrator.CreatePattern(expressionText);
            _newLimitListener.Events += HandleNewLimitEvent;

            expressionText = "tick=StockTick(StockSymbol='" + limit.StockSymbol + "')";
            _initialPriceListener = epService.EPAdministrator.CreatePattern(expressionText);
            _initialPriceListener.Events += HandleInitialPriceEvent;
        }

        private void HandleEvents(Object sender, UpdateEventArgs e)
        {
            var limitBean = (PriceLimit) e.NewEvents[0].Get("pricelimit");

            if (Log.IsDebugEnabled) {
                Log.Debug(".update Received new limit, user=" + limitBean.UserId +
                          "  stock=" + limitBean.StockSymbol +
                          "  pct=" + limitBean.LimitPct);
            }

            new StockTickerMonitor(_epService, limitBean, _stockTickerResultListener);
        }

        private void HandleNewLimitEvent(Object sender, UpdateEventArgs e)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".update Received an override limit, stopping listeners");
            }

            Die();
        }

        private void HandleInitialPriceEvent(Object sender, UpdateEventArgs e)
        {
            var tick = (StockTick) e.NewEvents[0].Get("tick");
            PriceLimit limit = _limit;

            _initialPriceListener = null;

            double limitPct = limit.LimitPct;
            double upperLimit = tick.Price*(1.0 + (limitPct/100.0));
            double lowerLimit = tick.Price*(1.0 - (limitPct/100.0));

            if (Log.IsDebugEnabled) {
                Log.Debug(".update Received initial tick, stock=" + tick.StockSymbol +
                          "  price=" + tick.Price +
                          "  limit.LimitPct=" + limitPct +
                          "  lowerLimit=" + lowerLimit +
                          "  upperLimit=" + upperLimit);
            }

            var listener =
                new StockTickerAlertListener(_epService,
                                             limit,
                                             tick,
                                             _stockTickerResultListener);

            String expressionText = "every tick=StockTick" +
                                    "(StockSymbol='" + limit.StockSymbol +
                                    "', Price < " + lowerLimit + ")";
            _lowPriceListener = _epService.EPAdministrator.CreatePattern(expressionText);
            _lowPriceListener.Events += listener.Update;

            expressionText = "every tick=StockTick" +
                             "(StockSymbol='" + limit.StockSymbol + "', Price > " +
                             upperLimit + ")";
            _highPriceListener = _epService.EPAdministrator.CreatePattern(expressionText);
            _highPriceListener.Events += listener.Update;
        }

        private void Die()
        {
            if (_newLimitListener != null) _newLimitListener.RemoveAllEventHandlers();
            if (_initialPriceListener != null) _initialPriceListener.RemoveAllEventHandlers();
            if (_lowPriceListener != null) _lowPriceListener.RemoveAllEventHandlers();
            if (_highPriceListener != null) _highPriceListener.RemoveAllEventHandlers();
        }
    }
}

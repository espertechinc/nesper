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
using NEsper.Examples.Support;

namespace NEsper.Examples.StockTicker.monitor
{
    public class StockTickerMonitor
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly EPRuntime _runtime;
        private readonly PriceLimit _limit;
        private readonly EPStatement _newLimitListener;
        private readonly StockTickerResultListener _stockTickerResultListener;
        private EPStatement _highPriceListener;

        private EPStatement _initialPriceListener;
        private EPStatement _lowPriceListener;

        public StockTickerMonitor(
            EPRuntime runtime,
            StockTickerResultListener stockTickerResultListener)
        {
            _runtime = runtime;
            _stockTickerResultListener = stockTickerResultListener;

            // Listen to all limits to be set
            var expressionText = "every Pricelimit=PriceLimit()";
            EPStatement factory = runtime.DeployStatement(expressionText);

            factory.Events += HandleEvents;
        }

        public StockTickerMonitor(
            EPRuntime runtime,
            PriceLimit limit,
            StockTickerResultListener stockTickerResultListener)
        {
            _runtime = runtime;
            _limit = limit;
            _stockTickerResultListener = stockTickerResultListener;

            var expressionText = "every Pricelimit=PriceLimit" +
                                 "(UserId='" +
                                 limit.UserId +
                                 "'," +
                                 "StockSymbol='" +
                                 limit.StockSymbol +
                                 "')";
            _newLimitListener = runtime.DeployStatement(expressionText);
            _newLimitListener.Events += HandleNewLimitEvent;

            expressionText = "tick=StockTick(StockSymbol='" + limit.StockSymbol + "')";
            _initialPriceListener = runtime.DeployStatement(expressionText);
            _initialPriceListener.Events += HandleInitialPriceEvent;
        }

        private void HandleEvents(object sender,
            UpdateEventArgs e)
        {
            var limitBean = (PriceLimit) e.NewEvents[0].Get("Pricelimit");

            if (Log.IsDebugEnabled) {
                Log.Debug(
                    ".update Received new limit, user=" +
                    limitBean.UserId +
                    "  stock=" +
                    limitBean.StockSymbol +
                    "  pct=" +
                    limitBean.LimitPct);
            }

            new StockTickerMonitor(_runtime, limitBean, _stockTickerResultListener);
        }

        private void HandleNewLimitEvent(object sender,
            UpdateEventArgs e)
        {
            if (Log.IsDebugEnabled) {
                Log.Debug(".update Received an override limit, stopping listeners");
            }

            Die();
        }

        private void HandleInitialPriceEvent(object sender,
            UpdateEventArgs e)
        {
            var tick = (StockTick) e.NewEvents[0].Get("tick");
            var limit = _limit;

            _initialPriceListener = null;

            var limitPct = limit.LimitPct;
            var upperLimit = tick.Price * (1.0 + limitPct / 100.0);
            var lowerLimit = tick.Price * (1.0 - limitPct / 100.0);

            if (Log.IsDebugEnabled) {
                Log.Debug(
                    ".update Received initial tick, stock=" +
                    tick.StockSymbol +
                    "  Price=" +
                    tick.Price +
                    "  limit.LimitPct=" +
                    limitPct +
                    "  lowerLimit=" +
                    lowerLimit +
                    "  upperLimit=" +
                    upperLimit);
            }

            var listener =
                new StockTickerAlertListener(
                    _runtime,
                    limit,
                    tick,
                    _stockTickerResultListener);

            var expressionText = "every tick=StockTick" +
                                 "(StockSymbol='" +
                                 limit.StockSymbol +
                                 "', Price < " +
                                 lowerLimit +
                                 ")";
            _lowPriceListener = _runtime.DeployStatement(expressionText);
            _lowPriceListener.Events += listener.Update;

            expressionText = "every tick=StockTick" +
                             "(StockSymbol='" +
                             limit.StockSymbol +
                             "', Price > " +
                             upperLimit +
                             ")";
            _highPriceListener = _runtime.DeployStatement(expressionText);
            _highPriceListener.Events += listener.Update;
        }

        private void Die()
        {
            _newLimitListener?.RemoveAllEventHandlers();
            _initialPriceListener?.RemoveAllEventHandlers();
            _lowPriceListener?.RemoveAllEventHandlers();
            _highPriceListener?.RemoveAllEventHandlers();
        }
    }
}
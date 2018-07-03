///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.RSI
{
    /// <summary>
    /// RSI gives you the trend for a stock and for more complete explanation, you can
    /// visit the link:<see cref="http://www.stockcharts.com/education/IndicatorAnalysis/indic_RSI.html">RSI</see>
    /// <para>
    /// After a definite number of stock events, or accumulation period, the first RSI is
    /// computed.  Then for each subsequent stock event, the RSI calculations use the previous
    /// period’s Average Gain andLoss to determine the “smoothed RSI”.
    /// </para>
    /// </summary>

    public class RSIStockTickerListener
    {
        private readonly EPServiceProvider _epService;
        private EventBean[] _oldEvents = null;
        private int _period;
        private int _count = 0;
        private readonly List<Double> _adv, _decl;
        private double _avgGain = Double.MinValue, _avgLoss = Double.MinValue;
        private double _rs = Double.MinValue, _rsi = Double.MinValue;

        public RSIStockTickerListener(EPServiceProvider epService, int periods)
        {
            _epService = epService;
            _period = periods;
            _oldEvents = null;
            _adv = new List<Double>();
            _decl = new List<Double>();
        }

        public void Reset(int period)
        {
            _period = period;
            _oldEvents = null;
            _adv.Clear();
            _decl.Clear();
            _avgGain = Double.MinValue;
            _avgLoss = Double.MinValue;
            _rs = Double.MinValue;
            _rsi = Double.MinValue;
        }

        public int Count
        {
            get { return _count; }
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            var newEvents = e.NewEvents;
            var eventBean = newEvents[0]["tick"];
            var newTick = (StockTick)eventBean;
            Log.Info(".update for stock=" + newTick.StockSymbol + "  price=" + newTick.Price);

            if (_oldEvents != null)
            {
                eventBean = _oldEvents[0]["tick"];
                var oldTick = (StockTick)eventBean;
                Compute(newTick, oldTick);
                _epService.EPRuntime.SendEvent(new RSIEvent(newTick, _avgLoss, _avgGain, _rs, _rsi));
            }
            _oldEvents = newEvents;
        }

        private void Compute(StockTick newTick, StockTick oldTick)
        {
            _count++;
            var change = newTick.Price - oldTick.Price;
            if (_count <= _period)
            {
                if (change > 0)
                {
                    Log.Info(".Count " + _count + " Advance " + change);
                    _adv.Add(change);
                }
                else
                {
                    Log.Info(".Count " + _count + " Decline " + change);
                    _decl.Add(change);
                }
            }

            if (_count >= _period)
            {
                if (_count == _period)
                {
                    _avgLoss = AvgValueList(_decl);
                    _avgGain = AvgValueList(_adv);
                }
                else
                {
                    _adv.Clear();
                    _decl.Clear();
                    var adv = 0.0;
                    var decl = 0.0;
                    if (change > 0)
                    {
                        Log.Info(".Count " + _count + " Advance " + change);
                        adv = change;
                    }
                    else
                    {
                        Log.Info(".Count " + _count + " Decline " + change);
                        decl = change;
                    }
                    _avgGain = ((_avgGain * (_period - 1)) + adv) / _period;
                    _avgLoss = ((_avgLoss * (_period - 1)) + decl) / _period;
                }
                if (_avgLoss == 0)
                {
                    _rs = 100.0;
                    _rsi = 100.0;
                }
                else
                {
                    _rs = Math.Abs(_avgGain / _avgLoss);
                    var to1 = 1.0 + _rs;
                    var to100 = 100.0 / to1;
                    _rsi = 100.0 - to100;
                }
            }
        }

        private double AvgValueList(IEnumerable<double> lValues)
        {
            var sum = lValues.Aggregate(0.0, (current, val) => current + val);
            return (sum / _count);
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}


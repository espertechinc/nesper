///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.RSI
{
    public class RSIListener 
    {
        private double _avgLoss, _avgGain, _rs, _rsi;
        private int _rsiCount;

        public void Reset()
        {
            _avgLoss = 0;
            _avgGain = 0;
            _rs = 0;
            _rsi = 0;
            _rsiCount = 0;
        }

        public double AvgLoss
        {
            get { return _avgLoss; }
        }

        public double AvgGain
        {
            get { return _avgGain; }
        }

        public double RS
        {
            get { return _rs; }
        }

        public double RSI
        {
            get { return _rsi; }
        }

        public int RSICount
        {
            get { return _rsiCount; }
        }

        public void Update(Object sender, UpdateEventArgs e)
        {
            var newEvents = e.NewEvents;
            var eventBean = newEvents[0]["Tick"];
            var tick = (StockTick)eventBean;
            Log.Info(" Stock " + tick.StockSymbol + " Price " + tick.Price);
            eventBean = newEvents[0]["AvgLoss"];
            _avgLoss = (Double)eventBean;
            if (_avgLoss == Double.MinValue)
            {
                Log.Info(" Not Meaningful ");
            }
            else
            {
                _avgLoss = To1tenthPrecision((Double)eventBean);
                Log.Info(" AvgLoss " + _avgLoss);
            }
            eventBean = newEvents[0]["AvgGain"];
            _avgGain = (Double)eventBean;
            if (_avgGain == Double.MinValue)
            {
                Log.Info(" Not Meaningful ");
            }
            else
            {
                _avgGain = To1tenthPrecision((Double)eventBean);
                Log.Info(" AvgGain " + _avgGain);
            }

            eventBean = newEvents[0]["RS"];
            _rs = (Double)eventBean;
            if (_rs == Double.MinValue)
            {
                Log.Info(" Not Meaningful ");
            }
            else
            {
                _rs = To1tenthPrecision((Double)eventBean);
                Log.Info(" RS " + _rs);
            }
            eventBean = newEvents[0]["RSI"] ;
            _rsi = (Double)eventBean;
            if (_rsi == Double.MinValue)
            {
                Log.Info(" Not Meaningful ");
            }
            else
            {
                _rsiCount++;
                _rsi = To1tenthPrecision((Double)eventBean);
                Log.Info(" RSI " + _rsi);
            }
        }

        private double To1tenthPrecision(double aDouble)
        {
            var intValue = (int)(aDouble * 10);
            return intValue / 10.0;
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}

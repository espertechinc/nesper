///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NEsper.Examples.StockTicker.eventbean;

namespace NEsper.Examples.RSI
{
    public class RSIEvent
    {
        private StockTick _tick;
        private double _avgLoss, _avgGain;
        private double _rs, _rsi;

        public RSIEvent(StockTick tick_, double avgLoss_, double avgGain_, double rs_, double rsi_)
        {
            _tick = tick_;
            _avgLoss = avgLoss_;
            _avgGain = avgGain_;
            _rs = rs_;
            _rsi = rsi_;
        }

        public StockTick Tick
        {
            get { return _tick; }
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

        public override String ToString()
        {
            return _tick.ToString() +
                    "  avgLoss=" + _avgLoss +
                    "  avgGain=" + _avgGain +
                    " RS=" + _rs + " RSI=" + +_rsi;
        }

    }
}

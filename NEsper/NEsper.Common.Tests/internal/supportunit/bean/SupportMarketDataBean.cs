///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.supportunit.bean
{
    [Serializable]
    public class SupportMarketDataBean
    {
        public SupportMarketDataBean(
            string symbol,
            double price,
            long? volume,
            string feed)
        {
            Symbol = symbol;
            Price = price;
            Volume = volume;
            Feed = feed;
        }

        public SupportMarketDataBean(
            string symbol,
            string id,
            double price)
        {
            Symbol = symbol;
            Id = id;
            Price = price;
        }

        public string Symbol { get; }

        public double Price { get; }

        public long? Volume { get; }

        public string Feed { get; }

        public string Id { get; }

        public double GetPriceTimesVolume(double factor)
        {
            return Price * (Volume ?? 0) * factor;
        }

        public override string ToString()
        {
            return "SupportMarketDataBean " +
                   "symbol=" + Symbol +
                   " Price=" + Price +
                   " Volume=" + Volume +
                   " feed=" + Feed;
        }
    }
} // end of namespace

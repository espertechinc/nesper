///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportMarketDataBean
    {
        private string _id;

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
            _id = id;
            Price = price;
        }

        public string Symbol { get; }

        public double Price { get; }

        public long? Volume { get; }

        public string Feed { get; }

        public string Id {
            get => _id;
            set => _id = value;
        }

        public string GetSymbol()
        {
            return Symbol;
        }

        public double GetPrice()
        {
            return Price;
        }

        public long? GetVolume()
        {
            return Volume;
        }
        
        public double GetPriceTimesVolume(double factor)
        {
            return Price * Volume.GetValueOrDefault(0L) * factor;
        }

        public override string ToString()
        {
            return "SupportMarketDataBean" +
                   " Symbol=" + Symbol +
                   " Price=" +  Price +
                   " Volume=" + Volume +
                   " Feed=" + Feed;
        }
    }
} // end of namespace
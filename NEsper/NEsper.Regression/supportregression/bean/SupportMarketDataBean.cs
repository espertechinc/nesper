///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.supportregression.bean
{
    [Serializable]
	public class SupportMarketDataBean
	{
        public SupportMarketDataBean(String symbol, double price, long? volume, String feed)
	    {
	        Symbol = symbol;
	        Price = price;
	        Volume = volume;
	        Feed = feed;
	    }

	    public SupportMarketDataBean(String symbol, String id, double price)
	    {
	        Symbol = symbol;
	        Id = id;
	        Price = price;
	    }

        public string Symbol { get; private set; }

        public double Price { get; private set; }

        public long? Volume { get; private set; }

        public string Feed { get; private set; }

        public string Id { get; private set; }

        public double GetPriceTimesVolume(double factor)
        {
            return Price * (Volume ?? 0) * factor;
        }

        // NOTE: the following methods do not register as properties to esper
        //       this keeps them out of esper's space, but allows them to be
        //       called as instance methods.

        public String GetSymbol()
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

        public String GetFeed()
        {
            return Feed;
        }

        public String GetId()
        {
            return Id;
        }

	    public override String ToString()
	    {
	        return "SupportMarketDataBean " +
	               "symbol=" + Symbol +
	               " Price=" + Price +
	               " Volume=" + Volume +
	               " feed=" + Feed;
	    }
	}
} // End of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.example.feedexample
{
	public class FeedEvent
	{
	    private readonly FeedEnum feed;
        private readonly String symbol;
        private readonly double price;

	    public FeedEvent(FeedEnum feed, String symbol, double price)
	    {
	        this.feed = feed;
	        this.symbol = symbol;
	        this.price = price;
	    }

	    public FeedEnum Feed
	    {
            get { return feed; }
	    }

	    public String Symbol
	    {
	        get {return symbol;}
	    }

	    public double Price
	    {
	        get {return price;}
	    }
	}
} // End of namespace

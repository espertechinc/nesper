///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

namespace NEsper.Examples.MarketDataFeed
{
    public class MarketDataEvent
    {
        public MarketDataEvent(String symbol, FeedEnum feed)
        {
            Symbol = symbol;
            Feed = feed;
        }

        public string Symbol { get; private set; }

        public FeedEnum Feed { get; private set; }
    }
}

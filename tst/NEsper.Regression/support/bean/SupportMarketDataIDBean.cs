///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.regressionlib.support.bean
{
    public class SupportMarketDataIDBean
    {
        public SupportMarketDataIDBean(
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

        public string Id { get; }
    }
} // end of namespace
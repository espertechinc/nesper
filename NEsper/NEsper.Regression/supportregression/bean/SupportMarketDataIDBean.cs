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
	public class SupportMarketDataIDBean
	{
	    public SupportMarketDataIDBean(String symbol, String id, double price)
	    {
	        Symbol = symbol;
	        Id = id;
	        Price = price;
	    }

	    public string Symbol { get; private set; }

	    public double Price { get; private set; }

	    public string Id { get; private set; }
	}
} // End of namespace

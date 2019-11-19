///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using NUnit.Framework;

namespace NEsper.Examples.MarketDataFeed
{
	[TestFixture]
	public class TestFeedSimMain : IDisposable
	{
		[Test]
		public void TestRun()
	    {
            var main = new FeedSimMain(100, 50, 5, false, "FeedSimMain");
	        main.Run();
	    }

	    public void Dispose()
	    {
	    }
	}
}

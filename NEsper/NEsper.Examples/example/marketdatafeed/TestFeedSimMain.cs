///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;

using NUnit.Framework;

using com.espertech.esper.client;
using com.espertech.esper.client.time;
using com.espertech.esper.support;
using com.espertech.esper.support.util;


namespace com.espertech.esper.example.marketdatafeed
{
	[TestFixture]
	public class TestFeedSimMain : IDisposable
	{
		[Test]
		public void TestRun()
	    {
            FeedSimMain main = new FeedSimMain(100, 50, 5, false, "FeedSimMain");
	        main.Run();
	    }

	    public void Dispose()
	    {
	    }
	}
}

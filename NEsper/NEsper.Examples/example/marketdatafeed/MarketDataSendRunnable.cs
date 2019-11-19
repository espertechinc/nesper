///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.MarketDataFeed
{
	public class MarketDataSendRunnable
	{
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	    private readonly EPServiceProvider _engine;

	    private FeedEnum? _rateDropOffFeed;

	    private volatile bool _isShutdown;
	    private readonly Random _random = new Random();

	    public MarketDataSendRunnable(EPServiceProvider engine)
	    {
	        _engine = engine;
	    }

	    public void Run()
	    {
	        Log.Info(".call Thread " + Thread.CurrentThread + " starting");

	        try
	        {
                var enumValues = Enum.GetValues(typeof(FeedEnum));

                while (!_isShutdown)
	            {

	                var nextFeed = Math.Abs(_random.Next() % 2);
	                var feed = (FeedEnum) enumValues.GetValue(nextFeed);
	                if (_rateDropOffFeed != feed)
	                {
	                    _engine.EPRuntime.SendEvent(new MarketDataEvent("SYM", feed));
	                }
	            }
	        }
	        catch (Exception ex)
	        {
	            Log.Error("Error in send loop", ex);
	        }

	        Log.Info(".call Thread " + Thread.CurrentThread + " done");
	    }

	    public void SetRateDropOffFeed(FeedEnum? feedToDrop)
	    {
	        _rateDropOffFeed = feedToDrop;
	    }

	    public void SetShutdown()
	    {
	        _isShutdown = true;
	    }
	}
} // End of namespace

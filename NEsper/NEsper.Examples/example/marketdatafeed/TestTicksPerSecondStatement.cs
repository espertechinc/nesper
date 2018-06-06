///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;

using NUnit.Framework;

namespace NEsper.Examples.MarketDataFeed
{
	[TestFixture]
	public class TestTicksPerSecondStatement : IDisposable
	{
		private EPServiceProvider epService;
	    private SupportUpdateListener listener;
	
	    [SetUp]
	    public void SetUp() {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.AddEventType("MarketDataEvent", typeof(MarketDataEvent).FullName);
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

	        epService = EPServiceProviderManager.GetProvider(container, "TestTicksPerSecondStatement", configuration);
	        epService.Initialize();
	
	        listener = new SupportUpdateListener();
	        var stmt = TicksPerSecondStatement.Create(epService.EPAdministrator);
	        stmt.Events += listener.Update;
	
	        // Use external clocking for the test
	        epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
	    }
	
	    [Test]
	    public void TestFlow() {
	        sendEvent(new CurrentTimeEvent(1000)); // Set the start time to 1 second
	
	        sendEvent(new MarketDataEvent("CSC", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("IBM", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("GE", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("MS", FeedEnum.FEED_B));
	        Assert.IsFalse(listener.IsInvoked);
	
	        sendEvent(new CurrentTimeEvent(1500)); // Now events arriving around 1.5 sec
	        sendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("CSC", FeedEnum.FEED_B));
	        Assert.IsFalse(listener.IsInvoked);
	
	        sendEvent(new CurrentTimeEvent(2000)); // Now events arriving around 2 sec
	        sendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("IBM", FeedEnum.FEED_B));
	        sendEvent(new MarketDataEvent("GE", FeedEnum.FEED_B));
	        sendEvent(new MarketDataEvent("IOU", FeedEnum.FEED_B));
	        assertCounts(4, 2);
	
	        sendEvent(new CurrentTimeEvent(2500)); // Now events arriving around 2.5 sec
	        sendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("GE", FeedEnum.FEED_B));
	        sendEvent(new MarketDataEvent("MS", FeedEnum.FEED_B));
            Assert.IsFalse(listener.IsInvoked);
	
	        sendEvent(new CurrentTimeEvent(3000));
	        assertCounts(2, 5);
	
	        sendEvent(new CurrentTimeEvent(3500));
	        sendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("IBM", FeedEnum.FEED_A));
	        sendEvent(new MarketDataEvent("UUS", FeedEnum.FEED_A));
            Assert.IsFalse(listener.IsInvoked);
	
	        sendEvent(new CurrentTimeEvent(4000));
	        sendEvent(new MarketDataEvent("NBOT", FeedEnum.FEED_B));
	        sendEvent(new MarketDataEvent("YAH", FeedEnum.FEED_B));
	        assertCounts(3, 0);
	
	        sendEvent(new CurrentTimeEvent(4500));
            Assert.IsFalse(listener.IsInvoked);
	
	        sendEvent(new CurrentTimeEvent(5000));
	        assertCounts(0, 2);
	    }
	
	    private void assertCounts(long countFeedA, long countFeedB)
	    {
	    	var countPerFeed = new Dictionary<FeedEnum, long>();
            countPerFeed.Put((FeedEnum)listener.LastNewData[0]["feed"], (long)listener.LastNewData[0]["cnt"]);
	        countPerFeed.Put((FeedEnum)listener.LastNewData[1]["feed"], (long)listener.LastNewData[1]["cnt"]);
            Assert.AreEqual(2, listener.LastNewData.Length);
	        listener.Reset();
	
	        Assert.AreEqual(countFeedA, (long) countPerFeed.Get(FeedEnum.FEED_A)); // casting to long to avoid JUnit ambiguous assert
	        Assert.AreEqual(countFeedB, (long) countPerFeed.Get(FeedEnum.FEED_B));
	    }
	
	    private void sendEvent(Object eventBean) {
	        epService.EPRuntime.SendEvent(eventBean);
	    }

	    public void Dispose()
	    {
	    }
	}
}

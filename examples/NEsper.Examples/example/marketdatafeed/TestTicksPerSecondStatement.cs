///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.MarketDataFeed
{
	[TestFixture]
	public class TestTicksPerSecondStatement : IDisposable
	{
		private EPRuntime _runtime;
		private EventSender _marketDataEventSender;
	    private SupportUpdateListener _listener;
	
	    [SetUp]
	    public void SetUp() {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.Common.AddEventType("MarketDataEvent", typeof(MarketDataEvent).FullName);
            configuration.Common.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

	        _runtime = EPRuntimeProvider.GetRuntime("TestTicksPerSecondStatement", configuration);
	        _runtime.Initialize();

	        _marketDataEventSender = _runtime.EventService.GetEventSender(typeof(MarketDataEvent).Name);
	
	        _listener = new SupportUpdateListener();
	        var stmt = TicksPerSecondStatement.Create(_runtime);
	        stmt.Events += _listener.Update;
	
	        // Use external clocking for the test
	        _runtime.EventService.ClockExternal();
	    }
	
	    [Test]
	    public void TestFlow() {
	        _runtime.EventService.AdvanceTime(1000); // Set the start time to 1 second
	
	        SendEvent(new MarketDataEvent("CSC", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("IBM", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("GE", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("MS", FeedEnum.FEED_B));
	        Assert.IsFalse(_listener.IsInvoked);
	
	        _runtime.EventService.AdvanceTime(1500); // Now events arriving around 1.5 sec
	        SendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("CSC", FeedEnum.FEED_B));
	        Assert.IsFalse(_listener.IsInvoked);
	
	        _runtime.EventService.AdvanceTime(2000); // Now events arriving around 2 sec
	        SendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("IBM", FeedEnum.FEED_B));
	        SendEvent(new MarketDataEvent("GE", FeedEnum.FEED_B));
	        SendEvent(new MarketDataEvent("IOU", FeedEnum.FEED_B));
	        AssertCounts(4, 2);
	
	        _runtime.EventService.AdvanceTime(2500); // Now events arriving around 2.5 sec
	        SendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("GE", FeedEnum.FEED_B));
	        SendEvent(new MarketDataEvent("MS", FeedEnum.FEED_B));
            Assert.IsFalse(_listener.IsInvoked);
	
	        _runtime.EventService.AdvanceTime(3000);
	        AssertCounts(2, 5);
	
	        _runtime.EventService.AdvanceTime(3500);
	        SendEvent(new MarketDataEvent("TEL", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("IBM", FeedEnum.FEED_A));
	        SendEvent(new MarketDataEvent("UUS", FeedEnum.FEED_A));
            Assert.IsFalse(_listener.IsInvoked);
	
	        _runtime.EventService.AdvanceTime(4000);
	        SendEvent(new MarketDataEvent("NBOT", FeedEnum.FEED_B));
	        SendEvent(new MarketDataEvent("YAH", FeedEnum.FEED_B));
	        AssertCounts(3, 0);
	
	        _runtime.EventService.AdvanceTime(4500);
            Assert.IsFalse(_listener.IsInvoked);
	
	        _runtime.EventService.AdvanceTime(5000);
	        AssertCounts(0, 2);
	    }
	
	    private void AssertCounts(long countFeedA, long countFeedB)
	    {
	    	var countPerFeed = new Dictionary<FeedEnum, long>();
            countPerFeed.Put((FeedEnum)_listener.LastNewData[0]["feed"], (long)_listener.LastNewData[0]["cnt"]);
	        countPerFeed.Put((FeedEnum)_listener.LastNewData[1]["feed"], (long)_listener.LastNewData[1]["cnt"]);
            Assert.AreEqual(2, _listener.LastNewData.Length);
	        _listener.Reset();
	
	        Assert.AreEqual(countFeedA, (long) countPerFeed.Get(FeedEnum.FEED_A)); // casting to long to avoid JUnit ambiguous assert
	        Assert.AreEqual(countFeedB, (long) countPerFeed.Get(FeedEnum.FEED_B));
	    }
	
	    private void SendEvent(MarketDataEvent eventBean) {
		    _marketDataEventSender.SendEvent(eventBean);
	    }

	    public void Dispose()
	    {
	    }
	}
}

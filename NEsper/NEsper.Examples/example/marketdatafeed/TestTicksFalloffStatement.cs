///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.client.time;
using com.espertech.esper.compat.container;

using NUnit.Framework;

namespace NEsper.Examples.MarketDataFeed
{
	[TestFixture]
	public class TestTicksFalloffStatement : IDisposable
	{
	    private EPServiceProvider _epService;
	    private SupportUpdateListener _listener;
	
	    [SetUp]
	    public void SetUp()
	    {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

	        var configuration = new Configuration(container);
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = false;
	        configuration.AddEventType("MarketDataEvent", typeof(MarketDataEvent).FullName);
            configuration.EngineDefaults.EventMeta.ClassPropertyResolutionStyle = PropertyResolutionStyle.CASE_INSENSITIVE;

	        _epService = EPServiceProviderManager.GetProvider(container, "TestTicksPerSecondStatement", configuration);
	        _epService.Initialize();
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(0));
	
	        TicksPerSecondStatement.Create(_epService.EPAdministrator);
	        var stmt = TicksFalloffStatement.Create(_epService.EPAdministrator);
	        _listener = new SupportUpdateListener();
	        stmt.Events += _listener.Update;
	
	        // Use external clocking for the test
	        _epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
	    }
	
	    [Test]
	    public void TestFlow() {
	
	        sendEvents(1000, 50, 150); // Set time to 1 second, send 100 feed A and 150 feed B
	        sendEvents(1500, 50, 50);
	        sendEvents(2000, 60, 130);
	        sendEvents(2500, 40, 70);
	        sendEvents(3000, 50, 150);
	        sendEvents(3500, 50, 50);
	        sendEvents(4000, 50, 150);
	        sendEvents(4500, 50, 50);
	        sendEvents(5000, 50, 150);
	        sendEvents(5500, 50, 50);
	        sendEvents(6000, 50, 24);
            Assert.IsFalse(_listener.IsInvoked);
	        sendEvents(6500, 50, 50);
	        sendEvents(7000, 50, 150);
	        assertReceived(FeedEnum.FEED_B, (200 * 5 + 74) / 6, 74);
	        sendEvents(7500, 50, 50);
	        sendEvents(8000, 50, 150);
	        sendEvents(8500, 50, 50);
	        sendEvents(9000, 60, 150);
	        sendEvents(9500, 40, 50);
	        sendEvents(10000, 50, 150);
	        sendEvents(10500, 70, 50);
	        sendEvents(11000, 30, 150);
	        sendEvents(11500, 50, 50);
	        sendEvents(12000, 40, 150);
            Assert.IsFalse(_listener.IsInvoked);
	        sendEvents(12500, 30, 150);
	        sendEvents(13000, 50, 150);
	        assertReceived(FeedEnum.FEED_A, (100 * 9 + 70) / 10, 70);
	    }
	
	    private void assertReceived(FeedEnum feedEnum, double average, long count)
	    {
            Assert.IsTrue(_listener.IsInvoked);
	        Assert.AreEqual(1, _listener.LastNewData.Length);
	        var eventBean = _listener.LastNewData[0];
	        Assert.AreEqual(feedEnum, eventBean["feed"]);
	        Assert.AreEqual(average, eventBean["avgCnt"]);
	        Assert.AreEqual(count, eventBean["feedCnt"]);
	        _listener.Reset();
	    }
	
	    private void sendEvents(long timestamp, int numFeedA, int numFeedB) {
	        _epService.EPRuntime.SendEvent(new CurrentTimeEvent(timestamp));
	        send(FeedEnum.FEED_A, numFeedA);
	        send(FeedEnum.FEED_B, numFeedB);
	    }
	
	    private void send(FeedEnum feedEnum, int numEvents)
	    {
	        for (var i = 0; i < numEvents; i++)
	        {
	            _epService.EPRuntime.SendEvent(new MarketDataEvent("CSC", feedEnum));
	        }
	    }

	    public void Dispose()
	    {
	    }
	}
}

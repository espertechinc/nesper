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
using com.espertech.esper.client.time;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esperio;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;

using NUnit.Framework;


namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestAdapterCoordinator 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    	private SupportUpdateListener listener;
    	private String eventTypeName;
    	private EPServiceProvider epService;
    	private long currentTime;
    	private AdapterCoordinator coordinator;
    	private CSVInputAdapterSpec timestampsLooping;
    	private CSVInputAdapterSpec noTimestampsLooping;
    	private CSVInputAdapterSpec noTimestampsNotLooping;
    	private CSVInputAdapterSpec timestampsNotLooping;
    	private String[] propertyOrderNoTimestamp;

        [SetUp]
        public void SetUp()
    	{
    		IDictionary<String, Object> propertyTypes = new LinkedHashMap<String, Object>();
    		propertyTypes.Put("myInt", typeof(int));
    		propertyTypes.Put("myDouble", typeof(double?));
    		propertyTypes.Put("myString", typeof(String));
    
    		eventTypeName = "mapEvent";
    		Configuration configuration = new Configuration();
    		configuration.AddEventType(eventTypeName, propertyTypes);
    
    		epService = EPServiceProviderManager.GetProvider("Adapter", configuration);
    		epService.Initialize();
    		EPAdministrator administrator = epService.EPAdministrator;
    		String statementText = "select * from mapEvent.win:length(5)";
    		EPStatement statement = administrator.CreateEPL(statementText);
    		listener = new SupportUpdateListener();
    		statement.Events += listener.Update;
    
    		// Turn off external clocking
    		epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
    
    		// Set the clock to 0
    		currentTime = 0;
    		SendTimeEvent(0);
    
    		coordinator = new AdapterCoordinatorImpl(epService, true);
    
        	propertyOrderNoTimestamp = new String[] { "myInt", "myDouble", "myString" };
        	String[] propertyOrderTimestamp = new String[] { "timestamp", "myInt", "myDouble", "myString" };
    
    		// A CSVPlayer for a file with timestamps, not looping
    		timestampsNotLooping = new CSVInputAdapterSpec(new AdapterInputSource("/regression/timestampOne.csv"), eventTypeName);
            timestampsNotLooping.IsUsingEngineThread = true;
    		timestampsNotLooping.PropertyOrder = propertyOrderTimestamp;
    		timestampsNotLooping.TimestampColumn = "timestamp";
    
    		// A CSVAdapter for a file with timestamps, looping
    		timestampsLooping = new CSVInputAdapterSpec(new AdapterInputSource("/regression/timestampTwo.csv"), eventTypeName);
            timestampsLooping.IsLooping = true;
    		timestampsLooping.IsUsingEngineThread = true;
    		timestampsLooping.PropertyOrder = propertyOrderTimestamp;
    		timestampsLooping.TimestampColumn = "timestamp";
    
    		// A CSVAdapter that sends 10 events per sec, not looping
    		noTimestampsNotLooping = new CSVInputAdapterSpec(new AdapterInputSource("/regression/noTimestampOne.csv"), eventTypeName);
    		noTimestampsNotLooping.EventsPerSec = 10;
    		noTimestampsNotLooping.PropertyOrder = propertyOrderNoTimestamp;
            noTimestampsNotLooping.IsUsingEngineThread = true;
    
    		// A CSVAdapter that sends 5 events per sec, looping
    		noTimestampsLooping = new CSVInputAdapterSpec(new AdapterInputSource("/regression/noTimestampTwo.csv"), eventTypeName);
    		noTimestampsLooping.EventsPerSec = 5;
            noTimestampsLooping.IsLooping = true;
    		noTimestampsLooping.PropertyOrder = propertyOrderNoTimestamp;
            noTimestampsLooping.IsUsingEngineThread = true;
    	}
    
        [Test]
    	public void TestRun()
    	{
    		coordinator.Coordinate(new CSVInputAdapter(timestampsNotLooping));
    		coordinator.Coordinate(new CSVInputAdapter(timestampsLooping));
    		coordinator.Coordinate(new CSVInputAdapter(noTimestampsNotLooping));
    		coordinator.Coordinate(new CSVInputAdapter(noTimestampsLooping));
    
    		// TimeInMillis is 0
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    		coordinator.Start();
    
    		// TimeInMillis is 50
    		SendTimeEvent(50);
    
    		// TimeInMillis is 100
    		SendTimeEvent(50);
    		AssertEvent(0, 1, 1.1, "timestampOne.one");
    		AssertEvent(1, 1, 1.1, "noTimestampOne.one");
    		AssertSizeAndReset(2);
    
    		// TimeInMillis is 150
    		SendTimeEvent(50);
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 200
    		SendTimeEvent(50);
    		AssertEvent(0, 2, 2.2, "timestampTwo.two");
    		AssertEvent(1, 2, 2.2, "noTimestampOne.two");
    		AssertEvent(2, 2, 2.2, "noTimestampTwo.two");
    		AssertSizeAndReset(3);
    
    		// TimeInMillis is 250
    		SendTimeEvent(50);
    
    		// TimeInMillis is 300
    		SendTimeEvent(50);
    		AssertEvent(0, 3, 3.3, "timestampOne.three");
    		AssertEvent(1, 3, 3.3, "noTimestampOne.three");
    		AssertSizeAndReset(2);
    
    		// TimeInMillis is 350
    		SendTimeEvent(50);
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    
    		coordinator.Pause();
    
    		// TimeInMillis is 400
    		SendTimeEvent(50);
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 450
    		SendTimeEvent(50);
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    
    		coordinator.Resume();
    
    		AssertEvent(0, 4, 4.4, "timestampTwo.four");
    		AssertEvent(1, 4, 4.4, "noTimestampTwo.four");
    		AssertSizeAndReset(2);
    
    		// TimeInMillis is 500
    		SendTimeEvent(50);
    		AssertEvent(0, 5, 5.5, "timestampOne.five");
    		AssertSizeAndReset(1);
    
    		// TimeInMillis is 600
    		SendTimeEvent(100);
    		AssertEvent(0, 6, 6.6, "timestampTwo.six");
    		AssertEvent(1, 2, 2.2, "noTimestampTwo.two");
    		AssertSizeAndReset(2);
    
    		// TimeInMillis is 800
    		SendTimeEvent(200);
    		AssertEvent(0, 2, 2.2, "timestampTwo.two");
    		AssertEvent(1, 4, 4.4, "noTimestampTwo.four");
    		AssertSizeAndReset(2);
    
    		coordinator.Stop();
    		SendTimeEvent(1000);
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    	}
    
        [Test]
    	public void TestRunTillNull()
    	{
    		coordinator.Coordinate(new CSVInputAdapter(epService, timestampsNotLooping));
    		coordinator.Start();
    
    		// TimeInMillis is 100
    		SendTimeEvent(100);
            Log.Debug(".testRunTillNull time==100");
    		AssertEvent(0, 1, 1.1, "timestampOne.one");
    		AssertSizeAndReset(1);
    
    		// TimeInMillis is 300
    		SendTimeEvent(200);
            Log.Debug(".testRunTillNull time==300");
    		AssertEvent(0, 3, 3.3, "timestampOne.three");
    		AssertSizeAndReset(1);
    
    		// TimeInMillis is 500
    		SendTimeEvent(200);
            Log.Debug(".testRunTillNull time==500");
    		AssertEvent(0, 5, 5.5, "timestampOne.five");
    		AssertSizeAndReset(1);
    
    		// TimeInMillis is 600
    		SendTimeEvent(100);
            Log.Debug(".testRunTillNull time==600");
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 700
    		SendTimeEvent(100);
            Log.Debug(".testRunTillNull time==700");
    		Assert.IsFalse(listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 800
    		SendTimeEvent(100);
            Log.Debug(".testRunTillNull time==800");
    	}
    
        [Test]
    	public void TestNotUsingEngineThread()
    	{
    		coordinator = new AdapterCoordinatorImpl(epService, false);
    		coordinator.Coordinate(new CSVInputAdapter(epService, noTimestampsNotLooping));
    		coordinator.Coordinate(new CSVInputAdapter(epService, timestampsNotLooping));
    
    		long startTime = Environment.TickCount;
    		coordinator.Start();
    		long endTime = Environment.TickCount;
    
    		// The last event should be sent after 500 ms
    		Assert.IsTrue(endTime - startTime > 500);
    
    		Assert.AreEqual(6, listener.GetNewDataList().Count);
    		AssertEvent(0, 1, 1.1, "noTimestampOne.one");
    		AssertEvent(1, 1, 1.1, "timestampOne.one");
    		AssertEvent(2, 2, 2.2, "noTimestampOne.two");
    		AssertEvent(3, 3, 3.3, "noTimestampOne.three");
    		AssertEvent(4, 3, 3.3, "timestampOne.three");
    		AssertEvent(5, 5, 5.5, "timestampOne.five");
    	}
    
        [Test]
    	public void TestExternalTimer()
    	{
    		coordinator = new AdapterCoordinatorImpl(epService, false, true, false);
    		coordinator.Coordinate(new CSVInputAdapter(epService, noTimestampsNotLooping));
    		coordinator.Coordinate(new CSVInputAdapter(epService, timestampsNotLooping));
    
    		long startTime = Environment.TickCount;
    		coordinator.Start();
    		long endTime = Environment.TickCount;
    
    		// Check that we haven't been kept waiting
            Assert.That(endTime - startTime, Is.LessThan(100));
    
    		Assert.AreEqual(6, listener.GetNewDataList().Count);
    		AssertEvent(0, 1, 1.1, "noTimestampOne.one");
    		AssertEvent(1, 1, 1.1, "timestampOne.one");
    		AssertEvent(2, 2, 2.2, "noTimestampOne.two");
    		AssertEvent(3, 3, 3.3, "noTimestampOne.three");
    		AssertEvent(4, 3, 3.3, "timestampOne.three");
    		AssertEvent(5, 5, 5.5, "timestampOne.five");
    	}
    
    	private void AssertEvent(int howManyBack, int? myInt, double? myDouble, String myString)
    	{
    		Assert.IsTrue(listener.IsInvoked());
    		Assert.IsTrue(howManyBack < listener.GetNewDataList().Count);
    		EventBean[] data = listener.GetNewDataList()[howManyBack];
    		Assert.AreEqual(1, data.Length);
    		EventBean theEvent = data[0];
    		Assert.AreEqual(myInt, theEvent.Get("myInt"));
    		Assert.AreEqual(myDouble, theEvent.Get("myDouble"));
    		Assert.AreEqual(myString, theEvent.Get("myString"));
    	}
    
    
    	private void SendTimeEvent(int timeIncrement){
    		currentTime += timeIncrement;
    	    CurrentTimeEvent theEvent = new CurrentTimeEvent(currentTime);
    	    epService.EPRuntime.SendEvent(theEvent);
    	}
    
    	private void AssertSizeAndReset(int size)
    	{
    		IList<EventBean[]> list = listener.GetNewDataList();
    		Assert.AreEqual(size, list.Count);
    		list.Clear();
    		listener.GetAndClearIsInvoked();
    	}
    
    }
}

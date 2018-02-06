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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esperio.csv;
using com.espertech.esperio.support.util;

using NUnit.Framework;

namespace com.espertech.esperio.regression.adapter
{
    [TestFixture]
    public class TestAdapterCoordinator 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
    	private SupportUpdateListener _listener;
    	private String _eventTypeName;
        private IContainer _container;
    	private EPServiceProvider _epService;
    	private long _currentTime;
    	private AdapterCoordinator _coordinator;
    	private CSVInputAdapterSpec _timestampsLooping;
    	private CSVInputAdapterSpec _noTimestampsLooping;
    	private CSVInputAdapterSpec _noTimestampsNotLooping;
    	private CSVInputAdapterSpec _timestampsNotLooping;
    	private String[] _propertyOrderNoTimestamp;

        [SetUp]
        public void SetUp()
        {
            _container = SupportContainer.Reset();

    		IDictionary<String, Object> propertyTypes = new LinkedHashMap<String, Object>();
    		propertyTypes.Put("myInt", typeof(int));
    		propertyTypes.Put("myDouble", typeof(double?));
    		propertyTypes.Put("myString", typeof(String));
    
    		_eventTypeName = "mapEvent";
    		Configuration configuration = new Configuration(_container);
    		configuration.AddEventType(_eventTypeName, propertyTypes);
    
    		_epService = EPServiceProviderManager.GetProvider(_container, "Adapter", configuration);
    		_epService.Initialize();
    		EPAdministrator administrator = _epService.EPAdministrator;
    		String statementText = "select * from mapEvent#length(5)";
    		EPStatement statement = administrator.CreateEPL(statementText);
    		_listener = new SupportUpdateListener();
    		statement.Events += _listener.Update;
    
    		// Turn off external clocking
    		_epService.EPRuntime.SendEvent(new TimerControlEvent(TimerControlEvent.ClockTypeEnum.CLOCK_EXTERNAL));
    
    		// Set the clock to 0
    		_currentTime = 0;
    		SendTimeEvent(0);
    
    		_coordinator = new AdapterCoordinatorImpl(_epService, true);
    
        	_propertyOrderNoTimestamp = new String[] { "myInt", "myDouble", "myString" };
        	String[] propertyOrderTimestamp = new String[] { "timestamp", "myInt", "myDouble", "myString" };
    
    		// A CSVPlayer for a file with timestamps, not looping
    		_timestampsNotLooping = new CSVInputAdapterSpec(new AdapterInputSource("regression/timestampOne.csv"), _eventTypeName);
            _timestampsNotLooping.IsUsingEngineThread = true;
    		_timestampsNotLooping.PropertyOrder = propertyOrderTimestamp;
    		_timestampsNotLooping.TimestampColumn = "timestamp";
    
    		// A CSVAdapter for a file with timestamps, looping
    		_timestampsLooping = new CSVInputAdapterSpec(new AdapterInputSource("regression/timestampTwo.csv"), _eventTypeName);
            _timestampsLooping.IsLooping = true;
    		_timestampsLooping.IsUsingEngineThread = true;
    		_timestampsLooping.PropertyOrder = propertyOrderTimestamp;
    		_timestampsLooping.TimestampColumn = "timestamp";
    
    		// A CSVAdapter that sends 10 events per sec, not looping
    		_noTimestampsNotLooping = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampOne.csv"), _eventTypeName);
    		_noTimestampsNotLooping.EventsPerSec = 10;
    		_noTimestampsNotLooping.PropertyOrder = _propertyOrderNoTimestamp;
            _noTimestampsNotLooping.IsUsingEngineThread = true;
    
    		// A CSVAdapter that sends 5 events per sec, looping
    		_noTimestampsLooping = new CSVInputAdapterSpec(new AdapterInputSource("regression/noTimestampTwo.csv"), _eventTypeName);
    		_noTimestampsLooping.EventsPerSec = 5;
            _noTimestampsLooping.IsLooping = true;
    		_noTimestampsLooping.PropertyOrder = _propertyOrderNoTimestamp;
            _noTimestampsLooping.IsUsingEngineThread = true;
    	}
    
        [Test]
    	public void TestRun()
    	{
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _timestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _timestampsLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _noTimestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _noTimestampsLooping));
    
    		// TimeInMillis is 0
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    		_coordinator.Start();
    
    		// TimeInMillis is 50
    		SendTimeEvent(50);
    
    		// TimeInMillis is 100
    		SendTimeEvent(50);
    		AssertEvent(0, 1, 1.1, "timestampOne.one");
    		AssertEvent(1, 1, 1.1, "noTimestampOne.one");
    		AssertSizeAndReset(2);
    
    		// TimeInMillis is 150
    		SendTimeEvent(50);
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
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
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
    		_coordinator.Pause();
    
    		// TimeInMillis is 400
    		SendTimeEvent(50);
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 450
    		SendTimeEvent(50);
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
    		_coordinator.Resume();
    
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
    
    		_coordinator.Stop();
    		SendTimeEvent(1000);
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    	}
    
        [Test]
    	public void TestRunTillNull()
    	{
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _epService, _timestampsNotLooping));
    		_coordinator.Start();
    
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
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 700
    		SendTimeEvent(100);
            Log.Debug(".testRunTillNull time==700");
    		Assert.IsFalse(_listener.GetAndClearIsInvoked());
    
    		// TimeInMillis is 800
    		SendTimeEvent(100);
            Log.Debug(".testRunTillNull time==800");
    	}
    
        [Test]
    	public void TestNotUsingEngineThread()
    	{
    		_coordinator = new AdapterCoordinatorImpl(_epService, false);
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _epService, _noTimestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _epService, _timestampsNotLooping));
    
    		long startTime = Environment.TickCount;
    		_coordinator.Start();
    		long endTime = Environment.TickCount;
    
    		// The last event should be sent after 500 ms
    		Assert.IsTrue(endTime - startTime > 500);
    
    		Assert.AreEqual(6, _listener.GetNewDataList().Count);
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
    		_coordinator = new AdapterCoordinatorImpl(_epService, false, true, false);
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _epService, _noTimestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_container, _epService, _timestampsNotLooping));
    
    		long startTime = Environment.TickCount;
    		_coordinator.Start();
    		long endTime = Environment.TickCount;
    
    		// Check that we haven't been kept waiting
            Assert.That(endTime - startTime, Is.LessThan(100));
    
    		Assert.AreEqual(6, _listener.GetNewDataList().Count);
    		AssertEvent(0, 1, 1.1, "noTimestampOne.one");
    		AssertEvent(1, 1, 1.1, "timestampOne.one");
    		AssertEvent(2, 2, 2.2, "noTimestampOne.two");
    		AssertEvent(3, 3, 3.3, "noTimestampOne.three");
    		AssertEvent(4, 3, 3.3, "timestampOne.three");
    		AssertEvent(5, 5, 5.5, "timestampOne.five");
    	}
    
    	private void AssertEvent(int howManyBack, int? myInt, double? myDouble, String myString)
    	{
    		Assert.IsTrue(_listener.IsInvoked());
    		Assert.IsTrue(howManyBack < _listener.GetNewDataList().Count);
    		EventBean[] data = _listener.GetNewDataList()[howManyBack];
    		Assert.AreEqual(1, data.Length);
    		EventBean theEvent = data[0];
    		Assert.AreEqual(myInt, theEvent.Get("myInt"));
    		Assert.AreEqual(myDouble, theEvent.Get("myDouble"));
    		Assert.AreEqual(myString, theEvent.Get("myString"));
    	}
    
    
    	private void SendTimeEvent(int timeIncrement){
    		_currentTime += timeIncrement;
    	    CurrentTimeEvent theEvent = new CurrentTimeEvent(_currentTime);
    	    _epService.EPRuntime.SendEvent(theEvent);
    	}
    
    	private void AssertSizeAndReset(int size)
    	{
    		IList<EventBean[]> list = _listener.GetNewDataList();
    		Assert.AreEqual(size, list.Count);
    		list.Clear();
    		_listener.GetAndClearIsInvoked();
    	}
    
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;
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
        private EPRuntimeProvider _runtimeProvider;
    	private EPRuntime _runtime;
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
    		propertyTypes.Put("MyInt", typeof(int));
    		propertyTypes.Put("MyDouble", typeof(double?));
    		propertyTypes.Put("MyString", typeof(String));
    
    		_eventTypeName = "mapEvent";
    		var configuration = new Configuration(_container);
            configuration.Runtime.Threading.IsInternalTimerEnabled = false;
            configuration.Common.AddEventType(_eventTypeName, propertyTypes);

            _runtimeProvider = new EPRuntimeProvider();
    		_runtime = _runtimeProvider.GetRuntime("Adapter", configuration);
    		_runtime.Initialize();
            
    		var statementText = "select * from mapEvent#length(5)";
            var statement = CompileUtil.CompileDeploy(_runtime, statementText).Statements[0];
    		_listener = new SupportUpdateListener();
    		statement.Events += _listener.Update;
    
    		// Set the clock to 0
    		_currentTime = 0;
    		SendTimeEvent(0);
    
    		_coordinator = new AdapterCoordinatorImpl(_runtime, true);
    
        	_propertyOrderNoTimestamp = new[] { "MyInt", "MyDouble", "MyString" };
        	var propertyOrderTimestamp = new[] { "timestamp", "MyInt", "MyDouble", "MyString" };
    
    		// A CSVPlayer for a file with timestamps, not looping
    		_timestampsNotLooping = new CSVInputAdapterSpec(new AdapterInputSource(_container, "regression/timestampOne.csv"), _eventTypeName);
            _timestampsNotLooping.IsUsingEngineThread = true;
    		_timestampsNotLooping.PropertyOrder = propertyOrderTimestamp;
    		_timestampsNotLooping.TimestampColumn = "timestamp";
    
    		// A CSVAdapter for a file with timestamps, looping
    		_timestampsLooping = new CSVInputAdapterSpec(new AdapterInputSource(_container, "regression/timestampTwo.csv"), _eventTypeName);
            _timestampsLooping.IsLooping = true;
    		_timestampsLooping.IsUsingEngineThread = true;
    		_timestampsLooping.PropertyOrder = propertyOrderTimestamp;
    		_timestampsLooping.TimestampColumn = "timestamp";
    
    		// A CSVAdapter that sends 10 events per sec, not looping
    		_noTimestampsNotLooping = new CSVInputAdapterSpec(new AdapterInputSource(_container, "regression/noTimestampOne.csv"), _eventTypeName);
    		_noTimestampsNotLooping.EventsPerSec = 10;
    		_noTimestampsNotLooping.PropertyOrder = _propertyOrderNoTimestamp;
            _noTimestampsNotLooping.IsUsingEngineThread = true;
    
    		// A CSVAdapter that sends 5 events per sec, looping
    		_noTimestampsLooping = new CSVInputAdapterSpec(new AdapterInputSource(_container, "regression/noTimestampTwo.csv"), _eventTypeName);
    		_noTimestampsLooping.EventsPerSec = 5;
            _noTimestampsLooping.IsLooping = true;
    		_noTimestampsLooping.PropertyOrder = _propertyOrderNoTimestamp;
            _noTimestampsLooping.IsUsingEngineThread = true;
    	}
    
        [Test]
    	public void TestRun()
    	{
    		_coordinator.Coordinate(new CSVInputAdapter(_timestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_timestampsLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_noTimestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_noTimestampsLooping));
    
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
    		_coordinator.Coordinate(new CSVInputAdapter(_runtime, _timestampsNotLooping));
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
    		_coordinator = new AdapterCoordinatorImpl(_runtime, false);
    		_coordinator.Coordinate(new CSVInputAdapter(_runtime, _noTimestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_runtime, _timestampsNotLooping));
    
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
    		_coordinator = new AdapterCoordinatorImpl(_runtime, false, true, false);
    		_coordinator.Coordinate(new CSVInputAdapter(_runtime, _noTimestampsNotLooping));
    		_coordinator.Coordinate(new CSVInputAdapter(_runtime, _timestampsNotLooping));
    
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
    		var data = _listener.GetNewDataList()[howManyBack];
    		Assert.AreEqual(1, data.Length);
    		var theEvent = data[0];
    		Assert.AreEqual(myInt, theEvent.Get("MyInt"));
    		Assert.AreEqual(myDouble, theEvent.Get("MyDouble"));
    		Assert.AreEqual(myString, theEvent.Get("MyString"));
    	}
    
    
    	private void SendTimeEvent(int timeIncrement){
    		_currentTime += timeIncrement;
            _runtime.EventService.AdvanceTime(_currentTime);
    	}
    
    	private void AssertSizeAndReset(int size)
    	{
    		var list = _listener.GetNewDataList();
    		Assert.AreEqual(size, list.Count);
    		list.Clear();
    		_listener.GetAndClearIsInvoked();
    	}
    
    }
}

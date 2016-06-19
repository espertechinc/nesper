///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.resultset
{
    [TestFixture]
	public class TestGroupedTimeWinUniqueSortMinMax
    {
	    private Configuration Setup()
	    {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("Sensor", typeof(Sensor));
	        return config;
	    }

	    private void LogEvent (object theEvent)
        {
	        log.Info("Sending " + theEvent);
	    }

        [Test]
	    public void TestSensorQuery()
        {
	        var configuration = Setup();
	        configuration.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
	        var epService = EPServiceProviderManager.GetDefaultProvider(configuration);
	        epService.Initialize();
	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.StartTest(epService, GetType(), GetType().FullName);}
	        var listener = new MatchListener();

	        var stmtString =
	              "SELECT max(high.type) as type, \n" +
	              " max(high.measurement) as highMeasurement, max(high.confidence) as confidenceOfHigh, max(high.device) as deviceOfHigh\n" +
	              ",min(low.measurement) as lowMeasurement, min(low.confidence) as confidenceOfLow, min(low.device) as deviceOfLow\n" +
	              "FROM\n" +
	              " Sensor.std:groupwin(type).win:time(1 hour).std:unique(device).ext:sort(1, measurement desc) as high " +
	              ",Sensor.std:groupwin(type).win:time(1 hour).std:unique(device).ext:sort(1, measurement asc) as low ";

	        var stmt = epService.EPAdministrator.CreateEPL(stmtString);
	        log.Info(stmtString);
            stmt.Events += listener.Update;

	        var runtime = epService.EPRuntime;
	        IList<Sensor> events = new List<Sensor>();
	        events.Add(new Sensor("Temperature", "Device1", 68.0, 96.5));
	        events.Add(new Sensor("Temperature", "Device2", 65.0, 98.5));
	        events.Add(new Sensor("Temperature", "Device1", 62.0, 95.3));
	        events.Add(new Sensor("Temperature", "Device2", 71.3, 99.3));
	        foreach (var theEvent in events) {
	            LogEvent (theEvent);
	            runtime.SendEvent(theEvent);
	        }
	        var lastEvent = listener.LastEventBean;
	        Assert.IsTrue (lastEvent != null);
	        Assert.AreEqual (62.0,lastEvent.Get("lowMeasurement"));
	        Assert.AreEqual ("Device1",lastEvent.Get("deviceOfLow"));
	        Assert.AreEqual (95.3,lastEvent.Get("confidenceOfLow"));
	        Assert.AreEqual (71.3,lastEvent.Get("highMeasurement"));
	        Assert.AreEqual ("Device2",lastEvent.Get("deviceOfHigh"));
	        Assert.AreEqual (99.3,lastEvent.Get("confidenceOfHigh"));

	        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.EndTest();}
	        epService.Dispose();
	    }

	    public class Sensor
        {
	        public Sensor() { }
            public Sensor(string type, string device, double? measurement, double? confidence)
            {
	            Type = type;
	            Device = device;
	            Measurement = measurement;
	            Confidence = confidence;
	         }

	        public string Type { get; set; }

	        public string Device { get; set; }

	        public double? Measurement { get; set; }

	        public double? Confidence { get; set; }
        }

	    public class MatchListener
        {
	        public MatchListener()
	        {
	            LastEvent = null;
	            LastEventBean = null;
	            Count = 0;
	        }

	        public void Update(object sender, UpdateEventArgs e)
	        {
	            Update(e.NewEvents, e.OldEvents);
	        }

	        public void Update(EventBean[] newEvents, EventBean[] oldEvents)
            {
	            log.Info("New events.................");
	            if (newEvents != null) {
	                for (var i = 0; i < newEvents.Length; i++) {
	                    var e = newEvents[i];
	                    var t = e.EventType;
	                    var propNames = t.PropertyNames;
	                    log.Info("event[" + i + "] of type " + t);
	                    for (var j=0; j < propNames.Length; j++) {
	                        log.Info("    " + propNames[j] + ": " + e.Get(propNames[j]));
	                    }
	                    Count++;
	                    LastEvent = e.Underlying;
	                    LastEventBean = e;
	                }
	            }
	            log.Info("Removing events.................");
	            if (oldEvents != null) {
	                for (var i = 0; i < oldEvents.Length; i++) {
	                    var e = oldEvents[i];
	                    var t = e.EventType;
	                    var propNames = t.PropertyNames;
	                    log.Info("event[" + i + "] of type " + t);
	                    for (var j=0; j < propNames.Length; j++) {
	                        log.Info("    " + propNames[j] + ": " + e.Get(propNames[j]));
	                    }
	                    Count--;
	                }
	            }
	            log.Info("......................................");
	        }

	        public int Count { get; private set; }

	        public object LastEvent { get; private set; }

	        public EventBean LastEventBean { get; private set; }
        }

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	}
} // end of namespace

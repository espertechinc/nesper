///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    using Map = IDictionary<string, object>;

    [TestFixture]
    public class TestGroupedTimeWinUniqueSortMinMax
    {
        private static Configuration GetConfiguration()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Sensor", typeof(Sensor));
            return config;
        }
    
        private void LogEvent (Object theEvent) {
            Log.Info("Sending " + theEvent);
        }
    
        [Test]
        public void TestSensorQuery()
        {
            Configuration configuration = GetConfiguration();
            configuration.EngineDefaults.ViewResourcesConfig.IsAllowMultipleExpiryPolicies = true;
            EPServiceProvider epService = EPServiceProviderManager.GetProvider("testSensorQuery", configuration);
            epService.Initialize();
            MatchListener listener = new MatchListener();
    
            String stmtString =
                  "SELECT max(high.type) as type, \n" +
                  " max(high.measurement) as highMeasurement, max(high.confidence) as confidenceOfHigh, max(high.device) as deviceOfHigh\n" +
                  ",min(low.measurement) as lowMeasurement, min(low.confidence) as confidenceOfLow, min(low.device) as deviceOfLow\n" +
                  "FROM\n" +
                  " Sensor.std:groupwin(type).win:time(1 hour).std:unique(device).ext:sort(1, measurement desc) as high " +
                  ",Sensor.std:groupwin(type).win:time(1 hour).std:unique(device).ext:sort(1, measurement asc) as low ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
            Log.Info(stmtString);
            stmt.Events += (sender, args) => listener.Update(args.NewEvents, args.OldEvents);
    
            EPRuntime runtime = epService.EPRuntime;
            List<Sensor> events = new List<Sensor>();
            events.Add(new Sensor("Temperature", "Device1", 68.0, 96.5));
            events.Add(new Sensor("Temperature", "Device2", 65.0, 98.5));
            events.Add(new Sensor("Temperature", "Device1", 62.0, 95.3));
            events.Add(new Sensor("Temperature", "Device2", 71.3, 99.3));
            foreach (Sensor theEvent in events) {
                LogEvent (theEvent);
                runtime.SendEvent(theEvent);
            }
            EventBean lastEvent = listener.LastEventBean;
            Assert.IsTrue (lastEvent != null);
            Assert.AreEqual (62.0,lastEvent.Get("lowMeasurement"));
            Assert.AreEqual ("Device1",lastEvent.Get("deviceOfLow"));
            Assert.AreEqual (95.3,lastEvent.Get("confidenceOfLow"));
            Assert.AreEqual (71.3,lastEvent.Get("highMeasurement"));
            Assert.AreEqual ("Device2",lastEvent.Get("deviceOfHigh"));
            Assert.AreEqual (99.3,lastEvent.Get("confidenceOfHigh"));
    
            epService.Dispose();
        }
    
        public class Sensor {
            public Sensor() {
            }

            public Sensor(String type, String device, Double? measurement, Double? confidence)
            {
                Type = type;
                Device = device;
                Measurement = measurement;
                Confidence = confidence;
             }

            public string Type { get; set; }

            public string Device { get; set; }

            public Double? Measurement { get; set; }

            public Double? Confidence { get; set; }
        }
    
        class MatchListener
        {
            public void Update(EventBean[] newEvents, EventBean[] oldEvents)
            {
                Log.Info("New events.................");
                if (newEvents != null) {
                    for (int i = 0; i < newEvents.Length; i++) {
                        var e = newEvents[i];
                        var t = e.EventType;
                        var propNames = t.PropertyNames;
                        Log.Info("event[" + i + "] of type " + t);
                        foreach (string propName in propNames)
                        {
                            Log.Info("    " + propName + ": " + e.Get(propName));
                        }
                        Count++;
                        LastEvent = e.Underlying;
                        LastEventBean = e;
                    }
                }
                Log.Info("Removing events.................");
                if (oldEvents != null) {
                    for (int i = 0; i < oldEvents.Length; i++) {
                        var e = oldEvents[i];
                        var t = e.EventType;
                        var propNames = t.PropertyNames;
                        Log.Info("event[" + i + "] of type " + t);
                        foreach (string propName in propNames)
                        {
                            Log.Info("    " + propName + ": " + e.Get(propName));
                        }
                        Count--;
                    }
                }
                Log.Info("......................................");
            }

            public int Count { get; private set; }

            public object LastEvent { get; private set; }

            public EventBean LastEventBean { get; private set; }
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}

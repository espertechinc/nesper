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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.resultset.querytype
{
    public class ExecQuerytypeRowForAllWGroupedTimeWinUnique : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.AddEventType("Sensor", typeof(Sensor));
            configuration.EngineDefaults.ViewResources.IsAllowMultipleExpiryPolicies = true;
        }
    
        public override void Run(EPServiceProvider epService)
        {
            var listener = new MatchListener();
    
            string stmtString =
                    "SELECT max(high.type) as type, \n" +
                            " max(high.measurement) as highMeasurement, max(high.confidence) as confidenceOfHigh, max(high.device) as deviceOfHigh\n" +
                            ",min(low.measurement) as lowMeasurement, min(low.confidence) as confidenceOfLow, min(low.device) as deviceOfLow\n" +
                            "FROM\n" +
                            " Sensor#groupwin(type)#time(1 hour)#unique(device)#sort(1, measurement desc) as high " +
                            ",Sensor#groupwin(type)#time(1 hour)#unique(device)#sort(1, measurement asc) as low ";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
            Log.Info(stmtString);
            stmt.Events += listener.Update;
    
            EPRuntime runtime = epService.EPRuntime;
            var events = new List<Sensor>();
            events.Add(new Sensor("Temperature", "Device1", 68.0, 96.5));
            events.Add(new Sensor("Temperature", "Device2", 65.0, 98.5));
            events.Add(new Sensor("Temperature", "Device1", 62.0, 95.3));
            events.Add(new Sensor("Temperature", "Device2", 71.3, 99.3));
            foreach (Sensor theEvent in events) {
                runtime.SendEvent(theEvent);
            }
            EventBean lastEvent = listener.LastEventBean;
            Assert.IsTrue(lastEvent != null);
            Assert.AreEqual(62.0, lastEvent.Get("lowMeasurement"));
            Assert.AreEqual("Device1", lastEvent.Get("deviceOfLow"));
            Assert.AreEqual(95.3, lastEvent.Get("confidenceOfLow"));
            Assert.AreEqual(71.3, lastEvent.Get("highMeasurement"));
            Assert.AreEqual("Device2", lastEvent.Get("deviceOfHigh"));
            Assert.AreEqual(99.3, lastEvent.Get("confidenceOfHigh"));
        }

        public class Sensor
        {
            public Sensor()
            {
            }

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

        class MatchListener
        {
            public void Update(object sender, UpdateEventArgs args)
            {
                var oldEvents = args.OldEvents;
                var newEvents = args.NewEvents;

                Log.Info("New events.................");
                if (newEvents != null) {
                    for (int i = 0; i < newEvents.Length; i++) {
                        var e = newEvents[i];
                        EventType t = e.EventType;
                        string[] propNames = t.PropertyNames;
                        Log.Info("event[" + i + "] of type " + t);
                        for (int j = 0; j < propNames.Length; j++) {
                            Log.Info("    " + propNames[j] + ": " + e.Get(propNames[j]));
                        }
                        Count++;
                        LastEvent = e.Underlying;
                        LastEventBean = e;
                    }
                }
                Log.Info("Removing events.................");
                if (oldEvents != null) {
                    for (int i = 0; i < oldEvents.Length; i++) {
                        EventBean e = oldEvents[i];
                        EventType t = e.EventType;
                        string[] propNames = t.PropertyNames;
                        Log.Info("event[" + i + "] of type " + t);
                        for (int j = 0; j < propNames.Length; j++) {
                            Log.Info("    " + propNames[j] + ": " + e.Get(propNames[j]));
                        }
                        Count--;
                    }
                }
                Log.Info("......................................");
            }

            public int Count { get; private set; } = 0;

            public object LastEvent { get; private set; } = null;

            public EventBean LastEventBean { get; private set; } = null;
        }
    
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
} // end of namespace

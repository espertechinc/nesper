///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewGroupLengthWinWeightAvg : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Sensor", typeof(Sensor));
        }
    
        public override void Run(EPServiceProvider epService) {
            bool useGroup = true;
            var listener = new SupportUpdateListener();
            if (useGroup) {
                // 0.69 sec for 100k
                string stmtString = "select * from Sensor#Groupwin(type)#length(10000000)#Weighted_avg(measurement, confidence)";
                //string stmtString = "SELECT * FROM Sensor#Groupwin(type)#length(1000)#Weighted_avg('measurement','confidence')";
                EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
                stmt.Events += listener.Update;
            } else {
                // 0.53 sec for 100k
                for (int i = 0; i < 10; i++) {
                    string stmtString = "SELECT * FROM Sensor(type='A" + i + "')#length(1000000)#Weighted_avg(measurement,confidence)";
                    EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
                    stmt.Events += listener.Update;
                }
            }
    
            // prime
            for (int i = 0; i < 100; i++) {
                epService.EPRuntime.SendEvent(new Sensor("A", "1", (double) i, (double) i));
            }
    
            // measure
            long numEvents = 10000;
            long startTime = PerformanceObserver.NanoTime;
            for (int i = 0; i < numEvents; i++) {
                //int modulo = i % 10;
                int modulo = 1;
                string type = "A" + modulo;
                epService.EPRuntime.SendEvent(new Sensor(type, "1", (double) i, (double) i));
    
                if (i % 1000 == 0) {
                    //Log.Info("Send " + i + " events");
                    listener.Reset();
                }
            }
            long endTime = PerformanceObserver.NanoTime;
            double delta = (endTime - startTime) / 1000d / 1000d / 1000d;
            // Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 1);
        }
    
        [Serializable]
        public class Sensor  {
    
            public Sensor() {
            }
    
            public Sensor(string type, string device, double? measurement, double? confidence) {
                this.type = type;
                this.device = device;
                this.measurement = measurement;
                this.confidence = confidence;
            }
    
            public void SetType(string type) {
                this.type = type;
            }
    
            public string GetType() {
                return type;
            }
    
            public void SetDevice(string device) {
                this.device = device;
            }
    
            public string GetDevice() {
                return device;
            }
    
            public void SetMeasurement(double? measurement) {
                this.measurement = measurement;
            }
    
            public double? GetMeasurement() {
                return measurement;
            }
    
            public void SetConfidence(double? confidence) {
                this.confidence = confidence;
            }
    
            public double? GetConfidence() {
                return confidence;
            }
    
            private string type;
            private string device;
            private double? measurement;
            private double? confidence;
        }
    }
} // end of namespace

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
using com.espertech.esper.compat;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    public class ExecViewGroupLengthWinWeightAvg : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.AddEventType("Sensor", typeof(Sensor));
        }
    
        public override void Run(EPServiceProvider epService) {
            var useGroup = true;
            var listener = new SupportUpdateListener();
            if (useGroup) {
                // 0.69 sec for 100k
                var stmtString = "select * from Sensor#groupwin(Type)#length(10000000)#weighted_avg(Measurement, Confidence)";
                var stmt = epService.EPAdministrator.CreateEPL(stmtString);
                stmt.Events += listener.Update;
            } else {
                // 0.53 sec for 100k
                for (var i = 0; i < 10; i++) {
                    var stmtString = "SELECT * FROM Sensor(type='A" + i + "')#length(1000000)#weighted_avg(measurement,confidence)";
                    var stmt = epService.EPAdministrator.CreateEPL(stmtString);
                    stmt.Events += listener.Update;
                }
            }
    
            // prime
            for (var i = 0; i < 100; i++) {
                epService.EPRuntime.SendEvent(new Sensor("A", "1", (double) i, (double) i));
            }
    
            // measure
            long numEvents = 10000;
            var startTime = PerformanceObserver.NanoTime;
            for (var i = 0; i < numEvents; i++) {
                //int modulo = i % 10;
                var modulo = 1;
                var type = "A" + modulo;
                epService.EPRuntime.SendEvent(new Sensor(type, "1", (double) i, (double) i));
    
                if (i % 1000 == 0) {
                    //Log.Info("Send " + i + " events");
                    listener.Reset();
                }
            }
            var endTime = PerformanceObserver.NanoTime;
            var delta = (endTime - startTime) / 1000d / 1000d / 1000d;
            // Log.Info("delta=" + delta);
            Assert.IsTrue(delta < 1);
        }
    
        [Serializable]
        public class Sensor  {
    
            public Sensor() {
            }
    
            public Sensor(string type, string device, double? measurement, double? confidence) {
                this._type = type;
                this._device = device;
                this._measurement = measurement;
                this._confidence = confidence;
            }

            public string Type {
                get => _type;
                set => _type = value;
            }

            public string Device {
                get => _device;
                set => _device = value;
            }

            public double? Measurement {
                get => _measurement;
                set => _measurement = value;
            }

            public double? Confidence {
                get => _confidence;
                set => _confidence = value;
            }

            private string _type;
            private string _device;
            private double? _measurement;
            private double? _confidence;
        }
    }
} // end of namespace

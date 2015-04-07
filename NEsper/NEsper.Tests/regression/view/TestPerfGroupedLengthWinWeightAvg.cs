///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat;
using com.espertech.esper.support.client;

using NUnit.Framework;


namespace com.espertech.esper.regression.view
{
    [TestFixture]
    public class TestPerfGroupedLengthWinWeightAvg 
    {
        [Test]
        public void TestSensorQuery()
        {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.AddEventType("Sensor", typeof(Sensor));
            EPServiceProvider epService = EPServiceProviderManager.GetProvider("testSensorQuery", config);
    
            bool useGroup = true;
            SupportUpdateListener listener = new SupportUpdateListener();
            if (useGroup)
            {
                // 0.69 sec for 100k
                String stmtString = "select * from Sensor.std:groupwin(type).win:length(10000000).stat:weighted_avg(measurement, confidence)";
                //String stmtString = "SELECT * FROM Sensor.std:groupwin(type).win:length(1000).stat:weighted_avg('measurement','confidence')";
                EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
                stmt.Events += listener.Update;
            }
            else
            {
                // 0.53 sec for 100k
                for (int i = 0; i < 10; i++)
                {
                    String stmtString = "SELECT * FROM Sensor(type='A" + i + "').win:length(1000000).stat:weighted_avg(measurement,confidence)";
                    EPStatement stmt = epService.EPAdministrator.CreateEPL(stmtString);
                    stmt.Events += listener.Update;
                }
            }
    
            // prime
            for (int i = 0; i < 100; i++) {
                epService.EPRuntime.SendEvent(new Sensor("A", "1", i, i));
            }
    
            // measure
            long numEvents = 10000;
            long delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < numEvents; i++)
                    {
                        //int modulo = i % 10;
                        int modulo = 1;
                        String type = "A" + modulo;
                        epService.EPRuntime.SendEvent(new Sensor(type, "1", i, i));

                        if (i%1000 == 0)
                        {
                            //Console.Out.WriteLine("Send " + i + " events");
                            listener.Reset();
                        }
                    }
                });
            // Console.Out.WriteLine("delta=" + delta);
            Assert.IsTrue(delta < 1000);
    
            epService.Dispose();
        }

        public class Sensor
        {
            public Sensor()
            {
            }

            public Sensor(String type, String device, double? measurement, double? confidence)
            {
                Type = type;
                Device = device;
                Measurement = measurement;
                Confidence = confidence;
            }

            public String Type { get; set; }
            public String Device { get; set; }
            public double? Measurement { get; set; }
            public double? Confidence { get; set; }
        }
    }
}

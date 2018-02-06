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
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.view
{
    [TestFixture]
	public class TestViewGroupLengthWinWeightAvg 
	{
        [Test]
	    public void TestSensorQuery()
        {
	        var config = SupportConfigFactory.GetConfiguration();
	        config.AddEventType("Sensor", typeof(Sensor));
	        var epService = EPServiceProviderManager.GetDefaultProvider(config);
	        epService.Initialize();

	        var useGroup = true;
	        var listener = new SupportUpdateListener();
	        if (useGroup)
	        {
	            // 0.69 sec for 100k
	            var stmtString = "select * from Sensor#groupwin(type)#length(10000000)#weighted_avg(measurement, confidence)";
	            //String stmtString = "SELECT * FROM Sensor#groupwin(type)#length(1000)#weighted_avg('measurement','confidence')";
	            var stmt = epService.EPAdministrator.CreateEPL(stmtString);
	            stmt.AddListener(listener);
	        }
	        else
	        {
	            // 0.53 sec for 100k
	            for (var i = 0; i < 10; i++)
	            {
	                var stmtString = "SELECT * FROM Sensor(type='A" + i + "')#length(1000000)#weighted_avg(measurement,confidence)";
	                var stmt = epService.EPAdministrator.CreateEPL(stmtString);
	                stmt.AddListener(listener);
	            }
	        }

	        // prime
	        for (var i = 0; i < 100; i++) {
	            epService.EPRuntime.SendEvent(new Sensor("A", "1", i, i));
	        }

	        // measure
	        var numEvents = 10000L;
            var delta = PerformanceObserver.TimeMicro(
                () =>
                {
                    for (var i = 0; i < numEvents; i++)
                    {
                        //int modulo = i % 10;
                        var modulo = 1;
                        var type = "A" + modulo;
                        epService.EPRuntime.SendEvent(new Sensor(type, "1", i, i));

                        if (i % 1000 == 0)
                        {
                            //System.out.println("Send " + i + " events");
                            listener.Reset();
                        }
                    }
                });
            
	        // System.out.println("delta=" + delta);
	        Assert.That(delta, Is.LessThan(1000000));

	        epService.Dispose();
	    }

	    [Serializable]
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
	}
} // end of namespace

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Xml;

using com.espertech.esper.common.client;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.StockTicker.eventbean;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.AutoId
{
	public class AutoIdSimMain : IDisposable
    {
	    private static readonly Random RANDOM = new Random();
	    private static readonly string[] SENSOR_IDS = {
	                                                      "urn:epc:1:4.16.30",
                                                          "urn:epc:1:4.16.32", 
                                                          "urn:epc:1:4.16.36", 
                                                          "urn:epc:1:4.16.38"
	                                                  };

	    private const string XML_ROOT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
	            "<pmlcore:Sensor \n" +
	            "  xmlns=\"urn:autoid:specification:interchange:PMLCore:xml:schema:1\" \n" +
	            "  xmlns:pmlcore=\"urn:autoid:specification:interchange:PMLCore:xml:schema:1\" \n" +
	            "  xmlns:autoid=\"http://www.autoidcenter.org/2003/xml\" \n" +
	            "  xmlns:pmluid=\"urn:autoid:specification:universal:Identifier:xml:schema:1\" \n" +
	            "  xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" \n" +
	            "  xsi:schemaLocation=\"urn:autoid:specification:interchange:PMLCore:xml:schema:1 AutoIdPmlCore.xsd\">\n";
	
	    private readonly int numEvents;
        private readonly string runtimeUri;
	
	    public static void Main(string[] args) 
	    {
	        LoggerNLog.BasicConfig();
	        LoggerNLog.Register();

            if (args.Length < 1) {
	            Console.Out.WriteLine("Arguments are: <numberOfEvents>");
	            Environment.Exit(-1);
	        }
	
	        int events;
	        try {
	            events = int.Parse(args[0]);
	        } catch (NullReferenceException) {
	            Console.Out.WriteLine("Invalid numberOfEvents:" + args[0]);
	            Environment.Exit(-2);
	            return;
	        }

            // Prime a few assemblies into memory
            var tempA = new StockTick(null, 0.0);
	        var tempB = new PriceLimit(null, null, 0.0);

	        // Run the sample
            var autoIdSimMain = new AutoIdSimMain(events, "AutoIDSim");
            autoIdSimMain.Run();
	    }

        public AutoIdSimMain(int numEvents, string runtimeURI)
        {
	        this.numEvents = numEvents;
            this.runtimeUri = runtimeURI;
	    }

	    /// <summary>
	    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	    /// </summary>
	    /// <filterpriority>2</filterpriority>
	    public void Dispose()
	    {
            EPRuntimeProvider.GetRuntime(runtimeUri)
	            .DeploymentService
	            .UndeployAll();
        }

	    public void Run()
	    {
	        var container = ContainerExtensions.CreateDefaultContainer()
	            .InitializeDefaultServices()
	            .InitializeDatabaseDrivers();

	        // load config - this defines the XML event types to be processed
	        var configFile = "esper.examples.cfg.xml";
	        var url = container.ResourceManager().ResolveResourceURL(configFile);
	        var config = new Configuration(container);
	        config.Configure(url);
	
	        // get engine instance
	        var runtime = EPRuntimeProvider.GetRuntime(runtimeUri, config);
	
	        // set up statement
	        var rfidStmt = RFIDTagsPerSensorStmt.Create(runtime);
	        rfidStmt.Events += LogRate;
	
	        // Send events
	        var eventCount = 0;
	        while(eventCount < numEvents) {
	            SendEvent(runtime);
	            eventCount++;
	        }
	    }

        public static void LogRate(object sender, UpdateEventArgs e)
        {
            if (e.NewEvents != null)
            {
                LogRate(e.NewEvents[0]);
            }
        }

        private static void LogRate(EventBean eventBean)
        {
            var sensorId = (string)eventBean["sensorId"];
            var numTags = (double)eventBean["numTagsPerSensor"];

            Log.Info("Sensor " + sensorId + " totals " + numTags + " tags");
        }

	    private void SendEvent(EPRuntime runtime)
	    {
	        var eventXMLText = GenerateEvent();
	        var simpleDoc = new XmlDocument() ;
	        simpleDoc.LoadXml(eventXMLText) ;
	        runtime.EventService.SendEventXMLDOM(simpleDoc, "AutoIdRFIDExample");
	    }
	
	    private string GenerateEvent()
	    {
	        var buffer = new StringBuilder();
	        buffer.Append(XML_ROOT);
	
	        var sensorId = SENSOR_IDS[RANDOM.Next(SENSOR_IDS.Length)];
	        buffer.Append("<pmluid:ID>");
	        buffer.Append(sensorId);
	        buffer.Append("</pmluid:ID>");
	
	        buffer.Append("<pmlcore:Observation>");
	        buffer.Append("<pmlcore:Command>READ_PALLET_TAGS_ONLY</pmlcore:Command>");
	
	        for (var i = 0; i < RANDOM.Next(6) + 1; i++)
	        {
	            buffer.Append("<pmlcore:Tag><pmluid:ID>urn:epc:1:2.24.400</pmluid:ID></pmlcore:Tag>");
	        }
	
	        buffer.Append("</pmlcore:Observation>");
	        buffer.Append("</pmlcore:Sensor>");
	
	        return buffer.ToString();
	    }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	}
}

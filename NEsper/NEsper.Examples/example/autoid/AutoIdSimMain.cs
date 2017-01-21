///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////


using System;
using System.Text;
using System.Xml;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using NEsper.Examples.StockTicker.eventbean;

namespace com.espertech.esper.example.autoid
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
        private readonly String engineURI;
	
	    public static void Main(string[] args) 
	    {
            log4net.Config.XmlConfigurator.Configure();

	        if (args.Length < 1) {
	            Console.Out.WriteLine("Arguments are: <numberOfEvents>");
	            Environment.Exit(-1);
	        }
	
	        int events;
	        try {
	            events = Int32.Parse(args[0]);
	        } catch (NullReferenceException) {
	            Console.Out.WriteLine("Invalid numberOfEvents:" + args[0]);
	            Environment.Exit(-2);
	            return;
	        }

            // Prime a few assemblies into memory
            StockTick tempA = new StockTick(null, 0.0);
	        PriceLimit tempB = new PriceLimit(null, null, 0.0);

	        // Run the sample
            AutoIdSimMain autoIdSimMain = new AutoIdSimMain(events, "AutoIDSim");
            autoIdSimMain.Run();
	    }

        public AutoIdSimMain(int numEvents, String engineURI)
        {
	        this.numEvents = numEvents;
            this.engineURI = engineURI;
	    }

	    /// <summary>
	    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	    /// </summary>
	    /// <filterpriority>2</filterpriority>
	    public void Dispose()
	    {
            EPServiceProviderManager.GetProvider(engineURI).EPAdministrator.DestroyAllStatements();
        }

	    public void Run()
	    {
	        // load config - this defines the XML event types to be processed
	        string configFile = "esper.examples.cfg.xml";
	        Uri url = ResourceManager.ResolveResourceURL(configFile);
	        Configuration config = new Configuration();
	        config.Configure(url);
	
	        // get engine instance
	        EPServiceProvider epService = EPServiceProviderManager.GetProvider(engineURI, config);
	
	        // set up statement
	        var rfidStmt = RFIDTagsPerSensorStmt.Create(epService.EPAdministrator);
	        rfidStmt.Events += LogRate;
	
	        // Send events
	        int eventCount = 0;
	        while(eventCount < numEvents) {
	            SendEvent(epService.EPRuntime);
	            eventCount++;
	        }
	    }

        public static void LogRate(Object sender, UpdateEventArgs e)
        {
            if (e.NewEvents != null)
            {
                LogRate(e.NewEvents[0]);
            }
        }

        private static void LogRate(EventBean eventBean)
        {
            string sensorId = (string)eventBean["sensorId"];
            double numTags = (double)eventBean["numTagsPerSensor"];

            Log.Info("Sensor " + sensorId + " totals " + numTags + " tags");
        }

	    private void SendEvent(EPRuntime epRuntime)
	    {
	        String eventXMLText = GenerateEvent();
	        XmlDocument simpleDoc = new XmlDocument() ;
	        simpleDoc.LoadXml(eventXMLText) ;
	        epRuntime.SendEvent(simpleDoc);
	    }
	
	    private String GenerateEvent()
	    {
	        StringBuilder buffer = new StringBuilder();
	        buffer.Append(XML_ROOT);
	
	        String sensorId = SENSOR_IDS[RANDOM.Next(SENSOR_IDS.Length)];
	        buffer.Append("<pmluid:ID>");
	        buffer.Append(sensorId);
	        buffer.Append("</pmluid:ID>");
	
	        buffer.Append("<pmlcore:Observation>");
	        buffer.Append("<pmlcore:Command>READ_PALLET_TAGS_ONLY</pmlcore:Command>");
	
	        for (int i = 0; i < RANDOM.Next(6) + 1; i++)
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

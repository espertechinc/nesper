///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.client;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logger;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.NamedWindowQuery
{
    public class NamedWindowQueryMain
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(String[] args)
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();

            var main = new NamedWindowQueryMain();

            try {
                main.RunExample(false, "NamedWindowQuery");
            }
            catch (Exception ex) {
                Log.Error("Unexpected error occured running example:" + ex.Message);
            }
        }

        public void RunExample(bool isRunFromUnitTest, string engineURI)
        {
            int numEventsToLoad = 100000;
            int numFireAndForgetExecutions = 100;
            int numOnEventQueryExecutions = 100000;
            if (isRunFromUnitTest) {
                numEventsToLoad = 1000;
                numFireAndForgetExecutions = 5;
                numOnEventQueryExecutions = 5;
            }

            EPServiceProvider epService = EPServiceProviderManager.GetProvider(engineURI);

            // This example initializes the engine instance as it is running within an overall test suite.
            // This step would not be required unless re-using the same engine instance with different configurations. 
            epService.Initialize();

            // define event type - this example uses Map event representation
            //
            IDictionary<String, Object> definition = new LinkedHashMap<String, Object>();
            definition.Put("sensor", typeof (string));
            definition.Put("temperature", typeof (double));

            epService.EPAdministrator.Configuration.AddEventType("SensorEvent", definition);

            // define a named window to hold the last 1000000 (1M) events
            //
            String stmtText = "create window SensorWindow.win:keepall() as select * from SensorEvent";
            Log.Info("Creating named window : " + stmtText);
            epService.EPAdministrator.CreateEPL(stmtText);

            stmtText = "insert into SensorWindow select * from SensorEvent";
            Log.Info("Creating insert statement for named window : " + stmtText);
            epService.EPAdministrator.CreateEPL(stmtText);

            // load 1M events
            //
            var random = new Random();
            String[] sensors = "s1,s2,s3,s4,s5,s6".Split(',');

            Log.Info("Generating " + numEventsToLoad + " sensor events for the named window");
            IList<IDictionary<String, Object>> events = new List<IDictionary<String, Object>>();
            for (int i = 0; i < numEventsToLoad; i++) {
                double temperature = Math.Round(random.NextDouble()*10, 5, MidpointRounding.AwayFromZero) + 80;
                String sensor = sensors[random.Next(sensors.Length)];

                IDictionary<String, Object> data = new LinkedHashMap<String, Object>();
                data.Put("temperature", temperature);
                data.Put("sensor", sensor);

                events.Add(data);
            }
            Log.Info("Completed generating sensor events");

            Log.Info("Sending " + events.Count + " sensor events into engine");
            foreach (var @event in events) {
                epService.EPRuntime.SendEvent(@event, "SensorEvent");
            }
            Log.Info("Completed sending sensor events");

            // prepare on-demand query
            //
            var sampleTemperature = (double) events[0].Get("temperature");
            stmtText = "select * from SensorWindow where temperature = " + sampleTemperature;
            Log.Info("Preparing fire-and-forget query : " + stmtText);
            EPOnDemandPreparedQuery onDemandQuery = epService.EPRuntime.PrepareQuery(stmtText);

            Log.Info("Executing fire-and-forget query " + numFireAndForgetExecutions + " times");
            long startTime = Environment.TickCount;
            for (int i = 0; i < numFireAndForgetExecutions; i++) {
                EPOnDemandQueryResult result = onDemandQuery.Execute();
                if (result.Array.Length != 1) {
                    throw new ApplicationException(
                        "Failed assertion of result, expected a single row returned from query");
                }
            }
            long endTime = Environment.TickCount;
            double deltaSec = (endTime - startTime)/1000.0;
            Log.Info("Executing fire-and-forget query " + numFireAndForgetExecutions + " times took " + deltaSec +
                     " seconds");

            // prepare on-select
            //
            IDictionary<String, Object> definitionQuery = new LinkedHashMap<String, Object>();
            definitionQuery.Put("querytemp", typeof (double));
            epService.EPAdministrator.Configuration.AddEventType("SensorQueryEvent", definitionQuery);

            stmtText = "on SensorQueryEvent select sensor from SensorWindow where temperature = querytemp";
            Log.Info("Creating on-select statement for named window : " + stmtText);
            EPStatement onSelectStmt = epService.EPAdministrator.CreateEPL(stmtText);
            onSelectStmt.Subscriber = this;

            Log.Info("Executing on-select query " + numOnEventQueryExecutions + " times");
            startTime = Environment.TickCount;
            for (int i = 0; i < numOnEventQueryExecutions; i++) {
                IDictionary<String, Object> queryParams = new Dictionary<String, Object>();
                queryParams.Put("querytemp", sampleTemperature);

                epService.EPRuntime.SendEvent(queryParams, "SensorQueryEvent");
            }
            endTime = Environment.TickCount;
            deltaSec = (endTime - startTime)/1000.0;
            Log.Info("Executing on-select query " + numOnEventQueryExecutions + " times took " + deltaSec + " seconds");
        }

        public void Update(String result)
        {
            // No action taken here
            // System.out.Println(result);
        }
    }
}

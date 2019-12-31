///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Reflection;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.NamedWindowQuery
{
    public class NamedWindowQueryMain
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
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

        public void RunExample(
            bool isRunFromUnitTest,
            string runtimeUri)
        {
            var numEventsToLoad = 100000;
            var numFireAndForgetExecutions = 100;
            var numOnEventQueryExecutions = 100000;
            if (isRunFromUnitTest) {
                numEventsToLoad = 1000;
                numFireAndForgetExecutions = 5;
                numOnEventQueryExecutions = 5;
            }

            // define event type - this example uses Map event representation
            //
            var configuration = new Configuration();

            var definition = new Dictionary<string, object>();
            definition.Put("sensor", typeof(string));
            definition.Put("temperature", typeof(double));
            configuration.Common.AddEventType("SensorEvent", definition);

            var definitionQuery = new Dictionary<string, object>();
            definitionQuery.Put("querytemp", typeof(double));
            configuration.Common.AddEventType("SensorQueryEvent", definitionQuery);

            // This example initializes the engine instance as it is running within an overall test suite.
            // This step would not be required unless re-using the same engine instance with different configurations. 
            var runtime = EPRuntimeProvider.GetRuntime(runtimeUri, configuration);
            runtime.Initialize();

            // define a named window to hold the last 1000000 (1M) events
            //
            var epl = "create window SensorWindow.win:keepall() as select * from SensorEvent";
            Log.Info("Creating named window : " + epl);
            CompileDeploy(epl, runtime);

            epl = "insert into SensorWindow select * from SensorEvent";
            Log.Info("Creating insert statement for named window : " + epl);
            CompileDeploy(epl, runtime);

            // load 1M events
            //
            var random = new Random();
            var sensors = "s1,s2,s3,s4,s5,s6".Split(',');

            Log.Info("Generating " + numEventsToLoad + " sensor events for the named window");
            IList<IDictionary<string, object>> events = new List<IDictionary<string, object>>();
            for (var i = 0; i < numEventsToLoad; i++) {
                var temperature = Math.Round(random.NextDouble() * 10, 5, MidpointRounding.AwayFromZero) + 80;
                var sensor = sensors[random.Next(sensors.Length)];

                IDictionary<string, object> data = new LinkedHashMap<string, object>();
                data.Put("temperature", temperature);
                data.Put("sensor", sensor);

                events.Add(data);
            }

            Log.Info("Completed generating sensor events");

            Log.Info("Sending " + events.Count + " sensor events into runtime");

            foreach (var @event in events) {
                runtime.EventService.SendEventMap(@event, "SensorEvent");
            }

            Log.Info("Completed sending sensor events");

            // prepare on-demand query
            //
            var sampleTemperature = (double) events[0].Get("temperature");
            epl = "select * from SensorWindow where temperature = " + sampleTemperature;
            Log.Info("Compiling fire-and-forget query : " + epl);
            var args = new CompilerArguments();
            args.Path.Add(runtime.RuntimePath);

            var onDemandQueryCompiled = EPCompilerProvider.Compiler.CompileQuery(epl, args);
            var onDemandQuery = runtime.FireAndForgetService.PrepareQuery(onDemandQueryCompiled);

            Log.Info("Executing fire-and-forget query " + numFireAndForgetExecutions + " times");
            long startTime = Environment.TickCount;
            for (var i = 0; i < numFireAndForgetExecutions; i++) {
                var result = onDemandQuery.Execute();
                if (result.Array.Length != 1) {
                    throw new ApplicationException(
                        "Failed assertion of result, expected a single row returned from query");
                }
            }

            long endTime = Environment.TickCount;
            var deltaSec = (endTime - startTime) / 1000.0;
            Log.Info(
                "Executing fire-and-forget query " +
                numFireAndForgetExecutions +
                " times took " +
                deltaSec +
                " seconds");

            // prepare on-select
            //
            epl = "on SensorQueryEvent select sensor from SensorWindow where temperature = querytemp";
            Log.Info("Creating on-select statement for named window : " + epl);
            var onSelectStmt = CompileDeploy(epl, runtime);
            onSelectStmt.Subscriber = this;

            Log.Info("Executing on-select query " + numOnEventQueryExecutions + " times");
            startTime = Environment.TickCount;
            for (var i = 0; i < numOnEventQueryExecutions; i++) {
                var queryParams = new Dictionary<string, object> {
                    ["querytemp"] = sampleTemperature
                };

                runtime.EventService.SendEventMap(queryParams, "SensorQueryEvent");
            }

            endTime = Environment.TickCount;
            deltaSec = (endTime - startTime) / 1000.0;
            Log.Info("Executing on-select query " + numOnEventQueryExecutions + " times took " + deltaSec + " seconds");
        }

        public void Update(string result)
        {
            // No action taken here
            // System.out.Println(result);
        }

        private EPStatement CompileDeploy(
            string epl,
            EPRuntime runtime)
        {
            var args = new CompilerArguments();
            args.Path.Add(runtime.RuntimePath);
            args.Options.AccessModifierNamedWindow = env => NameAccessModifier.PUBLIC; // All named windows are visibile
            args.Configuration.Compiler.ByteCode.AllowSubscriber = true; // allow subscribers

            var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
            var deployment = runtime.DeploymentService.Deploy(compiled);
            return deployment.Statements[0];
        }
    }
}

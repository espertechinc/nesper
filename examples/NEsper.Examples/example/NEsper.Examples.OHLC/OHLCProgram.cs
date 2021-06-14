using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.configuration;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.runtime.client;

namespace NEsper.Examples.OHLC
{
    public class OHLCProgram
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OHLCProgram));

        static void Main(string[] args)
        {
            (new OHLCProgram()).Run("OHLCRuntimeURI");
        }

        public void Run(string runtimeURI)
        {
            Log.Info("Setting up EPL");

            var config = new Configuration();
            config.Common.AddEventType<OHLCTick>("OHLCTick");
            config.Compiler.AddPlugInView("examples", "ohlcbarminute", typeof(OHLCBarPlugInViewForge));
            config.Runtime.Threading.IsInternalTimerEnabled = false; // external timer for testing

            var runtime = EPRuntimeProvider.GetRuntime(runtimeURI, config);
            runtime.Initialize(); // Since running in a unit test may use the same runtime many times

            // set time as an arbitrary start time
            SendTimer(runtime, ToTime("9:01:50"));

            var statements = new string[] {"@name('S1') select * from OHLCTick#groupwin(ticker)#ohlcbarminute(timestamp, price)"};

            foreach (var statement in statements) {
                Log.Info("Creating statement: " + statement);
                var stmt = CompileDeploy(statement, config, runtime);

                if (stmt.Name == "S1") {
                    OHLCUpdateListener listener = new OHLCUpdateListener();
                    stmt.AddListener(listener);
                }
            }

            Log.Info("Sending test events");

            var input = new object[][] {
                new object[] {"9:01:51", null}, // lets start simulating at 9:01:51
                new object[] {"9:01:52", "IBM", 100.5, "9:01:52"}, // lets have an event arrive on time
                new object[] {"9:02:03", "IBM", 100.0, "9:02:03"},
                new object[] {"9:02:10", "IBM", 99.0, "9:02:04"}, // lets have an event arrive later; this timer event also triggers a bucket
                new object[] {"9:02:20", "IBM", 98.0, "9:02:16"},
                new object[] {"9:02:30", "NOC", 11.0, "9:02:30"},
                new object[] {"9:02:45", "NOC", 12.0, "9:02:45"},
                new object[] {"9:02:55", "NOC", 13.0, "9:02:55"},
                new object[] {"9:03:02", "IBM", 101.0, "9:02:58"}, // this event arrives late but counts in the same bucket
                new object[] {"9:03:06", "IBM", 109.0, "9:02:59"}, // this event arrives too late: it should be ignored (5 second cutoff time, see view)
                new object[] {"9:03:07", "IBM", 103.0, "9:03:00"}, // this event should count for the next bucket
                new object[] {"9:03:55", "NOC", 12.5, "9:03:55"},
                new object[] {"9:03:58", "NOC", 12.75, "9:03:58"},
                new object[] {"9:04:00", "IBM", 104.0, "9:03:59"},
                new object[] {"9:04:02", "IBM", 105.0, "9:04:00"}, // next bucket starts with this event
                new object[] {"9:04:07", null}, // should complete next bucket even though there is no event arriving
                new object[] {"9:04:30", null}, // pretend no events
                new object[] {"9:04:59", null},
                new object[] {"9:05:00", null},
                new object[] {"9:05:10", null},
                new object[] {"9:05:15", "IBM", 105.5, "9:05:13"},
                new object[] {"9:05:59", null},
                new object[] {"9:06:07", null},
            };

            for (var i = 0; i < input.Length; i++) {
                var timestampArrival = (string) input[i][0];
                Log.Info("Sending timer event " + timestampArrival);
                SendTimer(runtime, ToTime(timestampArrival));

                var ticker = (string) input[i][1];
                if (ticker != null) {
                    var price = input[i][2].AsDouble();
                    var timestampTick = (string) input[i][3];
                    var theEvent = new OHLCTick(ticker, price, ToTime(timestampTick));

                    Log.Info("Sending event " + theEvent);
                    runtime.EventService.SendEventBean(theEvent, "OHLCTick");
                }
            }
        }

        private EPStatement CompileDeploy(
            string expression,
            Configuration config,
            EPRuntime runtime)
        {
            try {
                var args = new CompilerArguments(config);
                var compiled = EPCompilerProvider.Compiler.Compile(expression, args);
                var deployment = runtime.DeploymentService.Deploy(compiled);

                // EPL modules can have multiple statements
                // We return the first statement here.
                return deployment.Statements[0];
            }
            catch (Exception ex) {
                throw new EPRuntimeException(ex);
            }
        }

        private static void SendTimer(
            EPRuntime runtime,
            long timestamp)
        {
            runtime.EventService.AdvanceTime(timestamp);
        }

        private static long ToTime(string time)
        {
            var fields = time.Split(":");
            var hour = int.Parse(fields[0]);
            var min = int.Parse(fields[1]);
            var sec = int.Parse(fields[2]);

            var dateTimeEx = DateTimeEx.NowUtc();
            dateTimeEx.Set(2008, 1, 1, hour, min, sec, 0);
            return dateTimeEx.UtcMillis;
        }
    }
}
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.Transaction.sim
{
    public class TxnGenMain : IRunnable
    {
        private static readonly IDictionary<string, int> BUCKET_SIZES = new LinkedHashMap<string, int>();

        static TxnGenMain()
        {
            BUCKET_SIZES.Put("tiniest", 20);
            BUCKET_SIZES.Put("tiny", 499);
            BUCKET_SIZES.Put("small", 4999);
            BUCKET_SIZES.Put("medium", 14983);
            BUCKET_SIZES.Put("large", 49999);
            BUCKET_SIZES.Put("larger", 1999993);
            BUCKET_SIZES.Put("largerer", 9999991);
        }
        
        /// <summary>
        /// Application entry point
        /// </summary>
        /// <param name="args"></param>

        public static void Main(string[] args)
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();

            if (args.Length < 2)
            {
                Console.Out.WriteLine("Arguments are: <bucket_size> <num_transactions>");
                Environment.Exit(-1);
            }

            int? bucketSize;
            try
            {
                bucketSize = BUCKET_SIZES.Get(args[0]);
            }
            catch (NullReferenceException)
            {
                Console.Out.WriteLine("Invalid bucket size:");
                foreach (string key in BUCKET_SIZES.Keys)
                {
                    Console.Out.WriteLine("\t" + key + " -> " + BUCKET_SIZES.Get(key));
                }

                Environment.Exit(-2);
                return;
            }

            int numTransactions;
            try
            {
                numTransactions = int.Parse(args[1]);
            }
            catch (FormatException)
            {
                Console.Out.WriteLine("Invalid num transactions");
                Environment.Exit(-2);
                return;
            }

            // Run the sample
            Console.Out.WriteLine("Using bucket size of " + bucketSize + " with " + numTransactions + " transactions");
            TxnGenMain txnGenMain = new TxnGenMain(bucketSize.Value, numTransactions, "TransactionExample", false);
            txnGenMain.Run();
        }

        private readonly int _bucketSize;
        private readonly int _numTransactions;
        private readonly string _engineURI; 
        private readonly bool _continuousSimulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TxnGenMain"/> class.
        /// </summary>
        /// <param name="bucketSize">Size of the bucket.</param>
        /// <param name="numTransactions">The num transactions.</param>
        /// <param name="engineURI">The engine URI.</param>
        /// <param name="continuousSimulation">if set to <c>true</c> [continuous simulation].</param>
        public TxnGenMain(int bucketSize, int numTransactions, string engineURI, bool continuousSimulation)
        {
            _bucketSize = bucketSize;
            _numTransactions = numTransactions;
            _engineURI = engineURI;
            _continuousSimulation = continuousSimulation;
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            var container = ContainerExtensions.CreateDefaultContainer();

            // Configure engine with event names to make the statements more readable.
            // This could also be done in a configuration file.
            var configuration = new Configuration(container);
            configuration.Common.AddEventType("TxnEventA", typeof(TxnEventA));
            configuration.Common.AddEventType("TxnEventB", typeof(TxnEventB));
            configuration.Common.AddEventType("TxnEventC", typeof(TxnEventC));

            // Get engine instance
            var runtime = EPRuntimeProvider.GetRuntime(_engineURI, configuration);

            // We will be supplying timer events externally.
            // We will assume that each bucket arrives within a defined period of time.
            runtime.EventService.ClockExternal();

            // Set up statement for listening to combined events
            var combinedEventStmt = CombinedEventStmt.Create(runtime);
            combinedEventStmt.Events += LogCombinedEvents;

            // Set up statements for realtime summary latency data - overall totals and totals per customer and per supplier
            RealtimeSummaryStmt realtimeSummaryStmt = new RealtimeSummaryStmt(runtime);
            realtimeSummaryStmt.TotalsStatement.Events += LogSummaryTotals;
            realtimeSummaryStmt.CustomerStatement.Events +=
                (sender, e) => LogSummaryGroup("customerId", e);
            realtimeSummaryStmt.SupplierStatement.Events +=
                (sender, e) => LogSummaryGroup("supplierId", e);

            // Set up statement for finding missing events
            var findMissingEventStmt = FindMissingEventStmt.Create(runtime);
            findMissingEventStmt.Events += FindMissingEvent;

            // The feeder to feed the engine
            var feeder = new FeederOutputStream(runtime);

            // Generate transactions
            var source = new TransactionEventSource(_numTransactions);
            var output = new ShuffledBucketOutput(source, feeder, _bucketSize);

            // Feed events
            if (_continuousSimulation)
            {
                while (true)
                {
                    output.Output();
                    Thread.Sleep(5000); // Send a batch every 5 seconds
                }
            }
            else
            {
                output.Output();
            }
        }

        public void FindMissingEvent(object sender, UpdateEventArgs e)
        {
            var oldEvents = e.OldEvents;
            if (oldEvents == null)
            {
                // we don't care about events entering the window (new events)
                // this is because we must wait for C to arri
                return;
            }

            // Missing C events can be reported either through A or through B
            // We assume that duplicates are ok, if not, then streams A and B could be joined and then fed,
            // or duplicates could be removed via another statement as well.
            TxnEventA eventA = (TxnEventA)oldEvents[0]["A"];
            TxnEventB eventB = (TxnEventB)oldEvents[0]["B"];

            if (eventA != null)
            {
                Log.Debug("Missing TxnEventC event detected for TxnEventA " + eventA);
            }
            else
            {
                Log.Debug("Missing TxnEventC event detected for TxnEventB " + eventB);
            }
        }

        public static void LogCombinedEvents(object sender, UpdateEventArgs e)
        {
            if (e.NewEvents == null)
            {
                // we don't care about events leaving the window (old events)
                return;
            }

            EventBean eventBean = e.NewEvents[0];
            Log.Debug("Combined event detected " +
                    " transactionId=" + eventBean["transactionId"] +
                    " customerId=" + eventBean["customerId"] +
                    " supplierId=" + eventBean["supplierId"] +
                    " latencyAC=" + eventBean["latencyAC"] +
                    " latencyAB=" + eventBean["latencyAB"] +
                    " latencyBC=" + eventBean["latencyBC"]
                    );
        }

        public void LogSummaryTotals(object sender, UpdateEventArgs e)
        {
            if (e.NewEvents == null)
            {
                // we don't care about events leaving the window (old events)            
                return;
            }

            EventBean eventBean = e.NewEvents[0];
            Log.Debug(
                    " Totals minAC=" + eventBean["minLatencyAC"] +
                    " maxAC=" + eventBean["maxLatencyAC"] +
                    " avgAC=" + eventBean["avgLatencyAC"] +
                    " minAB=" + eventBean["minLatencyAB"] +
                    " maxAB=" + eventBean["maxLatencyAB"] +
                    " avgAB=" + eventBean["avgLatencyAB"] +
                    " minBC=" + eventBean["minLatencyBC"] +
                    " maxBC=" + eventBean["maxLatencyBC"] +
                    " avgBC=" + eventBean["avgLatencyBC"]
                    );
        }

        public void LogSummaryGroup(string groupIdentifier, UpdateEventArgs e)
        {
            if (e.NewEvents == null)
            {
                // we don't care about events leaving the window (old events)
                return;
            }

            EventBean eventBean = e.NewEvents[0];
            Log.Debug(
                    groupIdentifier + "=" + eventBean[groupIdentifier] +
                    " minAC=" + eventBean["minLatency"] +
                    " maxAC=" + eventBean["maxLatency"] +
                    " avgAC=" + eventBean["avgLatency"]
                    );
        }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}

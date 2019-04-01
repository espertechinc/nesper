///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logger;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;

namespace NEsper.Examples.MarketDataFeed
{
    public class FeedSimMain
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(String[] args)
        {
            LoggerNLog.BasicConfig();
            LoggerNLog.Register();
            
            if (args.Length < 3)
            {
                Console.WriteLine(
                    "Arguments are: <number of threads> <drop probability percent> <number of seconds to run>");
                Console.WriteLine("  number of threads: the number of threads sending feed events into the engine");
                Console.WriteLine("  drop probability percent: a number between zero and 100 that dictates the ");
                Console.WriteLine("                            probability that per second one of the feeds drops off");
                Console.WriteLine("  number of seconds: the number of seconds the simulation runs");
                Environment.Exit(-1);
            }

            int numberOfThreads;
            try
            {
                numberOfThreads = Int32.Parse(args[0]);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid number of threads: " + args[0]);
                Environment.Exit(-2);
                return;
            }

            double dropProbability;
            try
            {
                dropProbability = Double.Parse(args[1]);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid drop probability:" + args[1]);
                Environment.Exit(-2);
                return;
            }

            int numberOfSeconds;
            try
            {
                numberOfSeconds = Int32.Parse(args[2]);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid number of seconds to run:" + args[2]);
                Environment.Exit(-2);
                return;
            }

            // Run the sample
            Console.WriteLine("Using " + numberOfThreads + " threads with a drop probability of " + dropProbability +
                              "%, for " + numberOfSeconds + " seconds");
            var feedSimMain = new FeedSimMain(numberOfThreads, dropProbability, numberOfSeconds, true, "FeedSimMain");
            feedSimMain.Run();
        }

        private readonly int _numberOfThreads;
        private readonly double _dropProbability;
        private readonly int _numSeconds;
        private readonly bool _isWaitKeypress;
        private readonly String _engineURI;

        public FeedSimMain(int numberOfThreads, double dropProbability, int numSeconds, bool isWaitKeypress, string engineURI)
        {
            _numberOfThreads = numberOfThreads;
            _dropProbability = dropProbability;
            _numSeconds = numSeconds;
            _isWaitKeypress = isWaitKeypress;
            _engineURI = engineURI;
        }

        public void Run()
        {
            if (_isWaitKeypress)
            {
                Console.WriteLine("...press enter to start simulation...");
                Console.ReadKey();
            }

            var container = ContainerExtensions.CreateDefaultContainer()
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            // Configure engine with event names to make the statements more readable.
            // This could also be done in a configuration file.
            var configuration = new Configuration(container);
            configuration.AddEventType("MarketDataEvent", typeof(MarketDataEvent).FullName);

            // Get engine instance
            var epService = EPServiceProviderManager.GetProvider(container, _engineURI, configuration);

            // Set up statements
            var tickPerSecStmt = TicksPerSecondStatement.Create(epService.EPAdministrator);
            tickPerSecStmt.Events += LogRate;

            var falloffStmt = TicksFalloffStatement.Create(epService.EPAdministrator);
            falloffStmt.Events += MonitorRate;

            // Send events
            var threadPool = new DedicatedExecutorService(string.Empty, _numberOfThreads);
            var runnables = new MarketDataSendRunnable[_numberOfThreads];
            for (var i = 0; i < _numberOfThreads; i++)
            {
                runnables[i] = new MarketDataSendRunnable(epService);
                threadPool.Submit(runnables[i].Run);
            }

            var seconds = 0;
            var random = new Random();
            while (seconds < _numSeconds)
            {
                seconds++;
                Thread.Sleep(1000);

                FeedEnum? feedToDropOff;
                if (random.NextDouble() * 100 < _dropProbability)
                {
                    feedToDropOff = FeedEnum.FEED_A;
                    if (random.Next(0, 2) == 1)
                    {
                        feedToDropOff = FeedEnum.FEED_B;
                    }
                    Log.Info("Setting drop-off for feed {0}", feedToDropOff);
                }
                else
                {
                    feedToDropOff = null;
                }
                
                foreach (var t in runnables) 
                {
                    t.SetRateDropOffFeed(feedToDropOff);
                }
            }

            Log.Info("Shutting down threadpool");
            for (var i = 0; i < runnables.Length; i++)
            {
                runnables[i].SetShutdown();
            }
            threadPool.Shutdown();
            threadPool.AwaitTermination(new TimeSpan(0, 0, 0, 10));
        }

        public static void MonitorRate(Object sender, UpdateEventArgs e)
        {
            if (e.NewEvents == null)
            {
                return; // ignore old events for events leaving the window
            }

            var eventBean = e.NewEvents[0];
            Log.Info("Rate fall-off detected for feed={0} and rate={1} and average={2}",
                     eventBean["Feed"],
                     eventBean["FeedCnt"],
                     eventBean["AvgCnt"]);
        }



        public static void LogRate(Object sender, UpdateEventArgs e)
        {
            var newEvents = e.NewEvents;
            if (newEvents.Length > 0)
            {
                LogRate(newEvents[0]);
            }
            if (newEvents.Length > 1)
            {
                LogRate(newEvents[1]);
            }
        }

        private static void LogRate(EventBean eventBean)
        {
            Log.Info("Current rate for feed {0} is {1}",
                     eventBean["feed"],
                     eventBean["cnt"]);
        }
    }
}

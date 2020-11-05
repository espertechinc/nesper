///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
using System.Threading;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.StockTicker.monitor;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.StockTicker
{
    public class StockTickerMain : IRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly bool _continuousSimulation;

        private readonly string _engineURI;

        public StockTickerMain(string engineURI,
            bool continuousSimulation)
        {
            _engineURI = engineURI;
            _continuousSimulation = continuousSimulation;
        }

        public void Run()
        {
            var container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.Common.AddEventType("PriceLimit", typeof(PriceLimit));
            configuration.Common.AddEventType("StockTick", typeof(StockTick));

            Log.Info("Setting up EPL");

            var runtime = EPRuntimeProvider.GetRuntime(_engineURI, configuration);
            runtime.Initialize();

            var eventService = runtime.EventService;
            
            new StockTickerMonitor(runtime, new StockTickerResultListener());

            Log.Info("Generating test events: 1 million ticks, ratio 2 hits, 100 stocks");
            var generator = new StockTickerEventGenerator();
            var stream = generator.MakeEventStream(1000000, 500000, 100, 25, 30, 48, 52, false);
            Log.Info("Generating " + stream.Count + " events");

            Log.Info("Sending " + stream.Count + " limit and tick events");
            foreach (var @event in stream) {
                eventService.SendEventBean(@event, @event.GetType().Name);

                if (_continuousSimulation) {
                    try {
                        Thread.Sleep(200);
                    }
                    catch (ThreadInterruptedException e) {
                        Log.Debug("Interrupted", e);
                        break;
                    }
                }
            }

            Log.Info("Done.");
        }
    }
}
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
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;

using NEsper.Examples.StockTicker.eventbean;
using NEsper.Examples.StockTicker.monitor;

namespace NEsper.Examples.StockTicker
{
    public class StockTickerMain : IRunnable
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly String _engineURI;
        private readonly bool _continuousSimulation;
        
        public StockTickerMain(String engineURI, bool continuousSimulation) {
            _engineURI = engineURI;
            _continuousSimulation = continuousSimulation;
        }
    
        public void Run() {
            var container = ContainerExtensions.CreateDefaultContainer(false)
                .InitializeDefaultServices()
                .InitializeDatabaseDrivers();

            var configuration = new Configuration(container);
            configuration.AddEventType("PriceLimit", typeof(PriceLimit).FullName);
            configuration.AddEventType("StockTick", typeof(StockTick).FullName);
    
            Log.Info("Setting up EPL");
            
            var epService = EPServiceProviderManager.GetProvider(
                container, _engineURI, configuration);
            epService.Initialize();

            new StockTickerMonitor(epService, new StockTickerResultListener());
    
            Log.Info("Generating test events: 1 million ticks, ratio 2 hits, 100 stocks");
            var generator = new StockTickerEventGenerator();
            var stream = generator.MakeEventStream(1000000, 500000, 100, 25, 30, 48, 52, false);
            Log.Info("Generating " + stream.Count + " events");
    
            Log.Info("Sending " + stream.Count + " limit and tick events");
            foreach (var @event in stream)
            {
                epService.EPRuntime.SendEvent(@event);
    
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

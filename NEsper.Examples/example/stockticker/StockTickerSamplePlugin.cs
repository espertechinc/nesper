///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.plugin;

namespace NEsper.Examples.StockTicker
{
    public class StockTickerSamplePlugin : PluginLoader
    {
        private const string RUNTIME_URI = "engineURI";
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private string _runtimeURI;
        private StockTickerMain _main;
        private Thread _simulationThread;

        public void Init(PluginLoaderInitContext context)
        {
            if (context.Properties.Get(RUNTIME_URI) != null) {
                _runtimeURI = context.Properties.Get(RUNTIME_URI);
            }
            else {
                _runtimeURI = context.Runtime.URI;
            }
        }

        public void PostInitialize()
        {
            Log.Info("Starting StockTicker-example for engine URI '" + _runtimeURI + "'.");

            try {
                _main = new StockTickerMain(_runtimeURI, true);
                _simulationThread = new Thread(() => _main.Run());
                _simulationThread.Name = GetType().FullName + "-simulator";
                _simulationThread.IsBackground = true;
                _simulationThread.Start();
                _main.Run();
            }
            catch (Exception e) {
                Log.Error("Error starting StockTicker example: " + e.Message);
            }

            Log.Info("StockTicker-example started.");
        }

        public void Dispose()
        {
            if (_main != null) { 
                EPRuntimeProvider.GetRuntime(_runtimeURI)
                    .DeploymentService
                    .UndeployAll();
            }

            try {
                _simulationThread.Interrupt();
                _simulationThread.Join();
            }
            catch (ThreadInterruptedException e) {
                Log.Info("Interrupted", e);
            }

            _main = null;
            Log.Info("StockTicker-example stopped.");
        }
    }
}
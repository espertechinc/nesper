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
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.plugin;

using NEsper.Examples.Transaction.sim;

namespace NEsper.Examples.Transaction
{
    public class TransactionSamplePlugin : PluginLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private const String ENGINE_URI = "engineURI";
    
        private String _engineURI;
        private TxnGenMain _main;
        private Thread _simulationThread;
    
        public void Init(PluginLoaderInitContext context)
        {
            if (context.Properties.Get(ENGINE_URI) != null)
            {
                _engineURI = context.Properties.Get(ENGINE_URI);
            }
            else
            {
                _engineURI = context.EpServiceProvider.URI;
            }
        }
    
        public void PostInitialize()
        {
            Log.Info("Starting Transaction-example for engine URI '" + _engineURI + "'.");
    
            try {
                _main = new TxnGenMain(20, 200, _engineURI, true);
                _simulationThread = new Thread(() => _main.Run());
                _simulationThread.Name = GetType().FullName + "-simulator";
                _simulationThread.IsBackground = true;
                _simulationThread.Start();
            }
            catch (Exception e) {
                Log.Error("Error starting Transaction example: " + e.Message);
            }
    
            Log.Info("Transaction-example started.");
        }
    
        public void Dispose()
        {
            if (_main != null) {
                EPServiceProviderManager.GetProvider(_engineURI).EPAdministrator.DestroyAllStatements();
            }
    
            try {
                _simulationThread.Interrupt();
                _simulationThread.Join();
            }
            catch (ThreadInterruptedException e) {
                Log.Info("Interrupted", e);
            }
            _main = null;
            Log.Info("Transaction-example stopped.");
        }
    }
}

using System;

using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.plugin;

namespace NEsper.Examples.OHLC
{
    public class OHLCSamplePlugin : PluginLoader
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OHLCSamplePlugin));

        private const string RuntimeUri = "runtimeURI";

        private string _runtimeUri;
        private OHLCProgram _main;

        public void Init(PluginLoaderInitContext context)
        {
            if (!context.Properties.TryGetValue(RuntimeUri, out _runtimeUri)) {
                _runtimeUri = context.Runtime.URI;
            }
        }

        public void PostInitialize()
        {
            Log.Info("Starting OHLCSample-example for runtime URI '" + _runtimeUri + "'.");

            try {
                _main = new OHLCProgram();
                _main.Run(_runtimeUri);
            }
            catch (Exception e) {
                Log.Error("Error starting OHLCSample example: " + e.Message);
            }

            Log.Info("OHLCSample-example started.");
        }

        public void Dispose()
        {
            if (_main != null) {
                try {
                    EPRuntimeProvider.GetRuntime(_runtimeUri).DeploymentService.UndeployAll();
                }
                catch (EPUndeployException e) {
                    Log.Warn("Failed to undeploy: " + e.Message, e);
                }
            }

            Log.Info("OHLCSample-example stopped.");
        }
    }
}

///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client.util;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compiler.client;
using com.espertech.esper.container;
using com.espertech.esper.runtime.client;

using Configuration = com.espertech.esper.common.client.configuration.Configuration;

namespace NEsper.Examples.VirtualDW
{
    public class SampleVirtualDataWindowMain
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static void Main(String[] args)
        {
            var sample = new SampleVirtualDataWindowMain();
            try
            {
                sample.Run();
            }
            catch (Exception ex)
            {
                Log.Error("Unexpected exception :" + ex.Message, ex);
            }
        }
    
        public void Run()
        {
            Log.Info("Setting up engine instance.");

            var container = ContainerExtensions.CreateDefaultContainer();

            var config = new Configuration(container);
            config.Common.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
            config.Compiler.AddPlugInVirtualDataWindow("sample", "samplevdw", typeof(SampleVirtualDataWindowFactory).FullName);
            config.Common.AddEventType(typeof(SampleTriggerEvent));
            config.Common.AddEventType(typeof(SampleJoinEvent));
            config.Common.AddEventType(typeof(SampleMergeEvent));
    
            var runtime = EPRuntimeProvider.GetRuntime("LargeExternalDataExample", config);
    
            // First: Create an event type for rows of the external data - here the example use a Map-based event and any of the other types (PONO, XML) can be used as well.
            // Populate event property names and types.
            // Note: the type must match the data returned by virtual data window indexes.
            CompileDeploy(runtime, "create schema SampleEvent as (key1 string, key2 string, value1 int, value2 double)");
    
            Log.Info("Creating named window with virtual.");
    
            // Create Named Window holding SampleEvent instances
            CompileDeploy(runtime, "create window MySampleWindow.sample:samplevdw() as SampleEvent");
    
            // Example subquery
            Log.Info("Running subquery example.");
            RunSubquerySample(runtime);
    
            // Example joins
            Log.Info("Running join example.");
            RunJoinSample(runtime);
    
            // Sample FAF
            Log.Info("Running fire-and-forget query example.");
            RunSampleFireAndForgetQuery(runtime);
    
            // Sample On-Merge
            Log.Info("Running on-merge example.");
            RunSampleOnMerge(runtime);
    
            // Cleanup
            Log.Info("Destroying engine instance, sample completed successfully.");
            runtime.Destroy();
        }
    
        private void RunSubquerySample(EPRuntime runtime) {
    
            var epl = "select (select key2 from MySampleWindow where key1 = ste.triggerKey) as key2 from SampleTriggerEvent ste";
            var stmt = CompileDeploy(runtime, epl);
            var sampleListener = new SampleUpdateListener();
            stmt.Events += sampleListener.Update;
    
            runtime.EventService.SendEventBean(new SampleTriggerEvent("sample1"), "SampleTriggerEvent");
            Log.Info("Subquery returned: " + sampleListener.LastEvent.Get("key2"));
            // For assertions against expected results please see the regression test suite
        }
    
        private void RunJoinSample(EPRuntime runtime) {
            var epl = "select sw.* " +
                    "from SampleJoinEvent#lastevent() sje, MySampleWindow sw " +
                    "where sw.Key1 = sje.propOne and sw.Key2 = sje.propTwo";
            EPStatement stmt = CompileDeploy(runtime, epl);
            var sampleListener = new SampleUpdateListener();
            stmt.Events += sampleListener.Update;
    
            runtime.EventService.SendEventBean(new SampleJoinEvent("sample1", "sample2"), "SampleJoinEvent"); // see values in SampleVirtualDataWindowIndex
            Log.Info("Join query returned: " + sampleListener.LastEvent.Get("key1") + " and " + sampleListener.LastEvent.Get("key2"));
    
            // For assertions against expected results please see the regression test suite
        }
    
        private void RunSampleFireAndForgetQuery(EPRuntime runtime) {
            var fireAndForget = "select * from MySampleWindow where key1 = 'sample1' and key2 = 'sample2'"; // see values in SampleVirtualDataWindowIndex
            var compilerArgs = new CompilerArguments();
            compilerArgs.Path.Add(runtime.RuntimePath);

            var compiled = EPCompilerProvider.Compiler.CompileQuery(fireAndForget, compilerArgs);
            var result = runtime.FireAndForgetService.ExecuteQuery(compiled);
    
            Log.Info("Fire-and-forget query returned: " + result.Array[0].Get("key1") + " and " + result.Array[0].Get("key2"));
    
            // For assertions against expected results please see the regression test suite
        }
    
        private void RunSampleOnMerge(EPRuntime runtime) {
    
            var onDelete =
                    "on SampleMergeEvent " +
                    "merge MySampleWindow " +
                    "where key1 = propOne " +
                    "when not matched then insert select propOne as key1, propTwo as key2, 0 as value1, 0d as value2 " +
                    "when matched then update set key2 = propTwo";
    
            var stmt = CompileDeploy(runtime, onDelete);
            var sampleListener = new SampleUpdateListener();
            stmt.Events += sampleListener.Update;
    
            // not-matching case
            runtime.EventService.SendEventBean(new SampleMergeEvent("mykey-sample", "hello"), "SampleMergeEvent");
            Log.Info("Received inserted key: " + sampleListener.LastEvent.Get("key1") + " and " + sampleListener.LastEvent.Get("key2"));
    
            // matching case
            runtime.EventService.SendEventBean(new SampleMergeEvent("sample1", "newvalue"), "SampleMergeEvent");  // see values in SampleVirtualDataWindowIndex
            Log.Info("Received updated key: " + sampleListener.LastEvent.Get("key1") + " and " + sampleListener.LastEvent.Get("key2"));
    
            // For assertions against expected results please see the regression test suite
        }
        
        private EPStatement CompileDeploy(
            EPRuntime runtime,
            string epl)
        {
            var args = new CompilerArguments();
            args.Path.Add(runtime.RuntimePath);
            args.Options.AccessModifierNamedWindow = env => NameAccessModifier.PUBLIC;
            args.Configuration.Compiler.ByteCode.AllowSubscriber = true;

            var compiled = EPCompilerProvider.Compiler.Compile(epl, args);
            var deployment = runtime.DeploymentService.Deploy(compiled);
            return deployment.Statements[0];
        }
    }
}

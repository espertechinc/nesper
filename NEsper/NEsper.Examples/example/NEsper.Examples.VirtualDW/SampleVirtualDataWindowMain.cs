///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.client;
using com.espertech.esper.client.util;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;

namespace NEsper.Examples.VirtualDW
{
    public class SampleVirtualDataWindowMain
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public static void Main(String[] args)
        {
            SampleVirtualDataWindowMain sample = new SampleVirtualDataWindowMain();
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

            Configuration config = new Configuration(container);
            config.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
            config.AddPlugInVirtualDataWindow("sample", "samplevdw", typeof(SampleVirtualDataWindowFactory).FullName);
            config.AddEventTypeAutoName(typeof(SampleVirtualDataWindowMain).Namespace);    // import all event classes
    
            EPServiceProvider epService = EPServiceProviderManager.GetProvider(container, "LargeExternalDataExample", config);
    
            // First: Create an event type for rows of the external data - here the example use a Map-based event and any of the other types (POJO, XML) can be used as well.
            // Populate event property names and types.
            // Note: the type must match the data returned by virtual data window indexes.
            epService.EPAdministrator.CreateEPL("create schema SampleEvent as (key1 string, key2 string, value1 int, value2 double)");
    
            Log.Info("Creating named window with virtual.");
    
            // Create Named Window holding SampleEvent instances
            epService.EPAdministrator.CreateEPL("create window MySampleWindow.sample:samplevdw() as SampleEvent");
    
            // Example subquery
            Log.Info("Running subquery example.");
            RunSubquerySample(epService);
    
            // Example joins
            Log.Info("Running join example.");
            RunJoinSample(epService);
    
            // Sample FAF
            Log.Info("Running fire-and-forget query example.");
            RunSampleFireAndForgetQuery(epService);
    
            // Sample On-Merge
            Log.Info("Running on-merge example.");
            RunSampleOnMerge(epService);
    
            // Cleanup
            Log.Info("Destroying engine instance, sample completed successfully.");
            epService.Dispose();
        }
    
        private void RunSubquerySample(EPServiceProvider epService) {
    
            String epl = "select (select key2 from MySampleWindow where key1 = ste.triggerKey) as key2 from SampleTriggerEvent ste";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            SampleUpdateListener sampleListener = new SampleUpdateListener();
            stmt.Events += sampleListener.Update;
    
            epService.EPRuntime.SendEvent(new SampleTriggerEvent("sample1"));
            Log.Info("Subquery returned: " + sampleListener.LastEvent.Get("key2"));
            // For assertions against expected results please see the regression test suite
        }
    
        private void RunJoinSample(EPServiceProvider epService) {
            String epl = "select sw.* " +
                    "from SampleJoinEvent#lastevent() sje, MySampleWindow sw " +
                    "where sw.key1 = sje.propOne and sw.key2 = sje.propTwo";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(epl);
            SampleUpdateListener sampleListener = new SampleUpdateListener();
            stmt.Events += sampleListener.Update;
    
            epService.EPRuntime.SendEvent(new SampleJoinEvent("sample1", "sample2")); // see values in SampleVirtualDataWindowIndex
            Log.Info("Join query returned: " + sampleListener.LastEvent.Get("key1") + " and " + sampleListener.LastEvent.Get("key2"));
    
            // For assertions against expected results please see the regression test suite
        }
    
        private void RunSampleFireAndForgetQuery(EPServiceProvider epService) {
            String fireAndForget = "select * from MySampleWindow where key1 = 'sample1' and key2 = 'sample2'"; // see values in SampleVirtualDataWindowIndex
            EPOnDemandQueryResult result = epService.EPRuntime.ExecuteQuery(fireAndForget);
    
            Log.Info("Fire-and-forget query returned: " + result.Array[0].Get("key1") + " and " + result.Array[0].Get("key2"));
    
            // For assertions against expected results please see the regression test suite
        }
    
        private void RunSampleOnMerge(EPServiceProvider epService) {
    
            String onDelete =
                    "on SampleMergeEvent " +
                    "merge MySampleWindow " +
                    "where key1 = propOne " +
                    "when not matched then insert select propOne as key1, propTwo as key2, 0 as value1, 0d as value2 " +
                    "when matched then update set key2 = propTwo";
    
            EPStatement stmt = epService.EPAdministrator.CreateEPL(onDelete);
            SampleUpdateListener sampleListener = new SampleUpdateListener();
            stmt.Events += sampleListener.Update;
    
            // not-matching case
            epService.EPRuntime.SendEvent(new SampleMergeEvent("mykey-sample", "hello"));
            Log.Info("Received inserted key: " + sampleListener.LastEvent.Get("key1") + " and " + sampleListener.LastEvent.Get("key2"));
    
            // matching case
            epService.EPRuntime.SendEvent(new SampleMergeEvent("sample1", "newvalue"));  // see values in SampleVirtualDataWindowIndex
            Log.Info("Received updated key: " + sampleListener.LastEvent.Get("key1") + " and " + sampleListener.LastEvent.Get("key2"));
    
            // For assertions against expected results please see the regression test suite
        }
    }
    
}

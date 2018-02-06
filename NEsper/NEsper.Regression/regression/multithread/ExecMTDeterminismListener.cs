///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    public class ExecMTDeterminismListener : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            TrySend(4, 10000, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND);
            TrySend(4, 10000, true, ConfigurationEngineDefaults.ThreadingConfig.Locking.SPIN);
        }
    
        public void ManualTestOrderedDeliveryFail() {
            // Commented out as this is a manual test -- it should fail since the disable preserve order.
            TrySend(3, 1000, false, null);
        }
    
        private void TrySend(int numThreads, int numEvents, bool isPreserveOrder, ConfigurationEngineDefaults.ThreadingConfig.Locking? locking) {
            Configuration config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.Threading.IsListenerDispatchPreserveOrder = isPreserveOrder;
            config.EngineDefaults.Threading.ListenerDispatchLocking = locking.GetValueOrDefault();
    
            EPServiceProvider engine = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, this.GetType().Name, config);
            engine.Initialize();
    
            // setup statements
            EPStatement stmtInsert = engine.EPAdministrator.CreateEPL("select count(*) as cnt from " + typeof(SupportBean).FullName);
            var listener = new SupportMTUpdateListener();
            stmtInsert.Events += listener.Update;
    
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                future[i] = threadPool.Submit(new SendEventCallable(i, engine, new GeneratorIterator(numEvents)));
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
    
            EventBean[] events = listener.GetNewDataListFlattened();
            var result = new long[events.Length];
            for (int i = 0; i < events.Length; i++) {
                result[i] = (long) events[i].Get("cnt");
            }
            //Log.Info(".trySend result=" + CompatExtensions.Render(result));
    
            // assert result
            Assert.AreEqual(numEvents * numThreads, events.Length);
            for (int i = 0; i < numEvents * numThreads; i++) {
                Assert.AreEqual(result[i], (long) i + 1);
            }
    
            engine.Dispose();
        }
    }
} // end of namespace

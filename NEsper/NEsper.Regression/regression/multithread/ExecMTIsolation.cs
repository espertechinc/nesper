///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety and deterministic behavior when using insert-into.
    /// </summary>
    public class ExecMTIsolation : RegressionExecution
    {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.ViewResources.IsShareViews = false;
            configuration.EngineDefaults.Execution.IsAllowIsolatedService = true;
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryIsolated(epService, 2, 500);
        }
    
        private void TryIsolated(EPServiceProvider engine, int numThreads, int numLoops) {
            // execute
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var sharedStartLock = SupportContainer.Instance.RWLockManager().CreateDefaultLock();
            using (sharedStartLock.WriteLock.Acquire())
            {
                for (int i = 0; i < numThreads; i++)
                {
                    future[i] = threadPool.Submit(new IsolateUnisolateCallable(i, engine, numLoops));
                }

                Thread.Sleep(100);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numThreads; i++) {
                Assert.IsTrue(future[i].GetValueOrDefault());
            }
        }
    }
} // end of namespace

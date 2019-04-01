///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;


using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for pattern statement parallel execution by threads.</summary>
    public class ExecMTStmtPattern : RegressionExecution {
        public override void Run(EPServiceProvider epService) {
            string type = typeof(SupportBean).FullName;
    
            string pattern = "a=" + type;
            TryPattern(epService, pattern, 4, 20);
    
            pattern = "a=" + type + " or a=" + type;
            TryPattern(epService, pattern, 2, 20);
        }
    
        private void TryPattern(EPServiceProvider epService, string pattern, int numThreads, int numEvents) {
            var sendLock = new Object();
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            var callables = new SendEventWaitCallable[numThreads];
            for (int i = 0; i < numThreads; i++) {
                callables[i] = new SendEventWaitCallable(i, epService, sendLock, new GeneratorIterator(numEvents));
                future[i] = threadPool.Submit(callables[i]);
            }
    
            var listener = new SupportMTUpdateListener[numEvents];
            for (int i = 0; i < numEvents; i++) {
                EPStatement stmt = epService.EPAdministrator.CreatePattern(pattern);
                listener[i] = new SupportMTUpdateListener();
                stmt.Events += listener[i].Update;
    
                lock (sendLock) {
                    Monitor.PulseAll(sendLock);
                }
            }
    
            foreach (SendEventWaitCallable callable in callables) {
                callable.SetShutdown(true);
            }
            lock (sendLock) {
                Monitor.PulseAll(sendLock);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            for (int i = 0; i < numEvents; i++) {
                Assert.IsTrue(listener[i].AssertOneGetNewAndReset().Get("a") is SupportBean);
            }
        }
    }
} // end of namespace

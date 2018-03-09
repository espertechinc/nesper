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
using com.espertech.esper.client.util;
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
    /// <summary>Test for multithread-safety of context.</summary>
    public class ExecMTContextCountSimple : RegressionExecution {
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.EventMeta.DefaultEventRepresentation = EventUnderlyingType.MAP; // use Map-type events for testing
        }
    
        public override void Run(EPServiceProvider epService) {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create context HashByUserCtx as coalesce by Consistent_hash_crc32(TheString) from SupportBean granularity 10000000");
            epService.EPAdministrator.CreateEPL("@Name('select') context HashByUserCtx select TheString from SupportBean");
    
            TrySendContextCountSimple(epService, 4, 5);
        }
    
        private void TrySendContextCountSimple(EPServiceProvider epService, int numThreads, int numRepeats) {
            var listener = new SupportMTUpdateListener();
            epService.EPAdministrator.GetStatement("select").Events += listener.Update;
    
            var events = new List<object>();
            for (int i = 0; i < numRepeats; i++) {
                events.Add(new SupportBean("E" + i, i));
            }
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var future = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++) {
                var callable = new SendEventCallable(i, epService, events.GetEnumerator());
                future[i] = threadPool.Submit(callable);
            }
    
            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);
    
            EventBean[] result = listener.GetNewDataListFlattened();
            Assert.AreEqual(numRepeats * numThreads, result.Length);
        }
    }
} // end of namespace

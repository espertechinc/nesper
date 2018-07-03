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
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.epl;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety of context with database access.
    /// </summary>
    public class ExecMTContextDBAccess : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Logging.IsEnableADO = true;
            configuration.EngineDefaults.Threading.IsListenerDispatchPreserveOrder = false;

            var configDB = SupportDatabaseService.CreateDefaultConfig();
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configuration.AddDatabaseReference("MyDB", configDB);
        }

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create context CtxEachString partition by TheString from SupportBean");
            epService.EPAdministrator.CreateEPL(
                "@Name('select') context CtxEachString " +
                "select * from SupportBean, " +
                "  sql:MyDB ['select mycol3 from mytesttable_large where ${TheString} = mycol1']");

            // up to 10 threads, up to 1000 combinations (1 to 1000)
            TryThreadSafetyHistoricalJoin(epService, 8, 20);
        }

        private void TryThreadSafetyHistoricalJoin(EPServiceProvider epService, int numThreads, int numRepeats)
        {
            var listener = new MyListener();
            epService.EPAdministrator.GetStatement("select").Events += listener.Update;

            var events = new IList<object>[numThreads];
            for (int threadNum = 0; threadNum < numThreads; threadNum++)
            {
                events[threadNum] = new List<object>();
                for (int eventNum = 0; eventNum < numRepeats; eventNum++)
                {
                    // range: 1 to 1000
                    int partition = eventNum + 1;
                    events[threadNum].Add(new SupportBean(Convert.ToString(partition), 0));
                }
            }

            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var futures = new Future<bool>[numThreads];
            for (int i = 0; i < numThreads; i++)
            {
                var callable = new SendEventCallable(i, epService, events[i].GetEnumerator());
                futures[i] = threadPool.Submit(callable);
            }

            foreach (var future in futures)
            {
                Assert.AreEqual(true, future.GetValue(TimeSpan.FromSeconds(60)));
            }

            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);

            Assert.AreEqual(numRepeats * numThreads, listener.Count);
        }

        public class MyListener
        {
            private readonly ILockable _lock = SupportContainer.Instance.LockManager().CreateDefaultLock();

            public void Update(object sender, UpdateEventArgs args)
            {
                using (_lock.Acquire())
                {
                    if (args.NewEvents.Length > 1)
                    {
                        Assert.AreEqual(1, args.NewEvents.Length);
                    }

                    Count += 1;
                }
            }

            public int Count { get; private set; }
        }
    }
} // end of namespace

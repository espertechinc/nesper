///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.container;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.multithread;
using com.espertech.esper.supportregression.util;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>
    /// Test for multithread-safety of context with database access.
    /// </summary>
    public class ExecMTContextListenerDispatch : RegressionExecution
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            epService.EPAdministrator.CreateEPL("create context CtxEachString partition by TheString from SupportBean");
            epService.EPAdministrator.CreateEPL("@Name('select') context CtxEachString select * from SupportBean");

            TryPerformanceDispatch(epService, 8, 100);
        }

        private void TryPerformanceDispatch(EPServiceProvider epService, int numThreads, int numRepeats)
        {
            var listener = new MyListener();
            epService.EPAdministrator.GetStatement("select").Events += listener.Update;

            var random = new Random();
            var eventId = 0;
            var events = new IList<object>[numThreads];
            for (int threadNum = 0; threadNum < numThreads; threadNum++)
            {
                events[threadNum] = new List<object>();
                for (int eventNum = 0; eventNum < numRepeats; eventNum++)
                {
                    // range: 1 to 1000
                    int partition = random.Next(0, 51);
                    eventId++;
                    events[threadNum].Add(new SupportBean(partition.ToString(CultureInfo.InvariantCulture), eventId));
                }
            }

            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var futures = new Future<bool>[numThreads];
            var delta = PerformanceObserver.TimeMillis(
                () => {

                    for (int i = 0; i < numThreads; i++) {
                        var callable = new SendEventCallable(i, epService, events[i].GetEnumerator());
                        futures[i] = threadPool.Submit(callable);
                    }

                    foreach (var future in futures) {
                        Assert.AreEqual(true, future.GetValue(TimeSpan.FromSeconds(60)));
                    }
                });

            threadPool.Shutdown();
            threadPool.AwaitTermination(10, TimeUnit.SECONDS);

            // print those events not received
            foreach (var eventList in events)
            {
                foreach (var @event in eventList.Cast<SupportBean>())
                {
                    if (!listener.Beans.Contains(@event))
                    {
                        Log.Info("Expected event was not received, event " + @event);
                    }
                }
            }

            Assert.AreEqual(numRepeats * numThreads, listener.Beans.Count);
            Assert.IsTrue(delta < 500, "delta=" + delta);
        }

        public class MyListener
        {
            private readonly List<SupportBean> _beans = new List<SupportBean>();
            private readonly ILockable _lock = SupportContainer.Instance.LockManager().CreateDefaultLock();

            public void Update(Object sender, UpdateEventArgs args)
            {
                using (_lock.Acquire())
                {
                    if (args.NewEvents.Length > 1)
                    {
                        Assert.AreEqual(1, args.NewEvents.Length);
                    }

                    _beans.Add((SupportBean) args.NewEvents[0].Underlying);
                }
            }

            public List<SupportBean> Beans => _beans;
        }
    }
} // end of namespace

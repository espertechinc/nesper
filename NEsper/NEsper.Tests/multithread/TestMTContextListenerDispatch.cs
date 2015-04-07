///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety of context with database access. </summary>
    [TestFixture]
    public class TestMTContextListenerDispatch 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            EPServiceProviderManager.PurgeAllProviders();

            Configuration configuration = SupportConfigFactory.GetConfiguration();
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
        }
    
        [Test]
        public void TestPerformanceDispatch()
        {
            _engine.EPAdministrator.Configuration.AddEventType(typeof(SupportBean));
            _engine.EPAdministrator.CreateEPL("create context CtxEachString partition by TheString from SupportBean");
            _engine.EPAdministrator.CreateEPL("@Name('select') context CtxEachString select * from SupportBean");
    
            TryPerformanceDispatch(8, 100);
        }
    
        private void TryPerformanceDispatch(int numThreads, int numRepeats)
        {
            var listener = new MyListener();
            _engine.EPAdministrator.GetStatement("select").Events += listener.Update;

            var random = new Random();
            var events = new IList<object>[numThreads];
            for (int threadNum = 0; threadNum < numThreads; threadNum++) {
                events[threadNum] = new List<Object>();
                for (int eventNum = 0; eventNum < numRepeats; eventNum++)
                {
                    // range: 1 to 1000
                    int partition = random.Next(0, 51);
                    events[threadNum].Add(new SupportBean(partition.ToString(CultureInfo.InvariantCulture), 0));
                }
            }
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var futures = new Future<object>[numThreads];

            var delta = PerformanceObserver.TimeMillis(
                () =>
                {
                    for (int i = 0; i < numThreads; i++)
                    {
                        var callable = new SendEventCallable(i, _engine, events[i].GetEnumerator());
                        futures[i] = threadPool.Submit(callable);
                    }

                    threadPool.Shutdown();
                    threadPool.AwaitTermination(TimeSpan.FromSeconds(10));

                    foreach (var future in futures)
                    {
                        Assert.AreEqual(true, future.GetValueOrDefault());
                    }
                });

            Assert.That(listener.Count, Is.EqualTo(numRepeats*numThreads));
            Assert.That(delta, Is.LessThan(500), "delta=" + delta);
        }
    
        public class MyListener
        {
            private int _count;
    
            public void Update(Object sender, UpdateEventArgs args)
            {
                lock (this)
                {
                    if (args.NewEvents.Length > 1)
                    {
                        Assert.AreEqual(1, args.NewEvents.Length);
                    }
                    _count += 1;
                }
            }

            public int Count
            {
                get { return _count; }
            }
        }
    }
}

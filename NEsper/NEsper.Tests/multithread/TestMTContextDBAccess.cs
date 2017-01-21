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
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;
using com.espertech.esper.support.epl;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    /// <summary>Test for multithread-safety of context with database access. </summary>
    [TestFixture]
    public class TestMTContextDBAccess 
    {
        private EPServiceProvider _engine;
    
        [SetUp]
        public void SetUp()
        {
            Configuration configuration = SupportConfigFactory.GetConfiguration();
            configuration.EngineDefaults.LoggingConfig.IsEnableADO = true;
            configuration.EngineDefaults.ThreadingConfig.IsListenerDispatchPreserveOrder = false;

            var configDB = new ConfigurationDBRef();
            configDB.SetDatabaseDriver(SupportDatabaseService.DbDriverFactoryNative);
            configDB.ConnectionLifecycle = ConnectionLifecycleEnum.RETAIN;
            configuration.AddDatabaseReference("MyDB", configDB);
    
            _engine = EPServiceProviderManager.GetDefaultProvider(configuration);
            _engine.Initialize();
        }
    
        [Test]
        public void TestThreadSafetyHistoricalJoin()
        {
            _engine.EPAdministrator.Configuration.AddEventType<SupportBean>();
            _engine.EPAdministrator.CreateEPL("create context CtxEachString partition by TheString from SupportBean");
            _engine.EPAdministrator.CreateEPL("@Name('select') context CtxEachString " +
                    "select * from SupportBean, " +
                    "  sql:MyDB ['select mycol3 from mytesttable_large where ${TheString} = mycol1']");
    
            // up to 10 threads, up to 1000 combinations (1 to 1000)
            TryThreadSafetyHistoricalJoin(8, 20);
        }
    
        private void TryThreadSafetyHistoricalJoin(int numThreads, int numRepeats)
        {
            var listener = new MyListener();
            _engine.EPAdministrator.GetStatement("select").Events += listener.Update;
    
            var events = new IList<Object>[numThreads];
            for (var threadNum = 0; threadNum < numThreads; threadNum++) {
                events[threadNum] = new List<Object>();
                for (var eventNum = 0; eventNum < numRepeats; eventNum++) {
                    // range: 1 to 1000
                    var partition = eventNum + 1;
                    events[threadNum].Add(new SupportBean(partition.ToString(), 0));
                }
            }
    
            var threadPool = Executors.NewFixedThreadPool(numThreads);
            var futures = new Future<object>[numThreads];
            for (var i = 0; i < numThreads; i++)
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

            Assert.AreEqual(numRepeats * numThreads, listener.Count);
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

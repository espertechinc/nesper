///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.client.scopetest;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd 
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private const int NUM_KEYS = 10;
        private const int OFFSET_ADDED = 100000000;
    
        private EPServiceProvider epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType(typeof(SupportBean_S0));
            config.AddEventType(typeof(SupportBean_S1));
            epService = EPServiceProviderManager.GetDefaultProvider(config);
            epService.Initialize();
        }
    
        /// <summary>
        /// Tests concurrent updates on a secondary index also read by a join:
        /// create table MyTable (key string primary key, value int)
        /// create index MyIndex on MyTable (value)
        /// select * from SupportBean_S0, MyTable where intPrimitive = id
        /// Prefill MyTable with MyTable={key='A_N', value=Count} with Count between 0 and NUM_KEYS-1
        /// For x seconds:
        /// Single reader thread sends SupportBean events, asserts that either one or two rows are found (A_N and maybe B_N)
        /// Single writer thread inserts MyTable={key='B_N', value=100000+Count} and deletes each row.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(2);
        }
    
        private void TryMT(int numSeconds) 
        {
            var epl =
                    "create table MyTable (key1 string primary key, value int);\n" +
                    "create index MyIndex on MyTable (value);\n" +
                    "on SupportBean merge MyTable where TheString = key1 when not matched then insert select TheString as key1, IntPrimitive as value;\n" +
                    "@Name('out') select * from SupportBean_S0, MyTable where value = id;\n" +
                    "on SupportBean_S1 delete from MyTable where key1 like 'B%';\n";
                    epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            // preload A_n events
            for (var i = 0; i < NUM_KEYS; i++) {
                epService.EPRuntime.SendEvent(new SupportBean("A_" + i, i));
            }
    
            var writeRunnable = new WriteRunnable(epService);
            var readRunnable = new ReadRunnable(epService);
    
            // start
            var threadWrite = new Thread(writeRunnable.Run);
            var threadRead = new Thread(readRunnable.Run);
            threadWrite.Start();
            threadRead.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;
    
            // join
            log.Info("Waiting for completion");
            threadWrite.Join();
            threadRead.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Console.WriteLine("Write loops " + writeRunnable.NumLoops + " and performed " + readRunnable.NumQueries + " reads");
            Assert.IsTrue(writeRunnable.NumLoops > 1);
            Assert.IsTrue(readRunnable.NumQueries > 100);
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private EPException _exception;

            internal bool Shutdown;
            internal int NumLoops;
    
            public WriteRunnable(EPServiceProvider epService)
            {
                _epService = epService;
            }

            public void Run()
            {
                log.Info("Started event send for write");
    
                try
                {
                    while(!Shutdown)
                    {
                        // write additional B_n events
                        for (var i = 0; i < 10000; i++)
                        {
                            _epService.EPRuntime.SendEvent(new SupportBean("B_" + i, i+OFFSET_ADDED));
                        }
                        // delete B_n events
                        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
                        NumLoops++;
                    }
                }
                catch (EPException ex)
                {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                log.Info("Completed event send for write");
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;

            internal bool Shutdown;
            internal int NumQueries;
    
            public ReadRunnable(EPServiceProvider epService)
            {
                _epService = epService;
                _listener = new SupportUpdateListener();
                epService.EPAdministrator.GetStatement("out").AddListener(_listener);
            }

            public void Run() {
                log.Info("Started event send for read");
    
                try {
                    while(!Shutdown) {
                        for (var i = 0; i < NUM_KEYS; i++) {
                            _epService.EPRuntime.SendEvent(new SupportBean_S0(i));
                            var events = _listener.GetAndResetLastNewData();
                            Assert.IsTrue(events.Length > 0);
                        }
                        NumQueries++;
                    }
                }
                catch (EPException ex)
                {
                    log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                log.Info("Completed event send for read");
            }

            public EPException Exception { get; private set; }
        }
    }
}

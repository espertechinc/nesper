///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Reflection;
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
    public class TestTableMTUngroupedAccessReadMergeWrite 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType(typeof(SupportBean_S0));
            config.AddEventType(typeof(SupportBean_S1));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// For a given number of seconds:
        /// Multiple writer threads each update their thread-id into a shared ungrouped row with plain props,
        /// and a single reader thread reads the row and asserts that the values is the same for all cols.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(2, 3);
        }
    
        private void TryMT(int numSeconds, int numWriteThreads) 
        {
            var eplCreateVariable = "create table varagg (c0 int, c1 int, c2 int, c3 int, c4 int, c5 int)";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplMerge = "on SupportBean_S0 merge varagg " +
                    "when not matched then insert select -1 as c0, -1 as c1, -1 as c2, -1 as c3, -1 as c4, -1 as c5 " +
                    "when matched then update set c0=id, c1=id, c2=id, c3=id, c4=id, c5=id";
            _epService.EPAdministrator.CreateEPL(eplMerge);
    
            var listener = new SupportUpdateListener();
            var eplQuery = "select varagg.c0 as c0, varagg.c1 as c1, varagg.c2 as c2," +
                    "varagg.c3 as c3, varagg.c4 as c4, varagg.c5 as c5 from SupportBean_S1";
            _epService.EPAdministrator.CreateEPL(eplQuery).AddListener(listener);
    
            var writeThreads = new Thread[numWriteThreads];
            var writeRunnables = new WriteRunnable[numWriteThreads];
            for (var i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i] = new WriteRunnable(_epService, i);
                writeThreads[i] = new Thread(writeRunnables[i].Run);
                writeThreads[i].Start();
            }
    
            var readRunnable = new ReadRunnable(_epService, listener);
            var readThread = new Thread(readRunnable.Run);
            readThread.Start();
    
            Thread.Sleep(numSeconds * 1000);
    
            // join
            Log.Info("Waiting for completion");
            for (var i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i].Shutdown = true;
                writeThreads[i].Join();
                Assert.IsNull(writeRunnables[i].Exception);
            }
            readRunnable.Shutdown = true;
            readThread.Join();
            Assert.IsNull(readRunnable.Exception);
        }

        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _threadNum;

            private bool _shutdown;
            private EPException _exception;

            public WriteRunnable(EPServiceProvider epService, int threadNum)
            {
                _epService = epService;
                _threadNum = threadNum;
            }

            public void Run()
            {
                Log.Info("Started event send for write");

                try
                {
                    while (!_shutdown)
                    {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(_threadNum));
                    }
                }
                catch (EPException ex)
                {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }

                Log.Info("Completed event send for write");
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }

        public class ReadRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly SupportUpdateListener _listener;

            private EPException _exception;
            private bool _shutdown;

            public ReadRunnable(EPServiceProvider engine, SupportUpdateListener listener)
            {
                _engine = engine;
                _listener = listener;
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public void Run()
            {
                Log.Info("Started event send for read");

                try
                {
                    while (!_shutdown)
                    {
                        var fields = "c1,c2,c3,c4,c5".Split(',');
                        _engine.EPRuntime.SendEvent(new SupportBean_S1(0));
                        var @event = _listener.AssertOneGetNewAndReset();
                        var valueOne = @event.Get("c0");
                        foreach (var field in fields)
                        {
                            Assert.AreEqual(valueOne, @event.Get(field));
                        }
                    }
                }
                catch (EPException ex)
                {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }

                Log.Info("Completed event send for read");
            }

            public EPException Exception
            {
                get { return _exception; }
            }
        }
    }
}

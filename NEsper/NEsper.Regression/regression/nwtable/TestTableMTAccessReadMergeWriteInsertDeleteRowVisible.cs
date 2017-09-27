///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTAccessReadMergeWriteInsertDeleteRowVisible 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType<SupportBean>();
            config.AddEventType(typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Table:
        /// create table MyTable(key string primary key, p0 int, p1 int, p2, int, p3 int, p4 int)
        /// For a given number of seconds:
        /// - Single writer uses merge in a loop:
        /// - inserts MyTable={key='K1', p0=1, p1=1, p2=1, p3=1, p4=1}
        /// - deletes the row
        /// - Single reader outputs p0 to p4 using "MyTable['K1'].px"
        /// Row should either exist with all values found or not exist.
        /// </summary>
        [Test]
        public void TestMTGrouped() {
            TryMT(1, true);
        }
    
        [Test]
        public void TestMTUngrouped() {
            TryMT(1, false);
        }
    
        private void TryMT(int numSeconds, bool grouped) {
            var eplCreateTable = "create table MyTable (key string " + (grouped ? "primary key" : "") +
                    ", p0 int, p1 int, p2 int, p3 int, p4 int, p5 int)";
            _epService.EPAdministrator.CreateEPL(eplCreateTable);
    
            var eplSelect = grouped ?
                    "select MyTable['K1'].p0 as c0, MyTable['K1'].p1 as c1, MyTable['K1'].p2 as c2, " +
                    "MyTable['K1'].p3 as c3, MyTable['K1'].p4 as c4, MyTable['K1'].p5 as c5 from SupportBean_S0"
                    :
                    "select MyTable.p0 as c0, MyTable.p1 as c1, MyTable.p2 as c2, " +
                    "MyTable.p3 as c3, MyTable.p4 as c4, MyTable.p5 as c5 from SupportBean_S0";
            var stmt = _epService.EPAdministrator.CreateEPL(eplSelect);
            var listener = new SupportUpdateListener();
            stmt.AddListener(listener);
    
            var eplMerge = "on SupportBean merge MyTable " +
                    "when not matched then insert select 'K1' as key, 1 as p0, 1 as p1, 1 as p2, 1 as p3, 1 as p4, 1 as p5 " +
                    "when matched then delete";
            _epService.EPAdministrator.CreateEPL(eplMerge);
    
            var writeRunnable = new WriteRunnable(_epService);
            var readRunnable = new ReadRunnable(_epService, listener);
    
            // start
            var t1 = new Thread(writeRunnable.Run);
            var t2 = new Thread(readRunnable.Run);
            t1.Start();
            t2.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;
    
            // join
            Log.Info("Waiting for completion");
            t1.Join();
            t2.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Assert.IsTrue(readRunnable.NotFoundCount > 2);
            Assert.IsTrue(readRunnable.FoundCount > 2);
            Console.WriteLine("Send " + writeRunnable.NumEvents + " and performed " + readRunnable.NumQueries +
                    " reads (found " + readRunnable.FoundCount + ") (not found " + readRunnable.NotFoundCount + ")");
        }
    
        public class WriteRunnable 
        {
            private readonly EPServiceProvider _epService;

            private int _numEvents;
    
            public WriteRunnable(EPServiceProvider epService)
            {
                _epService = epService;
            }

            public int NumEvents
            {
                get { return _numEvents; }
            }

            public bool Shutdown { get; set; }

            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while(!Shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
                        _numEvents++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }

            public EPException Exception { get; private set; }
        }
    
        public class ReadRunnable 
        {
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;

            private int _numQueries;

            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listener)
            {
                this._epService = epService;
                this._listener = listener;
            }

            public int NumQueries
            {
                get { return _numQueries; }
            }

            public bool Shutdown { get; set; }

            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    var fields = "c0,c1,c2,c3,c4,c5".Split(',');
                    var expected = new object[] {1, 1, 1, 1, 1, 1};
                    while(!Shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        var @event = _listener.AssertOneGetNewAndReset();
                        if (@event.Get("c0") == null) {
                            NotFoundCount++;
                        }
                        else {
                            FoundCount++;
                            EPAssertionUtil.AssertProps(@event, fields, expected);
                        }
                        _numQueries++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }

            public EPException Exception { get; private set; }

            public int FoundCount { get; private set; }

            public int NotFoundCount { get; private set; }
        }
    }
}

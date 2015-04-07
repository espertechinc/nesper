///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
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
    public class TestTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd 
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.AddEventType(typeof(LocalGroupEvent));
            config.AddEventType(typeof(SupportBean));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Primary key is composite: {topgroup, subgroup}. Secondary index on {topgroup}.
        /// Single group that always exists is {0,0}. Topgroup is always zero.
        /// For a given number of seconds:
        /// Single writer merge-inserts such as {0,1}, {0,2} to {0, N} then merge-deletes all rows one by one.
        /// Single reader subquery-selects the count all values where subgroup equals 0, should always receive a count of 1 and up.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(3);
        }
    
        private void TryMT(int numSeconds) 
        {
            var eplCreateVariable = "create table vartotal (topgroup int primary key, subgroup int primary key)";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplCreateIndex = "create index myindex on vartotal (topgroup)";
            _epService.EPAdministrator.CreateEPL(eplCreateIndex);
    
            // insert and delete merge
            var eplMergeInsDel = "on LocalGroupEvent as lge merge vartotal as vt " +
                    "where vt.topgroup = lge.topgroup and vt.subgroup = lge.subgroup " +
                    "when not matched and lge.op = 'insert' then insert select lge.topgroup as topgroup, lge.subgroup as subgroup " +
                    "when matched and lge.op = 'delete' then delete";
            _epService.EPAdministrator.CreateEPL(eplMergeInsDel);
    
            // seed with {0, 0} group
            _epService.EPRuntime.SendEvent(new LocalGroupEvent("insert", 0, 0));
    
            // select/read
            var eplSubselect = "select (select count(*) from vartotal where topgroup=sb.IntPrimitive) as c0 " +
                    "from SupportBean as sb";
            var stmtSubselect = _epService.EPAdministrator.CreateEPL(eplSubselect);
            var listener = new SupportUpdateListener();
            stmtSubselect.AddListener(listener);
    
            var writeRunnable = new WriteRunnable(_epService);
            var readRunnable = new ReadRunnable(_epService, listener);
    
            // start
            var writeThread = new Thread(writeRunnable.Run);
            var readThread = new Thread(readRunnable.Run);
            writeThread.Start();
            readThread.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.Shutdown = true;
            readRunnable.Shutdown = true;
    
            // join
            Log.Info("Waiting for completion");
            writeThread.Join();
            readThread.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumLoops > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Console.WriteLine("Send " + writeRunnable.NumLoops + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable {
    
            private readonly EPServiceProvider epService;
    
            private EPException _exception;
            private bool _shutdown;
            private int _numLoops;
    
            public WriteRunnable(EPServiceProvider epService)
            {
                this.epService = epService;
            }

            public int NumLoops
            {
                get { return _numLoops; }
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while(!_shutdown) {
                        for (var i = 0; i < 10; i++) {
                            epService.EPRuntime.SendEvent(new LocalGroupEvent("insert", 0, i+1));
                        }
                        for (var i = 0; i < 10; i++) {
                            epService.EPRuntime.SendEvent(new LocalGroupEvent("delete", 0, i+1));
                        }
                        _numLoops++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
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
    
            private int _numQueries;
            private EPException _exception;
            private bool _shutdown;
    
            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listener)
            {
                _epService = epService;
                _listener = listener;
            }

            public int NumQueries
            {
                get { return _numQueries; }
            }

            public bool Shutdown
            {
                set { _shutdown = value; }
            }

            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while(!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
                        var value = _listener.AssertOneGetNewAndReset().Get("c0");
                        Assert.IsTrue((long?) value >= 1);
                        _numQueries++;
                    }
                }
                catch (EPException ex) {
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

        public class LocalGroupEvent
        {
            public LocalGroupEvent(string op, int topgroup, int subgroup)
            {
                Op = op;
                Topgroup = topgroup;
                Subgroup = subgroup;
            }

            public int Topgroup { get; private set; }

            public int Subgroup { get; private set; }

            public string Op { get; private set; }
        }
    }
}

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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.support.bean;
using com.espertech.esper.support.client;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable
{
    [TestFixture]
    public class TestTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd 
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private EPServiceProvider _epService;
    
        [SetUp]
        public void SetUp()
        {
            var config = SupportConfigFactory.GetConfiguration();
            config.EngineDefaults.ExecutionConfig.IsFairlock = true;
            config.AddEventType(typeof(LocalGroupEvent));
            config.AddEventType<SupportBean>();
            config.AddEventType(typeof(SupportBean_S0));
            _epService = EPServiceProviderManager.GetDefaultProvider(config);
            _epService.Initialize();
        }
    
        /// <summary>
        /// Primary key is composite: {topgroup, subgroup}. Secondary index on {topgroup}.
        /// For a given number of seconds:
        /// Single writer inserts such as {0,1}, {0,2} to {0, Count}, each event a new subgroup and topgroup always 0.
        /// Single reader tries to count all values where subgroup equals 0, should always receive a count of 1 and increasing.
        /// </summary>
        [Test]
        public void TestMT() 
        {
            TryMT(3);
        }
    
        private void TryMT(int numSeconds) 
        {
            var eplCreateVariable = "create table vartotal (topgroup int primary key, subgroup int primary key, thecnt count(*))";
            _epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var eplCreateIndex = "create index myindex on vartotal (topgroup)";
            _epService.EPAdministrator.CreateEPL(eplCreateIndex);
    
            // populate
            var eplInto = "into table vartotal select count(*) as thecnt from LocalGroupEvent.win:length(100) group by topgroup, subgroup";
            _epService.EPAdministrator.CreateEPL(eplInto);
    
            // delete empty groups
            var eplDelete = "on SupportBean_S0 merge vartotal when matched and thecnt = 0 then delete";
            _epService.EPAdministrator.CreateEPL(eplDelete);
    
            // seed with {0, 0} group
            _epService.EPRuntime.SendEvent(new LocalGroupEvent(0, 0));
    
            // select/read
            var eplMergeSelect = "on SupportBean merge vartotal as vt " +
                    "where vt.topgroup = IntPrimitive and vt.thecnt > 0 " +
                    "when matched then insert into MyOutputStream select *";
            _epService.EPAdministrator.CreateEPL(eplMergeSelect);
            var listener = new SupportUpdateListener();
            _epService.EPAdministrator.CreateEPL("select * from MyOutputStream").AddListener(listener);
    
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
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Console.WriteLine("Send " + writeRunnable.NumEvents + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider epService;
    
            internal bool Shutdown;
            internal int NumEvents;
            internal EPException Exception;
    
            public WriteRunnable(EPServiceProvider epService) {
                this.epService = epService;
            }
    
            public void SetShutdown(bool shutdown) {
                this.Shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    var subgroup = 1;
                    while(!Shutdown) {
                        epService.EPRuntime.SendEvent(new LocalGroupEvent(0, subgroup));
                        subgroup++;
    
                        // send delete event
                        if (subgroup % 100 == 0) {
                            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        }
                        NumEvents++;
                    }
                }
                catch (EPException ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider epService;
            private readonly SupportUpdateListener listener;
    
            internal int NumQueries;
            internal bool Shutdown;
            internal EPException Exception;
    
            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listener)
            {
                this.epService = epService;
                this.listener = listener;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while(!Shutdown) {
                        epService.EPRuntime.SendEvent(new SupportBean(null, 0));
                        var len = listener.NewDataList.Count;
                        // Comment me in: Console.WriteLine("Number of events found: " + len);
                        listener.Reset();
                        Assert.IsTrue(len >= 1);
                        NumQueries++;
                    }
                }
                catch (EPException ex)
                {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    Exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }
        }
    
        public class LocalGroupEvent
        {
            public LocalGroupEvent(int topgroup, int subgroup)
            {
                Topgroup = topgroup;
                Subgroup = subgroup;
            }

            public int Topgroup { get; private set; }
            public int Subgroup { get; private set; }
        }
    }
}

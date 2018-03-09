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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTGroupedMergeReadMergeWriteSecondaryIndexUpd : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Primary key is composite: {topgroup, subgroup}. Secondary index on {topgroup}.
        /// For a given number of seconds:
        /// Single writer inserts such as {0,1}, {0,2} to {0, N}, each event a new subgroup and topgroup always 0.
        /// Single reader tries to count all values where subgroup equals 0, should always receive a count of 1 and increasing.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Execution.IsFairlock = true;
            configuration.AddEventType(typeof(LocalGroupEvent));
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 3);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds) {
            string eplCreateVariable = "create table vartotal (topgroup int primary key, subgroup int primary key, thecnt count(*))";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplCreateIndex = "create index myindex on vartotal (topgroup)";
            epService.EPAdministrator.CreateEPL(eplCreateIndex);
    
            // populate
            string eplInto = "into table vartotal select count(*) as thecnt from LocalGroupEvent#length(100) group by topgroup, subgroup";
            epService.EPAdministrator.CreateEPL(eplInto);
    
            // delete empty groups
            string eplDelete = "on SupportBean_S0 merge vartotal when matched and thecnt = 0 then delete";
            epService.EPAdministrator.CreateEPL(eplDelete);
    
            // seed with {0, 0} group
            epService.EPRuntime.SendEvent(new LocalGroupEvent(0, 0));
    
            // select/read
            string eplMergeSelect = "on SupportBean merge vartotal as vt " +
                    "where vt.topgroup = IntPrimitive and vt.thecnt > 0 " +
                    "when matched then insert into MyOutputStream select *";
            epService.EPAdministrator.CreateEPL(eplMergeSelect);
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from MyOutputStream").Events += listener.Update;
    
            var writeRunnable = new WriteRunnable(epService);
            var readRunnable = new ReadRunnable(epService, listener);
    
            // start
            var writeThread = new Thread(writeRunnable.Run);
            var readThread = new Thread(readRunnable.Run);
            writeThread.Start();
            readThread.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.SetShutdown(true);
            readRunnable.SetShutdown(true);
    
            // join
            Log.Info("Waiting for completion");
            writeThread.Join();
            readThread.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Log.Info("Send " + writeRunnable.NumEvents + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numEvents;

            public Exception Exception => _exception;

            public bool Shutdown => _shutdown;

            public int NumEvents => _numEvents;

            public WriteRunnable(EPServiceProvider epService) {
                this._epService = epService;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    int subgroup = 1;
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new LocalGroupEvent(0, subgroup));
                        subgroup++;
    
                        // send delete event
                        if (subgroup % 100 == 0) {
                            _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        }
                        _numEvents++;
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;
    
            private int _numQueries;
            private Exception _exception;
            private bool _shutdown;

            public int NumQueries => _numQueries;

            public Exception Exception => _exception;

            public bool Shutdown => _shutdown;

            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listener) {
                this._epService = epService;
                this._listener = listener;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run()
            {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
                        int len = _listener.NewDataList.Count;
                        // Comment me in: Log.Info("Number of events found: " + len);
                        _listener.Reset();
                        Assert.IsTrue(len >= 1);
                        _numQueries++;
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }
        }
    
        public class LocalGroupEvent {
            private readonly int _topgroup;
            private readonly int _subgroup;

            public int Topgroup => _topgroup;

            public int Subgroup => _subgroup;

            public LocalGroupEvent(int topgroup, int subgroup) {
                this._topgroup = topgroup;
                this._subgroup = subgroup;
            }
        }
    }
} // end of namespace

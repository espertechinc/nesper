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
    public class ExecTableMTGroupedSubqueryReadMergeWriteSecondaryIndexUpd : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Primary key is composite: {topgroup, subgroup}. Secondary index on {topgroup}.
        /// Single group that always Exists is {0,0}. Topgroup is always zero.
        /// For a given number of seconds:
        /// Single writer merge-inserts such as {0,1}, {0,2} to {0, N} then merge-deletes all rows one by one.
        /// Single reader subquery-selects the count all values where subgroup equals 0, should always receive a count of 1 and up.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType(typeof(LocalGroupEvent));
            configuration.AddEventType<SupportBean>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 3);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds) {
            string eplCreateVariable = "create table vartotal (topgroup int primary key, subgroup int primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplCreateIndex = "create index myindex on vartotal (topgroup)";
            epService.EPAdministrator.CreateEPL(eplCreateIndex);
    
            // insert and delete merge
            string eplMergeInsDel = "on LocalGroupEvent as lge merge vartotal as vt " +
                    "where vt.topgroup = lge.topgroup and vt.subgroup = lge.subgroup " +
                    "when not matched and lge.op = 'insert' then insert select lge.topgroup as topgroup, lge.subgroup as subgroup " +
                    "when matched and lge.op = 'delete' then delete";
            epService.EPAdministrator.CreateEPL(eplMergeInsDel);
    
            // seed with {0, 0} group
            epService.EPRuntime.SendEvent(new LocalGroupEvent("insert", 0, 0));
    
            // select/read
            string eplSubselect = "select (select count(*) from vartotal where topgroup=sb.IntPrimitive) as c0 " +
                    "from SupportBean as sb";
            EPStatement stmtSubselect = epService.EPAdministrator.CreateEPL(eplSubselect);
            var listener = new SupportUpdateListener();
            stmtSubselect.Events += listener.Update;
    
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
            Assert.IsTrue(writeRunnable.NumLoops > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Log.Info("Send " + writeRunnable.NumLoops + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numLoops;

            public Exception Exception => _exception;

            public bool Shutdown => _shutdown;

            public int NumLoops => _numLoops;

            public WriteRunnable(EPServiceProvider epService) {
                _epService = epService;
            }
    
            public void SetShutdown(bool shutdown) {
                _shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while (!_shutdown) {
                        for (int i = 0; i < 10; i++) {
                            _epService.EPRuntime.SendEvent(new LocalGroupEvent("insert", 0, i + 1));
                        }
                        for (int i = 0; i < 10; i++) {
                            _epService.EPRuntime.SendEvent(new LocalGroupEvent("delete", 0, i + 1));
                        }
                        _numLoops++;
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
                _epService = epService;
                _listener = listener;
            }
    
            public void SetShutdown(bool shutdown) {
                _shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean(null, 0));
                        Object value = _listener.AssertOneGetNewAndReset().Get("c0");
                        Assert.IsTrue((long) value >= 1);
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
            private readonly string _op;
            private readonly int _topgroup;
            private readonly int _subgroup;

            public string Op => _op;

            public int Topgroup => _topgroup;

            public int Subgroup => _subgroup;

            public LocalGroupEvent(string op, int topgroup, int subgroup)
            {
                _op = op;
                _topgroup = topgroup;
                _subgroup = subgroup;
            }
        }
    }
} // end of namespace

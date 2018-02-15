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
    public class ExecTableMTUngroupedAccessReadMergeWrite : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// For a given number of seconds:
        /// Multiple writer threads each update their thread-id into a shared ungrouped row with plain props,
        /// and a single reader thread reads the row and asserts that the values is the same for all cols.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 2, 3);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds, int numWriteThreads) {
            string eplCreateVariable = "create table varagg (c0 int, c1 int, c2 int, c3 int, c4 int, c5 int)";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplMerge = "on SupportBean_S0 merge varagg " +
                    "when not matched then insert select -1 as c0, -1 as c1, -1 as c2, -1 as c3, -1 as c4, -1 as c5 " +
                    "when matched then update set c0=id, c1=id, c2=id, c3=id, c4=id, c5=id";
            epService.EPAdministrator.CreateEPL(eplMerge);
    
            var listener = new SupportUpdateListener();
            string eplQuery = "select varagg.c0 as c0, varagg.c1 as c1, varagg.c2 as c2," +
                    "varagg.c3 as c3, varagg.c4 as c4, varagg.c5 as c5 from SupportBean_S1";
            epService.EPAdministrator.CreateEPL(eplQuery).Events += listener.Update;
    
            var writeThreads = new Thread[numWriteThreads];
            var writeRunnables = new WriteRunnable[numWriteThreads];
            for (int i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i] = new WriteRunnable(epService, i);
                writeThreads[i] = new Thread(writeRunnables[i].Run);
                writeThreads[i].Start();
            }
    
            var readRunnable = new ReadRunnable(epService, listener);
            var readThread = new Thread(readRunnable.Run);
            readThread.Start();
    
            Thread.Sleep(numSeconds * 1000);
    
            // join
            Log.Info("Waiting for completion");
            for (int i = 0; i < writeThreads.Length; i++) {
                writeRunnables[i].SetShutdown(true);
                writeThreads[i].Join();
                Assert.IsNull(writeRunnables[i].Exception);
            }
            readRunnable.SetShutdown(true);
            readThread.Join();
            Assert.IsNull(readRunnable.Exception);
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _threadNum;
    
            private bool _shutdown;
            private Exception _exception;
    
            public WriteRunnable(EPServiceProvider epService, int threadNum) {
                this._epService = epService;
                this._threadNum = threadNum;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(_threadNum));
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }

            public Exception Exception
            {
                get { return _exception; }
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly SupportUpdateListener _listener;
    
            private Exception _exception;
            private bool _shutdown;
    
            public ReadRunnable(EPServiceProvider engine, SupportUpdateListener listener) {
                this._engine = engine;
                this._listener = listener;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        string[] fields = "c1,c2,c3,c4,c5".Split(',');
                        _engine.EPRuntime.SendEvent(new SupportBean_S1(0));
                        EventBean @event = _listener.AssertOneGetNewAndReset();
                        Object valueOne = @event.Get("c0");
                        foreach (string field in fields) {
                            Assert.AreEqual(valueOne, @event.Get(field));
                        }
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }

            public Exception Exception
            {
                get { return _exception; }
            }
        }
    }
} // end of namespace

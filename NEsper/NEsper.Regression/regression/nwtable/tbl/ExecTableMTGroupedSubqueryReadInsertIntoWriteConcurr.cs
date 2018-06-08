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
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTGroupedSubqueryReadInsertIntoWriteConcurr : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Primary key is single: {id}
        /// For a given number of seconds:
        /// Single writer insert-into such as {0} to {N}.
        /// Single reader subquery-selects the count all rows.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 3);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds) {
            string eplCreateVariable = "create table MyTable (pkey string primary key)";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplInsertInto = "insert into MyTable select TheString as pkey from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInsertInto);
    
            // seed with count 1
            epService.EPRuntime.SendEvent(new SupportBean("E0", 0));
    
            // select/read
            string eplSubselect = "select (select count(*) from MyTable) as c0 from SupportBean_S0";
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
                        _epService.EPRuntime.SendEvent(new SupportBean("E" + _numLoops + 1, 0));
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
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
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
    
            public Exception GetException() {
                return _exception;
            }
    
            public int GetNumQueries() {
                return _numQueries;
            }
        }
    }
} // end of namespace

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
    public class ExecTableMTGroupedJoinReadMergeWriteSecondaryIndexUpd : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private static readonly int NUM_KEYS = 10;
        private static readonly int OFFSET_ADDED = 100000000;
    
        /// <summary>
        /// Tests concurrent updates on a secondary index also read by a join:
        /// create table MyTable (key string primary key, value int)
        /// create index MyIndex on MyTable (value)
        /// select * from SupportBean_S0, MyTable where intPrimitive = id
        /// <para>
        /// Prefill MyTable with MyTable={key='A_N', value=N} with N between 0 and NUM_KEYS-1
        /// </para>
        /// <para>
        /// For x seconds:
        /// Single reader thread sends SupportBean events, asserts that either one or two rows are found (A_N and maybe B_N)
        /// Single writer thread inserts MyTable={key='B_N', value=100000+N} and deletes each row.
        /// </para>
        /// </summary>
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
            configuration.AddEventType(typeof(SupportBean_S1));
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 2);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds) {
            string epl =
                    "create table MyTable (key1 string primary key, value int);\n" +
                            "create index MyIndex on MyTable (value);\n" +
                            "on SupportBean merge MyTable where TheString = key1 when not matched then insert select TheString as key1, IntPrimitive as value;\n" +
                            "@Name('out') select * from SupportBean_S0, MyTable where value = id;\n" +
                            "on SupportBean_S1 delete from MyTable where key1 like 'B%';\n";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            // preload A_n events
            for (int i = 0; i < NUM_KEYS; i++) {
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
            writeRunnable.SetShutdown(true);
            readRunnable.SetShutdown(true);
    
            // join
            Log.Info("Waiting for completion");
            threadWrite.Join();
            threadRead.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsNull(readRunnable.Exception);
            Log.Info("Write loops " + writeRunnable.NumLoops + " and performed " + readRunnable.NumQueries + " reads");
            Assert.IsTrue(writeRunnable.NumLoops > 1);
            Assert.IsTrue(readRunnable.NumQueries > 100);
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numLoops;

            public bool Shutdown => _shutdown;

            public int NumLoops => _numLoops;

            public WriteRunnable(EPServiceProvider epService) {
                this._epService = epService;
            }

            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while (!_shutdown) {
                        // write additional B_n events
                        for (int i = 0; i < 10000; i++) {
                            _epService.EPRuntime.SendEvent(new SupportBean("B_" + i, i + OFFSET_ADDED));
                        }
                        // delete B_n events
                        _epService.EPRuntime.SendEvent(new SupportBean_S1(0));
                        _numLoops++;
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }

            public Exception Exception
            {
                get { return _exception; }
            }
        }
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numQueries;

            public int NumQueries => _numQueries;

            public ReadRunnable(EPServiceProvider epService) {
                this._epService = epService;
                _listener = new SupportUpdateListener();
                epService.EPAdministrator.GetStatement("out").Events += _listener.Update;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        for (int i = 0; i < NUM_KEYS; i++) {
                            _epService.EPRuntime.SendEvent(new SupportBean_S0(i));
                            EventBean[] events = _listener.GetAndResetLastNewData();
                            Assert.IsTrue(events.Length > 0);
                        }
                        _numQueries++;
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

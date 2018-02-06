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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTUngroupedSubqueryReadMergeWriteColumnUpd : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Tests column-consistency for subquery:
        /// create table MyTable(p0 string, p1 string, ..., p4 string)   (5 props)
        /// Insert row single: MyTable={p0="1", p1="1", p2="1", p3="1", p4="1"}
        /// <para>
        /// A writer-thread uses an on-merge statement to update the p0 to p4 columns from "1" to "2", then "2" to "1"
        /// A reader-thread uses a subquery checking ("p1="1" and p2="1" and p3="1" and p4="1")
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
                    "create table MyTable (p0 string, p1 string, p2 string, p3 string, p4 string);\n" +
                            "on SupportBean merge MyTable " +
                            "  when not matched then insert select '1' as p0, '1' as p1, '1' as p2, '1' as p3, '1' as p4;\n" +
                            "on SupportBean_S0 merge MyTable " +
                            "  when matched then update set p0=p00, p1=p00, p2=p00, p3=p00, p4=p00;\n" +
                            "@Name('out') select " +
                            "(select p0 from MyTable where " +
                            "  (p0='1' and p1='1' and p2='1' and p3='1' and p4='1') " +
                            "  or (p0='2' and p1='2' and p2='2' and p3='2' and p4='2')) as c0 " +
                            "from SupportBean_S1;\n";
            epService.EPAdministrator.DeploymentAdmin.ParseDeploy(epl);
    
            // preload
            epService.EPRuntime.SendEvent(new SupportBean());
    
            var writeRunnable = new ExecTableMTUngroupedJoinColumnConsistency.UpdateWriteRunnable(epService);
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
    
        public class ReadRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly SupportUpdateListener _listener;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numQueries;

            public Exception Exception1 => _exception;

            public bool Shutdown => _shutdown;

            public int NumQueries => _numQueries;

            public ReadRunnable(EPServiceProvider epService) {
                _epService = epService;
                _listener = new SupportUpdateListener();
                epService.EPAdministrator.GetStatement("out").Events += _listener.Update;
            }
    
            public void SetShutdown(bool shutdown) {
                _shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S1(0, null));
                        Object value = _listener.AssertOneGetNewAndReset().Get("c0");
                        if (value == null) {
                            throw new IllegalStateException("Value is null");
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

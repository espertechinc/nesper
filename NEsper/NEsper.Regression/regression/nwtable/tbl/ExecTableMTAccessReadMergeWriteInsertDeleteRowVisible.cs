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
    public class ExecTableMTAccessReadMergeWriteInsertDeleteRowVisible : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Table:
        /// create table MyTable(key string primary key, p0 int, p1 int, p2, int, p3 int, p4 int)
        /// <para>
        /// For a given number of seconds:
        /// - Single writer uses merge in a loop:
        /// - inserts MyTable={key='K1', p0=1, p1=1, p2=1, p3=1, p4=1}
        /// - deletes the row
        /// - Single reader outputs p0 to p4 using "MyTable['K1'].px"
        /// Row should either exist with all values found or not exist.
        /// </para>
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 1, true);
            TryMT(epService, 1, false);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds, bool grouped) {
            string eplCreateTable = "create table MyTable (key string " + (grouped ? "primary key" : "") +
                    ", p0 int, p1 int, p2 int, p3 int, p4 int, p5 int)";
            epService.EPAdministrator.CreateEPL(eplCreateTable);
    
            string eplSelect = grouped ?
                    "select MyTable['K1'].p0 as c0, MyTable['K1'].p1 as c1, MyTable['K1'].p2 as c2, " +
                            "MyTable['K1'].p3 as c3, MyTable['K1'].p4 as c4, MyTable['K1'].p5 as c5 from SupportBean_S0"
                    :
                    "select MyTable.p0 as c0, MyTable.p1 as c1, MyTable.p2 as c2, " +
                            "MyTable.p3 as c3, MyTable.p4 as c4, MyTable.p5 as c5 from SupportBean_S0";
            EPStatement stmt = epService.EPAdministrator.CreateEPL(eplSelect);
            var listener = new SupportUpdateListener();
            stmt.Events += listener.Update;
    
            string eplMerge = "on SupportBean merge MyTable " +
                    "when not matched then insert select 'K1' as key, 1 as p0, 1 as p1, 1 as p2, 1 as p3, 1 as p4, 1 as p5 " +
                    "when matched then delete";
            epService.EPAdministrator.CreateEPL(eplMerge);
    
            var writeRunnable = new WriteRunnable(epService);
            var readRunnable = new ReadRunnable(epService, listener);
    
            // start
            var t1 = new Thread(writeRunnable.Run);
            var t2 = new Thread(readRunnable.Run);
            t1.Start();
            t2.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.SetShutdown(true);
            readRunnable.SetShutdown(true);
    
            // join
            Log.Info("Waiting for completion");
            t1.Join();
            t2.Join();
    
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsTrue(writeRunnable.numEvents > 100);
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(readRunnable.numQueries > 100);
            Assert.IsTrue(readRunnable.NotFoundCount > 2);
            Assert.IsTrue(readRunnable.FoundCount > 2);
            Log.Info("Send " + writeRunnable.numEvents + " and performed " + readRunnable.numQueries +
                    " reads (found " + readRunnable.FoundCount + ") (not found " + readRunnable.NotFoundCount + ")");
    
            epService.EPAdministrator.DestroyAllStatements();
            epService.EPAdministrator.Configuration.RemoveEventType("table_MyTable__internal", false);
            epService.EPAdministrator.Configuration.RemoveEventType("table_MyTable__public", false);
        }
    
        public class WriteRunnable {
    
            private readonly EPServiceProvider epService;
    
            private Exception exception;
            private bool shutdown;
            internal int numEvents;
    
            public WriteRunnable(EPServiceProvider epService) {
                this.epService = epService;
            }
    
            public void SetShutdown(bool shutdown) {
                this.shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    while (!shutdown) {
                        epService.EPRuntime.SendEvent(new SupportBean(null, 0));
                        numEvents++;
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }

            public Exception Exception => exception;
        }
    
        public class ReadRunnable {
    
            private readonly EPServiceProvider epService;
            private readonly SupportUpdateListener listener;
    
            private Exception exception;
            private bool shutdown;
            internal int numQueries;
            private int foundCount;
            private int notFoundCount;
    
            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listener) {
                this.epService = epService;
                this.listener = listener;
            }
    
            public void SetShutdown(bool shutdown) {
                this.shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    string[] fields = "c0,c1,c2,c3,c4,c5".Split(',');
                    var expected = new object[]{1, 1, 1, 1, 1, 1};
                    while (!shutdown) {
                        epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        EventBean @event = listener.AssertOneGetNewAndReset();
                        if (@event.Get("c0") == null) {
                            notFoundCount++;
                        } else {
                            foundCount++;
                            EPAssertionUtil.AssertProps(@event, fields, expected);
                        }
                        numQueries++;
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }

            public Exception Exception => exception;

            public int FoundCount => foundCount;

            public int NotFoundCount => notFoundCount;
        }
    }
} // end of namespace

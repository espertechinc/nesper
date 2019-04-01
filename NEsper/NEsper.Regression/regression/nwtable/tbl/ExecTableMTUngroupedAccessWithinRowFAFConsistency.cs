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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;


using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTUngroupedAccessWithinRowFAFConsistency : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// For a given number of seconds:
        /// Single writer updates the group (round-robin) count, sum and avg.
        /// A FAF reader thread pulls the value and checks they are consistent.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 2);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds) {
            string eplCreateVariable = "create table vartotal (cnt count(*), sumint sum(int), avgint avg(int))";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplInto = "into table vartotal select count(*) as cnt, sum(IntPrimitive) as sumint, avg(IntPrimitive) as avgint from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInto);
    
            epService.EPAdministrator.CreateEPL("create window MyWindow#lastevent as SupportBean_S0");
            epService.EPAdministrator.CreateEPL("insert into MyWindow select * from SupportBean_S0");
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            var writeRunnable = new WriteRunnable(epService);
            var readRunnable = new ReadRunnable(epService);
    
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
            Assert.IsNull(readRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            Assert.IsTrue(readRunnable.NumQueries > 100);
            Log.Info("Send " + writeRunnable.NumEvents + " and performed " + readRunnable.NumQueries + " reads");
        }
    
        public class WriteRunnable {
    
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
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean("E1", 2));
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
    
            private Exception _exception;
            private bool _shutdown;
            private int _numQueries;

            public Exception Exception => _exception;

            public bool Shutdown => _shutdown;

            public int NumQueries => _numQueries;

            public ReadRunnable(EPServiceProvider epService) {
                this._epService = epService;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                // warmup
                try {
                    Thread.Sleep(100);
                } catch (ThreadInterruptedException) {
                }
    
                try {
                    string eplSelect = "select vartotal.cnt as c0, vartotal.sumint as c1, vartotal.avgint as c2 from MyWindow";
    
                    while (!_shutdown) {
                        EPOnDemandQueryResult result = _epService.EPRuntime.ExecuteQuery(eplSelect);
                        long count = result.Array[0].Get("c0").AsLong();
                        int sumint = result.Array[0].Get("c1").AsInt();
                        double avgint = result.Array[0].Get("c2").AsDouble();
                        Assert.AreEqual(2d, avgint);
                        Assert.AreEqual(sumint, count * 2);
                        _numQueries++;
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for read");
            }
        }
    }
} // end of namespace

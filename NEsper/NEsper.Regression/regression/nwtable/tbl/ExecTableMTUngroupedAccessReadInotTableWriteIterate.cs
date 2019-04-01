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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.nwtable.tbl
{
    public class ExecTableMTUngroupedAccessReadInotTableWriteIterate : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// Proof that multiple threads iterating the same statement
        /// can safely access a row that is currently changing.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 3, 3);
        }
    
        private void TryMT(EPServiceProvider epService, int numReadThreads, int numSeconds) {
            string eplCreateVariable = "create table vartotal (s0 sum(int), s1 sum(double), s2 sum(long))";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplInto = "into table vartotal select sum(IntPrimitive) as s0, " +
                    "sum(DoublePrimitive) as s1, sum(LongPrimitive) as s2 from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInto);
            epService.EPRuntime.SendEvent(MakeSupportBean("E", 1, 1, 1));
    
            EPStatement iterateStatement = epService.EPAdministrator.CreateEPL("select vartotal.s0 as c0, vartotal.s1 as c1, vartotal.s2 as c2 from SupportBean_S0#lastevent");
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
    
            // setup writer
            var writeRunnable = new WriteRunnable(epService);
            var writeThread = new Thread(writeRunnable.Run);
    
            // setup readers
            var readThreads = new Thread[numReadThreads];
            var readRunnables = new ReadRunnable[numReadThreads];
            for (int i = 0; i < readThreads.Length; i++) {
                readRunnables[i] = new ReadRunnable(iterateStatement);
                readThreads[i] = new Thread(readRunnables[i].Run);
            }
    
            // start
            foreach (Thread readThread in readThreads) {
                readThread.Start();
            }
            writeThread.Start();
    
            // wait
            Thread.Sleep(numSeconds * 1000);
    
            // shutdown
            writeRunnable.SetShutdown(true);
            foreach (ReadRunnable readRunnable in readRunnables) {
                readRunnable.SetShutdown(true);
            }
    
            // join
            Log.Info("Waiting for completion");
            writeThread.Join();
            foreach (Thread readThread in readThreads) {
                readThread.Join();
            }
    
            // assert
            Assert.IsNull(writeRunnable.Exception);
            Assert.IsTrue(writeRunnable.NumEvents > 100);
            foreach (ReadRunnable readRunnable in readRunnables) {
                Assert.IsNull(readRunnable.Exception);
                Assert.IsTrue(readRunnable.NumQueries > 100);
            }
        }
    
        private static SupportBean MakeSupportBean(string theString, int intPrimitive, double doublePrimitive, long longPrimitive) {
            var b = new SupportBean(theString, intPrimitive);
            b.DoublePrimitive = doublePrimitive;
            b.LongPrimitive = longPrimitive;
            return b;
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
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(MakeSupportBean("E", 1, 1, 1));
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
            private readonly EPStatement _iterateStatement;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numQueries;

            public Exception Exception => _exception;

            public bool Shutdown => _shutdown;

            public int NumQueries => _numQueries;

            public ReadRunnable(EPStatement iterateStatement) {
                this._iterateStatement = iterateStatement;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        using (var iterator = _iterateStatement.GetSafeEnumerator())
                        {
                            iterator.MoveNext();
                            EventBean @event = iterator.Current;
                            int c0 = @event.Get("c0").AsInt();
                            Assert.AreEqual((double)c0, @event.Get("c1"));
                            Assert.AreEqual((long)c0, @event.Get("c2"));
                        }

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

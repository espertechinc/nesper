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
    public class ExecTableMTUngroupedAccessReadIntoTableWriteFilterUse : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        /// <summary>
        /// For a given number of seconds:
        /// Single writer updates a total sum, continuously adding 1 and subtracting 1.
        /// Two statements are set up, one listens to "0" and the other to "1"
        /// Single reader sends event and that event must be received by any one of the listeners.
        /// </summary>
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 3);
        }
    
        private void TryMT(EPServiceProvider epService, int numSeconds) {
            string eplCreateVariable = "create table vartotal (total sum(int))";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            string eplInto = "into table vartotal select sum(IntPrimitive) as total from SupportBean";
            epService.EPAdministrator.CreateEPL(eplInto);
    
            var listenerZero = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean_S0(1 = vartotal.total)").Events += listenerZero.Update;
    
            var listenerOne = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select * from SupportBean_S0(0 = vartotal.total)").Events += listenerOne.Update;
    
            var writeRunnable = new WriteRunnable(epService);
            var readRunnable = new ReadRunnable(epService, listenerZero, listenerOne);
    
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
                        _epService.EPRuntime.SendEvent(new SupportBean("E", 1));
                        _epService.EPRuntime.SendEvent(new SupportBean("E", -1));
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
            private readonly SupportUpdateListener _listenerZero;
            private readonly SupportUpdateListener _listenerOne;
    
            private Exception _exception;
            private bool _shutdown;
            private int _numQueries;

            public Exception Exception => _exception;

            public bool Shutdown => _shutdown;

            public int NumQueries => _numQueries;

            public ReadRunnable(EPServiceProvider epService, SupportUpdateListener listenerZero, SupportUpdateListener listenerOne) {
                this._epService = epService;
                this._listenerZero = listenerZero;
                this._listenerOne = listenerOne;
            }
    
            public void SetShutdown(bool shutdown) {
                this._shutdown = shutdown;
            }
    
            public void Run() {
                Log.Info("Started event send for read");
    
                try {
                    while (!_shutdown) {
                        _epService.EPRuntime.SendEvent(new SupportBean_S0(0));
                        _listenerZero.Reset();
                        _listenerOne.Reset();
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

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
    public class ExecTableMTUngroupedIntoTableWriteMultiWriterAgg : RegressionExecution {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        public override void Configure(Configuration configuration) {
            configuration.AddEventType<SupportBean>();
            configuration.AddEventType<SupportBean_S0>();
        }
    
        /// <summary>
        /// For a given number of seconds:
        /// Configurable number of into-writers update a shared aggregation.
        /// At the end of the test we read and assert.
        /// </summary>
        public override void Run(EPServiceProvider epService) {
            TryMT(epService, 3, 10000);
        }
    
        private void TryMT(EPServiceProvider epService, int numThreads, int numEvents) {
            string eplCreateVariable = "create table varagg (theEvents window(*) @Type(SupportBean))";
            epService.EPAdministrator.CreateEPL(eplCreateVariable);
    
            var threads = new Thread[numThreads];
            var runnables = new WriteRunnable[numThreads];
            for (int i = 0; i < threads.Length; i++) {
                runnables[i] = new WriteRunnable(epService, numEvents, i);
                threads[i] = new Thread(runnables[i].Run);
                threads[i].Start();
            }
    
            // join
            Log.Info("Waiting for completion");
            for (int i = 0; i < threads.Length; i++) {
                threads[i].Join();
                Assert.IsNull(runnables[i].Exception);
            }
    
            // verify
            var listener = new SupportUpdateListener();
            epService.EPAdministrator.CreateEPL("select varagg.theEvents as c0 from SupportBean_S0").Events += listener.Update;
            epService.EPRuntime.SendEvent(new SupportBean_S0(0));
            EventBean @event = listener.AssertOneGetNewAndReset();
            SupportBean[] window = (SupportBean[]) @event.Get("c0");
            Assert.AreEqual(numThreads * 3, window.Length);
        }
    
        public class WriteRunnable
        {
            private readonly EPServiceProvider _epService;
            private readonly int _numEvents;
            private readonly int _threadNum;
    
            private Exception _exception;

            public int NumEvents => _numEvents;

            public int ThreadNum => _threadNum;

            public Exception Exception => _exception;

            public WriteRunnable(EPServiceProvider epService, int numEvents, int threadNum) {
                this._epService = epService;
                this._numEvents = numEvents;
                this._threadNum = threadNum;
            }
    
            public void Run() {
                Log.Info("Started event send for write");
    
                try {
                    string eplInto = "into table varagg select window(*) as theEvents from SupportBean(TheString='E" + _threadNum + "')#length(3)";
                    _epService.EPAdministrator.CreateEPL(eplInto);
    
                    for (int i = 0; i < _numEvents; i++) {
                        _epService.EPRuntime.SendEvent(new SupportBean("E" + _threadNum, i));
                    }
                } catch (Exception ex) {
                    Log.Error("Exception encountered: " + ex.Message, ex);
                    _exception = ex;
                }
    
                Log.Info("Completed event send for write");
            }
    
            public Exception GetException() {
                return _exception;
            }
        }
    }
} // end of namespace

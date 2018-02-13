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
using com.espertech.esper.compat.logging;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextStartedBySameEvent : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Configure(Configuration configuration) {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = true;
            configuration.AddEventType(typeof(PayloadEvent));
        }
    
        public override void Run(EPServiceProvider epService) {
            var eplStatement = "create context MyContext start PayloadEvent end after 0.5 seconds";
            epService.EPAdministrator.CreateEPL(eplStatement);
    
            var aggStatement = "@Name('select') context MyContext " +
                    "select count(*) as theCount " +
                    "from PayloadEvent " +
                    "output snapshot when terminated";
            var epAggStatement = epService.EPAdministrator.CreateEPL(aggStatement);
            var listener = new MyListener();
            epAggStatement.Events += listener.Update;
    
            // start thread
            long numEvents = 10000000;
            var myRunnable = new MyRunnable(epService, numEvents);
            var thread = new Thread(myRunnable.Run);
            thread.Start();
            thread.Join();
    
            Thread.Sleep(1000);
    
            // assert
            Assert.IsNull(myRunnable.Exception);
            Assert.AreEqual(numEvents, listener.Total);
        }
    
        public class PayloadEvent {
        }
    
        public class MyRunnable {
            private readonly EPServiceProvider _engine;
            private readonly long _numEvents;
            private Exception _exception;

            public long NumEvents => _numEvents;

            public Exception Exception => _exception;

            public MyRunnable(EPServiceProvider engine, long numEvents) {
                this._engine = engine;
                this._numEvents = numEvents;
            }
    
            public void Run() {
                try {
                    for (var i = 0; i < _numEvents; i++) {
                        var payloadEvent = new PayloadEvent();
                        _engine.EPRuntime.SendEvent(payloadEvent);
                        if (i > 0 && i % 1000000 == 0) {
                            Log.Info("sent " + i + " events");
                        }
                    }
                    Log.Info("sent " + _numEvents + " events");
                } catch (Exception ex) {
                    this._exception = ex;
                }
            }
        }
    
        public class MyListener
        {
            private long _total;

            public long Total => _total;

            public void Update(object sender, UpdateEventArgs args)
            {
                var newEvents = args.NewEvents;
                var theCount = (long) newEvents[0].Get("theCount");
                _total += theCount;
                Log.Info("count " + theCount + " total " + _total);
            }
        }
    }
} // end of namespace

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
using com.espertech.esper.compat.threading;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using com.espertech.esper.supportregression.util;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTInsertIntoTimerConcurrency : RegressionExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private AtomicLong _idCounter;
        private BasicExecutorService _executorService;
        private EPRuntime _epRuntime;
        private EPAdministrator _epAdministrator;
        private NoActionUpdateListener _noActionUpdateListener;
    
        public override void Run(EPServiceProvider epService) {
            RunAssertion();
        }
    
        private void RunAssertion() {
            _idCounter = new AtomicLong(0);
            _executorService = Executors.NewCachedThreadPool();
            _noActionUpdateListener = new NoActionUpdateListener();
    
            var epConfig = new Configuration(SupportContainer.Instance);
            epConfig.AddEventType<SupportBean>();
            epConfig.EngineDefaults.Threading.InsertIntoDispatchLocking = ConfigurationEngineDefaults.ThreadingConfig.Locking.SUSPEND;
    
            EPServiceProvider epServiceProvider = EPServiceProviderManager.GetProvider(
                SupportContainer.Instance, this.GetType().Name, epConfig);
            epServiceProvider.Initialize();
    
            _epAdministrator = epServiceProvider.EPAdministrator;
            _epRuntime = epServiceProvider.EPRuntime;
    
            _epAdministrator.StartAllStatements();
    
            string epl = "insert into Stream1 select count(*) as cnt from SupportBean#time(7 sec)";
            CreateEPL(epl, _noActionUpdateListener.Update);
            epl = epl + " output every 10 seconds";
            CreateEPL(epl, _noActionUpdateListener.Update);
    
            var sendTickEventRunnable = new SendEventRunnable(10000, SendEvent);
            Start(sendTickEventRunnable, 4);
    
            // Adjust here for long-running test
            Thread.Sleep(3000);
    
            _executorService.Shutdown();
            _executorService.AwaitTermination(1, TimeUnit.SECONDS);
        }
    
        private void CreateEPL(string epl, UpdateEventHandler updateEventHandler) {
            var statement = _epAdministrator.CreateEPL(epl);
            statement.Events += updateEventHandler;
        }
    
        private void Start<T>(ICallable<T> task, int numInstances) {
            for (int i = 0; i < numInstances; i++) {
                Start(task);
            }
        }
    
        private Future<T> Start<T>(ICallable<T> task) {
            Future<T> future = _executorService.Submit(task);
            return future;
        }
    
        private void SendEvent() {
            long id = _idCounter.GetAndIncrement();
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = id;
            _epRuntime.SendEvent(theEvent);
        }
    
        class SendEventRunnable : ICallable<object> {
            private readonly int _maxSent;
            private readonly Action _sendEvent;
    
            public SendEventRunnable(int maxSent, Action sendEvent) {
                _maxSent = maxSent;
                _sendEvent = sendEvent;
            }
    
            public object Call() {
                int count = 0;
                while (true) {
                    _sendEvent.Invoke();
                    Thread.Sleep(1);
                    count++;
    
                    if (count % 1000 == 0) {
                        Log.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " send " + count + " events");
                    }
    
                    if (count > _maxSent) {
                        break;
                    }
                }
    
                return null;
            }
        }
    }
} // end of namespace

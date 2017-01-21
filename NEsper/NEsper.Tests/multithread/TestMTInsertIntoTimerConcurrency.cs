///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.compat.threading;
using com.espertech.esper.support.bean;

using NUnit.Framework;

namespace com.espertech.esper.multithread
{
    [TestFixture]
    public class TestMTInsertIntoTimerConcurrency
    {
        private long _idCounter;
        private IExecutorService _executorService;
        private EPRuntime _epRuntime;
        private EPAdministrator _epAdministrator;
        private UpdateEventHandler _noActionUpdateEventHandler;

        private EPStatement CreateEPL(String epl, UpdateEventHandler eventHandler)
        {
            EPStatement statement = _epAdministrator.CreateEPL(epl);
            statement.Events += eventHandler;
            return statement;
        }

        private void Start(ICallable<object> task, int numInstances)
        {
            for (int i = 0; i < numInstances; i++) {
                Start(task);
            }
        }

        private void Start(ICallable<object> task)
        {
            _executorService.Submit(task);
        }

        private void SendEvent()
        {
            long id = Interlocked.Increment(ref _idCounter);
            var theEvent = new SupportBean();
            theEvent.LongPrimitive = id;
            _epRuntime.SendEvent(theEvent);
        }

        private class SendEventRunnable<T> : ICallable<T>
        {
            private readonly int _maxSent;
            private readonly Runnable _sendEvent;

            public SendEventRunnable(Runnable sendEvent, int maxSent)
            {
                _sendEvent = sendEvent;
                _maxSent = maxSent;
            }

            #region ICallable Members

            public T Call()
            {
                int count = 0;
                while (true) {
                    _sendEvent();
                    Thread.Sleep(1);
                    count++;

                    if (count%1000 == 0) {
                        Log.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " send " + count + " events");
                    }

                    if (count > _maxSent) {
                        break;
                    }
                }

                return default(T);
            }

            #endregion
        }

        [Test]
        public void TestRun()
        {
            _idCounter = 0L;
            _executorService = Executors.NewCachedThreadPool();
            _noActionUpdateEventHandler = (sender, arg) => { };

            var epConfig = new Configuration();
            epConfig.AddEventType<SupportBean>();
            epConfig.EngineDefaults.ThreadingConfig.InsertIntoDispatchLocking = ConfigurationEngineDefaults.Threading.Locking.SUSPEND;

            EPServiceProvider epServiceProvider = EPServiceProviderManager.GetDefaultProvider(epConfig);
            epServiceProvider.Initialize();

            _epAdministrator = epServiceProvider.EPAdministrator;
            _epRuntime = epServiceProvider.EPRuntime;

            _epAdministrator.StartAllStatements();

            String epl = "insert into Stream1 select count(*) as cnt from SupportBean.win:time(7 sec)";
            CreateEPL(epl, delegate { });
            epl = epl + " output every 10 seconds";
            CreateEPL(epl, delegate { });

            var sendTickEventRunnable = new SendEventRunnable<object>(SendEvent, 10000);
            Start(sendTickEventRunnable, 4);

            // Adjust here for long-running test
            Thread.Sleep(3000);

            _executorService.Shutdown();
            _executorService.AwaitTermination(new TimeSpan(0, 0, 1));
        }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    }
}

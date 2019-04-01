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
using com.espertech.esper.core.service;
using com.espertech.esper.supportregression.bean.word;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTStmtStateless : RegressionExecution
    {
        private static readonly ILog Log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void Run(EPServiceProvider epService)
        {
            TrySend(epService, 4, 1000);
        }

        private void TrySend(EPServiceProvider epService, int numThreads, int numRepeats)
        {
            epService.EPAdministrator.Configuration.AddEventType(typeof(SentenceEvent));
            EPStatementSPI spi =
                (EPStatementSPI) epService.EPAdministrator.CreateEPL("select * from SentenceEvent[words]");
            Assert.IsTrue(spi.StatementContext.IsStatelessSelect);

            var runnables = new StatelessRunnable[numThreads];
            for (int i = 0; i < runnables.Length; i++)
            {
                runnables[i] = new StatelessRunnable(epService, numRepeats);
            }

            var threads = new Thread[numThreads];
            for (int i = 0; i < runnables.Length; i++)
            {
                threads[i] = new Thread(runnables[i].Run);
            }

            long start = PerformanceObserver.MilliTime;
            foreach (Thread t in threads)
            {
                t.Start();
            }

            foreach (Thread t in threads)
            {
                t.Join();
            }

            long delta = DateTimeHelper.CurrentTimeMillis - start;
            Log.Info("Delta=" + delta + " for " + numThreads * numRepeats + " events");

            foreach (StatelessRunnable r in runnables)
            {
                Assert.IsNull(r.Exception);
            }
        }

        public class StatelessRunnable
        {
            private readonly EPServiceProvider _engine;
            private readonly int _numRepeats;
            private Exception _exception;

            public StatelessRunnable(EPServiceProvider engine, int numRepeats)
            {
                _engine = engine;
                _numRepeats = numRepeats;
            }

            public void Run()
            {
                try
                {
                    for (int i = 0; i < _numRepeats; i++)
                    {
                        _engine.EPRuntime.SendEvent(new SentenceEvent("This is stateless statement testing"));

                        if (i % 10000 == 0)
                        {
                            Log.Info("Thread " + Thread.CurrentThread.ManagedThreadId + " sending event " + i);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _exception = ex;
                }
            }

            public Exception Exception => _exception;
        }
    }
} // end of namespace

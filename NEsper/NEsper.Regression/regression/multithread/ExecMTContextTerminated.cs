///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Threading;

using com.espertech.esper.client;
using com.espertech.esper.supportregression.execution;

using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    public class ExecMTContextTerminated : RegressionExecution
    {
        public override void Configure(Configuration configuration)
        {
            configuration.EngineDefaults.Threading.IsInternalTimerEnabled = true;
            configuration.AddEventType(typeof(StartContextEvent));
            configuration.AddEventType(typeof(PayloadEvent));
        }

        public override void Run(EPServiceProvider epService)
        {
            var eplStatement = "create context StartThenTwoSeconds start StartContextEvent end after 2 seconds";
            epService.EPAdministrator.CreateEPL(eplStatement);

            var aggStatement = "@Name('select') context StartThenTwoSeconds " +
                               "select account, count(*) as totalCount " +
                               "from PayloadEvent " +
                               "group by account " +
                               "output snapshot when terminated";
            var epAggStatement = epService.EPAdministrator.CreateEPL(aggStatement);
            epAggStatement.Events += (sender, args) =>
            {
                // no action, still listening to make sure select-clause evaluates
            };

            // start context
            epService.EPRuntime.SendEvent(new StartContextEvent());

            // start threads
            var threads = new List<Thread>();
            var runnables = new List<MyRunnable>();
            for (var i = 0; i < 8; i++)
            {
                var myRunnable = new MyRunnable(epService);
                runnables.Add(myRunnable);
                var thread = new Thread(myRunnable.Run);
                thread.Name = "Thread" + i;
                thread.Start();
                threads.Add(thread);
            }

            // join
            foreach (var thread in threads)
            {
                thread.Join();
            }

            // assert
            foreach (var runnable in runnables)
            {
                Assert.IsNull(runnable.Exception);
            }
        }

        public class StartContextEvent
        {
        }

        public class PayloadEvent
        {
            public PayloadEvent(string account)
            {
                Account = account;
            }

            public string Account { get; }
        }

        public class MyRunnable
        {
            private readonly EPServiceProvider _engine;

            public MyRunnable(EPServiceProvider engine)
            {
                _engine = engine;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                try
                {
                    for (var i = 0; i < 2000000; i++)
                    {
                        var payloadEvent = new PayloadEvent("A1");
                        _engine.EPRuntime.SendEvent(payloadEvent);
                    }
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
            }
        }
    }
} // end of namespace
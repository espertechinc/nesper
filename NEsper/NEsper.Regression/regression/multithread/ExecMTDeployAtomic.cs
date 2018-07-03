///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using com.espertech.esper.client;
using com.espertech.esper.collection;
using com.espertech.esper.compat.collections;
using com.espertech.esper.supportregression.bean;
using com.espertech.esper.supportregression.execution;
using NUnit.Framework;

namespace com.espertech.esper.regression.multithread
{
    /// <summary>Test for multithread-safety for atomic module deployment.</summary>
    public class ExecMTDeployAtomic : RegressionExecution
    {
        private static readonly int NUM_STMTS = 100;

        public override void Run(EPServiceProvider epService)
        {
            epService.EPAdministrator.Configuration.AddEventType<SupportBean>();
            var runnable = new MySendRunnable(epService);
            var thread = new Thread(runnable.Run);
            thread.Start();

            var listener = new MyKeepFirstPerStmtListener();
            epService.StatementCreate += (sender, args) => args.Statement.Events += listener.Update;

            // deploy
            var buf = new StringWriter();
            for (var i = 0; i < NUM_STMTS; i++)
            {
                buf.Write("select * from SupportBean;");
            }

            var deploymentResult = epService.EPAdministrator.DeploymentAdmin.ParseDeploy(buf.ToString());

            // wait for some deliveries
            Thread.Sleep(1000);

            // undeploy
            epService.EPAdministrator.DeploymentAdmin.Undeploy(deploymentResult.DeploymentId);

            // cooldown
            Thread.Sleep(500);

            runnable.IsShutdown = true;
            thread.Join();

            Assert.IsNull(runnable.Throwable);
            Assert.AreEqual(NUM_STMTS, listener.FirstLastPerStmt.Count);

            // all first events should be the same
            UniformPair<EventBean> reference = listener.FirstLastPerStmt.Values.First();
            Assert.IsNotNull(reference.First);
            Assert.IsNotNull(reference.Second);
            Assert.AreNotSame(reference.First, reference.Second);
            foreach (UniformPair<EventBean> other in listener.FirstLastPerStmt.Values)
            {
                Assert.AreSame(reference.First, other.First);
                Assert.AreSame(reference.Second, other.Second);
            }
        }

#pragma warning disable 612
        private sealed class MyKeepFirstPerStmtListener : StatementAwareUpdateListener
#pragma warning restore 612
        {
            public IDictionary<EPStatement, UniformPair<EventBean>> FirstLastPerStmt { get; } =
                new Dictionary<EPStatement, UniformPair<EventBean>>();

            public void Update(
                EventBean[] newEvents,
                EventBean[] oldEvents,
                EPStatement statement,
                EPServiceProvider svcProvider)
            {
                var pair = FirstLastPerStmt.Get(statement);
                if (pair == null)
                {
                    FirstLastPerStmt.Put(statement, new UniformPair<EventBean>(newEvents[0], null));
                }
                else
                {
                    pair.Second = newEvents[0];
                }
            }

            public void Update(object sender, UpdateEventArgs e)
            {
                Update(e.NewEvents, e.OldEvents, e.Statement, e.ServiceProvider);
            }
        }

        private sealed class MySendRunnable
        {
            private readonly EPServiceProvider _engine;
            private int _current;

            public MySendRunnable(EPServiceProvider engine)
            {
                _engine = engine;
            }

            public bool IsShutdown { get; set; }

            public Exception Throwable { get; private set; }

            public void Run()
            {
                try
                {
                    while (!IsShutdown)
                    {
                        _engine.EPRuntime.SendEvent(new SupportBean(null, _current++));
                    }
                }
                catch (Exception ex)
                {
                    Throwable = ex;
                }
            }
        }
    }
} // end of namespace
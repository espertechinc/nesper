///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.client.SupportCompileDeployUtil;

namespace com.espertech.esper.regressionlib.suite.multithread
{
    /// <summary>
    ///     Test for multithread-safety for atomic module deployment.
    /// </summary>
    public class MultithreadDeployAtomic : RegressionExecution
    {
        private const int NUM_STMTS = 100;

        public void Run(RegressionEnvironment env)
        {
            var runnable = new MySendRunnable(env.Runtime);
            var thread = new Thread(runnable.Run);
            thread.Name = "MultithreadDeployAtomic-" + typeof(MultithreadDeployAtomic).FullName;
            thread.Start();

            var listener = new MyKeepFirstPerStmtListener();
            env.Deployment.AddDeploymentStateListener(
                new ProxyDeploymentStateListener {
                    ProcOnDeployment = @event => {
                        foreach (var stmt in @event.Statements) {
                            stmt.AddListener(listener);
                        }
                    },

                    ProcOnUndeployment = @event => { }
                });

            // deploy
            var buf = new StringBuilder();
            for (var i = 0; i < NUM_STMTS; i++) {
                buf.Append("select * from SupportBean;");
            }

            var deploymentResult = CompileDeploy(buf.ToString(), env.Runtime, env.Configuration);

            // wait for some deliveries
            ThreadSleep(1000);

            // undeploy
            env.Undeploy(deploymentResult.DeploymentId);

            // cooldown
            ThreadSleep(500);

            runnable.Shutdown = true;
            ThreadJoin(thread);

            Assert.IsNull(runnable.Exception);
            Assert.AreEqual(NUM_STMTS, listener.FirstLastPerStmt.Count);

            // all first events should be the same
            var reference = listener.FirstLastPerStmt.Values.First();
            Assert.IsNotNull(reference.First);
            Assert.IsNotNull(reference.Second);
            Assert.AreNotSame(reference.First, reference.Second);
            foreach (var other in listener.FirstLastPerStmt.Values) {
                Assert.AreSame(reference.Second, other.Second, "last event not the same");
            }

            foreach (var other in listener.FirstLastPerStmt.Values) {
                Assert.AreSame(reference.First, other.First, "first event not the same");
            }

            env.Deployment.RemoveAllDeploymentStateListeners();
        }

        private class MyKeepFirstPerStmtListener : UpdateListener
        {
            public IDictionary<EPStatement, UniformPair<EventBean>> FirstLastPerStmt { get; } =
                new Dictionary<EPStatement, UniformPair<EventBean>>();

            public void Update(
                object sender,
                UpdateEventArgs eventArgs)
            {
                lock (this) {
                    var statement = eventArgs.Statement;
                    var pair = FirstLastPerStmt.Get(statement);
                    if (pair == null) {
                        FirstLastPerStmt.Put(statement, new UniformPair<EventBean>(eventArgs.NewEvents[0], null));
                    }
                    else {
                        pair.Second = eventArgs.NewEvents[0];
                    }
                }
            }
        }

        private class MySendRunnable : IRunnable
        {
            private readonly EPRuntime runtime;
            private int current;
            private bool shutdown;

            public MySendRunnable(EPRuntime runtime)
            {
                this.runtime = runtime;
            }

            public bool Shutdown {
                set => shutdown = value;
            }

            public Exception Exception { get; private set; }

            public void Run()
            {
                try {
                    while (!shutdown) {
                        runtime.EventService.SendEventBean(new SupportBean(null, current++), "SupportBean");
                        Thread.Sleep(50);
                    }
                }
                catch (Exception t) {
                    Exception = t;
                }
            }
        }
    }
} // end of namespace
///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeUnmatchedListener
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithSendEvent(execs);
            WithCreateStatement(execs);
            WithInsertInto(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithInsertInto(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeUnmatchedInsertInto());
            return execs;
        }

        public static IList<RegressionExecution> WithCreateStatement(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeUnmatchedCreateStatement());
            return execs;
        }

        public static IList<RegressionExecution> WithSendEvent(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ClientRuntimeUnmatchedSendEvent());
            return execs;
        }

        private static SupportBean SendEvent(
            RegressionEnvironment env,
            string theString)
        {
            var bean = new SupportBean();
            bean.TheString = theString;
            env.SendEventBean(bean);
            return bean;
        }

        internal class ClientRuntimeUnmatchedSendEvent : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var compiled = env.Compile("@name('s0') select * from SupportBean");
                var listener = new MyUnmatchedListener();
                env.EventService.UnmatchedListener = listener.Update;

                // no statement, should be unmatched
                var theEvent = SendEvent(env, "E1");
                Assert.AreEqual(1, listener.Received.Count);
                Assert.AreSame(theEvent, listener.Received[0].Underlying);
                listener.Reset();

                // no unmatched listener
                env.EventService.UnmatchedListener = null;
                SendEvent(env, "E1");
                Assert.AreEqual(0, listener.Received.Count);

                // create statement and re-register unmatched listener
                env.Deploy(compiled);
                env.EventService.UnmatchedListener = listener.Update;
                SendEvent(env, "E1");
                Assert.AreEqual(0, listener.Received.Count);

                // stop statement
                env.UndeployModuleContaining("s0");
                theEvent = SendEvent(env, "E1");
                Assert.AreEqual(1, listener.Received.Count);
                Assert.AreSame(theEvent, listener.Received[0].Underlying);
                listener.Reset();

                // start statement
                env.Deploy(compiled);
                SendEvent(env, "E1");
                Assert.AreEqual(0, listener.Received.Count);

                // destroy statement
                env.UndeployModuleContaining("s0");
                theEvent = SendEvent(env, "E1");
                Assert.AreEqual(1, listener.Received.Count);
                Assert.AreSame(theEvent, listener.Received[0].Underlying);
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientRuntimeUnmatchedCreateStatement : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new UnmatchListenerCreateStmt(env);
                env.EventService.UnmatchedListener = listener.Update;

                // no statement, should be unmatched
                SendEvent(env, "E1");
                Assert.AreEqual(1, listener.Received.Count);
                listener.Reset();

                SendEvent(env, "E1");
                Assert.AreEqual(0, listener.Received.Count);

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        internal class ClientRuntimeUnmatchedInsertInto : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new MyUnmatchedListener();
                env.EventService.UnmatchedListener = listener.Update;

                // create insert into
                env.CompileDeploy("@name('s0') insert into MyEvent select TheString from SupportBean");

                // no statement, should be unmatched
                SendEvent(env, "E1");
                Assert.AreEqual(1, listener.Received.Count);
                Assert.AreEqual("E1", listener.Received[0].Get("TheString"));
                listener.Reset();

                // stop insert into, now SupportBean itself is unmatched
                env.UndeployModuleContaining("s0");
                var theEvent = SendEvent(env, "E2");
                Assert.AreEqual(1, listener.Received.Count);
                Assert.AreSame(theEvent, listener.Received[0].Underlying);
                listener.Reset();

                // start insert-into
                SendEvent(env, "E3");
                Assert.AreEqual(1, listener.Received.Count);
                Assert.AreEqual("E3", listener.Received[0].Get("TheString"));
                listener.Reset();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        public class MyUnmatchedListener
        {
            internal MyUnmatchedListener()
            {
                Received = new List<EventBean>();
            }

            public IList<EventBean> Received { get; }

            public void Update(EventBean theEvent)
            {
                Received.Add(theEvent);
            }

            public void Reset()
            {
                Received.Clear();
            }
        }

        public class UnmatchListenerCreateStmt
        {
            private readonly RegressionEnvironment env;

            internal UnmatchListenerCreateStmt(RegressionEnvironment env)
            {
                this.env = env;
                Received = new List<EventBean>();
            }

            public IList<EventBean> Received { get; }

            public void Update(EventBean theEvent)
            {
                Received.Add(theEvent);
                env.CompileDeploy("select * from SupportBean");
            }

            public void Reset()
            {
                Received.Clear();
            }
        }
    }
} // end of namespace
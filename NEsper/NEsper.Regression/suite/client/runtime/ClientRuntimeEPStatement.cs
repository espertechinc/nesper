///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.context;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.function;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.runtime.client;
using com.espertech.esper.runtime.client.scopetest;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.client.runtime
{
    public class ClientRuntimeEPStatement
    {
        public static IList<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            execs.Add(new ClientRuntimeEPStatementListenerWReplay());
            execs.Add(new ClientRuntimeEPStatementAlreadyDestroyed());
            return execs;
        }

        private static void TryInvalid(
            EPStatement stmt,
            Consumer<EPStatement> consumer)
        {
            try {
                consumer.Invoke(stmt);
                Assert.Fail();
            }
            catch (IllegalStateException ex) {
                Assert.AreEqual(ex.Message, "Statement has already been undeployed");
            }
        }

        internal class ClientRuntimeEPStatementListenerWReplay : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from SupportBean#length(2)");
                var listener = new SupportUpdateListener();

                // test empty statement
                env.Statement("s0").AddListenerWithReplay(listener);
                Assert.IsTrue(listener.IsInvoked);
                Assert.AreEqual(1, listener.NewDataList.Count);
                Assert.IsNull(listener.NewDataList[0]);
                listener.Reset();

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("TheString"));
                env.UndeployAll();
                listener.Reset();

                // test 1 event
                env.CompileDeploy("@name('s0') select * from SupportBean#length(2)");
                env.SendEventBean(new SupportBean("E1", 1));
                env.Statement("s0").AddListenerWithReplay(listener);
                Assert.AreEqual("E1", listener.AssertOneGetNewAndReset().Get("TheString"));
                env.UndeployAll();
                listener.Reset();

                // test 2 events
                env.CompileDeploy("@name('s0') select * from SupportBean#length(2)");
                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 1));
                env.Statement("s0").AddListenerWithReplay(listener);
                EPAssertionUtil.AssertPropsPerRow(
                    listener.LastNewData,
                    new[] {"TheString"},
                    new[] {
                        new object[] {"E1"},
                        new object[] {"E2"}
                    });
                var stmt = env.Statement("s0");
                env.UndeployAll();
                listener.Reset();
            }
        }

        internal class ClientRuntimeEPStatementAlreadyDestroyed : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                env.CompileDeploy("@name('s0') select * from SupportBean");
                var statement = env.Statement("s0");
                env.UndeployAll();
                Assert.IsTrue(statement.IsDestroyed);
                TryInvalid(statement, stmt => stmt.GetEnumerator());
                TryInvalid(statement, stmt => stmt.GetEnumerator(new ContextPartitionSelectorAll()));
                TryInvalid(statement, stmt => stmt.GetSafeEnumerator());
                TryInvalid(statement, stmt => stmt.GetSafeEnumerator(new ContextPartitionSelectorAll()));
                TryInvalid(statement, stmt => stmt.AddListenerWithReplay(new SupportUpdateListener()));
                TryInvalid(statement, stmt => stmt.AddListener(new SupportUpdateListener()));
                TryInvalid(statement, stmt => stmt.SetSubscriber(this));
                TryInvalid(statement, stmt => stmt.SetSubscriber(this, "somemethod"));
            }
        }
    }
} // end of namespace
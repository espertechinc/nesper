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
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.support.context.SupportContextListenUtil;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextAdminListen
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextAdminListenInitTerm());
            execs.Add(new ContextAdminListenHash());
            execs.Add(new ContextAdminListenCategory());
            execs.Add(new ContextAdminListenNested());
            execs.Add(new ContextAddRemoveListener());
            execs.Add(new ContextAdminPartitionAddRemoveListener());
            execs.Add(new ContextAdminListenMultipleStatements());
            return execs;
        }

        private static void RunAssertionPartitionAddRemoveListener(
            RegressionEnvironment env,
            string eplContext,
            string contextName)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@name('ctx') " + eplContext, path);
            env.CompileDeploy("@name('s0') context " + contextName + " select count(*) from SupportBean", path);
            var api = env.Runtime.ContextPartitionService;
            var depIdCtx = env.DeploymentId("ctx");

            var listeners = new SupportContextListener[3];
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i] = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextPartitionStateListener(
                    depIdCtx,
                    contextName,
                    listeners[i]);
            }

            env.SendEventBean(new SupportBean_S0(1));
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i]
                    .AssertAndReset(
                        EventPartitionInitTerm(
                            depIdCtx,
                            contextName,
                            typeof(ContextStateEventContextPartitionAllocated)));
            }

            api.RemoveContextPartitionStateListener(depIdCtx, contextName, listeners[0]);
            env.SendEventBean(new SupportBean_S1(1));
            listeners[0].AssertNotInvoked();
            listeners[1]
                .AssertAndReset(
                    EventPartitionInitTerm(
                        depIdCtx,
                        contextName,
                        typeof(ContextStateEventContextPartitionDeallocated)));
            listeners[2]
                .AssertAndReset(
                    EventPartitionInitTerm(
                        depIdCtx,
                        contextName,
                        typeof(ContextStateEventContextPartitionDeallocated)));

            var enumerator = api.GetContextPartitionStateListeners(depIdCtx, contextName);
            Assert.AreSame(listeners[1], enumerator.Advance());
            Assert.AreSame(listeners[2], enumerator.Advance());
            Assert.IsFalse(enumerator.MoveNext());

            api.RemoveContextPartitionStateListeners(depIdCtx, contextName);
            Assert.IsFalse(api.GetContextPartitionStateListeners(depIdCtx, contextName).MoveNext());

            env.SendEventBean(new SupportBean_S0(2));
            env.SendEventBean(new SupportBean_S1(2));
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i].AssertNotInvoked();
            }

            env.UndeployAll();
        }

        internal class ContextAdminListenMultipleStatements : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var name = "MyContextStartS0EndS1";
                var path = new RegressionPath();
                var contextEPL =
                    "@name('ctx') create context MyContextStartS0EndS1 start SupportBean_S0 as S0 end SupportBean_S1";
                env.CompileDeploy(contextEPL, path);
                var depIdCtx = env.DeploymentId("ctx");

                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextPartitionStateListener(
                    depIdCtx,
                    "MyContextStartS0EndS1",
                    listener);
                env.CompileDeploy("@name('a') context MyContextStartS0EndS1 select count(*) from SupportBean", path);
                var depIdA = env.DeploymentId("a");
                env.CompileDeploy("@name('b') context MyContextStartS0EndS1 select count(*) from SupportBean_S0", path);
                var depIdB = env.DeploymentId("b");

                listener.AssertAndReset(
                    EventContextWStmt(depIdCtx, name, typeof(ContextStateEventContextStatementAdded), depIdA, "a"),
                    EventContext(depIdCtx, name, typeof(ContextStateEventContextActivated)),
                    EventContextWStmt(depIdCtx, name, typeof(ContextStateEventContextStatementAdded), depIdB, "b"));

                env.SendEventBean(new SupportBean_S0(1));
                listener.AssertAndReset(
                    EventPartitionInitTerm(depIdCtx, name, typeof(ContextStateEventContextPartitionAllocated)));

                env.UndeployAll();
            }
        }

        internal class ContextAdminPartitionAddRemoveListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContextStartEnd start SupportBean_S0 as S0 end SupportBean_S1";
                RunAssertionPartitionAddRemoveListener(env, epl, "MyContextStartEnd");

                epl = "create context MyContextStartEndWithNeverEnding " +
                      "context NeverEndingStory start @now, " +
                      "context ABSession start SupportBean_S0 as S0 end SupportBean_S1";
                RunAssertionPartitionAddRemoveListener(env, epl, "MyContextStartEndWithNeverEnding");
            }
        }

        internal class ContextAddRemoveListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var api = env.Runtime.ContextPartitionService;

                var epl = "@name('ctx') create context MyContext start SupportBean_S0 as S0 end SupportBean_S1";
                var listeners = new SupportContextListener[3];
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i] = new SupportContextListener(env);
                    env.Runtime.ContextPartitionService.AddContextStateListener(listeners[i]);
                }

                env.CompileDeploy(epl);
                var depIdCtx = env.DeploymentId("ctx");
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i]
                        .AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextCreated)));
                }

                api.RemoveContextStateListener(listeners[0]);
                env.UndeployModuleContaining("ctx");
                listeners[0].AssertNotInvoked();
                listeners[1]
                    .AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextDestroyed)));
                listeners[2]
                    .AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextDestroyed)));

                var enumerator = api.ContextStateListeners;
                Assert.AreSame(listeners[1], enumerator.Advance());
                Assert.AreSame(listeners[2], enumerator.Advance());
                Assert.IsFalse(enumerator.MoveNext());

                api.RemoveContextStateListeners();
                Assert.IsFalse(api.ContextStateListeners.MoveNext());

                env.CompileDeploy(epl);
                env.UndeployAll();
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i].AssertNotInvoked();
                }
            }
        }

        internal class ContextAdminListenNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context MyContext " +
                    "context ContextPosNeg group by IntPrimitive > 0 as pos, group by IntPrimitive < 0 as neg from SupportBean, " +
                    "context ByString partition by TheString from SupportBean",
                    path);
                var depIdCtx = env.DeploymentId("ctx");
                listener.AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextCreated)));

                env.CompileDeploy("@name('s0') context MyContext select count(*) from SupportBean", path);
                var depIdStmt = env.DeploymentId("s0");
                listener.AssertAndReset(
                    EventContextWStmt(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextStatementAdded),
                        depIdStmt,
                        "s0"),
                    EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextActivated)));

                env.SendEventBean(new SupportBean("E1", 1));
                var allocated = listener.GetAllocatedEvents();
                Assert.AreEqual(1, allocated.Count);
                var nested = (ContextPartitionIdentifierNested) allocated[0].Identifier;
                EPAssertionUtil.AssertEqualsExactOrder(
                    new [] { "E1" },
                    ((ContextPartitionIdentifierPartitioned) nested.Identifiers[1]).Keys);
                Assert.AreEqual(1, listener.GetAndReset().Count);

                env.UndeployModuleContaining("s0");
                listener.AssertAndReset(
                    EventContextWStmt(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextStatementRemoved),
                        depIdStmt,
                        "s0"),
                    EventPartitionInitTerm(depIdCtx, "MyContext", typeof(ContextStateEventContextPartitionDeallocated)),
                    EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextDeactivated)));

                env.UndeployModuleContaining("ctx");
                listener.AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextDestroyed)));

                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }
        }

        internal class ContextAdminListenCategory : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context MyContext group by IntPrimitive > 0 as pos, group by IntPrimitive < 0 as neg from SupportBean",
                    path);
                env.CompileDeploy("@name('s0') context MyContext select count(*) from SupportBean", path);

                var allocated = listener.GetAllocatedEvents();
                Assert.AreEqual(2, allocated.Count);
                Assert.AreEqual("neg", ((ContextPartitionIdentifierCategory) allocated[1].Identifier).Label);
                listener.GetAndReset();

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("ctx");
                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }
        }

        internal class ContextAdminListenHash : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var epl =
                    "@name('ctx') create context MyContext coalesce by consistent_hash_crc32(TheString) from SupportBean granularity 2 preallocate;\n" +
                    "@name('s0') context MyContext select count(*) from SupportBean;\n";
                env.CompileDeploy(epl);
                var deploymentId = env.DeploymentId("s0");

                var allocated = listener.GetAllocatedEvents();
                Assert.AreEqual(2, allocated.Count);
                Assert.AreEqual(1, ((ContextPartitionIdentifierHash) allocated[1].Identifier).Hash);
                listener.GetAndReset();

                env.UndeployAll();

                listener.AssertAndReset(
                    EventContextWStmt(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextStatementRemoved),
                        deploymentId,
                        "s0"),
                    EventPartitionInitTerm(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextPartitionDeallocated)),
                    EventPartitionInitTerm(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextPartitionDeallocated)),
                    EventContext(deploymentId, "MyContext", typeof(ContextStateEventContextDeactivated)),
                    EventContext(deploymentId, "MyContext", typeof(ContextStateEventContextDestroyed)));

                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }
        }

        internal class ContextAdminListenInitTerm : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') create context MyContext start SupportBean_S0 as S0 end SupportBean_S1",
                    path);
                var depIdCtx = env.DeploymentId("ctx");
                listener.AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextCreated)));

                env.CompileDeploy("@name('s0') context MyContext select count(*) from SupportBean", path);
                var depIdStmt = env.DeploymentId("s0");
                listener.AssertAndReset(
                    EventContextWStmt(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextStatementAdded),
                        depIdStmt,
                        "s0"),
                    EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextActivated)));

                env.SendEventBean(new SupportBean_S0(1));
                listener.AssertAndReset(
                    EventPartitionInitTerm(depIdCtx, "MyContext", typeof(ContextStateEventContextPartitionAllocated)));

                env.SendEventBean(new SupportBean_S1(1));
                listener.AssertAndReset(
                    EventPartitionInitTerm(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextPartitionDeallocated)));

                env.UndeployModuleContaining("s0");
                listener.AssertAndReset(
                    EventContextWStmt(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextStatementRemoved),
                        depIdStmt,
                        "s0"),
                    EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextDeactivated)));

                env.UndeployModuleContaining("ctx");
                listener.AssertAndReset(EventContext(depIdCtx, "MyContext", typeof(ContextStateEventContextDestroyed)));

                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }
        }
    }
} // end of namespace
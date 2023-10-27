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

using static com.espertech.esper.regressionlib.support.context.SupportContextListenUtil;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextAdminListen
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithMinListenInitTerm(execs);
            WithMinListenHash(execs);
            WithMinListenCategory(execs);
            WithMinListenNested(execs);
            WithAddRemoveListener(execs);
            WithMinPartitionAddRemoveListener(execs);
            WithMinListenMultipleStatements(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithMinListenMultipleStatements(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAdminListenMultipleStatements());
            return execs;
        }

        public static IList<RegressionExecution> WithMinPartitionAddRemoveListener(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAdminPartitionAddRemoveListener());
            return execs;
        }

        public static IList<RegressionExecution> WithAddRemoveListener(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAddRemoveListener());
            return execs;
        }

        public static IList<RegressionExecution> WithMinListenNested(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAdminListenNested());
            return execs;
        }

        public static IList<RegressionExecution> WithMinListenCategory(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAdminListenCategory());
            return execs;
        }

        public static IList<RegressionExecution> WithMinListenHash(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAdminListenHash());
            return execs;
        }

        public static IList<RegressionExecution> WithMinListenInitTerm(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextAdminListenInitTerm());
            return execs;
        }

        private class ContextAdminListenMultipleStatements : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var name = "MyContextStartS0EndS1";
                var path = new RegressionPath();
                var contextEPL =
                    "@name('ctx') @public create context MyContextStartS0EndS1 start SupportBean_S0 as s0 end SupportBean_S1";
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
                    SupportContextListenUtil.EventContextWStmt(
                        depIdCtx,
                        name,
                        typeof(ContextStateEventContextStatementAdded),
                        depIdA,
                        "a"),
                    SupportContextListenUtil.EventContext(depIdCtx, name, typeof(ContextStateEventContextActivated)),
                    SupportContextListenUtil.EventContextWStmt(
                        depIdCtx,
                        name,
                        typeof(ContextStateEventContextStatementAdded),
                        depIdB,
                        "b"));

                env.SendEventBean(new SupportBean_S0(1));
                listener.AssertAndReset(
                    SupportContextListenUtil.EventPartitionInitTerm(
                        depIdCtx,
                        name,
                        typeof(ContextStateEventContextPartitionAllocated)));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ContextAdminPartitionAddRemoveListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "create context MyContextStartEnd start SupportBean_S0 as s0 end SupportBean_S1";
                RunAssertionPartitionAddRemoveListener(env, epl, "MyContextStartEnd");

                epl = "create context MyContextStartEndWithNeverEnding " +
                      "context NeverEndingStory start @now, " +
                      "context ABSession start SupportBean_S0 as s0 end SupportBean_S1";
                RunAssertionPartitionAddRemoveListener(env, epl, "MyContextStartEndWithNeverEnding");
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private static void RunAssertionPartitionAddRemoveListener(
            RegressionEnvironment env,
            string eplContext,
            string contextName)
        {
            var path = new RegressionPath();
            env.CompileDeploy("@name('ctx') @public " + eplContext, path);
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
                        SupportContextListenUtil.EventPartitionInitTerm(
                            depIdCtx,
                            contextName,
                            typeof(ContextStateEventContextPartitionAllocated)));
            }

            api.RemoveContextPartitionStateListener(depIdCtx, contextName, listeners[0]);
            env.SendEventBean(new SupportBean_S1(1));
            listeners[0].AssertNotInvoked();
            listeners[1]
                .AssertAndReset(
                    SupportContextListenUtil.EventPartitionInitTerm(
                        depIdCtx,
                        contextName,
                        typeof(ContextStateEventContextPartitionDeallocated)));
            listeners[2]
                .AssertAndReset(
                    SupportContextListenUtil.EventPartitionInitTerm(
                        depIdCtx,
                        contextName,
                        typeof(ContextStateEventContextPartitionDeallocated)));

            var it = api.GetContextPartitionStateListeners(depIdCtx, contextName);
            Assert.AreSame(listeners[1], it.Advance());
            Assert.AreSame(listeners[2], it.Advance());
            Assert.IsFalse(it.MoveNext());

            api.RemoveContextPartitionStateListeners(depIdCtx, contextName);
            Assert.IsFalse(api.GetContextPartitionStateListeners(depIdCtx, contextName).MoveNext());

            env.SendEventBean(new SupportBean_S0(2));
            env.SendEventBean(new SupportBean_S1(2));
            for (var i = 0; i < listeners.Length; i++) {
                listeners[i].AssertNotInvoked();
            }

            env.UndeployAll();
        }

        private class ContextAddRemoveListener : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var api = env.Runtime.ContextPartitionService;

                var epl = "@name('ctx') @public create context MyContext start SupportBean_S0 as s0 end SupportBean_S1";
                var listeners = new SupportContextListener[3];
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i] = new SupportContextListener(env);
                    env.Runtime.ContextPartitionService.AddContextStateListener(listeners[i]);
                }

                env.CompileDeploy(epl);
                var depIdCtx = env.DeploymentId("ctx");
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i]
                        .AssertAndReset(
                            SupportContextListenUtil.EventContext(
                                depIdCtx,
                                "MyContext",
                                typeof(ContextStateEventContextCreated)));
                }

                api.RemoveContextStateListener(listeners[0]);
                env.UndeployModuleContaining("ctx");
                listeners[0].AssertNotInvoked();
                listeners[1]
                    .AssertAndReset(
                        SupportContextListenUtil.EventContext(
                            depIdCtx,
                            "MyContext",
                            typeof(ContextStateEventContextDestroyed)));
                listeners[2]
                    .AssertAndReset(
                        SupportContextListenUtil.EventContext(
                            depIdCtx,
                            "MyContext",
                            typeof(ContextStateEventContextDestroyed)));

                var it = api.ContextStateListeners;
                Assert.AreSame(listeners[1], it.Advance());
                Assert.AreSame(listeners[2], it.Advance());
                Assert.IsFalse(it.MoveNext());

                api.RemoveContextStateListeners();
                Assert.IsFalse(api.ContextStateListeners.MoveNext());

                env.CompileDeploy(epl);
                env.UndeployAll();
                for (var i = 0; i < listeners.Length; i++) {
                    listeners[i].AssertNotInvoked();
                }
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS, RegressionFlag.OBSERVEROPS);
            }
        }

        private class ContextAdminListenNested : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') @public create context MyContext " +
                    "context ContextPosNeg group by IntPrimitive > 0 as pos, group by IntPrimitive < 0 as neg from SupportBean, " +
                    "context ByString partition by TheString from SupportBean",
                    path);
                var depIdCtx = env.DeploymentId("ctx");
                listener.AssertAndReset(
                    SupportContextListenUtil.EventContext(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextCreated)));

                env.CompileDeploy("@name('s0') context MyContext select count(*) from SupportBean", path);
                var depIdStmt = env.DeploymentId("s0");
                listener.AssertAndReset(
                    SupportContextListenUtil.EventContextWStmt(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextStatementAdded),
                        depIdStmt,
                        "s0"),
                    SupportContextListenUtil.EventContext(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextActivated)));

                env.SendEventBean(new SupportBean("E1", 1));
                var allocated = listener.AllocatedEvents;
                Assert.AreEqual(1, allocated.Count);
                var nested = (ContextPartitionIdentifierNested)allocated[0].Identifier;
                EPAssertionUtil.AssertEqualsExactOrder(
                    "E1".SplitCsv(),
                    ((ContextPartitionIdentifierPartitioned)nested.Identifiers[1]).Keys);
                Assert.AreEqual(1, listener.GetAndReset().Count);

                env.UndeployModuleContaining("s0");
                listener.AssertAndReset(
                    SupportContextListenUtil.EventContextWStmt(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextStatementRemoved),
                        depIdStmt,
                        "s0"),
                    SupportContextListenUtil.EventPartitionInitTerm(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextPartitionDeallocated)),
                    SupportContextListenUtil.EventContext(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextDeactivated)));

                env.UndeployModuleContaining("ctx");
                listener.AssertAndReset(
                    SupportContextListenUtil.EventContext(
                        depIdCtx,
                        "MyContext",
                        typeof(ContextStateEventContextDestroyed)));

                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ContextAdminListenCategory : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') @public create context MyContext group by IntPrimitive > 0 as pos, group by IntPrimitive < 0 as neg from SupportBean",
                    path);
                env.CompileDeploy("@name('s0') context MyContext select count(*) from SupportBean", path);

                var allocated = listener.AllocatedEvents;
                Assert.AreEqual(2, allocated.Count);
                Assert.AreEqual("neg", ((ContextPartitionIdentifierCategory)allocated[1].Identifier).Label);
                listener.GetAndReset();

                env.UndeployModuleContaining("s0");
                env.UndeployModuleContaining("ctx");
                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.RUNTIMEOPS);
            }
        }

        private class ContextAdminListenHash : RegressionExecution
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

                var allocated = listener.AllocatedEvents;
                Assert.AreEqual(2, allocated.Count);
                Assert.AreEqual(1, ((ContextPartitionIdentifierHash)allocated[1].Identifier).Hash);
                listener.GetAndReset();

                env.UndeployAll();

                listener.AssertAndReset(
                    SupportContextListenUtil.EventContextWStmt(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextStatementRemoved),
                        deploymentId,
                        "s0"),
                    SupportContextListenUtil.EventPartitionInitTerm(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextPartitionDeallocated)),
                    SupportContextListenUtil.EventPartitionInitTerm(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextPartitionDeallocated)),
                    SupportContextListenUtil.EventContext(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextDeactivated)),
                    SupportContextListenUtil.EventContext(
                        deploymentId,
                        "MyContext",
                        typeof(ContextStateEventContextDestroyed)));

                env.Runtime.ContextPartitionService.RemoveContextStateListeners();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class ContextAdminListenInitTerm : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var listener = new SupportContextListener(env);
                env.Runtime.ContextPartitionService.AddContextStateListener(listener);

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('ctx') @public create context MyContext start SupportBean_S0 as s0 end SupportBean_S1",
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

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }
    }
} // end of namespace
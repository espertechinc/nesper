///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.extend.vdw;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

using static com.espertech.esper.regressionlib.framework.SupportMessageAssertUtil;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextLifecycle
    {
        public static IList<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            execs.Add(new ContextLifecycleSplitStream());
            execs.Add(new ContextLifecycleVirtualDataWindow());
            execs.Add(new ContextLifecycleNWOtherContextOnExpr());
            execs.Add(new ContextLifecycleInvalid());
            execs.Add(new ContextLifecycleSimple());
            return execs;
        }

        internal class ContextLifecycleSplitStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplOne = "create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                             "@Name('out') context CtxSegmentedByTarget on SupportBean insert into NewSupportBean select * where IntPrimitive = 100;";
                env.CompileDeploy(eplOne, path);
                env.CompileDeploy("@Name('s0') select * from NewSupportBean", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                Assert.IsFalse(env.Listener("s0").GetAndClearIsInvoked());

                env.SendEventBean(new SupportBean("E1", 100));
                Assert.IsTrue(env.Listener("s0").GetAndClearIsInvoked());
                env.UndeployAll();
                path.Clear();

                // test with subquery
                var fields = "mymax".SplitCsv();
                var eplTwo = "create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                             "context CtxSegmentedByTarget create window NewEvent#unique(TheString) as SupportBean;" +
                             "@Name('out') context CtxSegmentedByTarget on SupportBean " +
                             "insert into NewEvent select * where IntPrimitive = 100 " +
                             "insert into NewEventTwo select (select max(IntPrimitive) from NewEvent) as mymax  " +
                             "output all;";
                env.CompileDeploy(eplTwo, path);
                env.CompileDeploy("@Name('s0') select * from NewEventTwo", path).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.SendEventBean(new SupportBean("E1", 100));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {null});

                env.SendEventBean(new SupportBean("E1", 0));
                EPAssertionUtil.AssertProps(
                    env.Listener("s0").AssertOneGetNewAndReset(),
                    fields,
                    new object[] {100});

                env.UndeployAll();
            }
        }

        internal class ContextLifecycleVirtualDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportVirtualDWFactory.Windows.Clear();
                SupportVirtualDWFactory.IsDestroyed = false;

                var path = new RegressionPath();
                env.CompileDeploy("create context CtxSegmented as partition by TheString from SupportBean", path);
                env.CompileDeploy("context CtxSegmented create window TestVDWWindow.test:vdw() as SupportBean", path);
                env.CompileDeploy("select * from TestVDWWindow", path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                Assert.AreEqual(
                    2,
                    SupportVirtualDWFactory.Windows.Count); // Independent windows for independent contexts

                env.UndeployAll();
                foreach (var vdw in SupportVirtualDWFactory.Windows) {
                    Assert.IsTrue(vdw.IsDestroyed);
                }

                Assert.IsTrue(SupportVirtualDWFactory.IsDestroyed);
            }
        }

        internal class ContextLifecycleNWOtherContextOnExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy("create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)", path);
                env.CompileDeploy("create context TenToFive as start (0, 10, *, *, *) end (0, 17, *, *, *)", path);

                // Trigger not in context
                env.CompileDeploy(
                    "@Name('createwindow') context NineToFive create window MyWindow#keepall as SupportBean",
                    path);
                TryInvalidCompile(
                    env,
                    path,
                    "on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1",
                    "Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please declare the same context name");

                // Trigger in different context
                TryInvalidCompile(
                    env,
                    path,
                    "context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1",
                    "Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please use the same context instead");

                // Named window not in context, trigger in different context
                env.UndeployModuleContaining("createwindow");
                env.CompileDeploy("create window MyWindowTwo#keepall as SupportBean", path);
                TryInvalidCompile(
                    env,
                    path,
                    "context TenToFive on SupportBean_S0 s0 merge MyWindowTwo mw when matched then update set IntPrimitive = 1",
                    "Cannot create on-trigger expression: Named window 'MyWindowTwo' was declared with context 'null', please use the same context instead");

                env.UndeployAll();
            }
        }

        internal class ContextLifecycleSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl = "@Name('context') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                // create and destroy
                env.CompileDeploy(epl);
                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployModuleContaining("context");
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));

                // create context, create statement, destroy statement, destroy context
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));

                env.CompileDeploy("@Name('s0') context NineToFive select * from SupportBean", path);
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployModuleContaining("context");
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));

                // create same context
                path.Clear();
                env.CompileDeploy(epl, path);
                env.CompileDeploy("@Name('C') context NineToFive select * from SupportBean", path);
                env.CompileDeploy("@Name('D') context NineToFive select * from SupportBean", path);

                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployAll();
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env));

                env.UndeployAll();
            }
        }

        internal class ContextLifecycleInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // same context twice
                var eplCreateCtx =
                    "@Name('ctx') create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
                env.CompileDeploy(eplCreateCtx, path);
                TryInvalidCompile(env, path, eplCreateCtx, "Context by name 'NineToFive' already exists");

                // still in use
                env.CompileDeploy("context NineToFive select * from SupportBean", path);
                try {
                    env.Deployment.Undeploy(env.DeploymentId("ctx"));
                    Assert.Fail();
                }
                catch (EPUndeployException ex) {
                    AssertMessage(
                        ex.Message,
                        "A precondition is not satisfied: Context 'NineToFive' cannot be un-deployed as it is referenced by deployment");
                }

                // not found
                TryInvalidCompile(
                    env,
                    path,
                    "context EightToSix select * from SupportBean",
                    "Context by name 'EightToSix' could not be found");

                // test update: update is not allowed as it is processed out-of-context by eventService
                env.CompileDeploy("insert into ABCStream select * from SupportBean", path);
                env.CompileDeploy(
                    "@Name('context') create context SegmentedByAString partition by TheString from ABCStream",
                    path);
                TryInvalidCompile(
                    env,
                    path,
                    "context SegmentedByAString update istream ABCStream set IntPrimitive = (select Id from SupportBean_S0#lastevent) where IntPrimitive < 0",
                    "Update IStream is not supported in conjunction with a context");

                // context declaration for create-context
                env.CompileDeploy("create context ABC start @now end after 5 seconds", path);
                TryInvalidCompile(
                    env,
                    path,
                    "context ABC create context DEF start @now end after 5 seconds",
                    "A create-context statement cannot itself be associated to a context, please declare a nested context instead [context ABC create context DEF start @now end after 5 seconds]");

                // statement references context but there is none
                TryInvalidCompile(
                    env,
                    "select context.sb.TheString from SupportBean as sb",
                    "Failed to validate select-clause expression 'context.sb.TheString': Failed to resolve property 'context.sb.TheString' to a stream or nested property in a stream");

                env.UndeployAll();
            }
        }
    }
} // end of namespace
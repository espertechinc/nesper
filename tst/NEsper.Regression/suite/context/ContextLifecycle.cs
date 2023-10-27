///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.context;
using com.espertech.esper.regressionlib.support.extend.vdw;
using com.espertech.esper.regressionlib.support.util;
using com.espertech.esper.runtime.client;

using NUnit.Framework;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextLifecycle
    {
        public static ICollection<RegressionExecution> Executions()
        {
            var execs = new List<RegressionExecution>();
            WithSplitStream(execs);
            WithVirtualDataWindow(execs);
            WithNWOtherContextOnExpr(execs);
            WithInvalid(execs);
            WithSimple(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSimple(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextLifecycleSimple());
            return execs;
        }

        public static IList<RegressionExecution> WithInvalid(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextLifecycleInvalid());
            return execs;
        }

        public static IList<RegressionExecution> WithNWOtherContextOnExpr(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextLifecycleNWOtherContextOnExpr());
            return execs;
        }

        public static IList<RegressionExecution> WithVirtualDataWindow(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextLifecycleVirtualDataWindow());
            return execs;
        }

        public static IList<RegressionExecution> WithSplitStream(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextLifecycleSplitStream());
            return execs;
        }

        private class ContextLifecycleSplitStream : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                var eplOne = "@Public create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                             "@name('out') @public context CtxSegmentedByTarget on SupportBean insert into NewSupportBean select * where IntPrimitive = 100;";
                env.CompileDeploy(eplOne, path);
                env.CompileDeploy("@name('s0') select * from NewSupportBean", path).AddListener("s0");

                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertListenerNotInvoked("s0");

                env.SendEventBean(new SupportBean("E1", 100));
                env.AssertListenerInvoked("s0");
                env.UndeployAll();
                path.Clear();

                // test with subquery
                var fields = "mymax".SplitCsv();
                var eplTwo = "@Public create context CtxSegmentedByTarget partition by TheString from SupportBean;" +
                             "@Public context CtxSegmentedByTarget create window NewEvent#unique(TheString) as SupportBean;" +
                             "@name('out') @public context CtxSegmentedByTarget on SupportBean " +
                             "insert into NewEvent select * where IntPrimitive = 100 " +
                             "insert into NewEventTwo select (select max(IntPrimitive) from NewEvent) as mymax  " +
                             "output all;";
                env.CompileDeploy(eplTwo, path);
                env.CompileDeploy("@name('s0') select * from NewEventTwo", path).AddListener("s0");
                env.SendEventBean(new SupportBean("E1", 1));
                env.AssertPropsNew("s0", fields, new object[] { null });

                env.SendEventBean(new SupportBean("E1", 100));
                env.AssertPropsNew("s0", fields, new object[] { null });

                env.SendEventBean(new SupportBean("E1", 0));
                env.AssertPropsNew("s0", fields, new object[] { 100 });

                env.UndeployAll();
            }
        }

        private class ContextLifecycleVirtualDataWindow : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                SupportVirtualDWFactory.Windows.Clear();
                SupportVirtualDWFactory.IsDestroyed = false;

                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Public create context CtxSegmented as partition by TheString from SupportBean",
                    path);
                env.CompileDeploy(
                    "@Public context CtxSegmented create window TestVDWWindow.test:vdw() as SupportBean",
                    path);
                env.CompileDeploy("select * from TestVDWWindow", path);

                env.SendEventBean(new SupportBean("E1", 1));
                env.SendEventBean(new SupportBean("E2", 2));
                env.AssertThat(
                    () => Assert.AreEqual(
                        2,
                        SupportVirtualDWFactory.Windows.Count)); // Independent windows for independent contexts

                env.UndeployAll();
                env.AssertThat(
                    () => {
                        foreach (var vdw in SupportVirtualDWFactory.Windows) {
                            Assert.IsTrue(vdw.IsDestroyed);
                        }

                        Assert.IsTrue(SupportVirtualDWFactory.IsDestroyed);
                    });
            }
        }

        private class ContextLifecycleNWOtherContextOnExpr : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@Public create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)",
                    path);
                env.CompileDeploy(
                    "@Public create context TenToFive as start (0, 10, *, *, *) end (0, 17, *, *, *)",
                    path);

                // Trigger not in context
                env.CompileDeploy(
                    "@name('createwindow') @public context NineToFive create window MyWindow#keepall as SupportBean",
                    path);
                env.TryInvalidCompile(
                    path,
                    "on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1",
                    "Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please declare the same context name");

                // Trigger in different context
                env.TryInvalidCompile(
                    path,
                    "context TenToFive on SupportBean_S0 s0 merge MyWindow mw when matched then update set IntPrimitive = 1",
                    "Cannot create on-trigger expression: Named window 'MyWindow' was declared with context 'NineToFive', please use the same context instead");

                // Named window not in context, trigger in different context
                env.UndeployModuleContaining("createwindow");
                env.CompileDeploy("@Public create window MyWindowTwo#keepall as SupportBean", path);
                env.TryInvalidCompile(
                    path,
                    "context TenToFive on SupportBean_S0 s0 merge MyWindowTwo mw when matched then update set IntPrimitive = 1",
                    "Cannot create on-trigger expression: Named window 'MyWindowTwo' was declared without a context");

                env.UndeployAll();
            }
        }

        private class ContextLifecycleSimple : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('context') @public create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env.Runtime));

                // create and destroy
                env.CompileDeploy(epl);
                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env.Runtime));

                env.UndeployModuleContaining("context");
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));

                // create context, create statement, destroy statement, destroy context
                var path = new RegressionPath();
                env.CompileDeploy(epl, path);
                Assert.AreEqual(1, SupportContextMgmtHelper.GetContextCount(env));

                env.CompileDeploy("@name('s0') context NineToFive select * from SupportBean", path);
                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env.Runtime));

                env.UndeployModuleContaining("s0");
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env.Runtime));

                env.UndeployModuleContaining("context");
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));

                // create same context
                path.Clear();
                env.CompileDeploy(epl, path);
                env.CompileDeploy("@name('C') context NineToFive select * from SupportBean", path);
                env.CompileDeploy("@name('D') context NineToFive select * from SupportBean", path);

                Assert.AreEqual(1, SupportScheduleHelper.ScheduleCountOverall(env.Runtime));

                env.UndeployAll();
                Assert.AreEqual(0, SupportContextMgmtHelper.GetContextCount(env));
                Assert.AreEqual(0, SupportScheduleHelper.ScheduleCountOverall(env.Runtime));

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.STATICHOOK);
            }
        }

        private class ContextLifecycleInvalid : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();

                // same context twice
                var eplCreateCtx =
                    "@name('ctx') @public create context NineToFive as start (0, 9, *, *, *) end (0, 17, *, *, *)";
                env.CompileDeploy(eplCreateCtx, path);
                env.TryInvalidCompile(path, eplCreateCtx, "Context by name 'NineToFive' already exists");

                // still in use
                env.CompileDeploy("context NineToFive select * from SupportBean", path);
                try {
                    env.Deployment.Undeploy(env.DeploymentId("ctx"));
                    Assert.Fail();
                }
                catch (EPUndeployException ex) {
                    SupportMessageAssertUtil.AssertMessage(
                        ex.Message,
                        "A precondition is not satisfied: Context 'NineToFive' cannot be un-deployed as it is referenced by deployment");
                }

                // not found
                env.TryInvalidCompile(
                    path,
                    "context EightToSix select * from SupportBean",
                    "Context by name 'EightToSix' could not be found");

                // test update: update is not allowed as it is processed out-of-context by eventService
                env.CompileDeploy("@Public insert into ABCStream select * from SupportBean", path);
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByAString partition by TheString from ABCStream",
                    path);
                env.TryInvalidCompile(
                    path,
                    "context SegmentedByAString update istream ABCStream set IntPrimitive = (select Id from SupportBean_S0#lastevent) where IntPrimitive < 0",
                    "Update IStream is not supported in conjunction with a context");

                // context declaration for create-context
                env.CompileDeploy("@Public create context ABC start @now end after 5 seconds", path);
                env.TryInvalidCompile(
                    path,
                    "context ABC create context DEF start @now end after 5 seconds",
                    "A create-context statement cannot itself be associated to a context, please declare a nested context instead [context ABC create context DEF start @now end after 5 seconds]");

                // statement references context but there is none
                env.TryInvalidCompile(
                    "select context.sb.TheString from SupportBean as sb",
                    "Failed to validate select-clause expression 'context.sb.TheString': Failed to resolve property 'context.sb.TheString' to a stream or nested property in a stream");

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.INVALIDITY);
            }
        }
    }
} // end of namespace
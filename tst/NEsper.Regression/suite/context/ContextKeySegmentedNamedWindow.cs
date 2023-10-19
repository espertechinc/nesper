///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.scopetest;
using com.espertech.esper.common.@internal.support;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.regressionlib.framework;
using com.espertech.esper.regressionlib.support.bean;

namespace com.espertech.esper.regressionlib.suite.context
{
    public class ContextKeySegmentedNamedWindow
    {
        public static ICollection<RegressionExecution> Executions()
        {
            IList<RegressionExecution> execs = new List<RegressionExecution>();
            WithNamedWindowBasic(execs);
            WithNamedWindowNonPattern(execs);
            WithNamedWindowPattern(execs);
            WithNamedWindowFAF(execs);
            WithSubqueryNamedWindowIndexUnShared(execs);
            WithSubqueryNamedWindowIndexShared(execs);
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryNamedWindowIndexShared(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedSubqueryNamedWindowIndexShared());
            return execs;
        }

        public static IList<RegressionExecution> WithSubqueryNamedWindowIndexUnShared(
            IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedSubqueryNamedWindowIndexUnShared());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowFAF(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedNamedWindowFAF());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedNamedWindowPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowNonPattern(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedNamedWindowNonPattern());
            return execs;
        }

        public static IList<RegressionExecution> WithNamedWindowBasic(IList<RegressionExecution> execs = null)
        {
            execs = execs ?? new List<RegressionExecution>();
            execs.Add(new ContextKeyedNamedWindowBasic());
            return execs;
        }

        private class ContextKeyedNamedWindowFAF : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@public create context SegmentedByString partition by theString from SupportBean",
                    path);
                env.CompileDeploy(
                    "@public context SegmentedByString create window MyWindow#keepall as SupportBean",
                    path);
                env.CompileDeploy("context SegmentedByString insert into MyWindow select * from SupportBean", path);
                var compiled = env.CompileFAF("select * from MyWindow", path);

                env.SendEventBean(new SupportBean("G1", 0));

                env.Milestone(0);

                EPAssertionUtil.AssertPropsPerRow(
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled).Array,
                    "theString".SplitCsv(),
                    new object[][] { new object[] { "G1" } });

                env.SendEventBean(new SupportBean("G2", 0));

                env.Milestone(1);

                EPAssertionUtil.AssertPropsPerRowAnyOrder(
                    env.Runtime.FireAndForgetService.ExecuteQuery(compiled).Array,
                    "theString".SplitCsv(),
                    new object[][] { new object[] { "G1" }, new object[] { "G2" } });

                env.UndeployAll();
            }

            public ISet<RegressionFlag> Flags()
            {
                return Collections.Set(RegressionFlag.FIREANDFORGET);
            }
        }

        private class ContextKeyedNamedWindowBasic : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                // ESPER-663
                var epl =
                    "@Audit @Name('CTX') create context Ctx partition by grp, subGrp from SupportGroupSubgroupEvent;\n" +
                    "@Audit @Name('Window') context Ctx create window EventData#unique(type) as SupportGroupSubgroupEvent;" +
                    "@Audit @Name('Insert') context Ctx insert into EventData select * from SupportGroupSubgroupEvent;" +
                    "@Audit @Name('Test') context Ctx select irstream * from EventData;";
                env.CompileDeploy(epl);
                env.AddListener("Test");
                env.SendEventBean(new SupportGroupSubgroupEvent("G1", "SG1", 1, 10.45));
                env.AssertListenerInvoked("Test");
                env.UndeployAll();
            }
        }

        private class ContextKeyedNamedWindowNonPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionNamedWindow(env, "MyWindow as a");
            }
        }

        private class ContextKeyedNamedWindowPattern : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                RunAssertionNamedWindow(env, "pattern [every a=MyWindow]");
            }
        }

        private class ContextKeyedSubqueryNamedWindowIndexShared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var path = new RegressionPath();
                env.CompileDeploy(
                    "@name('context') @public create context SegmentedByString partition by theString from SupportBean",
                    path);
                env.CompileDeploy(
                    "@Hint('enable_window_subquery_indexshare') @public create window MyWindowTwo#keepall as SupportBean_S0",
                    path);
                env.CompileDeploy("insert into MyWindowTwo select * from SupportBean_S0", path);

                env.CompileDeploy(
                    "@name('s0') context SegmentedByString " +
                    "select theString, intPrimitive, (select p00 from MyWindowTwo as s0 where sb.intPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb",
                    path);
                env.AddListener("s0");

                TryAssertionSubqueryNW(env);

                env.UndeployAll();
            }
        }

        private class ContextKeyedSubqueryNamedWindowIndexUnShared : RegressionExecution
        {
            public void Run(RegressionEnvironment env)
            {
                var epl =
                    "@name('context') create context SegmentedByString partition by theString from SupportBean;\n" +
                    "create window MyWindowThree#keepall as SupportBean_S0;\n" +
                    "insert into MyWindowThree select * from SupportBean_S0;\n" +
                    "@name('s0') context SegmentedByString " +
                    "select theString, intPrimitive, (select p00 from MyWindowThree as s0 where sb.intPrimitive = s0.id) as val0 " +
                    "from SupportBean as sb;\n";
                env.CompileDeploy(epl).AddListener("s0");

                TryAssertionSubqueryNW(env);

                env.UndeployAll();
            }
        }

        private static void TryAssertionSubqueryNW(RegressionEnvironment env)
        {
            var fields = new string[] { "theString", "intPrimitive", "val0" };

            env.SendEventBean(new SupportBean_S0(10, "s1"));
            env.SendEventBean(new SupportBean("G1", 10));
            env.AssertPropsNew("s0", fields, new object[] { "G1", 10, "s1" });

            env.Milestone(0);

            env.SendEventBean(new SupportBean("G2", 10));
            env.AssertPropsNew("s0", fields, new object[] { "G2", 10, "s1" });

            env.SendEventBean(new SupportBean("G3", 20));
            env.AssertPropsNew("s0", fields, new object[] { "G3", 20, null });

            env.Milestone(1);

            env.SendEventBean(new SupportBean_S0(20, "s2"));
            env.SendEventBean(new SupportBean("G3", 20));
            env.AssertPropsNew("s0", fields, new object[] { "G3", 20, "s2" });

            env.SendEventBean(new SupportBean("G1", 20));
            env.AssertPropsNew("s0", fields, new object[] { "G1", 20, "s2" });
        }

        private static void RunAssertionNamedWindow(
            RegressionEnvironment env,
            string fromClause)
        {
            var path = new RegressionPath();
            var epl = "@public create context Ctx partition by theString from SupportBean;\n" +
                      "@name('window') @public context Ctx create window MyWindow#keepall as SupportBean;" +
                      "@name('insert') context Ctx insert into MyWindow select * from SupportBean;" +
                      "@name('s0') context Ctx select irstream context.key1 as c0, a.intPrimitive as c1 from " +
                      fromClause;
            env.CompileDeploy(epl, path).AddListener("s0");
            var fields = "c0,c1".SplitCsv();

            env.SendEventBean(new SupportBean("E1", 1));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 1 });

            env.SendEventBean(new SupportBean("E2", 2));
            env.AssertPropsNew("s0", fields, new object[] { "E2", 2 });

            env.SendEventBean(new SupportBean("E1", 3));
            env.AssertPropsNew("s0", fields, new object[] { "E1", 3 });

            env.SendEventBean(new SupportBean("E2", 4));
            env.AssertPropsNew("s0", fields, new object[] { "E2", 4 });

            TryInvalidCreateWindow(env, path);
            TryInvalidCreateWindow(env, path); // making sure all is cleaned up

            env.UndeployAll();
        }

        private static void TryInvalidCreateWindow(
            RegressionEnvironment env,
            RegressionPath path)
        {
            env.TryInvalidCompile(
                path,
                "context Ctx create window MyInvalidWindow#unique(p00) as SupportBean_S0",
                "Segmented context 'Ctx' requires that any of the event types that are listed in the segmented context also appear in any of the filter expressions of the statement, type 'SupportBean_S0' is not one of the types listed [context Ctx create window MyInvalidWindow#unique(p00) as SupportBean_S0]");
        }
    }
} // end of namespace